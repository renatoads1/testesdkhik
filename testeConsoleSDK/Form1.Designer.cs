
namespace testeConsoleSDK
{
    public partial class frmRenato
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
        public void InitializeComponent()
        {
            this.ptbRenato = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.ptbRenato)).BeginInit();
            this.SuspendLayout();
            // 
            // ptbRenato
            // 
            this.ptbRenato.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ptbRenato.Location = new System.Drawing.Point(12, 12);
            this.ptbRenato.Name = "ptbRenato";
            this.ptbRenato.Size = new System.Drawing.Size(428, 369);
            this.ptbRenato.TabIndex = 0;
            this.ptbRenato.TabStop = false;
            // 
            // frmRenato
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 388);
            this.Controls.Add(this.ptbRenato);
            this.Name = "frmRenato";
            this.Text = "Visualização em tempo real";
            ((System.ComponentModel.ISupportInitialize)(this.ptbRenato)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PictureBox ptbRenato;
    }
}