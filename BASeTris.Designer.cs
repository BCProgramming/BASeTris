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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.gameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.picTetrisField)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStatistics)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picTetrisField
            // 
            this.picTetrisField.BackColor = System.Drawing.Color.White;
            this.picTetrisField.Location = new System.Drawing.Point(4, 39);
            this.picTetrisField.Name = "picTetrisField";
            this.picTetrisField.Size = new System.Drawing.Size(332, 641);
            this.picTetrisField.TabIndex = 0;
            this.picTetrisField.TabStop = false;
            this.picTetrisField.Click += new System.EventHandler(this.picTetrisField_Click);
            this.picTetrisField.Paint += new System.Windows.Forms.PaintEventHandler(this.picTetrisField_Paint);
            this.picTetrisField.Resize += new System.EventHandler(this.picTetrisField_Resize);
            // 
            // picStatistics
            // 
            this.picStatistics.BackColor = System.Drawing.Color.Black;
            this.picStatistics.Location = new System.Drawing.Point(342, 39);
            this.picStatistics.Name = "picStatistics";
            this.picStatistics.Size = new System.Drawing.Size(280, 644);
            this.picStatistics.TabIndex = 1;
            this.picStatistics.TabStop = false;
            this.picStatistics.Paint += new System.Windows.Forms.PaintEventHandler(this.picStatistics_Paint);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gameToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(625, 28);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // gameToolStripMenuItem
            // 
            this.gameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newGameToolStripMenuItem});
            this.gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            this.gameToolStripMenuItem.Size = new System.Drawing.Size(60, 24);
            this.gameToolStripMenuItem.Text = "Game";
            // 
            // newGameToolStripMenuItem
            // 
            this.newGameToolStripMenuItem.Name = "newGameToolStripMenuItem";
            this.newGameToolStripMenuItem.Size = new System.Drawing.Size(181, 26);
            this.newGameToolStripMenuItem.Text = "New Game";
            this.newGameToolStripMenuItem.Click += new System.EventHandler(this.newGameToolStripMenuItem_Click);
            // 
            // BASeTris
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(625, 687);
            this.Controls.Add(this.picStatistics);
            this.Controls.Add(this.picTetrisField);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "BASeTris";
            this.Text = "BASeTris";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.picTetrisField)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStatistics)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picTetrisField;
        private System.Windows.Forms.PictureBox picStatistics;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem gameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newGameToolStripMenuItem;
    }
}

