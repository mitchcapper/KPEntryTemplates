using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using KeePassLib;

namespace KPEntryTemplates {
	partial class EntryTemplateManager {
		private DataGridView dataGridView;
		private DataGridViewTextBoxColumn colTitle;
		private DataGridViewComboBoxColumn colField;
		private DataGridViewTextBoxColumn colFieldName;
		private DataGridViewComboBoxColumn colType;
		private DataGridViewDisableButtonColumn colOpt;
		private DataGridViewTextBoxColumn colOptionValue;
		private Button remove_as_template_button;

		private void InitializeGridView(TabPage page) {

			if (dataGridView != null) {
				page.Controls.Add(dataGridView);
				page.Controls.Add(remove_as_template_button);
				return;
			}

			page.SuspendLayout();
			dataGridView = new DataGridView();
			colTitle = new DataGridViewTextBoxColumn();
			colField = new DataGridViewComboBoxColumn();
			colFieldName = new DataGridViewTextBoxColumn();
			colType = new DataGridViewComboBoxColumn();
			colOpt = new DataGridViewDisableButtonColumn();
			colOptionValue = new DataGridViewTextBoxColumn();
			// 
			// dataGridView
			// 
			dataGridView.AllowUserToResizeRows = false;
			dataGridView.MultiSelect = false;
			dataGridView.AllowDrop = true;
			dataGridView.AllowUserToDeleteRows = true;
			dataGridView.AutoGenerateColumns = false;

			dataGridView.EditMode = DataGridViewEditMode.EditOnEnter;
			dataGridView.DefaultValuesNeeded += dataGridView_DefaultValuesNeeded;
			dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView.Columns.AddRange(new DataGridViewColumn[] {
            colTitle,
			colField,
            colFieldName,
            colType,
			colOpt,colOptionValue});
			dataGridView.Location = new Point(0, 0);
			dataGridView.Name = "dataGridView";
			dataGridView.Size = new System.Drawing.Size(TAB_WIDTH, TAB_HEIGHT);
            dataGridView.TabIndex = 0;
			dataGridView.DragDrop += dataGridView_DragDrop;
			dataGridView.DragOver += dataGridView_DragOver;
			dataGridView.MouseDown += dataGridView_MouseDown;
			dataGridView.MouseMove += dataGridView_MouseMove;
			dataGridView.CellEnter += dataGridView_CellEnter;
			dataGridView.MouseClick += dataGridView_MouseClick;
			dataGridView.RowValidating += dataGridView_RowValidating;
			dataGridView.CellValidating += dataGridView_CellValidating;
			dataGridView.RowStateChanged += dataGridView_RowStateChanged;
			dataGridView.SelectionChanged += dataGridView_SelectionChanged;
			dataGridView.CellClick += dataGridView_CellClick;
			dataGridView.EditingControlShowing += dataGridView_EditingControlShowing;
			// 
			// colTitle
			// 
			colTitle.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			colTitle.HeaderText = "Title";
			colTitle.Name = "colTitle";
			colTitle.SortMode = DataGridViewColumnSortMode.NotSortable;
			//
			// colField
			// 
			colField.HeaderText = "Field";
			colField.Items.AddRange(new object[] {
            "Custom",
            "Title",
            "Username",
            "Password",
			"Password Confirmation",
			"URL",
			"Notes",
			"Override URL",
			"Expiry Date"});
			colField.Name = "colField";
			colField.Width = 105;
			colField.DropDownWidth = 180;


			// 
			// colFieldName
			// 
			colFieldName.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
			colFieldName.HeaderText = "Field Name";
			colFieldName.Name = "colFieldName";
			colFieldName.SortMode = DataGridViewColumnSortMode.NotSortable;
			colFieldName.Width = 75;
			// 
			// colType
			// 
			colType.HeaderText = "Type";
			colType.Name = "colType";
			colType.Width = 100;
			colType.Items.AddRange(new object[] {
            "Inline",
			"Inline URL",
            "Popout",
            "Protected Inline",
            "Protected Popout",
			"Date",
			"Time",
			"Date Time",
			"Checkbox",
			"Divider",
			"Listbox",
			"RichTextbox"
			});
			colType.DropDownWidth = 150;
            colOpt.HeaderText = "Opt";
			colOpt.Name = "colOpt";
			colOpt.Width = 40;
			colOpt.UseColumnTextForButtonValue = true;
			colOpt.Text = "Opt";
			colOpt.ToolTipText = "Option";
			colOptionValue.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
			colOptionValue.Visible = false;
			colOptionValue.Name = "colOptionValue";
			colOptionValue.Width = 0;
			
			
			page.Controls.Add(dataGridView);
			remove_as_template_button = new Button();
			remove_as_template_button.Text = "Remove As Template";
			remove_as_template_button.Width = TAB_WIDTH - 140 - 45;
			remove_as_template_button.Left = 115;
			//remove_as_template_button.Height = 28;
			remove_as_template_button.UseVisualStyleBackColor = true;
			dataGridView.Size = new Size(TAB_WIDTH, TAB_HEIGHT - remove_as_template_button.Height - 10); //have to set its size before setting the buttons size
			remove_as_template_button.Top = dataGridView.Height + 5;
			remove_as_template_button.Click += remove_as_template_button_Click;
			page.Controls.Add(remove_as_template_button);
			form.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			form.AutoScaleMode = AutoScaleMode.Font;
			page.ResumeLayout();
			
		}
		private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e){
			if (e.ColumnIndex != dataGridView.Columns["colOpt"].Index)
				return;
			DataGridViewRow row = dataGridView.Rows[e.RowIndex];
			if (!(row.Cells["colOpt"] as DataGridViewDisableButtonCell).Enabled)
				return;
			String type = row.Cells["colType"].EditedFormattedValue.ToString();
			String msg = "";
			switch (type) {
				case "Inline":
				case "RichTextbox"://CustomRichTextBoxEx
				case "Protected Inline":
					msg = "How many lines to show for the textbox(1-100)?";
					break;
				case "Listbox":
					msg = "Listbox Items, seperate with each with a comma";
					break;
			}
			String ret = OptionsForm.GetOption(msg, (string) row.Cells["colOptionValue"].Value);
			if (ret != null)
				row.Cells["colOptionValue"].Value = ret;
		}

