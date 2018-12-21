
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
Public Class SheetSetup

    ' All Autodesk Revit external commands must support this interface
    Implements Autodesk.Revit.UI.IExternalCommand
    Public m_activeDoc As Document

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

        Dim infoform As New InfoForm

        m_activeDoc = commandData.Application.ActiveUIDocument.Document


        'MessageBox.Show("[" & M_activeDoc.PathName.ToString & "]")
        'OPEN EXCEL WORKBOOK
        Dim m_inputFileName As String = m_activeDoc.Title
        Dim m_inputFolder As String = m_activeDoc.PathName
        m_inputFolder = Left(m_activeDoc.PathName, Len(m_activeDoc.PathName) - Len(m_inputFileName) - 4)

        m_inputFileName = "Worksets and View Duplication"
        Dim filename As String = m_inputFolder & m_inputFileName & ".xlsx"
        'MessageBox.Show("[" & filename & "]")
        'Exit Function


        Dim Excel As MsExcel.Application = New Microsoft.Office.Interop.Excel.Application
        Dim openfiledialog1 As New OpenFileDialog()
        openfiledialog1.InitialDirectory = m_inputFolder
        openfiledialog1.Title = "Open the Sheets to Duplication File"
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
        Dim worksheet As MsExcel.Worksheet = Nothing
        Excel.Visible = True
        worksheet = workbook.ActiveSheet



