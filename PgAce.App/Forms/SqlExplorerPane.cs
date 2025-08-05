using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PgAce.App.Forms
{
    public class SqlExplorerPane : DockContent
    {
        public SqlExplorerPane()
        {
            Text = "SQL Explorer";
            var tree = new TreeView
            {
                Dock = DockStyle.Fill,
            };
            Controls.Add(tree);
        }
    }
}
