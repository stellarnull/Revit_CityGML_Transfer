#region Header
//
// CmdAnalyticalModelGeom.cs - retrieve analytical model geometry
//
// Copyright (C) 2011-2017 by Jeremy Tammik, Autodesk Inc. All rights reserved.
//
// Keywords: The Building Coder Revit API C# .NET add-in.
//
#endregion // Header

#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.PointClouds;
using System.IO;
using System.Xml;
using Autodesk.Revit.DB.Analysis;
using RvtElement = Autodesk.Revit.DB.Element;
using GeoElement = Autodesk.Revit.DB.GeometryElement;
#endregion // Namespaces

namespace TestRevit
{
    [Transaction(TransactionMode.Manual)]
    public class SlabBoundary: IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            List<Element> floors = new List<Element>();
            Selection sel = uiApp.ActiveUIDocument.Selection;
            if (0 < sel.Elements.Size)
            {
                foreach (Element e in sel.Elements)
                {
                    if (e is Floor)
                    {
                        floors.Add(e);
                    }
                }
                if (0 == floors.Count)
                {
                    message = "Please select some floor elements.";
                    return Result.Failed;
                }
            }
            else
            {
                doc.GetElements(typeof(Floor), floors);
                if (0 == floors.Count)
                {
                    message = "No floor elements found.";
                    return Result.Failed;
                }
            }

            List<List<XYZ>> polygons = new List<List<XYZ>>();
            Options opt = app.Create.NewGeometryOptions();

            foreach (Floor floor in floors)
            {
                GeoElement geo = floor.get_Geometry(opt);
                GeometryObjectArray objects = geo.Objects;
                foreach (GeometryObject obj in objects)
                {
                    Solid solid = obj as Solid;
                    if (solid != null)
                    {
                        GetBoundary(polygons, solid);
                    }
                }
            }

            return Result.Failed;
        }
    }
}
