
'
' (C) Copyright 2003-2010 by Autodesk, Inc.
'
' Permission to use, copy, modify, and distribute this software in
' object code form for any purpose and without fee is hereby granted,
' provided that the above copyright notice appears in all copies and
' that both that copyright notice and the limited warranty and
' restricted rights notice below appear in all supporting
' documentation.
'
' AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
' AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
' MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
' DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
' UNINTERRUPTED OR ERROR FREE.
'
' Use, duplication, or disclosure by the U.S. Government is subject to
' restrictions set forth in FAR 52.227-19 (Commercial Computer
' Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
' (Rights in Technical Data and Computer Software), as applicable.
'
Imports MsExcel = Microsoft.Office.Interop.Excel
Imports Autodesk
Imports Autodesk.Revit.DB
Imports Autodesk.Revit.UI
Imports Autodesk.Revit.ApplicationServices
Imports System.Windows.Forms
Imports System.Text

<Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Automatic)>
<Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)>
<Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)>
Public Class FTAsetup

    ' All Autodesk Revit external commands must support this interface
    Implements Autodesk.Revit.UI.IExternalCommand

    ''' <summary>
    ''' Implement this method as an external command for Revit.
    ''' </summary>
    ''' <param name="commandData">An object that is passed to the external application
    ''' which contains data related to the command,
    ''' such as the application object and active view.</param>
    ''' <param name="message">A message that can be set by the external application
    ''' which will be displayed if a failure or cancellation is returned by
    ''' the external command.</param>
    ''' <param name="elements">A set of elements to which the external application
    ''' can add elements that are to be highlighted in case of failure or cancellation.</param>
    ''' <returns>Return the status of the external command.
    ''' A result of Succeeded means that the API external method functioned as expected.
    ''' Cancelled can be used to signify that the user cancelled the external operation 
    ''' at some point. Failure should be returned if the application is unable to proceed with
    ''' the operation.</returns>
    Public Function Execute(ByVal commandData As Autodesk.Revit.UI.ExternalCommandData,
        ByRef message As String, ByVal elements As Autodesk.Revit.DB.ElementSet) _
        As Autodesk.Revit.UI.Result Implements Autodesk.Revit.UI.IExternalCommand.Execute

        Dim app As Revit.ApplicationServices.Application
        app = commandData.Application.Application

        Dim M_activeDoc As Document = commandData.Application.ActiveUIDocument.Document


        'MessageBox.Show("[" & M_activeDoc.PathName.ToString & "]")
        'OPEN EXCEL WORKBOOK
        Dim m_inputFileName As String = M_activeDoc.Title
        Dim m_inputFolder As String = M_activeDoc.PathName
        m_inputFolder = Left(M_activeDoc.PathName, Len(M_activeDoc.PathName) - Len(m_inputFileName) - 4)

        m_inputFileName = "F&T - Worksets and View Duplication"
        Dim filename As String = m_inputFolder & m_inputFileName & ".xlsx"
        'MessageBox.Show("[" & filename & "]")
        'Exit Function


        Dim Excel As MsExcel.Application = New Microsoft.Office.Interop.Excel.Application
        Dim openfiledialog1 As New OpenFileDialog()
        openfiledialog1.InitialDirectory = m_inputFolder
        openfiledialog1.Title = "Open the Worksets and View Duplication File"
        openfiledialog1.Filter = "Excel (*.xlsx)|*.xlsx|All files (*.*)|*.*"
        openfiledialog1.FileName = filename
        'openfiledialog1.FileName = mExcelFilename

        If openfiledialog1.ShowDialog = System.Windows.Forms.DialogResult.OK Then
            filename = openfiledialog1.FileName
        Else
            Exit Function
        End If

        If (Excel Is Nothing) Then
            message = "Failed to launch excel"
            Return Autodesk.Revit.UI.Result.Failed
        End If

        Excel.Visible = True

        Dim workbook As MsExcel.Workbook = Excel.Workbooks.Open(filename)
        Dim worksheet As MsExcel.Worksheet


        'CREATE WORKSETS----------------------------------------------------------------------------
        Excel.Application.Worksheets("Worksets").activate
        worksheet = workbook.ActiveSheet

        'Collect Existing Worksets
        Dim coll As New FilteredWorksetCollector(M_activeDoc)
        Dim worksetnames As New StringBuilder()
        Dim wscreated As Integer

        coll.OfKind(WorksetKind.UserWorkset)

        For Each workset As Workset In coll
            '    'worksetNames.AppendFormat("{0}: {1}" & vbLf, workset.Name, workset.Kind)
            worksetnames.AppendFormat("{0}" & vbLf, workset.Name)
        Next
        'MsgBox(worksetnames.ToString(), , "F&T Revit Cleanup")

        Dim row As Integer = 2
        Dim Wrkset As String = Nothing
        Dim wrkskip As Integer = 0
        Dim workst As Workset
        Dim worksetTable As WorksetTable = M_activeDoc.GetWorksetTable()

        If M_activeDoc.IsWorkshared = False Then
            MsgBox("Worksharing is not enabled!  Please go to the Collaborate menu and select worksets.  Set [Move Levels and Grids to Workset:] to FTA Setup.  This will take a few minutes.  Then re-run this Add-In", , "F&T Revit Cleanup")
            'MsgBox("Worksharing is being enabled!  Thia may take up to 5 minutes!")

            'M_activeDoc.EnableWorksharing("FTA Setup", "Workset1")
            Exit Function
        End If

        'MsgBox("Reading Workset and View Definitions from " & filename, , "F&T Revit Cleanup")


        Wrkset = worksheet.Cells(row, 1).Value

        Do

            If worksetnames.ToString.Contains(Wrkset & vbLf) Then
                wrkskip += 1

            Else
                workst = Workset.Create(M_activeDoc, Wrkset)
                worksetnames.AppendFormat("{0}" & vbLf, Wrkset)
                wscreated += 1
            End If

            row += 1
            Wrkset = worksheet.Cells(row, 1).Value
            'MsgBox("Workset:" & Wrkset, , "F&T Revit Cleanup")
        Loop Until (Wrkset Is Nothing)


        'DUPLICATE VIEWS-----------------------------------------------------------------------------
        Dim View2Dup As String = Nothing
        Dim DupViewName As String = Nothing
        Dim ViewDis As String = Nothing
        Dim ViewSubDis As String = Nothing
        Dim View2DupView As Revit.DB.View = Nothing
        Dim View2DupID As ElementId
        Dim NewId As ElementId = Nothing
        Dim NewElement As Element
        Dim newview As Revit.DB.View
        Dim Viewcollector As New Autodesk.Revit.DB.FilteredElementCollector(M_activeDoc)
        Dim ViewList As IList(Of Autodesk.Revit.DB.Element) = Viewcollector.OfClass(GetType(Revit.DB.View)).ToElements  'WhereElementIsNotElementType().ToElements()
        Dim workingview As Autodesk.Revit.DB.View = Nothing
        Dim vcreated As Integer
        Dim vskip As Integer
        Dim para As Parameter


        'collect existing view names
        Dim PLanNames As New StringBuilder()
        For Each Viewelement As Revit.DB.Element In ViewList
            workingview = DirectCast(Viewelement, Autodesk.Revit.DB.View)
            If workingview.ViewType = ViewType.FloorPlan Or workingview.ViewType = ViewType.CeilingPlan Then
                PLanNames.AppendFormat("{0}" & vbLf, workingview.Name)
            End If
        Next

        'MessageBox.Show(viewnames.ToString)
        'Exit Function



        Excel.Application.Worksheets("Duplicate Views").activate
        worksheet = workbook.ActiveSheet

        row = 2
        'MsgBox("Reading View Definitions from " & filename, , "F&T Revit Cleanup")
        Do
            'MessageBox.Show("Debug1")
            View2Dup = worksheet.Cells(row, 2).Value
            DupViewName = worksheet.Cells(row, 4).Value
            ViewDis = worksheet.Cells(row, 5).Value
            ViewSubDis = worksheet.Cells(row, 6).Value

            If View2Dup IsNot Nothing Then

                If PLanNames.ToString.Contains(DupViewName & vbLf) Then
                    'MsgBox("View: " & DupViewName & " already exists", , "F&T Cleanup")
                    vskip += 1

                Else
                    If PLanNames.ToString.Contains(View2Dup & vbLf) Then

                        For Each Viewelement As Revit.DB.Element In ViewList
                            workingview = DirectCast(Viewelement, Autodesk.Revit.DB.View)


                            If workingview.Name = View2Dup Then
                                'para = workingview.Parameter(BuiltInParameter.VIEW_DISCIPLINE)

                                'MessageBox.Show(para.Definition.Name & "=" & para.AsValueString)

                                View2DupID = workingview.Id
                                Try
                                    NewId = workingview.Duplicate(ViewDuplicateOption.WithDetailing)
                                    'MessageBox.Show(View2Dup & " was duplicated")

                                Catch ex As Exception
                                    NewId = Nothing
                                    Return Autodesk.Revit.UI.Result.Succeeded

                                End Try

                            End If

                        Next

                        If NewId IsNot Nothing Then
                            NewElement = M_activeDoc.GetElement(NewId)
                            'MessageBox.Show(NewElement.Name)
                            NewElement.Name = DupViewName
                            newview = DirectCast(NewElement, Autodesk.Revit.DB.View)
                            para = newview.Parameter(BuiltInParameter.VIEW_DISCIPLINE)
                            If ViewDis = "Architectural" Then
                                para.Set(1)
                            ElseIf ViewDis = "Structural" Then
                                para.Set(2)
                            ElseIf ViewDis = "Mechanical" Then
                                para.Set(4)
                            ElseIf ViewDis = "Electrical" Then
                                para.Set(8)
                            Else 'ViewDis = "Coordination" Then
                                para.Set(4095)
                            End If

                            For Each ViewParm As Parameter In newview.Parameters
                                If ViewParm.Definition.Name = "Sub-Discipline" Then ViewParm.Set(ViewSubDis)
                            Next
                            'MessageBox.Show(para.Definition.Name & "=" & para.AsValueString)
                            'MessageBox.Show("Copied: " & View2Dup & " and Named it: " & DupViewName)
                            PLanNames.AppendFormat("{0}" & vbLf, DupViewName)
                            vcreated += 1

                        End If


                    Else

                        MsgBox("Template View:" & View2Dup & " was not found!" & vbLf & "View: " & DupViewName & " could not be created", , "F&T Cleanup")
                        vskip += 1
                    End If
                End If
            End If

            'Try to delete the elemenet
            'If workingview IsNot Nothing And View2Dup <> workingview.Name Then
            '    workingview.DeleteEntity()
            'End If
            row += 1

        Loop Until (View2Dup Is Nothing)
exithere:

        Excel.ActiveWorkbook.Close()
        Excel = Nothing
        row -= 3
        'MsgBox(viewnames.ToString(), , "F&T Revit Cleanup")

        MsgBox("Created " & vcreated.ToString & " View(s)   Skipped " & vskip.ToString & vbLf & "Created " & wscreated & " Workset(s)  Skipped " & wrkskip.ToString, , "F&T Revit Cleanup")
        Return Autodesk.Revit.UI.Result.Succeeded

    End Function
End Class


