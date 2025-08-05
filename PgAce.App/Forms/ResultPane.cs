using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PgAce.App.Forms
{
    public class ResultPane : DockContent
    {
        public ResultPane()
        {
            Text = "Result";
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
            };
            Controls.Add(grid);
        }
    }
}
