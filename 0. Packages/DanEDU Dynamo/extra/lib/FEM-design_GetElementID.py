# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Get element id from FEM-design
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
elements = IN[1]
ids = IN[2]

# instantiate error log
error_log = None

# core data processing
try:
    # check if file is opened by others
    with open(file_, 'r') as i:
        root = ET.parse(i).getroot()
    i.close()
    entities = root.find('{urn:strusoft}entities')

    # get the id from the xml file
    fem_id = []
    for idx, element in zip(ids, elements):
        for entity in entities.iter():
            if idx == entity.get('guid'):
                if entity.get('name'):
                    fem_id.append(entity.get('name'))
                else:
                    fem_id.append(entity[0].get('name'))

    # set result output
    result = fem_id

except BaseException:
    err = 'File or Path is wrong/missing!\nPlease veryfy settings'
    error_log = err + traceback.format_exc()
    raise StandardError(error_log)

# return assigned the OUT port
if error_log:
    result = error_log
OUT = result