SheetDup:

        Using trans As New Transaction(m_activeDoc)
            trans.Start("F-tSetupSheets")
            m_activeDoc.Regenerate()
            'MsgBox("SheetDup")
            'Duplicate SHeets ================================================================================
            Dim TemplateVS As ViewSheet = Nothing   'TryCast(M_activeDoc.ActiveView, ViewSheet)
            Dim TemplateShtNumber As String = Nothing
            Dim DupsheetNumber As String
            Dim SheetCat As String
            Dim SheetIndex As Decimal
            Dim SubDiscipline As String
            Dim ViewDis As String
            Dim FloorPlanName As String
            Dim ExcelRow As Integer = 3

            'collect existing view names and lookup template
            Dim Viewcollector As New Autodesk.Revit.DB.FilteredElementCollector(m_activeDoc)
            Dim ViewList As IList(Of Autodesk.Revit.DB.Element) = Viewcollector.OfClass(GetType(Revit.DB.View)).ToElements  'WhereElementIsNotElementType().ToElements()
            Dim TemplateView As Autodesk.Revit.DB.View = Nothing

            Dim DupView As Autodesk.Revit.DB.View = Nothing
            Dim shtcreated As Integer = 0
            Dim shtskip As Integer = 0
            Dim PLanNames As New StringBuilder()
            Dim CreatedSheets As New StringBuilder()

            Dim shtnames As New StringBuilder()
            Dim plans As Integer = 0
            Dim Shts As Integer = 0
            Dim newview As Revit.DB.View
            Dim para As Parameter
            Dim workingview As Autodesk.Revit.DB.View = Nothing

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

            'MsgBox(shtnames.ToString)
            'MsgBox(PLanNames.ToString)
            'Exit Function


            Dim StartTime As DateTime = Now


            Do
                'MsgBox(ExcelRow)
                TemplateShtNumber = worksheet.Cells(ExcelRow, 1).Value.ToString
                DupsheetNumber = worksheet.Cells(ExcelRow, 3).value.ToString
                FloorPlanName = worksheet.Cells(ExcelRow, 4).value.ToString
                'SheetCat = worksheet.Cells(ExcelRow, 5).value.ToString
                SheetIndex = worksheet.Cells(ExcelRow, 5).value
                ViewDis = worksheet.Cells(ExcelRow, 6).value.ToString
                SubDiscipline = worksheet.Cells(ExcelRow, 6).value.ToString
                If TemplateShtNumber <> "NA" Then


                    'MessageBox.Show("[" & TemplateShtNumber & "]")

                    'MsgBox("[" & TemplateShtNumber & " -" & "]       [" & ": " & FloorPlanName & "]  [" & DupsheetNumber.ToString & "]")

                    TemplateVS = Nothing

                    If shtnames.ToString.Contains(": " & TemplateShtNumber & " -") Then 'And PLanNames.ToString.Contains(": " & FloorPlanName)
                        'MsgBox(shtnames.ToString)
                        'MsgBox(PLanNames.ToString)
                        'MsgBox("[: " & TemplateShtNumber & " -" & "]       [" & ": " & FloorPlanName & "]  [" & DupsheetNumber.ToString & "]")

                        For Each Shtelement As Revit.DB.Element In ViewList

                            'MsgBox("Pre-cast:" & Shtelement.Name.ToString) 'Shtelement.Name.ToString &
                            TemplateView = DirectCast(Shtelement, Autodesk.Revit.DB.View)

                            If TemplateView.Title.ToString.Contains(TemplateShtNumber) Then
                                'MsgBox("Directcast:" & Shtelement.Name.ToString) '& Shtelement.Category.ToString)
                                TemplateVS = DirectCast(Shtelement, Autodesk.Revit.DB.ViewSheet)
                                Exit For
                            End If

                        Next

                        If DupsheetNumber = "NA" Then
                            'MessageBox.Show("DupSheetNumber" & DupsheetNumber & " = NA.  Skipped")
                            'SkippedSheets.AppendFormat("{0}" & vbCrLf, DupsheetNumber & " - " & FloorPlanName)
                            shtskip += 1

                        ElseIf shtnames.ToString.Contains(DupsheetNumber) Then
                            'MessageBox.Show(DupsheetNumber & " Already exists!")
                            'SkippedSheets.AppendFormat("{0}" & vbCrLf, DupsheetNumber & " - " & FloorPlanName)

                            shtskip += 1

                            'Exit Function

                        Else

                            Dim titleblock As FamilyInstance = New FilteredElementCollector(m_activeDoc).OfClass(GetType(FamilyInstance)).OfCategory(BuiltInCategory.OST_TitleBlocks).Cast(Of FamilyInstance)().First(Function(q) q.OwnerViewId = TemplateVS.Id)

                            Dim newsheet As ViewSheet = ViewSheet.Create(m_activeDoc, titleblock.GetTypeId())
                            newsheet.SheetNumber = DupsheetNumber 'TempVS.SheetNumber + "-DUP"
                            newsheet.Name = FloorPlanName         'TempVS.Name
                            shtnames.AppendFormat("{0}" & vbCrLf, newsheet.Title)
                            'MessageBox.Show(shtnames.ToString)
                            'Paras = newsheet.Parameters
                            'Set the sheet category

                            For Each ViewParm As Parameter In newsheet.Parameters
                                If ViewParm.Definition.Name = "Sheet Index" Then ViewParm.Set(SheetIndex)
                            Next

                            CreatedSheets.AppendFormat("{0}" & vbCrLf, DupsheetNumber & " - " & FloorPlanName)
                            shtcreated += 1

                            ' all views but schedules
                            For Each eid As ElementId In TemplateVS.GetAllPlacedViews()
                                Dim ExistingView As Revit.DB.View = TryCast(m_activeDoc.GetElement(eid), Revit.DB.View)
                                'MessageBox.Show("Existing Template view name:" & ExistingView.Name.ToString & "Existing Template View Type:" & ExistingView.ViewType.ToString)

                                'Dim newview As Revit.DB.View = Nothing
                                newview = Nothing

                                'create legends
                                If ExistingView.ViewType = ViewType.Legend Then
                                    newview = ExistingView

                                    'create floorplan
                                ElseIf ExistingView.ViewType = ViewType.FloorPlan Then
                                    'MessageBox.Show("Create Floor Plan")
                                    ' all non-legend and non-schedule views
                                    'Dim newviewid As ElementId = ev.Duplicate(ViewDuplicateOption.WithDetailing)
                                    'newview = TryCast(M_activeDoc.GetElement(newviewid), Revit.DB.View)
                                    'newview.Name = ev.Name + "-FLP"

                                    For Each Shtelement As Revit.DB.Element In ViewList

                                        DupView = DirectCast(Shtelement, Autodesk.Revit.DB.View)
                                        'MsgBox("Dupview:" & DupView.Name.ToString)
                                        If DupView.Name = FloorPlanName Then
                                            'MsgBox("dupview.name = floorplanname")
                                            newview = Shtelement
                                            'TempVS = DirectCast(Shtelement, Autodesk.Revit.DB.ViewSheet)
                                            Exit For
                                        End If

                                    Next
                                    'If newview IsNot Nothing Then
                                    '    'MsgBox("newview.name:" & newview.Name.ToString & "  Newsheet:" & newsheet.Name.ToString)
                                    'Else
                                    '    MsgBox("Newview is nothing!")
                                    '    Return Autodesk.Revit.UI.Result.Succeeded

                                    'End If

                                ElseIf ExistingView.ViewType = ViewType.DraftingView Then

                                    ' all non-legend and non-schedule views
                                    Dim newviewid As ElementId = ExistingView.Duplicate(ViewDuplicateOption.WithDetailing)
                                    newview = TryCast(m_activeDoc.GetElement(newviewid), Revit.DB.View)
                                    'newview.ViewName = DupsheetNumber & " - " & DupSheetName
                                    Try
                                        newview.Name = DupsheetNumber & " - " & FloorPlanName & "- " & ExistingView.Name.ToString
                                        'MsgBox(newview.GetType.Name.ToString & " " & ExistingView.GetType.Name.ToString)

                                    Catch ex As Exception
                                        MsgBox("Error Creating:" & DupsheetNumber & " - " & FloorPlanName & "- " & ExistingView.Name.ToString & "  - program will continue.", , "F-T Setup Sheets")

                                        newview = Nothing
                                        'trans.Commit()
                                        'Return Autodesk.Revit.UI.Result.Succeeded

                                    End Try

                                    If newview IsNot Nothing Then



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
                                            If ViewParm.Definition.Name = "Sub-Discipline" Then ViewParm.Set(SubDiscipline)
                                        Next
                                    End If


                                Else
                                    ' all non-legend and non-schedule views
                                    Dim newviewid As ElementId = ExistingView.Duplicate(ViewDuplicateOption.WithDetailing)
                                    Try
                                        newview = TryCast(m_activeDoc.GetElement(newviewid), Revit.DB.View)

                                        newview.Name = DupsheetNumber & " - " & FloorPlanName & "-XXX"
                                    Catch ex As Exception

                                        MsgBox("Error Creating:" & DupsheetNumber & " - " & FloorPlanName & "-XXX       " & ExistingView.Name.ToString & "  - program will continue.", , "F-T Setup Sheets")

                                        newview = Nothing
                                    End Try

                                    If newview IsNot Nothing Then
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
                                            If ViewParm.Definition.Name = "Sub-Discipline" Then ViewParm.Set(SubDiscipline)
                                        Next
                                    End If

                                End If

                                'if newsheet is nothing then skip creating viewports. 
                                'MessageBox.Show("  newsheet.title:" & newsheet.Title & "  newsheet.id" & newsheet.Id.ToString & "newview:" & newview.Name.ToString)


                                'MessageBox.Show("  newsheet.title:" & newsheet.Title & "  newsheet.id" & newsheet.Id.ToString)
                                If newview IsNot Nothing Then
                                    'Create the viewports
                                    For Each VwPort As Viewport In New FilteredElementCollector(m_activeDoc).OfClass(GetType(Viewport))

                                        If VwPort.SheetId = TemplateVS.Id AndAlso VwPort.ViewId = ExistingView.Id Then

                                            'viewPortNew.Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).Set(viewPortTemplate.Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsElementId)

                                            'MsgBox(VwPort.Name.ToString & "==" & ExistingView.Name)
                                            Dim vpbb As BoundingBoxXYZ = VwPort.BoundingBox(TemplateVS) 'get_BoundingBox(TempVS)
                                            Dim initialCenter As XYZ = VwPort.GetBoxCenter

                                            'MessageBox.Show("VwPort.name:" & VwPort.Name.ToString & "  newsheet.title:" & newsheet.Title & "  newsheet.id" & newsheet.Id.ToString & "  newview.name:" & newview.Name.ToString & "  newview.id:" & newview.Id.ToString)

                                            Dim newvp As Viewport = Viewport.Create(m_activeDoc, newsheet.Id, newview.Id, XYZ.Zero)

                                            Dim newvpbb As BoundingBoxXYZ = vpbb
                                            'get_BoundingBox(newsheet)
                                            Dim newCenter As XYZ = newvp.GetBoxCenter

                                            ElementTransformUtils.MoveElement(m_activeDoc, newvp.Id, New XYZ(initialCenter.X - newCenter.X, initialCenter.Y - newCenter.Y, 0))

                                            'set the Viewport Type to match Original Template
                                            newvp.Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).Set(VwPort.Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsElementId)
                                            'set the scopebox to match original template
                                            'newvp.Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(VwPort.Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).AsElementId)
                                            'Try
                                            '    MsgBox(VwPort.Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Element.Name.ToString)
                                            '    '& VwPort.Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Definition.ToString)
                                            '    '& "  -  " & ExistingView.Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Element.ToString)

                                            'Catch ex As Exception

                                            'End Try
                                        End If

                                    Next
                                End If

                                'trans.Commit()

                            Next

                            'create schedules
                            For Each si As ScheduleSheetInstance In (New FilteredElementCollector(m_activeDoc).OfClass(GetType(ScheduleSheetInstance)))
                                If si.OwnerViewId = TemplateVS.Id Then
                                    If Not si.IsTitleblockRevisionSchedule Then
                                        For Each vsc As ViewSchedule In New FilteredElementCollector(m_activeDoc).OfClass(GetType(ViewSchedule))
                                            If si.ScheduleId = vsc.Id Then
                                                Dim sibb As BoundingBoxXYZ = si.BoundingBox(TemplateVS)
                                                Dim initialCenter As XYZ = (sibb.Max + sibb.Min) / 2

                                                Dim newssi As ScheduleSheetInstance = ScheduleSheetInstance.Create(m_activeDoc, newsheet.Id, vsc.Id, XYZ.Zero)

                                                Dim newsibb As BoundingBoxXYZ = newssi.BoundingBox(newsheet)
                                                Dim newCenter As XYZ = (newsibb.Max + newsibb.Min) / 2

                                                ElementTransformUtils.MoveElement(m_activeDoc, newssi.Id, New XYZ(initialCenter.X - newCenter.X, initialCenter.Y - newCenter.Y, 0))
                                            End If
                                        Next

                                    End If
                                End If
                            Next
                        End If
                    Else

                        MessageBox.Show("[" & TemplateShtNumber & "] - not found!")
                        'SkippedSheets.AppendFormat("{0}" & vbCrLf, DupsheetNumber & " - " & FloorPlanName)

                        shtskip += 1

                    End If
                    'Exit Function

                Else
                    'SkippedSheets.AppendFormat("{0}" & vbCrLf, DupsheetNumber & " - " & FloorPlanName)
                    shtskip += 1
                End If

                ExcelRow += 1
                'MsgBox("Created " & shtcreated.ToString & " Sheets(s)   Skipped " & shtskip, , "F&T Revit Cleanup")

            Loop Until (worksheet.Cells(ExcelRow, 1).Value Is Nothing)

            'exithere2:
            trans.Commit()
            Excel.ActiveWorkbook.Close()
            Excel = Nothing

            'MsgBox(viewnames.ToString(), , "F&T Revit Cleanup")
            Dim Elapsed As TimeSpan = Now - StartTime
            infoform.Text = "F-T Setup Sheets"
            infoform.InfoTextBox.Text = "Created " & shtcreated.ToString & " Sheets(s)   Skipped " & shtskip & "   Elapsed Time:" & Elapsed.ToString & vbCrLf & vbCrLf & "Sheets Created:" & vbCrLf & CreatedSheets.ToString & vbCrLf
            infoform.ShowDialog()
            Clipboard.SetText(infoform.InfoTextBox.Text, TextDataFormat.Text)

            'MsgBox("Created " & shtcreated.ToString & " Sheets(s)   Skipped " & shtskip & "   Elapsed Time:" & Elapsed.ToString & vbcrlf & vbcrlf & "Views Skipped:" & vbcrlf & SkippedSheets.ToString & vbcrlf, , "F-T Setup Sheets")

        End Using
        Return Autodesk.Revit.UI.Result.Succeeded

    End Function



End Class


