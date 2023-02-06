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