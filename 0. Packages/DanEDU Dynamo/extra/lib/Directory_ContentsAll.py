# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Get all content from directory,
Takes only content that are not read only and hidden
'''

# .Net module
from System.IO import Directory, File, FileAttributes, Path, SearchOption

# argument assigned the IN port
folders = IN[0]
search = IN[1]
deep = IN[2]

# instantiate error log
error_log = None

files = []
try:
    # determine if deepsearch from current directory is needed
    if deep:
        docs = Directory.GetFiles(folders, search, SearchOption.AllDirectories)
    else:
        docs = Directory.GetFiles(folders, search)

    # get only docs that are not read-only and hidden (and thereby system etc.)
    ro = FileAttributes.ReadOnly
    hi = FileAttributes.Hidden

    for doc in docs:
        if File.GetAttributes(doc).HasFlag(ro) == 0:
            if File.GetAttributes(doc).HasFlag(hi) == 0:
                files.append(Path.GetFullPath(doc))
        else:
            error_log = 'File(s) are read only, correct input!'
except BaseException:
    error_log = 'Directory does not exist, correct input!'

# return assigned the OUT port
if error_log:
    files = error_log
OUT = files
