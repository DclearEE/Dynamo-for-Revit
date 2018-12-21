
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

<Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)>
<Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)>
<Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)>
Public Class Dependantsetup

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

        Dim InfoForm As New InfoForm

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
        openfiledialog1.Title = "Open Dependant View Duplication File"
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





        Dim ExcelRow As Integer = 2
        Dim Wrkset As String = Nothing
        Dim wrkskip As Integer = 0
        'Dim workst As Workset
        Dim worksetTable As WorksetTable = M_activeDoc.GetWorksetTable()
        Dim View2Dup As String = Nothing
        Dim DupViewName As String = Nothing
        Dim ViewDis As String = Nothing
        Dim ViewSubDis As String = Nothing
        Dim ViewDependance As String = Nothing
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
        Dim TemplateView As Autodesk.Revit.DB.View = Nothing
        Dim TView As String = Nothing
        Dim TviewScopeBox As ElementId = Nothing
        Dim tviewview As Revit.DB.View = Nothing
        Dim PLanNames As New StringBuilder()
        Dim ViewsCreated As New StringBuilder()

        Dim shtnames As New StringBuilder()
        'shtnames.Append("Sheet Names" & vbcrlf)

        'Dim PlanNames As New StringBuilder()
        'PlanNames.Append("Plan Names" & vbcrlf)
        Dim plans As Integer = 0
        Dim Shts As Integer = 0

        ''Look up and list scope boxes
        'Dim Scopecollector As New Autodesk.Revit.DB.FilteredElementCollector(M_activeDoc)
        'Dim ScopeList As IList(Of Autodesk.Revit.DB.Element) = Scopecollector.OfCategory(BuiltInCategory.OST_VolumeOfInterest).ToElements()

        'For Each Elem As Element In ScopeList
        '    MsgBox(Elem.Name.ToString & "-" & Elem.Id.ToString)

        'Next


        Using trans As New Transaction(M_activeDoc)

            'MsgBox("DuplicateViews")
            trans.Start("F-tDependantViews")


            'DUPLICATE VIEWS-----------------------------------------------------------------------------
            Excel.Application.Worksheets("Duplicate Views").activate
            worksheet = workbook.ActiveSheet


            'collect existing view names
            For Each Shtelement As Revit.DB.Element In ViewList
                TemplateView = DirectCast(Shtelement, Autodesk.Revit.DB.View)

                If TemplateView.ViewType = ViewType.FloorPlan Or TemplateView.ViewType = ViewType.CeilingPlan Then
                    PLanNames.AppendFormat("{0}" & vbCrLf, TemplateView.Title)
                    plans += 1
                End If

                If TemplateView.ViewType = ViewType.DrawingSheet Then
                    shtnames.AppendFormat("{0}" & vbCrLf, TemplateView.Title)
                    Shts += 1
                End If

            Next

            ExcelRow = 3

            Do

                View2Dup = worksheet.Cells(ExcelRow, 2).Value
                TView = worksheet.Cells(ExcelRow, 2).Value
                DupViewName = worksheet.Cells(ExcelRow, 4).Value
                ViewDis = worksheet.Cells(ExcelRow, 5).Value
                ViewSubDis = worksheet.Cells(ExcelRow, 6).Value
                ViewDependance = worksheet.Cells(ExcelRow, 7).Value
                If ViewDependance = "" Then
                    ViewDependance = "N"
                    'MsgBox("ViewDendance=" & ViewDependance & "   DupViewName=" & DupViewName & " TView=" & TView)
                End If
                'MsgBox(ExcelRow.ToString)
                If View2Dup IsNot Nothing And ViewDependance.ToUpper <> "N" Then

                    View2Dup = ViewDependance
                    'MsgBox("----View2Dup:" & View2Dup.ToString & " - " & DupViewName.ToString & "-" & ViewDependance.ToString & "-" & ExcelRow.ToString)

                    If PLanNames.ToString.Contains(DupViewName & vbCrLf) Then
                        'MsgBox("View: " & DupViewName & " already exists", , "F&T Setup Worksets/Views")
                        vskip += 1
                        'SkippedViews.AppendFormat("{0}" & vbCrLf, DupViewName)
                    Else
                        If PLanNames.ToString.Contains(View2Dup & vbCrLf) Then
                            'MsgBox(DupViewName.ToString)
                            For Each Viewelement As Revit.DB.Element In ViewList
                                workingview = DirectCast(Viewelement, Autodesk.Revit.DB.View)


                                If workingview.Name = View2Dup Then
                                    'para = workingview.Parameter(BuiltInParameter.VIEW_DISCIPLINE)
                                    'MsgBox("View2Dup Scope Box:" & workingview.Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).AsValueString)

                                    'MessageBox.Show(para.Definition.Name & "=" & para.AsValueString)

                                    View2DupID = workingview.Id
                                    Try
                                        NewId = workingview.Duplicate(ViewDuplicateOption.AsDependent)
                                        'MessageBox.Show(View2Dup & " was duplicated")

                                    Catch ex As Exception
                                        NewId = Nothing
                                        MsgBox("Try NewId = workingview.Duplicate(ViewDuplicateOption.AsDependent)" & ex.ToString)
                                        Return Autodesk.Revit.UI.Result.Succeeded

                                    End Try

                                End If
                                If workingview.Name = TView Then
                                    'MsgBox("TView Scope Box:" & workingview.Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).AsValueString)
                                    TviewScopeBox = workingview.Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).AsElementId
                                    tviewview = workingview

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
                                    'para.Set(4095)
                                End If

                                For Each ViewParm As Parameter In newview.Parameters
                                    If ViewParm.Definition.Name = "Sub-Discipline" Then ViewParm.Set(ViewSubDis)
                                Next


                                'MsgBox("newview:" & newview.ViewName.ToString & "   tview:" & TView & "  Tviewscopebox" & TviewScopeBox.ToString & "  tviewview.scale:" & tviewview.Scale.ToString)
                                'Set newview parameters = tviewview  parameters
                                newview.Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(TviewScopeBox)
                                newview.Scale = tviewview.Scale
                                newview.CropBoxVisible = tviewview.CropBoxVisible
                                newview.CropBoxActive = tviewview.CropBoxActive
                                newview.CropBox = tviewview.CropBox


                                'para = newview.Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP)
                                'para.SetValueString(TView)

                                'MessageBox.Show(para.Definition.Name & "=" & para.AsValueString)
                                'MessageBox.Show("Copied: " & View2Dup & " and Named it: " & DupViewName)
                                PLanNames.AppendFormat("{0}" & vbCrLf, "Floor Plan: " & DupViewName)
                                ViewsCreated.AppendFormat("{0}" & vbCrLf, DupViewName)
                                vcreated += 1
                                'MsgBox("view created")
                            End If

                        Else

                            'MsgBox("Template View:" & View2Dup & " was not found!" & vbCrLf & "View: " & DupViewName & " could not be created", , "F&T Setup worksets/views")
                            'SkippedViews.AppendFormat("{0}" & vbCrLf, DupViewName & "- Template Not Found!")
                            vskip += 1
                        End If
                    End If
                Else
                    'SkippedViews.AppendFormat("{0}" & vbCrLf, DupViewName)
                    vskip += 1
                End If


                ExcelRow += 1

            Loop Until (worksheet.Cells(ExcelRow, 1).Value Is Nothing)

            trans.Commit()

        End Using

        Excel.ActiveWorkbook.Close()
        Excel = Nothing

        'MsgBox(viewnames.ToString(), , "F&T Revit Cleanup")
        InfoForm.Text = "F-T Setup Dependant Views"
        InfoForm.InfoTextBox.Text = "Created " & vcreated.ToString & " View(s)   Skipped " & vskip.ToString & vbCrLf & vbCrLf & vbCrLf & "Views Created:" & vbCrLf & ViewsCreated.ToString & vbCrLf
        InfoForm.ShowDialog()
        Clipboard.SetText(InfoForm.InfoTextBox.Text, TextDataFormat.Text)

        'MsgBox("Created " & vcreated.ToString & " View(s)   Skipped " & vskip.ToString & vbcrlf & "Created " & wscreated & " Workset(s)  Skipped " & wrkskip.ToString & vbcrlf & vbcrlf & "Views Skipped:" & vbcrlf & SkippedViews.ToString & vbcrlf, , "F&T Setup Worksets/Views")


        Return Autodesk.Revit.UI.Result.Succeeded

    End Function

End Class


