namespace EyePaint
{
    partial class ConfirmBox
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
            this.ConfirmButton = new System.Windows.Forms.Button();
            this.AbortButton = new System.Windows.Forms.Button();
            this.TextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // ConfirmButton
            // 
            this.ConfirmButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ConfirmButton.Location = new System.Drawing.Point(389, 249);
            this.ConfirmButton.Name = "ConfirmButton";
            this.ConfirmButton.Size = new System.Drawing.Size(203, 106);
            this.ConfirmButton.TabIndex = 0;
            this.ConfirmButton.Text = "OK";
            this.ConfirmButton.UseVisualStyleBackColor = true;
            // 
            // AbortButton
            // 
            this.AbortButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.AbortButton.Location = new System.Drawing.Point(37, 249);
            this.AbortButton.Name = "AbortButton";
            this.AbortButton.Size = new System.Drawing.Size(203, 106);
            this.AbortButton.TabIndex = 1;
            this.AbortButton.Text = "Avbryt";
            this.AbortButton.UseVisualStyleBackColor = true;
            // 
            // TextBox
            // 
            this.TextBox.BackColor = System.Drawing.SystemColors.Control;
            this.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBox.Location = new System.Drawing.Point(37, 33);
            this.TextBox.Name = "TextBox";
            this.TextBox.Size = new System.Drawing.Size(555, 182);
            this.TextBox.TabIndex = 2;
            this.TextBox.Text = "";
            // 
            // ConfirmBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(625, 388);
            this.Controls.Add(this.TextBox);
            this.Controls.Add(this.AbortButton);
            this.Controls.Add(this.ConfirmButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ConfirmBox";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ConfirmBox";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ConfirmButton;
        private System.Windows.Forms.Button AbortButton;
        private System.Windows.Forms.RichTextBox TextBox;
    }
}