using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
namespace Yata.Settings
{
    internal sealed partial class SensorPickerForm : Form
    {
        readonly List<SensorChoice> sensors = new List<SensorChoice>();
        public SensorChoice SelectedSensor { get; private set; }
        public SensorPickerForm() { InitializeComponent(); }
        public SensorPickerForm(List<SensorChoice> sensors) : this()
        {
            this.sensors = sensors ?? new List<SensorChoice>();
            RefreshList();
        }
        void RefreshList() => list.DataSource = sensors.Where(x => x.Description.IndexOf(filter.Text, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        void Filter_TextChanged(object sender, EventArgs e) => RefreshList();
        void SelectButton_Click(object sender, EventArgs e)
        {
            if (list.SelectedItem is SensorChoice choice) { SelectedSensor = choice; DialogResult = DialogResult.OK; }
        }
        void List_DoubleClick(object sender, EventArgs e) => selectButton.PerformClick();
        void CancelButton_Click(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;
    }
}
