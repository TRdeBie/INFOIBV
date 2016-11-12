using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace INFOIBV
{
    public class Object     // Object class to keep track of necessary properties of the object
    {
        public int id;
        public List<Point> pixels = new List<Point>();

        // Constructor
        public Object(int id)
        {
            this.id = id;
        }

        // Add pixels to the already existing object
        public void AddPixel(int x, int y)
        {
            pixels.Add(new Point(x, y));
        }

        // Properties
        public int Size
        {
            get { return pixels.Count; }
        }


    }
}
