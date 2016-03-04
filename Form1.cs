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



        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Get the original img Mat
                Mat originalImage = new Mat(openFileDialog1.FileName, LoadImageType.Color);

                //Load the original image
                imageBox1.Image = originalImage;

                //Get gray image
                Mat grayImg = new Mat();
                CvInvoke.CvtColor(originalImage, grayImg, ColorConversion.Bgr2Gray);


                //Create paisage
                Mat illustratedImg = gradientFill(new Size(originalImage.width(), originalImage.height()));

                //Show gray image
                imageBox3.Image = grayImg;

                Mat featureImage = originalImage.Clone();

                //Run face detector on the image
                ArrayList<PersonFace> personFaces = faceDetector.Detect(featureImage);

                Mat hairImage = originalImage.Clone();

                foreach (PersonFace personFace in personFaces)
                {
                    personFace.Evaluate();

                    if (!personFace.IsValid())
                        continue;

                    drawFaceRectangles(personFace, originalImage.Clone());

                    detectSkinColor(personFace, originalImage.Clone());

                    
                    //detectHair(personFace, hairImage);


                    drawFaceFeatures(personFace, illustratedImg);

                    //----- Begin Histogram Tests ---------------


                    /*DenseHistogram hist = new DenseHistogram(256, new RangeF(0, 255));

                    Image<Gray, byte>[] imgComponents = testImg.Split();

                    //Get blue components
                    hist.Calculate(new Image<Gray, byte>[] { imgComponents[0] }, false, null);
                    float[] blueBins = hist.GetBinValues();

                    //Get green components
                    hist.Calculate(new Image<Gray, byte>[] { imgComponents[1] }, false, null);
                    float[] greenBins = hist.GetBinValues();

                    //Get red components
                    hist.Calculate(new Image<Gray, byte>[] { imgComponents[2] }, false, null);
                    float[] redBins = hist.GetBinValues();

                    double blueMean = 0;
                    for (int i = 0; i < blueBins.Length; i++)
                    {
                        blueMean += i * blueBins[i];
                    }

                    blueMean /= testImg.Width * testImg.Height;

                    double greenMean = 0;
                    for (int i = 0; i < greenBins.Length; i++)
                    {
                        greenMean += i * greenBins[i];
                    }

                    greenMean /= testImg.Width * testImg.Height;

                    double redMean = 0;
                    for (int i = 0; i < redBins.Length; i++)
                    {
                        redMean += i * redBins[i];
                    }

                    redMean /= testImg.Width * testImg.Height;

                    Scalar skinColor = new Scalar(
                        Math.Round(blueMean),
                        Math.Round(greenMean),
                        Math.Round(redMean));

                    setColorBox((byte)Math.Round(redMean), 
                        (byte)Math.Round(greenMean), 
                        (byte)Math.Round(blueMean));*/

                    //MessageBox.Show(redMean + " " + greenMean + " " + blueMean);


                    /*testImg.ROI = default(System.Drawing.Rectangle);

                    int[] blueValues = getMinMaxBins(blueBins);
                    int[] greenValues = getMinMaxBins(greenBins);
                    int[] redValues = getMinMaxBins(redBins);*/


                    //imageBox5.Image = testImg.InRange(
                    //new Bgr(blueValues[0], greenValues[0], redValues[0]), 
                    //new Bgr(blueValues[1], greenValues[1], redValues[1]));

                    //string testStr = "";

                    //foreach (float bin in blueBins)
                    //testStr += bin + " ";


                    //MessageBox.Show(testStr);

                    /* foreach (float bin in greenBins)
                         testStr += bin + " ";

                     MessageBox.Show(testStr);

                     foreach (float bin in redBins)
                         testStr += bin + " ";

                     MessageBox.Show(testStr);*/



                    //-----End Histogram Tests ----------------



                    //---------------- Hair box ------------------

                    //Top hair point
                    //Point hairTopPoint = faceLine.GetFromY(faceRect.y - faceRect.height / 3);
                    //Rect hairBox = new Rect(hairTopPoint.x - faceRect.width / 2, hairTopPoint.y,
                        //faceRect.width, faceRect.height / 3);

                    //Mat hairMat = new Mat(originalImage, new System.Drawing.Rectangle(hairBox.x, hairBox.y, 
                        //hairBox.width, hairBox.height));
                    
                    //imageBox5.Image = hairMat;

                    //Imgproc.line(hairMat, new Point(0, hairMat.height() / 2),
                    //new Point(hairMat.width(), hairMat.height() / 2), new Scalar(255, 255, 255));

                    //createHistogram(hairMat, 256, "Before");

                    //Mat transformedImg = ImgTransform.Get(hairMat.Clone());

                    //createHistogram(transformedImg, 256, "After");

                    //imageBox9.Image = transformedImg;


                    //pictureBox1.Image = ImgTransform.Get(originalImage.Bitmap);

                    //MessageBox.Show(hairMat.Size.ToString());

                    /*testImg.ROI = new System.Drawing.Rectangle(hairBox.x, hairBox.y,
                        hairBox.width, hairBox.height);*/

                    /*System.Drawing.Bitmap hairBmp = testImg.Bitmap;
                    System.Drawing.Imaging.BitmapData hairData = hairBmp.LockBits(new System.Drawing.Rectangle(0, 0, hairBmp.Width, hairBmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    byte[] hairBytes = new byte[hairData.Stride * hairData.Height];

                    Marshal.Copy(hairData.Scan0, hairBytes, 0, hairBytes.Length);

                    //-------- Handle Bytes ---------------------

                    for (int i = 0; i < hairBytes.Length; i++)
                    {
                        hairBytes[i] = 128;
                    }

                    //-------------------------------------------

                    Marshal.Copy(hairBytes, 0, hairData.Scan0, hairBytes.Length);

                    hairBmp.UnlockBits(hairData);

                    //pictureBox2.Image = hairBmp;

                    testImg.ROI = default(System.Drawing.Rectangle);*/

                    //Imgproc.rectangle(featureImage, hairBox, new Scalar(64, 64, 0),2);


                    //--------------------------------------------

                    

                }

                //Load illustracted img
                imageBox4.Image = illustratedImg;

                //Load hair detected img
                imageBox12.Image = hairImage;

            }
        }

        void detectHair(PersonFace personFace, Mat hairImage)
        {
            Rect faceRect = personFace.GetFace();

            double adjWidth = faceRect.width * 0.85;
            double adjHeight = faceRect.height * 1.2;
            double adjX = faceRect.x + (faceRect.width - adjWidth) / 2;
            double adjY = faceRect.y + (faceRect.height - adjHeight) / 2;

            Rect adjFaceRect = new Rect((int)adjX, (int)adjY, (int)adjWidth, (int)adjHeight);

            double[] faceLineData = personFace.GetFaceLineData();
            PointGenerator faceLine = new PointGenerator(faceLineData[0], faceLineData[1]);
            Point faceTopPoint = faceLine.GetFromY(adjFaceRect.y);
            Point faceBottomPoint = faceLine.GetFromY(adjFaceRect.y + adjFaceRect.height);

            //Draw face line
            //Imgproc.line(hairImage, faceTopPoint, faceBottomPoint, new Scalar(255, 0, 0), 2);

            //Get face feature angle
            double faceFeatureAngle = Math.Atan(faceLineData[0]);
            faceFeatureAngle = RadianToDegree(faceFeatureAngle);
            faceFeatureAngle += faceFeatureAngle > 0 ? -90 : 90;

            //Imgproc.rectangle(hairImage, adjFaceRect, new Scalar(0, 255, 255), 2);

            /*Imgproc.ellipse(hairImage, 
                new Point(adjFaceRect.x + adjFaceRect.width / 2, adjFaceRect.y + adjFaceRect.height / 2), 
                new Size(adjFaceRect.width/2, adjFaceRect.height/2), faceFeatureAngle, 0, 360, new Scalar(255, 0, 0), 2);

            Imgproc.ellipse(hairImage,
                new Point(adjFaceRect.x + adjFaceRect.width / 2, adjFaceRect.y + adjFaceRect.height / 2),
                new Size((int)(adjFaceRect.width / 1.8), (int)(adjFaceRect.height / 1.8)), faceFeatureAngle, 0, 360, new Scalar(255, 0, 0), 2);*/

            Mat[] imgComponents = hairImage.Split();

            for (int i = 0; i < 5; i++)
            {
                double factor = 1.8 - i * 0.2;

                Mat maskImage = new Image<Gray, byte>(hairImage.width(), hairImage.height(), new Gray(0)).Mat;

                Imgproc.ellipse(maskImage,
                    new Point(adjFaceRect.x + adjFaceRect.width / 2, adjFaceRect.y + adjFaceRect.height / 2),
                    new Size((int)(adjFaceRect.width / factor), (int)(adjFaceRect.height / factor)), faceFeatureAngle + 180, 0, 180, new Scalar(255), -1);

                Imgproc.ellipse(maskImage,
                    new Point(adjFaceRect.x + adjFaceRect.width / 2, adjFaceRect.y + adjFaceRect.height / 2),
                    new Size(adjFaceRect.width / 2, adjFaceRect.height / 2), faceFeatureAngle, 0, 360, new Scalar(0), -1);

                //imageBox13.Image = maskImage;

                Mat testImg = new Mat();

                hairImage.CopyTo(testImg, maskImage);

                imageBox13.Image = testImg;

                Stack<string> titleStack = new Stack<string>();
                titleStack.Push("Red");
                titleStack.Push("Green");
                titleStack.Push("Blue");

                HistogramForm histForm = new HistogramForm();

                foreach (Mat img in imgComponents)
                {
                    //try histogram only the upper half to detect the most probable hair color range

                    Mat hist = new Mat();
                    CvInvoke.CalcHist(new VectorOfMat(img), new int[] { 0 }, maskImage, hist, new int[] { 256 }, new float[] { 0, 255 }, false);

                    string color = titleStack.Pop();

                    histForm.AddHist(hist, color, System.Drawing.Color.FromName(color));

                    /*byte[] testBuffer = new byte[256];
                    hist.CopyTo(testBuffer);

                    string msg = "";

                    foreach (byte value in testBuffer)
                        msg += value + " ";

                    msg += Environment.NewLine;
                    msg += Environment.NewLine;

                    textBox1.AppendText(msg);*/

                }

                histForm.Show(i.ToString());

            }

            Image<Bgr, byte> testImg2 = new Image<Bgr, byte>(hairImage.Bitmap);


            imageBox13.Image = testImg2.InRange(new Bgr(25, 25, 25), new Bgr(100, 85, 100));

            //createHistogram(new Image<Bgr, byte>(maskImage.Bitmap), 256, "teste");



            /*Imgproc.ellipse(hairImage,
                new Point(adjFaceRect.x + adjFaceRect.width / 2, adjFaceRect.y + adjFaceRect.height / 2),
                new Size((int)(adjFaceRect.width / 1.6), (int)(adjFaceRect.height / 1.6)), faceFeatureAngle, 0, 360, new Scalar(255, 0, 0), 2);

            Imgproc.ellipse(hairImage,
                new Point(adjFaceRect.x + adjFaceRect.width / 2, adjFaceRect.y + adjFaceRect.height / 2),
                new Size((int)(adjFaceRect.width / 1.4), (int)(adjFaceRect.height / 1.4)), faceFeatureAngle, 0, 360, new Scalar(255, 0, 0), 2);

            Imgproc.ellipse(hairImage,
                new Point(adjFaceRect.x + adjFaceRect.width / 2, adjFaceRect.y + adjFaceRect.height / 2),
                new Size((int)(adjFaceRect.width / 1.2), (int)(adjFaceRect.height / 1.2)), faceFeatureAngle, 0, 360, new Scalar(255, 0, 0), 2);

            Imgproc.ellipse(hairImage,
                new Point(adjFaceRect.x + adjFaceRect.width / 2, adjFaceRect.y + adjFaceRect.height / 2),
                new Size((int)(adjFaceRect.width / 1), (int)(adjFaceRect.height / 1)), faceFeatureAngle, 0, 360, new Scalar(255, 0, 0), 2);*/
        }

        void drawFaceRectangles(PersonFace personFace, Mat featureImage)
        {
            Rect faceRect = personFace.GetFace();
            Rect mouthRect = personFace.GetMouth();
            Rect noseRect = personFace.GetNose();
            Rect[] eyesRects = personFace.GetEyes();

            double[] faceLineData = personFace.GetFaceLineData();
            PointGenerator faceLine = new PointGenerator(faceLineData[0], faceLineData[1]);
            Point faceTopPoint = faceLine.GetFromY(faceRect.y);
            Point faceBottomPoint = faceLine.GetFromY(faceRect.y + faceRect.height);

            //Draw face line
            Imgproc.line(featureImage, faceTopPoint, faceBottomPoint, new Scalar(255, 0, 0), 1);

            //Draw rect around the face
            Imgproc.rectangle(featureImage, faceRect, new Scalar(0, 255, 255), 2);

            //Draw circle around face
            Point faceCenter = new Point(faceRect.x + faceRect.width / 2, faceRect.y + faceRect.height / 2);
            Size faceSize = new Size(faceRect.width / 2, faceRect.height / 2);
            Imgproc.ellipse(featureImage, faceCenter,faceSize,0,0,360,new Scalar(172, 203, 227),0);
            Imgproc.ellipse(featureImage, faceCenter, faceSize, 0, 0, 360,new Scalar(0,0,0), 1);

            //Draw rect around the mouth
            Imgproc.rectangle(featureImage, mouthRect, new Scalar(0, 255, 255), 1);

            //Draw rect around the nose
            Imgproc.rectangle(featureImage, noseRect, new Scalar(0, 255, 255), 1);

            //Draw eyes rect and circles
            for (var i = 0; i < eyesRects.Length; i++)
            {
                Rect eye = eyesRects[i];
                Imgproc.rectangle(featureImage, eye, new Scalar(0, 255, 255), 1);
            }

            imageBox2.Image = featureImage;
        }

        void detectSkinColor(PersonFace face, Mat skinColorImage)
        {
            Rect[] eyesRects = face.GetEyes();
            Rect[] cheeksRect = new Rect[eyesRects.Length];
            Rect faceRect = face.GetFace();

            //Get cheek rectangles
            for (var i = 0; i < eyesRects.Length; i++)
            {
                Rect eye = eyesRects[i];

                int skinWidth = eye.width / 2;

                //Create rectangle for skin detection
                cheeksRect[i] = new Rect(eye.x + (eye.width - skinWidth) / 2, eye.y + eye.height + 5, skinWidth, eye.height / 2);
                Imgproc.rectangle(skinColorImage, cheeksRect[i], new Scalar(255, 0, 0));
            }

            //Get Forehead rectangle

            int fhWidth = faceRect.width / 4;
            int fhHeight = faceRect.width / 5;

            Rect fhRect = new Rect(faceRect.x + (faceRect.width - fhWidth) / 2,
                faceRect.y + 5, fhWidth, fhHeight);

            Imgproc.rectangle(skinColorImage, fhRect, new Scalar(255, 0, 0));


            //Get rects pixel means and calculate skin color

            //Forehead roi
            Mat fhArea = new Mat(skinColorImage, new System.Drawing.Rectangle(
                fhRect.x, fhRect.y, fhRect.width, fhRect.height));

            //Forehead mean
            MCvScalar fhMean = CvInvoke.Mean(fhArea);

            //Skin color mean
            MCvScalar skinMean = pointsMean(fhMean);

            foreach (Rect cheekRect in cheeksRect)
            {
                //Cheek roi
                Mat cheekArea = new Mat(skinColorImage, new System.Drawing.Rectangle(
                cheekRect.x, cheekRect.y, cheekRect.width, cheekRect.height));

                //Cheek mean
                MCvScalar cheekMean = CvInvoke.Mean(cheekArea);

                //Updates skin mean
                skinMean = pointsMean(skinMean, cheekMean);
            }

            //Set image of the rectangles
            imageBox11.Image = skinColorImage;

            //Set skin color image
            pictureBox2.Image = new Image<Bgr, byte>(200, 200,
                new Bgr(skinMean.V0, skinMean.V1, skinMean.V2)).Bitmap;
        }

        void drawFaceFeatures(PersonFace personFace, Mat illustratedImg)
        {

            Rect faceRect = personFace.GetFace();
            Rect mouthRect = personFace.GetMouth();
            Rect noseRect = personFace.GetNose();
            Rect[] eyesRects = personFace.GetEyes();

            //Draw face division line
            double[] faceLineData = personFace.GetFaceLineData();
            PointGenerator faceLine = new PointGenerator(faceLineData[0], faceLineData[1]);
            Point faceTopPoint = faceLine.GetFromY(faceRect.y);
            Point faceBottomPoint = faceLine.GetFromY(faceRect.y + faceRect.height);

            //Imgproc.line(illustratedImg, faceTopPoint, faceBottomPoint, new Scalar(255, 0, 0), 1);

            //Get face feature angle
            double faceFeatureAngle = Math.Atan(faceLineData[0]);
            faceFeatureAngle = RadianToDegree(faceFeatureAngle);
            faceFeatureAngle += faceFeatureAngle > 0 ? -90 : 90;

            //Draw face lateral boundaries lines
            //Detect right and left eye
            Rect rightEye, leftEye;
            if (eyesRects[0].x > eyesRects[1].x)
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
            Point endPointL = leftFaceLine.GetFromY(illustratedImg.Height);

            //Right side
            double rightFaceSideOffset = rightFacePoint.y - rightFacePoint.x * faceLineSlope;
            PointGenerator rightFaceLine = new PointGenerator(faceLineSlope, rightFaceSideOffset);

            Point startPointR = rightFaceLine.GetFromY(0);
            Point endPointR = rightFaceLine.GetFromY(illustratedImg.Height);


            //Imgproc.line(illustratedImg, startPointL, endPointL, new Scalar(0,255,0), 5);
            //Imgproc.line(illustratedImg, startPointR, endPointR,new Scalar(255,0,0), 3);

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


            /*CvInvoke.Circle(illustratedImg, leftTopFaceCross, 5, new MCvScalar(), -1);
            CvInvoke.Circle(illustratedImg, rightTopFaceCross, 5, new MCvScalar(), -1);
            CvInvoke.Circle(illustratedImg, leftFaceMouthCross, 5, new MCvScalar(), -1);
            CvInvoke.Circle(illustratedImg, rightFaceMouthCross, 5, new MCvScalar(), -1);
            CvInvoke.Circle(illustratedImg, faceBottomPoint, 5, new MCvScalar(), -1);*/


            MatOfPoint facePointsMat = new MatOfPoint(leftTopFaceCross,
                rightTopFaceCross,
                rightFaceMouthCross,
                faceBottomPoint,
                leftFaceMouthCross);


            //CvInvoke.Polylines(image, facePointsVector, true, new Bgr(172, 203, 227).MCvScalar, 1);

            Imgproc.fillConvexPoly(illustratedImg, facePointsMat, new Scalar(255,255,255));


            Imgproc.ellipse(illustratedImg, mouthCenter, mouthSize, faceFeatureAngle, 0, 180, new Scalar(0,0,0), 2);

            Point p1 = faceTopLine.GetFromX(0);
            Point p2 = faceTopLine.GetFromX(illustratedImg.Width);

            //Imgproc.line(illustratedImg, p1, p2, new Scalar(0, 0, 0), 3);




            //Draw nose line
            Point noseCenter = new Point(noseRect.x + noseRect.width / 2,
                noseRect.y + noseRect.height / 2);
            Size noseSize = new Size(noseRect.width / 2, noseRect.height / 2);
            double noseAngle = Math.Atan(faceLineData[0]);
            noseAngle = RadianToDegree(noseAngle);



            Imgproc.ellipse(illustratedImg, noseCenter, noseSize, noseAngle, 0, 180, new Scalar(0, 0, 0), 2);




            //Draw eyes ellipses
            foreach (Rect eye in personFace.GetEyes())
            {
                Point eyeCenter = new Point(eye.x + eye.width / 2, eye.y + eye.height / 2);
                Size ellipseSize = new Size(eye.width / 5, eye.height / 2);

                Imgproc.ellipse(illustratedImg, eyeCenter, ellipseSize, faceFeatureAngle, 0, 360, new Scalar(0, 0, 0), -1);
            }

            Imgproc.line(illustratedImg, faceBottomPoint, new Point(illustratedImg.width() / 2, illustratedImg.height()), new Scalar(0, 0, 0));


        }


        /*void createHistogram(IImage image, int bins, string title)
        {
            HistogramForm histForm = new HistogramForm();
            histForm.SetHistogram(image, bins, title);
            histForm.Show();
        }

        void createHistogram(Mat hist,string title)
        {
            HistogramForm histForm = new HistogramForm();
            histForm.ShowHist(hist, title);
            histForm.Show();
        }*/


        int[] getMinMaxBins(float[] bins)
        {
            //Create groups of the pixel ranges
            List<List<int>> groups = new List<List<int>>();
            List<int> currentGroup = null;

            for (int i = 0; i < bins.Length; i++)
            {
                float bin = bins[i];

                if (bin < 1)
                {
                    currentGroup = null;
                }
                else
                {
                    if (currentGroup == null)
                    {
                        currentGroup = new List<int>();
                        groups.Add(currentGroup);
                    }

                    currentGroup.Add(i);
                }
            }

            if (groups.Count == 0)
                return new int[] { 0, 255 };

            //Get largest group
            List<int> largestGroup = groups[0];

            foreach (List<int> group in groups)
            {
                if (group.Count > largestGroup.Count)
                    largestGroup = group;
            }

            int minValue = largestGroup[0];
            int maxValue = largestGroup[largestGroup.Count - 1];

            return new int[] { minValue, maxValue };
        }

        double euclideanDistance(MCvScalar point1, MCvScalar point2)
        {
            return Math.Sqrt(
                Math.Pow(point1.V0 - point2.V0, 2) +
                Math.Pow(point1.V1 - point2.V1, 2) +
                Math.Pow(point1.V2 - point2.V2, 2) +
                Math.Pow(point1.V3 - point2.V3, 2)
            );
        }

        MCvScalar pointsMean(params MCvScalar[] points)
        {
            double V0 = 0, V1 = 0, V2 = 0, V3 = 0;

            int pointsLength = points.Length;

            foreach(MCvScalar point in points)
            {
                V0 += point.V0;
                V1 += point.V1;
                V2 += point.V2;
                V3 += point.V3;
            }

            return new MCvScalar(
                V0 / pointsLength,
                V1 / pointsLength,
                V2 / pointsLength,
                V3 / pointsLength
            );            
        }

        private double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        public Mat gradientFill(Size size)
        {
            System.Drawing.Bitmap gradImg = new System.Drawing.Bitmap(size.width, size.height);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(gradImg);
            LinearGradientBrush grad = new LinearGradientBrush(
                new System.Drawing.Rectangle(0, 0, gradImg.Width, gradImg.Height),
                System.Drawing.Color.Blue,
                System.Drawing.Color.White,
                LinearGradientMode.Vertical);

            graphics.FillRectangle(grad, grad.Rectangle);

            return new Image<Bgr, byte>(gradImg).Mat;
        }

        void setColorBox(byte red, byte green, byte blue)
        {
            System.Drawing.Bitmap colorBox = new System.Drawing.Bitmap(200, 200);
            System.Drawing.Imaging.BitmapData colorData = colorBox.LockBits(new System.Drawing.Rectangle(0, 0, 200, 200),
                System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            byte[] colorArr = new byte[colorData.Height * colorData.Stride];

            for (int i = 0; i < colorArr.Length; i++)
            {
                switch(i%3)
                {
                    case 0:
                        colorArr[i] = blue;
                        break;

                    case 1:
                        colorArr[i] = green;
                        break;

                    case 2:
                        colorArr[i] = red;
                        break;
                }
            }

            Marshal.Copy(colorArr, 0, colorData.Scan0, colorArr.Length);

            colorBox.UnlockBits(colorData);

            //pictureBox1.Image = colorBox;
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
