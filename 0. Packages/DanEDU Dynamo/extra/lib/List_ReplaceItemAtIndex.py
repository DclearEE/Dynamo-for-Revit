# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Replace item at index in nD list
'''

# argument assigned the IN port
lst = IN[0]
index = IN[1]
item = IN[2]

# wrap input inside a list (if not a list)
if not isinstance(index, list):
    index = [index]

def set_nested_value(lst, indices, value):
    for index in indices[:-1]:
        lst = lst[index]
    lst[indices[-1]] = value

for idx in index:
    set_nested_value(lst, idx, item)

# return assigned the OUT port
OUT = lst
