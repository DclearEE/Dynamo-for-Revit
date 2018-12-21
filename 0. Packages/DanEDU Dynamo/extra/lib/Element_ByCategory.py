# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Get elements by category
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

# .Net module
import System

# Common Language Runtime module
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitServices')
clr.AddReference('RevitNodes')

# Revit and Dynamo module
from Autodesk.Revit.DB import BuiltInCategory, FilteredElementCollector
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager
from Revit.Elements import Category

uiapp = DocumentManager.Instance.CurrentUIApplication
app = uiapp.Application

# argument assigned the IN port
docs = IN[0]  # documents
cats = IN[1]  # categories

# default document set to DocumentManager.Instance.CurrentDBDocument
if docs == 'Current.Document':
    docs = DocumentManager.Instance.CurrentDBDocument

# wrap input inside a list (if not a list)
if not isinstance(docs, list):
    docs = [docs]
if not isinstance(cats, list):
    cats = [cats]

bics = []  # built-in categories
for cat in cats:
    # change string to BuiltInParameterGroup
    if isinstance(cat, basestring):
        cat = Category.ByName(cat)
    bics.append(System.Enum.ToObject(BuiltInCategory, cat.Id))

# elements grouped by category (group by key)
element, category = [], []
for doc in docs:
    TransactionManager.Instance.EnsureInTransaction(doc)
    t1, t2, t3 = [], [], []
    for bic in bics:
        elems = (
            FilteredElementCollector(doc).OfCategory(bic)
            .WhereElementIsNotElementType().ToElements())
        if elems:
            t1.append(elems)
            t2.append(elems[0].Category.Name)
    element.append(t1)
    category.append(t2)
    TransactionManager.Instance.ForceCloseTransaction()

# check if only one document in list
# return assigned the OUT port
OUT = SingleOut(element), SingleOut(category)
