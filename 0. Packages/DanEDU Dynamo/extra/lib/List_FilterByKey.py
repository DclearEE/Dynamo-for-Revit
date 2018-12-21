# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Filter two list by key
'''

# argument assigned the IN port
# list 1
list_1 = IN[0]
key_1 = IN[1]

# list 2
list_2 = IN[2]
key_2 = IN[3]

# core data processing
list_a, list_b, key = [], [], []
for k1, l1 in zip(key_1, list_1):
    for k2, l2 in zip(key_2, list_2):
        if k1 == k2:
            list_a.append(l1)
            list_b.append(l2)
            key.append(k1)

# return assigned the OUT port
OUT = list_a, list_b, key
