# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Set (structural) floor area load
'''

# Common Language Runtime module
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitServices')

# Revit and Dynamo module
from Autodesk.Revit.DB import XYZ, DisplayUnitType, UnitUtils
from Autodesk.Revit.DB.Structure import AreaLoad
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

doc = DocumentManager.Instance.CurrentDBDocument

# argument assigned the IN port
elements = list(UnwrapElement(IN[0]))
load_value = IN[1]
load_type = list(UnwrapElement(IN[2]))

# wrap input inside a list (if not a list)
if not isinstance(load_value, list):
    load_value = [load_value]

# output unit
dut = DisplayUnitType.DUT_METERS

# core data processing
loads = []
TransactionManager.Instance.EnsureInTransaction(doc)
for ids, (element, outer) in enumerate(zip(elements, load_value)):
    for inner in outer:
        Zaxis = UnitUtils.ConvertFromInternalUnits(inner, dut)
        areaload = AreaLoad.Create(doc, element, XYZ(0, 0, Zaxis), load_type[0])
        loads.append(areaload)
TransactionManager.Instance.ForceCloseTransaction()

# return assigned the OUT port
OUT = loads
