<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class InfoForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.InfoTextBox = New System.Windows.Forms.TextBox()
        Me.InfoButton = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'InfoTextBox
        '
        Me.InfoTextBox.Location = New System.Drawing.Point(12, 12)
        Me.InfoTextBox.Multiline = True
        Me.InfoTextBox.Name = "InfoTextBox"
        Me.InfoTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.InfoTextBox.Size = New System.Drawing.Size(345, 333)
        Me.InfoTextBox.TabIndex = 0
        '
        'InfoButton
        '
        Me.InfoButton.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.InfoButton.Location = New System.Drawing.Point(282, 369)
        Me.InfoButton.Name = "InfoButton"
        Me.InfoButton.Size = New System.Drawing.Size(75, 23)
        Me.InfoButton.TabIndex = 1
        Me.InfoButton.Text = "OK"
        Me.InfoButton.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 348)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(187, 13)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "This info has been copied to clipboard"
        '
        'InfoForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSize = True
        Me.ClientSize = New System.Drawing.Size(369, 414)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.InfoButton)
        Me.Controls.Add(Me.InfoTextBox)
        Me.Name = "InfoForm"
        Me.Text = "FTA Setup"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents InfoTextBox As TextBox
    Friend WithEvents InfoButton As Button
    Friend WithEvents Label1 As Label
End Class
