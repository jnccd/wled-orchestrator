namespace WledOrchestrator
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.ledsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ledButtonTemplate = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.stateLabel = new System.Windows.Forms.Label();
            this.ledsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ledsPanel
            // 
            this.ledsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ledsPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ledsPanel.Controls.Add(this.ledButtonTemplate);
            this.ledsPanel.Location = new System.Drawing.Point(12, 12);
            this.ledsPanel.Name = "ledsPanel";
            this.ledsPanel.Size = new System.Drawing.Size(776, 57);
            this.ledsPanel.TabIndex = 0;
            // 
            // ledButtonTemplate
            // 
            this.ledButtonTemplate.Location = new System.Drawing.Point(3, 3);
            this.ledButtonTemplate.Name = "ledButtonTemplate";
            this.ledButtonTemplate.Size = new System.Drawing.Size(67, 49);
            this.ledButtonTemplate.TabIndex = 1;
            this.ledButtonTemplate.Text = "button1";
            this.ledButtonTemplate.UseVisualStyleBackColor = true;
            this.ledButtonTemplate.Visible = false;
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "WLED Orchestrator";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // stateLabel
            // 
            this.stateLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.stateLabel.AutoSize = true;
            this.stateLabel.Location = new System.Drawing.Point(12, 426);
            this.stateLabel.Name = "stateLabel";
            this.stateLabel.Size = new System.Drawing.Size(42, 15);
            this.stateLabel.TabIndex = 1;
            this.stateLabel.Text = "Status:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.stateLabel);
            this.Controls.Add(this.ledsPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "WLED Orchestrator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.ledsPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FlowLayoutPanel ledsPanel;
        private Button ledButtonTemplate;
        private NotifyIcon notifyIcon;
        private Label stateLabel;
    }
}