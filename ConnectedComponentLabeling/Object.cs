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
        public double longestChord, longestChordAngle;

        // Constructor
        public Object(int id) {
            this.id = id;
        }

        // Add pixels to the already existing object
        public void AddPixel(int x, int y) {
            pixels.Add(new Point(x, y));
        }

        // Properties
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
                if (longestChord == null || longestChord <= 0)
                    GetLongestChord();
                return longestChord;
            }
        }

        public double LongestChordAngle {
            get {
                if (longestChordAngle == null)
                    GetLongestChord();
                return longestChordAngle;
            }
        }

        private void GetLongestChord() {
            longestChord = 0;
            longestChordAngle = 0;
            if (pixelsPerimeter == null) CalculateNeighbourGrid();
            foreach (Point pixel1 in pixelsPerimeter) {
                foreach(Point pixel2 in pixelsPerimeter) {
                    double c = Math.Sqrt((pixel2.X - pixel1.X) ^ 2 + (pixel2.Y - pixel1.Y) ^ 2);
                    if (longestChord < c) {
                        longestChord = c;
                        if (pixel2.X - pixel1.X != 0)
                            longestChordAngle = Math.Atan((pixel2.Y - pixel1.Y) / (pixel2.X - pixel1.X)) * (180 / Math.PI);
                        else longestChordAngle = 90;
                    }
                }
            }
        }

        public double CalculatePerimete() {
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
