namespace KPEntryTemplates {
	partial class OptionsForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.lblOptionDescr = new System.Windows.Forms.Label();
			this.txtOptionVal = new System.Windows.Forms.TextBox();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lblOptionDescr
			// 
			this.lblOptionDescr.AutoSize = true;
			this.lblOptionDescr.Location = new System.Drawing.Point(13, 13);
			this.lblOptionDescr.Name = "lblOptionDescr";
			this.lblOptionDescr.Size = new System.Drawing.Size(148, 13);
			this.lblOptionDescr.TabIndex = 0;
			this.lblOptionDescr.Text = "Some Option Description here";
			// 
			// txtOptionVal
			// 
			this.txtOptionVal.Location = new System.Drawing.Point(12, 29);
			this.txtOptionVal.Name = "txtOptionVal";
			this.txtOptionVal.Size = new System.Drawing.Size(288, 20);
			this.txtOptionVal.TabIndex = 1;
			this.txtOptionVal.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtOptionVal_KeyDown);
			// 
			// btnOk
			// 
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Location = new System.Drawing.Point(225, 55);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 2;
			this.btnOk.Text = "Save";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(12, 55);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// OptionsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(316, 82);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.txtOptionVal);
			this.Controls.Add(this.lblOptionDescr);
			this.Name = "OptionsForm";
			this.Text = "Item Options";
			this.Load += new System.EventHandler(this.OptionsForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblOptionDescr;
		private System.Windows.Forms.TextBox txtOptionVal;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
	}
}