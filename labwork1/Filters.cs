using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using System.Windows.Forms;
using System.Security.Policy;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ScrollBar;


namespace labwork1
{
    public abstract class Filters
    {
        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        public Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for(int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for(int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j)); 
                }
            }
            return resultImage;
        }

    }
    public class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }
    } 
    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int intensity = (int)(0.36 * sourceColor.R) + (int)(0.53 * sourceColor.G) + (int)(0.11 * sourceColor.B);
            Color resultColor = Color.FromArgb(intensity, intensity, intensity);
            return resultColor;
        }
    }
    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int k = 20;
            Color sourceColor = sourceImage.GetPixel(x, y);
            int intensity = (int)(0.36 * sourceColor.R) + (int)(0.53 * sourceColor.G) + (int)(0.11 * sourceColor.B);
            Color resultColor = Color.FromArgb(Clamp((int)intensity + 2 * k, 0 , 255), Clamp((int)intensity + (int)(0.5 * k), 0, 255), Clamp((int)intensity - 1 * k, 0, 255));
            return resultColor;
        }
    }
    class BrightnessFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int k = 40;
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(Clamp((int)sourceColor.R + k, 0, 255), Clamp((int)sourceColor.G + k, 0, 255), Clamp(sourceColor.B + k, 0, 255));
            return resultColor;
        }
    }
    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() {}
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for(int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }
            return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
        }
    }
    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
        }
    }
    class GaussianFilter : MatrixFilter
    {
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }
        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }
    }
    class SobelFilter_X : MatrixFilter
    {
        public SobelFilter_X()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];

            kernel[0, 0] = -1;
            kernel[0, 1] = 0;
            kernel[0, 2] = 1;
            kernel[1, 0] = -2;
            kernel[1, 1] = 0;
            kernel[1, 2] = 2;
            kernel[2, 0] = -1;
            kernel[2, 1] = 0;
            kernel[2, 2] = 1;
        }
    }

    class SobelFilter_Y : MatrixFilter
    {
        public SobelFilter_Y()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];

            kernel[0, 0] = -1;
            kernel[0, 1] = -2;
            kernel[0, 2] = -1;
            kernel[1, 0] = 0;
            kernel[1, 1] = 0;
            kernel[1, 2] = 0;
            kernel[2, 0] = 1;
            kernel[2, 1] = 2;
            kernel[2, 2] = 1;
        }
    }

    class SharpnessFilter : MatrixFilter
    {
        public SharpnessFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];

            kernel[0, 0] = 0;
            kernel[0, 1] = -1;
            kernel[0, 2] = 0;
            kernel[1, 0] = -1;
            kernel[1, 1] = 5;
            kernel[1, 2] = -1;
            kernel[2, 0] = 0;
            kernel[2, 1] = -1;
            kernel[2, 2] = 0;
        }
    }
    class EmbossingFilter : MatrixFilter
    {
        public EmbossingFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];

            kernel[0, 0] = 0;
            kernel[0, 1] = 1;
            kernel[0, 2] = 0;
            kernel[1, 0] = 1;
            kernel[1, 1] = 0;
            kernel[1, 2] = -1;
            kernel[2, 0] = 0;
            kernel[2, 1] = -1;
            kernel[2, 2] = 0;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            int f = 100;
            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }
            Color resultColor = Color.FromArgb(Clamp((int)resultR + f, 0, 255), Clamp((int)resultG + f, 0, 255), Clamp((int)resultB + f, 0, 255));
            return resultColor;
        }
    }
    class MotionBlurFilter : MatrixFilter
    {
        public MotionBlurFilter()
        {
            int sizeN = 5;
            kernel = new float[sizeN, sizeN];
            for(int i = 0; i < sizeN; i++)
            {
                for(int j = 0; j < sizeN; j++)
                {
                    if(i == j)
                    {
                        kernel[i, j] = 1.0f * (1.0f / (float)sizeN);
                    } else
                    {
                        kernel[i, j] = 0.0f;
                    }
                }
            }
        }
    } 
}
