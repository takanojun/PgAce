Imports WeifenLuo.WinFormsUI.Docking

Namespace PgAce.App.Forms
    Public Class DbExplorerPane
        Inherits DockContent

        Public Sub New()
            Text = "DB Explorer"
            DockAreas = DockAreas.DockLeft Or DockAreas.DockRight Or DockAreas.Float
        End Sub
    End Class
End Namespace
