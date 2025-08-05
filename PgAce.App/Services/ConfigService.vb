Imports System.IO
Imports Newtonsoft.Json
Imports PgAce.App.Models

Namespace PgAce.App.Services
    Public Class ConfigService
        Private ReadOnly _path As String

        Public Sub New(path As String)
            _path = path
        End Sub

        Public Function Load() As ConfigRoot
            If Not File.Exists(_path) Then
                Return New ConfigRoot()
            End If

            Dim json = File.ReadAllText(_path)
            Dim cfg = JsonConvert.DeserializeObject(Of ConfigRoot)(json)
            If cfg Is Nothing Then
                Return New ConfigRoot()
            End If
            Return cfg
        End Function

        Public Sub Save(config As ConfigRoot)
            Dim json = JsonConvert.SerializeObject(config, Formatting.Indented)
            File.WriteAllText(_path, json)
        End Sub
    End Class
End Namespace
