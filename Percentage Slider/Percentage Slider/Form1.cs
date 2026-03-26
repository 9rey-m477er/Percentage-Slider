using System;
using System.Windows.Forms;

namespace Percentage_Slider
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SetupControls();
        }

        private void SetupControls()
        {
            this.Text = "Slider Percentage";
            this.Size = new System.Drawing.Size(400, 220);

            // "Max Value" label
            Label lblMaxValue = new Label();
            lblMaxValue.Text = "Max Value:";
            lblMaxValue.Location = new System.Drawing.Point(20, 20);
            lblMaxValue.AutoSize = true;

            // NumericUpDown for max value input
            NumericUpDown nudMaxValue = new NumericUpDown();
            nudMaxValue.Name = "nudMaxValue";
            nudMaxValue.Location = new System.Drawing.Point(100, 17);
            nudMaxValue.Width = 80;
            nudMaxValue.Minimum = 1;
            nudMaxValue.Maximum = 10000;
            nudMaxValue.Value = 100;
            nudMaxValue.ValueChanged += InputChanged;

            // NumericUpDown for current value input
            NumericUpDown nudCurrentValue = new NumericUpDown();
            nudCurrentValue.Name = "nudCurrentValue";
            nudCurrentValue.Location = new System.Drawing.Point(200, 17);
            nudCurrentValue.Width = 80;
            nudCurrentValue.Minimum = 1;
            nudCurrentValue.Maximum = nudMaxValue.Value;
            nudCurrentValue.Value = 100;
            nudCurrentValue.ValueChanged += InputChanged;

            // TrackBar (slider)
            TrackBar trackBar = new TrackBar();
            trackBar.Name = "trackBar1";
            trackBar.Location = new System.Drawing.Point(20, 60);
            trackBar.Width = 340;
            trackBar.Minimum = 0;
            trackBar.Maximum = (int)nudMaxValue.Value;
            trackBar.TickFrequency = 10;
            trackBar.ValueChanged += InputChanged;

            // Percentage display label
            Label lblPercentage = new Label();
            lblPercentage.Name = "lblPercentage";
            lblPercentage.Text = "0.00%";
            lblPercentage.Location = new System.Drawing.Point(20, 120);
            lblPercentage.AutoSize = true;
            lblPercentage.Font = new System.Drawing.Font("Segoe UI", 24, System.Drawing.FontStyle.Bold);

            this.Controls.Add(lblMaxValue);
            this.Controls.Add(nudMaxValue);
            this.Controls.Add(trackBar);
            this.Controls.Add(lblPercentage);
            this.Controls.Add(nudCurrentValue);
        }

        private void InputChanged(object sender, EventArgs e)
        {
            var nudMaxValue = (NumericUpDown)this.Controls["nudMaxValue"];
            var nudCurrentValue = (NumericUpDown)this.Controls["nudCurrentValue"];
            var trackBar = (TrackBar)this.Controls["trackBar1"];
            var lblPercentage = (Label)this.Controls["lblPercentage"];

            int maxVal = (int)nudMaxValue.Value;

            // Keep slider max in sync with the max value input
            if (trackBar.Maximum != maxVal)
            {
                trackBar.Maximum = maxVal;
                if (trackBar.Value > maxVal)
                    trackBar.Value = maxVal;
            }

            // Keep nudCurrentValue max in sync too
            nudCurrentValue.Maximum = maxVal;

            // Sync slider and nudCurrentValue without infinite loops
            if (sender == trackBar)
            {
                nudCurrentValue.ValueChanged -= InputChanged;
                nudCurrentValue.Value = Math.Min(trackBar.Value, nudCurrentValue.Maximum);
                nudCurrentValue.ValueChanged += InputChanged;
            }
            else if (sender == nudCurrentValue)
            {
                trackBar.ValueChanged -= InputChanged;
                trackBar.Value = Math.Min((int)nudCurrentValue.Value, trackBar.Maximum);
                trackBar.ValueChanged += InputChanged;
            }

            double percentage = maxVal > 0 ? (double)trackBar.Value / maxVal * 100.0 : 0;
            lblPercentage.Text = $"{percentage:F2}%";
        }
    }
}