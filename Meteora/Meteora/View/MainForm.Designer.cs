
namespace MeteoraDesktop.View
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.temperature = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.humidity = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.pressure = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.altitude = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.batteryLevel = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // temperature
            // 
            this.temperature.AutoSize = true;
            this.temperature.Font = new System.Drawing.Font("DS-Digital", 22.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.temperature.Location = new System.Drawing.Point(22, 94);
            this.temperature.Name = "temperature";
            this.temperature.Size = new System.Drawing.Size(53, 37);
            this.temperature.TabIndex = 0;
            this.temperature.Text = "--";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.humidity);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.temperature);
            this.groupBox1.Font = new System.Drawing.Font("Calibri", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(273, 245);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "DHT-22";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(26, 149);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(130, 29);
            this.label2.TabIndex = 3;
            this.label2.Text = "Humidity, %";
            // 
            // humidity
            // 
            this.humidity.AutoSize = true;
            this.humidity.Font = new System.Drawing.Font("DS-Digital", 22.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.humidity.Location = new System.Drawing.Point(22, 186);
            this.humidity.Name = "humidity";
            this.humidity.Size = new System.Drawing.Size(53, 37);
            this.humidity.TabIndex = 2;
            this.humidity.Text = "--";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(26, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(164, 29);
            this.label1.TabIndex = 1;
            this.label1.Text = "Temperature, C";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.pressure);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.altitude);
            this.groupBox2.Font = new System.Drawing.Font("Calibri", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(291, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(281, 245);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "BMP-180";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Calibri", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(21, 147);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(166, 29);
            this.label3.TabIndex = 3;
            this.label3.Text = "Pressure, mm rt";
            // 
            // pressure
            // 
            this.pressure.AutoSize = true;
            this.pressure.Font = new System.Drawing.Font("DS-Digital", 22.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pressure.Location = new System.Drawing.Point(17, 183);
            this.pressure.Name = "pressure";
            this.pressure.Size = new System.Drawing.Size(53, 37);
            this.pressure.TabIndex = 2;
            this.pressure.Text = "--";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Calibri", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(22, 56);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(124, 29);
            this.label5.TabIndex = 1;
            this.label5.Text = "Altitude, M";
            // 
            // altitude
            // 
            this.altitude.AutoSize = true;
            this.altitude.Font = new System.Drawing.Font("DS-Digital", 22.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.altitude.Location = new System.Drawing.Point(18, 93);
            this.altitude.Name = "altitude";
            this.altitude.Size = new System.Drawing.Size(53, 37);
            this.altitude.TabIndex = 0;
            this.altitude.Text = "--";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.batteryLevel);
            this.groupBox3.Font = new System.Drawing.Font("Calibri", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(36, 296);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(496, 49);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Battery level, %";
            // 
            // batteryLevel
            // 
            this.batteryLevel.AutoSize = true;
            this.batteryLevel.Font = new System.Drawing.Font("DS-Digital", 22.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.batteryLevel.Location = new System.Drawing.Point(223, 5);
            this.batteryLevel.Name = "batteryLevel";
            this.batteryLevel.Size = new System.Drawing.Size(53, 37);
            this.batteryLevel.TabIndex = 4;
            this.batteryLevel.Text = "--";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(584, 353);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(588, 400);
            this.Name = "MainForm";
            this.Text = "Meteora";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label temperature;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label humidity;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label pressure;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label altitude;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label batteryLevel;
    }
}

