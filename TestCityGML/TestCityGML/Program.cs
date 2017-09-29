using org.citygml4j;
using org.citygml4j.builder;
using org.citygml4j.factory;
using org.citygml4j.model.citygml;
using org.citygml4j.model.citygml.building;
using org.citygml4j.model.citygml.core;
using org.citygml4j.model.gml.geometry.aggregates;
using org.citygml4j.model.gml.geometry.complexes;
using org.citygml4j.model.gml.geometry.primitives;
using org.citygml4j.model.module;
using org.citygml4j.model.module.citygml;
using org.citygml4j.util.gmlid;
using org.citygml4j.xml.io;
using org.citygml4j.xml.io.reader;
using org.citygml4j.xml.io.writer;
using org.citygml4j.model.gml.@base;
using Console = System.Console;
using GmlSolid = org.citygml4j.model.gml.geometry.primitives.Solid;
using java.io;
using java.util;
using System;
using System.Collections.Generic;
using SysIO = System.IO;
using TestRevit.Model;

namespace TestCityGML
{
    class Program
    {
        
        static void Main(string[] args)
        {
            //simple_citygml_reader();
            //write_citygml();
            create_citygml_fromJSON();

            Console.ReadKey();
        }


    #region reading citygml
    static void simple_citygml_reader()
        {
            Console.WriteLine("{0} setting up citygml4j context and JAXB builder", DateTime.Now.ToShortTimeString());
            //TaskDialog.Show("Revit_gml", "i'm alive");
            CityGMLContext ctx = new CityGMLContext();
            CityGMLBuilder builder = ctx.createCityGMLBuilder();

            Console.WriteLine("{0} reading CityGML file LOD2_Buildings_v100.gml completely into main memory", DateTime.Now.ToShortTimeString());

            CityGMLInputFactory input = builder.createCityGMLInputFactory();
            //CityGMLReader reader = input.createCityGMLReader(new File("../datasets/LOD2_Buildings_v100.gml"));
            CityGMLReader reader = input.createCityGMLReader(new File("../datasets/b1_lod1_s_2.gml"));

            while (reader.hasNext())
            {
                CityGML citygml = reader.nextFeature();

                Console.WriteLine("Found " + citygml.getCityGMLClass() +
                        " version " + citygml.getCityGMLModule().getVersion());

                if (citygml.getCityGMLClass() == CityGMLClass.CITY_MODEL)
                {
                    CityModel cityModel = (CityModel)citygml;

                    Console.WriteLine("{0} going through city model and counting building instances", DateTime.Now.ToShortTimeString());

                    int count = 0;     // building count
                    foreach (CityObjectMember cityObjectMember in cityModel.getCityObjectMember().toArray())
                    {
                        AbstractCityObject cityObject = cityObjectMember.getCityObject();
                        if (cityObject.getCityGMLClass() == CityGMLClass.BUILDING)
                            count++;
                    }

                    Console.WriteLine("The city model contains " + count + " building features");
                }
            }

            reader.close();
            Console.WriteLine("{0} sample citygml4j application successfully finished", DateTime.Now.ToShortTimeString());
        }

        #endregion

