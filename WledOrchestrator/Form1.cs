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

            if (Config.Data.Leds == null)
            {
                WLEDOrchestrator.Leds = WLEDOrchestrator.FindLEDs();
                Config.Data.Leds = WLEDOrchestrator.Leds;
                Config.Save();
            }
            else
                WLEDOrchestrator.Leds = Config.Data.Leds;

            // Add leds Gui
            foreach (var led in WLEDOrchestrator.Leds)
                ledsPanel.Controls.Add(new Button() { Text = led.address.Split(".").Last(), Bounds = ledButtonTemplate.Bounds });

            // Update thread
            Task.Run(() =>
            {
                var labelUpdateInterval = 1000;
                var updateInterval = 15000;

                var luPerUu = updateInterval / labelUpdateInterval;

                while (true)
                {
                    UpdateLEDs();

                    for (int i = 0; i < luPerUu; i++)
                    {
                        stateLabel.InvokeIfRequired(() => stateLabel.Text = $"Status: Updating again in {(luPerUu - i) * labelUpdateInterval / 1000} sec");
                        Thread.Sleep(labelUpdateInterval);
                    }
                    
                }
            });
        }

        void UpdateLEDs()
        {
            var curDayTimePercent = DateTime.Now.TimeOfDay.TotalDays;

            WLEDOrchestrator.SetGlobalBrightness(currentTheme.GetBrightness(curDayTimePercent));
            WLEDOrchestrator.SetLedColors(currentTheme.GetColors(curDayTimePercent));

            Debug.WriteLine("Updated LEDs");
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