# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Item summarized and grouped by key
'''

# Set sys.path
import sys
sys.path.insert(0, 'C:\Program files (x86)\IronPython 2.7\Lib')

# Python modules
import collections

# argument assigned the IN port
values = IN[0]
keys = IN[1]

# group the values in a dictionary
keys_values = collections.defaultdict(int)

# sum grouped values
for key, val in zip(keys, values):
    keys_values[key] += val

# return assigned the OUT port
OUT = zip(*keys_values.items())
