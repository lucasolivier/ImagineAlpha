using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Emgu.CV;
using Emgu.CV.Structure;

namespace ImagineAlpha
{
    public static class ImgTransform
    {
        public static Bitmap Get(Bitmap image)
        {
            BitmapData imgData = image.LockBits(new Rectangle(0, 0, 10, 10), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte[] test = new byte[imgData.Width * imgData.Height];
            Marshal.Copy(test, 0, imgData.Scan0, test.Length);

            Debug.WriteLine("discretizar pixels criando blocks para detectar provavel cabelo etc em volta do quadrado da face");
            


            image.UnlockBits(imgData);

            return image;
        }

        public static Mat Get(Mat img)
        {
            //CvInvoke.GaussianBlur(img, img, new Size(3, 3), 0);
            //return img;

            float squareRatio = img.Width / img.Height;

            int squareWidth = img.Width / 16;
            int squareHeight = img.Height / 8;


            for (int y = 0; y < img.Height / squareHeight; y++)
            {
                for (int x = 0; x < img.Width / squareWidth; x++)
                {

                    //Create 8x8 box
                    Mat box = new Mat(img, new Rectangle(x * squareWidth, y * squareHeight, squareWidth, squareHeight));

                    //Get roi mean color
                    MCvScalar boxMean = CvInvoke.Mean(box);
                    //MCvScalar boxMean = new MCvScalar(0, 255, 255);
                    int boxLength = box.Height * box.Width * box.ElementSize;
                    IntPtr boxDataPointer = box.DataPointer;

                    for (int j = 0, offsetPointer = 0; j < boxLength; j++)
                    {
                        if (j > 0 && j % (box.Width * box.ElementSize) == 0)
                        {
                            offsetPointer += (img.Width - box.Width) * box.ElementSize;
                        }

                        switch (j % 3)
                        {
                            case 0:
                                Marshal.WriteByte(boxDataPointer + offsetPointer + j, (byte)boxMean.V0);
                                break;

                            case 1:
                                Marshal.WriteByte(boxDataPointer + offsetPointer + j, (byte)boxMean.V1);
                                break;

                            case 2:
                                Marshal.WriteByte(boxDataPointer + offsetPointer + j, (byte)boxMean.V2);
                                break;
                        }
                    }

                }
            }

            return img;
















            int matLength = img.Width * img.Height * img.NumberOfChannels;
            MCvScalar meanPixel = CvInvoke.Mean(img);

            IntPtr dataPointer = img.DataPointer;

            for (int i = 0; i < matLength; i++)
            {

                switch (i % 3)
                {
                    case 0:
                        Marshal.WriteByte(dataPointer + i, (byte)meanPixel.V0);
                        break;

                    case 1:
                        Marshal.WriteByte(dataPointer + i, (byte)meanPixel.V1);
                        break;

                    case 2:
                        Marshal.WriteByte(dataPointer + i, (byte)meanPixel.V2);
                        break;
                }
            }

            return img;
        }



    }
}
