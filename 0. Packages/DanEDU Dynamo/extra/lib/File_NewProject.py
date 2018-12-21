# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Create new project by name
'''

# Common Language Runtime module
import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitServices')

# Revit and Dynamo module
from Autodesk.Revit.DB import SaveAsOptions
from RevitServices.Persistence import DocumentManager

uiapp = DocumentManager.Instance.CurrentUIApplication
app = uiapp.Application

# argument assigned the IN port
new_projects = IN[0]  # list of new projects
template = IN[1]  # single template or list of templates

# wrap input inside a list (if not a list)
if not isinstance(new_projects, list):
    new_projects = [new_projects]

# instantiate error log
error_log = None

# save as options
option = SaveAsOptions()
option.OverwriteExistingFile = True

# single template definition
def single_template():
    for new_project in new_projects:
        t = app.NewProjectDocument(template)
        t.SaveAs(new_project, option)
        t.Close(False)

# multiple template definition
def multiple_template(xx = []):
    for new_project, temp in zip(new_projects, template):
        new_projects.pop(0)
        template.pop(0)
        t = app.NewProjectDocument(temp)
        t.SaveAs(new_project, option)
        t.Close(False)

# if template is list, then check length
if isinstance(template, list):
    if len(new_projects) == len(template):
        multiple_template()
    else:
        error_log = 'Length of new files and templates doesnt math!'
else:
    single_template()

# return assigned the OUT port
if error_log:
    new_projects = error_log
OUT = new_projects
