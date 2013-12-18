namespace EyePaint
{
    partial class EyeTrackingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EyeTrackingForm));
            this.ErrorMessagePanel = new System.Windows.Forms.Panel();
            this.EnableMouse = new System.Windows.Forms.Button();
            this.SuppressErrorMessage = new System.Windows.Forms.Button();
            this.Retry = new System.Windows.Forms.Button();
            this.Resolve = new System.Windows.Forms.Button();
            this.ErrorMessage = new System.Windows.Forms.Label();
            this.InfoMessage = new System.Windows.Forms.Label();
            this.ErrorMessagePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ErrorMessagePanel
            // 
            resources.ApplyResources(this.ErrorMessagePanel, "ErrorMessagePanel");
            this.ErrorMessagePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ErrorMessagePanel.Controls.Add(this.EnableMouse);
            this.ErrorMessagePanel.Controls.Add(this.SuppressErrorMessage);
            this.ErrorMessagePanel.Controls.Add(this.Retry);
            this.ErrorMessagePanel.Controls.Add(this.Resolve);
            this.ErrorMessagePanel.Controls.Add(this.ErrorMessage);
            this.ErrorMessagePanel.Name = "ErrorMessagePanel";
            // 
            // EnableMouse
            // 
            resources.ApplyResources(this.EnableMouse, "EnableMouse");
            this.EnableMouse.Name = "EnableMouse";
            this.EnableMouse.UseVisualStyleBackColor = true;
            this.EnableMouse.Click += new System.EventHandler(this.EnableMouseClick);
            // 
            // SuppressErrorMessage
            // 
            resources.ApplyResources(this.SuppressErrorMessage, "SuppressErrorMessage");
            this.SuppressErrorMessage.Name = "SuppressErrorMessage";
            this.SuppressErrorMessage.UseVisualStyleBackColor = true;
            this.SuppressErrorMessage.Click += new System.EventHandler(this.SuppressErrorMessageClick);
            // 
            // Retry
            // 
            resources.ApplyResources(this.Retry, "Retry");
            this.Retry.Name = "Retry";
            this.Retry.UseVisualStyleBackColor = true;
            this.Retry.Click += new System.EventHandler(this.RetryClick);
            // 
            // Resolve
            // 
            resources.ApplyResources(this.Resolve, "Resolve");
            this.Resolve.Name = "Resolve";
            this.Resolve.UseVisualStyleBackColor = true;
            this.Resolve.Click += new System.EventHandler(this.OpenControlPanelClick);
            // 
            // ErrorMessage
            // 
            resources.ApplyResources(this.ErrorMessage, "ErrorMessage");
            this.ErrorMessage.Name = "ErrorMessage";
            // 
            // InfoMessage
            // 
            resources.ApplyResources(this.InfoMessage, "InfoMessage");
            this.InfoMessage.Name = "InfoMessage";
            // 
            // EyeTrackingForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ControlBox = false;
            this.Controls.Add(this.InfoMessage);
            this.Controls.Add(this.ErrorMessagePanel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "EyeTrackingForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ErrorMessagePanel.ResumeLayout(false);
            this.ErrorMessagePanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ErrorMessagePanel;
        private System.Windows.Forms.Button Resolve;
        private System.Windows.Forms.Label ErrorMessage;
        private System.Windows.Forms.Button Retry;
        private System.Windows.Forms.Button SuppressErrorMessage;
        private System.Windows.Forms.Label InfoMessage;
        private System.Windows.Forms.Button EnableMouse;
    }
}

