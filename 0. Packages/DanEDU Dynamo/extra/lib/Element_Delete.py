# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Delete element, output boolean
'''

# Common Language Runtime module
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitServices')

# Revit and Dynamo module
from Autodesk.Revit.DB import ElementId
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

doc = DocumentManager.Instance.CurrentDBDocument

# argument assigned the IN port
elements = IN[0]

# wrap input inside a list (if not a list)
if not isinstance(elements, list):
    elements = [elements]

# delete elements
def DeleteElement(elements):
    """
    delete elements in nD list
    """
    for element in elements:
        if isinstance(element, list):
            DeleteElement(element)
        else:
            doc.Delete(ElementId(element.Id))

# Start Transaction
TransactionManager.Instance.EnsureInTransaction(doc)

try:
    DeleteElement(elements)
except BaseException:
    pass

# Close all transactions
TransactionManager.Instance.ForceCloseTransaction()
