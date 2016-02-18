//----------------------------------------------------------------------------
//  Copyright (C) 2004-2016 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

#define CVCuda

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
#if !(__IOS__ || NETFX_CORE)
using Emgu.CV.Cuda;
#endif



namespace ImagineAlpha
{
    public class FaceDetector
    {
        List<CascadeClassifier> frontFaceCascades = new List<CascadeClassifier>();
        List<CascadeClassifier> eyesCascades = new List<CascadeClassifier>();
        List<CascadeClassifier> nosesCascades = new List<CascadeClassifier>();
        List<CascadeClassifier> mouthsCascades = new List<CascadeClassifier>();

        public FaceDetector(String[] frontFaceCascadeFiles,
            String[] eyesCascadeFiles,
            String[] nosesCascadeFiles,
            String[] mouthsCascadeFiles)
        {
            foreach (String cascadeFile in frontFaceCascadeFiles)
            {
                frontFaceCascades.Add(new CascadeClassifier(cascadeFile));
            }

            foreach (String cascadeFile in eyesCascadeFiles)
            {
                eyesCascades.Add(new CascadeClassifier(cascadeFile));
            }

            foreach (String cascadeFile in nosesCascadeFiles)
            {
                nosesCascades.Add(new CascadeClassifier(cascadeFile));
            }

            foreach (String cascadeFile in mouthsCascadeFiles)
            {
                mouthsCascades.Add(new CascadeClassifier(cascadeFile));
            }
        }

        public List<PersonFace> Detect(Mat image)
        {
            List<PersonFace> personFaces = new List<PersonFace>();

            using (UMat ugray = new UMat())
            {
                CvInvoke.CvtColor(image, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                //normalizes brightness and increases contrast of the image
                CvInvoke.EqualizeHist(ugray, ugray);

                foreach (CascadeClassifier faceCascade in frontFaceCascades)
                {
                    Rectangle[] detectedFaces = faceCascade.DetectMultiScale(ugray,1.1,3,new Size(20, 20));

                    //faces.AddRange(detectedFaces);

                    foreach (Rectangle face in detectedFaces)
                    {
                        PersonFace personFace = new PersonFace(face);

                        personFaces.Add(personFace);

                        //personFace.FaceRect = face;

                        //Get the region of interest on the faces
                        using (UMat faceRegion = new UMat(ugray, face))
                        {
                            //Detect eyes
                            foreach (CascadeClassifier eyeCascade in eyesCascades)
                            {
                                Rectangle[] detectedEyes = eyeCascade.DetectMultiScale(
                                    faceRegion,
                                    1.1,
                                    3,
                                    new Size(10, 10));

                                //List<Rectangle> eyes = new List<Rectangle>();

                                foreach (Rectangle eye in detectedEyes)
                                {
                                    //Ensure the eyes are in the upper half of the img region
                                    //if (eye.Y + eye.Height > faceRegion.Size.Height / 2)
                                        //continue;

                                    eye.Offset(face.X, face.Y);
                                    personFace.AddEye(eye);
                                    //eyes.Add(eye);
                                }

                                //personFace.EyesRects = eyes.ToArray();

                            }

                            //Detect mouths
                            foreach (CascadeClassifier mouthCascade in mouthsCascades)
                            {
                               /*UMat mouthRegion = new UMat(ugray, new Rectangle(
                                    face.X,
                                    face.Y + face.Height / 2,
                                    face.Width,
                                    face.Height/2));*/

                                Rectangle[] detectedMouths = mouthCascade.DetectMultiScale(
                                    faceRegion,
                                    1.1,
                                    3,
                                    new Size(25, 15));

                                foreach (Rectangle mouth in detectedMouths)
                                {
                                    mouth.Offset(face.X, face.Y);
                                    personFace.AddMouth(mouth);
                                }
                            }

                            //Detect noses
                            List<Rectangle> noses = new List<Rectangle>();
                            foreach (CascadeClassifier noseCascade in nosesCascades)
                            {
                                Rectangle[] detectedNoses = noseCascade.DetectMultiScale(
                                    faceRegion,
                                    1.1,
                                    10,
                                    new Size(25, 15));

                                foreach (Rectangle nose in detectedNoses)
                                {
                                    nose.Offset(face.X, face.Y);
                                    noses.Add(nose);
                                }

                            }

                            personFace.NoseRects = noses.ToArray();

                        }
                    }
                    
                }


            }

            return personFaces;
        }

    }


