# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Load family by family document, same core process as File.LoadFamily
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
fam_docs = IN[0]  # document list

# wrap input inside a list (if not a list)
if not isinstance(fam_docs, list):
    fam_docs = [fam_docs]

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
for fam_doc in fam_docs:
    fam_doc.LoadFamily(doc, FamilyOption())

# return assigned the OUT port
OUT = fam_docs
