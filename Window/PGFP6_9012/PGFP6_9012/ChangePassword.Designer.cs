namespace MINATO_M1950
{
    partial class ChangePassword
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
            this.OldPass = new System.Windows.Forms.TextBox();
            this.NewPass = new System.Windows.Forms.TextBox();
            this.VerifyPass = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Ch_password = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // OldPass
            // 
            this.OldPass.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OldPass.Location = new System.Drawing.Point(159, 26);
            this.OldPass.Name = "OldPass";
            this.OldPass.PasswordChar = '*';
            this.OldPass.Size = new System.Drawing.Size(152, 26);
            this.OldPass.TabIndex = 0;
            this.OldPass.TextChanged += new System.EventHandler(this.OldPass_TextChanged);
            // 
            // NewPass
            // 
            this.NewPass.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewPass.Location = new System.Drawing.Point(159, 61);
            this.NewPass.Name = "NewPass";
            this.NewPass.Size = new System.Drawing.Size(152, 26);
            this.NewPass.TabIndex = 1;
            // 
            // VerifyPass
            // 
            this.VerifyPass.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.VerifyPass.Location = new System.Drawing.Point(159, 96);
            this.VerifyPass.Name = "VerifyPass";
            this.VerifyPass.Size = new System.Drawing.Size(152, 26);
            this.VerifyPass.TabIndex = 2;
            this.VerifyPass.TextChanged += new System.EventHandler(this.VerifyPass_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Old Password";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(38, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "New Password";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(38, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Verify Password";
            // 
            // Ch_password
            // 
            this.Ch_password.Location = new System.Drawing.Point(159, 146);
            this.Ch_password.Name = "Ch_password";
            this.Ch_password.Size = new System.Drawing.Size(152, 23);
            this.Ch_password.TabIndex = 6;
            this.Ch_password.Text = "Change Password";
            this.Ch_password.UseVisualStyleBackColor = true;
            this.Ch_password.Click += new System.EventHandler(this.Ch_password_Click);
            // 
            // ChangePassword
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(427, 204);
            this.Controls.Add(this.Ch_password);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.VerifyPass);
            this.Controls.Add(this.NewPass);
            this.Controls.Add(this.OldPass);
            this.Name = "ChangePassword";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox OldPass;
        private System.Windows.Forms.TextBox NewPass;
        private System.Windows.Forms.TextBox VerifyPass;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button Ch_password;
    }
}