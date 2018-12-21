# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Set stiffness value in FEM-design xml-file
'''

# Set sys.path
import sys
sys.path.insert(0, 'C:\Program files (x86)\IronPython 2.7\Lib')

# Python and DanEDU module
import traceback
import xml.etree.ElementTree as ET
ET.register_namespace('','urn:strusoft')

# argument assigned the IN port
file_ = IN[0]
stiffness = IN[1]
value = IN[2]

# instantiate error log
error_log = None

# core data processing
try:
    # read the XML file
    with open(file_, 'r') as i:
        root = ET.parse(i).getroot()
    i.close()
    entities = root.find('{urn:strusoft}entities')

    # find all tags
    tags = entities.findall('.//{urn:strusoft}' + stiffness)
    for item in list(tags):
        item.set('neg', str(value))
        item.set('pos', str(value))

    # write to the XML file.
    tree = ET.ElementTree(root)
    with open(file_, 'w') as f:
        tree.write(f, xml_declaration=True, encoding='utf-8', method='xml')
    f.close()

    # set result output
    result = file_

except BaseException:
    err = 'File or Path is wrong/missing!\nPlease veryfy settings'
    error_log = err + traceback.format_exc()
    raise StandardError(error_log)

# return assigned the OUT port
if error_log:
    result = error_log
OUT = result
