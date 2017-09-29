using Autodesk.Revit.DB;
using BuildingCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.UI.Selection;

namespace TestRevit
{
    class Utility
    {
        public static void SaveData<T>(T data, string path)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            using (var writer = new StreamWriter(path))
            {
                var result = JsonConvert.SerializeObject(data, settings);
                writer.WriteLine(result);
            }

            MessageBox.Show("written");
        }

        public static T LoadData<T>(string path)
        {
            using (var reader = new StreamReader(path))
            {
                var str = reader.ReadToEnd();
                var dt = (T)JsonConvert.DeserializeObject(str, typeof(T));
                return dt;
            }

            MessageBox.Show("read");
        }

        public static void WriteTXT(string s, string path)
        {
            //path: absolute
            FileStream fs = new FileStream(path, FileMode.Create);
            byte[] data = Encoding.Default.GetBytes(s);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
        }
    }

    /// <summary>
    /// A vertex lookup class to eliminate duplicate 
    /// vertex definitions
    /// </summary>
    class VertexLookupXyz : Dictionary<XYZ, int>
    {
        #region XyzEqualityComparer
        /// <summary>
        /// Define equality for Revit XYZ points.
        /// Very rough tolerance, as used by Revit itself.
        /// </summary>
        class XyzEqualityComparer : IEqualityComparer<XYZ>
        {
            const double _sixteenthInchInFeet
              = 1.0 / (16.0 * 12.0);

            public bool Equals(XYZ p, XYZ q)
            {
                return p.IsAlmostEqualTo(q,
                  _sixteenthInchInFeet);
            }

            public int GetHashCode(XYZ p)
            {
                return Util.PointString(p).GetHashCode();
            }
        }
        #endregion // XyzEqualityComparer

        public VertexLookupXyz() : base(new XyzEqualityComparer()) { }


        /// <summary>
        /// Return the index of the given vertex,
        /// adding a new entry if required.
        /// </summary>
        public int AddVertex(XYZ p)
        {
            return ContainsKey(p)
              ? this[p]
              : this[p] = Count;
        }
    }

    class TopoGraph
    {
        public Dictionary<string, Vertex> Rooms = new Dictionary<string, Vertex>();
        public List<Edge> Connections = new List<Edge>();

        //private static int count = 0;
        
        public bool[,] genAdjMat()
        {
            int nodeCount = Rooms.Count;
            bool[,] adjMat = new bool[nodeCount, nodeCount];
            Dictionary<string, int> tempRoom = new Dictionary<string, int>();
            int count = 0;
            foreach (string key in Rooms.Keys)
            {
                tempRoom.Add(key, count++);
            }
            foreach (Edge e in Connections)
            {
                adjMat[tempRoom[e.FromTo[0].Id], tempRoom[e.FromTo[1].Id]] = true;
                adjMat[tempRoom[e.FromTo[1].Id], tempRoom[e.FromTo[0].Id]] = true;
            }
            return adjMat;

        }

        public override string ToString()
        {
            string graph2string = "";
            graph2string += "Room Count: " + Rooms.Count + "\n";
            graph2string += "Edge Count: " + Connections.Count + "\n";

            foreach (var item in Rooms)
            {
                graph2string += "Room Info: " + item.Key + "\n";

            }

            bool[,] adjMat = genAdjMat();
            for (int i = 0; i < Rooms.Count; i++)
            {
                for (int j = 0; j < Rooms.Count; j++)
                {
                    graph2string += adjMat[i, j] + " ";
                }
                graph2string += "\n";
            }
            return graph2string;
        }

        public string createGraphData()
        {
            string res = "";
            foreach (string key in Rooms.Keys)
            {
                res += "CREATE(" + key + ": Location { name: \"" + key + "\" })";
            }
            res += "CREATE";
            int edgeCount = 0;
            foreach (Edge e in Connections)
            {
                edgeCount++;
                string from = e.FromTo[0].Id;
                string to = e.FromTo[1].Id;
                res += "(" + from + ") -[:CONNECTED_TO { distance: 1 }]->(" + to + "),";
                res += "(" + to + ") -[:CONNECTED_TO { distance: 1 }]->(" + from + "),";
                
            }
            res = res.Substring(0, res.Length - 1);
            //MessageBox.Show(res);
            return res;
        }
    }

    
    class Vertex
    {
        public string Id { get; set; }

        public Vertex(string Id)
        {
            this.Id = Id;
        }

        public override bool Equals(object obj)
        {
            Vertex target = obj as Vertex;
            return target.Id.Equals(this.Id);
        }
    }

    class Edge
    {
        public string Id { get; set; }

        public Vertex[] FromTo = new Vertex[2];
    }

    public class MySelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            FamilyInstance familyInstance = elem as FamilyInstance;
            //MessageBox.Show(familyInstance.Category.Id.ToString().Equals(((int)BuiltInCategory.OST_Windows).ToString()).ToString());
            if (familyInstance.Category.Id.ToString().Equals(((int)BuiltInCategory.OST_Doors).ToString()))
            {
                return true;
            }
            return false;
        }
        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
}