		private bool have_deleted_as_template;
		void remove_as_template_button_Click(object sender, EventArgs e) {
			have_deleted_as_template = true;
			DialogResult res = MessageBox.Show("Are you sure you want to remove this item as a template? This will erase all of the template configuration you have done in this item.", "Confirm Erase", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			if (res != DialogResult.Yes)
				return;
			our_page.Controls.Remove(dataGridView);
			our_page.Controls.Remove(remove_as_template_button);

		}

		void dataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e) {
			DataGridViewCell cell = dataGridView.CurrentCell;
			ComboBox box = e.Control as ComboBox;
			if (cell.OwningColumn.Name == "colField") {
				box.SelectedIndexChanged += col_field_box_SelectedIndexChanged;
			} else if (cell.OwningColumn.Name == "colOpt")
				cell.ToolTipText = "Options";
			else if (cell.OwningColumn.Name == "colType") {
				box.SelectedIndexChanged += col_type_box_SelectedIndexChanged;
			}
			if (box != null){
				box.DropDown += box_DropDown;
				box.SelectedIndexChanged += box_SelectedIndexChanged;
			}
		}

		void box_SelectedIndexChanged(object sender, EventArgs e) {
			((DataGridViewComboBoxEditingControl)sender).BackColor = Color.White;

		}

		void box_DropDown(object sender, EventArgs e) {
			((DataGridViewComboBoxEditingControl)sender).BackColor = Color.White;
		}
		private void SetRowOptionEnabled(DataGridViewRow row, String type){
			bool opt_enabled = false;
			switch (type) {
				case "Inline":
				case "Protected Inline":
				case "RichTextbox":
				case "Listbox":
					opt_enabled = true;
					break;
			}

			(row.Cells["colOpt"] as DataGridViewDisableButtonCell).Enabled = opt_enabled;
		}
		private void col_type_box_SelectedIndexChanged(object sender, EventArgs e){
			DataGridViewCell cell = dataGridView.CurrentCell;
			String type = cell.EditedFormattedValue.ToString();
			SetRowOptionEnabled(cell.OwningRow, type);
		}

