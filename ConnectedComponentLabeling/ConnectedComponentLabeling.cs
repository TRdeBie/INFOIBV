﻿using System;
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
                        for (int i = x - 1; i <= x + 2 && i < width; i++)
                            for (int j = y - 1; j <= y + 2 && j < height; j++)
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
    }
}