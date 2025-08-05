using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PgAce.App.Forms
{
    public class PropertiesPane : DockContent
    {
        public PropertiesPane()
        {
            Text = "Properties";
            var propertyGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
            };
            Controls.Add(propertyGrid);
        }
    }
}
