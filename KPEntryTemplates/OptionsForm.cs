using System;
using System.Windows.Forms;

namespace KPEntryTemplates {
	public partial class OptionsForm : Form {
		public OptionsForm() {
			InitializeComponent();
		}
		public static String GetOption(String msg, String val){
			OptionsForm form = new OptionsForm();
			form.txtOptionVal.Text = val;
			form.lblOptionDescr.Text = msg;
			DialogResult res = form.ShowDialog();
			if (res != DialogResult.OK)
				return null;
			return form.txtOptionVal.Text;
		}
		private void btnOk_Click(object sender, EventArgs e){
			DialogResult = DialogResult.OK;
			
		}

		private void btnCancel_Click(object sender, EventArgs e){
			
		}

		private void txtOptionVal_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Return) {
				e.Handled = true;
				btnOk_Click(null, null);
			}
		}

		private void OptionsForm_Load(object sender, EventArgs e){
			txtOptionVal.Focus();
		}
	}
}
