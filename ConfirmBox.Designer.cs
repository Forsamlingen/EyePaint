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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfirmBox));
            this.ConfirmButton = new System.Windows.Forms.Button();
            this.AbortButton = new System.Windows.Forms.Button();
            this.TextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // ConfirmButton
            // 
            this.ConfirmButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ConfirmButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.ConfirmButton.FlatAppearance.BorderSize = 10;
            this.ConfirmButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ConfirmButton.Image = ((System.Drawing.Image)(resources.GetObject("ConfirmButton.Image")));
            this.ConfirmButton.Location = new System.Drawing.Point(345, 203);
            this.ConfirmButton.Name = "ConfirmButton";
            this.ConfirmButton.Size = new System.Drawing.Size(247, 152);
            this.ConfirmButton.TabIndex = 0;
            this.ConfirmButton.TabStop = false;
            this.ConfirmButton.UseVisualStyleBackColor = true;
            this.ConfirmButton.Click += new System.EventHandler(this.onButtonClicked);
            this.ConfirmButton.Enter += new System.EventHandler(this.onButtonFocus);
            this.ConfirmButton.Leave += new System.EventHandler(this.onButtonBlur);
            // 
            // AbortButton
            // 
            this.AbortButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.AbortButton.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.AbortButton.FlatAppearance.BorderSize = 10;
            this.AbortButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AbortButton.Image = ((System.Drawing.Image)(resources.GetObject("AbortButton.Image")));
            this.AbortButton.Location = new System.Drawing.Point(37, 203);
            this.AbortButton.Name = "AbortButton";
            this.AbortButton.Size = new System.Drawing.Size(247, 152);
            this.AbortButton.TabIndex = 1;
            this.AbortButton.TabStop = false;
            this.AbortButton.UseVisualStyleBackColor = true;
            this.AbortButton.Click += new System.EventHandler(this.onButtonClicked);
            this.AbortButton.Enter += new System.EventHandler(this.onButtonFocus);
            this.AbortButton.Leave += new System.EventHandler(this.onButtonBlur);
            // 
            // TextBox
            // 
            this.TextBox.BackColor = System.Drawing.SystemColors.Control;
            this.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBox.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBox.Location = new System.Drawing.Point(37, 33);
            this.TextBox.Name = "TextBox";
            this.TextBox.Size = new System.Drawing.Size(555, 164);
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