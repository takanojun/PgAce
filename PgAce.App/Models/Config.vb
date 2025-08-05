Imports System.Collections.Generic

Namespace PgAce.App.Models
    Public Class ConnectionProfile
        Public Property Name As String = String.Empty
        Public Property Host As String = String.Empty
        Public Property Port As Integer = 5432
        Public Property Database As String = String.Empty
        Public Property Username As String = String.Empty
        Public Property Password As String = String.Empty
    End Class

    Public Class AppSettings
        Public Property SavePassword As Boolean = True
        Public Property MaxHistory As Integer = 500
    End Class

    Public Class ConfigRoot
        Public Property Profiles As List(Of ConnectionProfile) = New List(Of ConnectionProfile)()
        Public Property Settings As AppSettings = New AppSettings()
    End Class
End Namespace