		void col_field_box_SelectedIndexChanged(object sender, EventArgs e) {
			DataGridViewCell cell = dataGridView.CurrentCell;
			DataGridViewRow row = cell.OwningRow;
			String field = cell.EditedFormattedValue.ToString();
			bool read_only = true;
			string type = "Inline";
			string fieldName = "";
			MemoryProtectionConfig conf = m_host.Database.MemoryProtection;
			switch (field) {
				case "Title":
					fieldName = PwDefs.TitleField;
					type = conf.ProtectTitle ? "Protected Inline" : "Inline";
					break;
				case "Username":
					fieldName = PwDefs.UserNameField;
					type = conf.ProtectUserName ? "Protected Inline" : "Inline";
					break;
				case "Password":
					fieldName = PwDefs.PasswordField;
					type = conf.ProtectPassword ? "Protected Inline" : "Inline";
					break;
				case "Password Confirmation":
					fieldName = "@confirm";
					type = conf.ProtectPassword ? "Protected Inline" : "Inline";
					break;
				case "URL":
					fieldName = PwDefs.UrlField;
					type = conf.ProtectUrl ? "Protected Inline" : "Inline URL";
					break;
				case "Override URL":
					fieldName = "@override";
					break;
				case "Expiry Date":
					fieldName = "@exp_date";
					type = "Date Time";
					break;
				case "Notes":
					fieldName = PwDefs.NotesField;
					type = conf.ProtectNotes ? "Protected Inline" : "Inline";
					break;
				default:
					type = "";
					read_only = false;
					break;
			}
			if (field != "Custom" && !field_is_unique(row, fieldName)) {
				MessageBox.Show("Another entry in the template is already using that field, cannot change");
				cell.Value = "Custom";
				dataGridView.RefreshEdit();
				return;
			}
			row.Cells["colType"].ReadOnly = row.Cells["colFieldName"].ReadOnly = read_only;
			if (type != ""){
				row.Cells["colType"].Value = type;
				SetRowOptionEnabled(row, type);
			}
			if (fieldName != "")
				row.Cells["colFieldName"].Value = fieldName;
			

		}


		void dataGridView_SelectionChanged(object sender, EventArgs e) {
			if (to_del != null) {
				if (dataGridView.Rows.Contains(to_del))
					dataGridView.Rows.Remove(to_del);
			}
			to_del = null;

		}
		private void RemoveToDel(){
			if (to_del != null) {
				if (dataGridView.Rows.Contains(to_del))
					dataGridView.Rows.Remove(to_del);
			}
			to_del = null;
		}
		private delegate void InvokeDelegate();

