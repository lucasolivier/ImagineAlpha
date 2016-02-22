using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCVJavaInterface;

namespace ImagineAlpha
{
    public class PersonFace
    {
        Rect face;
        public PersonFace(Rect faceRect) { face = faceRect; }
        public Rect GetFace() { return face; }

        ArrayList<Rect> mouths = new ArrayList<Rect>();

        public void AddMouth(Rect mouth) { mouths.add(mouth); }

        Rect evaluatedMouth = new Rect(0, 0, 0, 0);
        public Rect GetMouth() { return evaluatedMouth; }

        ArrayList<Rect> eyes = new ArrayList<Rect>();
        ArrayList<Rect> evaluatedEyes = new ArrayList<Rect>();
        public void AddEye(Rect eye) { eyes.add(eye); }
        public Rect[] GetEyes() { return evaluatedEyes.toArray(); }

        public Rect GetNose()
        {
            PointGenerator faceLinePoint = new PointGenerator(faceLineSlope, faceLineOffset);

            Size noseSize = new Size(face.width / 7, face.height / 7);

            Point projNosePos = faceLinePoint.GetFromY(evaluatedMouth.y - noseSize.height);

            Rect createdNose = new Rect(
                projNosePos.x - noseSize.width / 2,
                projNosePos.y - noseSize.height / 2,
                noseSize.width,
                noseSize.height);

            return createdNose;
        }

        bool isValid = false;
        public bool IsValid() { return isValid; }

        double faceLineSlope = 0;
        double faceLineOffset = 0;
        public double[] GetFaceLineData() { return new double[] { faceLineSlope, faceLineOffset }; }


