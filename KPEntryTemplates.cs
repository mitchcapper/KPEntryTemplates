using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using KeePass.Forms;
using KeePass.Plugins;
using KeePass.UI;
using KeePass.Util;
using KeePassLib;
namespace KPEntryTemplates {
	public sealed class KPEntryTemplatesExt : Plugin {
		private IPluginHost m_host;
		private ToolStripMenuItem m_tsmi_set_template_parent;
		private ToolStripMenuItem m_tsmi_copy_template_string;
		private DynamicMenu m_dynCustomStrings;
		private System.ComponentModel.CancelEventHandler entry_context_menu_opening_handler;
		private EventHandler<TemplateEntryEventArgs> entry_templates_entry_creating_handler;
		private EventHandler<GwmWindowEventArgs> global_window_manager_window_added_handler;
		public override bool Initialize(IPluginHost host) {
			Debug.Assert(host != null);
			if (host == null) return false;
			m_host = host;
			global_window_manager_window_added_handler = new EventHandler<GwmWindowEventArgs>(GlobalWindowManager_WindowAdded);
			GlobalWindowManager.WindowAdded += global_window_manager_window_added_handler;
			ToolStripItemCollection tsMenu = m_host.MainWindow.EntryContextMenu.Items;
			m_tsmi_set_template_parent = new ToolStripMenuItem();
			m_tsmi_set_template_parent.Text = "Set Template Parent";
			m_tsmi_set_template_parent.Click += m_tsmi_set_template_parent_Click;
			m_tsmi_set_template_parent.Image = Resources.Resources.B16x16_KOrganizer;
			tsMenu.Add(m_tsmi_set_template_parent);
			m_tsmi_copy_template_string = new ToolStripMenuItem();
			m_tsmi_copy_template_string.Text = "Copy Template String";
			m_tsmi_copy_template_string.Image = Resources.Resources.B16x16_KOrganizer;
			m_dynCustomStrings = new DynamicMenu(m_tsmi_copy_template_string);
			m_dynCustomStrings.MenuClick += m_dynCustomStrings_MenuClick;
			tsMenu.Add(m_tsmi_copy_template_string);

			entry_context_menu_opening_handler = new System.ComponentModel.CancelEventHandler(EntryContextMenu_Opening);
			m_host.MainWindow.EntryContextMenu.Opening += entry_context_menu_opening_handler;
			entry_templates_entry_creating_handler = new EventHandler<TemplateEntryEventArgs>(EntryTemplates_EntryCreating);
			EntryTemplates.EntryCreating += entry_templates_entry_creating_handler;

			return true;
		}

		void GlobalWindowManager_WindowAdded(object sender, GwmWindowEventArgs e) {
			PwEntryForm form = e.Form as PwEntryForm;
			if (form == null)
				return;
			form.Shown += form_Shown;
            form.Resize += form_Resize;
		}

        void form_Shown(object sender, EventArgs e)
        {
            PwEntryForm form = sender as PwEntryForm;
            new EntryTemplateManager(m_host, form);
        }

        void form_Resize(object sender, EventArgs e)
        {
            // on form resize, change edits and bottom button widths;
            // also reposition right side buttons

            PwEntryForm form = sender as PwEntryForm;

            TabControl tabControl = null;
            foreach (Control c in form.Controls) {
                if (c is TabControl) {
                    tabControl = c as TabControl;
                    break;
                }
            }
            if (tabControl == null) return;

            TabPage tmplPage = tabControl.TabPages[0];
            if (tmplPage.Text != "Template") return;

            foreach (Control c in tmplPage.Controls) {
                if (!(c is Label)) {
                    if (c is CheckBox) {
                        c.Left = tmplPage.Width - ((c.Width + 55) / 2);
                    } else if (c is Button) {
                        if ((c as Button).Text == "Remove As Template Child") {
                            c.Width = tmplPage.Width - c.Left - 55;
                        } else {
                            c.Left = tmplPage.Width - ((c.Width + 55) / 2);
                        }
                    } else {
                        c.Width = tmplPage.Width - c.Left - 55;
                    }
                }
            }
        }

		void EntryTemplates_EntryCreating(object sender, TemplateEntryEventArgs e) {
			EntryTemplateManager.InitChildEntry(e.TemplateEntry, e.Entry);
		}

		void m_dynCustomStrings_MenuClick(object sender, DynamicMenuEventArgs e) {
			PwEntry pe = m_host.MainWindow.GetSelectedEntry(false);
			if (pe == null) return;

			if (ClipboardUtil.CopyAndMinimize(pe.Strings.ReadSafe((string)e.Tag), true, m_host.MainWindow, pe, m_host.Database))
				m_host.MainWindow.StartClipboardCountdown();
		}

		void m_tsmi_set_template_parent_Click(object sender, EventArgs e) {
			if (EntryTemplateManager.entry_is_in_template_group(m_host, m_host.MainWindow.GetSelectedGroup())) {
				MessageBox.Show("Cannot set the template parent on a template");
				return;
			}
			PwEntry parent = EntryTemplateManager.show_parent_template_chooser(m_host);
			if (parent == null)
				return;
			PwEntry[] entries = m_host.MainWindow.GetSelectedEntries();
			foreach (PwEntry entry in entries) {
				EntryTemplateManager.set_entry_template_parent(m_host.Database,entry, parent);
			}
			m_host.MainWindow.UpdateUI(false, null, false, m_host.MainWindow.GetSelectedGroup(), false, null, true);
		}


		private bool show_copy_menu() {
			return m_host.MainWindow.GetSelectedEntriesCount() == 1 && EntryTemplateManager.is_template_child(m_host.MainWindow.GetSelectedEntry(false));
		}
		void EntryContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
			m_tsmi_set_template_parent.Enabled = m_host.MainWindow.GetSelectedEntriesCount() != 0;
			m_tsmi_copy_template_string.Visible = show_copy_menu();
			if (show_copy_menu()) {
				m_dynCustomStrings.Clear();
				PwEntry pe = m_host.MainWindow.GetSelectedEntry(true);
				Dictionary<string, string> title_to_field = EntryTemplateManager.get_template_title_to_field_dict(m_host, EntryTemplateManager.get_entry_template_parent_uuid(pe));
				foreach (KeyValuePair<string, string> kvp in title_to_field)
					m_dynCustomStrings.AddItem(kvp.Key, Resources.Resources.B16x16_KGPG_Info, kvp.Value);
			}
		}
		public override string UpdateUrl {
			get {
				return "http://mitchcapper.com/keepass_versions.txt?KPET";
			}
		}
		public override void Terminate() {
			ToolStripItemCollection tsMenu = m_host.MainWindow.EntryContextMenu.Items;
			GlobalWindowManager.WindowAdded -= global_window_manager_window_added_handler;
			tsMenu.Remove(m_tsmi_set_template_parent);
			tsMenu.Remove(m_tsmi_copy_template_string);
			m_host.MainWindow.EntryContextMenu.Opening -= entry_context_menu_opening_handler;
			EntryTemplates.EntryCreating -= entry_templates_entry_creating_handler;
		}
	}
}
