# coding=utf-8
# This file is covered by the LICENSE file in the dynamo package 'extra' folder
'''
Create parameter in a family
'''

# Common Language Runtime module
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitServices')

# Revit and Dynamo module
from Autodesk.Revit.DB import FamilyManager, BuiltInParameterGroup, ParameterType
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

uiapp = DocumentManager.Instance.CurrentUIApplication
app = uiapp.Application

# argument assigned the IN port
docs = IN[0]  # document list
parameter_names = IN[1]  # parameter name in shared parameter file
parameter_type = IN[2]  # ParameterType Enumeration
parameter_group = IN[3]  # BuiltInParameterGroup Enumeration
instance = IN[4]  # boolean value for instance

# default document set to DocumentManager.Instance.CurrentDBDocument
if docs == 'Current.Document':
    docs = DocumentManager.Instance.CurrentDBDocument

# wrap input inside a list (if not a list)
if not isinstance(docs, list):
    docs = [docs]
if not isinstance(parameter_names, list):
    parameter_names = [parameter_names]

# change string to BuiltInParameterGroup
if isinstance(parameter_group, basestring):
    exec("parameter_group = BuiltInParameterGroup.%s" % parameter_group)
if isinstance(parameter_type, basestring):
    exec("parameter_type = ParameterType.%s" % parameter_type)

# core data processing
for doc in docs:
    TransactionManager.Instance.EnsureInTransaction(doc)
    for parameter_name in parameter_names:
        try:
            doc.FamilyManager.AddParameter(
                parameter_name, parameter_group, parameter_type, instance)
        except BaseException:
            # error will not be shown
            pass
    TransactionManager.Instance.ForceCloseTransaction()

# return assigned the OUT port
OUT = docs
