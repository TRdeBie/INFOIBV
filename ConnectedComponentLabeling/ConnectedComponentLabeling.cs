using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace INFOIBV
{
    // Based on http://www.codeproject.com/Articles/336915/Connected-Component-Labeling-Algorithm
    public partial class INFOIBV
    {
        public List<Object> objectList = new List<Object>();

        public int DetectObjects(Color[,] image, int width, int height)
        {
            // Clear the list from previous detects
            objectList.Clear();

            int[,] pixelLabels = new int[width, height];                            // Create tabel equal to amount of pixels in the original image            
            Dictionary<int, Label> allLabels = new Dictionary<int, Label>();        // Dictionary of all label values used            
            int labelNumber = 1;                                                    // Start label numbering at 1

            // First pass to label all the pixels
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Check if pixel is not part of the background
                    if (image[x, y] != Color.Black)
                    {
                        // Create a list to keep record of all the neighbouring pixels
                        List<int> neighbourLabels = new List<int>();

                        // Find all labelled neighbours in range
                        for (int i = x - 1; i < x + 2 && i < width; i++)
                            for (int j = y - 1; j < y + 2 && j < height; j++)
                                if (i > -1 && j > -1 && pixelLabels[i, j] != 0)
                                    neighbourLabels.Add(pixelLabels[i, j]);

                        // Give the pixel a new label when no neighbours were found
                        if (neighbourLabels.Count == 0)
                        {
                            pixelLabels[x, y] = labelNumber;
                            allLabels.Add(labelNumber, new Label(labelNumber));
                            labelNumber++;
                        }
                        else
                        {
                            // Get the smallest label from the neighbours and set the pixelLabel to this value
                            int minLabel = neighbourLabels.Min();
                            pixelLabels[x, y] = minLabel;

                            // Set root equal to the root of the label for all neighbours
                            foreach (int label in neighbourLabels)
                                if (allLabels[minLabel].GetRoot().id != allLabels[label].GetRoot().id)
                                    allLabels[label].Join(allLabels[minLabel]);
                        }
                    }
                }
            }

            // Loop through the pixels again to form the objects
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if(pixelLabels[x, y] != 0)
                    {
                        // Get the root of the current pixel
                        int rootLabel = allLabels[pixelLabels[x, y]].GetRoot().id;

                        // Check if the object which this pixel is part off already exists
                        Object pixelObject = objectList.Find(r => r.id == rootLabel);

                        if (pixelObject == null)
                        {
                            pixelObject = new Object(rootLabel);
                            objectList.Add(pixelObject);
                        }

                        // Add pixel to the object it belongs to
                        pixelObject.AddPixel(x, y);
                    }
                }
            }

            return objectList.Count;
        }
        /// <summary>
        /// Using the objectlist, 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Color[,] ColorObjects(int width, int height) {
            Random r = new Random();
            Color[] options = new Color[7] { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple, Color.Orange, Color.White };
            int counter = 0;
            Color[,] image = new Color[width, height];
            for (int x =0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    image[x, y] = Color.Black;
                }
            }
            foreach(Object o in objectList) {
                //Color c = options[counter % 7];
                Color c = Color.FromArgb(255, r.Next(255), r.Next(255), r.Next(255));
                foreach(Point pixel in o.pixels) {
                    image[pixel.X, pixel.Y] = c;
                }
                counter++;
            }
            return image;
        }

        public Color[,] RemoveNoiseBySize(Color[,] image, int width, int height)
        {
            List<Object> removables = new List<Object>();

            foreach (Object singleObj in objectList)
                if (singleObj.Size < 15)
                {
                    foreach (Point pixel in singleObj.pixels)
                        image[pixel.X, pixel.Y] = Color.Black;
                    removables.Add(singleObj);
                }

            foreach (Object toRemove in removables)
                objectList.Remove(toRemove);

            return image;
        }

        public void CalculateCompactness() {

        }

        public Color[,] DrawObjectPerimeter(int width, int height) {
            Color[,] image = new Color[width, height];
            for(int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    image[x, y] = Color.Black;
                }
            }
            foreach(Object o in objectList) {
                int[,] perimeter = o.NeighbourGrid;
                Point origin = o.origin;
                int oWidth = o.gridWidth;
                int oHeight = o.gridHeight;
                for (int x = 0; x < oWidth; x++) {
                    for (int y = 0; y < oHeight; y++) {
                        if (perimeter[x,y] > 0) {
                            image[origin.X + x, origin.Y + y] = Color.White;
                        }
                    }
                }
            }
            return image;
        }

        public Color[,] DrawObjectsFromChordAngle(int width, int height) {
            Color[,] image = new Color[width, height];
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    image[x, y] = Color.Black;
                }
            }
            foreach (Object o in objectList) {
                int newValue = (int)Math.Max(0, Math.Min(255, o.LongestChordAngle));
                Color c = Color.FromArgb(255, newValue, 255 - newValue, 0);
                foreach (Point p in o.pixels) {
                    image[p.X, p.Y] = c;
                }
            }
            return image;
        }
        /// <summary>
        /// Draws an overlay of the chords onto the image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Color[,] DrawObjectLongestChords(Color[,] image, int width, int height) {
            foreach (Object o in objectList) {
                //Point[] chord = o.LongestChordCoordinates;
                Point[] chord = o.LongChordCoordinates;
                double xDiff = chord[1].X - chord[0].X;
                double yDiff = chord[1].Y - chord[0].Y;
                double length = Math.Sqrt(Math.Pow(xDiff, 2) + Math.Pow(yDiff, 2));
                if (length > 0) {
                    double xDir = xDiff / length; //Normalized
                    double yDir = yDiff / length;
                    double x = chord[0].X;
                    double y = chord[0].Y;
                    image[(int)x, (int)y] = Color.White;
                    // Traverse the way from chord[0] to chord[1]
                    for (int i = 0; i < (int)Math.Ceiling(length); i++) {
                        x += xDir;
                        y += yDir;
                        image[(int)x, (int)y] = Color.Red;
                    }
                }
            }
            return image;
        }

        public Color[,] RemoveLongestChords(Color[,] image, int width, int height) {
            List<double> chordLengths = new List<double>();
            foreach(Object o in objectList) {
                chordLengths.Add(o.LongestChord);
            }
            double threshold = chordLengths.Average() * 2;
            List<Object> removables = new List<Object>();
            foreach(Object o in objectList) {
                if (o.LongestChord > threshold) {
                    foreach (Point pixel in o.pixels)
                        image[pixel.X, pixel.Y] = Color.Black;
                    removables.Add(o);
                }
            }
            foreach(Object o in removables) {
                objectList.Remove(o);
            }
            return image;
        }

        public void RecalculateChords()
        {
            foreach (Object o in objectList)
            {
                o.RecalculateChord();
            }
        }

        public Color[,] RemoveStraightObjects(Color[,] image, int width, int height)
        {
            List<Object> removables = new List<Object>();
            foreach (Object o in objectList)
                if (o.LongestPerpChord == -1) { 
                    foreach (Point pixel in o.pixels)
                        image[pixel.X, pixel.Y] = Color.Black;
                    removables.Add(o);
                }
            foreach (Object o in removables)
                objectList.Remove(o);

            return image;
        }

        public Color[,] ColorOnEccentricity(int width, int height) {
            Color[,] image = new Color[width, height];
            foreach(Object o in objectList) {
                double e = o.Eccentricity;
                double m = o.MinimalBoundingBox;
                double s = o.Size;
                int r = (int)Math.Max(0, Math.Min(255, (e * 50)));
                int g = (int)Math.Max(0, Math.Min(255, (m * 50)));
                int b = (int)Math.Max(0, Math.Min(255, (s * 1)));
                foreach(Point pixel in o.pixels) {
                    image[pixel.X, pixel.Y] = Color.FromArgb(255, r, g, b);
                }
            }
            return image;
        }

        public Color[,] FilterOnEccentricity(int width, int height) {
            Color[,] image = new Color[width, height];
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    image[x, y] = Color.Black;
                }
            }
            List<Object> removables = new List<Object>();
            foreach (Object o in objectList) {
                double e = o.Eccentricity;
                double m = o.MinimalBoundingBox;
                double s = o.Size;
                int r = (int)Math.Max(0, Math.Min(255, (e * 75)));
                int g = (int)Math.Max(0, Math.Min(255, (m * 5)));
                int b = (int)Math.Max(0, Math.Min(255, (s * 1)));
                Color c = Color.Black;
                int threshold = (int)((width * height) / 5630);
                //threshold = 220;
                if (b > threshold) {
                    c = Color.White;
                }
                else {
                    removables.Add(o);
                }
            }
            foreach (Object o in removables) {
                objectList.Remove(o);
            }
            // After removing the last bits from the objectlist, give every object remaining a different shade of grey
            double value = 50;
            double increment = (255 - value) / objectList.Count;
            foreach(Object o in objectList) {
                Color c = Color.FromArgb(255, (int)value, (int)value, (int)value);
                foreach(Point pixel in o.pixels) {
                    image[pixel.X, pixel.Y] = c;
                }
                value += increment;
                value = Math.Min(255, value);
            }
            return image;
        }
    }
}
