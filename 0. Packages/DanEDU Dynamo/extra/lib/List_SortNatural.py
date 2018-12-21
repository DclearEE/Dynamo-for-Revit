# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Sort elements using the PC 'locale'. Sort elements recursively
preserving elements types and elements structure. Sort elements in natural
sort order by splitting digits from string and add leading zeros.
<http://en.wikipedia.org/wiki/Natural_sort_order>
'''

# Set sys.path
import sys
sys.path.insert(0, 'C:\Program files (x86)\IronPython 2.7\Lib')

# Python modules
import re
import locale

# set locale character map, comment out when unit testing
locale.setlocale(locale.LC_ALL, locale.getdefaultlocale()[0])

# argument assigned the IN port, comment out below when unit testing
elements = IN[0]  # input elements
sort_descending = IN[1]  # descending, default false

# key builder -> return strings, numbers and objects
# splitting digits from string.
def sort_key(var, temp_key=str()):
    if isinstance(var, basestring):
        keys = re.split('([0-9]+)', var)
        for key in keys:
            if key.isdigit():
                temp_key += key.zfill(5)  # add padding to digits
            else:
                temp_key += key  # add basestring
        return temp_key.lower()
    else:
        return var

# cmp builder -> return strings
def sort_cmp(var):
    if isinstance(var, basestring):
        return var
    else:
        return str(var)

# recursive natural sort mechanism
def sort_recursive(_elements):
    # split into sub-lists and regular elements
    temp_sub_list = []  # temp list of sub-lists
    temp_list = []  # list of regular elements
    for element in _elements:
        if isinstance(element, list):
            temp_sub_list.append(element)
        else:
            temp_list.append(element)

    # sort elements in this list
    temp_list.sort(cmp=lambda a, b: locale.strcoll(
        sort_cmp(a), sort_cmp(b)), key=sort_key, reverse=sort_descending)

    # recursively sort sub_lists and add to list
    for sub_list in temp_sub_list:
        temp_list.append(sort_recursive(sub_list))
    return temp_list

# return assigned the OUT port, comment out below when unit testing
OUT = sort_recursive(elements)


''' # unit testing
import unittest
sort_descending = False
class NaturalSortTests(unittest.TestCase):
    def test_sort_flat_list(self):
        test_input = ['Elm10', 'elm9', 'elm1', 'elm0', 'Elm 2', 'A 10', 'a2']
        test_output = ['A 10', 'a2', 'Elm 2', 'elm0', 'elm1', 'elm9', 'Elm10']
        self.assertEqual(sort_recursive(test_input), test_output)

    def test_sort_nested_list_simple(self):
        test_input = ['Elm 1', ['Elm 1.1']]
        test_output = ['Elm 1', ['Elm 1.1']]
        self.assertEqual(sort_recursive(test_input), test_output)

    def test_sort_nested_list_complex(self):
        test_input = [['elm0', 'Elm 2', 'elm10', 'elm1', 'elm9', ['olm0',
            'olm10', 'olm1', 'Olm 2', 'olm9']], ['Elm11', 'alm12', 'elm13']]
        test_output = [['Elm 2', 'elm0', 'elm1', 'elm9', 'elm10', ['Olm 2',
            'olm0', 'olm1', 'olm9', 'olm10']], ['alm12', 'Elm11', 'elm13']]
        self.assertEqual(sort_recursive(test_input), test_output)

unittest.main() '''
