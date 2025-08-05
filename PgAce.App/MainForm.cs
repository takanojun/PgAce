using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PgAce.App
{
    public class MainForm : Form
    {
        private readonly DockPanel _dockPanel;

        public MainForm()
        {
            Text = "PgAce";
            Width = 800;
            Height = 600;

            _dockPanel = new DockPanel();
            _dockPanel.Dock = DockStyle.Fill;
            Controls.Add(_dockPanel);

            var dbExplorer = new Views.DbExplorerView();
            dbExplorer.Show(_dockPanel, DockState.DockLeft);

            var sqlEditor = new Views.SqlEditorView();
            sqlEditor.Show(_dockPanel, DockState.Document);

            var sqlExplorer = new Views.SqlExplorerView();
            sqlExplorer.Show(_dockPanel, DockState.DockRight);

            var resultGrid = new Views.ResultGridView();
            resultGrid.Show(_dockPanel, DockState.DockBottom);

            var properties = new Views.PropertiesView();
            properties.Show(resultGrid.Pane, DockAlignment.Right, 0.5);
        }
    }
}
