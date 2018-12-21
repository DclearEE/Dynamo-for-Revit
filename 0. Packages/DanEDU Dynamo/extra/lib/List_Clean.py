# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Clean nD list
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
from Utilities import Clean

# argument assigned the IN port
items = IN[0]
search_for = IN[1]

# return assigned the OUT port
OUT = Clean(items, search_for)
