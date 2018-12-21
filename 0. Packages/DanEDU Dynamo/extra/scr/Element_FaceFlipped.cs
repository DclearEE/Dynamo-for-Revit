// This file is covered by the LICENSE in this dynamo package 'extra' folder
/*
Get the element face flip condition, output as element
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

public static IDictionary FaceFlipped(IList element)
{
    List<object> flippedTrue = new List<object>();
    List<object> flippedFalse = new List<object>();

    foreach (Revit.Elements.Element elem in element)
    {
        Autodesk.Revit.DB.FamilyInstance item = Elem(elem) as Autodesk.Revit.DB.FamilyInstance;
        if (item.FacingFlipped)
        {
            flippedTrue.Add(elem);
        }
        else
        {
            flippedFalse.Add(elem);
        }
    }
    return new Dictionary<string, object>
            {
                {"True", flippedTrue},
                {"False", flippedFalse}
            };
}

#region Script section for using CSharpScript.FromString

public IList tFaceFlipped()
{
    // argument assigned the IN port
    var element = (IList)IN[0];

    // cast IDictionary to IList
    var idict = FaceFlipped(element);
    var dict = (Dictionary<string, object>)idict;
    var ilist = new List<object>(dict.Values.ToList());

    // return assigned the OUT port
    return ilist;
}

// Invoke the script (method)
tFaceFlipped()

#endregion
