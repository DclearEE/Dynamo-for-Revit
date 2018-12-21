# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Replace item in nD list
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

# argument assigned the IN port
items = IN[0]
search_for = IN[1]
replace_with = IN[2]

# return assigned the OUT port
if isinstance(replace_with, list):
    if not len(replace_with) == len(search_for):
        OUT = (
            'list of search items and\n'
            'list of replacements must\nhave the same length')
    else:
        for t1, t2 in zip(search_for, replace_with):
            search_for = t1
            replace_with = t2
            items = Replace(items, search_for, replace_with)
        OUT = items
else:
    OUT = Replace(items, search_for, replace_with)
