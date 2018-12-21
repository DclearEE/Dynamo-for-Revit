# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Add shared parameter in a family
'''

# Set sys.path
import sys
sys.path.insert(0, 'C:\Program files (x86)\IronPython 2.7\Lib')

# Python modules
import os
import traceback

# Common Language Runtime module
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitServices')

# Revit and Dynamo module
from Autodesk.Revit.DB import FamilyManager, BuiltInParameterGroup
from RevitServices.Persistence import DocumentManager
from RevitServices.Transactions import TransactionManager

uiapp = DocumentManager.Instance.CurrentUIApplication
app = uiapp.Application

# argument assigned the IN port
docs = IN[0]  # document list
sp_parameter_names = IN[1]  # parameter name in shared parameter file
sp_parameter_group = IN[2]  # group in shared parameter file
parameter_group = IN[3]  # BuiltInParameterGroup
instance = IN[4]  # boolean value for instance

# default document set to DocumentManager.Instance.CurrentDBDocument
if docs == 'Current.Document':
    docs = DocumentManager.Instance.CurrentDBDocument

# wrap input inside a list (if not a list)
if not isinstance(docs, list):
    docs = [docs]
if not isinstance(sp_parameter_names, list):
    sp_parameter_names = [sp_parameter_names]

# change string to BuiltInParameterGroup
if isinstance(parameter_group, basestring):
    exec("parameter_group = BuiltInParameterGroup.%s" % parameter_group)

# external definition
def external_definition(_file, _group, _param):
    definition_group = _file.Groups.get_Item(_group)
    return definition_group.Definitions.get_Item(_param)

for doc in docs:
    TransactionManager.Instance.EnsureInTransaction(doc)
    try:
        if not os.path.isfile(app.SharedParametersFilename):
            raise BaseException
        # read the default shared parameter file
        sp_file = app.OpenSharedParameterFile()
    except BaseException:
        err = '\nNo Shared Parameter file found, correct input!\n\n'
        error_log = err + traceback.format_exc()
        raise StandardError(error_log)

    # core data processing
    for sp_parameter_name in sp_parameter_names:
        try:
            ext_definition = external_definition(
                sp_file, sp_parameter_group, sp_parameter_name)
            doc.FamilyManager.AddParameter(
                ext_definition, parameter_group, instance)
        except BaseException:
            # error will not be shown
            pass
    TransactionManager.Instance.ForceCloseTransaction()

# return assigned the OUT port
OUT = docs