        //Function to cross all informations added to this face and evaluate the best values
        public void Evaluate()
        {
            //Evaluate mouth
            evaluatedMouth = new Rect(0, 0, 0, 0);
            //Random randomizer = new Random();

            //evaluatedMouth = mouths[randomizer.Next(0, mouths.Count - 1)];

            //must work a few on the mouth to choose the best one and proceed to histogram check for try to determinate skin color, eye color, hair color etc..

            foreach (Rect mouth in mouths)
            {
                if (mouth.y < face.y + face.height / 2)
                    continue;

                if (evaluatedMouth.width > mouth.width)
                    continue;

                evaluatedMouth = mouth;
            }

            //Evaluate eyes
            evaluatedEyes = new ArrayList<Rect>();
            ArrayList<Rect> rightCandidates = new ArrayList<Rect>();
            ArrayList<Rect> leftCandidates = new ArrayList<Rect>();

            foreach (Rect eye in eyes)
            {
                //Ensure the eyes are in the upper half of the img region
                if (eye.y + eye.height / 2 > face.y + face.height / 2)
                    continue;

                if (eye.x + eye.width / 2 < face.x + face.width / 2)
                    rightCandidates.add(eye);
                else
                    leftCandidates.add(eye);
            }

            //get centers for each side weighted by their areas
            int totalAreas = 0;
            int totalX = 0;
            int totalY = 0;

            if (rightCandidates.size() > 0)
            {
                foreach (Rect eye in rightCandidates)
                {
                    int eyeArea = eye.width * eye.height;
                    totalAreas += eyeArea;

                    totalX += (eye.x + eye.width / 2) * eyeArea;
                    totalY += (eye.y + eye.height / 2) * eyeArea;
                }

                Point rightPoint = new Point(totalX / totalAreas, totalY / totalAreas);

                int rightEyeSide = (int)Math.Sqrt((double)totalAreas / (double)rightCandidates.size());

                Rect rightEye = new Rect(
                    rightPoint.x - rightEyeSide / 2,
                    rightPoint.y - rightEyeSide / 2,
                    rightEyeSide,
                    rightEyeSide);

                //rightEye.Offset(-rightEye.Width / 2, -rightEye.Height / 2);

                evaluatedEyes.add(rightEye);
            }

            if (leftCandidates.size() > 0)
            {
                totalAreas = 0;
                totalX = 0;
                totalY = 0;

                foreach (Rect eye in leftCandidates)
                {
                    int eyeArea = eye.width * eye.height;
                    totalAreas += eyeArea;

                    totalX += (eye.x + eye.width / 2) * eyeArea;
                    totalY += (eye.y + eye.height / 2) * eyeArea;
                }

                Point leftPoint = new Point(totalX / totalAreas, totalY / totalAreas);

                int leftEyeSide = (int)Math.Sqrt((double)totalAreas / (double)leftCandidates.size());

                Rect leftEye = new Rect(
                    leftPoint.x - leftEyeSide / 2,
                    leftPoint.y - leftEyeSide / 2,
                    leftEyeSide,
                    leftEyeSide);

                //leftEye.Offset(-leftEye.Width / 2, -leftEye.Height / 2);

                evaluatedEyes.add(leftEye);
            }

            //Check if it is valid
            isValid = false;

            if (evaluatedEyes.size() > 2)
                throw new Exception("Eyes count must be equal or less than two");

            if (evaluatedEyes.size() == 2)
            {
                isValid = true;

                //Get the face line data

                Point eye1Center = new Point(evaluatedEyes.get(0).x + evaluatedEyes.get(0).width / 2,
                    evaluatedEyes.get(0).y + evaluatedEyes.get(0).height / 2);

                Point eye2Center = new Point(evaluatedEyes.get(1).x + evaluatedEyes.get(1).width / 2,
                    evaluatedEyes.get(1).y + evaluatedEyes.get(1).height / 2);

                int xOffset = (eye2Center.x - eye1Center.x) / 2;
                int yOffset = (eye2Center.y - eye1Center.y) / 2;

                Point eyeLineCenter = new Point(eye1Center.x + xOffset, eye1Center.y + yOffset);

                int zeroDivFac = eye1Center.x == eye2Center.x ? 1 : 0;

                //Generate face line slope and offset
                double aFact = (double)(eye1Center.y - eye2Center.y) /
                    (double)(eye1Center.x - eye2Center.x + zeroDivFac);

                aFact = Math.Atan(aFact) + Math.PI / 2;
                aFact = Math.Tan(aFact);

                double bFact = eyeLineCenter.y - aFact * eyeLineCenter.x;

                faceLineSlope = aFact;
                faceLineOffset = bFact;

                //If the mouth is invalid, project a new based on the face line
                if (evaluatedMouth.width == 0)
                {
                    PointGenerator faceLinePoint = new PointGenerator(aFact, bFact);

                    Point projMouthPos = faceLinePoint.GetFromY(face.y + face.height * 0.8);

                    evaluatedMouth = new Rect(
                        projMouthPos.x - (face.width / 3) / 2,
                        projMouthPos.y - (face.height / 5) / 2,
                        face.width / 3,
                        face.height / 5);

                    //evaluatedMouth.Offset(-evaluatedMouth.Width / 2, -evaluatedMouth.Height / 2);
                }
            }

            if (evaluatedEyes.size() == 1 && evaluatedMouth.width > 0)
            {
                isValid = true;

                //Project the other eye based on the mouth

                //Get the bottom mouth coords
                Point mouthBottomCenter = new Point(
                    evaluatedMouth.x + evaluatedMouth.width / 2,
                    evaluatedMouth.y + evaluatedMouth.height);

                //get the facetop coords
                Point faceTopCenter = new Point(face.width / 2 +
                    face.x, face.y);

                //Apply an experimental correct factor to the values
                int correctFact = mouthBottomCenter.x - faceTopCenter.x;
                //correctFact = (int)(correctFact * 0.5);

                mouthBottomCenter.x += correctFact;
                faceTopCenter.x -= correctFact;

                //Get the slope of the faceline

                //In case they are the same value, add a pixel to prevent division by 0
                int zeroDivFac = mouthBottomCenter.x == faceTopCenter.x ? 1 : 0;

                double a = (double)(mouthBottomCenter.y - faceTopCenter.y) /
                        (double)(mouthBottomCenter.x - faceTopCenter.x + zeroDivFac);

                //Get the offset of the face line
                double b = mouthBottomCenter.y - a * mouthBottomCenter.x;

                faceLineSlope = a;
                faceLineOffset = b;

                //Get the line function of the face
                PointGenerator faceLinePoint = new PointGenerator(a, b);

                //Get the reference of the existing eye and its center point
                Rect eyeRef = evaluatedEyes.get(0);
                Point eyeCenter = new Point(eyeRef.x + eyeRef.width / 2, eyeRef.y + eyeRef.height / 2);

                //Get the slope of the eye line (it must be normal to the face line, so we turn it Pi/2
                double aEyeFact = Math.Atan(a) + Math.PI / 2;
                aEyeFact = Math.Tan(aEyeFact);

                //Get the eye line offset
                double bEyeFact = eyeCenter.y - aEyeFact * eyeCenter.x;

                //Get the line function of the eye
                PointGenerator eyeLinePoint = new PointGenerator(aEyeFact, bEyeFact);

                //Get the horizontal difference between the center of the existing eye and the face line
                int diff = faceLinePoint.GetFromY(eyeCenter.y).x - eyeCenter.x;

                //Get the project eye coords
                Point projEyePoint = eyeLinePoint.GetFromX(eyeCenter.x + diff * 2);

                //Get the project eye rectangle
                Rect projEyeRect = new Rect(
                    projEyePoint.x - eyeRef.width / 2,
                    projEyePoint.y - eyeRef.height / 2,
                    eyeRef.width,
                    eyeRef.height);
                //projEyeRect.Offset(-eyeRef.Width / 2, -eyeRef.Height / 2);

                evaluatedEyes.add(projEyeRect);
            }

            //If the face keep invalid, put the face line on the middle of the face square
            if (!isValid)
            {
                faceLineSlope = -face.height / 0.01;
                faceLineOffset = face.y - faceLineSlope * face.x + face.width / 2;
            }
        }




        //public Rectangle[] NoseRects { get; set; }


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
}