        static void create_citygml_fromJSON()
        {
            Console.WriteLine("{0} setting up citygml4j context and JAXB builder", DateTime.Now.ToShortTimeString());
            CityGMLContext ctx = new CityGMLContext();
            CityGMLBuilder builder = ctx.createCityGMLBuilder();

            Console.WriteLine("{0} creating LOD2 building as citygml4j in-memory object tree", DateTime.Now.ToShortTimeString());
            GMLGeometryFactory geom = new GMLGeometryFactory();

            Console.WriteLine(SysIO.Path.GetFullPath("./"));




            MyBuilding curBuilding = Utility.LoadData<MyBuilding>("./output/test.json");
            //Console.WriteLine(curBuilding.Walls.Count);

            GMLIdManager gmlIdManager = DefaultGMLIdManager.getInstance();
            Building building = new Building();
            CompositeSolid compositeSolid = new CompositeSolid();
            List boundedBy = new ArrayList();
            List<MyWall> curWalls = curBuilding.Walls;
            List<MyRoof> curRoofs = curBuilding.Roofs;
            List<MyFloor> curFloors = curBuilding.Floors;
            List<MyCeiling> curCeilings = curBuilding.Ceilings;

            List surfaceMember = new ArrayList();

            
            #region
            //proc walls
            for (int i = 0; i < curWalls.Count; i++)
            {
                //List<Polygon> poly = new List<Polygon>();
                MyWall curWall = curWalls[i];

                for (int j = 0; j < curWall.Faces.Count; j++)
                {
                    //poly: wall exterior poly
                    MyFace curFace = curWall.Faces[j];
                    //if (curFace.windowOpening.Count == 0) continue;
                    StringOrRef sor = new StringOrRef();
                    Polygon poly = new Polygon();
                    if (curFace.Vertices != null)
                    {
                        poly = geom.createLinearPolygon(curFace.Vertices, 3);
                        sor.setValue(curWall.Description);
                        poly.setDescription(sor);
                        poly.setId(gmlIdManager.generateUUID());
                    }
                  
                    for (int loopCount = 0; loopCount < curFace.windowOpening.Count; loopCount++)
                    {
                        if (loopCount + 1 <= curWall.Windows.Count)
                        {
                            MyWindow curWindow = curWall.Windows[loopCount];
                            LinearRing interiorRing = new LinearRing();
                            sor.setValue(curWindow.Description);
                            interiorRing.setDescription(sor);
                            interiorRing.setPosList(geom.createDirectPositionList(curFace.windowOpening[loopCount], 3));
                            poly.addInterior(new Interior(interiorRing));
                            //boundedBy.add(createBoundarySurface(CityGMLClass.BUILDING_WINDOW, poly));
                        }

                    }
                    //Console.WriteLine(poly.isSetExterior());
                    //Console.WriteLine(poly.isSetInterior());
                    surfaceMember.add(new SurfaceProperty('#' + poly.getId()));
                    boundedBy.add(createBoundarySurface(CityGMLClass.BUILDING_WALL_SURFACE, poly));

                }
            }
            #endregion

            #region
            
            #region
            //proc floors
            for (int i = 0; i < curFloors.Count; i++)
            {

                MyFloor curFloor = curFloors[i];
                for (int j = 0; j < curFloor.Faces.Count; j++)
                {

                    MyFace curFace = curFloor.Faces[j];
                    StringOrRef sor = new StringOrRef();
                    Polygon poly = new Polygon();
                    if (curFace.Vertices != null)
                    {
                        poly = geom.createLinearPolygon(curFace.Vertices, 3);
                        sor.setValue(curFloor.Description);
                        poly.setDescription(sor);
                        poly.setId(gmlIdManager.generateUUID());
                    }
                    surfaceMember.add(new SurfaceProperty('#' + poly.getId()));
                    boundedBy.add(createBoundarySurface(CityGMLClass.BUILDING_FLOOR_SURFACE, poly));
                    createBoundarySurface(CityGMLClass.BUILDING_FLOOR_SURFACE, poly);
                }
            }
            #endregion

            #region
            //proc ceilings
            for (int i = 0; i < curCeilings.Count; i++)
            {
                MyFloor curCeiling = curFloors[i];
                for (int j = 0; j < curCeiling.Faces.Count; j++)
                {

                    MyFace curFace = curCeiling.Faces[j];
                    StringOrRef sor = new StringOrRef();
                    Polygon poly = new Polygon();
                    if (curFace.Vertices != null)
                    {
                        poly = geom.createLinearPolygon(curFace.Vertices, 3);
                        sor.setValue(curCeiling.Description);
                        poly.setDescription(sor);
                        poly.setId(gmlIdManager.generateUUID());
                    }
                    surfaceMember.add(new SurfaceProperty('#' + poly.getId()));
                    boundedBy.add(createBoundarySurface(CityGMLClass.BUILDING_CEILING_SURFACE, poly));
                    createBoundarySurface(CityGMLClass.BUILDING_CEILING_SURFACE, poly);
                }
            }
            #endregion

            #region
            //proc roofs
            for (int i = 0; i < curRoofs.Count; i++)
            {
                MyFloor curRoof = curFloors[i];
                for (int j = 0; j < curRoof.Faces.Count; j++)
                {

                    MyFace curFace = curRoof.Faces[j];
                    StringOrRef sor = new StringOrRef();
                    Polygon poly = new Polygon();
                    if (curFace.Vertices != null)
                    {
                        poly = geom.createLinearPolygon(curFace.Vertices, 3);
                        sor.setValue(curRoof.Description);
                        poly.setDescription(sor);
                        poly.setId(gmlIdManager.generateUUID());
                    }
                    surfaceMember.add(new SurfaceProperty('#' + poly.getId()));
                    boundedBy.add(createBoundarySurface(CityGMLClass.BUILDING_ROOF_SURFACE, poly));
                    createBoundarySurface(CityGMLClass.BUILDING_ROOF_SURFACE, poly);
                }
            }
            #endregion
        
            #endregion

            CompositeSurface compositeSurface = new CompositeSurface();
            compositeSurface.setSurfaceMember(surfaceMember);
            GmlSolid solid = new GmlSolid();
            solid.setExterior(new SurfaceProperty(compositeSurface));
            compositeSolid.addSolidMember(new SolidProperty(solid));

            building.setLod4Solid(new SolidProperty(compositeSolid));
            building.setBoundedBySurface(boundedBy);

            CityModel cityModel = new CityModel();
            cityModel.setBoundedBy(building.calcBoundedBy(false));
            cityModel.addCityObjectMember(new CityObjectMember(building));

            Console.WriteLine("{0} writing citygml4j object tree");
            CityGMLOutputFactory output = builder.createCityGMLOutputFactory(CityGMLVersion.DEFAULT);
            CityGMLWriter writer = output.createCityGMLWriter(new File("test.gml"));

            writer.setPrefixes(CityGMLVersion.DEFAULT);
            writer.setSchemaLocations(CityGMLVersion.DEFAULT);
            writer.setIndentString("  ");
            writer.write(cityModel);
            writer.close();

            Console.WriteLine("{0} CityGML file written", DateTime.Now.ToShortTimeString());
            Console.WriteLine("{0} sample citygml4j application successfully finished", DateTime.Now.ToShortTimeString());
        }

