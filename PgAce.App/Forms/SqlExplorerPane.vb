Imports System.Windows.Forms
Imports WeifenLuo.WinFormsUI.Docking

Namespace PgAce.App.Forms
    Public Class SqlExplorerPane
        Inherits DockContent

        Public Sub New()
            Text = "SQL Explorer"
            Dim tree = New TreeView() With {
                .Dock = DockStyle.Fill
            }
            Controls.Add(tree)
        End Sub
    End Class
End Namespace
