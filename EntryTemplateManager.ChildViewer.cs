using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using KeePass;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;
using KeePassLib;
using KeePassLib.Cryptography.PasswordGenerator;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KPEntryTemplates {
	partial class EntryTemplateManager {
		Dictionary<EntryTemplate, Label> et_to_label;
		Dictionary<EntryTemplate, Control> et_to_control;
		Dictionary<EntryTemplate, Control> et_to_control2;
		Dictionary<EntryTemplate, SecureTextBoxEx> et_to_secure_edit;

		private Button client_remove_button;
		private Button m_btnGenPw;
		private DynamicMenu m_dynGenProfiles;
		private ContextMenuStrip m_ctxPwGen;
		private ToolStripMenuItem m_ctxPwGenOpen;
		private ToolStripSeparator m_ctxPwGenSep0;
		private ToolStripMenuItem m_ctxPwGenProfiles;
		private void add_child_items_to_tab(TabPage page) {
			foreach (KeyValuePair<EntryTemplate, Control> pair in et_to_control) {
				page.Controls.Add(et_to_label[pair.Key]);
				if (pair.Value != null)
					page.Controls.Add(pair.Value);
				Control cntrl2;
				if (et_to_control2.TryGetValue(pair.Key, out cntrl2))
					page.Controls.Add(cntrl2);

			}
			page.Controls.Add(client_remove_button);

		}
		private SecureTextBoxEx current_password_field;
		private SecureTextBoxEx current_password_confirm_field;
		private TextBox current_password_confirm_field_txt;
		private readonly string DeriveFromPrevious = "(" + KPRes.GenPwBasedOnPrevious + ")";
		private void OnPwGenClick(object sender, EventArgs e) {
			m_dynGenProfiles.Clear();
			m_dynGenProfiles.AddItem(DeriveFromPrevious, Resources.Resources.B16x16_CompFile);

			if (Program.Config.PasswordGenerator.UserProfiles.Count > 0)
				m_dynGenProfiles.AddSeparator();
			foreach (PwProfile pwgo in Program.Config.PasswordGenerator.UserProfiles) {
				if (pwgo.Name != DeriveFromPrevious)
					m_dynGenProfiles.AddItem(pwgo.Name,
						Resources.Resources.B16x16_KOrganizer);
			}

			m_ctxPwGen.Show(m_btnGenPw, new Point(0, m_btnGenPw.Height));
		}
		private void OnPwGenOpen(object sender, EventArgs e) {
			PwGeneratorForm pgf = new PwGeneratorForm();
			ProtectedString ps = current_password_field.TextEx;
			bool bAtLeastOneChar = (ps.Length > 0);
			PwProfile opt = PwProfile.DeriveFromPassword(ps);

			pgf.InitEx(bAtLeastOneChar ? opt : null, true, false);
			if (pgf.ShowDialog() == DialogResult.OK) {
				byte[] pbEntropy = EntropyForm.CollectEntropyIfEnabled(pgf.SelectedProfile);
				ProtectedString psNew;
				PwGenerator.Generate(out psNew, pgf.SelectedProfile, pbEntropy,
					Program.PwGeneratorPool);

				current_password_confirm_field.TextEx = current_password_field.TextEx = psNew;
			}

		}
		private void OnProfilesDynamicMenuClick(object sender, DynamicMenuEventArgs e) {
			PwProfile pwp = null;
			if (e.ItemName == DeriveFromPrevious) {
				pwp = PwProfile.DeriveFromPassword(current_password_field.TextEx);
			} else {
				foreach (PwProfile pwgo in Program.Config.PasswordGenerator.UserProfiles) {
					if (pwgo.Name == e.ItemName) {
						pwp = pwgo;
						break;
					}
				}
			}

			if (pwp != null) {
				ProtectedString psNew;

				PwGenerator.Generate(out psNew, pwp, null, m_host.PwGeneratorPool);
				current_password_confirm_field.TextEx = current_password_field.TextEx = psNew;
			} else { Debug.Assert(false); }
		}
		private void init_pwgen_button() {
			m_ctxPwGenOpen = new ToolStripMenuItem();
			m_ctxPwGenSep0 = new ToolStripSeparator();
			m_ctxPwGenProfiles = new ToolStripMenuItem();
			m_ctxPwGenProfiles.Name = "m_ctxPwGenProfiles";
			m_ctxPwGenProfiles.Size = new Size(208, 22);
			m_ctxPwGenProfiles.Text = "Generate Using Profile";
			m_btnGenPw = new Button();
			m_btnGenPw.Image = DpiUtil.ScaleImage(Resources.Resources.B15x13_KGPG_Gen, false);
			m_btnGenPw.Location = new Point(423, 90);

			m_btnGenPw.Size = new Size(DpiUtil.ScaleIntX(32), DpiUtil.ScaleIntY(23));
			m_btnGenPw.UseVisualStyleBackColor = true;
			m_btnGenPw.Click += OnPwGenClick;
			m_ctxPwGen = new ContextMenuStrip();
			m_dynGenProfiles = new DynamicMenu(m_ctxPwGenProfiles);
			m_dynGenProfiles.MenuClick += OnProfilesDynamicMenuClick;

			m_ctxPwGen.Items.AddRange(new ToolStripItem[] {
			m_ctxPwGenOpen,
			m_ctxPwGenSep0,
			m_ctxPwGenProfiles});
			m_ctxPwGen.Name = "m_ctxPwGen";
			m_ctxPwGen.Size = new Size(209, 54);
			// 
			// m_ctxPwGenOpen
			// 
			m_ctxPwGenOpen.Image = Resources.Resources.B16x16_KGPG_Gen;
			m_ctxPwGenOpen.Name = "m_ctxPwGenOpen";
			m_ctxPwGenOpen.Size = new Size(208, 22);
			m_ctxPwGenOpen.Text = "&Open Password Generator...";
			m_ctxPwGenOpen.Click += OnPwGenOpen;
			// 
			// m_ctxPwGenSep0
			// 
			m_ctxPwGenSep0.Name = "m_ctxPwGenSep0";
			m_ctxPwGenSep0.Size = new Size(205, 6);
			// 
			// m_ctxPwGenProfiles
			// 
			m_ctxPwGenProfiles.Name = "m_ctxPwGenProfiles";
			m_ctxPwGenProfiles.Size = new Size(208, 22);
			m_ctxPwGenProfiles.Text = "Generate Using Profile";
		}
		public static Dictionary<String, String> get_template_title_to_field_dict(IPluginHost m_host, String template_uuid) {
			Dictionary<String, String> ret = new Dictionary<string, string>();
			PwUuid par_uuid = new PwUuid(KeePassLib.Utility.MemUtil.HexStringToByteArray(template_uuid));
			PwEntry par_template = m_host.Database.RootGroup.FindEntry(par_uuid, true);
			if (par_template == null)
				return ret;
			if (par_template.Strings.Get("_etm_template") == null)
				return ret;
			List<EntryTemplate> cur = parse_entry(par_template.Strings);
			foreach (EntryTemplate tmp in cur) {
				if (tmp.type == "Divider")
					continue;
				if (tmp.fieldName.Length < 1 || tmp.fieldName[0] == '@')
					continue;
				ret[tmp.title] = tmp.fieldName;
			}
			return ret;
		}
		public static void UpdateControlSize(Control control) {
			var template = control.Tag as EntryTemplate;
			if (template == null) {
				return;
			}
			SetControlSizing(template, control);
		}
		private static void SetControlSizing(EntryTemplate template, Control control) {
			int? width = CONTROL_WIDTH;
			int? left = LEFT_CONTROL_OFFSET;
			int? top=null;
			int? height=null;
			if (template == null) {
				if (control is Label) {
					width = LABEL_WIDTH;
					left = 0;
				}
			} else {
				switch (template.type) {
					case "DataGridView":
						left = null;
						
						width = PAGE_WIDTH;
						height = PAGE_HEIGHT - BUTTON_HEIGHT - DpiUtil.ScaleIntY(10);
						if (control is Button) {
							top = height + DpiUtil.ScaleIntY(5);
							height = BUTTON_HEIGHT;
							width = PAGE_WIDTH - DpiUtil.ScaleIntX(140) - DpiUtil.ScaleIntX(45);
						}
						break;
					case "Divider":
						width = CONTROL_WIDTH + LABEL_WIDTH;
						break;
					case "Checkbox":
						width = null;
						break;
					case "Inline URL":
						width = CONTROL_WIDTH - 30;
						if (control is LinkLabel) {
							left = left + width + DpiUtil.ScaleIntX(10);
							width = null;
						}
						break;
					case "Protected Inline":
						if (control is CheckBox || control is Button) {
							left = left + width + DpiUtil.ScaleIntX(10);
							width = null;
						}
						break;
				}
			}
			if (left != null)
				control.Left = (int)left;
			if (width != null)
				control.Width = (int)width;
			if (top != null)
				control.Top = (int)top;
			if (height != null)
				control.Height = (int)height;
		}
		public static void SetBaseSizes(TabPage page) {
			LABEL_WIDTH = DpiUtil.ScaleIntX(130);
			LEFT_CONTROL_OFFSET = LABEL_WIDTH + DpiUtil.ScaleIntX(5);
			CONTROL_WIDTH = page.ClientSize.Width - LABEL_WIDTH - DpiUtil.ScaleIntX(70);//minus some so scrolling doesnt cause an issue
			PAGE_WIDTH = page.ClientSize.Width;
			PAGE_HEIGHT = page.ClientSize.Height;
			BUTTON_HEIGHT = DpiUtil.ScaleIntY(20);
		}
		private static int BUTTON_HEIGHT;
		private static int CONTROL_WIDTH;
		private static int LEFT_CONTROL_OFFSET;
		private static int LABEL_WIDTH;
		private static int PAGE_WIDTH;
		private static int PAGE_HEIGHT;
		private bool InitializeChildView(TabPage page, String uuid) {
			if (et_to_label != null) {
				add_child_items_to_tab(page);
				return true;
			}
			SetBaseSizes(page);
			init_pwgen_button();

			et_to_label = new Dictionary<EntryTemplate, Label>();
			et_to_control = new Dictionary<EntryTemplate, Control>();
			et_to_secure_edit = new Dictionary<EntryTemplate, SecureTextBoxEx>();
			et_to_control2 = new Dictionary<EntryTemplate, Control>();
			SecureTextBoxEx entry_pass = null;
			SecureTextBoxEx entry_pass_confirm = null;

			int control_offset_y = DpiUtil.ScaleIntY(10);
			PwUuid par_uuid = new PwUuid(KeePassLib.Utility.MemUtil.HexStringToByteArray(uuid));
			PwEntry par_template = m_host.Database.RootGroup.FindEntry(par_uuid, true);
			if (par_template == null)
				return false;
			if (par_template.Strings.Get("_etm_template") == null)
				return false;
			List<EntryTemplate> cur = parse_entry(par_template.Strings);


			foreach (EntryTemplate t in cur) {
				Label label = new Label();
				label.Text = t.title + ":";
				//label.AutoSize = false;
				label.Top = control_offset_y;
				label.AutoSize = false;
				SetControlSizing(null, label);
				label.AutoEllipsis = true;
				label.TextAlign = ContentAlignment.MiddleRight;
				FontUtil.AssignDefaultBold(label);

				et_to_label[t] = label;
				if (t.type == "Divider") {
					label.Font = new Font(label.Font.FontFamily, label.Font.Size * 1.1f, FontStyle.Bold | FontStyle.Underline);
					label.TextAlign = ContentAlignment.BottomLeft;
					label.Text = t.title;//remove :
					et_to_control[t] = null;
				} else if (t.type == "Checkbox") {
					CheckBox checkbox = new CheckBox();
					checkbox.Top = control_offset_y;
					et_to_control[t] = checkbox;
				} else if (t.type == "Listbox") {
					ComboBox combobox = new ComboBox();
					combobox.Top = control_offset_y;
					et_to_control[t] = combobox;
					if (!String.IsNullOrEmpty(t.options)) {
						String[] opts = t.options.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (String opt in opts)
							combobox.Items.Add(opt.Trim());
					}
				} else if (t.type == "Date" || t.type == "Time" || t.type == "Date Time") {
					DateTimePicker picker = new DateTimePicker();
					picker.Top = control_offset_y;
					picker.CustomFormat = "";
					picker.Format = DateTimePickerFormat.Custom;
					if (t.type == "Date" || t.type == "Date Time")
						picker.CustomFormat = System.Globalization.DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
					if (t.type == "Date Time")
						picker.CustomFormat += " ";
					if (t.type == "Time" || t.type == "Date Time")
						picker.CustomFormat += System.Globalization.DateTimeFormatInfo.CurrentInfo.LongTimePattern;
					if (t.fieldName == "@exp_date")
						picker.ShowCheckBox = true;
					et_to_control[t] = picker;
				} else if (t.type == "RichTextbox") {
					var box = new CustomRichTextBoxEx();
					box.Top = control_offset_y;
					int lines = LinesFromOption(t.options);
					box.Multiline = lines > 1;
					box.Height = DpiUtil.ScaleIntY(13) * lines + DpiUtil.ScaleIntY(10);
					box.CtrlEnterAccepts = true;
					box.ScrollBars = RichTextBoxScrollBars.Both;
					control_offset_y += DpiUtil.ScaleIntY(13) * (lines - 1);
					UIUtil.PrepareStandardMultilineControl(box, true, lines > 1);

					et_to_control[t] = box;
				} else if (t.type == "Inline" || t.type == "Protected Inline" || t.type == "Inline URL") {
					var box = new TextBox();
					int lines = LinesFromOption(t.options);
					if (t.type == "Inline URL")
						lines = 1;
					if (t.type == "Protected Inline")
						box = new SecureTextBoxEx();
					box.Top = control_offset_y;

					if (lines > 1) {
						box.Multiline = true;
						box.AcceptsReturn = true;
						box.Height = DpiUtil.ScaleIntY(13) * lines + DpiUtil.ScaleIntY(10);
						box.ScrollBars = ScrollBars.Both;
						control_offset_y += DpiUtil.ScaleIntY(13) * (lines - 1);
					}
					et_to_control[t] = box;
					if (t.type == "Protected Inline") {
						et_to_secure_edit[t] = box as SecureTextBoxEx;
						if (t.fieldName != "@confirm") {
							CheckBox chk = new CheckBox();
							chk.Appearance = Appearance.Button;
							chk.Image = DpiUtil.ScaleImage(Resources.Resources.B17x05_3BlackDots, false);
							chk.Size = new Size(DpiUtil.ScaleIntX(32), DpiUtil.ScaleIntY(23));
							chk.TextAlign = ContentAlignment.MiddleCenter;
							chk.UseVisualStyleBackColor = true;
							chk.Top = control_offset_y;
							chk.Checked = true;
							chk.CheckedChanged += chk_CheckedChanged;
							et_to_control2[t] = chk;
						} else {
							et_to_control2[t] = m_btnGenPw;
							et_to_control2[t].Top = control_offset_y;
							current_password_confirm_field = box as SecureTextBoxEx;
							current_password_confirm_field_txt = box;
							entry_pass_confirm = box as SecureTextBoxEx;
						}
						if (t.fieldName == PwDefs.PasswordField) {
							entry_pass = current_password_field = box as SecureTextBoxEx;
						}
					} else if (t.type == "Inline URL") {
						var link = new LinkLabel { Text = "Open" };
						link.LinkClicked += (sender, args) => WinUtil.OpenUrl(box.Text ?? "", form.EntryRef);
						link.Top = control_offset_y;
						link.Width = DpiUtil.ScaleIntY(50);
						et_to_control2[t] = link;
					}

				} else if (t.type == "Popout" || t.type == "Protected Popout") {
					Button btn = new Button();
					btn.Text = "View/Edit";
					if (t.type == "Protected Popout")
						btn.Text = "View/Edit Secure";
					btn.Height = BUTTON_HEIGHT;
					btn.Top = control_offset_y;

					btn.Click += btn_popout_Click;
					et_to_control[t] = btn;
				}
				control_offset_y += DpiUtil.ScaleIntY(30);
				if (et_to_control[t] != null) { //only the divider does not
					et_to_control[t].Tag = t;
					SetControlSizing(t, et_to_control[t]);
				}
				if (et_to_control2.ContainsKey(t)) {
					et_to_control2[t].Tag = t;
					SetControlSizing(t, et_to_control2[t]);
				}
			}
			client_remove_button = new Button();
			client_remove_button.Text = "Remove As Template Child";
			client_remove_button.Height = BUTTON_HEIGHT;
			SetControlSizing(null, client_remove_button);
			//client_remove_button.Height = 20;
			client_remove_button.Top = control_offset_y;
			client_remove_button.Click += client_remove_button_Click;

			add_child_items_to_tab(page);
			return true;
		}

		void client_remove_button_Click(object sender, EventArgs e) {
			ProtectedString str = form.EntryStrings.Get("_etm_template_uuid");
			if (str != null)
				form.EntryStrings.Remove("_etm_template_uuid");
			MessageBox.Show("Please close the entry to remove the template view, please note this did not delete any of the actual data");
		}

		void btn_popout_Click(object sender, EventArgs e) {
			Button btn = (Button)sender;
			EntryTemplate t = (EntryTemplate)btn.Tag;
			if (form.EntryStrings.Get(t.fieldName) == null)
				form.EntryStrings.Set(t.fieldName, new ProtectedString(t.type.StartsWith("Protected"), ""));

			ProtectedString psValue = form.EntryStrings.Get(t.fieldName);
			Debug.Assert(psValue != null);
			EditStringForm esf = new EditStringForm();
			esf.InitEx(form.EntryStrings, t.fieldName, psValue, m_host.Database);
			if (esf.ShowDialog() == DialogResult.OK)
				form.UpdateEntryStrings(false, true);

		}

		void chk_CheckedChanged(object sender, EventArgs e) {
			CheckBox chk = (CheckBox)sender;
			EntryTemplate t = (EntryTemplate)chk.Tag;
			var sedit = et_to_secure_edit[t];
			sedit.EnableProtection(chk.Checked);
			if (sedit == current_password_field && current_password_confirm_field != null) {
				if (chk.Checked)
					current_password_confirm_field.TextEx = sedit.TextEx;
				current_password_confirm_field.Enabled = chk.Checked;
				current_password_confirm_field.EnableProtection(chk.Checked);

			}
		}
		private DateTimePicker expires_control;
		private CheckBox expires_cbx_control;

		private void find_expires_control(Form form) {
			if (expires_control != null)
				return;
			expires_control = get_control_from_form(form, "m_dtExpireDateTime") as DateTimePicker;
			expires_cbx_control = get_control_from_form(form, "m_cbExpires") as CheckBox;

		}
		private TextBox override_url_control;
		private ImageComboBoxEx new_override_url_control;
		private void find_override_url_control(Form form) {
			if (new_override_url_control != null || override_url_control != null)
				return;

			new_override_url_control = get_control_from_form(form, "m_cmbOverrideUrl") as ImageComboBoxEx;
			if (new_override_url_control == null)//older keepass
				override_url_control = get_control_from_form(form, "m_tbOverrideUrl") as TextBox;
		}
		private int LinesFromOption(String val) {
			if (String.IsNullOrEmpty(val))
				return 1;
			int ret = 1;
			Int32.TryParse(val, out ret);
			if (ret < 1 || ret > 100)
				ret = 1;
			return ret;
		}
		private void save_child_vals() {
			foreach (KeyValuePair<EntryTemplate, Control> pair in et_to_control) {
				if (pair.Value == null)
					continue;
				EntryTemplate t = pair.Key;
				ProtectedString str;
				if (t.type == "Date" || t.type == "Time" || t.type == "Date Time") {
					DateTimePicker picker = (DateTimePicker)pair.Value;
					if (t.fieldName == "@exp_date") {
						find_expires_control(form);
						Debug.Assert(expires_control != null && expires_cbx_control != null);
						expires_cbx_control.Checked = picker.Checked;
						expires_control.Value = picker.Value;
						continue;
					}
					str = new ProtectedString(false, picker.Value.ToString());
				} else if (t.type == "Checkbox") {
					CheckBox checkbox = (CheckBox)pair.Value;
					str = new ProtectedString(false, checkbox.Checked.ToString());
				} else if (t.type == "Inline" || t.type == "Inline URL") {
					TextBox box = (TextBox)pair.Value;
					str = new ProtectedString(false, box.Text == null ? "" : box.Text.Replace("\r", ""));
				} else if (t.type == "RichTextbox") {
					var box = (CustomRichTextBoxEx)pair.Value;
					str = new ProtectedString(false, box.Text == null ? "" : box.Text.Replace("\r", ""));
				} else if (t.type == "Listbox") {
					ComboBox combobox = (ComboBox)pair.Value;
					str = new ProtectedString(false, combobox.SelectedItem == null ? "" : combobox.SelectedItem.ToString());
				} else if (t.type == "Protected Inline") {
					var sedit = et_to_secure_edit[t];
					str = sedit.TextEx;
				} else
					continue;
				str = str.WithProtection(t.type.StartsWith("Protected"));
				if (t.fieldName == "@confirm") {
					//form.m_secRepeat.SetProtectedString(str, form.EntryRef, "Viewing/Editing Entry");
					continue;
				}
				if (t.fieldName == "@override") {
					find_override_url_control(form);
					Debug.Assert(new_override_url_control != null || override_url_control != null);
					if (new_override_url_control != null)
						new_override_url_control.Text = str.ReadString();
					else
						override_url_control.Text = str.ReadString();
					continue;
				}
				form.EntryStrings.Set(t.fieldName, str);
				//if (t.fieldName == PwDefs.PasswordField && current_password_confirm_field == null)
				//form.m_secRepeat.SetProtectedString(str, form.EntryRef, "Viewing/Editing Entry");
			}
		}
		private ToolTip m_ttValidationError;
		private void init_validation_err() {
			if (m_ttValidationError != null)
				return;
			m_ttValidationError = new ToolTip();
			m_ttValidationError.AutoPopDelay = 32000;
			m_ttValidationError.InitialDelay = 250;
			m_ttValidationError.ReshowDelay = 100;
			m_ttValidationError.ToolTipIcon = ToolTipIcon.Warning;
			m_ttValidationError.ToolTipTitle = "Validation Warning";
		}
		private bool check_confirm_password_ok() {
			if (current_password_field == null || current_password_confirm_field == null)
				return true;
			if (!current_password_confirm_field.Enabled)
				current_password_confirm_field.TextEx = current_password_field.TextEx;
			if (current_password_field.TextEx.Equals(current_password_confirm_field.TextEx, false) == false) {
				init_validation_err();
				current_password_confirm_field_txt.BackColor = KeePass.App.AppDefs.ColorEditError;
				m_ttValidationError.Show(KPRes.PasswordRepeatFailed, current_password_confirm_field_txt);
				return false;
			} else if (m_ttValidationError != null)
				m_ttValidationError.Hide(current_password_confirm_field_txt);
			current_password_confirm_field_txt.BackColor = Color.White;

			return true;
		}
		private void init_child_vals() {
			foreach (KeyValuePair<EntryTemplate, Control> pair in et_to_control) {
				if (pair.Value == null)
					continue;
				EntryTemplate t = pair.Key;
				String get_name = t.fieldName;
				if (t.fieldName == "@confirm")
					get_name = PwDefs.PasswordField;
				ProtectedString str = null;
				if (t.fieldName == "@override") {
					get_name = "";
					find_override_url_control(form);
					Debug.Assert(override_url_control != null || new_override_url_control != null);
					if (new_override_url_control != null)
						str = new ProtectedString(false, new_override_url_control.Text);
					else
						str = new ProtectedString(false, override_url_control.Text);

				}
				if (get_name != "")
					str = form.EntryStrings.Get(get_name);
				if (str == null)
					str = new ProtectedString(t.type.StartsWith("Protected"), "");
				if (t.type == "Inline" || t.type == "Inline URL") {
					TextBox box = (TextBox)pair.Value;
					String val = str.ReadString();
					val = val.Replace("\r", "");
					val = val.Replace("\n", "\r\n");
					box.Text = val;

				} else if (t.type == "Protected Inline") {
					var sedit = et_to_secure_edit[t];
					sedit.TextEx = str;
				} else if (t.type == "RichTextbox") {
					var box = (CustomRichTextBoxEx)pair.Value;
					String val = str.ReadString();
					val = val.Replace("\r", "");
					val = val.Replace("\n", "\r\n");
					box.Text = val;
				} else if (t.type == "Listbox") {
					ComboBox combobox = (ComboBox)pair.Value;
					combobox.SelectedItem = str.ReadString();
				} else if (t.type == "Checkbox") {
					bool val;
					CheckBox box = (CheckBox)pair.Value;
					box.Checked = false;
					if (Boolean.TryParse(str.ReadString(), out val))
						box.Checked = val;
				} else if (t.type == "Date" || t.type == "Time" || t.type == "Date Time") {
					DateTimePicker picker = (DateTimePicker)pair.Value;
					if (t.fieldName == "@exp_date") {
						find_expires_control(form);
						Debug.Assert(expires_control != null && expires_cbx_control != null);
						picker.Value = expires_control.Value;
						picker.Checked = expires_cbx_control.Checked;
					} else {
						DateTime val;
						if (DateTime.TryParse(str.ReadString(), out val))
							picker.Value = val;
					}
				}
			}
		}
	}
}
