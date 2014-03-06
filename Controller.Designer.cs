namespace EyePaint
{
    partial class EyePaintingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        System.ComponentModel.IContainer components = null;

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
        void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EyePaintingForm));
            this.ProgramControlPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.NewSessionButton = new System.Windows.Forms.Button();
            this.SavePaintingButton = new System.Windows.Forms.Button();
            this.ClearPaintingButton = new System.Windows.Forms.Button();
            this.ToolPaneToggleButton = new System.Windows.Forms.Button();
            this.PaintToolsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ColorToolsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.Menu = new System.Windows.Forms.Panel();
            this.ProgramControlPanel.SuspendLayout();
            this.Menu.SuspendLayout();
            this.SuspendLayout();
            // 
            // ProgramControlPanel
            // 
            this.ProgramControlPanel.AutoSize = true;
            this.ProgramControlPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ProgramControlPanel.BackColor = System.Drawing.Color.Transparent;
            this.ProgramControlPanel.Controls.Add(this.NewSessionButton);
            this.ProgramControlPanel.Controls.Add(this.SavePaintingButton);
            this.ProgramControlPanel.Controls.Add(this.ClearPaintingButton);
            this.ProgramControlPanel.Controls.Add(this.ToolPaneToggleButton);
            this.ProgramControlPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.ProgramControlPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.ProgramControlPanel.Location = new System.Drawing.Point(0, 0);
            this.ProgramControlPanel.Name = "ProgramControlPanel";
            this.ProgramControlPanel.Size = new System.Drawing.Size(400, 100);
            this.ProgramControlPanel.TabIndex = 0;
            // 
            // NewSessionButton
            // 
            this.NewSessionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NewSessionButton.AutoSize = true;
            this.NewSessionButton.FlatAppearance.BorderSize = 0;
            this.NewSessionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NewSessionButton.Image = ((System.Drawing.Image)(resources.GetObject("NewSessionButton.Image")));
            this.NewSessionButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.NewSessionButton.Location = new System.Drawing.Point(0, 0);
            this.NewSessionButton.Margin = new System.Windows.Forms.Padding(0);
            this.NewSessionButton.Name = "NewSessionButton";
            this.NewSessionButton.Size = new System.Drawing.Size(100, 100);
            this.NewSessionButton.TabIndex = 0;
            this.NewSessionButton.UseVisualStyleBackColor = true;
            // 
            // SavePaintingButton
            // 
            this.SavePaintingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SavePaintingButton.AutoSize = true;
            this.SavePaintingButton.FlatAppearance.BorderSize = 0;
            this.SavePaintingButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SavePaintingButton.Image = ((System.Drawing.Image)(resources.GetObject("SavePaintingButton.Image")));
            this.SavePaintingButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SavePaintingButton.Location = new System.Drawing.Point(100, 0);
            this.SavePaintingButton.Margin = new System.Windows.Forms.Padding(0);
            this.SavePaintingButton.Name = "SavePaintingButton";
            this.SavePaintingButton.Size = new System.Drawing.Size(100, 100);
            this.SavePaintingButton.TabIndex = 1;
            this.SavePaintingButton.UseVisualStyleBackColor = true;
            // 
            // ClearPaintingButton
            // 
            this.ClearPaintingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ClearPaintingButton.AutoSize = true;
            this.ClearPaintingButton.FlatAppearance.BorderSize = 0;
            this.ClearPaintingButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ClearPaintingButton.Image = ((System.Drawing.Image)(resources.GetObject("ClearPaintingButton.Image")));
            this.ClearPaintingButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ClearPaintingButton.Location = new System.Drawing.Point(200, 0);
            this.ClearPaintingButton.Margin = new System.Windows.Forms.Padding(0);
            this.ClearPaintingButton.Name = "ClearPaintingButton";
            this.ClearPaintingButton.Size = new System.Drawing.Size(100, 100);
            this.ClearPaintingButton.TabIndex = 2;
            this.ClearPaintingButton.UseVisualStyleBackColor = true;
            // 
            // ToolPaneToggleButton
            // 
            this.ToolPaneToggleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ToolPaneToggleButton.AutoSize = true;
            this.ToolPaneToggleButton.FlatAppearance.BorderSize = 0;
            this.ToolPaneToggleButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ToolPaneToggleButton.Image = global::EyePaint.Properties.Resources.color_tool;
            this.ToolPaneToggleButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ToolPaneToggleButton.Location = new System.Drawing.Point(300, 0);
            this.ToolPaneToggleButton.Margin = new System.Windows.Forms.Padding(0);
            this.ToolPaneToggleButton.Name = "ToolPaneToggleButton";
            this.ToolPaneToggleButton.Size = new System.Drawing.Size(100, 100);
            this.ToolPaneToggleButton.TabIndex = 3;
            this.ToolPaneToggleButton.UseVisualStyleBackColor = true;
            // 
            // PaintToolsPanel
            // 
            this.PaintToolsPanel.AutoSize = true;
            this.PaintToolsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PaintToolsPanel.BackColor = System.Drawing.Color.Transparent;
            this.PaintToolsPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.PaintToolsPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.PaintToolsPanel.Location = new System.Drawing.Point(1920, 0);
            this.PaintToolsPanel.Name = "PaintToolsPanel";
            this.PaintToolsPanel.Size = new System.Drawing.Size(0, 100);
            this.PaintToolsPanel.TabIndex = 0;
            // 
            // ColorToolsPanel
            // 
            this.ColorToolsPanel.AutoSize = true;
            this.ColorToolsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ColorToolsPanel.BackColor = System.Drawing.Color.Transparent;
            this.ColorToolsPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.ColorToolsPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.ColorToolsPanel.Location = new System.Drawing.Point(1920, 0);
            this.ColorToolsPanel.Name = "ColorToolsPanel";
            this.ColorToolsPanel.Size = new System.Drawing.Size(0, 100);
            this.ColorToolsPanel.TabIndex = 4;
            this.ColorToolsPanel.Visible = false;
            // 
            // Menu
            // 
            this.Menu.AutoSize = true;
            this.Menu.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Menu.BackColor = System.Drawing.Color.White;
            this.Menu.Controls.Add(this.ProgramControlPanel);
            this.Menu.Controls.Add(this.PaintToolsPanel);
            this.Menu.Controls.Add(this.ColorToolsPanel);
            this.Menu.Dock = System.Windows.Forms.DockStyle.Top;
            this.Menu.Location = new System.Drawing.Point(0, 0);
            this.Menu.Margin = new System.Windows.Forms.Padding(0);
            this.Menu.MinimumSize = new System.Drawing.Size(0, 100);
            this.Menu.Name = "Menu";
            this.Menu.Size = new System.Drawing.Size(1920, 100);
            this.Menu.TabIndex = 6;
            // 
            // EyePaintingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1920, 1080);
            this.ControlBox = false;
            this.Controls.Add(this.Menu);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "EyePaintingForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ProgramControlPanel.ResumeLayout(false);
            this.ProgramControlPanel.PerformLayout();
            this.Menu.ResumeLayout(false);
            this.Menu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel ProgramControlPanel;
        private System.Windows.Forms.Button NewSessionButton;
        private System.Windows.Forms.Button SavePaintingButton;
        private System.Windows.Forms.Button ClearPaintingButton;
        private System.Windows.Forms.Button ToolPaneToggleButton;
        private System.Windows.Forms.FlowLayoutPanel PaintToolsPanel;
        private System.Windows.Forms.FlowLayoutPanel ColorToolsPanel;
        private System.Windows.Forms.Panel Menu;
    }
}

