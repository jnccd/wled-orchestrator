using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WledOrchestrator.Themes
{
    enum ThemeInputVar {
        DayTime
    }

    public class LedTheme
    {
        public virtual byte GetBrightness(double input) => 255;

        public virtual Color[] GetColors(double input) => new Color[] { Color.CornflowerBlue };
    }
}
