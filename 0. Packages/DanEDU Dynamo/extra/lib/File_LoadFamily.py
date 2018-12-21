# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Load family by file name, same core process as FamilyDocument.LoadFamily
'''

# Common Language Runtime module
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitServices')

# Revit and Dynamo module
from Autodesk.Revit.DB import Document, FamilySource, IFamilyLoadOptions
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

doc = DocumentManager.Instance.CurrentDBDocument
uiapp = DocumentManager.Instance.CurrentUIApplication
app = uiapp.Application

# argument assigned the IN port
paths = IN[0]

# wrap input inside a list (if not a list)
if not isinstance(paths, list):
    paths = [paths]

# ensure loaded families can overwrite existing families.
class FamilyOption(IFamilyLoadOptions):
    def OnFamilyFound(self, familyInUse, overwriteParameterValues):
        overwriteParameterValues = True
        return True

    def OnSharedFamilyFound(
            self, sharedFamily, familyInUse, source, overwriteParameterValues):
        source = FamilySource.Family
        overwriteParameterValues = True
        return True

# core data processing
for path in paths:
    family_doc = app.OpenDocumentFile(path)
    family_doc.LoadFamily(doc, FamilyOption())
    family_doc.Close(False)

# return assigned the OUT port
OUT = paths