		void dataGridView_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e) {
			m_host.MainWindow.BeginInvoke(new InvokeDelegate(RemoveToDel));
		}


		void dataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) {
			DataGridViewRow row = dataGridView.Rows[e.RowIndex];
			row.ErrorText = "";
			String err = validate_row(row, true, row.Cells[e.ColumnIndex]);
			if (err != "") {
				DialogResult res = MessageBox.Show("Error " + err + "\r\nDo you want to retry editing or cancel changes?", "Error Editing Cell", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
				if (res != DialogResult.Retry) {

					dataGridView.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
					dataGridView.CancelEdit();
					return;
				}
			}
			row.ErrorText = err;
			if (err != "")
				e.Cancel = true;
		}
		private bool field_is_unique(DataGridViewRow row, String field_name) {
			foreach (DataGridViewRow r in dataGridView.Rows) {
				if (r.IsNewRow)
					continue;
				if (row == r)
					continue;
				String val = (string)r.Cells["colFieldName"].Value;
				if (val != null && val == field_name)
					return false;
			}
			return true;
		}
		private string validate_row(DataGridViewRow row, bool edit_vals, DataGridViewCell only_cell) {
			if (row.IsNewRow)
				return "";
			String err = "";
			DataGridViewCell field_cell = row.Cells["colFieldName"];
			DataGridViewCell title_cell = row.Cells["colTitle"];
			String field_name = (string)(edit_vals ? field_cell.EditedFormattedValue : field_cell.Value);
			String title = (string)(edit_vals ? title_cell.EditedFormattedValue : title_cell.Value);
			if (only_cell == null || field_cell == only_cell) {
				if (String.IsNullOrEmpty(field_name))
					err = "Field name cannot be empty";
				else {
					if (!field_is_unique(row, field_name))
						err = "Field name must be unique, and cannot the same as another entry in the template.";
				}
			}
			if (only_cell == null || title_cell == only_cell) {
				if (String.IsNullOrEmpty(title))
					err = "Title cannot be left empty";
			}
			return err;
		}
		DataGridViewRow to_del;
		void dataGridView_RowValidating(object sender, DataGridViewCellCancelEventArgs e) {
			DataGridViewRow row = dataGridView.Rows[e.RowIndex];
			row.ErrorText = "";
			String old_row_err = validate_row(row, false, null);
			if (String.IsNullOrEmpty(old_row_err))
				return;
			DialogResult res = MessageBox.Show("The old row values are not valid, if you continue to cancel we will delete the row (make sure all fields are filled out and not named after another)", "Error Editing Row", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
			if (res != DialogResult.Retry) {
				dataGridView.CancelEdit();
				to_del = row;
			}
			else {
				e.Cancel = true;
				row.ErrorText = old_row_err;
			}
		}




		void dataGridView_MouseClick(object sender, MouseEventArgs e) {
			DataGridView.HitTestInfo hti = dataGridView.HitTest(e.X, e.Y);
			if (hti.Type == DataGridViewHitTestType.RowHeader) {
				dataGridView.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
				dataGridView.EndEdit();
			}
		}

		void dataGridView_CellEnter(object sender, DataGridViewCellEventArgs e) {
			dataGridView.EditMode = DataGridViewEditMode.EditOnEnter;
		}
		private void RemoveDataGridView(TabPage page) {
			page.Controls.Remove(dataGridView);
		}
		void dataGridView_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e) {
			e.Row.Cells["colType"].Value = "Inline";
			e.Row.Cells["colField"].Value = "Custom";
		}
		private Rectangle dragBoxFromMouseDown;
		private int rowIndexFromMouseDown;
		private int rowIndexOfItemUnderMouseToDrop;
		private void init_data_table() {
			have_deleted_as_template = false;
			try {
				dataGridView.Rows.Clear();
				List<EntryTemplate> to_add = parse_entry(form.EntryStrings);
				foreach (EntryTemplate t in to_add) {
					int idx = dataGridView.Rows.Add();
					DataGridViewRow row = dataGridView.Rows[idx];
					row.Cells["colTitle"].Value = t.title;
					row.Cells["colFieldName"].Value = t.fieldName;
					row.Cells["colType"].Value = t.type;
					SetRowOptionEnabled(row, t.type);
					String field = "Custom";
					bool read_only = true;
					switch (t.fieldName) {
						case PwDefs.TitleField:
							field = "Title";
							break;
						case PwDefs.UserNameField:
							field = "Username";
							break;
						case PwDefs.PasswordField:
							field = "Password";
							break;
						case PwDefs.UrlField:
							field = "URL";
							break;
						case PwDefs.NotesField:
							field = "Notes";
							break;
						case "@confirm":
							field = "Password Confirmation";
							break;
						case "@override":
							field = "Override URL";
							break;
						case "@exp_date":
							field = "Expiry Date";
							break;
						default:
							read_only = false;
							break;
					}
					row.Cells["colField"].Value = field;
					row.Cells["colType"].ReadOnly = row.Cells["colFieldName"].ReadOnly = read_only;
					row.Cells["colOptionValue"].Value = t.options;
				}

			}
			catch (Exception e) {
				MessageBox.Show(e.Message);
			}
		}
		private List<EntryTemplate> export_table() {
			List<EntryTemplate> ret = new List<EntryTemplate>();
			if (have_deleted_as_template)
				return ret;
			int pos = 0;
			foreach (DataGridViewRow row in dataGridView.Rows) {
				if (pos == dataGridView.Rows.Count - 1)//don't add last row
					continue;
				ret.Add(new EntryTemplate((string)row.Cells["colTitle"].Value, (string)row.Cells["colFieldName"].Value, (string)row.Cells["colType"].Value, pos++, (string)row.Cells["colOptionValue"].Value));
			}
			return ret;
		}
		private void dataGridView_MouseMove(object sender, MouseEventArgs e) {
			if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
				if (dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(e.X, e.Y)) {
					if (dataGridView.IsCurrentCellDirty)
						dataGridView.EndEdit();
					dataGridView.DoDragDrop(dataGridView.Rows[rowIndexFromMouseDown], DragDropEffects.Move);
				}
			}
		}

		private void dataGridView_MouseDown(object sender, MouseEventArgs e) {
			rowIndexFromMouseDown = dataGridView.HitTest(e.X, e.Y).RowIndex;
			if (rowIndexFromMouseDown != -1 && rowIndexFromMouseDown < dataGridView.RowCount - 1) {
				Size dragSize = SystemInformation.DragSize;
				dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)),
				dragSize);
			}
			else
				dragBoxFromMouseDown = Rectangle.Empty;
		}

		private void dataGridView_DragOver(object sender, DragEventArgs e) {
			e.Effect = DragDropEffects.Move;
		}

		private void dataGridView_DragDrop(object sender, DragEventArgs e) {
			Point clientPoint = dataGridView.PointToClient(new Point(e.X, e.Y));
			rowIndexOfItemUnderMouseToDrop = dataGridView.HitTest(clientPoint.X, clientPoint.Y).RowIndex;
			if (e.Effect == DragDropEffects.Move) {
				DataGridViewRow rowToMove = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
				dataGridView.Rows.RemoveAt(rowIndexFromMouseDown);
				if (rowIndexOfItemUnderMouseToDrop == dataGridView.Rows.Count)//can't insert after last
					rowIndexOfItemUnderMouseToDrop--;
				dataGridView.Rows.Insert(rowIndexOfItemUnderMouseToDrop, rowToMove);
			}
		}
	}
}
