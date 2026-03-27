using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace Percentage_Slider
{
    public partial class Form1 : Form
    {
        private Panel scrollPanel;
        private Button btnAdd;
        private Button btnSave;
        private const string SaveFile = "sliders.json";

        public Form1()
        {
            InitializeComponent();
            SetupForm();
            LoadFromFile();
        }

        // ── Layout ────────────────────────────────────────────────────────────

        private void SetupForm()
        {
            this.Text = "Slider Manager";
            this.Size = new Size(540, 500);
            this.MinimumSize = new Size(540, 300);

            var toolbar = new Panel();
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 45;
            toolbar.Padding = new Padding(8, 8, 8, 0);

            btnAdd = new Button();
            btnAdd.Text = "＋ Add Slider";
            btnAdd.Width = 110;
            btnAdd.Height = 30;
            btnAdd.Location = new Point(8, 8);
            btnAdd.Click += (s, e) => AddSliderEntry("", 100, 0);

            btnSave = new Button();
            btnSave.Text = "💾 Save";
            btnSave.Width = 80;
            btnSave.Height = 30;
            btnSave.Location = new Point(126, 8);
            btnSave.Click += (s, e) => SaveToFile();

            toolbar.Controls.Add(btnAdd);
            toolbar.Controls.Add(btnSave);

            scrollPanel = new Panel();
            scrollPanel.Dock = DockStyle.Fill;
            scrollPanel.AutoScroll = true;
            scrollPanel.Padding = new Padding(8);

            this.Controls.Add(scrollPanel);
            this.Controls.Add(toolbar);
        }

        // ── Slider Entry ──────────────────────────────────────────────────────

        private void AddSliderEntry(string name, int max, int current)
        {
            var entry = new Panel();
            entry.Width = scrollPanel.ClientSize.Width - 20;
            entry.Height = 110;
            entry.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            entry.BorderStyle = BorderStyle.FixedSingle;
            entry.Margin = new Padding(0, 0, 0, 8);

            // Row 1: Name, Max Value, Remove button
            var lblName = new Label() { Text = "Name:", Location = new Point(8, 10), AutoSize = true };

            var txtName = new TextBox();
            txtName.Name = "txtName";
            txtName.Text = name;
            txtName.Location = new Point(55, 7);
            txtName.Width = 140;

            var lblMax = new Label() { Text = "Max:", Location = new Point(210, 10), AutoSize = true };

            var nudMax = new NumericUpDown();
            nudMax.Name = "nudMax";
            nudMax.Location = new Point(240, 7);
            nudMax.Width = 70;
            nudMax.Minimum = 1;
            nudMax.Maximum = 100000;
            nudMax.Value = Math.Max(1, max);

            var btnRemove = new Button();
            btnRemove.Text = "✕";
            btnRemove.Width = 30;
            btnRemove.Height = 24;
            btnRemove.Location = new Point(entry.Width - 40, 6);
            btnRemove.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRemove.Click += (s, e) =>
            {
                scrollPanel.Controls.Remove(entry);
                ReflowEntries();
            };

            // Row 2: Slider
            var trackBar = new TrackBar();
            trackBar.Name = "trackBar";
            trackBar.Location = new Point(8, 35);
            trackBar.Width = entry.Width - 20;
            trackBar.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            trackBar.Minimum = 0;
            trackBar.Maximum = (int)nudMax.Value;
            trackBar.Value = Math.Min(current, (int)nudMax.Value);
            trackBar.TickFrequency = Math.Max(1, (int)nudMax.Value / 10);

            // Row 3: Current value NUD and percentage label
            var lblCurrent = new Label() { Text = "Value:", Location = new Point(8, 78), AutoSize = true };

            var nudCurrent = new NumericUpDown();
            nudCurrent.Name = "nudCurrent";
            nudCurrent.Location = new Point(55, 75);
            nudCurrent.Width = 70;
            nudCurrent.Minimum = 0;
            nudCurrent.Maximum = nudMax.Value;
            nudCurrent.Value = Math.Min(current, (int)nudMax.Value);

            var lblPercent = new Label();
            lblPercent.Name = "lblPercent";
            lblPercent.Location = new Point(140, 78);
            lblPercent.AutoSize = true;
            lblPercent.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblPercent.Text = FormatPercent(current, (int)nudMax.Value);

            // ── Event wiring ──────────────────────────────────────────────────

            nudMax.ValueChanged += (s, e) =>
            {
                int maxVal = (int)nudMax.Value;
                trackBar.Maximum = maxVal;
                trackBar.TickFrequency = Math.Max(1, maxVal / 10);
                nudCurrent.Maximum = maxVal;
                if (nudCurrent.Value > maxVal) nudCurrent.Value = maxVal;
                if (trackBar.Value > maxVal) trackBar.Value = maxVal;
                lblPercent.Text = FormatPercent((int)nudCurrent.Value, maxVal);
            };

            trackBar.ValueChanged += (s, e) =>
            {
                nudCurrent.ValueChanged -= NudCurrentChanged;
                nudCurrent.Value = trackBar.Value;
                nudCurrent.ValueChanged += NudCurrentChanged;
                lblPercent.Text = FormatPercent(trackBar.Value, (int)nudMax.Value);
            };

            nudCurrent.ValueChanged += NudCurrentChanged;

            void NudCurrentChanged(object s, EventArgs e)
            {
                trackBar.ValueChanged -= TrackBarChanged;
                trackBar.Value = Math.Min((int)nudCurrent.Value, trackBar.Maximum);
                trackBar.ValueChanged += TrackBarChanged;
                lblPercent.Text = FormatPercent((int)nudCurrent.Value, (int)nudMax.Value);
            }

            void TrackBarChanged(object s, EventArgs e)
            {
                nudCurrent.ValueChanged -= NudCurrentChanged;
                nudCurrent.Value = trackBar.Value;
                nudCurrent.ValueChanged += NudCurrentChanged;
                lblPercent.Text = FormatPercent(trackBar.Value, (int)nudMax.Value);
            }

            entry.Controls.AddRange(new Control[]
            {
                lblName, txtName, lblMax, nudMax, btnRemove,
                trackBar,
                lblCurrent, nudCurrent, lblPercent
            });

            scrollPanel.Controls.Add(entry);
            ReflowEntries();
        }

        private void ReflowEntries()
        {
            int y = 8;
            foreach (Control c in scrollPanel.Controls)
            {
                c.Top = y;
                c.Width = scrollPanel.ClientSize.Width - 20;
                y += c.Height + 8;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private string FormatPercent(int value, int max) =>
            max > 0 ? $"{(double)value / max * 100:F2}%" : "0.00%";

        // ── Save / Load ───────────────────────────────────────────────────────

        private void SaveToFile()
        {
            var entries = new List<SliderData>();

            foreach (Control c in scrollPanel.Controls)
            {
                if (c is not Panel entry) continue;

                var txtName = entry.Controls["txtName"] as TextBox;
                var nudMax = entry.Controls["nudMax"] as NumericUpDown;
                var nudCurrent = entry.Controls["nudCurrent"] as NumericUpDown;

                entries.Add(new SliderData
                {
                    Name = txtName?.Text ?? "",
                    Max = (int)(nudMax?.Value ?? 100),
                    Current = (int)(nudCurrent?.Value ?? 0)
                });
            }

            var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SaveFile, json);
            MessageBox.Show("Saved!", "Slider Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadFromFile()
        {
            if (!File.Exists(SaveFile)) return;

            try
            {
                var json = File.ReadAllText(SaveFile);
                var entries = JsonSerializer.Deserialize<List<SliderData>>(json);
                if (entries == null) return;

                foreach (var entry in entries)
                    AddSliderEntry(entry.Name, entry.Max, entry.Current);
            }
            catch
            {
                MessageBox.Show("Could not load save file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    // ── Data model ────────────────────────────────────────────────────────────

    public class SliderData
    {
        public string Name { get; set; } = "";
        public int Max { get; set; } = 100;
        public int Current { get; set; } = 0;
    }
}