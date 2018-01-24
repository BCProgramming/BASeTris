namespace BASeTris
{
    partial class BASeTris
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
            this.picTetrisField = new System.Windows.Forms.PictureBox();
            this.picStatistics = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picTetrisField)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStatistics)).BeginInit();
            this.SuspendLayout();
            // 
            // picTetrisField
            // 
            this.picTetrisField.BackColor = System.Drawing.Color.White;
            this.picTetrisField.Location = new System.Drawing.Point(2, 12);
            this.picTetrisField.Name = "picTetrisField";
            this.picTetrisField.Size = new System.Drawing.Size(314, 629);
            this.picTetrisField.TabIndex = 0;
            this.picTetrisField.TabStop = false;
            this.picTetrisField.Click += new System.EventHandler(this.picTetrisField_Click);
            this.picTetrisField.Paint += new System.Windows.Forms.PaintEventHandler(this.picTetrisField_Paint);
            this.picTetrisField.Resize += new System.EventHandler(this.picTetrisField_Resize);
            // 
            // picStatistics
            // 
            this.picStatistics.BackColor = System.Drawing.Color.White;
            this.picStatistics.Location = new System.Drawing.Point(323, 12);
            this.picStatistics.Name = "picStatistics";
            this.picStatistics.Size = new System.Drawing.Size(237, 630);
            this.picStatistics.TabIndex = 1;
            this.picStatistics.TabStop = false;
            // 
            // BASeTris
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(572, 654);
            this.Controls.Add(this.picStatistics);
            this.Controls.Add(this.picTetrisField);
            this.KeyPreview = true;
            this.Name = "BASeTris";
            this.Text = "BASeTris";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.picTetrisField)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStatistics)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picTetrisField;
        private System.Windows.Forms.PictureBox picStatistics;
    }
}

