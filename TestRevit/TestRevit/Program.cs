using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using java.io;
using java.util;
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
using Console = System.Console;

namespace citygmlSamples
{
    class Program
    {
        static void Main(string[] args)
        {
            simple_citygml_reader();
            //write_citygml();
            //create_citygml();

            Console.ReadKey();
        }


        #region reading citygml
        static void simple_citygml_reader()
        {
            Console.WriteLine("{0} setting up citygml4j context and JAXB builder", DateTime.Now.ToShortTimeString());
            CityGMLContext ctx = new CityGMLContext();
            CityGMLBuilder builder = ctx.createCityGMLBuilder();

            Console.WriteLine("{0} reading CityGML file LOD2_Buildings_v100.gml completely into main memory", DateTime.Now.ToShortTimeString());

            CityGMLInputFactory input = builder.createCityGMLInputFactory();
            //CityGMLReader reader = input.createCityGMLReader(new File("../datasets/LOD2_Buildings_v100.gml"));
            CityGMLReader reader = input.createCityGMLReader(new File("../datasets/b1_lod1_s_2.gml"));

            while (reader.hasNext())
            {
                CityGML citygml = reader.nextFeature();

                Console.WriteLine("Found "+citygml.getCityGMLClass()+
                        " version "+citygml.getCityGMLModule().getVersion());

                if(citygml.getCityGMLClass()==CityGMLClass.CITY_MODEL) {
                    CityModel cityModel = (CityModel)citygml;

                    Console.WriteLine("{0} going through city model and counting building instances", DateTime.Now.ToShortTimeString());

                    int count = 0;     // building count
                    foreach (CityObjectMember cityObjectMember in cityModel.getCityObjectMember().toArray())
                    {
                        AbstractCityObject cityObject = cityObjectMember.getCityObject();
                        if (cityObject.getCityGMLClass() == CityGMLClass.BUILDING)
                            count++;
                    }

                    Console.WriteLine("The city model contains "+count+" building features");
                }
            }

            reader.close();
            Console.WriteLine("{0} sample citygml4j application successfully finished", DateTime.Now.ToShortTimeString());
        }

        #endregion

        #region create citygml

