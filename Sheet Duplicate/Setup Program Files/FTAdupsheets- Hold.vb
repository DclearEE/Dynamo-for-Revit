
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
Public Class FTAdupsheets
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

        'Dim M_activeDoc As Document = commandData.Application.ActiveUIDocument.Document


        'Dim uidoc As UIDocument = commandData.Application.ActiveUIDocument.Document
        Dim M_activeDoc As Document = commandData.Application.ActiveUIDocument.Document
        Dim TemplateVS As ViewSheet = TryCast(M_activeDoc.ActiveView, ViewSheet)

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

        Excel.Application.Worksheets("Duplicate Views").activate
        worksheet = workbook.ActiveSheet
        Dim TemplateShtNumber As String = Nothing
        Dim DupsheetNumber As String
        Dim SheetCat As String
        Dim SubDiscipline As String
        Dim ViewDis As String
        Dim FloorPlanName As String
        Dim row As Integer = 2

        'collect existing view names and lookup template
        Dim Viewcollector As New Autodesk.Revit.DB.FilteredElementCollector(M_activeDoc)
        Dim ViewList As IList(Of Autodesk.Revit.DB.Element) = Viewcollector.OfClass(GetType(Revit.DB.View)).ToElements  'WhereElementIsNotElementType().ToElements()
        Dim TemplateView As Autodesk.Revit.DB.View = Nothing
        Dim DupView As Autodesk.Revit.DB.View = Nothing
        Dim shtcreated As Integer = 0
        Dim shtskip As Integer = 0
        Dim para As Parameter

        'Collect all Floorplan & Ceiling Plan views and lookup template view
        Dim shtnames As New StringBuilder()
        'shtnames.Append("Sheet Names" & vbLf)

        Dim PlanNames As New StringBuilder()
        'PlanNames.Append("Plan Names" & vbLf)

        For Each Shtelement As Revit.DB.Element In ViewList
            TemplateView = DirectCast(Shtelement, Autodesk.Revit.DB.View)

            If TemplateView.ViewType = ViewType.FloorPlan Or TemplateView.ViewType = ViewType.CeilingPlan Then
                PlanNames.AppendFormat("{0}" & vbLf, TemplateView.Title)
            End If

            If TemplateView.ViewType = ViewType.DrawingSheet Then
                shtnames.AppendFormat("{0}" & vbLf, TemplateView.Title)
            End If

        Next
        'MessageBox.Show(PlanNames.ToString)
        'MessageBox.Show(shtnames.ToString)
        '=====================================

        'Exit Function


        Do
            'MessageBox.Show("[" & TempShtNumber & "]")


            TemplateShtNumber = worksheet.Cells(row, 1).Value.ToString
            DupsheetNumber = worksheet.Cells(row, 3).value.ToString
            FloorPlanName = worksheet.Cells(row, 4).value.ToString
            SheetCat = worksheet.Cells(row, 5).value.ToString
            ViewDis = SheetCat
            SubDiscipline = worksheet.Cells(row, 6).value.ToString

            TemplateVS = Nothing

            If shtnames.ToString.Contains(": " & TemplateShtNumber & " -") And PlanNames.ToString.Contains(": " & FloorPlanName) Then
                For Each Shtelement As Revit.DB.Element In ViewList

                    TemplateView = DirectCast(Shtelement, Autodesk.Revit.DB.View)

                    If TemplateView.Title.ToString.Contains(TemplateShtNumber) Then
                        TemplateVS = DirectCast(Shtelement, Autodesk.Revit.DB.ViewSheet)
                        Exit For
                    End If

                Next
                'MessageBox.Show("[" & TempVS.Name.ToString & "]")

                If shtnames.ToString.Contains(DupsheetNumber) Then
                    'MessageBox.Show(DupsheetNumber & " Already exists!")
                    shtskip += 1

                    'Exit Function

                Else

                    Dim titleblock As FamilyInstance = New FilteredElementCollector(M_activeDoc).OfClass(GetType(FamilyInstance)).OfCategory(BuiltInCategory.OST_TitleBlocks).Cast(Of FamilyInstance)().First(Function(q) q.OwnerViewId = TemplateVS.Id)

                    Dim newsheet As ViewSheet = ViewSheet.Create(M_activeDoc, titleblock.GetTypeId())
                    newsheet.SheetNumber = DupsheetNumber 'TempVS.SheetNumber + "-DUP"
                    newsheet.Name = FloorPlanName         'TempVS.Name
                    shtnames.AppendFormat("{0}" & vbLf, newsheet.Title)

                    'Paras = newsheet.Parameters
                    'Set the sheet category

                    For Each ViewParm As Parameter In newsheet.Parameters
                        If ViewParm.Definition.Name = "Sheet Category" Then ViewParm.Set(SheetCat)
                    Next

                    shtcreated += 1
                    ' all views but schedules
                    For Each eid As ElementId In TemplateVS.GetAllPlacedViews()
                        Dim ExistingView As Revit.DB.View = TryCast(M_activeDoc.GetElement(eid), Revit.DB.View)
                        'MessageBox.Show(ev.Name.ToString & ev.ViewType.ToString)

                        Dim newview As Revit.DB.View = Nothing

                        ' legends
                        If ExistingView.ViewType = ViewType.Legend Then
                            newview = ExistingView
                        ElseIf ExistingView.ViewType = ViewType.FloorPlan Then
                            ' all non-legend and non-schedule views
                            'Dim newviewid As ElementId = ev.Duplicate(ViewDuplicateOption.WithDetailing)
                            'newview = TryCast(M_activeDoc.GetElement(newviewid), Revit.DB.View)
                            'newview.Name = ev.Name + "-FLP"

                            For Each Shtelement As Revit.DB.Element In ViewList

                                DupView = DirectCast(Shtelement, Autodesk.Revit.DB.View)

                                If DupView.Name = FloorPlanName Then
                                    newview = Shtelement
                                    'TempVS = DirectCast(Shtelement, Autodesk.Revit.DB.ViewSheet)
                                    Exit For
                                End If

                            Next


                        ElseIf ExistingView.ViewType = ViewType.DraftingView Then
                            ' all non-legend and non-schedule views
                            Dim newviewid As ElementId = ExistingView.Duplicate(ViewDuplicateOption.WithDetailing)
                            newview = TryCast(M_activeDoc.GetElement(newviewid), Revit.DB.View)
                            'newview.ViewName = DupsheetNumber & " - " & DupSheetName
                            newview.Name = DupsheetNumber & " - " & FloorPlanName 'ev.Name + "-DV"

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

                        Else
                            ' all non-legend and non-schedule views
                            Dim newviewid As ElementId = ExistingView.Duplicate(ViewDuplicateOption.WithDetailing)
                            newview = TryCast(M_activeDoc.GetElement(newviewid), Revit.DB.View)
                            newview.Name = DupsheetNumber & " - " & FloorPlanName & "-???"
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

                        'if newsheet is nothing then skip creating viewports.  
                        For Each VwPort As Viewport In New FilteredElementCollector(M_activeDoc).OfClass(GetType(Viewport))
                            If VwPort.SheetId = TemplateVS.Id AndAlso VwPort.ViewId = ExistingView.Id Then
                                Dim vpbb As BoundingBoxXYZ = VwPort.BoundingBox(TemplateVS) 'get_BoundingBox(TempVS)
                                Dim initialCenter As XYZ = VwPort.GetBoxCenter
                                'MessageBox.Show("Line 314" & newsheet.Title)
                                Dim newvp As Viewport = Viewport.Create(M_activeDoc, newsheet.Id, newview.Id, XYZ.Zero)

                                Dim newvpbb As BoundingBoxXYZ = vpbb
                                'get_BoundingBox(newsheet)
                                Dim newCenter As XYZ = newvp.GetBoxCenter

                                ElementTransformUtils.MoveElement(M_activeDoc, newvp.Id, New XYZ(initialCenter.X - newCenter.X, initialCenter.Y - newCenter.Y, 0))

                            End If

                        Next
                    Next

                    ' schedules

                    For Each si As ScheduleSheetInstance In (New FilteredElementCollector(M_activeDoc).OfClass(GetType(ScheduleSheetInstance)))
                        If si.OwnerViewId = TemplateVS.Id Then
                            If Not si.IsTitleblockRevisionSchedule Then
                                For Each vsc As ViewSchedule In New FilteredElementCollector(M_activeDoc).OfClass(GetType(ViewSchedule))
                                    If si.ScheduleId = vsc.Id Then
                                        Dim sibb As BoundingBoxXYZ = si.BoundingBox(TemplateVS)
                                        Dim initialCenter As XYZ = (sibb.Max + sibb.Min) / 2

                                        Dim newssi As ScheduleSheetInstance = ScheduleSheetInstance.Create(M_activeDoc, newsheet.Id, vsc.Id, XYZ.Zero)

                                        Dim newsibb As BoundingBoxXYZ = newssi.BoundingBox(newsheet)
                                        Dim newCenter As XYZ = (newsibb.Max + newsibb.Min) / 2

                                        ElementTransformUtils.MoveElement(M_activeDoc, newssi.Id, New XYZ(initialCenter.X - newCenter.X, initialCenter.Y - newCenter.Y, 0))
                                    End If
                                Next

                            End If
                        End If
                    Next
                End If
            Else

                'MessageBox.Show("[" & TempShtNumber & "] - not found!")
                shtskip += 1

            End If
            'Exit Function

            row += 1
            'MsgBox("Created " & shtcreated.ToString & " Sheets(s)   Skipped " & shtskip, , "F&T Revit Cleanup")
        Loop Until (worksheet.Cells(row, 1).Value Is Nothing)
exithere:

        Excel.ActiveWorkbook.Close()
        Excel = Nothing
        'MessageBox.Show(shtnames.ToString)
        MsgBox("Created " & shtcreated.ToString & " Sheets(s)   Skipped " & shtskip, , "F&T Revit Cleanup")

        Return Autodesk.Revit.UI.Result.Succeeded

    End Function


End Class
