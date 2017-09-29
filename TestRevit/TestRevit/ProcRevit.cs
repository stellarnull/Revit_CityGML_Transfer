using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using RvtSolid = Autodesk.Revit.DB.Solid;
using RvtCurve = Autodesk.Revit.DB.Curve;
using Application = Autodesk.Revit.ApplicationServices.Application;


using System.Windows.Forms;
using TestRevit.Model;
using System.IO;
using Autodesk.Revit.UI.Selection;
using System.Collections;

namespace TestRevit
{
    [Transaction(TransactionMode.Manual)]
    public class ProcRevit : IExternalCommand
    {
        MyBuilding curBuilding = new MyBuilding();
        bool flag = false;

        public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
        {
            bool modeAll = true;
            UIApplication uiApp = commandData.Application;
            Application app = uiApp.Application;
            Document doc = uiApp.ActiveUIDocument.Document;

            curBuilding.Walls = new List<MyWall>();
            curBuilding.Roofs = new List<MyRoof>();
            curBuilding.Floors = new List<MyFloor>();
            curBuilding.Ceilings = new List<MyCeiling>();

            if (modeAll)
            {
                FilteredElementCollector collectorWall = new FilteredElementCollector(doc);
                collectorWall.OfClass(typeof(Wall)).OfCategory(BuiltInCategory.OST_Walls);
                IList<Element> wallElemLists = collectorWall.ToElements();
                AnaWall(doc, wallElemLists);

                FilteredElementCollector collectorRoof = new FilteredElementCollector(doc);
                collectorRoof.OfClass(typeof(RoofBase)).OfCategory(BuiltInCategory.OST_Roofs);
                IList<Element> roofElemLists = collectorRoof.ToElements();
                AnaRoof(roofElemLists);

                FilteredElementCollector collectorFloor = new FilteredElementCollector(doc);
                collectorFloor.OfClass(typeof(Floor)).OfCategory(BuiltInCategory.OST_Floors);
                IList<Element> floorElemLists = collectorFloor.ToElements();
                AnaFloor(floorElemLists);

                FilteredElementCollector collectorCeiling = new FilteredElementCollector(doc);
                collectorCeiling.OfClass(typeof(Ceiling)).OfCategory(BuiltInCategory.OST_Ceilings);
                IList<Element> ceilingElemLists = collectorCeiling.ToElements();
                AnaCeiling(ceilingElemLists);
            }
            else
            {
                Document revitDoc = commandData.Application.ActiveUIDocument.Document;  //取得文档
                Application revitApp = commandData.Application.Application;             //取得应用程序
                UIDocument uiDoc = commandData.Application.ActiveUIDocument;

                //select sth. from UI
                Selection sel = uiDoc.Selection;
                Reference ref1 = sel.PickObject(ObjectType.Element);
                Element elem = revitDoc.GetElement(ref1);
                IList<Element> elemLists = new List<Element>();
                elemLists.Add(elem);

                MessageBox.Show(elem.GetType().ToString());

                AnaWall(doc, elemLists);
                AnaRoof(elemLists);
                AnaFloor(elemLists);
                AnaCeiling(elemLists);
            }
            
            //MessageBox.Show(Path.GetFullPath("./"));
            Utility.SaveData(curBuilding, "F:/grad_thesis/test/TestCityGML/TestCityGML/bin/Debug/output/test.json");

            MessageBox.Show(curBuilding.ToString());
            return Result.Succeeded;
        }

