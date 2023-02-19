using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WledOrchestrator.Themes
{
    public class SunTheme : LedTheme
    {
        Color SkyColor = Color.FromArgb(142, 215, 253);
        Color SunColor = Color.FromArgb(252, 200, 20);
        int ColorArrayResolution = 300;
        static double sunRise = 0.3;
        static double sunTime = 0.5;

        static double halfSunTime = sunTime / 2;
        static double sunTop = sunRise + halfSunTime;
        static double sunSet = sunRise + sunTime;

        static double dayLightFunMult = -Math.Log2(0.05) / (halfSunTime * halfSunTime);
        Func<double, double> DayLightFunction = (x) => Math.Pow(2, -((x - sunTop) * (x - sunTop)) * dayLightFunMult);

        double invertedSunSize = 150;

        public override byte GetBrightness(double input)
        {
            // Set Brightness
            var curDayTimePercent = DateTime.Now.TimeOfDay.TotalDays;
            var funOut = DayLightFunction(curDayTimePercent);
            var bri = (int)(funOut * 255) + 0;
            WLEDOrchestrator.SetGlobalBrightness(bri);

            return base.GetBrightness(input);
        }

        public override Color[] GetColors(double input)
        {
            // Create Color Array
            Color[] colors = new Color[ColorArrayResolution];
            for (int i = 0; i < ColorArrayResolution; i++)
            {
                var sunRiseDayTime = input / sunTime - sunRise;

                var x = (i / (double)ColorArrayResolution) - sunRiseDayTime;
                var gaussianSun = Math.Exp(-(x * x) * invertedSunSize);
                colors[i] = SkyColor.Lerp(SunColor, gaussianSun);
            }
            WLEDOrchestrator.SetLedColors(colors);

            return base.GetColors(input);
        }
    }
}
