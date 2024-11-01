Imports MySql.Data.MySqlClient

Public Class Form1
    Private Sub btnEnter_Click(sender As Object, e As EventArgs) Handles btnEnter.Click

        Dim username As String = txtUsername.Text
        Dim password As String = txtPassword.Text

        Dim connectionString As String = authenticationMod.connectDatabase()

        Using conn As New MySqlConnection(connectionString)

            conn.Open()

            Using cmd As New MySqlCommand("sp_RetrievePasswordSalt", conn)

                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@inputUsername", username)

                Using reader As MySqlDataReader = cmd.ExecuteReader

                    If reader.Read() Then

                        Dim storedPassword As String = reader("passwd").ToString()
                        Dim storedSalt As String = reader("salt").ToString()

                        Dim hashedPassword As String = authenticationMod.ComputeSHA256HashWithSalt(password, storedSalt)

                        MessageBox.Show("storedPassword: " & storedPassword)
                        MessageBox.Show("hashedPassword: " & hashedPassword)

                        If (storedPassword = hashedPassword) Then

                            MessageBox.Show("Correct login")

                            txtUsername.Clear()
                            txtPassword.Clear()

                            pgEnterUserDetails.Show()
                            Me.Hide()

                        Else

                            MessageBox.Show("Incorrect login")

                        End If


                    End If

                End Using


            End Using


        End Using



    End Sub

    Private Sub lblClose_Click(sender As Object, e As EventArgs) Handles lblClose.Click

        Me.Close()

    End Sub


End Class