        static void create_citygml()
        {
            Console.WriteLine("{0} setting up citygml4j context and JAXB builder", DateTime.Now.ToShortTimeString());
            CityGMLContext ctx = new CityGMLContext();
            CityGMLBuilder builder = ctx.createCityGMLBuilder();

            Console.WriteLine("{0} creating LOD2 building as citygml4j in-memory object tree", DateTime.Now.ToShortTimeString());
            GMLGeometryFactory geom = new GMLGeometryFactory();

            GMLIdManager gmlIdManager = DefaultGMLIdManager.getInstance();

            Building building = new Building();

            Polygon ground = geom.createLinearPolygon(new double[] { 0, 0, 0, 0, 12, 0, 6, 12, 0, 6, 0, 0, 0, 0, 0 }, 3);
            Polygon wall_1 = geom.createLinearPolygon(new double[] { 6, 0, 0, 6, 12, 0, 6, 12, 6, 6, 0, 6, 6, 0, 0 }, 3);
            Polygon wall_2 = geom.createLinearPolygon(new double[] { 0, 0, 0, 0, 0, 6, 0, 12, 6, 0, 12, 0, 0, 0, 0 }, 3);
            Polygon wall_3 = geom.createLinearPolygon(new double[] { 0, 0, 0, 6, 0, 0, 6, 0, 6, 3, 0, 9, 0, 0, 6, 0, 0, 0 }, 3);
            Polygon wall_4 = geom.createLinearPolygon(new double[] { 6, 12, 0, 0, 12, 0, 0, 12, 6, 3, 12, 9, 6, 12, 6, 6, 12, 0 }, 3);
            Polygon roof_1 = geom.createLinearPolygon(new double[] { 6, 0, 6, 6, 12, 6, 3, 12, 9, 3, 0, 9, 6, 0, 6 }, 3);
            Polygon roof_2 = geom.createLinearPolygon(new double[] { 0, 0, 6, 3, 0, 9, 3, 12, 9, 0, 12, 6, 0, 0, 6 }, 3);
            
            ground.setId(gmlIdManager.generateUUID());
            wall_1.setId(gmlIdManager.generateUUID());
            wall_2.setId(gmlIdManager.generateUUID());
            wall_3.setId(gmlIdManager.generateUUID());
            wall_4.setId(gmlIdManager.generateUUID());
            roof_1.setId(gmlIdManager.generateUUID());
            roof_2.setId(gmlIdManager.generateUUID());

            // lod2 solid
            List surfaceMember = new ArrayList();
            surfaceMember.add(new SurfaceProperty('#'+ground.getId()));
            surfaceMember.add(new SurfaceProperty('#'+wall_1.getId()));
            surfaceMember.add(new SurfaceProperty('#'+wall_2.getId()));
            surfaceMember.add(new SurfaceProperty('#'+wall_3.getId()));
            surfaceMember.add(new SurfaceProperty('#'+wall_4.getId()));
            surfaceMember.add(new SurfaceProperty('#'+roof_1.getId()));
            surfaceMember.add(new SurfaceProperty('#'+roof_2.getId()));

            CompositeSurface compositeSurface = new CompositeSurface();
            compositeSurface.setSurfaceMember(surfaceMember);
            Solid solid = new Solid();
            solid.setExterior(new SurfaceProperty(compositeSurface));

            building.setLod2Solid(new SolidProperty(solid));

            // thematic boundary surfaces
            List boundedBy = new ArrayList();
            boundedBy.add(createBoundarySurface(CityGMLClass.BUILDING_GROUND_SURFACE, ground));
            boundedBy.add(createBoundarySurface(CityGMLClass.BUILDING_WALL_SURFACE, wall_1));
            boundedBy.add(createBoundarySurface(CityGMLClass.BUILDING_WALL_SURFACE, wall_2));
            boundedBy.add(createBoundarySurface(CityGMLClass.BUILDING_WALL_SURFACE, wall_3));
            boundedBy.add(createBoundarySurface(CityGMLClass.BUILDING_WALL_SURFACE, wall_4));
            boundedBy.add(createBoundarySurface(CityGMLClass.BUILDING_ROOF_SURFACE, roof_1));
            boundedBy.add(createBoundarySurface(CityGMLClass.BUILDING_ROOF_SURFACE, roof_2));
            building.setBoundedBySurface(boundedBy);

            CityModel cityModel = new CityModel();
            cityModel.setBoundedBy(building.calcBoundedBy(false));
            cityModel.addCityObjectMember(new CityObjectMember(building));

            Console.WriteLine("{0} writing citygml4j object tree");
            CityGMLOutputFactory output = builder.createCityGMLOutputFactory(CityGMLVersion.DEFAULT);
            CityGMLWriter writer = output.createCityGMLWriter(new File("LOD2_Building_v200.gml"));

            writer.setPrefixes(CityGMLVersion.DEFAULT);
            writer.setSchemaLocations(CityGMLVersion.DEFAULT);
            writer.setIndentString("  ");
            writer.write(cityModel);
            writer.close();

            Console.WriteLine("{0} CityGML file LOD2_Building_v200.gml written", DateTime.Now.ToShortTimeString());
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
            if(type==CityGMLClass.BUILDING_WALL_SURFACE) {
                boundarySurface=new WallSurface();
            } else if(type==CityGMLClass.BUILDING_ROOF_SURFACE) {
                boundarySurface=new RoofSurface();
            } else if(type==CityGMLClass.BUILDING_GROUND_SURFACE) {
                boundarySurface=new GroundSurface();
            } else {

            }

            if(boundarySurface!=null) {
                boundarySurface.setLod2MultiSurface(new MultiSurfaceProperty(new MultiSurface(geometry)));
                return new BoundarySurfaceProperty(boundarySurface);
            }

            return null;
        }

