# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Sort and group by key
'''

# argument assigned the IN port
element = IN[0]
keys = IN[1]
sorted_keys = IN[2]

# core data processing
element_list, key_list = [], []
for _, sorted_key in enumerate(sorted_keys):
    t1, t2 = [], []
    for idx, key in enumerate(keys):
        if sorted_key == key:
            t1.append(element[idx])
            t2.append(key)
    element_list.append(t1)
    key_list.append(t2)

# return assigned the OUT port
OUT = element_list, key_list
