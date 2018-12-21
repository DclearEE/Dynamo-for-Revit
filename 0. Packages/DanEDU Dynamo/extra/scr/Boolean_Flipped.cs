// This file is covered by the LICENSE in this dynamo package 'extra' folder
/*
Get the element flip condition, output as boolean
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Revit.Elements;
using RevitServices.Persistence;
using Autodesk.Revit.DB;

internal static Document Doc
{
    get { return DocumentManager.Instance.CurrentDBDocument; }
}

internal static Autodesk.Revit.DB.Element Elem(Revit.Elements.Element elem)
{
    return Doc.GetElement(elem.UniqueId.ToString());
}

public static IList Flipped(IList element)
{
    List<object> flipped = new List<object>();

    foreach (Revit.Elements.Element elem in element)
    {
        Autodesk.Revit.DB.Wall item = Elem(elem) as Autodesk.Revit.DB.Wall;
        if (item.Flipped)
        {
            flipped.Add(true);
        }
        else
        {
            flipped.Add(false);
        }
    }
    return flipped;
}

#region Script section for using CSharpScript.FromString

public IList tFlipped()
{
    // argument assigned the IN port
    var element = (IList)IN[0];

    // return assigned the OUT port
    return Flipped(element);
}

// Invoke the script (method)
tFlipped()

#endregion
