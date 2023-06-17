using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components
{
    class SensorAnnotator
    {
        public LibreHardwareMonitor.Hardware.ISensor Sensor { get; set; }
        public string DisplayName { get; set; }
        public System.Drawing.Color Color { get; set; }
    }
}
