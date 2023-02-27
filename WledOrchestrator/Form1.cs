using Configuration;
using System;
using System.Diagnostics;
using WledOrchestrator.Themes;

namespace WledOrchestrator
{
    public partial class Form1 : Form
    {
        LedTheme currentTheme = new SunTheme();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => this.InvokeIfRequired(this.ForceHide));

            // Load or find Leds
            if (Config.Data.Leds == null)
            {
                WLEDOrchestrator.Leds = WLEDOrchestrator.FindLEDs();
                Config.Data.Leds = WLEDOrchestrator.Leds;
                Config.Save();
            }
            else
                WLEDOrchestrator.Leds = Config.Data.Leds;

            // Add Leds Gui
            foreach (var led in WLEDOrchestrator.Leds)
                ledsPanel.Controls.Add(new Button() { Text = led.address.Split(".").Last(), Bounds = ledButtonTemplate.Bounds });

            // Start Update thread
            Task.Run(() =>
            {
                try
                {
                    var labelUpdateInterval = 1000;
                    var updateInterval = 15000;

                    var luPerUu = updateInterval / labelUpdateInterval;

                    Exception? e = null;

                    while (true)
                    {
                        try
                        {
                            UpdateLEDs();
                            e = null;
                        }
                        catch (Exception ex)
                        {
                            e = ex;
                        }

                        for (int i = 0; i < luPerUu; i++)
                        {
                            stateLabel.InvokeIfRequired(() => stateLabel.Text = $"Status: Updating again in {(luPerUu - i) * labelUpdateInterval / 1000} sec" + (e == null ? "" : $" Recent Exception in Update Thread: {e}"));
                            Thread.Sleep(labelUpdateInterval);
                        }

                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText("Log.txt", $"==={DateTime.Now}==============================\n{ex}\n==============================");
                }
            });
        }

        void UpdateLEDs()
        {
            var curDayTimePercent = DateTime.Now.TimeOfDay.TotalDays;

            var bri = currentTheme.GetBrightness(curDayTimePercent);
            WLEDOrchestrator.SetGlobalBrightness(bri);
            var cols = currentTheme.GetColors(curDayTimePercent);
            WLEDOrchestrator.SetLedColors(cols);

            Debug.WriteLine($"Updated LEDs {bri} {cols}");
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.InvokeIfRequired(this.ForceShow);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                this.ForceHide();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            WLEDOrchestrator.SetGlobalBrightness(0);
        }
    }
}