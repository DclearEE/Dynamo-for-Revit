# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Save family by name
'''

# .Net module
from System.IO import Directory

# Common Language Runtime module
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitServices')

# Revit and Dynamo module
from Autodesk.Revit.DB import Document, SaveAsOptions, Family
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

doc = DocumentManager.Instance.CurrentDBDocument

# argument assigned the IN port
path = IN[0]
families = IN[1]
sub_folder = IN[2]
boolean = IN[3]

# wrap input inside a list (if not a list)
if not isinstance(families, list):
    families = [families]

# unwrap Dynamo elements
families = list(UnwrapElement(families))

# Overwrite option
overwrite = SaveAsOptions()
overwrite.OverwriteExistingFile = boolean

# Close all transactions
TransactionManager.Instance.ForceCloseTransaction()

# build paths
def path_builder(path, category, sub_folder):
    if sub_folder:
        new_path = path + '\\' + category
        Directory.CreateDirectory(new_path)
        return new_path
    else:
        return path

# load families
t = 0
for family in families:
    category = family.FamilyCategory.Name
    new_path = path_builder(path, category, sub_folder)
    family_doc = doc.EditFamily(family)
    family_doc.SaveAs(new_path + '\\' + family.Name + '.rfa', overwrite)
    family_doc.Close(False)
    t += 1

# return assigned the OUT port
OUT = str(t) + ' File(s) Saved: ' + category
