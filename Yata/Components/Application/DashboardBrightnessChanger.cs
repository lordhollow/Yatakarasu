using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components.Application
{
    /// <summary>
    /// ウィジェットコンテナの明るさを指定するアプリ
    /// </summary>
    class DashboardBrightnessChanger : IInternalApplication
    {
        WidgetContainer target;
        
        public DashboardBrightnessChanger(WidgetContainer widgetContainer)
        {
            target = widgetContainer;
        }

        public void Execute(Karasu karasu, string option)
        {
            var f = float.Parse(option);
            target.Brightness = f;
        }
    }
}
