Imports System
Imports System.Security.Cryptography
Imports System.Text

Namespace PgAce.App.Services
    Public NotInheritable Class EncryptionService
        Private Sub New()
        End Sub

        Private Const Prefix As String = "dpapi:"

        Public Shared Function Protect(plaintext As String) As String
            If String.IsNullOrEmpty(plaintext) Then Return plaintext
            Dim data = Encoding.UTF8.GetBytes(plaintext)
            Dim protectedData = ProtectedData.Protect(data, Nothing, DataProtectionScope.CurrentUser)
            Return Prefix & Convert.ToBase64String(protectedData)
        End Function

        Public Shared Function Unprotect(cipher As String) As String
            If String.IsNullOrEmpty(cipher) Then Return cipher
            If Not cipher.StartsWith(Prefix) Then Return cipher
            Dim base64 = cipher.Substring(Prefix.Length)
            Dim protectedData = Convert.FromBase64String(base64)
            Try
                Dim data = ProtectedData.Unprotect(protectedData, Nothing, DataProtectionScope.CurrentUser)
                Return Encoding.UTF8.GetString(data)
            Catch ex As CryptographicException
                Return String.Empty ' failed to decrypt
            End Try
        End Function
    End Class
End Namespace
