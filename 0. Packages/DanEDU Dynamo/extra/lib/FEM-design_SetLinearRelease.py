# coding=utf-8
# This file is covered by the LICENSE in this dynamo package 'extra' folder
'''
Write (set) linear release in FEM-design xml-file
'''

# Set sys.path
import sys
sys.path.insert(0, 'C:\Program files (x86)\IronPython 2.7\Lib')

# Python modules
import os
import traceback

# Python and DanEDU module
import math
import xml.etree.ElementTree as ET
ET.register_namespace('','urn:strusoft')

# argument assigned the IN port
file_ = IN[0]
elements = IN[1]
ids = IN[2]
rvt_releases = IN[3]
rvt_edges = IN[4]
rvt_Zaxis = IN[5]

unwrapped_elements = list(UnwrapElement(elements))

# instantiate error log
error_log = None


# group two lines into pairs, with the corresponding edge from the XML file
def pairs(values, edges, guids, releases):
    return zip(values[::2], values[1::2], edges[::2], guids[::2], releases[::2])

# set neg/pos values
def release(edge, var, value):
    for e in edge.iter('{urn:strusoft}' + var):
        e.set('neg', str(value))
        e.set('pos', str(value))
        return str(e.attrib)

# set stiffness values, based on the edge
def stiffness(edge, mov_x, mov_y, mov_z, rot_x, rot_y, rot_z):
    t1, t2, t3 = ['mov_x'], ['mov_y'], ['mov_z']
    t4, t5, t6 = ['rot_x'], ['rot_y'], ['rot_z']
    t1.append(release(edge, 'mov_x', mov_x))
    t2.append(release(edge, 'mov_y', mov_y))
    t3.append(release(edge, 'mov_z', mov_z))
    t4.append(release(edge, 'rot_x', rot_x))
    t5.append(release(edge, 'rot_y', rot_y))
    t6.append(release(edge, 'rot_z', rot_z))
    return t1, t2, t3, t4, t5, t6

# set release values, based on the edge
def xml_release(edge, index):
    if wall[4][index].lower() == 'hinged':
        return stiffness(edge, 10000000, 10000000, 10000000, 0, 0, 0)
    if wall[4][index].lower() == 'free in z':
        return stiffness(edge, 10000000, 10000000, 0, 0, 0, 0)
    if wall[4][index].lower() == 'free':
        return stiffness(edge, 0, 0, 0, 0, 0, 0)
    # if wall[4][index] == 'Rigid' or (wall[4][index] == '' or wall[4][index] is None):
    else:  # last option -> Rigid
        connections = edge.find('{urn:strusoft}edge_connection')
        edge.remove(connections)
        return None


# core data processing
try:
    # read the XML file
    with open(file_, 'r') as i:
        root = ET.parse(i).getroot()
    i.close()

    entities = root.find('{urn:strusoft}entities')
    slabs = root.find('{urn:strusoft}entities/{urn:strusoft}slab')

    # find x, y and z values for each point in each edge in the XML file
    values, edges, guids, releases = [], [], [], []
    for i, rvt_release in zip(ids, rvt_releases):
        for slab in entities.iter('{urn:strusoft}slab'):
            if i == slab.get('guid'):
                for contour in slab.iter('{urn:strusoft}contour'):
                    for edge in contour.iter('{urn:strusoft}edge'):
                        for point in edge.iter('{urn:strusoft}point'):
                            xml_x = round(float(point.get('x')), 3)
                            xml_y = round(float(point.get('y')), 3)
                            xml_z = round(float(point.get('z')), 3)
                            xml_xyz = ([xml_x, xml_y, xml_z])
                            values.append(xml_xyz)
                            edges.append([edge])
                            guids.append([slab.get('guid')])
                            releases.append(rvt_release)

    # group points into pairs of two for each line in an edge
    line_xml = pairs(values, edges, guids, releases)

    # check each lines set of two points. If the z-values are equal, then
    # the line is horizontal and belongs to either top or bottom if they dont
    # match, then the line is vertical and belongs to either left or right
    horizontal, vertical = [], []
    for line in line_xml:
        for idx, (point1, point2) in enumerate(zip(line[0], line[1])):
            if idx == 2 and point1 == point2:
                horizontal.append(line)
            elif idx == 2 and point1 != point2:
                vertical.append(line)

    # find matching values for each wall in the revit file, and the XML file
    top, bottom = [], []
    for wall in rvt_Zaxis:
        for line in horizontal:
            if line[3] == wall[1] and line[0][2] == wall[0][0]:
                bottom.append(line)
            elif line[3] == wall[1] and line[0][2] == wall[0][1]:
                top.append(line)

    rvt_vertical = []
    for e, i in zip(unwrapped_elements, ids):
        for rvt_edge in rvt_edges:
            if [i] == rvt_edge[1]:
                if e.Flipped:
                    rvt_edge = [rvt_edge[0][1], rvt_edge[0][0]], rvt_edge[1]
                    rvt_vertical.append(rvt_edge)
                else:
                    rvt_vertical.append(rvt_edge)

    left, right = [], []
    for line in vertical:
        for rvt_line in rvt_vertical:
            if line[3] == rvt_line[1]:
                if [line[0][0], line[0][1]] == rvt_line[0][0]:
                    right.append(line)
                if [line[0][0], line[0][1]] == rvt_line[0][1]:
                    left.append(line)

    # set output release values
    releases = []
    for wall in top:
        index = 0
        edge = wall[2][0]
        releases.append(xml_release(edge, index))
        if releases[-1] is None:
            del releases[-1]

    for wall in bottom:
        index = 1
        edge = wall[2][0]
        releases.append(xml_release(edge, index))
        if releases[-1] is None:
            del releases[-1]

    for wall in right:
        index = 2
        edge = wall[2][0]
        releases.append(xml_release(edge, index))
        if releases[-1] is None:
            del releases[-1]

    for wall in left:
        index = 3
        edge = wall[2][0]
        releases.append(xml_release(edge, index))
        if releases[-1] is None:
            del releases[-1]

    # write to the XML file.
    tree = ET.ElementTree(root)
    with open(file_, 'w') as f:
        tree.write(f, xml_declaration=True, encoding='utf-8', method='xml')
    f.close()

    # set result output
    result = releases

except BaseException:
    err = 'File or Path is wrong/missing!\nPlease veryfy settings'
    error_log = err + traceback.format_exc()
    raise StandardError(error_log)

# return assigned the OUT port
if error_log:
    result = error_log
OUT = result
