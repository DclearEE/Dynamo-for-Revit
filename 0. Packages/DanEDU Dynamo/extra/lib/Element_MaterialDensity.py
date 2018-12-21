# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Get material density from element
'''

# Common Language Runtime module
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitServices')

# Revit and Dynamo module
from Autodesk.Revit.DB import Material, DisplayUnitType, UnitUtils
from RevitServices.Persistence import DocumentManager

doc = DocumentManager.Instance.CurrentDBDocument

# argument assigned the IN port
elements = list(UnwrapElement(IN[0]))

# wrap input inside a list (if not a list)
if not isinstance(elements, list):
    elements = [elements]

# core data processing
unit = DisplayUnitType.DUT_KILOGRAMS_PER_CUBIC_METER
structural_asset, thermal_asset, element = [], [], []
for elem in elements:
    try:
        t1 = doc.GetElement(
            elem.StructuralAssetId).GetStructuralAsset().Density
        structural_asset.append(UnitUtils.ConvertFromInternalUnits(t1, unit))
        t2 = doc.GetElement(elem.ThermalAssetId).GetThermalAsset().Density
        thermal_asset.append(UnitUtils.ConvertFromInternalUnits(t2, unit))
        element.append(elem)
    except BaseException:
        pass

# return assigned the OUT port
OUT = structural_asset, thermal_asset, element
