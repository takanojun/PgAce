using System;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using PgAce.App.Forms;

namespace PgAce.App
{
    public class MainForm : Form
    {
        private readonly DockPanel _dockPanel = new DockPanel();
        private readonly MenuStrip _menuStrip = new MenuStrip();
        private readonly StatusStrip _statusStrip = new StatusStrip();
        private const string LayoutFile = "layout.xml";

        public MainForm()
        {
            Text = "PgAce";
            Width = 1024;
            Height = 768;

            _dockPanel.Dock = DockStyle.Fill;
            Controls.Add(_dockPanel);

            // menu
            var fileMenu = new ToolStripMenuItem("&File");
            var exitItem = new ToolStripMenuItem("E&xit", null, (s, e) => Close());
            fileMenu.DropDownItems.Add(exitItem);
            _menuStrip.Items.Add(fileMenu);
            MainMenuStrip = _menuStrip;
            Controls.Add(_menuStrip);

            Controls.Add(_statusStrip);

            Load += MainForm_Load;
            FormClosing += MainForm_FormClosing;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(LayoutFile))
            {
                try
                {
                    _dockPanel.LoadFromXml(LayoutFile, DeserializeDockContent);
                }
                catch
                {
                    ShowDefaultLayout();
                }
            }
            else
            {
                ShowDefaultLayout();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _dockPanel.SaveAsXml(LayoutFile);
        }

        private IDockContent DeserializeDockContent(string persistString)
        {
            switch (persistString)
            {
                case nameof(DbExplorerPane):
                case "PgAce.App.Forms.DbExplorerPane":
                    return new DbExplorerPane();
                case nameof(SqlEditorPane):
                case "PgAce.App.Forms.SqlEditorPane":
                    return new SqlEditorPane();
                case nameof(ResultPane):
                case "PgAce.App.Forms.ResultPane":
                    return new ResultPane();
                case nameof(PropertiesPane):
                case "PgAce.App.Forms.PropertiesPane":
                    return new PropertiesPane();
                case nameof(SqlExplorerPane):
                case "PgAce.App.Forms.SqlExplorerPane":
                    return new SqlExplorerPane();
            }
            return null;
        }

        private void ShowDefaultLayout()
        {
            var dbExplorer = new DbExplorerPane();
            dbExplorer.Show(_dockPanel, DockState.DockLeft);

            var sqlExplorer = new SqlExplorerPane();
            sqlExplorer.Show(_dockPanel, DockState.DockLeft);

            var sqlEditor = new SqlEditorPane();
            sqlEditor.Show(_dockPanel, DockState.Document);

            var result = new ResultPane();
            result.Show(_dockPanel, DockState.DockBottom);

            var properties = new PropertiesPane();
            properties.Show(_dockPanel, DockState.DockRight);
        }
    }
}
