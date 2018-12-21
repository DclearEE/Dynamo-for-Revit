# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Get index of item in nD list
'''

# argument assigned the IN port
elements = IN[0]
search = IN[1]

# value convert int and float to string (into to make it iterable)
if isinstance(search, (int, float)):
    search = str(search)

# list convert int and float to string (into to make it iterable)
def str_val(lst):
    for (idx, item) in enumerate(lst):
        if isinstance(item, list):
            lst[idx] = str_val(item)
    return [str(item) if isinstance(item, (int, float)) else item for item in lst]

def find_index(lst, var, indices=[]):
    index = []
    for idx, items in enumerate(lst):
        if items == var:
            current_index = indices + [idx]
            index.append(current_index)
        else:
            if not any(isinstance(i, list) for i in items):
                try:
                    current_index = indices + [idx]
                    for i, val in enumerate(items):
                        if val == var:
                            index.append(current_index + [i])
                except BaseException:
                    pass

            for item in items:
                if isinstance(item, list):
                    indices.append(idx)
                    index.extend(find_index(items, var, indices[:]))
    return index

# return assigned the OUT port
OUT = find_index(str_val(elements), search)
