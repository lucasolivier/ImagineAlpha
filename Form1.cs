using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Cuda;

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

                CvInvoke.CvtColor(image, grayImg, ColorConversion.Bgr2Gray);

                pictureBox2.Image = grayImg.Bitmap;
                //pictureBox3.Image = grayImg.ThresholdToZero(new Gray(253)).Bitmap;

                List<PersonFace> personFaces = faceDetector.Detect(image);

                foreach (PersonFace personFace in personFaces)
                {
                    personFace.Evaluate();

                    if (!personFace.IsValid())
                        continue;

                    //----- Begin Histogram Tests ---------------

                    Image<Bgr, byte> testImg = new Image<Bgr, byte>(image.Bitmap);


                    you will get the most used values from the histogram and compare to pre created colors to find the one that matches better

                    must verify which color spaces we will compare <- this later

                        for the mvp, everyone will be white rgb 227,203,172

                    //testImg.InRange()

                    pictureBox3.Image = testImg.Bitmap;

                    testImg.ROI = personFace.GetFace();
                    grayImg.ROI = personFace.GetFace();

                    createHistogram(testImg, 256);
                    createHistogram(grayImg, 256);

                    pictureBox2.Image = grayImg.Bitmap;
                    pictureBox3.Image = testImg.Bitmap;


                    //-----End Histogram Tests ----------------



                    Rectangle faceRect = personFace.GetFace();
                    Rectangle mouthRect = personFace.GetMouth();

                    double[] faceLineData = personFace.GetFaceLineData();

                    PointGenerator faceLine = new PointGenerator(faceLineData[0], faceLineData[1]);
                    Point faceTopPoint = faceLine.GetFromY(faceRect.Y);
                    Point faceBottomPoint = faceLine.GetFromY(faceRect.Bottom);

                    CvInvoke.Line(image, faceTopPoint, faceBottomPoint, new Bgr(Color.Orange).MCvScalar, 2);

                    CvInvoke.Rectangle(image, faceRect, new Bgr(Color.Yellow).MCvScalar, 2);
                    CvInvoke.Rectangle(image, mouthRect, new Bgr(Color.Blue).MCvScalar, 2);

                    Point faceCenter = new Point(faceRect.X + faceRect.Width / 2, faceRect.Y + faceRect.Height / 2);

                    Size faceSize = new Size(faceRect.Width / 2, faceRect.Height / 2);

                    double faceFeatureAngle = Math.Atan(faceLineData[0]);
                    faceFeatureAngle = RadianToDegree(faceFeatureAngle);

                    if (faceFeatureAngle > 0)
                        faceFeatureAngle -= 90;
                    else
                        faceFeatureAngle += 90;

                    //CvInvoke.Ellipse(image, faceCenter,faceSize,0,0,360,new Bgr(Color.Aquamarine).MCvScalar, -1);
                    
                    /*CvInvoke.Ellipse(image, faceCenter, faceSize, 0, 0, 360, 
                        new Bgr(Color.Black).MCvScalar, 1);*/

                    //Put center on the top for the mouth stay in the middle of the mouth square
                    Point mouthCenter = new Point(mouthRect.X + mouthRect.Width / 2, mouthRect.Y);

                    Size mouthSize = new Size(mouthRect.Width / 2, mouthRect.Height / 2);

                    CvInvoke.Ellipse(image, mouthCenter, mouthSize, faceFeatureAngle, 0, 180,
                        new Bgr(Color.Black).MCvScalar, 2);

                    

                    foreach (Rectangle eye in personFace.GetEyes())
                    {
                        CvInvoke.Rectangle(image, eye, new Bgr(Color.White).MCvScalar, 2);

                        Point eyeCenter = new Point(eye.X + eye.Width / 2, eye.Y + eye.Height / 2);
                        Size elipseSize = new Size(eye.Width / 5, eye.Height / 2);

                        CvInvoke.Ellipse(image, eyeCenter, elipseSize, faceFeatureAngle, 0, 360, new Bgr(Color.Black).MCvScalar, -1);
                    }

                    foreach (Rectangle nose in personFace.NoseRects)
                    {
                        //CvInvoke.Rectangle(image, nose, new Bgr(Color.Black).MCvScalar, 2);
                    }
                }

                pictureBox1.Image = image.Bitmap;

            }
        }

        private double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        byte gValue = 252;
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            //pictureBox3.Image = grayImg.ThresholdToZero(new Gray(gValue--)).Bitmap;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double grayLevel = double.Parse(textBox1.Text);

            //pictureBox3.Image = grayImg.ThresholdToZero(new Gray(grayLevel)).Bitmap;
        }
    }

    public class PointGenerator
    {
        double aFact;
        double bFact;

        public PointGenerator(double aFact, double bFact)
        {
            this.aFact = aFact;
            this.bFact = bFact;
        }

        public PointGenerator(Point p1, Point p2)
        {
            if (p1.X == p2.X)
                p1.X += 1;

            aFact = (double)(p1.Y - p2.Y) / (double)(p1.X - p2.X);
            bFact = p1.Y - aFact * p1.X;
        }

        public Point GetFromX(double xValue)
        {
            return new Point((int)xValue, (int)(aFact * xValue + bFact));
        }

        public Point GetFromY(double yValue)
        {
            return new Point((int)((yValue - bFact) / aFact), (int)yValue);
        }
    }
}
