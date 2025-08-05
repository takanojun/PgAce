using ScintillaNET;
using WeifenLuo.WinFormsUI.Docking;
using System.Windows.Forms;

namespace PgAce.App.Forms
{
    public class SqlEditorPane : DockContent
    {
        public SqlEditorPane()
        {
            Text = "SQL Editor";
            var editor = new Scintilla
            {
                Dock = DockStyle.Fill,
            };
            Controls.Add(editor);
        }
    }
}
