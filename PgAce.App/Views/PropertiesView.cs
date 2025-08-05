using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace PgAce.App.Views
{
    public class PropertiesView : DockContent
    {
        private readonly TextBox _text;

        public PropertiesView()
        {
            Text = "Properties";
            _text = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both };
            Controls.Add(_text);
        }
    }
}
