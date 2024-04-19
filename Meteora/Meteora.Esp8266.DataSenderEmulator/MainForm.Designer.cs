namespace Meteora.Esp8266.DataSenderEmulator
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.runButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.sendTimeouts = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.currentIpLabel = new System.Windows.Forms.Label();
            this.networkAdapters = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.port = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(220, 150);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(125, 30);
            this.runButton.TabIndex = 0;
            this.runButton.Text = "Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.OnRunClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(217, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Send period, sec";
            // 
            // sendTimeouts
            // 
            this.sendTimeouts.FormattingEnabled = true;
            this.sendTimeouts.Location = new System.Drawing.Point(12, 107);
            this.sendTimeouts.Name = "sendTimeouts";
            this.sendTimeouts.Size = new System.Drawing.Size(121, 24);
            this.sendTimeouts.TabIndex = 4;
            this.sendTimeouts.SelectedIndexChanged += new System.EventHandler(this.OnSendTimeoutsChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(217, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(129, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "Selected IP address";
            // 
            // currentIpLabel
            // 
            this.currentIpLabel.AutoSize = true;
            this.currentIpLabel.Location = new System.Drawing.Point(217, 115);
            this.currentIpLabel.Name = "currentIpLabel";
            this.currentIpLabel.Size = new System.Drawing.Size(19, 16);
            this.currentIpLabel.TabIndex = 7;
            this.currentIpLabel.Text = "-/-";
            // 
            // networkAdapters
            // 
            this.networkAdapters.FormattingEnabled = true;
            this.networkAdapters.Location = new System.Drawing.Point(12, 43);
            this.networkAdapters.Name = "networkAdapters";
            this.networkAdapters.Size = new System.Drawing.Size(121, 24);
            this.networkAdapters.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(106, 16);
            this.label4.TabIndex = 8;
            this.label4.Text = "Network adapter";
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(220, 45);
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(125, 22);
            this.port.TabIndex = 10;
            this.port.Text = "8080";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(357, 203);
            this.Controls.Add(this.port);
            this.Controls.Add(this.networkAdapters);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.currentIpLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.sendTimeouts);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.runButton);
            this.Name = "MainForm";
            this.Text = "Esp8266 DataSenderEmulator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox sendTimeouts;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label currentIpLabel;
        private System.Windows.Forms.ComboBox networkAdapters;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox port;
    }
}

