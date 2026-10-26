using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
//using OpenHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Cpu;
using LibreHardwareMonitor.PawnIo;

namespace Yata.Components
{
    /// <summary>
    /// ハードウェアモニタ
    /// コンストラクタ終了後、AllSensorsから必要なものを選択しStartupを呼び出す。その後、IUpdatableの仕様に沿ってコールする。
    /// </summary>
    public class HardwareMonitor : IDisposable, IUpdatable
    {
        Computer computer;
        int updateInProgress;

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
            var nreg = new Regex(@"^\((.+)\)(.+)");
            var reg = new Regex(@"^(.+)/(\d+)-(\d+)$");
            foreach (var scode in sensorUsage)
            {
                var sensor = scode;
                var name = "";
                var n = nreg.Match(scode);
                if (n.Success)
                {
                    name = n.Groups[1].Value;
                    sensor = n.Groups[2].Value;
                }
                var m = reg.Match(sensor);
                if (m.Success)
                {
                    var b = m.Groups[1].Value;
                    var s = int.Parse(m.Groups[2].Value);
                    var e = int.Parse(m.Groups[3].Value);
                    var l = new List<ISensor>();
                    for (var i = s; i <= e; i++)
                    {
                        //var sens = AllSensors?.Find(x => x.Identifier.ToString() == $"{b}/{i}");
                        var sens = Select($"{b}/{i}", name);
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
                        //var sens = GetSensorById(s);
                        var sens = Select(s, name);
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
                    //var sens = AllSensors?.Find(x => x.Identifier.ToString() == sensor);
                    var sens = Select(sensor, name);
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
                var nreg = new Regex(@"^\((.+)\)(.+)");
                var n = nreg.Match(id);
                ISensor d;
                if (n.Success)
                {
                    d = SelectFromUpdateTarget(n.Groups[2].Value, n.Groups[1].Value);
                }
                else
                {
                    d = SelectFromUpdateTarget(id);
                }
                if (d == null)
                {
                    d = new ErrorSensor();
                }
                return d;
            }
        }

        /// <summary>
        /// 全センサーからidに一致するものを取る。nameが指定されるとき、センサーが複数見つかればnameを含む法を返す。id一致しない場合はnull
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public ISensor Select(string id, string name = null)
        {
            var found = AllSensors.FindAll(x => x.Identifier.ToString() == id);
            if (found.Count == 0) return null;
            if (found.Count == 1) return found[0];
            if (string.IsNullOrEmpty(name)) return found[0];
            return found.Find(x => x.Name.Contains(name));
        }

        public ISensor SelectFromUpdateTarget(string id, string name=null)
        {
            var found = UpdateTargetSensor.FindAll(x => x.Identifier.ToString() == id);
            if (found.Count == 0) return null;
            if (found.Count == 1) return found[0];
            if (string.IsNullOrEmpty(name)) return found[0];
            return found.Find(x => x.Name.Contains(name));
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
            //遅いストレージなどの更新が次の周期まで続いても、同じ機器を同時更新しない。
            if (Interlocked.CompareExchange(ref updateInProgress, 1, 0) != 0) return false;
            if (UpdateAsync)
            {
                _ = updateHardwareAsync();
            }
            else
            {
                try
                {
                    updateHardware();
                }
                finally
                {
                    Interlocked.Exchange(ref updateInProgress, 0);
                }
            }
            return false;
        }

        private async Task updateHardwareAsync()
        {
            try
            {
                await Task.Run((Action)updateHardware).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //async voidからUIスレッドへ例外を送らず、次の周期で再試行する。
                HardwareUpdateLog.Failure("Update cycle", ex);
            }
            finally
            {
                Interlocked.Exchange(ref updateInProgress, 0);
            }
        }

        private void updateHardware()
        {
            var succeeded = new bool[UpdateTargetHardware.Count];
            if (UpdateParall)
            {
                Parallel.For(0, succeeded.Length,
                    i => succeeded[i] = TryUpdateHardware(UpdateTargetHardware[i]));
            }
            else
            {
                for (var i = 0; i < succeeded.Length; i++)
                {
                    succeeded[i] = TryUpdateHardware(UpdateTargetHardware[i]);
                }
            }

            var failedHardware = new HashSet<IHardware>(
                UpdateTargetHardware.Where((hw, i) => !succeeded[i]));
            foreach (ValueHistorySensor sensor in UpdateTargetSensor)
            {
                var target = "Sensor " + sensor.Identifier;
                try
                {
                    sensor.Update(failedHardware);
                    HardwareUpdateLog.Success(target);
                }
                catch (Exception ex)
                {
                    sensor.Value = null;
                    HardwareUpdateLog.Failure(target, ex);
                }
            }
            HardwareUpdateLog.Success("Update cycle");
            Age++;
        }

        internal static bool TryUpdateHardware(IHardware hardware)
        {
            var target = "Hardware " + hardware.Identifier;
            try
            {
                hardware.Update();
                HardwareUpdateLog.Success(target);
                return true;
            }
            catch (Exception ex)
            {
                HardwareUpdateLog.Failure(target, ex);
                return false;
            }
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
        readonly object historyLock = new object();
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
            get { lock (historyLock) { return inValue; } }
            set
            {
                lock (historyLock)
                {
                    inValue = value;
                    if (!value.HasValue)
                    {
                        total = 0;
                        averageCounter = 0;
                        return;
                    }
                    value = (float)Math.Round(value.Value, 1);  //小数点1桁に丸め
                    if (ValuesTimeWindow != TimeSpan.Zero)
                    {
                        AverageSetter(value);
                    }
                    //2番目の標本までが期間外なら先頭を削除する。長時間休止した場合も繰り返す。
                    while (sensorValues.Count >= 2)
                    {
                        if (sensorValues.First.Next.Value.Time + ValuesTimeWindow >= DateTime.Now)
                        {
                            break;
                        }
                        sensorValues.RemoveFirst();
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
            get { lock (historyLock) { return sensorValues.ToArray(); } }
        }

        public void ClearValues()
        {
            lock (historyLock)
            {
                sensorValues.Clear();
                latestSensorValue = null;
                total = 0;
                averageCounter = 0;
            }
        }

        public TimeSpan ValuesTimeWindow { get; set; }

        public void Update()
        {
            if (baseSensors == null)
            {
                Value = baseSensor.Value;
            }
            else
            {
                var values = baseSensors.Select(x => x.Value).ToArray();
                Value = values.Any(x => !x.HasValue) ? (float?)null
                    : sumup ? values.Sum() : values.Average();
            }
        }

        internal void Update(ISet<IHardware> failedHardware)
        {
            //合計・平均の元センサーも含め、失敗した機器の古い値を新しい標本にしない。
            foreach (var sensor in baseSensors ?? new[] { baseSensor })
            {
                for (var hw = sensor.Hardware; hw != null; hw = hw.Parent)
                {
                    if (failedHardware.Contains(hw))
                    {
                        Value = null;
                        return;
                    }
                }
            }
            Update();
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
            HardwareMonitor.TryUpdateHardware(hardware);
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

    internal static class HardwareUpdateLog
    {
        static readonly ConcurrentDictionary<string, byte> failures = new ConcurrentDictionary<string, byte>();
        static readonly object fileLock = new object();

        public static void Success(string target)
        {
            byte ignored;
            failures.TryRemove(target, out ignored);
        }

        public static void Failure(string target, Exception exception)
        {
            //同じ機器が連続失敗している間は初回だけ記録し、ログの肥大化を避ける。
            if (!failures.TryAdd(target, 0)) return;
            var message = $"{DateTime.Now:O}\t{target}\r\n{exception}\r\n";
            try
            {
                Trace.TraceError(message);
                lock (fileLock)
                {
                    File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        "hardware-monitor.errors.log"), message);
                }
            }
            catch (Exception)
            {
                //ログを書けなくても監視処理は継続する。
            }
        }
    }
}
