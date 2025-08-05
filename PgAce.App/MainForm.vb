Imports System
Imports System.IO
Imports System.Windows.Forms
Imports WeifenLuo.WinFormsUI.Docking
Imports PgAce.App.Forms

Namespace PgAce.App
    Public Class MainForm
        Inherits Form

        Private ReadOnly _dockPanel As New DockPanel()
        Private ReadOnly _menuStrip As New MenuStrip()
        Private ReadOnly _statusStrip As New StatusStrip()
        Private Const LayoutFile As String = "layout.xml"

        Public Sub New()
            Text = "PgAce"
            Width = 1024
            Height = 768

            _dockPanel.Dock = DockStyle.Fill
            Controls.Add(_dockPanel)

            ' menu
            Dim fileMenu = New ToolStripMenuItem("&File")
            Dim exitItem = New ToolStripMenuItem("E&xit", Nothing, Sub(s, e) Close())
            fileMenu.DropDownItems.Add(exitItem)
            _menuStrip.Items.Add(fileMenu)
            MainMenuStrip = _menuStrip
            Controls.Add(_menuStrip)

            Controls.Add(_statusStrip)

            AddHandler Load, AddressOf MainForm_Load
            AddHandler FormClosing, AddressOf MainForm_FormClosing
        End Sub

        Private Sub MainForm_Load(sender As Object, e As EventArgs)
            If File.Exists(LayoutFile) Then
                Try
                    _dockPanel.LoadFromXml(LayoutFile, AddressOf DeserializeDockContent)
                Catch
                    ShowDefaultLayout()
                End Try
            Else
                ShowDefaultLayout()
            End If
        End Sub

        Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs)
            _dockPanel.SaveAsXml(LayoutFile)
        End Sub

        Private Function DeserializeDockContent(persistString As String) As IDockContent
            Select Case persistString
                Case NameOf(DbExplorerPane), "PgAce.App.Forms.DbExplorerPane"
                    Return New DbExplorerPane()
                Case NameOf(SqlEditorPane), "PgAce.App.Forms.SqlEditorPane"
                    Return New SqlEditorPane()
                Case NameOf(ResultPane), "PgAce.App.Forms.ResultPane"
                    Return New ResultPane()
                Case NameOf(PropertiesPane), "PgAce.App.Forms.PropertiesPane"
                    Return New PropertiesPane()
                Case NameOf(SqlExplorerPane), "PgAce.App.Forms.SqlExplorerPane"
                    Return New SqlExplorerPane()
            End Select
            Return Nothing
        End Function

        Private Sub ShowDefaultLayout()
            Dim dbExplorer = New DbExplorerPane()
            dbExplorer.Show(_dockPanel, DockState.DockLeft)

            Dim sqlExplorer = New SqlExplorerPane()
            sqlExplorer.Show(_dockPanel, DockState.DockLeft)

            Dim sqlEditor = New SqlEditorPane()
            sqlEditor.Show(_dockPanel, DockState.Document)

            Dim result = New ResultPane()
            result.Show(_dockPanel, DockState.DockBottom)

            Dim propertiesPane = New PropertiesPane()
            propertiesPane.Show(_dockPanel, DockState.DockRight)
        End Sub
    End Class
End Namespace
