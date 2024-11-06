Imports System.Security.Cryptography
Imports System.Text

' Module to stored functions that help authenticate the database and can be reused
Module authenticationMod

    ' Hashing algorith using SHA-256
    Public Function ComputeSHA256HashWithSalt(ByVal input As String, ByVal salt As String) As String
        Using sha256 As SHA256 = SHA256.Create()
            ' Combine the input and salt
            Dim combinedBytes As Byte() = Encoding.UTF8.GetBytes(input & salt)

            ' Compute the hash
            Dim hash As Byte() = sha256.ComputeHash(combinedBytes)

            ' Convert hash to hexadecimal string
            Dim builder As New StringBuilder()
            For Each b As Byte In hash
                builder.Append(b.ToString("x2"))
            Next
            Return builder.ToString()
        End Using
    End Function

    ' Program to generate a salt
    Public Function GenerateSalt(Optional ByVal size As Integer = 16) As String

        Dim rng As New RNGCryptoServiceProvider()
        Dim saltBytes(size - 1) As Byte
        rng.GetBytes(saltBytes)
        Return Convert.ToBase64String(saltBytes)

    End Function

    Public Function connectDatabase()


        ' Database connection string
        Dim connectionString As String = "Server=localhost; database=dbhash; User ID=root; Password=12Yellow34!"

        Return connectionString

    End Function

End Module
