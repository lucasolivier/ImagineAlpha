using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Cuda;
using Emgu.CV.Util;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

using OpenCVJavaInterface;

namespace ImagineAlpha
{

    public partial class Form1 : Form
    {

        FaceDetector faceDetector = new FaceDetector(new String[]
        {
            "haarcascade_frontalface_alt2.xml"
        }, new String[] {
            "haarcascade_lefteye_2splits.xml",
            "haarcascade_righteye_2splits.xml"
        }, new String[] {
           "Nose.xml",
           "haarcascade_mcs_nose.xml"
        }, new String[] {
            "Mouth.xml"
        });

        public Form1()
        {
            InitializeComponent();
        }

        Image<Gray,Byte> grayImg;

        void createHistogram(IImage image, int bins)
        {
            HistogramForm histForm = new HistogramForm();
            histForm.SetHistogram(image, bins);
            histForm.Show();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Mat image = new Mat(openFileDialog1.FileName, LoadImageType.Color);
                grayImg = new Image<Gray, Byte>(image.Bitmap);

                System.Drawing.Bitmap skyImg = new System.Drawing.Bitmap(image.Width, image.Height);
                System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(skyImg);
                LinearGradientBrush grad = new LinearGradientBrush(
                    new System.Drawing.Rectangle(0, 0, skyImg.Width, skyImg.Height),
                    System.Drawing.Color.Blue,
                    System.Drawing.Color.White,
                    LinearGradientMode.Vertical);

                graphics.FillRectangle(grad, grad.Rectangle);

                Mat newImage = new Mat(image.Size, image.Depth, image.NumberOfChannels);

                Image<Bgr, byte> skyImg2 = new Image<Bgr, byte>(skyImg);
                newImage = skyImg2.Mat;

                int newImgLength = newImage.Cols * newImage.Rows * newImage.NumberOfChannels;

                /*for (int i = 0; i < newImgLength; i++)
                {
                    newImage.SetValue(i, (byte)255);
                }*/


                CvInvoke.CvtColor(image, grayImg, ColorConversion.Bgr2Gray);

                pictureBox2.Image = grayImg.Bitmap;
                //pictureBox3.Image = grayImg.ThresholdToZero(new Gray(253)).Bitmap;

                ArrayList<PersonFace> personFaces = faceDetector.Detect(image);

                foreach (PersonFace personFace in personFaces)
                {
                    personFace.Evaluate();

                    if (!personFace.IsValid())
                        continue;

                    //----- Begin Histogram Tests ---------------

                    //Image<Bgr, byte> testImg = new Image<Bgr, byte>(image.Bitmap);


                    /*you will get the most used values from the histogram and compare to pre created colors to find the one that matches better

                    must verify which color spaces we will compare <- this later

                        for the mvp, everyone will be white rgb 227,203,172*/

                    //testImg.InRange()

                    /*pictureBox3.Image = testImg.Bitmap;

                    testImg.ROI = personFace.GetFace();
                    grayImg.ROI = personFace.GetFace();

                    createHistogram(testImg, 256);
                    createHistogram(grayImg, 256);

                    pictureBox2.Image = grayImg.Bitmap;
                    pictureBox3.Image = testImg.Bitmap;*/


                    //-----End Histogram Tests ----------------

                    Rect faceRect = personFace.GetFace();
                    Rect mouthRect = personFace.GetMouth();
                    Rect noseRect = personFace.GetNose();
                    Rect[] eyesRects = personFace.GetEyes();

                    //Draw face division line
                    double[] faceLineData = personFace.GetFaceLineData();
                    PointGenerator faceLine = new PointGenerator(faceLineData[0], faceLineData[1]);
                    Point faceTopPoint = faceLine.GetFromY(faceRect.y);
                    Point faceBottomPoint = faceLine.GetFromY(faceRect.y + faceRect.height);
                    //CvInvoke.Line(image, faceTopPoint, faceBottomPoint, new Bgr(Color.Orange).MCvScalar, 3);

                    //Draw rect around the face
                    //CvInvoke.Rectangle(image, faceRect, new Bgr(Color.Yellow).MCvScalar, 2);

                    //Draw rect around the mouth
                    //CvInvoke.Rectangle(image, mouthRect, new Bgr(Color.Blue).MCvScalar, 2);

                    //Draw rect around the nose
                    //CvInvoke.Rectangle(image, noseRect, new Bgr(Color.Green).MCvScalar, 2);

                    //Draw eyes rect and circles
                    foreach (Rect eye in eyesRects)
                    {
                        /*CvInvoke.Rectangle(image, eye, new Bgr(Color.White).MCvScalar, 2);
                        CvInvoke.Circle(image, new Point(eye.X + eye.Width / 2, eye.Y + eye.Height / 2), eye.Width / 2, new Bgr(Color.White).MCvScalar, 2);*/
                    }

                    //Get face feature angle
                    double faceFeatureAngle = Math.Atan(faceLineData[0]);
                    faceFeatureAngle = RadianToDegree(faceFeatureAngle);
                    faceFeatureAngle += faceFeatureAngle > 0 ? -90 : 90;


                    //Draw circle around face
                    /*Point faceCenter = new Point(faceRect.X + faceRect.Width / 2, 
                        faceRect.Y + faceRect.Height / 2);
                    Size faceSize = new Size(faceRect.Width / 2, faceRect.Height / 2);
                    CvInvoke.Ellipse(image, faceCenter,faceSize,0,0,360,
                        new Bgr(172, 203, 227).MCvScalar,0);
                    CvInvoke.Ellipse(image, faceCenter, faceSize, 0, 0, 360, 
                        new Bgr(Color.Black).MCvScalar, 1);*/

                    //Draw face lateral boundaries lines
                    //Detect right and left eye
                    Rect rightEye, leftEye;
                    if(eyesRects[0].x > eyesRects[1].x)
                    {
                        rightEye = eyesRects[1];
                        leftEye = eyesRects[0];
                    }
                    else
                    {
                        rightEye = eyesRects[0];
                        leftEye = eyesRects[1];
                    }

                    //get eye line generator
                    PointGenerator eyeLines = new PointGenerator(
                        getRectCenter(rightEye), getRectCenter(leftEye));

                    Point leftFacePoint = eyeLines.GetFromX(getRectCenter(leftEye).x + leftEye.width);
                    Point rightFacePoint = eyeLines.GetFromX(getRectCenter(rightEye).x - rightEye.width);

                   /* CvInvoke.Circle(image, leftFacePoint, 20,
                        new Bgr(Color.Green).MCvScalar, -1);

                    CvInvoke.Circle(image, rightFacePoint, 20,
                        new Bgr(Color.Blue).MCvScalar, -1);*/



                    //Get line generators for each side of the face
                    double faceLineSlope = faceLineData[0];

                    //Left side
                    double leftFaceSideOffset = leftFacePoint.y - leftFacePoint.x * faceLineSlope;
                    PointGenerator leftFaceLine = new PointGenerator(faceLineSlope, leftFaceSideOffset);

                    Point startPointL = leftFaceLine.GetFromY(0);
                    Point endPointL = leftFaceLine.GetFromY(image.Height);

                    //Right side
                    double rightFaceSideOffset = rightFacePoint.y - rightFacePoint.x * faceLineSlope;
                    PointGenerator rightFaceLine = new PointGenerator(faceLineSlope, rightFaceSideOffset);

                    Point startPointR = rightFaceLine.GetFromY(0);
                    Point endPointR = rightFaceLine.GetFromY(image.Height);

                    //CvInvoke.Line(image, startPointL, endPointL, new Bgr(Color.Green).MCvScalar, 3);
                    //CvInvoke.Line(image, startPointR, endPointR, new Bgr(Color.Blue).MCvScalar, 3);


                    //Draw mouth line
                    //Put center on the top for the mouth stay in the middle of the mouth square
                    Point mouthCenter = new Point(mouthRect.x + mouthRect.width / 2, mouthRect.y);
                    Size mouthSize = new Size(mouthRect.width / 2, mouthRect.height / 2);

                    Point mCenter = getRectCenter(mouthRect);

                    //Get mouth line generator
                    double aFactMouth = Math.Tan(Math.Atan(faceLineSlope) + Math.PI / 2);
                    double bfactMouth = mCenter.y - mCenter.x * aFactMouth;
                    PointGenerator mouthLine = new PointGenerator(aFactMouth, bfactMouth);

                    double leftFaceMouthCrossX = (bfactMouth - leftFaceSideOffset) /
                        (faceLineSlope - aFactMouth);

                    double rightFaceMouthCrossX = (bfactMouth - rightFaceSideOffset) /
                        (faceLineSlope - aFactMouth);

                    Point leftFaceMouthCross = mouthLine.GetFromX(leftFaceMouthCrossX);
                    Point rightFaceMouthCross = mouthLine.GetFromX(rightFaceMouthCrossX);

                    //Get face top line
                    double afactTopFace = aFactMouth;   //use the mouth line since this uses the same slope
                    double bfactTopFace = faceTopPoint.y - faceTopPoint.x * afactTopFace;
                    PointGenerator faceTopLine = new PointGenerator(afactTopFace, bfactTopFace);

                    double leftTopFaceCrossX = (bfactTopFace - leftFaceSideOffset) /
                         (faceLineSlope - afactTopFace);

                    double rightTopFaceCrossX = (bfactTopFace - rightFaceSideOffset) /
                        (faceLineSlope - afactTopFace);

                    Point leftTopFaceCross = faceTopLine.GetFromX(leftTopFaceCrossX);
                    Point rightTopFaceCross = faceTopLine.GetFromX(rightTopFaceCrossX);

                    //CvInvoke.Circle(image, leftTopFaceCross, 5, new Bgr(Color.Black).MCvScalar, -1);
                    //CvInvoke.Circle(image, rightTopFaceCross, 5, new Bgr(Color.Black).MCvScalar, -1);
                    //CvInvoke.Circle(image, leftFaceMouthCross, 5, new Bgr(Color.Black).MCvScalar, -1);
                    //CvInvoke.Circle(image, rightFaceMouthCross, 5, new Bgr(Color.Black).MCvScalar, -1);
                    //CvInvoke.Circle(image, faceBottomPoint, 5, new Bgr(Color.Black).MCvScalar, -1);

                    /*Point[] facePoints = new Point[]
                    {
                        leftTopFaceCross,
                        rightTopFaceCross,
                        rightFaceMouthCross,
                        faceBottomPoint,
                        leftFaceMouthCross
                    };*/

                    MatOfPoint facePointsMat = new MatOfPoint(leftTopFaceCross,
                        rightTopFaceCross,
                        rightFaceMouthCross,
                        faceBottomPoint,
                        leftFaceMouthCross);



                    //VectorOfPoint facePointsVector = new VectorOfPoint(facePoints);


                    //CvInvoke.Polylines(image, facePointsVector, true, new Bgr(172, 203, 227).MCvScalar, 1);

                    Imgproc.fillConvexPoly(newImage, facePointsMat, new Scalar(172, 203, 227));
                    //CvInvoke.FillConvexPoly(newImage, facePointsVector, new Bgr(172, 203, 227).MCvScalar);
                    //CvInvoke.FillPoly(image, facePointsVector, new Bgr(172, 203, 227).MCvScalar);

                    //CvInvoke.Ellipse(newImage, mouthCenter, mouthSize, faceFeatureAngle, 0, 180,
                    //new Bgr(System.Drawing.Color.Black).MCvScalar, 2);

                    Imgproc.ellipse(newImage, mouthCenter, mouthSize, faceFeatureAngle, 0, 180, new Scalar(0,0,0), 2);

                    Point p1 = faceTopLine.GetFromX(0);
                    Point p2 = faceTopLine.GetFromX(image.Width);
                    //CvInvoke.Line(image, p1, p2, new Bgr(Color.Black).MCvScalar, 3);

                    //Draw nose line
                    Point noseCenter = new Point(noseRect.x + noseRect.width / 2, 
                        noseRect.y + noseRect.height / 2);
                    Size noseSize = new Size(noseRect.width / 2, noseRect.height / 2);
                    double noseAngle = Math.Atan(faceLineData[0]);
                    noseAngle = RadianToDegree(noseAngle);
                    //CvInvoke.Ellipse(newImage, noseCenter, noseSize,
                        //noseAngle, 0, 180,
                        //new Bgr(System.Drawing.Color.Black).MCvScalar, 2);

                    Imgproc.ellipse(newImage, noseCenter, noseSize, noseAngle, 0, 180, new Scalar(0, 0, 0), 2);

                    //Draw eyes ellipses
                    foreach (Rect eye in personFace.GetEyes())
                    {
                        Point eyeCenter = new Point(eye.x + eye.width / 2, eye.y + eye.height / 2);
                        Size elipseSize = new Size(eye.width / 5, eye.height / 2);
                        //CvInvoke.Ellipse(newImage, eyeCenter, elipseSize, faceFeatureAngle, 0, 360, 
                            //new Bgr(System.Drawing.Color.Black).MCvScalar, -1);

                        Imgproc.ellipse(newImage, eyeCenter, elipseSize, faceFeatureAngle, 0, 360, new Scalar(0, 0, 0), -1);
                    }

                    //CvInvoke.Line(newImage, faceBottomPoint, 
                        //new Point(newImage.Width / 2, newImage.Height), new MCvScalar(0, 0, 0, 0));


                    Imgproc.line(newImage, faceBottomPoint, new Point(newImage.width() / 2, newImage.height()), new Scalar(0, 0, 0));

                }

                

                //Put the image on the picture box
                pictureBox1.Image = newImage.Bitmap;
                //pictureBox1.Image = skyImg2.Bitmap;

            }
        }

