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
            //Run();
        }

        public Bitmap Run(string imgPath)
        {
            //Mat image = new Mat("lena.jpg", LoadImageType.Color); //Read the files as an 8-bit Bgr image
            Mat image = new Mat(imgPath, LoadImageType.Color); //Read the files as an 8-bit Bgr image   
           // long detectionTime;
            List<Rectangle> faces1 = new List<Rectangle>();
            List<Rectangle> faces2 = new List<Rectangle>();
            List<Rectangle> faces3 = new List<Rectangle>();
            List<Rectangle> faces4 = new List<Rectangle>();
            List<Rectangle> faces5 = new List<Rectangle>();

            List<Rectangle> eyes = new List<Rectangle>();

            List<Rectangle> mouth = new List<Rectangle>();

            //The cuda cascade classifier doesn't seem to be able to load "haarcascade_frontalface_default.xml" file in this release
            //disabling CUDA module for now
            //bool tryUseCuda = true;

            /*DetectFace.Detect(
              image, "haarcascade_frontalface_alt.xml", "haarcascade_eye.xml",
              faces1, eyes,
              tryUseCuda,
              out detectionTime);

            DetectFace.Detect(
                image, "haarcascade_frontalface_alt_tree.xml", "haarcascade_eye.xml",
                faces2, eyes,
                tryUseCuda,
                out detectionTime);*/

            /*DetectFace.Detect(
                image, "haarcascade_frontalface_alt2.xml", "haarcascade_eye.xml",
                faces3, eyes,
                tryUseCuda,
                out detectionTime);*/

           /* DetectFace.Detect(
                image, "haarcascade_frontalface_default.xml", "haarcascade_eye.xml",
                faces4, eyes,
                tryUseCuda,
                out detectionTime);

            DetectFace.Detect(
                image, "haarcascade_frontalface_default2.xml", "haarcascade_eye.xml",
                faces5, eyes,
                tryUseCuda,
                out detectionTime);*/
            /*
            foreach (Rectangle face in faces1)
                CvInvoke.Rectangle(image, face, new Bgr(Color.Red).MCvScalar, 2);

            foreach (Rectangle face in faces2)
                CvInvoke.Rectangle(image, face, new Bgr(Color.Blue).MCvScalar, 2);
            */
            foreach (Rectangle face in faces3)
                CvInvoke.Rectangle(image, face, new Bgr(Color.Yellow).MCvScalar, 2);
            /*
            foreach (Rectangle face in faces4)
                CvInvoke.Rectangle(image, face, new Bgr(Color.White).MCvScalar, 2);

            foreach (Rectangle face in faces5)
                CvInvoke.Rectangle(image, face, new Bgr(Color.Black).MCvScalar, 2);*/

            label1.Text = "Alt Red -> " + faces1.Count;
            label2.Text = "Alt Tree Blue -> " + faces2.Count;
            label3.Text = "Alt 2 Yellow -> " + faces3.Count;
            label4.Text = "Default Pink -> " + faces4.Count;
            label5.Text = "Default 2 Black -> " + faces5.Count;



            foreach (Rectangle eye in eyes)
                CvInvoke.Rectangle(image, eye, new Bgr(Color.White).MCvScalar, 2);

            return image.Bitmap;

            //display the image 
            /*ImageViewer.Show(image, String.Format(
               "Completed face and eye detection using {0} in {1} milliseconds",
               (tryUseCuda && CudaInvoke.HasCuda) ? "GPU"
               : CvInvoke.UseOpenCL ? "OpenCL"
               : "CPU",
               detectionTime));*/
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Mat image = new Mat(openFileDialog1.FileName, LoadImageType.Color);

                /* float scale = 1f;

                CvInvoke.Resize(image, image, new Size((int)(image.Width * scale), (int)(image.Height * scale)));*/

                List<PersonFace> personFaces = faceDetector.Detect(image);

                foreach(PersonFace personFace in personFaces)
                {
                    personFace.Evaluate();

                    if (!personFace.IsValid())
                        continue;

                    CvInvoke.Rectangle(image, personFace.GetFace(), new Bgr(Color.Yellow).MCvScalar, 2);
                    CvInvoke.Rectangle(image, personFace.GetMouth(), new Bgr(Color.Blue).MCvScalar, 2);

                    foreach(Rectangle eye in personFace.GetEyes())
                    {
                        CvInvoke.Rectangle(image, eye, new Bgr(Color.White).MCvScalar, 2);
                    }

                    foreach (Rectangle nose in personFace.NoseRects)
                    {
                        //CvInvoke.Rectangle(image, nose, new Bgr(Color.Black).MCvScalar, 2);
                    }


                    continue;

                    if (personFace.GetMouth().Width == 0)
                        continue;

                    Point mouthBottomCenter = new Point(personFace.GetMouth().Width / 2 +
                        personFace.GetMouth().X,
                        personFace.GetMouth().Height +
                        personFace.GetMouth().Y);

                    Point faceTopCenter = new Point(personFace.GetFace().Width/2 +
                        personFace.GetFace().X, personFace.GetFace().Y);

                    int correctFact = mouthBottomCenter.X - faceTopCenter.X;
                    correctFact = (int)(correctFact * 0.5);

                    mouthBottomCenter.X += correctFact;
                    faceTopCenter.X -= correctFact;

                    //CvInvoke.Line(image, mouthCenter, faceTopCenter,new Bgr(Color.Orange).MCvScalar);
                    //CvInvoke.Line(image, mouthTopCenter, faceTopCenter, new Bgr(Color.White).MCvScalar);
                    CvInvoke.Line(image, mouthBottomCenter, faceTopCenter, new Bgr(Color.Orange).MCvScalar);

                    //Get the face line function y = ax + b
                    //b = ?
                    //a = (Y2 - Y1) / (X2 - X1)

                    int a = (mouthBottomCenter.Y - faceTopCenter.Y) /
                        (mouthBottomCenter.X - faceTopCenter.X);

                    int b = mouthBottomCenter.Y - a * mouthBottomCenter.X;

                    PointGenerator faceLinePoint = new PointGenerator(a, b);

                    //teste
                    Point point1 = faceLinePoint.GetFromY(0);
                    Point point2 = faceLinePoint.GetFromY(image.Height);
                    CvInvoke.Line(image, point1, point2, new Bgr(Color.Yellow).MCvScalar);

                    if (personFace.GetEyes().Length != 1)
                        continue;

                    //Eye line
                    Rectangle eyeRef = personFace.GetEyes()[0];
                    Point eyeCenter = new Point(eyeRef.X + eyeRef.Width / 2, eyeRef.Y + eyeRef.Height / 2);

                    double aEyeFact = Math.Atan(a) + Math.PI / 2;
                    aEyeFact = Math.Tan(aEyeFact);

                    double bEyeFact = eyeCenter.Y - aEyeFact * eyeCenter.X;

                    PointGenerator eyeLinePoint = new PointGenerator(aEyeFact, bEyeFact);
                    Point eyePoint1 = eyeLinePoint.GetFromX(0);
                    Point eyePoint2 = eyeLinePoint.GetFromX(image.Width);

                    CvInvoke.Line(image, eyePoint1, eyePoint2, new Bgr(Color.Blue).MCvScalar);

                    int diff = faceLinePoint.GetFromY(eyeCenter.Y).X - eyeCenter.X;

                    Point projEyePoint = eyeLinePoint.GetFromX(eyeCenter.X + diff * 2);
                    Rectangle projEyeRect = new Rectangle(projEyePoint, eyeRef.Size);
                    projEyeRect.Offset(-eyeRef.Width / 2, -eyeRef.Height / 2);

                    //MessageBox.Show(diff.ToString());

                    /*CvInvoke.Circle(image,
                        projEyePoint, 
                        20, 
                        new Bgr(Color.Pink).MCvScalar, 
                        3);*/

                    CvInvoke.Rectangle(image, projEyeRect, new Bgr(Color.Pink).MCvScalar, 2);

                }


                /*foreach (Rectangle face in faces)
                    CvInvoke.Rectangle(image, face, new Bgr(Color.Yellow).MCvScalar, 2);

                foreach (Rectangle mouth in mouths)
                {
                    CvInvoke.Rectangle(image, mouth, new Bgr(Color.Blue).MCvScalar, 2);
                    CvInvoke.Line(image, 
                        new Point(mouth.Width / 2 + mouth.X, mouth.Height / 2 + mouth.Y),
                        new Point(image.Width/ 2 + mouth.X, 0 + mouth.Y),
                        new Bgr(Color.Orange).MCvScalar);
                }

                foreach (Rectangle nose in noses)
                    CvInvoke.Rectangle(image, nose, new Bgr(Color.Red).MCvScalar, 2);

                foreach (Rectangle eye in eyes)
                    CvInvoke.Rectangle(image, eye, new Bgr(Color.White).MCvScalar, 2);

                label1.Text = "Faces: " + faces.Count;
                label2.Text = "Eyes: " + eyes.Count;
                label3.Text = "Noses: " + noses.Count;
                label4.Text = "Mouths: " + mouths.Count;*/

                pictureBox1.Image = image.Bitmap;

                //pictureBox1.Image = Run(openFileDialog1.FileName);

            }
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
