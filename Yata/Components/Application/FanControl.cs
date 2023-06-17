using HidSharp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components.Application
{
    /// <summary>
    /// ファン制御アプリ
    /// </summary>
    /// <remarks>
    /// ArmouryCrateなどアプリレベルでファン制御するSWとは当然干渉しデフォルトに戻せないので注意。
    /// 一般的なファン制御アプリは「自動」の時自前のゲイン表を用いて動機的に設定するがこのアプリはBiosのデフォルトに戻すような挙動のはず。
    /// </remarks>
    class FanControl : IInternalApplication
    {
        HardwareMonitor monitor;

        public FanControl(HardwareMonitor monitor)
        {
            this.monitor = monitor;
        }

        public void Execute(Karasu karasu, string option)
        {
            var dt = option.Split(',');
            var ctrlId = dt[0];
            var value = dt[1];

            var sensor = monitor.GetSensorById(ctrlId);
            if (value=="auto")
            {
                sensor?.Control?.SetDefault();
            }
            else
            {
                sensor?.Control?.SetSoftware(float.Parse(value));
            }
        }
    }
}
