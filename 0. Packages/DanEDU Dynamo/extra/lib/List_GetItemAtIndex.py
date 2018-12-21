# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Fetch item at index in nD list
'''

# argument assigned the IN port
lst = IN[0]
index = IN[1]

# wrap input inside a list (if not a list)
if not isinstance(index, list):
    index = [index]


def set_nested_value(lst, indices):
    for index in indices[:-1]:
        lst = lst[index]
    return lst[indices[-1]]

t = []
for idx in index:
    t.append(set_nested_value(lst, idx))

# return assigned the OUT port
OUT = t
