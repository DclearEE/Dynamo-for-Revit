# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Set parameter value by name in a family
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
values = IN[2]

# default document set to DocumentManager.Instance.CurrentDBDocument
if docs == 'Current.Document':
    docs = DocumentManager.Instance.CurrentDBDocument

# wrap input inside a list (if not a list)
if not isinstance(docs, list):
    docs = [docs]
if not isinstance(parameter_names, list):
    parameter_names = [parameter_names]
if not isinstance(values, list):
    values = [values]

# set the parameter value
def param_value(parameter, value):
    if parameter.StorageType.ToString() == 'ElementId':
        value = UnwrapElement(value).Id
    return value

# core data processing
for doc in docs:
    TransactionManager.Instance.EnsureInTransaction(doc)
    fam_types = doc.FamilyManager.Types
    for parameter_name in parameter_names:
        param = doc.FamilyManager.get_Parameter(parameter_name)        
        for idx, fam_type in enumerate(fam_types):
            doc.FamilyManager.CurrentType = fam_type
            doc.FamilyManager.Set(param, param_value(param, values[idx]))
    TransactionManager.Instance.ForceCloseTransaction()

# return assigned the OUT port
OUT = docs
