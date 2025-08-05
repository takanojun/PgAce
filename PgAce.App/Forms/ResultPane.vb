Imports System.Windows.Forms
Imports WeifenLuo.WinFormsUI.Docking

Namespace PgAce.App.Forms
    Public Class ResultPane
        Inherits DockContent

        Public Sub New()
            Text = "Result"
            Dim grid = New DataGridView() With {
                .Dock = DockStyle.Fill
            }
            Controls.Add(grid)
        End Sub
    End Class
End Namespace