        private static BoundarySurfaceProperty createBoundarySurface(CityGMLClass type, Polygon geometry)
        {
            AbstractBoundarySurface boundarySurface = null;

            //            switch(type) {
            //                case :
            //                    break;
            //                case BUILDING_ROOF_SURFACE:
            //                    break;
            //                case BUILDING_GROUND_SURFACE:
            //                    boundarySurface=new GroundSurface();
            //                    break;
            //                default:
            //                    break;
            //            }
            if (type == CityGMLClass.BUILDING_WALL_SURFACE)
            {
                boundarySurface = new WallSurface();
            }
            else if (type == CityGMLClass.BUILDING_ROOF_SURFACE)
            {
                boundarySurface = new RoofSurface();
            }
            else if (type == CityGMLClass.BUILDING_GROUND_SURFACE)
            {
                boundarySurface = new GroundSurface();
            }
            else if (type == CityGMLClass.BUILDING_FLOOR_SURFACE)
            {
                boundarySurface = new FloorSurface();
            }
            else if (type == CityGMLClass.BUILDING_CEILING_SURFACE)
            {
                boundarySurface = new CeilingSurface();
            }
            else if (type == CityGMLClass.BUILDING_WINDOW)
            {
                boundarySurface = new ClosureSurface();
                //Console.WriteLine("ClosureSurface");
            }

            if (boundarySurface != null)
            {
                boundarySurface.setLod4MultiSurface(new MultiSurfaceProperty(new MultiSurface(geometry)));
                return new BoundarySurfaceProperty(boundarySurface);
            }

            return null;
        }

        private static void setContext(AbstractCityGMLWriter writer,
            ModuleContext moduleContext,
            FeatureWriteMode writeMode,
            bool splitOnCopy)
        {
            writer.setPrefixes(moduleContext);
            writer.setPrefix("noise", "http://www.citygml.org/ade/noise_de/2.0");
            writer.setDefaultNamespace(moduleContext.getModule(CityGMLModuleType.CORE));
            writer.setSchemaLocation("http://www.citygml.org/ade/noise_de/2.0", "../../datasets/schemas/CityGML-NoiseADE-2_0_0.xsd");
            writer.setIndentString("  ");
            writer.setHeaderComment("written by citygml4j",
                    "using a CityGMLWriter instance",
                    "Split mode: " + writeMode,
                    "Split on copy: " + splitOnCopy);
        }
    }
}
