using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRevit.Model
{
    public class MyBuilding
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<MyWall> Walls { get; set; }

        public List<MyRoof> Roofs { get; set; }

        public List<MyFloor> Floors { get; set; }

        public List<MyCeiling> Ceilings { get; set; }

        public override string ToString()
        {
            string building2string = "";
            building2string += "Walls: " + this.Walls.Count.ToString() + "\n";
            building2string += "Roofs: " + this.Roofs.Count.ToString() + "\n";
            building2string += "Floors: " + this.Floors.Count.ToString() + "\n";
            building2string += "Ceilings: " + this.Ceilings.Count.ToString() + "\n";

            return building2string;
        }
    }

    public class MyWall
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<MyFace> Faces { get; set; }

        public List<MyWindow> Windows { get; set; }

        public List<MyDoor> Doors { get; set; }
    }

    public class MyDoor
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class MyWindow
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }

    public class MyFace
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public double[] Vertices { get; set; }

        public List<double[]> windowOpening { get; set; }

    }

    public class MyRoof
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<MyFace> Faces { get; set; }
    }

    public class MyFloor
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<MyFace> Faces { get; set; }
    }

    public class MyCeiling
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<MyFace> Faces { get; set; }
    }
}
