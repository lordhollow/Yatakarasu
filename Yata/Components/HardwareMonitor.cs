using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using OpenHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware;

namespace Yata.Components
{
    /// <summary>
    /// ハードウェアモニタ
    /// コンストラクタ終了後、AllSensorsから必要なものを選択しStartupを呼び出す。その後、IUpdatableの仕様に沿ってコールする。
    /// </summary>
    public class HardwareMonitor : IDisposable, IUpdatable
    {
        Computer computer;

        public static HardwareMonitor DefaultInstance { get; private set; }

        public HardwareMonitor()
        {
            computer = new Computer()
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMotherboardEnabled = true,
                IsMemoryEnabled = true,
                //IsControllerEnabled = true,
                //IsBatteryEnabled = true,
                //IsPsuEnabled = true,
                IsStorageEnabled = true,
                //IsNetworkEnabled = true,
            };
            computer.Open();
            EnumerateAllSensors();
            DefaultInstance = this;
        }

        /// <summary>
        /// 更新を並列化する
        /// </summary>
        public bool UpdateParall { get; set; } = false;

        /// <summary>
        /// 更新を非同期化する（全体のUpdateのインターバルが安定化する）
        /// </summary>
        public bool UpdateAsync { get; set; } = true;

        /// <summary>
        /// すべてのHWツリーを辿って全部のセンサーを列挙する。Updateしないと取れないものもあるのでUpdateしてから実施する。
        /// </summary>
        private void EnumerateAllSensors()
        {
            //初期化ビジターを使って全項目アップデート＆全センサーのウィンドウをゼロにしつつ収集する
            var visitor = new InitializerVisitor();
            computer.Accept(visitor);
            AllSensors = visitor.AllSensors;
        }

        /// <summary>
        /// 利用可能なすべてのセンサーの一覧
        /// </summary>
        public List<ISensor> AllSensors { get; private set; }

        /// <summary>
        /// 更新するハードウェアのリスト
        /// </summary>
        public List<IHardware> UpdateTargetHardware { get; private set; }

        /// <summary>
        /// 更新対象センサーリスト
        /// </summary>
        public List<ISensor> UpdateTargetSensor { get; private set; }

        /// <summary>
        /// セットアップ
        /// </summary>
        /// <param name="timeWindow">値を覚えておく期間</param>
        /// <param name="sensorUsage">有効にするセンサーの一覧</param>
        public void Startup(TimeSpan timeWindow, IEnumerable<ISensor> sensorUsage)
        {
            var updateList = new List<IHardware>();
            var sensList = new List<ISensor>();

            foreach (var sensor in sensorUsage)
            {
                //sensor.ValuesTimeWindow = timeWindow;
                addToUpdateList(updateList, sensor.Hardware);
                sensList.Add(new ValueHistorySensor(sensor) { ValuesTimeWindow = timeWindow });
            }
            UpdateTargetHardware = updateList;
            UpdateTargetSensor = sensList;

        }

        public void Startup(TimeSpan timeWindow, IEnumerable<string> sensorUsage)
        {
            var updateList = new List<IHardware>();
            var sensList = new List<ISensor>();

            var reg = new Regex(@"^(.+)/(\d+)-(\d+)$");
            foreach (var sensor in sensorUsage)
            {
                var m = reg.Match(sensor);
                if (m.Success)
                {
                    var b = m.Groups[1].Value;
                    var s = int.Parse(m.Groups[2].Value);
                    var e = int.Parse(m.Groups[3].Value);
                    var l = new List<ISensor>();
                    for (var i = s; i <= e; i++)
                    {
                        var sens = AllSensors?.Find(x => x.Identifier.ToString() == $"{b}/{i}");
                        if (sens != null)
                        {
                            addToUpdateList(updateList, sens.Hardware);
                            l.Add(sens);
                        }
                    }
                    if (l.Count > 0)
                    {
                        var id = new Identifier(sensor.Substring(1).Split('/'));
                        sensList.Add(new ValueHistorySensor(l.ToArray(), id, false) { ValuesTimeWindow = timeWindow });
                    }
                }
                else if (sensor.Contains('+'))
                {
                    var sensors = sensor.Split('+');
                    var l = new List<ISensor>();
                    foreach (var s in sensors)
                    {
                        var sens = GetSensorById(s);
                        if (sens != null)
                        {
                            addToUpdateList(updateList, sens.Hardware);
                            l.Add(sens);
                        }
                    }
                    if (l.Count > 0)
                    {
                        var id = new Identifier(sensor.Substring(1).Split('/'));
                        sensList.Add(new ValueHistorySensor(l.ToArray(), id, true) { ValuesTimeWindow = timeWindow });
                    }
                }
                else
                {
                    var sens = AllSensors?.Find(x => x.Identifier.ToString() == sensor);
                    if (sens != null)
                    {
                        addToUpdateList(updateList, sens.Hardware);
                        sensList.Add(new ValueHistorySensor(sens) { ValuesTimeWindow = timeWindow });
                    }
                }
            }
            UpdateTargetHardware = updateList;
            UpdateTargetSensor = sensList;
        }

