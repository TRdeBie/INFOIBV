using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;

        public INFOIBV()
        {
            InitializeComponent();
        }

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
           if (openImageDialog.ShowDialog() == DialogResult.OK)             // Open File Dialog
            {
                string file = openImageDialog.FileName;                     // Get the file name
                imageFileName.Text = file;                                  // Show file name
                if (InputImage != null) InputImage.Dispose();               // Reset image
                InputImage = new Bitmap(file);                              // Create new Bitmap from file
                /*
                if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0 ||
                    InputImage.Size.Height > 512 || InputImage.Size.Width > 512) // Dimension check
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                */
                pictureBox1.Image = (Image) InputImage;                 // Display input image
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            if (InputImage == null) return;                                 // Get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // Reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // Create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // Create array to speed-up operations (Bitmap functions are very slow)

            // Setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // Copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    Image[x, y] = InputImage.GetPixel(x, y);                // Set pixel color in array at (x,y)
                }
            }

            Image = ImageToGreyscale(Image);
            progressBar.Value = 1;
            Image = ImageEqualizeHistogram(Image);
            progressBar.Value = 1;
            Image = ImageSharpening(Image);
            //Image = ImageStretchContrast(Image, (int)numUpDownLowerBound.Value, 20, (int)numUpDownUpperBound.Value, 235);
            //Image = ImageWindowing(Image, (int)numUpDownLowerBound.Value, (int)numUpDownUpperBound.Value);
            //Image = ImageNegative(Image);

            // Copy array to output Bitmap
            for (int x = 0; x < InputImage.Size.Width; x++)
            {
                for (int y = 0; y < InputImage.Size.Height; y++)
                {
                    OutputImage.SetPixel(x, y, Image[x, y]);               // Set the pixel color at coordinate (x,y)
                }
            }
            
            pictureBox2.Image = (Image)OutputImage;                         // Display output image
            progressBar.Visible = false;                                    // Hide progress bar
        }
        
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // Get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // Save the output image
        }

        private Color[,] ImageToGreyscale(Color[,] Image) {
            for (int x =0; x<InputImage.Size.Width; x++) {
                for (int y =0; y<InputImage.Size.Height; y++) {
                    int bright = (int)(Image[x, y].GetBrightness() * 255);
                    Image[x, y] = Color.FromArgb(255, bright, bright, bright);
                    progressBar.PerformStep();
                }
            }
            return Image;
        }

        private Color[,] ImageWindowing(Color[,] Image, int lowerBound, int upperBound) {
            for (int x = 0; x < InputImage.Size.Width; x++) {
                for (int y = 0; y < InputImage.Size.Height; y++) {
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
                    progressBar.PerformStep();                                  // Increment progress bar
                }
            }
            return Image;
        }

        private Color[,] ImageNegative(Color[,] Image) {
            for (int x = 0; x < InputImage.Size.Width; x++) {
                for (int y = 0; y < InputImage.Size.Height; y++) {
                    Color pixelColor = Image[x, y];                         // Get the pixel color at coordinate (x,y)
                    Color updatedColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B); // Negative image
                    Image[x, y] = updatedColor;                             // Set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                              // Increment progress bar
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
        private Color[,] ImageStretchContrast(Color[,] Image, int lowerBound, int lowerValue, int upperBound, int upperValue) {
            for (int y = 0; y < InputImage.Size.Height; y++) {
                for (int x = 0; x < InputImage.Size.Width; x++) {
                    // If colour is between 0 and the lowerBound, squish it
                    if (Image[x,y].R < lowerBound) {
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
                    progressBar.PerformStep();
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
        private Color[,] ImageEqualizeHistogram(Color[,] Image) {
            // Constructing the histogram of the image
            int[] histogram = ImageCreateHistogram(Image);
            // Create a cumulative version of the histogram
            for (int i = 0; i < histogram.Length; i++) {
                if (i > 0) {
                    histogram[i] += histogram[i - 1];
                }
            }
            // Determining the ideal number of times each grey value should occur in the image
            double idealOccurences = (InputImage.Size.Width * InputImage.Size.Height) / 256;
            // Creating a remap
            int[] remapping = new int[256];
            for (int i = 0; i < remapping.Length; i++) {
                remapping[i] = (int)Math.Floor((double)histogram[i] / idealOccurences + 0.5) - 1;
                if (remapping[i] < 0) {
                    remapping[i] = 0;
                }
            }
            // Recolour the Image according to the remapping
            for (int x = 0; x < InputImage.Size.Width; x++) {
                for (int y = 0; y < InputImage.Size.Height; y++) {
                    int newValue = remapping[Image[x, y].R];
                    Image[x, y] = Color.FromArgb(255, newValue, newValue, newValue);
                    progressBar.PerformStep();
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
        private int[] ImageCreateHistogram(Color[,] Image) {
            int[] histogram = new int[256];
            for (int x = 0; x < InputImage.Size.Width; x++) {
                for (int y = 0; y < InputImage.Size.Height; y++) {
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
        private Color[,] ImageSharpening(Color[,] Image) {
            int kernelWidth = 3;
            int kernelHeight = 3;
            double[,] kernel = new double[kernelWidth, kernelHeight];
            for (int x = 0; x < kernelWidth; x++) {
                for (int y = 0; y<kernelHeight; y++) {
                    kernel[x, y] = -1;
                    if (x == (kernelWidth - 1) / 2) {
                        if (y == (kernelHeight - 1) / 2) {
                            kernel[x, y] = 9;
                        }
                    }
                }
            }
            return ImageApplyKernel(Image, kernel, kernelWidth, kernelHeight);
        }
        /// <summary>
        /// Applies a given kernel to the image
        /// </summary>
        /// <param name="Image"></param>
        /// <param name="kernel">Kernel, odd x odd in size so that the center one is the sampler</param>
        /// <returns></returns>
        private Color[,] ImageApplyKernel(Color[,] Image, double[,] kernel, int kernelWidth, int kernelHeight) {
            Color[,] newImage = new Color[InputImage.Size.Width, InputImage.Size.Height];
            int kernelCenterX = kernelWidth / 2;
            int kernelCenterY = kernelHeight / 2;
            for (int x = 0; x < InputImage.Size.Width; x++) {
                for (int y = 0; y < InputImage.Size.Height; y++) {
                    double sum = 0;
                    int count = 0;
                    for (int kx = -kernelCenterX; kx < -kernelCenterX + kernelWidth; kx++) {
                        for (int ky = -kernelCenterY; ky < -kernelCenterY + kernelHeight; ky++) {
                            if (x + kx >= 0 && x + kx < InputImage.Size.Width) {
                                if (y + ky >= 0 && y + ky < InputImage.Size.Height) {
                                    sum += Image[x + kx, y + ky].R * kernel[kx + kernelCenterX, ky + kernelCenterY];
                                    count++;
                                }
                            }
                        }
                    }
                    int newValue = Math.Min(Math.Max((int)(sum / count), 0), 255);
                    newImage[x, y] = Color.FromArgb(255, newValue, newValue, newValue);
                    progressBar.PerformStep();
                }
            }
            return newImage;
        }
    }
}