    public class PersonFace
    {
        Rectangle face;
        public PersonFace(Rectangle faceRect) { face = faceRect; }
        public Rectangle GetFace() { return face; }

        List<Rectangle> mouths = new List<Rectangle>();
        
        public void AddMouth(Rectangle mouth) { mouths.Add(mouth); }

        Rectangle evaluatedMouth = new Rectangle(0, 0, 0, 0);
        public Rectangle GetMouth() { return evaluatedMouth; }

        List<Rectangle> eyes = new List<Rectangle>();
        List<Rectangle> evaluatedEyes = new List<Rectangle>();
        public void AddEye(Rectangle eye) { eyes.Add(eye); }
        public Rectangle[] GetEyes() { return evaluatedEyes.ToArray(); }

        bool isValid = false;
        public bool IsValid(){ return isValid; }

        double faceLineSlope = 0;
        double faceLineOffset = 0;
        public double[] GetFaceLineData() { return new double[] { faceLineSlope, faceLineOffset }; }


        //Function to cross all informations added to this face and evaluate the best values
        public void Evaluate()
        {
            //Evaluate mouth
            evaluatedMouth = new Rectangle(0, 0, 0, 0);
            Random randomizer = new Random();

            //evaluatedMouth = mouths[randomizer.Next(0, mouths.Count - 1)];

            //must work a few on the mouth to choose the best one and proceed to histogram check for try to determinate skin color, eye color, hair color etc..

            foreach (Rectangle mouth in mouths)
            {
                if (mouth.Y < face.Y + face.Height / 2)
                    continue;

                if (evaluatedMouth.Width > mouth.Width)
                    continue;

                evaluatedMouth = mouth;
            }

            //Evaluate eyes
            evaluatedEyes = new List<Rectangle>();
            List<Rectangle> rightCandidates = new List<Rectangle>();
            List<Rectangle> leftCandidates = new List<Rectangle>();

            foreach (Rectangle eye in eyes)
            {
                //Ensure the eyes are in the upper half of the img region
                if (eye.Y + eye.Height / 2 > face.Y + face.Height / 2)
                    continue;

                if (eye.X + eye.Width / 2 < face.X + face.Width / 2)
                    rightCandidates.Add(eye);
                else
                    leftCandidates.Add(eye);
            }

            //get centers for each side weighted by their areas
            int totalAreas = 0;
            int totalX = 0;
            int totalY = 0;

            if(rightCandidates.Count > 0)
            {
                foreach (Rectangle eye in rightCandidates)
                {
                    int eyeArea = eye.Width * eye.Height;
                    totalAreas += eyeArea;

                    totalX += (eye.X + eye.Width / 2) * eyeArea;
                    totalY += (eye.Y + eye.Height / 2) * eyeArea;
                }

                Point rightPoint = new Point(totalX / totalAreas, totalY / totalAreas);

                int rightEyeSide = (int)Math.Sqrt((double)totalAreas / (double)rightCandidates.Count);

                Rectangle rightEye = new Rectangle(rightPoint, new Size(rightEyeSide, rightEyeSide));
                rightEye.Offset(-rightEye.Width / 2, -rightEye.Height / 2);

                evaluatedEyes.Add(rightEye);
            }

            if(leftCandidates.Count > 0)
            {
                totalAreas = 0;
                totalX = 0;
                totalY = 0;

                foreach (Rectangle eye in leftCandidates)
                {
                    int eyeArea = eye.Width * eye.Height;
                    totalAreas += eyeArea;

                    totalX += (eye.X + eye.Width / 2) * eyeArea;
                    totalY += (eye.Y + eye.Height / 2) * eyeArea;
                }

                Point leftPoint = new Point(totalX / totalAreas, totalY / totalAreas);

                int leftEyeSide = (int)Math.Sqrt((double)totalAreas / (double)leftCandidates.Count);

                Rectangle leftEye = new Rectangle(leftPoint, new Size(leftEyeSide, leftEyeSide));

                leftEye.Offset(-leftEye.Width / 2, -leftEye.Height / 2);

                evaluatedEyes.Add(leftEye);
            }

            //Check if it is valid
            isValid = false;

            if (evaluatedEyes.Count > 2)
                throw new Exception("Eyes count must be equal or less than two");

            if (evaluatedEyes.Count == 2)
            {
                isValid = true;

                //Get the face line data

                Point eye1Center = new Point(evaluatedEyes[0].X + evaluatedEyes[0].Width / 2,
                    evaluatedEyes[0].Y + evaluatedEyes[0].Height / 2);

                Point eye2Center = new Point(evaluatedEyes[1].X + evaluatedEyes[1].Width / 2,
                    evaluatedEyes[1].Y + evaluatedEyes[1].Height / 2);

                int xOffset = (eye2Center.X - eye1Center.X) / 2;
                int yOffset = (eye2Center.Y - eye1Center.Y) / 2;

                Point eyeLineCenter = new Point(eye1Center.X + xOffset, eye1Center.Y + yOffset);

                int zeroDivFac = eye1Center.X == eye2Center.X ? 1 : 0;

                //Generate face line slope and offset
                double aFact = (double)(eye1Center.Y - eye2Center.Y) /
                    (double)(eye1Center.X - eye2Center.X + zeroDivFac);

                aFact = Math.Atan(aFact) + Math.PI / 2;
                aFact = Math.Tan(aFact);

                double bFact = eyeLineCenter.Y - aFact * eyeLineCenter.X;

                faceLineSlope = aFact;
                faceLineOffset = bFact;

                //If the mouth is invalid, project a new based on the face line
                if (evaluatedMouth.Width == 0)
                {
                    PointGenerator faceLinePoint = new PointGenerator(aFact, bFact);

                    Point projMouthPos = faceLinePoint.GetFromY(face.Y + face.Height * 0.8);

                    evaluatedMouth = new Rectangle(projMouthPos, new Size(face.Width / 3, face.Height/5));

                    evaluatedMouth.Offset(-evaluatedMouth.Width / 2, -evaluatedMouth.Height / 2);
                }
            }

            if (evaluatedEyes.Count == 1 && evaluatedMouth.Width > 0)
            {
                isValid = true;

                //Project the other eye based on the mouth

                //Get the bottom mouth coords
                Point mouthBottomCenter = new Point(evaluatedMouth.Width / 2 +
                    evaluatedMouth.X,
                    evaluatedMouth.Bottom);

                //get the facetop coords
                Point faceTopCenter = new Point(face.Width / 2 +
                    face.X, face.Y);

                //Apply an experimental correct factor to the values
                int correctFact = mouthBottomCenter.X - faceTopCenter.X;
                //correctFact = (int)(correctFact * 0.5);

                mouthBottomCenter.X += correctFact;
                faceTopCenter.X -= correctFact;

                //Get the slope of the faceline

                //In case they are the same value, add a pixel to prevent division by 0
                int zeroDivFac = mouthBottomCenter.X == faceTopCenter.X ? 1 : 0;

                double a = (double)(mouthBottomCenter.Y - faceTopCenter.Y) /
                        (double)(mouthBottomCenter.X - faceTopCenter.X + zeroDivFac);

                //Get the offset of the face line
                double b = mouthBottomCenter.Y - a * mouthBottomCenter.X;

                faceLineSlope = a;
                faceLineOffset = b;

                //Get the line function of the face
                PointGenerator faceLinePoint = new PointGenerator(a, b);

                //Get the reference of the existing eye and its center point
                Rectangle eyeRef = evaluatedEyes[0];
                Point eyeCenter = new Point(eyeRef.X + eyeRef.Width / 2, eyeRef.Y + eyeRef.Height / 2);

                //Get the slope of the eye line (it must be normal to the face line, so we turn it Pi/2
                double aEyeFact = Math.Atan(a) + Math.PI / 2;
                aEyeFact = Math.Tan(aEyeFact);

                //Get the eye line offset
                double bEyeFact = eyeCenter.Y - aEyeFact * eyeCenter.X;

                //Get the line function of the eye
                PointGenerator eyeLinePoint = new PointGenerator(aEyeFact, bEyeFact);

                //Get the horizontal difference between the center of the existing eye and the face line
                int diff = faceLinePoint.GetFromY(eyeCenter.Y).X - eyeCenter.X;

                //Get the project eye coords
                Point projEyePoint = eyeLinePoint.GetFromX(eyeCenter.X + diff * 2);
                
                //Get the project eye rectangle
                Rectangle projEyeRect = new Rectangle(projEyePoint, eyeRef.Size);
                projEyeRect.Offset(-eyeRef.Width / 2, -eyeRef.Height / 2);

                evaluatedEyes.Add(projEyeRect);
            }              

            //If the face keep invalid, put the face line on the middle of the face square
            if(!isValid)
            {
                faceLineSlope = -face.Height/0.01;
                faceLineOffset = face.Y - faceLineSlope * face.X + face.Width / 2;
            }
        }




