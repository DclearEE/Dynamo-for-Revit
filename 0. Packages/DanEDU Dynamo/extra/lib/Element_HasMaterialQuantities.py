# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Get all used categories in document that has material quantities
'''

# .Net module
import System

# Common Language Runtime module
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitServices')

# Revit and Dynamo module
from Autodesk.Revit.DB import Category, BuiltInCategory, FilteredElementCollector
from RevitServices.Persistence import DocumentManager

doc = DocumentManager.Instance.CurrentDBDocument
cats = doc.Settings.Categories  # categories

bics = []  # built-in categories
for cat in cats:
    if cat.HasMaterialQuantities:
        bics.append(System.Enum.ToObject(BuiltInCategory, cat.Id.IntegerValue))

# elements grouped by category (group by key)
element, category = [], []
for bic in bics:
    elems = (
        FilteredElementCollector(doc).OfCategory(bic)
        .WhereElementIsNotElementType().ToElements())
    if elems:
        element.append(elems)
        category.append(elems[0].Category.Name)

# return assigned the OUT port
OUT = element, category