        private double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        //byte gValue = 252;
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            //pictureBox3.Image = grayImg.ThresholdToZero(new Gray(gValue--)).Bitmap;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double grayLevel = double.Parse(textBox1.Text);

            //pictureBox3.Image = grayImg.ThresholdToZero(new Gray(grayLevel)).Bitmap;
        }

        private Point getRectCenter(Rect rect)
        {
            return new Point(rect.x + rect.width / 2, rect.y + rect.height / 2);
        }
    }



    public static class ExtensionMethods
    {
        /*public static Point GetCenter(this Rectangle rect)
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }*/
    }

    public static class MatExtension
    {
        public static dynamic GetValue(this Mat mat, int row, int col)
        {
            var value = CreateElement(mat.Depth);
            Marshal.Copy(mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, value, 0, 1);
            return value[0];
        }

        public static void SetValue(this Mat mat, int row, int col, dynamic value)
        {
            var target = CreateElement(mat.Depth, value);
            Marshal.Copy(target, 0, mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, 1);
        }

        public static void SetValue(this Mat mat, int index, dynamic value)
        {
            var target = CreateElement(mat.Depth, value);
            Marshal.Copy(target, 0, mat.DataPointer + index, 1);
            //Marshal.Copy(target, 0, mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, 1);
        }

        private static dynamic CreateElement(DepthType depthType, dynamic value)
        {
            var element = CreateElement(depthType);
            element[0] = value;
            return element;
        }

        private static dynamic CreateElement(DepthType depthType)
        {
            if (depthType == DepthType.Cv8S)
            {
                return new sbyte[1];
            }
            if (depthType == DepthType.Cv8U)
            {
                return new byte[1];
            }
            if (depthType == DepthType.Cv16S)
            {
                return new short[1];
            }
            if (depthType == DepthType.Cv16U)
            {
                return new ushort[1];
            }
            if (depthType == DepthType.Cv32S)
            {
                return new int[1];
            }
            if (depthType == DepthType.Cv32F)
            {
                return new float[1];
            }
            if (depthType == DepthType.Cv64F)
            {
                return new double[1];
            }
            return new float[1];
        }
    }
}
