Imports System.Windows.Forms
Imports WeifenLuo.WinFormsUI.Docking

Namespace PgAce.App.Forms
    Public Class PropertiesPane
        Inherits DockContent

        Public Sub New()
            Text = "Properties"
            Dim propertyGrid = New PropertyGrid() With {
                .Dock = DockStyle.Fill
            }
            Controls.Add(propertyGrid)
        End Sub
    End Class
End Namespace
