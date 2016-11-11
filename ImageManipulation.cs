﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace INFOIBV {
    public static class ImageManipulation {
        

        public static Color[,] ImageToGreyscale(Color[,] Image, int width, int height) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    int bright = (int)(Image[x, y].GetBrightness() * 255);
                    Image[x, y] = Color.FromArgb(255, bright, bright, bright);
                    //progressBar.PerformStep();
                }
            }
            return Image;
        }

        public static Color[,] ImageWindowing(Color[,] Image, int width, int height, int lowerBound, int upperBound) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    Color pixelColor = Image[x, y];                             // Get the pixel color at coordinate (x,y)
                    int bright = (int)(pixelColor.GetBrightness() * 255);       // Get the brightness of the color [0-1] and convert it to [0-225]
                    Color updatedColor = new Color();
                    if (bright < lowerBound)
                        updatedColor = Color.Black;                             // Colours below the lower bound are set to pure black
                    else if (bright > upperBound)
                        updatedColor = Color.Black;                             // Colours above the upper bound are set to pure white
                    else
                        updatedColor = Color.FromArgb(bright, bright, bright);  // Colours in the window are converted to greyscale
                    Image[x, y] = updatedColor;                                 // Set the new pixel color at coordinate (x,y)
                }
            }
            return Image;
        }

        public static Color[,] ImageNegative(Color[,] Image, int width, int height) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    Color pixelColor = Image[x, y];                         // Get the pixel color at coordinate (x,y)
                    Color updatedColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B); // Negative image
                    Image[x, y] = updatedColor;                             // Set the new pixel color at coordinate (x,y)
                }
            }
            return Image;
        }
        /// <summary>
        /// Takes a (greyscale) image, and stretches the contrast between 2 values
        /// </summary>
        /// <param name="Image">Source image</param>
        /// <param name="lowerBound">The lower grey value of the area that's to be stretched</param>
        /// <param name="lowerValue">The target value of the lowerBound. Typically lowerValue < lowerBound</param>
        /// <param name="upperBound">The upper grey value of the area that's to be stretched</param>
        /// <param name="upperValue">The target value of the upperBound. Typically upperValue > upperBound</param>
        /// <returns>The edited image</returns>
        public static Color[,] ImageStretchContrast(Color[,] Image, int width, int height, int lowerBound, int lowerValue, int upperBound, int upperValue) {
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    // If colour is between 0 and the lowerBound, squish it
                    if (Image[x, y].R < lowerBound) {
                        double fraction = 0;
                        if (lowerBound > 0)
                            // if colour = 50 and lowerBound = 100, fraction should be 0.5
                            fraction = (double)Image[x, y].R / (double)lowerBound;

                        int newColor = (int)((lowerValue) * fraction);
                        Image[x, y] = Color.FromArgb(255, newColor, newColor, newColor);
                    }
                    // If colour is between upperBound and 255, squish it
                    else if (Image[x, y].R > upperBound) {
                        double fraction = (double)(Image[x, y].R - upperBound) / (double)(255 - upperBound);
                        int newColor = (int)(upperValue + (255 - upperValue) * fraction);
                        Image[x, y] = Color.FromArgb(255, newColor, newColor, newColor);
                    }
                    // Else colour is between lowerBound and upperBound, so stretch it
                    else {
                        double fraction = (double)(Image[x, y].R - lowerBound) / (double)(upperBound - lowerBound);
                        int newColor = (int)(lowerValue + (upperValue - lowerValue) * fraction);
                        Image[x, y] = Color.FromArgb(255, newColor, newColor, newColor);
                    }
                }
            }
            return Image;
        }
        /// <summary>
        /// Takes an image, and equalizes its histogram
        /// Assumes that the image is greyscale, otherwise only accesses the R values from the RGB
        /// </summary>
        /// <param name="Image">Input image</param>
        /// <returns>Histogram-equalized image</returns>
        public static Color[,] ImageEqualizeHistogram(Color[,] Image, int width, int height) {
            // Constructing the histogram of the image
            int[] histogram = ImageCreateHistogram(Image, width, height);
            // Create a cumulative version of the histogram
            for (int i = 0; i < histogram.Length; i++) {
                if (i > 0) {
                    histogram[i] += histogram[i - 1];
                }
            }
            // Determining the ideal number of times each grey value should occur in the image
            double idealOccurences = (width * height) / 256;
            // Creating a remap
            int[] remapping = new int[256];
            for (int i = 0; i < remapping.Length; i++) {
                remapping[i] = (int)Math.Floor((double)histogram[i] / idealOccurences + 0.5) - 1;
                if (remapping[i] < 0) {
                    remapping[i] = 0;
                }
            }
            // Recolour the Image according to the remapping
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    int newValue = remapping[Image[x, y].R];
                    Image[x, y] = Color.FromArgb(255, newValue, newValue, newValue);
                }
            }
            return Image;
        }
        /// <summary>
        /// Takes an image, and creates a histogram based on that image
        /// The histogram is an array of integers: the index is the greyvalue from 0-255, and the value of that index is how often it occurs
        /// </summary>
        /// <param name="Image">Input image</param>
        /// <returns>Histogram of the image's greyvalues</returns>
        public static int[] ImageCreateHistogram(Color[,] Image, int width, int height) {
            int[] histogram = new int[256];
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    int greyValue = Image[x, y].R;
                    if (greyValue >= 0 && greyValue <= 255) {
                        histogram[greyValue]++;
                    }
                }
            }
            return histogram;
        }
        /// <summary>
        /// Applies a kernel to sharpen the image
        /// </summary>
        /// <param name="Image"></param>
        /// <returns></returns>
        public static Color[,] ImageSharpening(Color[,] Image, int width, int height) {
            int kernelWidth = 3;
            int kernelHeight = 3;
            double[,] kernel = new double[kernelWidth, kernelHeight];
            for (int x = 0; x < kernelWidth; x++) {
                for (int y = 0; y < kernelHeight; y++) {
                    kernel[x, y] = -1;
                    if (x == (kernelWidth - 1) / 2) {
                        if (y == (kernelHeight - 1) / 2) {
                            kernel[x, y] = 9;
                        }
                    }
                }
            }
            return ImageApplyKernel(Image, width, height, kernel, kernelWidth, kernelHeight);
        }
        /// <summary>
        /// Applies a given kernel to the image
        /// </summary>
        /// <param name="Image"></param>
        /// <param name="kernel">Kernel, odd x odd in size so that the center one is the sampler</param>
        /// <returns></returns>
        public static Color[,] ImageApplyKernel(Color[,] Image, int width, int height, double[,] kernel, int kernelWidth, int kernelHeight) {
            Color[,] newImage = new Color[width, height];
            int kernelCenterX = kernelWidth / 2;
            int kernelCenterY = kernelHeight / 2;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    double sum = 0;
                    int count = 0;
                    for (int kx = -kernelCenterX; kx < -kernelCenterX + kernelWidth; kx++) {
                        for (int ky = -kernelCenterY; ky < -kernelCenterY + kernelHeight; ky++) {
                            if (x + kx >= 0 && x + kx < width) {
                                if (y + ky >= 0 && y + ky < height) {
                                    sum += Image[x + kx, y + ky].R * kernel[kx + kernelCenterX, ky + kernelCenterY];
                                    count++;
                                }
                            }
                        }
                    }
                    int newValue = Math.Min(Math.Max((int)(sum / count), 0), 255);
                    newImage[x, y] = Color.FromArgb(255, newValue, newValue, newValue);
                }
            }
            return newImage;
        }

        // Thresholding
        public static Color[,] ApplyThreshold(Color[,] Image, int width, int height, int thresholdValue)
        {
            Color[,] newImage = new Color[width, height];
            // Convert image to black and white based on average brightness
            for (int y = 0; y < height; y++){
                for (int x = 0; x < width; x++){
                    // Set this pixel to black or white based on threshold
                    int pixelValue = (int)(Image[x, y].GetBrightness() * 255);
                    if (pixelValue >= thresholdValue)
                        newImage[x, y] = Color.White;
                    else
                        newImage[x, y] = Color.Black;
                }
            }
            return newImage;
        }

        //Erosion
        public static Color[,] ApplyErosion(Color[,] Image, int width, int height)
        {
            Color[,] newImage = new Color[width, height];
            Color pixelValueAbove, pixelValueBelow, pixelValueLeft, pixelValueRight;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (y - 1 > 0)
                        pixelValueAbove = Image[x, y - 1];
                    else
                        pixelValueAbove = Color.Purple;
                    if (y + 1 < height)
                        pixelValueBelow = Image[x, y + 1];
                    else
                        pixelValueBelow = Color.Purple;
                    if (x - 1 < width)
                        pixelValueLeft = Image[x - 1, y];
                    else
                        pixelValueLeft = Color.Purple;
                    if (x + 1 > 0)
                        pixelValueRight = Image[x + 1, y];
                    else
                        pixelValueRight = Color.Purple;

                    if (pixelValueAbove == Color.Black || pixelValueBelow == Color.Black || pixelValueLeft == Color.Black || pixelValueRight == Color.Black)
                        newImage[x, y] = Color.Black;
                    else
                        newImage[x, y] = Color.White;
                }
            }
            return newImage;
        }

        //Dilation
        public static Color[,] ApplyDilation(Color[,] Image, int width, int height)
        {
            Color[,] newImage = new Color[width, height];
            Color pixelValueAbove, pixelValueBelow, pixelValueLeft, pixelValueRight;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Set this pixel to black or white based on threshold
                    if (y - 1 > 0)
                        pixelValueAbove = Image[x, y - 1];
                    else
                        pixelValueAbove = Color.Purple;
                    if (y + 1 < height)
                        pixelValueBelow = Image[x, y + 1];
                    else
                        pixelValueBelow = Color.Purple;
                    if (x - 1 < width)
                        pixelValueLeft = Image[x - 1, y];
                    else
                        pixelValueLeft = Color.Purple;
                    if (x + 1 > 0)
                        pixelValueRight = Image[x + 1, y];
                    else
                        pixelValueRight = Color.Purple;

                    if (pixelValueAbove == Color.White || pixelValueBelow == Color.White || pixelValueLeft == Color.White || pixelValueRight == Color.White)
                        newImage[x, y] = Color.White;
                    else
                        newImage[x, y] = Color.Black;
                }
            }
            return newImage;
        }
    }
}