        public Rectangle[] NoseRects { get; set; }
        

        //Comparators
        /*int RectAreaComparator(Rectangle a, Rectangle b)
        {
            if (a.Height * a.Width > b.Height * b.Width)
                return 1;

            return -1;
        }

        int RectYCenterComparator(Rectangle a, Rectangle b)
        {
            if (a.Y + a.Height / 2 > b.Y + b.Height / 2)
                return 1;
            return -1;
        }

        int RectXCenterComparator(Rectangle a, Rectangle b)
        {
            if (a.X + a.Width / 2 > b.X + b.Width / 2)
                return 1;
            return -1;
        }*/

    }

    /*
   public static class DetectFace
   {
      public static void Detect(
        Mat image, String faceFileName, String eyeFileName, 
        List<Rectangle> faces, List<Rectangle> eyes, 
        bool tryUseCuda,
        out long detectionTime)
      {
         Stopwatch watch;
         
         #if !(__IOS__ || NETFX_CORE)
         if (tryUseCuda && CudaInvoke.HasCuda)
         {
            using (CudaCascadeClassifier face = new CudaCascadeClassifier(faceFileName))
            using (CudaCascadeClassifier eye = new CudaCascadeClassifier(eyeFileName))
            {
               face.ScaleFactor = 1.1;
               face.MinNeighbors = 10;
               face.MinObjectSize = Size.Empty;
               eye.ScaleFactor = 1.1;
               eye.MinNeighbors = 10;
               eye.MinObjectSize = Size.Empty;
               watch = Stopwatch.StartNew();
               using (CudaImage<Bgr, Byte> gpuImage = new CudaImage<Bgr, byte>(image))
               using (CudaImage<Gray, Byte> gpuGray = gpuImage.Convert<Gray, Byte>())
               using (GpuMat region = new GpuMat())
               {
                  face.DetectMultiScale(gpuGray, region);
                  Rectangle[] faceRegion = face.Convert(region);
                  faces.AddRange(faceRegion);
                  foreach (Rectangle f in faceRegion)
                  {
                     using (CudaImage<Gray, Byte> faceImg = gpuGray.GetSubRect(f))
                     {
                        //For some reason a clone is required.
                        //Might be a bug of CudaCascadeClassifier in opencv
                        using (CudaImage<Gray, Byte> clone = faceImg.Clone(null))
                        using (GpuMat eyeRegionMat = new GpuMat())
                        {
                           eye.DetectMultiScale(clone, eyeRegionMat);
                           Rectangle[] eyeRegion = eye.Convert(eyeRegionMat);
                           foreach (Rectangle e in eyeRegion)
                           {
                              Rectangle eyeRect = e;
                              eyeRect.Offset(f.X, f.Y);
                              eyes.Add(eyeRect);
                           }
                        }
                     }
                  }
               }
               watch.Stop();
            }
         }
         else
         #endif
         {
            //Read the HaarCascade objects
            using (CascadeClassifier face = new CascadeClassifier(faceFileName))
            using (CascadeClassifier eye = new CascadeClassifier(eyeFileName))
            {
               watch = Stopwatch.StartNew();
               using (UMat ugray = new UMat())
               {
                  CvInvoke.CvtColor(image, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                  //normalizes brightness and increases contrast of the image
                  CvInvoke.EqualizeHist(ugray, ugray);

                  //Detect the faces  from the gray scale image and store the locations as rectangle
                  //The first dimensional is the channel
                  //The second dimension is the index of the rectangle in the specific channel
                  Rectangle[] facesDetected = face.DetectMultiScale(
                     ugray,
                     1.1,
                     10,
                     new Size(20, 20));
                     
                  faces.AddRange(facesDetected);

                  foreach (Rectangle f in facesDetected)
                  {
                     //Get the region of interest on the faces
                     using (UMat faceRegion = new UMat(ugray, f))
                     {
                        Rectangle[] eyesDetected = eye.DetectMultiScale(
                           faceRegion,
                           1.1,
                           10,
                           new Size(20, 20));
                        
                        foreach (Rectangle e in eyesDetected)
                        {
                           Rectangle eyeRect = e;
                           eyeRect.Offset(f.X, f.Y);
                           eyes.Add(eyeRect);
                        }
                     }
                  }
               }
               watch.Stop();
            }
         }
         detectionTime = watch.ElapsedMilliseconds;
      }
   }

    */
}