        /// <summary>
        /// 更新対象センサーをIDで取得
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ISensor this[string id]
        {
            get
            {
                var d = UpdateTargetSensor?.Find(x => x.Identifier.ToString() == id);
                if (d == null)
                {
                    d = new ErrorSensor();
                }
                return d;
            }
        }

        /// <summary>
        /// this[]と異なり、全センサーから取得する。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ISensor GetSensorById(string id)
        {
            var founder = new FounderVisitor { FindId = id };
            computer.Accept(founder);
            return founder.FoundSensor;
        }

        private void addToUpdateList(List<IHardware> list, IHardware hw)
        {
            //親から入れていく
            if (hw.Parent != null)
            {
                addToUpdateList(list, hw.Parent);
            }
            //入っているものは入れない
            if (!list.Contains(hw))
            {
                list.Add(hw);
            }
        }


        public void Dispose()
        {
            computer.Close();
            computer = null;
        }

        /// <summary>
        /// 世代。updateHardwareが完了するごとに+1。
        /// </summary>
        public int Age { get; private set; }


        #region IUpdatable

        /// <summary>
        /// 更新レート
        /// </summary>
        public int UpdateRate { get => 10; }

        /// <summary>
        /// 更新
        /// </summary>
        public bool Update()
        {
            if (UpdateAsync)
            {
                updateHardwareAsync();
            }
            else
            {
                updateHardware();
            }
            return false;
        }

        private async void updateHardwareAsync()
        {
            //100～200msぐらいかかるため非同期。UpdateRateは最低でも３、できれば５以上必要で、10あればよい。
            await Task.Run((Action)updateHardware);
        }

        private void updateHardware()
        {
            //全部をシーケンシャルにすると150～250msぐらい。パラレルにすると100ぐらいになる。
            if (UpdateParall)
            {
                //意外とかかるのでちょっと早い
                Parallel.ForEach(UpdateTargetHardware, (x) => x.Update());
                //ここは即終わるのであまり効果がない
                Parallel.ForEach(UpdateTargetSensor, (sensor) => (sensor as ValueHistorySensor).Update());
            }
            else
            {
                foreach (var hw in UpdateTargetHardware)
                {
                    hw.Update();
                }

                foreach (var sensor in UpdateTargetSensor)
                {
                    (sensor as ValueHistorySensor).Update();
                }
            }
            Age++;
        }

        #endregion

