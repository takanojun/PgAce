using WeifenLuo.WinFormsUI.Docking;

namespace PgAce.App.Forms
{
    public class DbExplorerPane : DockContent
    {
        public DbExplorerPane()
        {
            Text = "DB Explorer";
            DockAreas = DockAreas.DockLeft | DockAreas.DockRight | DockAreas.Float;
        }
    }
}
