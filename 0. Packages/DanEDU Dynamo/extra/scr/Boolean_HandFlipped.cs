// This file is covered by the LICENSE in this dynamo package 'extra' folder
/*
Get the element hand flip condition, output as boolean
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

public static IList HandFlipped(IList element)
{
    List<object> flipped = new List<object>();

    //Autodesk.Revit.DB.Document Doc = DocumentManager.Instance.CurrentDBDocument;
    //Autodesk.Revit.DB.Element elem = Doc.GetElement(elem.UniqueId.ToString());

    foreach (Revit.Elements.Element elem in element)
    {
        Autodesk.Revit.DB.FamilyInstance item = Elem(elem) as Autodesk.Revit.DB.FamilyInstance;
        if (item.HandFlipped != item.FacingFlipped)
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

public IList tHandFlipped()
{
    // argument assigned the IN port
    var element = (IList)IN[0];

    // return assigned the OUT port
    return HandFlipped(element);
}

// Invoke the script (method)
tHandFlipped()

#endregion