        public void AnaWall(Document doc, IList<Element> wallElemLists)
        {
            foreach (Element wallElem in wallElemLists)
            {
                Wall wall = wallElem as Wall;
                if (wall == null)
                {
                    MessageBox.Show("wall == null");
                    break;
                }
                Options opt = new Options();
                GeometryElement geomElem = wall.get_Geometry(opt);
                foreach (GeometryObject geomObj in geomElem)
                {
                    RvtSolid geomSolid = geomObj as RvtSolid;

                    if (null != geomSolid)
                    {
                        MyWall curWall = new MyWall();
                        curWall.Faces = new List<MyFace>();
                        
                        curWall.Windows = new List<MyWindow>();
                        curWall.Doors = new List<MyDoor>();
                        curWall.Description = procElem(wallElem);
                        IList<ElementId> wallInsertIds = wall.FindInserts(true, false, false, false);
                        foreach (ElementId curElemID in wallInsertIds)
                        {
                            //MessageBox.Show("window");
                            Element insertElem = doc.GetElement(curElemID);
                            FamilyInstance familyInstance = insertElem as FamilyInstance;
                            //MessageBox.Show(familyInstance.Category.Id.ToString().Equals(((int)BuiltInCategory.OST_Windows).ToString()).ToString());
                            if (familyInstance.Category.Id.ToString().Equals(((int)BuiltInCategory.OST_Windows).ToString()))
                            {
                                
                                MyWindow curWindow = AnaWindow(familyInstance);
                                if (curWindow != null)
                                {
                                    //MessageBox.Show("window");
                                    curWindow.Description = procElemFi(insertElem);
                                    MessageBox.Show(curWindow.Description);
                                    curWall.Windows.Add(curWindow);
                                }
                            }
                            if (familyInstance.Category.Id.ToString().Equals(((int)BuiltInCategory.OST_Doors).ToString()))
                            {
                                MyDoor curDoor = AnaDoor(familyInstance);
                                if (curDoor != null)
                                {
                                    curDoor.Description = procElemFi(insertElem);
                                    curWall.Doors.Add(curDoor);
                                } 
                            }
                        }
                        foreach (Face geomFace in geomSolid.Faces)
                        {
                            curWall.Faces.Add(getVecFromFace(geomFace));
                        }
                        curBuilding.Walls.Add(curWall);
                    }
                }
            }
        }

        public MyWindow AnaWindow(FamilyInstance fi)
        {
            Options opt = new Options();
            GeometryElement geomElem = fi.get_Geometry(opt);
            //MessageBox.Show((geomElem==null).ToString());//false //geomElem != null
            foreach (GeometryObject geomObj in geomElem)
            {
                MyWindow curWindow = new MyWindow();
                return curWindow;
            }
            return null;
        }

        public MyDoor AnaDoor(FamilyInstance fi)
        {
            Options opt = new Options();
            GeometryElement geomElem = fi.get_Geometry(opt);

            foreach (GeometryObject geomObj in geomElem)
            {
                MyDoor curDoor = new MyDoor();
                return curDoor;
            }
            return null;
        }

        public void AnaRoof(IList<Element> roofElemLists)
        {
            if (roofElemLists.Count() == 0)
            {
                MessageBox.Show("no ele from roof");
            }

            foreach (Element roofElem in roofElemLists)
            {
                RoofBase roof = roofElem as RoofBase;
                if (roof == null)
                {
                    //MessageBox.Show("roof == null");
                    break;
                }
                Options opt = new Options();
                GeometryElement geomElem = roof.get_Geometry(opt);

                foreach (GeometryObject geomObj in geomElem)
                {
                    RvtSolid geomSolid = geomObj as RvtSolid;

                    if (null != geomSolid)
                    {
                        MyRoof curRoof = new MyRoof();
                        curRoof.Faces = new List<MyFace>();
                        curRoof.Description = procElem(roofElem);
                        int roofFaceCount = 0;
                        foreach (Face geomFace in geomSolid.Faces)
                        {
                            curRoof.Faces.Add(getVecFromFace(geomFace));
                            roofFaceCount++;
                        }
                        //MessageBox.Show(roofFaceCount.ToString());
                        curBuilding.Roofs.Add(curRoof);
                    }
                }
            }
        }

        public void AnaFloor(IList<Element> floorElemLists)
        {
            if (floorElemLists.Count() == 0)
            {
                MessageBox.Show("no ele from floor");
            }

            foreach (Element floorElem in floorElemLists)
            {
                Floor floor = floorElem as Floor;
                if (floor == null)
                {
                    //MessageBox.Show("floor == null");
                    break;
                }
                Options opt = new Options();
                GeometryElement geomElem = floor.get_Geometry(opt);

                foreach (GeometryObject geomObj in geomElem)
                {
                    RvtSolid geomSolid = geomObj as RvtSolid;

                    if (null != geomSolid)
                    {
                        MyFloor curFloor = new MyFloor();
                        curFloor.Faces = new List<MyFace>();
                        curFloor.Description = procElem(floorElem);
                        int floorFaceCount = 0;
                        foreach (Face geomFace in geomSolid.Faces)
                        {
                            curFloor.Faces.Add(getVecFromFace(geomFace));
                            floorFaceCount++;
                        }
                        //MessageBox.Show(floorFaceCount.ToString());
                        curBuilding.Floors.Add(curFloor);
                    }
                }
            }
        }

