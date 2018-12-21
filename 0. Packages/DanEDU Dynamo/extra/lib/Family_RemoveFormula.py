# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Remove formula value by name in a family
'''

# Common Language Runtime module
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitServices')

# Revit and Dynamo module
from Autodesk.Revit.DB import FamilyManager
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

uiapp = DocumentManager.Instance.CurrentUIApplication
app = uiapp.Application

# argument assigned the IN port
docs = IN[0]
parameter_names = IN[1]

# default document set to DocumentManager.Instance.CurrentDBDocument
if docs == 'Current.Document':
    docs = DocumentManager.Instance.CurrentDBDocument

# wrap input inside a list (if not a list)
if not isinstance(docs, list):
    docs = [docs]
if not isinstance(parameter_names, list):
    parameter_names = [parameter_names]

# core data processing
for doc in docs:
    TransactionManager.Instance.EnsureInTransaction(doc)
    for parameter_name in parameter_names:
        param = doc.FamilyManager.get_Parameter(parameter_name)
        doc.FamilyManager.SetFormula(param, None)
    TransactionManager.Instance.ForceCloseTransaction()

# return assigned the OUT port
OUT = docs
