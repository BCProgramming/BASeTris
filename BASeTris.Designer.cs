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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BASeTris));
            picTetrisField = new PictureBox();
            picStatistics = new PictureBox();
            menuStrip1 = new MenuStrip();
            gameToolStripMenuItem = new ToolStripMenuItem();
            newGameToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            scaleToolStripMenuItem = new ToolStripMenuItem();
            mnuScale_Tiny = new ToolStripMenuItem();
            mnuScale_Small = new ToolStripMenuItem();
            mnuScale_Large = new ToolStripMenuItem();
            mnuScale_Biggliest = new ToolStripMenuItem();
            controlToolStripMenuItem = new ToolStripMenuItem();
            aIToolStripMenuItem = new ToolStripMenuItem();
            picFullSize = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)picTetrisField).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picStatistics).BeginInit();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picFullSize).BeginInit();
            SuspendLayout();
            // 
            // picTetrisField
            // 
            picTetrisField.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picTetrisField.BackColor = Color.White;
            picTetrisField.Location = new Point(5, 58);
            picTetrisField.Margin = new Padding(4, 4, 4, 4);
            picTetrisField.Name = "picTetrisField";
            picTetrisField.Size = new Size(390, 962);
            picTetrisField.TabIndex = 0;
            picTetrisField.TabStop = false;
            picTetrisField.Click += picTetrisField_Click;
            picTetrisField.Paint += picTetrisField_Paint;
            picTetrisField.MouseClick += picTetrisField_MouseClick;
            picTetrisField.Resize += picTetrisField_Resize;
            // 
            // picStatistics
            // 
            picStatistics.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            picStatistics.BackColor = Color.Black;
            picStatistics.Location = new Point(402, 58);
            picStatistics.Margin = new Padding(4, 4, 4, 4);
            picStatistics.Name = "picStatistics";
            picStatistics.Size = new Size(350, 966);
            picStatistics.TabIndex = 1;
            picStatistics.TabStop = false;
            picStatistics.Paint += picStatistics_Paint;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { gameToolStripMenuItem, viewToolStripMenuItem, controlToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(8, 3, 0, 3);
            menuStrip1.Size = new Size(756, 34);
            menuStrip1.TabIndex = 2;
            menuStrip1.Text = "menuStrip1";
            // 
            // gameToolStripMenuItem
            // 
            gameToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newGameToolStripMenuItem });
            gameToolStripMenuItem.Name = "gameToolStripMenuItem";
            gameToolStripMenuItem.Size = new Size(72, 28);
            gameToolStripMenuItem.Text = "Game";
            // 
            // newGameToolStripMenuItem
            // 
            newGameToolStripMenuItem.Name = "newGameToolStripMenuItem";
            newGameToolStripMenuItem.Size = new Size(196, 34);
            newGameToolStripMenuItem.Text = "New Game";
            newGameToolStripMenuItem.Click += newGameToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { scaleToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(63, 28);
            viewToolStripMenuItem.Text = "View";
            // 
            // scaleToolStripMenuItem
            // 
            scaleToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mnuScale_Tiny, mnuScale_Small, mnuScale_Large, mnuScale_Biggliest });
            scaleToolStripMenuItem.Name = "scaleToolStripMenuItem";
            scaleToolStripMenuItem.Size = new Size(150, 34);
            scaleToolStripMenuItem.Text = "Scale";
            // 
            // mnuScale_Tiny
            // 
            mnuScale_Tiny.Name = "mnuScale_Tiny";
            mnuScale_Tiny.Size = new Size(177, 34);
            mnuScale_Tiny.Text = "Tiny";
            mnuScale_Tiny.Click += toolStripMenuItem2_Click;
            // 
            // mnuScale_Small
            // 
            mnuScale_Small.Name = "mnuScale_Small";
            mnuScale_Small.Size = new Size(177, 34);
            mnuScale_Small.Text = "Small";
            mnuScale_Small.Click += xToolStripMenuItem_Click;
            // 
            // mnuScale_Large
            // 
            mnuScale_Large.Name = "mnuScale_Large";
            mnuScale_Large.Size = new Size(177, 34);
            mnuScale_Large.Text = "Large";
            mnuScale_Large.Click += xToolStripMenuItem1_Click;
            // 
            // mnuScale_Biggliest
            // 
            mnuScale_Biggliest.Name = "mnuScale_Biggliest";
            mnuScale_Biggliest.Size = new Size(177, 34);
            mnuScale_Biggliest.Text = "Biggliest";
            mnuScale_Biggliest.Click += xToolStripMenuItem2_Click;
            // 
            // controlToolStripMenuItem
            // 
            controlToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aIToolStripMenuItem });
            controlToolStripMenuItem.Name = "controlToolStripMenuItem";
            controlToolStripMenuItem.Size = new Size(85, 28);
            controlToolStripMenuItem.Text = "Control";
            // 
            // aIToolStripMenuItem
            // 
            aIToolStripMenuItem.Name = "aIToolStripMenuItem";
            aIToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.A;
            aIToolStripMenuItem.Size = new Size(188, 34);
            aIToolStripMenuItem.Text = "AI";
            aIToolStripMenuItem.Click += aIToolStripMenuItem_Click;
            // 
            // picFullSize
            // 
            picFullSize.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picFullSize.BackColor = Color.White;
            picFullSize.Location = new Point(5, 58);
            picFullSize.Margin = new Padding(4, 4, 4, 4);
            picFullSize.Name = "picFullSize";
            picFullSize.Size = new Size(748, 966);
            picFullSize.TabIndex = 3;
            picFullSize.TabStop = false;
            picFullSize.Visible = false;
            picFullSize.Paint += picFullSize_Paint;
            // 
            // BASeTris
            // 
            AutoScaleDimensions = new SizeF(10F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DimGray;
            ClientSize = new Size(756, 1030);
            Controls.Add(picStatistics);
            Controls.Add(picTetrisField);
            Controls.Add(menuStrip1);
            Controls.Add(picFullSize);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStrip1;
            Margin = new Padding(4, 4, 4, 4);
            Name = "BASeTris";
            Text = "BASeTris";
            FormClosing += BASeTris_FormClosing;
            Load += BASeTris_Load;
            ResizeEnd += BASeTris_ResizeEnd;
            SizeChanged += BASeTris_SizeChanged;
            Enter += BASeTris_Enter;
            KeyDown += BASeTris_KeyDown;
            KeyPress += BASeTris_KeyPress;
            KeyUp += BASeTris_KeyUp;
            Leave += BASeTris_Leave;
            ((System.ComponentModel.ISupportInitialize)picTetrisField).EndInit();
            ((System.ComponentModel.ISupportInitialize)picStatistics).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picFullSize).EndInit();
            ResumeLayout(false);
            PerformLayout();
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

