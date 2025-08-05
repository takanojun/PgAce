using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PgAce.App.Views
{
    public class ResultGridView : DockContent
    {
        private readonly DataGridView _grid;

        public ResultGridView()
        {
            Text = "Result";
            _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true };
            Controls.Add(_grid);
        }
    }
}
