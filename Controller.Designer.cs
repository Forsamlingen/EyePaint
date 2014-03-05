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
            this.SetupPanel = new System.Windows.Forms.Panel();
            this.EnableEyeTrackerButton = new System.Windows.Forms.Button();
            this.EnableMouseButton = new System.Windows.Forms.Button();
            this.CloseSetupPanelButton = new System.Windows.Forms.Button();
            this.OpenControlPanelButton = new System.Windows.Forms.Button();
            this.SetupMessage = new System.Windows.Forms.Label();
            this.ProgramControlPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.NewPaintingButton = new System.Windows.Forms.Button();
            this.SavePaintingButton = new System.Windows.Forms.Button();
            this.ClearPaintingButton = new System.Windows.Forms.Button();
            this.ToolPaneToggleButton = new System.Windows.Forms.Button();
            this.PaintToolsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ColorToolsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.Menu = new System.Windows.Forms.Panel();
            this.SetupPanel.SuspendLayout();
            this.ProgramControlPanel.SuspendLayout();
            this.PaintToolsPanel.SuspendLayout();
            this.Menu.SuspendLayout();
            this.SuspendLayout();
            // 
            // SetupPanel
            // 
            resources.ApplyResources(this.SetupPanel, "SetupPanel");
            this.SetupPanel.BackColor = System.Drawing.Color.White;
            this.SetupPanel.Controls.Add(this.EnableEyeTrackerButton);
            this.SetupPanel.Controls.Add(this.EnableMouseButton);
            this.SetupPanel.Controls.Add(this.CloseSetupPanelButton);
            this.SetupPanel.Controls.Add(this.OpenControlPanelButton);
            this.SetupPanel.Controls.Add(this.SetupMessage);
            this.SetupPanel.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.SetupPanel.Name = "SetupPanel";
            // 
            // EnableEyeTrackerButton
            // 
            this.EnableEyeTrackerButton.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.EnableEyeTrackerButton, "EnableEyeTrackerButton");
            this.EnableEyeTrackerButton.Name = "EnableEyeTrackerButton";
            this.EnableEyeTrackerButton.UseVisualStyleBackColor = true;
            // 
            // EnableMouseButton
            // 
            this.EnableMouseButton.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.EnableMouseButton, "EnableMouseButton");
            this.EnableMouseButton.Name = "EnableMouseButton";
            this.EnableMouseButton.UseVisualStyleBackColor = true;
            // 
            // CloseSetupPanelButton
            // 
            resources.ApplyResources(this.CloseSetupPanelButton, "CloseSetupPanelButton");
            this.CloseSetupPanelButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.CloseSetupPanelButton.Name = "CloseSetupPanelButton";
            this.CloseSetupPanelButton.UseVisualStyleBackColor = true;
            // 
            // OpenControlPanelButton
            // 
            this.OpenControlPanelButton.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.OpenControlPanelButton, "OpenControlPanelButton");
            this.OpenControlPanelButton.Name = "OpenControlPanelButton";
            this.OpenControlPanelButton.UseVisualStyleBackColor = true;
            // 
            // SetupMessage
            // 
            resources.ApplyResources(this.SetupMessage, "SetupMessage");
            this.SetupMessage.Name = "SetupMessage";
            // 
            // ProgramControlPanel
            // 
            resources.ApplyResources(this.ProgramControlPanel, "ProgramControlPanel");
            this.ProgramControlPanel.BackColor = System.Drawing.Color.Blue;
            this.ProgramControlPanel.Controls.Add(this.NewPaintingButton);
            this.ProgramControlPanel.Controls.Add(this.SavePaintingButton);
            this.ProgramControlPanel.Controls.Add(this.ClearPaintingButton);
            this.ProgramControlPanel.Controls.Add(this.ToolPaneToggleButton);
            this.ProgramControlPanel.Name = "ProgramControlPanel";
            // 
            // NewPaintingButton
            // 
            resources.ApplyResources(this.NewPaintingButton, "NewPaintingButton");
            this.NewPaintingButton.Name = "NewPaintingButton";
            this.NewPaintingButton.UseVisualStyleBackColor = true;
            // 
            // SavePaintingButton
            // 
            resources.ApplyResources(this.SavePaintingButton, "SavePaintingButton");
            this.SavePaintingButton.Name = "SavePaintingButton";
            this.SavePaintingButton.UseVisualStyleBackColor = true;
            // 
            // ClearPaintingButton
            // 
            resources.ApplyResources(this.ClearPaintingButton, "ClearPaintingButton");
            this.ClearPaintingButton.Name = "ClearPaintingButton";
            this.ClearPaintingButton.UseVisualStyleBackColor = true;
            // 
            // ToolPaneToggleButton
            // 
            resources.ApplyResources(this.ToolPaneToggleButton, "ToolPaneToggleButton");
            this.ToolPaneToggleButton.Name = "ToolPaneToggleButton";
            this.ToolPaneToggleButton.UseVisualStyleBackColor = true;
            // 
            // PaintToolsPanel
            // 
            resources.ApplyResources(this.PaintToolsPanel, "PaintToolsPanel");
            this.PaintToolsPanel.BackColor = System.Drawing.Color.Yellow;
            this.PaintToolsPanel.Controls.Add(this.ColorToolsPanel);
            this.PaintToolsPanel.Name = "PaintToolsPanel";
            // 
            // ColorToolsPanel
            // 
            resources.ApplyResources(this.ColorToolsPanel, "ColorToolsPanel");
            this.ColorToolsPanel.BackColor = System.Drawing.Color.Turquoise;
            this.ColorToolsPanel.Name = "ColorToolsPanel";
            // 
            // Menu
            // 
            resources.ApplyResources(this.Menu, "Menu");
            this.Menu.BackColor = System.Drawing.Color.Maroon;
            this.Menu.Controls.Add(this.ProgramControlPanel);
            this.Menu.Controls.Add(this.PaintToolsPanel);
            this.Menu.Name = "Menu";
            // 
            // EyePaintingForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ControlBox = false;
            this.Controls.Add(this.SetupPanel);
            this.Controls.Add(this.Menu);
            this.Cursor = System.Windows.Forms.Cursors.Cross;
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "EyePaintingForm";
            this.ShowIcon = false;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.SetupPanel.ResumeLayout(false);
            this.ProgramControlPanel.ResumeLayout(false);
            this.PaintToolsPanel.ResumeLayout(false);
            this.PaintToolsPanel.PerformLayout();
            this.Menu.ResumeLayout(false);
            this.Menu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        System.Windows.Forms.Panel SetupPanel;
        System.Windows.Forms.Button OpenControlPanelButton;
        System.Windows.Forms.Label SetupMessage;
        System.Windows.Forms.Button CloseSetupPanelButton;
        System.Windows.Forms.Button EnableMouseButton;
        System.Windows.Forms.Button EnableEyeTrackerButton;
        private System.Windows.Forms.FlowLayoutPanel ProgramControlPanel;
        private System.Windows.Forms.Button NewPaintingButton;
        private System.Windows.Forms.Button SavePaintingButton;
        private System.Windows.Forms.Button ClearPaintingButton;
        private System.Windows.Forms.Button ToolPaneToggleButton;
        private System.Windows.Forms.FlowLayoutPanel PaintToolsPanel;
        private System.Windows.Forms.FlowLayoutPanel ColorToolsPanel;
        private System.Windows.Forms.Panel Menu;
    }
}

