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
            Task.Factory.StartNew(() => { this.InvokeIfRequired(() => { this.ForceHide(); }); });

            WLEDOrchestrator.FindLEDs();
            Debug.WriteLine("Done");

            WLEDOrchestrator.SetGlobalBrightness(32);
            WLEDOrchestrator.SetLedColors(new Color[] { Color.Azure,Color.CornflowerBlue,Color.Cyan,Color.Coral,Color.Crimson });
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
    }
}