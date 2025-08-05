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
            For Each profile In cfg.Profiles
                profile.Password = EncryptionService.Unprotect(profile.Password)
            Next
            Return cfg
        End Function

        Public Sub Save(config As ConfigRoot)
            Dim clone = JsonConvert.DeserializeObject(Of ConfigRoot)(JsonConvert.SerializeObject(config))

            If clone.Settings.SavePassword Then
                For Each profile In clone.Profiles
                    profile.Password = EncryptionService.Protect(profile.Password)
                Next
            Else
                For Each profile In clone.Profiles
                    profile.Password = String.Empty
                Next
            End If

            Dim json = JsonConvert.SerializeObject(clone, Formatting.Indented)
            File.WriteAllText(_path, json)
        End Sub
    End Class
End Namespace
