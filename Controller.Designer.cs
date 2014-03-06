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
            resources.ApplyResources(this.ProgramControlPanel, "ProgramControlPanel");
            this.ProgramControlPanel.BackColor = System.Drawing.Color.Transparent;
            this.ProgramControlPanel.Controls.Add(this.NewSessionButton);
            this.ProgramControlPanel.Controls.Add(this.SavePaintingButton);
            this.ProgramControlPanel.Controls.Add(this.ClearPaintingButton);
            this.ProgramControlPanel.Controls.Add(this.ToolPaneToggleButton);
            this.ProgramControlPanel.Name = "ProgramControlPanel";
            // 
            // NewSessionButton
            // 
            resources.ApplyResources(this.NewSessionButton, "NewSessionButton");
            this.NewSessionButton.FlatAppearance.BorderSize = 0;
            this.NewSessionButton.Image = global::EyePaint.Properties.Resources.exit_program;
            this.NewSessionButton.Name = "NewSessionButton";
            this.NewSessionButton.UseVisualStyleBackColor = true;
            // 
            // SavePaintingButton
            // 
            resources.ApplyResources(this.SavePaintingButton, "SavePaintingButton");
            this.SavePaintingButton.FlatAppearance.BorderSize = 0;
            this.SavePaintingButton.Image = global::EyePaint.Properties.Resources.save_painting;
            this.SavePaintingButton.Name = "SavePaintingButton";
            this.SavePaintingButton.UseVisualStyleBackColor = true;
            // 
            // ClearPaintingButton
            // 
            resources.ApplyResources(this.ClearPaintingButton, "ClearPaintingButton");
            this.ClearPaintingButton.FlatAppearance.BorderSize = 0;
            this.ClearPaintingButton.Image = global::EyePaint.Properties.Resources.new_painting;
            this.ClearPaintingButton.Name = "ClearPaintingButton";
            this.ClearPaintingButton.UseVisualStyleBackColor = true;
            // 
            // ToolPaneToggleButton
            // 
            resources.ApplyResources(this.ToolPaneToggleButton, "ToolPaneToggleButton");
            this.ToolPaneToggleButton.FlatAppearance.BorderSize = 0;
            this.ToolPaneToggleButton.Image = global::EyePaint.Properties.Resources.color_tool;
            this.ToolPaneToggleButton.Name = "ToolPaneToggleButton";
            this.ToolPaneToggleButton.UseVisualStyleBackColor = true;
            // 
            // PaintToolsPanel
            // 
            resources.ApplyResources(this.PaintToolsPanel, "PaintToolsPanel");
            this.PaintToolsPanel.BackColor = System.Drawing.Color.Transparent;
            this.PaintToolsPanel.Name = "PaintToolsPanel";
            // 
            // ColorToolsPanel
            // 
            resources.ApplyResources(this.ColorToolsPanel, "ColorToolsPanel");
            this.ColorToolsPanel.BackColor = System.Drawing.Color.Transparent;
            this.ColorToolsPanel.Name = "ColorToolsPanel";
            // 
            // Menu
            // 
            resources.ApplyResources(this.Menu, "Menu");
            this.Menu.BackColor = System.Drawing.Color.White;
            this.Menu.Controls.Add(this.ProgramControlPanel);
            this.Menu.Controls.Add(this.PaintToolsPanel);
            this.Menu.Controls.Add(this.ColorToolsPanel);
            this.Menu.Name = "Menu";
            // 
            // EyePaintingForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ControlBox = false;
            this.Controls.Add(this.Menu);
            this.Cursor = System.Windows.Forms.Cursors.Cross;
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "EyePaintingForm";
            this.ShowIcon = false;
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