        #endregion

        #region writing citygml

        static void write_citygml()
        {
            Console.WriteLine("{0} setting up citygml4j context and JAXB builder", DateTime.Now.ToShortTimeString());
            CityGMLContext ctx = new CityGMLContext();
            CityGMLBuilder builder = ctx.createCityGMLBuilder();

            Console.WriteLine("{0} reading ADE-enriched CityGML file LOD0_Railway_NoiseADE_v200.gml", DateTime.Now.ToShortTimeString());
            Console.WriteLine("{0} ADE schema file is read from xsi:schemaLocation attribute on root XML element", DateTime.Now.ToShortTimeString());
            CityGMLInputFactory input = builder.createCityGMLInputFactory();
            CityGMLReader reader = input.createCityGMLReader(new File("../datasets/LOD0_Railway_NoiseADE_v200.gml"));
            input.parseSchema(new File("../datasets/schemas/CityGML-NoiseADE-2_0_0.xsd"));

            //note: if encounters error like "provider com.sun.org.apache.xalan.internal.xsltc.trax.TransformerFactoryImpl not found", add reference to IKVM.OpenJDK.XML.Transform.dll
            CityModel cityModel = (CityModel)reader.nextFeature();
            reader.close();

            Console.WriteLine("{0} creating CityGML 2.0.0 writer", DateTime.Now.ToShortTimeString());
            CityGMLOutputFactory output = builder.createCityGMLOutputFactory(input.getSchemaHandler());
            ModuleContext moduleContext = new ModuleContext(CityGMLVersion.v2_0_0);

            Console.WriteLine("{0} input file is split per feature member whilst writing", DateTime.Now.ToShortTimeString());
            FeatureWriteMode writeMode = FeatureWriteMode.SPLIT_PER_COLLECTION_MEMBER;

            // set to true and check the differences
            bool splitOnCopy = false;

            output.setModuleContext(moduleContext);
            output.setGMLIdManager(DefaultGMLIdManager.getInstance());
            output.setProperty(CityGMLOutputFactory.FEATURE_WRITE_MODE, writeMode);
            output.setProperty(CityGMLOutputFactory.SPLIT_COPY, splitOnCopy);

            //out.setProperty(CityGMLOutputFactory.EXCLUDE_FROM_SPLITTING, ADEComponent.class);

            Console.WriteLine("{0} writing split result", DateTime.Now.ToShortTimeString());
            CityGMLWriter writer = output.createCityGMLWriter(new File("LOD0_Railway_NoiseADE_split_v200.gml"), "utf-8");
            setContext(writer, moduleContext, writeMode, splitOnCopy);

            writer.write(cityModel);
            writer.close();

            Console.WriteLine("{0} CityGML file LOD0_Railway_NoiseADE_split_v200.gml written", DateTime.Now.ToShortTimeString());

            Console.WriteLine("{0} writing remaining original object tree");
            writer=output.createCityGMLWriter(new File("LOD0_Railway_NoiseADE_orig_v200.gml"), "utf-8");
            setContext(writer, moduleContext, writeMode, splitOnCopy);

            writer.write(cityModel);
            writer.close();

            Console.WriteLine("{0} CityGML file LOD0_Railway_NoiseADE_orig_v200.gml written", DateTime.Now.ToShortTimeString());
            Console.WriteLine("{0} sample citygml4j application successfully finished", DateTime.Now.ToShortTimeString());
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
                    "Split mode: "+writeMode,
                    "Split on copy: "+splitOnCopy);
        }
        #endregion
    }

    /*
    public class AbstractType<T>
    {
        private List<T> Items { get; set; } 
    }

    public class ChildType<T> : AbstractType<T>
    {
        
    }

    public class ChildType2<T2> : AbstractType<T2> where T2 : struct
    {
        
    }

    public class ChildType3 : AbstractType<int>
    {
        
    }

    */
}