        /// <summary>
        /// エラーセンサー。this[]のnull object。
        /// </summary>
        class ErrorSensor : ISensor
        {
            public IControl Control => null;
            public IHardware Hardware => null;
            public Identifier Identifier { get; set; }
            public int Index => 0;
            public bool IsDefaultHidden => true;
            public float? Max => 0;
            public float? Min => 0;
            public string Name { get; set; } = "Error.";
            public IReadOnlyList<IParameter> Parameters => null;
            public SensorType SensorType => SensorType.Data;
            public float? Value => 0;
            public IEnumerable<SensorValue> Values => new SensorValue[0];
            public TimeSpan ValuesTimeWindow { get; set; }
            public void Accept(IVisitor visitor) { }
            public void ClearValues() { }
            public void ResetMax() { }
            public void ResetMin() { }
            public void Traverse(IVisitor visitor) { }
        }

    }

    /// <summary>
    /// 履歴データを平均化せず、ランレングスで圧縮する(変化時点の時刻と値を保持する）センサー。
    /// </summary>
    /// <remarks>
    /// ついでに色情報がある
    /// </remarks>
    public class ValueHistorySensor : ISensor, IColorArranged
    {
        ISensor baseSensor;
        ISensor[] baseSensors;
        Identifier localId;
        float? inValue = 0;
        private bool sumup;

        public ValueHistorySensor(ISensor baseSensor)
        {
            this.baseSensor = baseSensor;
        }

        public ValueHistorySensor(ISensor[] baseSensors, Identifier newId, bool sumup)
        {
            this.baseSensor = baseSensors[0];
            this.baseSensors = baseSensors;
            this.localId = newId;
            this.sumup = sumup;
        }

        public IControl Control => baseSensor.Control;
        public IHardware Hardware => baseSensor.Hardware;
        public Identifier Identifier => (localId == null) ? baseSensor.Identifier : localId;
        public int Index => baseSensor.Index;
        public bool IsDefaultHidden => baseSensor.IsDefaultHidden;
        public float? Max => baseSensor.Max;
        public float? Min => baseSensor.Min;
        public string Name { get => baseSensor.Name; set => baseSensor.Name = value; }
        public IReadOnlyList<IParameter> Parameters => baseSensor.Parameters;
        public SensorType SensorType => baseSensor.SensorType;
        public void Accept(IVisitor visitor) => baseSensor.Accept(visitor);
        public void ResetMax() => baseSensor.ResetMax();
        public void ResetMin() => baseSensor.ResetMin();
        public void Traverse(IVisitor visitor) => baseSensor.Traverse(visitor);


        public float? Value
        {
            get => inValue;
            set
            {
                //Todo::履歴への書き込み
                inValue = value;
                if (!value.HasValue) return;
                value = (float)Math.Round(value.Value, 1);  //小数点1桁に丸め
                if (ValuesTimeWindow != TimeSpan.Zero)
                {
                    //RunlengthSetter(value);
                    AverageSetter(value);
                }
                //最初より2番目の時間が今よりWindow分前なら捨てる（2番目で判定するのは、最初を捨ててしまうと二番目のデータまでの時間のデータが無になるため。
                //記録時間(10分)以上スリープしていると全データが古くなっている可能性があり、
                //そういうデータをすべて削除する意味でも、削除するものがなくなるまで繰り返し削除する。
                bool checking = true;
                while (checking)
                {
                    checking = false;
                    if (sensorValues.Count >= 2)
                    {
                        var sv = sensorValues.First.Next.Value;
                        if (sv.Time + ValuesTimeWindow < DateTime.Now)
                        {
                            sensorValues.RemoveFirst();
                            checking = true;    //削除したらもう一回。
                        }
                    }
                }
            }
        }

        LinkedList<SensorValue> sensorValues = new LinkedList<SensorValue>();

        #region runlength

        void RunlengthSetter(float? value)
        {
            if (!latestSensorValue.HasValue)
            {
                //最初の一個の時はそのまま登録
                var sv = new SensorValue(value.Value, DateTime.Now);
                sensorValues.AddLast(sv);
                latestSensorValue = sv;
            }
            else
            {
                //値が最後のものと変わらないときは何もしない
                if (latestSensorValue.Value.Value == value.Value) return;
                var sv = new SensorValue(value.Value, DateTime.Now);
                sensorValues.AddLast(sv);
                latestSensorValue = sv;
            }
        }

        SensorValue? latestSensorValue;

        #endregion

        #region average
        float total;
        int averageWindow = 5;
        int averageCounter = 0;

        void AverageSetter(float? value)
        {
            total += value.Value;
            averageCounter++;
            if (averageCounter >= averageWindow)
            {
                var avg = total / averageCounter;
                total = 0;
                averageCounter = 0;
                sensorValues.AddLast(new SensorValue(avg, DateTime.Now));
            }
        }
        #endregion

        public IEnumerable<SensorValue> Values
        {
            get => sensorValues;
        }

        public void ClearValues()
        {
            //TODO::履歴のクリア
            sensorValues.Clear();
            latestSensorValue = null;
        }

        public TimeSpan ValuesTimeWindow { get; set; }

        public void Update()
        {
            if (baseSensors == null)
            {
                Value = baseSensor.Value;
            }
            else if (sumup)
            {
                Value = baseSensors.Select(x => x.Value).Sum();
            }
            else
            {
                Value = baseSensors.Select(x => x.Value).Average();
            }
        }

        /// <summary>
        /// 色
        /// </summary>
        public System.Drawing.Color Color { get; set; }
    }

    public interface IColorArranged
    {
        System.Drawing.Color Color { get; set; }
    }
    public class InitializerVisitor : IVisitor
    {
        public List<ISensor> AllSensors { get; private set; } = new List<ISensor>();

        StringBuilder log;

        public void VisitComputer(IComputer computer)
        {
            log = new StringBuilder();
            computer.Traverse(this);
            System.IO.File.WriteAllText(Components.Widget.CommonResource.YataFile("karasu.hw.txt"), log.ToString());
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (var sensor in hardware.Sensors)
            {
                sensor.ValuesTimeWindow = TimeSpan.Zero;
                log?.AppendLine($"{hardware.Name}\t{sensor.SensorType}\t{sensor.Name}\t{sensor.Identifier}\t{sensor.Value}");
                AllSensors.Add(sensor);
            }
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }

    public class FounderVisitor : IVisitor
    {
        public string FindId { get; set; }

        public ISensor FoundSensor { get; private set; }

        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            if (FoundSensor != null) { return; }
            foreach (var sensor in hardware.Sensors)
            {
                if (sensor.Identifier.ToString() == FindId)
                {
                    FoundSensor = sensor;
                    return;
                }
            }
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
}
