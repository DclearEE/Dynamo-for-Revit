# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Get parameter value by name in a family
'''

# Set sys.path
import sys
sys.path.insert(0, 'C:\Program files (x86)\IronPython 2.7\Lib')
import System
for app in System.AppDomain.CurrentDomain.GetAssemblies():
    if 'DanEDUicon' in app.FullName:
        loc = app.Location
app_path = System.IO.Path.GetDirectoryName(loc).rsplit('\\', 1)[0]
sys.path.insert(1, app_path + '\extra\lib')

# Python and DanEDU module
from Utilities import *

# Common Language Runtime module
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitServices')

# Revit and Dynamo module
from Autodesk.Revit.DB import FamilyManager, FamilyParameter
from RevitServices.Persistence import DocumentManager

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

# get the parameter value
def param_value(type, parameter):
    if parameter.StorageType.ToString() == 'Integer':
        value = type.AsInteger(parameter)
    elif parameter.StorageType.ToString() == 'Double':
        value = type.AsDouble(parameter)
    elif parameter.StorageType.ToString() == 'ElementId':
        elem_id = type.AsElementId(parameter)
        value = doc.GetElement(elem_id)
    else:
        value = type.AsString(parameter)
    return value

# core data processing
types = []
values = []
for doc in docs:
    t1 = []
    t2 = []
    doc_types = sorted(doc.FamilyManager.Types, key=lambda t: t.Name)
    doc_params = doc.FamilyManager.Parameters
    for idx, parameter_name in enumerate(parameter_names):
        for doc_param in doc_params:
            param_temp = []
            if parameter_name == doc_param.Definition.Name:
                type_temp = []
                for doc_type in doc_types:
                    type_temp.append(doc_type.Name)
                    param_temp.append(param_value(doc_type, doc_param))
                t2.append(param_temp)
        if idx == 0:
            t1.append(type_temp)
    types.append(SingleOut(t1))
    values.append(SingleOut(t2))

# return assigned the OUT port
OUT = SingleOut(values), SingleOut(types)