        public void AnaCeiling(IList<Element> ceilingElemLists)
        {
            //select floors
            /*
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(Ceiling)).OfCategory(BuiltInCategory.OST_Ceilings);
            IList<Element> ceilingElemLists = collector.ToElements();
            */
            if (ceilingElemLists.Count() == 0)
            {
                MessageBox.Show("no ele from ceiling");
            }

            foreach (Element ceilingElem in ceilingElemLists)
            {
                Ceiling ceiling = ceilingElem as Ceiling;
                if (ceiling == null)
                {
                    //MessageBox.Show("ceiling == null");
                    break;
                }
                Options opt = new Options();
                GeometryElement geomElem = ceiling.get_Geometry(opt);

                foreach (GeometryObject geomObj in geomElem)
                {
                    RvtSolid geomSolid = geomObj as RvtSolid;

                    if (null != geomSolid)
                    {
                        MyCeiling curCeiling = new MyCeiling();
                        curCeiling.Faces = new List<MyFace>();
                        curCeiling.Description = procElem(ceilingElem);
                        int ceilingFaceCount = 0;
                        foreach (Face geomFace in geomSolid.Faces)
                        {
                            curCeiling.Faces.Add(getVecFromFace(geomFace));
                            ceilingFaceCount++;
                        }
                        //MessageBox.Show(floorFaceCount.ToString());
                        curBuilding.Ceilings.Add(curCeiling);
                    }
                }
            }
        }

        private MyFace getVecFromFace(Face geomFace)
        {
            MyFace curFace = new MyFace();
            flag = false;
            int count = 0;
            IList<CurveLoop> curveLoopList = geomFace.GetEdgesAsCurveLoops();
            /// curveLoop[0] is a CurveLoop Instance
            /// for continuous walls with angle one face will have 6 edges
            //geomFace.EdgeLoops.get_Item(0).get_Item(0).
            curFace.windowOpening = new List<double[]>();
            for (int loopCount = 0; loopCount < curveLoopList.Count; loopCount++)
            {
                VertexLookupXyz faceVertices_lookup = new VertexLookupXyz();
                foreach (RvtCurve c in curveLoopList[loopCount])
                {
                    
                    foreach (XYZ t in c.Tessellate())
                    {
                        faceVertices_lookup.AddVertex(t);
                    }
                }
                count++;
                if (curveLoopList[loopCount].IsCounterclockwise(geomFace.ComputeNormal(new UV(0.1,0.1))))
                //if (count == 1)
                {
                    curFace.Vertices = vec2list(faceVertices_lookup.Keys, false);
                }
                else
                {
                    curFace.windowOpening.Add(vec2list(faceVertices_lookup.Keys, true));
                }
            }
            
            if (curveLoopList.Count > 1)
            {
                //MessageBox.Show(curveLoopList.Count.ToString());
                flag = true;
            }
            return curFace;
        }

        private double[] vec2list(Dictionary<XYZ, int>.KeyCollection s, bool reverse)
        {
            IEnumerable<XYZ> newS;
            if (reverse == true)
            {
                newS = s.Reverse();
            }
            else
            {
                newS = s;
            }
            
            List<double> faceVerticesList = new List<double>();
            foreach (XYZ point in newS)
            {
                faceVerticesList.Add(point.X);
                faceVerticesList.Add(point.Y);
                faceVerticesList.Add(point.Z);
            }
            return faceVerticesList.ToArray();
        }


