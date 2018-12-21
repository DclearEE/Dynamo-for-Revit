# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Get family by name in document
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
import os
from Utilities import *

# Common Language Runtime module
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitServices')

# Revit and Dynamo module
from Autodesk.Revit.DB import FilteredElementCollector, Family
from RevitServices.Persistence import DocumentManager

doc = DocumentManager.Instance.CurrentDBDocument

# argument assigned the IN port
elements = IN[0]

# wrap input inside a list (if not a list)
if not isinstance(elements, list):
    elements = [elements]

# get family_type from family
family_type = []
collector = FilteredElementCollector(doc).OfClass(Family)
for element in collector.ToElements():
    if element.Name in elements:
        t = []
        for family_type_id in element.GetFamilySymbolIds():
            t.append(doc.GetElement(family_type_id))
        family_type.append(t)

# return assigned the OUT port
OUT = SingleOut(family_type)
