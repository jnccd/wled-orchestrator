using System.Diagnostics;

namespace WledOrchestrator
{
    public partial class Form1 : Form
    {
        Color SkyColor = Color.FromArgb(142, 215, 253);
        Color SunColor = Color.FromArgb(252, 200, 20);
        int ColorArrayResolution = 300;
        static double sunRise = 0.3;
        Func<double, double> DayLightFunction = (x) => Math.Sin(2 * (x - sunRise) * Math.PI);
        double invertedSunSize = 150;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => this.InvokeIfRequired(this.ForceHide));
            WLEDOrchestrator.FindLEDs();

            foreach (var led in WLEDOrchestrator.Leds)
                ledsPanel.Controls.Add(new Button() { Text = led.address.Split(".").Last(), Bounds = ledButtonTemplate.Bounds });

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(30_000);
                    UpdateLEDs();
                }
            });
        }

        void UpdateLEDs()
        {
            // Set Brightness
            var curDayTimePercent = DateTime.Now.TimeOfDay.TotalDays;
            {
                var funOut = DayLightFunction(curDayTimePercent);

                funOut = (funOut + 0.5) / 1.5;
                funOut = funOut < 0.01 ? 0.01 : funOut;

                WLEDOrchestrator.SetGlobalBrightness((int)(funOut * 255));
            }

            // Create Color Array
            Color[] colors = new Color[ColorArrayResolution];
            for (int i = 0; i < ColorArrayResolution; i++)
            {
                var sunRiseDayTime = curDayTimePercent - sunRise;
                var x = i - sunRiseDayTime;
                var gaussianSun = Math.Exp(-(x * x) * invertedSunSize);
                colors[i] = SkyColor.Lerp(SunColor, gaussianSun);
            }
            WLEDOrchestrator.SetLedColors(colors);

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