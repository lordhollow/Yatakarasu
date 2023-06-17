using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components
{
    /// <summary>
    /// 更新間隔が開いたことからPCが休止状態にあったかどうかを判断する。基準は１秒。
    /// Realtimeスレッドで全コア占有しているみたいなレアケースのときは知らん。
    /// </summary>
    class SuspendedChecker : IUpdatable
    {

        /// <summary>
        /// 最終更新時刻
        /// </summary>
        DateTime lastUpdate;

        /// <summary>
        /// 初回？
        /// </summary>
        bool firstTime = true;

        /// <summary>
        /// 更新レート(10よりは小さい必要がある）
        /// </summary>
        public int UpdateRate => 1;

        /// <summary>
        /// 閾値(秒)
        /// </summary>
        public double Threshold { get; set; } = 5.0;

        /// <summary>
        /// 覚えておくためのzip
        /// </summary>
        struct Entry
        {
            public DateTime Begin;
            public DateTime End;
        }

        /// <summary>
        /// サスペンド時刻リスト
        /// </summary>
        List<Entry> suspendList = new List<Entry>();

        /// <summary>
        /// 覚えておく期限
        /// </summary>
        public TimeSpan Expire { get; set; } = TimeSpan.FromMinutes(10);

        public SuspendedChecker()
        {
            //以下は描画のデバッグ用に適当な位置にサスペンド領域を作るためもの。
            //suspendList.Add(new Entry
            //{
            //    Begin = DateTime.Now.AddMinutes(-13),
            //    End = DateTime.Now.AddMinutes(-8.5),
            //});
            //suspendList.Add(new Entry
            //{
            //    Begin = DateTime.Now.AddMinutes(-6.5),
            //    End = DateTime.Now.AddMinutes(-5.5),
            //});
            //suspendList.Add(new Entry
            //{
            //    Begin = DateTime.Now.AddMinutes(-1),
            //    End = DateTime.Now.AddMinutes(2),
            //});
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
            if(firstTime)
            {
                lastUpdate = DateTime.Now;
                firstTime = false;
            }
            else
            {
                var n = DateTime.Now;

                //期限より古いデータは捨てる
                suspendList.RemoveAll(e => (n - e.End) > Expire);

                //1秒以上開いたらサスペンドしてた判定
                if ((n - lastUpdate).TotalSeconds > Threshold)
                {
                    suspendList.Add(new Entry { Begin = lastUpdate, End = n});
                }
                lastUpdate = n;
            }
            return false;
        }

        /// <summary>
        /// ビジター(引数はサスペンド開始と終了の時刻）
        /// </summary>
        /// <param name="visitor"></param>
        public void Visit(Action<DateTime,DateTime> visitor)
        {
            foreach(var e in suspendList)
            {
                visitor(e.Begin, e.End);
            }
        }
    }
}
