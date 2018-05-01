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
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scaleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScale_Tiny = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScale_Small = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScale_Large = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScale_Biggliest = new System.Windows.Forms.ToolStripMenuItem();
            this.controlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.picFullSize = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picTetrisField)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStatistics)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picFullSize)).BeginInit();
            this.SuspendLayout();
            // 
            // picTetrisField
            // 
            this.picTetrisField.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.picStatistics.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.gameToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.controlToolStripMenuItem});
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
            this.newGameToolStripMenuItem.Size = new System.Drawing.Size(157, 26);
            this.newGameToolStripMenuItem.Text = "New Game";
            this.newGameToolStripMenuItem.Click += new System.EventHandler(this.newGameToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scaleToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(53, 24);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // scaleToolStripMenuItem
            // 
            this.scaleToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuScale_Tiny,
            this.mnuScale_Small,
            this.mnuScale_Large,
            this.mnuScale_Biggliest});
            this.scaleToolStripMenuItem.Name = "scaleToolStripMenuItem";
            this.scaleToolStripMenuItem.Size = new System.Drawing.Size(119, 26);
            this.scaleToolStripMenuItem.Text = "Scale";
            // 
            // mnuScale_Tiny
            // 
            this.mnuScale_Tiny.Name = "mnuScale_Tiny";
            this.mnuScale_Tiny.Size = new System.Drawing.Size(142, 26);
            this.mnuScale_Tiny.Text = "Tiny";
            this.mnuScale_Tiny.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // mnuScale_Small
            // 
            this.mnuScale_Small.Name = "mnuScale_Small";
            this.mnuScale_Small.Size = new System.Drawing.Size(142, 26);
            this.mnuScale_Small.Text = "Small";
            this.mnuScale_Small.Click += new System.EventHandler(this.xToolStripMenuItem_Click);
            // 
            // mnuScale_Large
            // 
            this.mnuScale_Large.Name = "mnuScale_Large";
            this.mnuScale_Large.Size = new System.Drawing.Size(142, 26);
            this.mnuScale_Large.Text = "Large";
            this.mnuScale_Large.Click += new System.EventHandler(this.xToolStripMenuItem1_Click);
            // 
            // mnuScale_Biggliest
            // 
            this.mnuScale_Biggliest.Name = "mnuScale_Biggliest";
            this.mnuScale_Biggliest.Size = new System.Drawing.Size(142, 26);
            this.mnuScale_Biggliest.Text = "Biggliest";
            this.mnuScale_Biggliest.Click += new System.EventHandler(this.xToolStripMenuItem2_Click);
            // 
            // controlToolStripMenuItem
            // 
            this.controlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aIToolStripMenuItem});
            this.controlToolStripMenuItem.Name = "controlToolStripMenuItem";
            this.controlToolStripMenuItem.Size = new System.Drawing.Size(70, 24);
            this.controlToolStripMenuItem.Text = "Control";
            // 
            // aIToolStripMenuItem
            // 
            this.aIToolStripMenuItem.Name = "aIToolStripMenuItem";
            this.aIToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.aIToolStripMenuItem.Size = new System.Drawing.Size(150, 26);
            this.aIToolStripMenuItem.Text = "AI";
            this.aIToolStripMenuItem.Click += new System.EventHandler(this.aIToolStripMenuItem_Click);
            // 
            // picFullSize
            // 
            this.picFullSize.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picFullSize.BackColor = System.Drawing.Color.White;
            this.picFullSize.Location = new System.Drawing.Point(4, 39);
            this.picFullSize.Name = "picFullSize";
            this.picFullSize.Size = new System.Drawing.Size(618, 644);
            this.picFullSize.TabIndex = 3;
            this.picFullSize.TabStop = false;
            this.picFullSize.Visible = false;
            this.picFullSize.Paint += new System.Windows.Forms.PaintEventHandler(this.picFullSize_Paint);
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
            this.Controls.Add(this.picFullSize);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BASeTris";
            this.Text = "BASeTris";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResizeEnd += new System.EventHandler(this.BASeTris_ResizeEnd);
            this.SizeChanged += new System.EventHandler(this.BASeTris_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BASeTris_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.BASeTris_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.picTetrisField)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStatistics)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picFullSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picTetrisField;
        private System.Windows.Forms.PictureBox picStatistics;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem gameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newGameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scaleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuScale_Tiny;
        private System.Windows.Forms.ToolStripMenuItem mnuScale_Small;
        private System.Windows.Forms.ToolStripMenuItem mnuScale_Large;
        private System.Windows.Forms.ToolStripMenuItem mnuScale_Biggliest;
        private System.Windows.Forms.ToolStripMenuItem controlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aIToolStripMenuItem;
        private System.Windows.Forms.PictureBox picFullSize;
    }
}

