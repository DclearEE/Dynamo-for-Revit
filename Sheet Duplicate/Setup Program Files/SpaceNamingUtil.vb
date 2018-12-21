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

Imports System.Collections.Generic
Imports System.Linq
Imports System.Math
Imports System.Text
Imports System.Windows
Imports System.Windows.Forms
'Imports AutoCoolUtil.VB.NET
Imports Autodesk.Revit
Imports Autodesk.Revit.ApplicationServices
Imports Autodesk.Revit.DB
Imports Autodesk.Revit.DB.Mechanical.Space
Imports Autodesk.Revit.DB.Structure
Imports Autodesk.Revit.UI
Imports Autodesk.Revit.DB.Architecture.Room
Imports Autodesk.Revit.UI.Selection
Imports Creation = Autodesk.Revit.Creation
Imports DialogResult = System.Windows.Forms.DialogResult
Imports Exceptions = Autodesk.Revit.Exceptions
Imports Reference = Autodesk.Revit.DB.Reference
Imports Autodesk.Revit.DB.Architecture

<Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)>
<Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)>
<Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)>
Public Class SpaceNamingUtil
    ' All Autodesk Revit external commands must support this interface
    Implements Autodesk.Revit.UI.IExternalCommand
    Public MepDocument As Document
    Public MepApplication As UIApplication
    Public Property linkInstance As RevitLinkInstance
    Public AcMessage As String
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
    Public Function Execute(ByVal commandData As ExternalCommandData, ByRef message As String, ByVal elements As Autodesk.Revit.DB.ElementSet) As Autodesk.Revit.UI.Result Implements Autodesk.Revit.UI.IExternalCommand.Execute

        MepApplication = commandData.Application 'UIapplication
        MepDocument = MepApplication.ActiveUIDocument.Document
        'MsgBox(MepDocument.IsLinked.ToString)
        'ExceedsMaxFlow = False
        Dim MepUIdoc As UIDocument = commandData.Application.ActiveUIDocument
        Dim ArchDocument As Document = Nothing
        Dim MEPspaces As New FilteredElementCollector(MepDocument)
        Dim LinkElems As IList(Of Element) = MEPspaces.OfCategory(BuiltInCategory.OST_RvtLinks).OfClass(GetType(RevitLinkType)).ToElements()
        Dim LinkedDoc As Document = Nothing
        Dim ArchRooms As IList(Of Room) = Nothing
        Dim debug As Boolean = False
        For Each LinkElement As Element In LinkElems
            Dim linkType As RevitLinkType = TryCast(LinkElement, RevitLinkType)

            For Each LinkedDoc In MepApplication.Application.Documents
                'MsgBox(LinkedDoc.Title)
                If LinkedDoc.Title.Equals(linkType.Name) Then
                    ArchDocument = LinkedDoc
                    ArchRooms = New FilteredElementCollector(LinkedDoc).OfClass(GetType(SpatialElement)).OfCategory(BuiltInCategory.OST_Rooms).Cast(Of Room)().Where(Function(q) q.Area > 3).ToList()
                    'Exit For
                End If
            Next LinkedDoc
            If ArchRooms.Count > 0 Then
                'MsgBox(ArchRooms.Count.ToString)
                Exit For
            End If
        Next LinkElement
        'MsgBox(ArchRooms.Count.ToString)
        'ArchRooms = New FilteredElementCollector(LinkedDoc).OfClass(GetType(SpatialElement)).OfCategory(BuiltInCategory.OST_Rooms).Cast(Of Room)().Where(Function(q) q.Area > 3).ToList()

        'AcMessage += "ArchDocument:" & LinkedDoc.PathName & vbCrLf

        'MsgBox("Updating SpaceNames with Architect RoomNames")
        Using trans As New Transaction(MepUIdoc.Document)
            trans.Start("SpaceNames")
            Dim filter As Autodesk.Revit.DB.Mechanical.SpaceFilter
            filter = New Autodesk.Revit.DB.Mechanical.SpaceFilter()
            Dim ACspace As Autodesk.Revit.DB.Mechanical.Space

            'Dim collector As Autodesk.Revit.DB.FilteredElementCollector
            MEPspaces = New Autodesk.Revit.DB.FilteredElementCollector(commandData.Application.ActiveUIDocument.Document)
            MEPspaces.WherePasses(filter).ToElements()

            AcMessage += vbCrLf & "# of MEP spaces: " & MEPspaces.Count.ToString & vbLf & "# of Architectural Rooms: " & ArchRooms.Count.ToString & vbCrLf & vbCrLf
            'MsgBox("Reading View Definitions from " & filename, , "F&T Revit Cleanup")

            Dim td As New TaskDialog("TaskDialog Demonstration by Spiderinnet")

            td.Id = "F-T Space Naming Utility"
            td.MainIcon = TaskDialogIcon.TaskDialogIconWarning

            td.Title = "F-T Space Naming Utility"
            td.TitleAutoPrefix = True
            td.AllowCancellation = True

            ' Message 
            td.MainInstruction = "Updating the current Revit view MEP space names/no's to match the Architectural room names/no's."
            td.MainContent = AcMessage

            'td.FooterText = ""
            td.ExpandedContent = "When creating a central file with a linked architectural file the room names and number do not automatically populate the space names and numbers." & ControlChars.Lf

            'VerificationText 
            'td.VerificationText = "This is 'VerificationText'."

            ' Command link
            'td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "This is 'CommandLink1'.")
            'td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "This is 'CommandLink2'.")
            'td.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "This is 'CommandLink3'.")
            'td.AddCommandLink(TaskDialogCommandLinkId.CommandLink4, "This is 'CommandLink4'.")

            ' Common button 
            td.CommonButtons = TaskDialogCommonButtons.Cancel Or TaskDialogCommonButtons.Ok '
            'Or TaskDialogCommonButtons.Close Or TaskDialogCommonButtons.No Or TaskDialogCommonButtons.Yes Or TaskDialogCommonButtons.Retry Or TaskDialogCommonButtons.None
            td.DefaultButton = TaskDialogResult.Ok

            ' Dialog showup 
            Dim tdRes As TaskDialogResult = td.Show()

            'MessageBox.Show(String.Format("Button result: {0}" & ControlChars.Lf & "VerifictionText checked: {1}", tdRes.ToString(), td.WasVerificationChecked()))
            If tdRes.ToString() <> "Ok" Then Exit Function

            Dim MEPspacesNamed As Integer = 0
            Dim Row As Integer = 1

            For Each SpaceRef As Element In MEPspaces
                ACspace = TryCast(SpaceRef, Autodesk.Revit.DB.Mechanical.Space)
                Dim oldACspaceName As String = ACspace.Name
                Dim oldACspaceNumber As String = ACspace.Number

                'Dim Ptlocation As LocationPoint
                'Dim SpacePoint As Autodesk.Revit.DB.XYZ = Nothing

                'If SpaceRef.Location IsNot Nothing Then
                '    Ptlocation = TryCast(SpaceRef.Location, LocationPoint)
                '    SpacePoint = Ptlocation.Point
                'End If

                If ACspace Is Nothing Then
                    Return Autodesk.Revit.UI.Result.Failed
                End If

                For Each LinkedRoom As Room In ArchRooms ' (New FilteredElementCollector(LinkedDoc)).OfClass(GetType(SpatialElement)).OfCategory(BuiltInCategory.OST_Rooms).Cast(Of Room)().Where(Function(q) q.Area > 3)
                    Dim roomPoint As XYZ = CType(LinkedRoom.Location, LocationPoint).Point

                    If ACspace.IsPointInSpace(roomPoint) Then
                        If debug Then If DsgBox(LinkedRoom.Name.ToString & "-" & LinkedRoom.Number.ToString & " is in space" & ACspace.Name.ToString) = "END" Then Return Autodesk.Revit.UI.Result.Cancelled
                        ACspace.Name = StripNumber(LinkedRoom.Name, LinkedRoom.Number)
                        ACspace.Number = LinkedRoom.Number
                        If debug Then If DsgBox(LinkedRoom.Name & ":" & StripNumber(LinkedRoom.Name, LinkedRoom.Number)) = "END" Then Return Autodesk.Revit.UI.Result.Cancelled
                        MEPspacesNamed += 1
                        Exit For
                    End If 'AcSpace.IsPointInSpace

                Next LinkedRoom

                'linked space name not found.  Use the room name
                'If ACspace.Name.ToString = oldACspaceName Then
                '    ACspace.Name = ACspace.Room.Name
                '    ACspace.Number = ACspace.Room.Number
                'End If

                Dim parameters As Autodesk.Revit.DB.ParameterSet = SpaceRef.Parameters
                Dim parameter As Autodesk.Revit.DB.Parameter

                For Each parameter In parameters

                    If (parameter.Definition.Name = "CADObjectId") Then
                        parameter.Set(SpaceRef.Id.IntegerValue)
                    End If

                    If (parameter.Definition.Name = "Comments") Then

                        parameter.Set(SpaceRef.Id.IntegerValue.ToString)
                    End If

                    If parameter.Definition.Name.Contains("SPACE ID") Then
                        parameter.Set(SpaceRef.Id.IntegerValue.ToString)
                        'MsgBox(space.Name.ToString & ":" & eRef.Name.ToString & " element " & Row.ToString)
                    End If

                Next

                Row += 1

            Next SpaceRef
            AcMessage += "MEP Spaces with name updated: " & MEPspacesNamed.ToString & vbCrLf & "MEP Spaces with CADObjectId added: " & (Row - 1).ToString & vbCrLf
            AcMessage += vbCrLf & vbCrLf & "Press OK to complete or Cancel to abort changes."
            td.Id = "F-T Space Naming Utility"
            td.MainIcon = TaskDialogIcon.TaskDialogIconWarning

            td.Title = "F-T Space Naming Utility"
            td.TitleAutoPrefix = True
            td.AllowCancellation = True

            ' Message 
            td.MainInstruction = "Revit MEP space names/no's now match the Architectural room names/no's."
            td.MainContent = AcMessage

            'td.FooterText = ""
            td.ExpandedContent = "When creating a central file with a linked architectural file the room names and number do not automatically populate the space names and numbers." & ControlChars.Lf

            'VerificationText 
            'td.VerificationText = "This is 'VerificationText'."

            ' Command link
            'td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "This is 'CommandLink1'.")
            'td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "This is 'CommandLink2'.")
            'td.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "This is 'CommandLink3'.")
            'td.AddCommandLink(TaskDialogCommandLinkId.CommandLink4, "This is 'CommandLink4'.")

            ' Common button 
            td.CommonButtons = TaskDialogCommonButtons.Cancel Or TaskDialogCommonButtons.Ok '
            'Or TaskDialogCommonButtons.Close Or TaskDialogCommonButtons.No Or TaskDialogCommonButtons.Yes Or TaskDialogCommonButtons.Retry Or TaskDialogCommonButtons.None
            td.DefaultButton = TaskDialogResult.Ok

            ' Dialog showup 
            tdRes = td.Show()

            'MessageBox.Show(String.Format("Button result: {0}" & ControlChars.Lf & "VerifictionText checked: {1}", tdRes.ToString(), td.WasVerificationChecked()))
            If tdRes.ToString() <> "Ok" Then Exit Function

            trans.Commit()
        End Using

        Return Autodesk.Revit.UI.Result.Succeeded


    End Function
    Public Function StripNumber(ByVal SearchWithin As String, SearchFor As String) As String
        Dim firstcharacter As Integer = SearchWithin.IndexOf(SearchFor)
        Return Left(SearchWithin, firstcharacter)
    End Function
    Public Function DsgBox(ByVal Msg As String) As String

        If MsgBox(Msg, MsgBoxStyle.OkCancel, "DEBUG CONTINUE") = MsgBoxResult.Cancel Then
            Return "END"
        Else
            Return "Continue"
        End If
    End Function
    Sub FilterTags() '
        'Dim tfilter As Autodesk.Revit.DB.Mechanical.SpaceTagFilter
        '            tfilter = New Autodesk.Revit.DB.Mechanical.SpaceTagFilter()
        '            'Dim tcollector As New FilteredElementCollector(doc, View.Id)
        '            'tcollector.WherePasses(New tfiler)
        '            Dim tcollector As Autodesk.Revit.DB.FilteredElementCollector
        '            tcollector = New Autodesk.Revit.DB.FilteredElementCollector(commandData.Application.ActiveUIDocument.Document)
        '            tcollector.WherePasses(tfilter).ToElements()
        '            Dim I As Integer = 1
        'For Each TRef As Element In tcollector
        'If MsgBox("Next Tref" & TRef.Name & TRef.Id, MsgBoxStyle.OkCancel, "DEBUG CONTINUE") = MsgBoxResult.Cancel Then
        'Exit Function
        'End If
        'Dim tparameters As Autodesk.Revit.DB.ParameterSet = TRef.Parameters
        'Dim tparameter As Autodesk.Revit.DB.Parameter
        'For Each tparameter In tparameters
        '' parameter(BuiltInParameter.SPACE_ZONE_NAME).AsString)
        'Dim Msg As String = TRef.Name & "Parameter:" & tparameter.Definition.Name & "=" & tparameter.AsString
        'If MsgBox(Msg, MsgBoxStyle.OkCancel, "DEBUG CONTINUE") = MsgBoxResult.Cancel Then
        'Exit For
        'End If
        ''If I = 1 Then Exit For
        'Next
        'If MsgBox("Next Tref", MsgBoxStyle.OkCancel, "DEBUG CONTINUE") = MsgBoxResult.Cancel Then
        'Exit Function
        'End If
        'Next
    End Sub

End Class


