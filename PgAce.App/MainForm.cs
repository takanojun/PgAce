using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using PgAce.App.Views;

namespace PgAce.App
{
    public class MainForm : Form
    {
        private readonly DockPanel _dockPanel;
        private readonly ResultGridView _resultGrid;

        public MainForm()
        {
            Text = "PgAce";
            Width = 800;
            Height = 600;

            _dockPanel = new DockPanel();
            _dockPanel.Dock = DockStyle.Fill;
            Controls.Add(_dockPanel);

            var dbExplorer = new DbExplorerView();
            dbExplorer.Show(_dockPanel, DockState.DockLeft);

            var sqlEditor = new SqlEditorView();
            sqlEditor.Show(_dockPanel, DockState.Document);

            var sqlExplorer = new SqlExplorerView();
            sqlExplorer.Show(_dockPanel, DockState.DockRight);

            _resultGrid = new ResultGridView();
            _resultGrid.Show(_dockPanel, DockState.DockBottom);

            var properties = new PropertiesView();
            properties.Show(_resultGrid.Pane, DockAlignment.Right, 0.5);

            var searchView = new SearchView();
            searchView.SearchRequested += OnSearchRequested;
            searchView.Show(_dockPanel, DockState.DockTop);
        }

        private void OnSearchRequested(string query)
        {
            var items = new[]
            {
                new { Id = 1, Name = "Apple" },
                new { Id = 2, Name = "Banana" },
                new { Id = 3, Name = "Cherry" },
                new { Id = 4, Name = "Date" }
            };

            var filtered = items.Where(i => i.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));

            foreach (var item in filtered)
            {
                table.Rows.Add(item.Id, item.Name);
            }

            _resultGrid.SetResults(table);
        }
    }
}
