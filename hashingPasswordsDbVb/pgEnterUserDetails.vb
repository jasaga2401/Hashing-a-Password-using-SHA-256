
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports MySql.Data.MySqlClient
Imports System.Security.Cryptography
Imports System.Text

Public Class pgEnterUserDetails

    Private connectionString As String = authenticationMod.connectDatabase()
    Private currentIndex As Integer = 0
    Private adapter As MySqlDataAdapter
    Private dataSet As DataSet


    Private Sub pgEnterUserDetails_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        loadUserInformation()

    End Sub

    Private Sub loadUserInformation(Optional ByVal currentIndex As Integer = 0)

        ' MessageBox.Show("Loading information here")

        Using conn As New MySqlConnection(connectionString)

            conn.Open()

            Using cmd As New MySqlCommand("sp_GetAllUsers", conn)

                cmd.CommandType = CommandType.StoredProcedure

                adapter = New MySqlDataAdapter(cmd)
                dataSet = New DataSet()
                adapter.Fill(dataSet, "tbluser")

                If dataSet.Tables("tbluser").Rows.Count > 0 Then
                    DisplayRecord(currentIndex)
                End If

            End Using

        End Using

    End Sub

    Private Sub DisplayRecord(currentIndex As Integer)

        'MessageBox.Show("Abount to display information" & currentIndex)


        Dim row As DataRow = dataSet.Tables("tbluser").Rows(currentIndex)

        If dataSet Is Nothing OrElse dataSet.Tables("tbluser").Rows.Count = 0 Then
            MessageBox.Show("No record found")

        Else

            txtUid.Text = row("uid").ToString()
            txtTitle.Text = row("title").ToString()
            txtForename.Text = row("forename").ToString()
            txtSurname.Text = row("surname").ToString()
            txtUsername.Text = row("username").ToString()
            txtPassword.Text = row("passwd").ToString()

        End If

    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click

        If currentIndex < dataSet.Tables("tbluser").Rows.Count - 1 Then
            currentIndex += 1
            DisplayRecord(currentIndex)
        End If

    End Sub

    Private Sub btnPrevious_Click(sender As Object, e As EventArgs) Handles btnPrevious.Click

        If currentIndex > 0 Then
            currentIndex -= 1
            DisplayRecord(currentIndex)
        End If

    End Sub

    Private Sub btnFirst_Click(sender As Object, e As EventArgs) Handles btnFirst.Click

        currentIndex = 0
        DisplayRecord(currentIndex)

    End Sub

    Private Sub btnLast_Click(sender As Object, e As EventArgs) Handles btnLast.Click

        currentIndex = dataSet.Tables("tbluser").Rows.Count - 1
        DisplayRecord(currentIndex)

    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click

        ' Textboxes are empty
        If String.IsNullOrWhiteSpace(txtUid.Text) AndAlso
            String.IsNullOrWhiteSpace(txtTitle.Text) AndAlso
            String.IsNullOrWhiteSpace(txtForename.Text) AndAlso
            String.IsNullOrWhiteSpace(txtSurname.Text) AndAlso
            String.IsNullOrWhiteSpace(txtUsername.Text) AndAlso
            String.IsNullOrWhiteSpace(txtPassword.Text) Then
            MessageBox.Show("Please enter a record.")
        Else

            ' Textboxes are not empty, they are full
            Using conn As New MySqlConnection(connectionString)

                conn.Open()

                Using cmd As New MySqlCommand("sp_CheckUserExists", conn)
                    cmd.CommandType = CommandType.StoredProcedure

                    cmd.Parameters.AddWithValue("@userid", txtUid.Text)
                    Dim userExists As Integer = Convert.ToInt32(cmd.ExecuteScalar())

                    ' The user exists
                    If userExists > 0 Then
                        clearTextBoxes()
                        btnAdd.Text = "Add User"
                        MessageBox.Show("Enter a new user")

                    ElseIf (userExists = 0) Then

                        ' Need to hash the password using SHA-256
                        Dim salt As String = authenticationMod.GenerateSalt()
                        Dim hashedPassword As String = authenticationMod.ComputeSHA256HashWithSalt(txtPassword.Text, salt)
                        MessageBox.Show("Password length: " & hashedPassword.Length)

                        Using cmdAdd As New MySqlCommand("sp_AddUser", conn)
                            cmdAdd.CommandType = CommandType.StoredProcedure

                            cmdAdd.Parameters.AddWithValue("@title", txtTitle.Text)
                            cmdAdd.Parameters.AddWithValue("@forename", txtForename.Text)
                            cmdAdd.Parameters.AddWithValue("@surname", txtSurname.Text)
                            cmdAdd.Parameters.AddWithValue("@username", txtUsername.Text)
                            cmdAdd.Parameters.AddWithValue("@passwd", hashedPassword)
                            cmdAdd.Parameters.AddWithValue("@salt", salt)

                            Dim rowsAffected As Integer = cmdAdd.ExecuteNonQuery()

                            If rowsAffected > 0 Then
                                MessageBox.Show("Record added successfully")
                                btnAdd.Text = "Enter User"
                                currentIndex = dataSet.Tables("tbluser").Rows.Count
                                loadUserInformation(currentIndex)
                            Else
                                MessageBox.Show("Record not added")
                            End If

                        End Using

                    End If

                End Using

            End Using

        End If

    End Sub

    Private Sub clearTextBoxes()

        txtUid.Text = ""
        txtTitle.Text = ""
        txtForename.Text = ""
        txtSurname.Text = ""
        txtUsername.Text = ""
        txtPassword.Text = ""

    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click

        If String.IsNullOrEmpty(txtUid.Text) Then
            MessageBox.Show("No records to delete.")
            Return
        End If

        Dim confirmResult As DialogResult = MessageBox.Show("Are you sure you want to delete this record?", "Confirm Delete", MessageBoxButtons.YesNo)

        If confirmResult = DialogResult.Yes Then

            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                Using cmdDelete As New MySqlCommand("sp_DeleteUser", conn)
                    cmdDelete.CommandType = CommandType.StoredProcedure
                    cmdDelete.Parameters.AddWithValue("@userid", txtUid.Text)

                    ' Execute the delete command
                    Dim rowsAffected As Integer = cmdDelete.ExecuteNonQuery()
                    If rowsAffected > 0 Then

                        ' checking the number of records and altering the 
                        Using cmdCount As New MySqlCommand("sp_CountUsers", conn)
                            cmdCount.CommandType = CommandType.StoredProcedure

                            ' Counts the number of recosrds in the table
                            Dim count As Integer = cmdCount.ExecuteScalar()

                            If (count = 0) Then
                                MessageBox.Show("No records in the database")
                                clearTextBoxes()
                            ElseIf (count = 1) Then
                                currentIndex = 0
                                loadUserInformation()
                            ElseIf (count > 1) Then
                                currentIndex = currentIndex - 1
                                loadUserInformation()
                            End If

                        End Using

                    Else
                        MessageBox.Show("No record found to delete.")
                    End If

                End Using

            End Using

        End If

    End Sub

    Private Sub lblClose_Click(sender As Object, e As EventArgs) Handles lblClose.Click

        Me.Close()

    End Sub

    Private Sub lblLogout_Click(sender As Object, e As EventArgs) Handles lblLogout.Click


        Form1.Show()
        Me.Hide()

    End Sub
End Class