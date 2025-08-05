Imports ScintillaNET
Imports WeifenLuo.WinFormsUI.Docking
Imports System.Windows.Forms

Namespace PgAce.App.Forms
    Public Class SqlEditorPane
        Inherits DockContent

        Public Sub New()
            Text = "SQL Editor"
            Dim editor = New Scintilla() With {
                .Dock = DockStyle.Fill
            }
            Controls.Add(editor)
        End Sub
    End Class
End Namespace
