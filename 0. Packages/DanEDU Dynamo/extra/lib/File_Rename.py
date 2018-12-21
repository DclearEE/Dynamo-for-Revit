# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Rename file
'''

# .Net module
from System.IO import File

# argument assigned the IN port
file_path = IN[0]
extension = IN[1]
exist_name = IN[2]
new_name = IN[3]

# core data processing
renamed = []
for e_name, n_name in zip(exist_name, new_name):
    exist_file = file_path + '\\' + e_name + '.' + extension
    new_file = file_path + '\\' + n_name + '.' + extension
    File.Move(exist_file, new_file)
    renamed.append(n_name + '.' + extension)

# return assigned the OUT port
OUT = renamed