        public string procElem(Element elem)
        {
            string info = "";
            Options opt = new Options();
            GeometryElement geomElem = elem.get_Geometry(opt);
            //MessageBox.Show((geomElem==null).ToString());

            if (geomElem != null)
            {
                //info += "has geom \n";
                //MessageBox.Show("true");
                foreach (GeometryObject geomObj in geomElem)
                {
                    RvtSolid geomSolid = geomObj as RvtSolid;
                    if (null != geomSolid)
                    {
                        //info += "has solid \n";
                        if (geomSolid.Volume > 0)
                        {
                            //info += "volume > 0 \n";
                            //count++;
                            //MessageBox.Show(geomSolid.Volume.ToString());
                            //objs[i].Category.Name   //栏杆etc.
                            //objs[i].GetType().FullName;    //FamilySymbol
                            //objs[i].ToString() //FamilySymbol
                            //objs[i].Name;  //20mm 
                            //info += elem.Category.Name + ", volume: " + geomSolid.Volume.ToString() + "\n";
                            ParameterSet paraSet = elem.Parameters;
                            int count = 0; 
                            foreach (Parameter para in paraSet)
                            {
                                count++;
                                info += para.Definition.Name + "='" + para.AsValueString() + "'";
                                if (count < paraSet.Size)
                                {
                                    info += ", ";
                                }
                            }
                            //MessageBox.Show(info);
                        }
                    }
                }
            }
            return info;
        }

        public string procElemFi(Element elem)
        {
            string info = "";
            Options opt = new Options();
            GeometryElement geomElem = elem.get_Geometry(opt);
            //MessageBox.Show((geomElem==null).ToString());
            
            if (geomElem != null)
            {
                //info += "has geom \n";
                //MessageBox.Show("fi");

                //count++;
                //MessageBox.Show(geomSolid.Volume.ToString());
                //objs[i].Category.Name   //栏杆etc.
                //objs[i].GetType().FullName;    //FamilySymbol
                //objs[i].ToString() //FamilySymbol
                //objs[i].Name;  //20mm 
                //info += elem.Category.Name + "\n";// + ", volume: " + geomSolid.Volume.ToString() + "\n";
                ParameterSet paraSet = elem.Parameters;
                int count = 0;
                foreach (Parameter para in paraSet)
                {
                    count++;
                    info += para.Definition.Name + "='" + para.AsValueString() + "'";
                    if (count < paraSet.Size)
                    {
                        info += ", ";
                    }
                }
            }
            return info;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class Topo : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Application app = uiApp.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            TopoGraph buildingTopo = new TopoGraph();

            //FilteredElementCollector collectorDoor = new FilteredElementCollector(doc);
            //collectorDoor.OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Doors);
            //IList<Element> doorLists = collectorDoor.ToElements();

            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            //select sth. from UI
            Selection sel = uiDoc.Selection;
            IList<Element> doorsSelected = sel.PickElementsByRectangle(new MySelectionFilter());
            //MessageBox.Show(elemsSelected.Count.ToString());

            MessageBox.Show(doorsSelected.Count.ToString());

            foreach (Element doorElement in doorsSelected)
            {
                buildingTopo = anaTopo(doorElement, buildingTopo);
            }
            Utility.WriteTXT(buildingTopo.createGraphData(), "F:/grad_thesis/test/Neo4jTest/graph.txt");
            MessageBox.Show(buildingTopo.ToString());

            return Result.Succeeded;

        }

        private TopoGraph anaTopo(Element elem, TopoGraph buildingTopo)
        {
            FamilyInstance fi = elem as FamilyInstance;
            Edge tempEdge = new Edge();
            string tempId;

            Room toRoom = fi.ToRoom;
            Vertex toVertex;
            if (toRoom == null)
            {
                tempId = "outside";
            }
            else
            {
                tempId = toRoom.Name.Replace(" ", "") + "_" + toRoom.Id.ToString();
            }
            //MessageBox.Show((toRoom==null).ToString());
            toVertex = new Vertex(tempId);
            if (!buildingTopo.Rooms.ContainsKey(tempId))
            {
                buildingTopo.Rooms.Add(tempId, toVertex);
            }
            tempEdge.FromTo[0] = toVertex;

            Room fromRoom = fi.FromRoom;
            Vertex fromVertex;
            if (fromRoom == null)
            {
                tempId = "outside";
            }
            else
            {
                tempId = fromRoom.Name.Replace(" ", "") + "_" + fromRoom.Id.ToString();
            }
            fromVertex = new Vertex(tempId);
            if (!buildingTopo.Rooms.ContainsKey(tempId))
            {
                buildingTopo.Rooms.Add(tempId, fromVertex);
            }
            tempEdge.FromTo[1] = fromVertex;

            buildingTopo.Connections.Add(tempEdge);

            return buildingTopo;
        }
    }

    
}

