using System.Diagnostics;

namespace WledOrchestrator
{
    public partial class Form1 : Form
    {
        Color SkyColor = Color.FromArgb(142, 215, 253);
        Color SunColor = Color.FromArgb(252, 200, 20);
        int ColorArrayResolution = 300;
        static double sunRise = 0.3;
        static double sunTime = 0.5;

        static double halfSunTime = sunTime / 2;
        static double sunTop = sunRise + halfSunTime;
        static double sunSet = sunRise + sunTime;

        static double dayLightFunMult = Math.Log2(0.05) / (halfSunTime * halfSunTime);
        Func<double, double> DayLightFunction = (x) => Math.Pow(2, -((x-sunTop)*(x-sunTop)) * dayLightFunMult);

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
                    Thread.Sleep(15_000);
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
                WLEDOrchestrator.SetGlobalBrightness((int)(funOut * 255));
            }

            // Create Color Array
            Color[] colors = new Color[ColorArrayResolution];
            for (int i = 0; i < ColorArrayResolution; i++)
            {
                var sunRiseDayTime = curDayTimePercent - sunRise;
                if (sunRiseDayTime < 0)
                    sunRiseDayTime += 1;

                var x = (i / (double)ColorArrayResolution) - sunRiseDayTime;
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