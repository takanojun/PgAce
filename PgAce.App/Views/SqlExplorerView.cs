using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PgAce.App.Views
{
    public class SqlExplorerView : DockContent
    {
        private readonly TreeView _tree;

        public SqlExplorerView()
        {
            Text = "SQL Explorer";
            _tree = new TreeView { Dock = DockStyle.Fill };
            Controls.Add(_tree);
        }
    }
}
