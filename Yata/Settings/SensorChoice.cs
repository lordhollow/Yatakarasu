using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;

namespace Yata.Settings
{
    internal sealed class SensorChoice
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }
        public override string ToString() => Description;
        public static List<SensorChoice> FromSensors(IEnumerable<ISensor> sensors)
        {
            var all = sensors.ToList();
            return all.Select(sensor => new SensorChoice
            {
                Id = all.Count(x => x.Identifier.ToString() == sensor.Identifier.ToString()) > 1 ? "(" + sensor.Name + ")" + sensor.Identifier : sensor.Identifier.ToString(),
                Name = sensor.Name, Unit = UnitFor(sensor.SensorType),
                Description = sensor.Name + "  |  " + sensor.SensorType + "  |  " + sensor.Identifier
            }).OrderBy(x => x.Description).ToList();
        }
        static string UnitFor(SensorType type)
        {
            switch (type)
            {
                case SensorType.Temperature: return "°C";
                case SensorType.Load: case SensorType.Control: return "%";
                case SensorType.Fan: return "rpm";
                case SensorType.Clock: return "MHz";
                case SensorType.Power: return "W";
                case SensorType.Voltage: return "V";
                case SensorType.Data: return "GB";
                case SensorType.SmallData: return "MB";
                case SensorType.Throughput: return "B/s";
                default: return " ";
            }
        }
    }

}
