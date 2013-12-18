namespace EyePaint
{
    partial class WinFormsSample
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
            this.ErrorMessagePanel = new System.Windows.Forms.Panel();
            this.SuppressErrorMessage = new System.Windows.Forms.Button();
            this.Retry = new System.Windows.Forms.Button();
            this.Resolve = new System.Windows.Forms.Button();
            this.ErrorMessage = new System.Windows.Forms.Label();
            this.InfoMessage = new System.Windows.Forms.Label();
            this.EnableMouse = new System.Windows.Forms.Button();
            this.ErrorMessagePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ErrorMessagePanel
            // 
            this.ErrorMessagePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ErrorMessagePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ErrorMessagePanel.Controls.Add(this.EnableMouse);
            this.ErrorMessagePanel.Controls.Add(this.SuppressErrorMessage);
            this.ErrorMessagePanel.Controls.Add(this.Retry);
            this.ErrorMessagePanel.Controls.Add(this.Resolve);
            this.ErrorMessagePanel.Controls.Add(this.ErrorMessage);
            this.ErrorMessagePanel.Location = new System.Drawing.Point(0, 0);
            this.ErrorMessagePanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ErrorMessagePanel.Name = "ErrorMessagePanel";
            this.ErrorMessagePanel.Size = new System.Drawing.Size(451, 74);
            this.ErrorMessagePanel.TabIndex = 0;
            // 
            // SuppressErrorMessage
            // 
            this.SuppressErrorMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SuppressErrorMessage.Location = new System.Drawing.Point(417, 10);
            this.SuppressErrorMessage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.SuppressErrorMessage.Name = "SuppressErrorMessage";
            this.SuppressErrorMessage.Size = new System.Drawing.Size(23, 21);
            this.SuppressErrorMessage.TabIndex = 3;
            this.SuppressErrorMessage.Text = "X";
            this.SuppressErrorMessage.UseVisualStyleBackColor = true;
            this.SuppressErrorMessage.Click += new System.EventHandler(this.SuppressErrorMessageClick);
            // 
            // Retry
            // 
            this.Retry.Location = new System.Drawing.Point(153, 33);
            this.Retry.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Retry.Name = "Retry";
            this.Retry.Size = new System.Drawing.Size(60, 26);
            this.Retry.TabIndex = 2;
            this.Retry.Text = "Retry";
            this.Retry.UseVisualStyleBackColor = true;
            this.Retry.Click += new System.EventHandler(this.RetryClick);
            // 
            // Resolve
            // 
            this.Resolve.Location = new System.Drawing.Point(6, 33);
            this.Resolve.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Resolve.Name = "Resolve";
            this.Resolve.Size = new System.Drawing.Size(117, 27);
            this.Resolve.TabIndex = 1;
            this.Resolve.Text = "Open Control Panel";
            this.Resolve.UseVisualStyleBackColor = true;
            this.Resolve.Click += new System.EventHandler(this.OpenControlPanelClick);
            // 
            // ErrorMessage
            // 
            this.ErrorMessage.AutoSize = true;
            this.ErrorMessage.Location = new System.Drawing.Point(4, 6);
            this.ErrorMessage.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ErrorMessage.Name = "ErrorMessage";
            this.ErrorMessage.Size = new System.Drawing.Size(228, 13);
            this.ErrorMessage.TabIndex = 0;
            this.ErrorMessage.Text = "Tobii Eye Tracking has not been set up for use";
            // 
            // InfoMessage
            // 
            this.InfoMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.InfoMessage.Location = new System.Drawing.Point(0, 125);
            this.InfoMessage.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.InfoMessage.Name = "InfoMessage";
            this.InfoMessage.Size = new System.Drawing.Size(451, 44);
            this.InfoMessage.TabIndex = 1;
            this.InfoMessage.Text = "Connecting...";
            this.InfoMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // EnableMouse
            // 
            this.EnableMouse.Location = new System.Drawing.Point(240, 33);
            this.EnableMouse.Margin = new System.Windows.Forms.Padding(2);
            this.EnableMouse.Name = "EnableMouse";
            this.EnableMouse.Size = new System.Drawing.Size(126, 26);
            this.EnableMouse.TabIndex = 4;
            this.EnableMouse.Text = "Use mouse instead";
            this.EnableMouse.UseVisualStyleBackColor = true;
            this.EnableMouse.Click += new System.EventHandler(this.EnableMouseClick);
            // 
            // WinFormsSample
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 277);
            this.Controls.Add(this.InfoMessage);
            this.Controls.Add(this.ErrorMessagePanel);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "WinFormsSample";
            this.Text = "Tobii Eye Tracking WinForms Sample";
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

