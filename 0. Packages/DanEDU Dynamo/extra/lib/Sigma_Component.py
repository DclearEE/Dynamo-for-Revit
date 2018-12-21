# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Create Component data for Sigma.Write node
Summarize instances or pass instances
'''

# Set sys.path
import sys
sys.path.insert(0, 'C:\Program files (x86)\IronPython 2.7\Lib')

# Python modules
import collections
import traceback

# argument assigned the IN port
instance = IN[0]
element = IN[1]

# group the values in a dictionary
element_quantity = collections.defaultdict(int)
element_id = collections.defaultdict(list)

# core data processing
element_val = element[1]
try:
    if instance or len(element_val) == 1:
        t = element_val
    else:
        for key, unit, qty, reg, el_id in element_val:
            element_quantity[key, unit, reg] += qty
            element_id[key, unit, reg].append(el_id)
        t = []
        ids = []
        # order ids
        for elem_id in element_id.items():
            if len(elem_id[1]) == 1:
                ids.append(elem_id[1][0])
            else:
                ids.append(elem_id[1])
        # order output
        for e_qty, e_id in zip(element_quantity.items(), ids):
            t.append([e_qty[0][0], e_qty[0][1], e_qty[1], e_qty[0][2], e_id])
    result = element[0], t
except BaseException:
    err = (
        'Component Failed, correct input!\n'
        'List [0] {"number","text"}, List [1]\n'
        '{code, unit, quantity, regul, id}\n'
        'Input cant be null or blank!\n\n')
    error_log = err + traceback.format_exc()
    raise StandardError(error_log)

# return assigned the OUT port
OUT = result
