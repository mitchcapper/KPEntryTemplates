using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using KeePass.Forms;
using KeePass.Plugins;
using KeePassLib;
using KeePassLib.Collections;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KPEntryTemplates {
	partial class EntryTemplateManager {
		private const int TAB_WIDTH = 465;
		private const int TAB_HEIGHT = 350;

		private PwEntryForm form;
		private IPluginHost m_host;
		private TabPage our_page;
		private TabControl form_tab_control;
		private Control get_control_from_form(Form form, String name) {
			Control[] cntrls = form.Controls.Find(name, true);
			if (cntrls.Length == 0)
				return null;
			return cntrls[0];
		}
		public EntryTemplateManager(IPluginHost host, PwEntryForm form) {
			m_host = host;
			this.form = form;
			our_page = new TabPage("Template");
			our_page.AutoScroll = true;

			form_tab_control = get_control_from_form(form, "m_tabMain") as TabControl;
			Debug.Assert(form_tab_control != null);
			form_tab_control.Selecting += main_tabs_control_Selecting;
			_entry_is_template = form.EntryStrings.Get("_etm_template") != null;
			ProtectedString str = form.EntryStrings.Get("_etm_template_uuid");
			entry_is_child = str != null;
			if (entry_is_child)
				child_template_uuid = str.ReadString();
			form.EntrySaving += form_EntrySaving;
			our_page.UseVisualStyleBackColor = true;
			form_tab_control.TabPages.Insert(0, our_page);
			if (entry_is_child || entry_is_template)
				form_tab_control.SelectTab(0);
		}

		void form_EntrySaving(object sender, KeePass.Util.CancellableOperationEventArgs e) {
			if (am_current_tab) {
				if (!check_confirm_password_ok()) {
					e.Cancel = true;
					return;
				}
				save_changes();
				form.UpdateEntryStrings(false, true);
			}
		}


		public static bool is_template_child(PwEntry entry) {
			if (entry == null)
				return false;
			return entry.Strings.Exists("_etm_template_uuid");
		}

		private string child_template_uuid;
		private bool am_current_tab;
		private bool _entry_is_template;
		private bool entry_is_child;
		private bool showing_data_grid;
		private bool showing_buttons;
		private bool entry_is_template {
			get { return _entry_is_template; }
			set {
				_entry_is_template = value;
				if (_entry_is_template)
					form.EntryStrings.Set("_etm_template", new ProtectedString(false, "1"));
				else {
					if (form.EntryStrings.Get("_etm_template") != null) {
						form.EntryStrings.Remove("_etm_template");
						RemoveDataGridView(our_page);
						showing_data_grid = false;
					}
				}
			}
		}
		private void data_grid_show() {
			if (!showing_data_grid)
				InitializeGridView(our_page);
			init_data_table();
		}
		private Button btnSetupAsTemplate;
		private Button btnAddAsChildOfTemplate;
		private void child_view_show() {
				if (!InitializeChildView(our_page, child_template_uuid)) {
					MessageBox.Show("Unable to find parent template for this entry, removing as child");
					ProtectedString str = form.EntryStrings.Get("_etm_template_uuid");
					if (str != null)
						form.EntryStrings.Remove("_etm_template_uuid");
					entry_is_child = false;
					buttons_show();
					return;
				}					

			init_child_vals();
		}
		public static bool entry_is_in_template_group(IPluginHost m_host, PwGroup group) {
			if (group == null)//viewing a history entry
				return false;
			PwGroup template_grp = template_group(m_host);
			return template_grp == group || group.IsContainedIn(template_grp);
		}
		private void buttons_show() {
			if (!showing_buttons) {
				if (btnSetupAsTemplate == null) {
					btnSetupAsTemplate = new Button();
					btnSetupAsTemplate.Text = "Init As Template";
					btnSetupAsTemplate.Width = 200;
					btnSetupAsTemplate.Height = 30;
					btnSetupAsTemplate.Top = 75;
					btnSetupAsTemplate.Left = (our_page.Width - btnSetupAsTemplate.Width) / 2;
					btnSetupAsTemplate.Click += btnSetupAsTemplate_Click;

					btnAddAsChildOfTemplate = new Button();
					btnAddAsChildOfTemplate.Text = "Set Template Parent";
					btnAddAsChildOfTemplate.Width = 200;
					btnAddAsChildOfTemplate.Height = 30;
					btnAddAsChildOfTemplate.Top = 75;
					btnAddAsChildOfTemplate.Left = (our_page.Width - btnSetupAsTemplate.Width) / 2;
					btnAddAsChildOfTemplate.Click += btnAddAsChildOfTemplate_Click;
				}

				btnSetupAsTemplate.Visible = entry_is_in_template_group(m_host, form.EntryRef.ParentGroup);
				btnAddAsChildOfTemplate.Visible = !btnSetupAsTemplate.Visible;
				our_page.Controls.Add(btnSetupAsTemplate);
				our_page.Controls.Add(btnAddAsChildOfTemplate);
				showing_buttons = true;
			}
		}
		private static PwGroup template_group(IPluginHost m_host) {
			return m_host.Database.RootGroup.FindGroup(m_host.Database.EntryTemplatesGroup, true);
		}
		public static PwObjectList<PwEntry> GetPossibleTemplates(IPluginHost m_host) {

			PwObjectList<PwEntry> entries = new PwObjectList<PwEntry>();
			PwGroup group = template_group(m_host);
			if (group == null)
				return entries;
			PwObjectList<PwEntry> all_entries = group.GetEntries(true);
			foreach (PwEntry entry in all_entries) {
				if (entry.Strings.Exists("_etm_template"))
					entries.Add(entry);
			}
			return entries;
		}
		public static PwEntry show_parent_template_chooser(IPluginHost m_host) {
			EntryListForm elf = new EntryListForm();
			PwObjectList<PwEntry> entries = GetPossibleTemplates(m_host);
			elf.InitEx("Select Parent Template Entry", "Selecting Parent Template Entry", "Select the parent entry to use as a template", Resources.Resources.B48x48_Folder_Txt, m_host.MainWindow.ClientIcons, entries);
			elf.EnsureForeground = true;

			if (elf.ShowDialog() != DialogResult.OK || elf.SelectedEntry == null)
				return null;
			return elf.SelectedEntry;
		}
		public static string get_entry_template_parent_uuid(PwEntry entry) {
			ProtectedString str = entry.Strings.Get("_etm_template_uuid");
			if (str == null)
				return null;
			return str.ReadString();
		}
		public static void set_entry_template_parent(PwDatabase m_pwDatabase, PwEntry entry, PwEntry parent) {
			entry.Strings.Set("_etm_template_uuid", new ProtectedString(false, parent.Uuid.ToHexString()));
			TouchSaveEntry(m_pwDatabase, entry, false, true);
		}
		private static void TouchSaveEntry(PwDatabase m_pwDatabase,PwEntry m_pwEntry, bool is_new, bool update_parents){
			//Save procedure taken from 

			PwObjectList<PwEntry> m_vHistory = m_pwEntry.History.CloneDeep();
			PwEntry peTarget = m_pwEntry;
			peTarget.History = m_vHistory; // Must be called before CreateBackup()
			if (!is_new)
				peTarget.CreateBackup(null);
			peTarget.Touch(true, update_parents); // Touch *after* backup
			StrUtil.NormalizeNewLines(peTarget.Strings, true);
			peTarget.MaintainBackups(m_pwDatabase);

		}
		void btnAddAsChildOfTemplate_Click(object sender, EventArgs e) {
			PwEntry ent = show_parent_template_chooser(m_host);
			if (ent == null)
				return;
			child_template_uuid = ent.Uuid.ToHexString();
			form.EntryStrings.Set("_etm_template_uuid", new ProtectedString(false, child_template_uuid));
			buttons_hide();
			entry_is_child = true;
			child_view_show();
		}
		private void buttons_hide() {
			if (!showing_buttons)
				return;
			our_page.Controls.Remove(btnSetupAsTemplate);
			our_page.Controls.Remove(btnAddAsChildOfTemplate);
			showing_buttons = false;
		}
		void btnSetupAsTemplate_Click(object sender, EventArgs e) {
			buttons_hide();
			entry_is_template = true;
			data_grid_show();


		}
		private void save_changes() {
			if (entry_is_template) {
				List<EntryTemplate> cur = export_table();
				write_entry(form, cur);
				entry_is_template = cur.Count != 0;
			}
			else if (entry_is_child)
				save_child_vals();
		}
		private void main_tabs_control_Selecting(object sender, TabControlCancelEventArgs e) {

			if (e.TabPage != our_page) {
				if (am_current_tab) {
					if (!check_confirm_password_ok()) {
						e.Cancel = true;
						return;
					}
					save_changes();
					form.UpdateEntryStrings(false, true);
				}
				am_current_tab = false;
			}
			else {
				if (!am_current_tab) {
					form.UpdateEntryStrings(true, false);
					if (entry_is_template)
						data_grid_show();
					else if (entry_is_child)
						child_view_show();
					else
						buttons_show();
					am_current_tab = true;
					if (entry_is_child & our_page != null && our_page.Controls.Count > 1){
						TextBox box = our_page.Controls[1] as TextBox;
						box.Select(0, 0);
					}
				}
				
			}
			e.Cancel = false;
		}
		private void erase_entry_template_items(ProtectedStringDictionary Strings) {
			string[] names = get_protected_dictionary_names(Strings);
			foreach (string name in names) {
				if (name.StartsWith("_etm_"))
					Strings.Remove(name);
			}
		}
		private static string[] get_protected_dictionary_names(ProtectedStringDictionary Strings) {
			String[] names = new string[Strings.UCount];
			int pos = 0;
			foreach (KeyValuePair<string, ProtectedString> kvpStr in Strings)
				names[pos++] = kvpStr.Key;
			return names;
		}
		public static void InitChildEntry(PwEntry template, PwEntry entry) {
			if (template.Strings.Get("_etm_template") == null)
				return;
			string[] names = get_protected_dictionary_names(entry.Strings);
			foreach (string name in names) {
				if (name.StartsWith("_etm_"))
					entry.Strings.Remove(name);
			}
			string child_template_uuid = template.Uuid.ToHexString();
			entry.Strings.Set("_etm_template_uuid", new ProtectedString(false, child_template_uuid));

		}
		private void write_entry(PwEntryForm form, IEnumerable<EntryTemplate> to_add) {
			erase_entry_template_items(form.EntryStrings);
			foreach (EntryTemplate t in to_add) {
				form.EntryStrings.Set("_etm_title_" + t.fieldName, new ProtectedString(false, t.title));
				form.EntryStrings.Set("_etm_type_" + t.fieldName, new ProtectedString(false, t.type));
				form.EntryStrings.Set("_etm_position_" + t.fieldName, new ProtectedString(false, t.position.ToString()));
				form.EntryStrings.Set("_etm_options_" + t.fieldName, new ProtectedString(false, t.options ?? ""));
			}
		}

		private static List<EntryTemplate> parse_entry(ProtectedStringDictionary Strings) {

			List<EntryTemplate> ret = new List<EntryTemplate>();
			string[] names = get_protected_dictionary_names(Strings);
			foreach (string name in names) {
				if (name.StartsWith("_etm_title_")) {
					String fieldName = name.Substring("_etm_title_".Length);
					ProtectedString str = Strings.Get("_etm_title_" + fieldName);
					String title = str == null ? "" : str.ReadString();
					str = Strings.Get("_etm_type_" + fieldName);
					String type = str == null ? "" : str.ReadString();
					str = Strings.Get("_etm_position_" + fieldName);
					String position = str == null ? "" : str.ReadString();
					str = Strings.Get("_etm_options_" + fieldName);
					String options = str == null ? "" : str.ReadString();
					ret.Add(new EntryTemplate(title, fieldName, type, position, options));
				}
			}
			ret.Sort((t1, t2) => t1.position.CompareTo(t2.position));

			return ret;
		}
		private class EntryTemplate {
			public string title;
			public string fieldName;
			public string type;
			public string options;
			public int position;
			public EntryTemplate(string title, string fieldName, string type, int position, String options) {
				this.title = title;
				this.fieldName = fieldName;
				this.type = type;
				this.position = position;
				this.options = options;
			}
			public EntryTemplate(string title, string fieldName, string type, string position, String options) {
				int pos;
				Int32.TryParse(position, out pos);
				this.title = title;
				this.fieldName = fieldName;
				this.type = type;
				this.position = pos;
				this.options = options;
			}
		}
	}
}
