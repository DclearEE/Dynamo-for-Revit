# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Utilities and minor procedures
'''


# used in: Sigma_WriteData
def Flatten(lst):
    """
    flatten nD list
    """   
    for item in lst:
        if isinstance(item, list):
            for i in Flatten(item):
                yield i
        else:
            yield item


# used in: List_Clean
def Clean(lst, _search):
    """
    cleaning nD list
    """   
    for (idx, item) in enumerate(lst):
        if isinstance(item, list):
            lst[idx] = Clean(item, _search)
    return [item for item in lst if item not in Search(_search)]


# used in: List_Replace
def Replace(lst, _search, _replace):
    """
    replacing nD list
    """  
    for (idx, item) in enumerate(lst):
        if isinstance(item, list):
            lst[idx] = Replace(item, _search, _replace)
    return [item if item not in Search(_search) else _replace for item in lst]


# used in: List_Clean, List_Replace
def Search(item):
    """
    search values, send default if values not exist
    """
    if not isinstance(item, list):
        return [item]
    elif not item:
        return [[]]
    else:
        return item


# used in: FamilyInstance_ByFamilyName, Element_ByCategory
def SingleOut(lst):
    """
    Return list zero if list length is one.

    :lst: input list of lists
    """
    if len(lst) == 1:
        return lst[0]
    else:
        return lst
