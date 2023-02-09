using System.Diagnostics;

namespace WledOrchestrator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Enabled = false;
            Task.Factory.StartNew(() => { this.InvokeIfRequired(() => { this.ForceHide(); }); });
            WLEDOrchestrator.FindLEDs();
            this.Enabled = true;
            Debug.WriteLine("Done");

            WLEDOrchestrator.SetGlobalBrightness(16);
            WLEDOrchestrator.SetLedColors(new Color[] { Color.Orange, Color.OrangeRed, Color.Crimson, Color.MintCream });
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.InvokeIfRequired(() => { this.ForceShow(); });
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