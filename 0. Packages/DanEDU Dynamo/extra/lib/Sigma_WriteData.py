# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Set Revit data in Sigma Estimate project file
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
import codecs
import traceback
import xml.etree.ElementTree as ET
from Utilities import *

# argument assigned the IN port
file_ = IN[0]
component_root = IN[1]
clearing = IN[2]
element = IN[3]

# instantiate error log
error_log = None

# clearing component root
def clear(root):
    try:
        if component_root == 'null' or component_root is None \
                or component_root == '':
            raise Exception
        else:
            # clear entire root
            r = root.findall('Component')
            for component in list(r):
                root.remove(component)
    except BaseException:
        err = (
            'Cleaning Failed, correct input!\n'
            'Component Root must exist when using clearning!\n\n')
        error_log = err + traceback.format_exc()
        raise StandardError(error_log)

# component builder
def component(items, tree):
    try:
        if clearing is False:
            # built list of ids
            id_in = []
            for item in items:
                id_in.append(item[4])
            ids = Flatten(id_in)
            # check if node exist
            try:
                for component in tree.findall('.//Component'):
                    definition_id = component.find('.//DefinitionId')
                    value_id = component.find('.//Value')
                    for i in ids:
                        if 'DynamoRevitId' in definition_id.text \
                                and str(i) in value_id.text:
                            tree.remove(component)
            except BaseException:
                pass
        else:
            pass
        # create missing node
        for item in items:
            t = ET.SubElement(tree, 'Component')
            ET.SubElement(t, 'Number').text = str(item[0])
            ET.SubElement(t, 'Name').text = 'updated via library'
            ET.SubElement(t, 'Amount').text = str(item[2])
            ET.SubElement(t, 'AmountMin').text = str(item[2])
            ET.SubElement(t, 'AmountTyp').text = str(item[2])
            ET.SubElement(t, 'AmountMax').text = str(item[2])
            ET.SubElement(t, 'Unit').text = str(item[1])
            ET.SubElement(t, 'Regulation').text = str(item[3])
            # Revit ids
            idt = item[4]
            # Sigma controlled
            if not isinstance(idt, list):
                t1 = ET.SubElement(t, 'ExternalData')
                t2 = ET.SubElement(t1, 'Item')
                ET.SubElement(t2, 'DataSourceId').text = 'Revit_1'
                ET.SubElement(t2, 'ExternalId').text = str(idt)
            # dynamo controlled
            i1 = ET.SubElement(t, 'CustomFieldValues')
            i2 = ET.SubElement(i1, 'CustomFieldValue')
            ET.SubElement(i2, 'DefinitionId').text = 'DynamoRevitId'
            if not isinstance(idt, list):
                ET.SubElement(i2, 'Value').text = str(idt)
            else:
                ET.SubElement(i2, 'Value').text = ', '.join(map(str, idt))
    except BaseException:
        err = (
            'Component Failed, correct input!\n'
            '{element, unit, quantity, regul, id}\n'
            'Input cant be null or blank!\n\n')
        error_log = err + traceback.format_exc()
        raise StandardError(error_log)

# recursive tree builder
def component_tree(items, num, tree):
    try:
        if num != 0:
            # check if node exist
            check = None
            for component in tree.findall('.//Component'):
                number = component.find('.//Number')
                if number.text == str(items[num - 1][0]):
                    check = number.text
                    tree = component
            # create missing node
            if check is None:
                t = ET.SubElement(tree, 'Component')
                ET.SubElement(t, 'Number').text = str(items[num - 1][0])
                ET.SubElement(t, 'Name').text = items[num - 1][1] \
                    .encode('iso-8859-1')
                ET.SubElement(t, 'Amount').text = '1'
                ET.SubElement(t, 'AmountMin').text = '1'
                ET.SubElement(t, 'AmountTyp').text = '1'
                ET.SubElement(t, 'AmountMax').text = '1'
                ET.SubElement(t, 'Regulation').text = '1'
                ET.SubElement(t, 'Flags').text = 'g'
                tree = t
            return component_tree(items, num - 1, tree)
        return tree
    except BaseException:
        err = (
            'Component Tree Failed, correct input!\n'
            '{"number","text"} input cant be null or blank!\n\n')
        error_log = err + traceback.format_exc()
        raise StandardError(error_log)

# existing XML document with 'data', encoded with iso-8859-1 in a utf-8 file!
try:
    # check if file is opened by others
    with codecs.open(file_, 'r', 'utf-8', 'iso8859-1') as data_read:
        root = ET.parse(data_read).getroot()
    data_read.close()
    project_data = root.find('ProjectData')
except BaseException:
    err = (
        '\nFile or Path is wrong/missing, correct input!\n'
        'This error will tricker all error messages!\n\n')
    error_log = err + traceback.format_exc()
    raise StandardError(error_log)

# check if CustomFieldDefinition for RevitId exist
check = False
for custom_field in project_data.findall('.//CustomFieldDefinition'):
    revit_id = custom_field.find('Id')
    if 'DynamoRevitId' in revit_id.text:
        check = True

# create CustomFieldDefinition for RevitId
if check is False:
    custom_field_definitions = project_data.find('CustomFieldDefinitions')
    t = ET.SubElement(custom_field_definitions, 'CustomFieldDefinition')
    ET.SubElement(t, 'Id').text = 'DynamoRevitId'
    ET.SubElement(t, 'DisplayName').text = 'DynamoRevitId'
    ET.SubElement(t, 'Description').text = 'RevitId passed from dynamo'

# check if a component root is handed
try:
    # dynamo input cant send null as default -> 'null' are therefore hardcoded
    if component_root == 'null' or component_root is None \
            or component_root == '':
        component_data = project_data.find('ComponentData')
        # project root in sigma file e.g. project name
        root_project = component_data.find('Component')
    else:
        # given base point (root) in sigma file e.g. 'Building'
        value = component_root.encode('iso-8859-1')
        t = root.findall(".//*[@Name='%s']" % value)
        root_project = t[0]
except BaseException:
    err = (
        '\nComponent Root Failed, correct input!\nor leave unconnected!\n\n')
    error_log = err + traceback.format_exc()
    raise StandardError(error_log)

# core data processing
try:
    # check for instance or summarized
    clear(root_project)
    for elem in element:
        t = list(reversed(elem[0]))
        component(elem[1], component_tree(t, len(t), root_project))
    result = 'Sigma file updated successfully'
except BaseException:
    err = ('Parsing Failed, correct input!\n')
    error_log = err + traceback.format_exc()
    raise StandardError(error_log)

# create xml tree and write to file
try:
    # check if file is opened by others
    tree = ET.ElementTree(root)
    with open(file_, 'w') as data_write:
        data_write.write('<?xml version="1.0" encoding="UTF-8"?>')
        data_write.write(
            '<?xml-stylesheet type="text/xsl" '
            'href="http://www.codegroup.dk/xsl/2008/07/04/SigmaFile.xsl"?>')
        tree.write(data_write, encoding='utf-8', xml_declaration=False)
    data_write.close()
except BaseException:
    err = (
        'Cant write to an open/missing file!\n'
        'Close/verify the Sigma file!\nDynamo graph must be restarted!\n\n')
    error_log = err + traceback.format_exc()
    raise StandardError(error_log)

# return assigned the OUT port
if error_log:
    result = error_log
OUT = result
