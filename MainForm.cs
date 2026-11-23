using System.Drawing;
using LibreHardwareMonitor.Hardware;

namespace TempMonitor;

public sealed class MainForm : Form
{
    private readonly FlowLayoutPanel list = new();
    private readonly Label status = new();
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 2000 };
    private readonly Computer computer = new()
    {
        IsCpuEnabled = true,
        IsGpuEnabled = true,
        IsMotherboardEnabled = true,
        IsMemoryEnabled = false,
        IsStorageEnabled = false,
        IsControllerEnabled = false,
        IsNetworkEnabled = false
    };
    private bool opened;

    public MainForm()
    {
        Text = "TempMonitor";
        MinimumSize = new Size(430, 360);
        Size = new Size(520, 520);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(18, 20, 25);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9.5f);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var header = new Panel { Dock = DockStyle.Top, Height = 92, Padding = new Padding(20, 14, 20, 8) };
        var title = new Label { Text = "TempMonitor", Font = new Font("Segoe UI Semibold", 19), ForeColor = Color.White, AutoSize = true, Location = new Point(18, 12) };
        var subtitle = new Label { Text = "Live hardware temperatures", ForeColor = Color.FromArgb(155, 165, 180), AutoSize = true, Location = new Point(21, 51) };
        header.Controls.Add(title);
        header.Controls.Add(subtitle);

        list.Dock = DockStyle.Fill;
        list.FlowDirection = FlowDirection.TopDown;
        list.WrapContents = false;
        list.AutoScroll = true;
        list.Padding = new Padding(16, 8, 16, 12);
        list.BackColor = BackColor;

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 54, Padding = new Padding(18, 8, 18, 8) };
        status.Dock = DockStyle.Fill;
        status.TextAlign = ContentAlignment.MiddleLeft;
        status.ForeColor = Color.FromArgb(155, 165, 180);
        var refresh = new Button { Text = "Refresh", Dock = DockStyle.Right, Width = 100, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(42, 49, 62), ForeColor = Color.White, Cursor = Cursors.Hand };
        refresh.FlatAppearance.BorderSize = 0;
        refresh.Click += (_, _) => UpdateReadings();
        footer.Controls.Add(status);
        footer.Controls.Add(refresh);

        Controls.Add(list);
        Controls.Add(footer);
        Controls.Add(header);

        Shown += (_, _) =>
        {
            try { computer.Open(); opened = true; }
            catch (Exception ex) { status.Text = "Sensor initialization failed: " + ex.Message; }
            UpdateReadings();
            timer.Tick += (_, _) => UpdateReadings();
            timer.Start();
        };
        FormClosing += (_, _) =>
        {
            timer.Stop();
            if (opened) computer.Close();
        };
    }

    private void UpdateReadings()
    {
        try
        {
            var readings = new List<(string Name, float Value, string Group)>();
            foreach (var hardware in computer.Hardware)
                Collect(hardware, readings);

            list.SuspendLayout();
            list.Controls.Clear();

            foreach (var group in readings.GroupBy(x => x.Group).OrderBy(g => GroupOrder(g.Key)))
            {
                var heading = new Label
                {
                    Text = group.Key.ToUpperInvariant(),
                    Width = 440,
                    Height = 28,
                    ForeColor = Color.FromArgb(135, 155, 180),
                    Font = new Font("Segoe UI Semibold", 9),
                    Padding = new Padding(2, 7, 0, 0)
                };
                list.Controls.Add(heading);

                foreach (var item in group.OrderBy(x => x.Name))
                {
                    var card = new Panel
                    {
                        Width = 440,
                        Height = 66,
                        BackColor = Color.FromArgb(29, 33, 42),
                        Margin = new Padding(0, 0, 0, 9)
                    };
                    var name = new Label
                    {
                        Text = item.Name,
                        Location = new Point(14, 10),
                        AutoSize = true,
                        ForeColor = Color.FromArgb(225, 230, 238),
                        Font = new Font("Segoe UI", 10)
                    };
                    var value = new Label
                    {
                        Text = $"{item.Value:0.0} °C",
                        Location = new Point(300, 13),
                        Size = new Size(125, 32),
                        TextAlign = ContentAlignment.MiddleRight,
                        ForeColor = TemperatureColor(item.Value),
                        Font = new Font("Segoe UI Semibold", 16)
                    };
                    card.Controls.Add(name);
                    card.Controls.Add(value);
                    list.Controls.Add(card);
                }
            }

            list.ResumeLayout();
            status.Text = readings.Count == 0
                ? "No temperature sensors detected. Try running as administrator."
                : $"Updated {DateTime.Now:HH:mm:ss}  •  {readings.Count} sensor(s)";
        }
        catch (Exception ex)
        {
            status.Text = "Could not read sensors: " + ex.Message;
        }
    }

    private static void Collect(IHardware hardware, List<(string Name, float Value, string Group)> output)
    {
        hardware.Update();
        string group = hardware.HardwareType switch
        {
            HardwareType.Cpu => "CPU",
            HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel => "GPU",
            HardwareType.Motherboard => "MOTHERBOARD",
            _ => hardware.Name.ToUpperInvariant()
        };

        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                output.Add((sensor.Name, sensor.Value.Value, group));
        }

        foreach (var sub in hardware.SubHardware)
            Collect(sub, output);
    }

    private static int GroupOrder(string group) => group switch
    {
        "CPU" => 0,
        "GPU" => 1,
        "MOTHERBOARD" => 2,
        _ => 3
    };

    private static Color TemperatureColor(float value) =>
        value >= 85 ? Color.FromArgb(255, 105, 105) :
        value >= 70 ? Color.FromArgb(255, 190, 95) :
        Color.FromArgb(110, 220, 160);
}
