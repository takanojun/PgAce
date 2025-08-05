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

            Dim data As Byte() = Encoding.UTF8.GetBytes(plaintext)
            Dim protectedData As Byte() = ProtectedData.Protect(data, Nothing, DataProtectionScope.CurrentUser)

            Return Prefix & Convert.ToBase64String(protectedData)
        End Function

        Public Shared Function Unprotect(cipher As String) As String
            If String.IsNullOrEmpty(cipher) Then Return cipher
            If Not cipher.StartsWith(Prefix) Then Return cipher

            Dim base64 As String = cipher.Substring(Prefix.Length)
            Dim protectedData As Byte() = Convert.FromBase64String(base64)

            Try
                Dim unprotectedData As Byte() = ProtectedData.Unprotect(protectedData, Nothing, DataProtectionScope.CurrentUser)
                Return Encoding.UTF8.GetString(unprotectedData)
            Catch ex As CryptographicException
                Return String.Empty ' failed to decrypt
            End Try
        End Function
    End Class
End Namespace
