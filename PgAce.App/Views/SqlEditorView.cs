using WeifenLuo.WinFormsUI.Docking;
using ScintillaNET;

namespace PgAce.App.Views
{
    public class SqlEditorView : DockContent
    {
        private readonly Scintilla _editor;

        public SqlEditorView()
        {
            Text = "SQL Editor";
            _editor = new Scintilla { Dock = System.Windows.Forms.DockStyle.Fill };
            Controls.Add(_editor);
        }
    }
}
