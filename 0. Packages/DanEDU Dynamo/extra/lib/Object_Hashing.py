# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
hash input value(s)
'''

# argument assigned the IN port
var = IN[0]

# core data processing
if not isinstance(var, basestring):
    string_var = str(var)

# return assigned the OUT port
OUT = var, hash(string_var)
