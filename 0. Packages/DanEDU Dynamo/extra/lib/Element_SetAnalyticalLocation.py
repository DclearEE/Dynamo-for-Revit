
# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Set (structural) element analytical location
'''

# Common Language Runtime module
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitServices')

# Revit and Dynamo module
from Autodesk.Revit.DB.Structure import SurfaceElementProjectionZ as PZ
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

doc = DocumentManager.Instance.CurrentDBDocument

# argument assigned the IN port
elements = list(UnwrapElement(IN[0]))
location = IN[1]

# location enum dictionary
location_enum = {
    # Analytical floor
    'Top of Element': PZ.TopOrInterior,
    'Center of Element': PZ.CenterOfElement,
    'Bottom of Element': PZ.BottomOrExterior,
    # Analytical wall
    'Interior Face of Element': PZ.TopOrInterior,
    'Center of Element': PZ.CenterOfElement,
    'Exterior Face of Element': PZ.BottomOrExterior,
    'Center of Core': PZ.CenterOfCore}

# core data processing
TransactionManager.Instance.EnsureInTransaction(doc)
for element in elements:
    element.ProjectionZ = location_enum[location]
TransactionManager.Instance.ForceCloseTransaction()

# return assigned the OUT port
OUT = elements
