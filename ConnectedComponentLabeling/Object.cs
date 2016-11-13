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
        public List<Point> pixelsPerimeter;
        private int[,] grid; // all pixels put into a relative grid
        private int[,] neighbourGrid; // all pixels with < 4 neighbours
        public Point origin;
        public int gridWidth, gridHeight;
        private double longestChordLength, longestChordAngle;
        private Point[] longestChord;
        private Point[] perpendicularChord;
        private double longestPerpChord = -1;

        // Constructor
        public Object(int id) {
            this.id = id;
        }

        // Add pixels to the already existing object
        public void AddPixel(int x, int y) {
            pixels.Add(new Point(x, y));
        }

        // Methods to pass on information 
        public int Size {
            get { return pixels.Count; }
        }

        public int[,] Grid {
            get {
                if (grid == null) CalculateGrid();
                return grid;
            }
        }

        public int[,] NeighbourGrid {
            get {
                if (neighbourGrid == null) CalculateNeighbourGrid();
                return neighbourGrid;
            }
        }

        public double LongestChord {
            get {
                if (longestChordLength <= 0)
                    GetLongestChord();
                return longestChordLength;
            }
        }

        public double LongestChordAngle {
            get {
                if (longestChordAngle <= 0)
                    GetLongestChord();
                return longestChordAngle;
            }
        }

        public Point[] LongChordCoordinates {
            get {
                if (longestChord == null)
                    GetLongestChord();
                return longestChord;
            }
        }

        public Point[] PerpendicularChordCoordinates {
            get {
                if (perpendicularChord == null)
                    GetLongestPerpChord();
                return perpendicularChord;
            }
        }

        public double MinimalBoundingBox {
            get {
                if (longestPerpChord < 1)
                    return longestChordLength;
                return longestPerpChord * longestChordLength;
            }
        }

        public double Eccentricity {
            get {
                if (longestPerpChord < 1)
                    return longestChordLength;
                return longestChordLength / longestPerpChord;
            }
        }

        public void RecalculateChord() {
            GetLongestChord();
            GetLongestPerpChord();
        }

        // Method to get the longest chord of an object
        private void GetLongestChord() {
            longestChord = new Point[2];
            longestChordLength = 0;
            longestChordAngle = 0;
            // Calculate the pixels in the perimeter if not done so already
            if (pixelsPerimeter == null) CalculateNeighbourGrid();
            // Loop through all pixels in the perimeter, two times to get 2 points
            foreach (Point pixel1 in pixelsPerimeter) {
                foreach(Point pixel2 in pixelsPerimeter) {
                    // Calculate the distance between the points
                    double c = Math.Sqrt((pixel2.X - pixel1.X) ^ 2 + (pixel2.Y - pixel1.Y) ^ 2);
                    // If it's longer than the previous longest chord, change all the properties of the longest chord to the current one
                    if (longestChordLength < c) {
                        longestChordLength = c;
                        longestChord[0] = new Point(pixel1.X + origin.X, pixel1.Y + origin.Y);
                        longestChord[1] = new Point(pixel2.X + origin.X, pixel2.Y + origin.Y);
                        // Calculate angle to a horizontal line
                        if (pixel2.X - pixel1.X != 0)
                            longestChordAngle = Math.Atan((pixel2.Y - pixel1.Y) / (pixel2.X - pixel1.X)) * (180 / Math.PI);
                        else longestChordAngle = 90;
                        if (longestChordAngle < 0)
                            longestChordAngle += 360;
                    }
                }
            }
        }

        // Method to pass on the length of the longest perpendicular chord to the longest chord of the object
        public double LongestPerpChord
        {
            get
            {
                if (longestPerpChord == -1)
                    GetLongestPerpChord();
                return longestPerpChord;
            }
        }

        // Calculate the longest perpendicular chord to the longest chord
        public void GetLongestPerpChord()
        {
            // Make sure not to divide by 0
            double lcs1 = longestChord[1].X - longestChord[0].X;
            double lcs2 = longestChord[1].Y - longestChord[0].Y;
            double longestChordSlope = 0;
            if (lcs2 != 0)
                longestChordSlope = lcs1 / lcs2;
            // Calculate the negativeReciprocal, which should be the slope for a perpendicular line
            double negativeReciprocal = Math.Ceiling(-1 * (1 / longestChordSlope));
            
            // Calculate the pixels in the perimeter if not done so already
            if (pixelsPerimeter == null) CalculateNeighbourGrid();
            // Loop through all pixels in the perimeter, two times to get 2 points
            foreach (Point pixel1 in pixelsPerimeter) { 
                foreach (Point pixel2 in pixelsPerimeter) {
                    // Prevent dividing by 0
                    double l1 = pixel2.X - pixel1.X;
                    double l2 = pixel2.Y - pixel1.Y;
                    double l = 0;
                    if (l2 != 0)
                        l = Math.Ceiling(l1 / l2);
                    // Check if the calculated slope is equal to the negative reciprocal of the longest chord
                    if (l == negativeReciprocal)
                    {
                        // Calculate the length and check if it's longer than the current longest prependicular chord
                        double c = Math.Sqrt((pixel2.X - pixel1.X) ^ 2 + (pixel2.Y - pixel1.Y) ^ 2);
                        if (longestPerpChord < c) {
                            longestPerpChord = c;
                            //perpendicularChord[0] = new Point(pixel1.X, pixel1.Y);
                            //perpendicularChord[1] = new Point(pixel2.X, pixel2.Y);
                        }
                    }                      
                }
            }
        }

        public double CalculatePerimeter() {
            if (grid == null) { CalculateGrid(); }
            if (neighbourGrid == null) { CalculateNeighbourGrid(); }
            int xInit, yInit;
            bool lookingForStart = true;
            for(int x = 0; x < gridWidth; x++) {
                if (!lookingForStart) break;
                for (int y = 0; y < gridHeight; y++) {
                    if (!lookingForStart) break;
                    if (grid[x, y] > 0) {
                        xInit = x;
                        yInit = y;
                        lookingForStart = false;
                    }
                }
            }
            // iterate along the edge of the object, incrementing the length of the perimeter
            double diagonal = Math.Sqrt(2);

            return 1;
        }
        /// <summary>
        /// Turn the grid into a grid where each value indicates the number of that pixel's neighbours
        /// </summary>
        private void CalculateNeighbourGrid() {
            pixelsPerimeter = new List<Point>();
            if (grid == null) CalculateGrid();
            neighbourGrid = new int[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++) {
                for (int y = 0; y < gridHeight; y++) {
                    if (grid[x, y] > 0) {
                        neighbourGrid[x, y] = 0;
                        int neighbours = 1;
                        if (x - 1 >= 0) {
                            if (grid[x - 1, y] > 0) neighbours++;
                        }
                        if (x+1 < gridWidth) {
                            if (grid[x + 1, y] > 0) neighbours++;
                        }
                        if (y - 1 >= 0) {
                            if (grid[x, y - 1] > 0) neighbours++;
                        }
                        if (y + 1 < gridHeight) {
                            if (grid[x, y + 1] > 0) neighbours++;
                        }
                        if (neighbours > 0 && neighbours < 4) {
                            neighbourGrid[x, y] = 1;
                            pixelsPerimeter.Add(new Point(x, y));
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Turns the list of pixels into a grid of bools
        /// true for a pixel, false for a void
        /// origin of the grid lies as the minimum x and y values
        /// </summary>
        private void CalculateGrid() {
            // First find the min and max values for x and y
            int minX = int.MaxValue;
            int maxX = -1;
            int minY = int.MaxValue;
            int maxY = -1;
            foreach (Point p in pixels) {
                if (p.X < minX) minX = p.X ;
                if (p.X > maxX) maxX = p.X ;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }
            gridWidth = maxX + 1 - minX;
            gridHeight = maxY + 1 - minY;
            origin = new Point(minX, minY);
            grid = new int[gridWidth, gridHeight];
            foreach (Point p in pixels) {
                grid[p.X - minX, p.Y - minY] = 1;
            }
        }
    }
}
