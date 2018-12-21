# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Get reinforcement from FEM-design
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
    slabs = root.find('{urn:strusoft}entities/{urn:strusoft}slab')
    shells = root.find('{urn:strusoft}entities/{urn:strusoft}surface_reinforcement')

    # list for elements and values
    t1, t2, t3, t4 = [], [], [], []

    for i, e in zip(ids, elements):
        for slab in entities.iter('{urn:strusoft}slab'):
            if i == slab.get('guid'):
                for part in slab.iter('{urn:strusoft}slab_part'):
                    z = part.get('guid')
        for shell in entities.iter('{urn:strusoft}surface_reinforcement'):
            for base in shell.iter('{urn:strusoft}base_shell'):
                y = base.get('guid')
                if z == y:
                    for wire in shell.iter('{urn:strusoft}wire'):
                        t1.append(e)
                        t2.append(wire.get('diameter'))
                    for wire in shell.iter('{urn:strusoft}straight'):
                        t3.append(wire.get('space'))
                        t4.append(wire.get('face'))

    # set result output
    result = t1, t2, t3, t4

except BaseException:
    err = 'File or Path is wrong/missing!\nPlease veryfy settings'
    error_log = err + traceback.format_exc()
    raise StandardError(error_log)

# return assigned the OUT port
if error_log:
    result = error_log
OUT = result
