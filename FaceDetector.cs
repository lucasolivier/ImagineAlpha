using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCVJavaInterface;

using Emgu.CV;
using Emgu.CV.Structure;

namespace ImagineAlpha
{
    public class FaceDetector
    {
        ArrayList<CascadeClassifier> frontFaceCascades = new ArrayList<CascadeClassifier>();
        ArrayList<CascadeClassifier> eyesCascades = new ArrayList<CascadeClassifier>();
        ArrayList<CascadeClassifier> nosesCascades = new ArrayList<CascadeClassifier>();
        ArrayList<CascadeClassifier> mouthsCascades = new ArrayList<CascadeClassifier>();

        public FaceDetector(String[] frontFaceCascadeFiles,
            String[] eyesCascadeFiles,
            String[] nosesCascadeFiles,
            String[] mouthsCascadeFiles)
        {
            foreach (String cascadeFile in frontFaceCascadeFiles)
            {
                frontFaceCascades.add(new CascadeClassifier(cascadeFile));
            }

            foreach (String cascadeFile in eyesCascadeFiles)
            {
                eyesCascades.add(new CascadeClassifier(cascadeFile));
            }

            foreach (String cascadeFile in nosesCascadeFiles)
            {
                nosesCascades.add(new CascadeClassifier(cascadeFile));
            }

            foreach (String cascadeFile in mouthsCascadeFiles)
            {
                mouthsCascades.add(new CascadeClassifier(cascadeFile));
            }
        }

        public ArrayList<PersonFace> Detect(Mat image)
        {
            ArrayList<PersonFace> personFaces = new ArrayList<PersonFace>();


            using (UMat ugray = new UMat())
            {
                CvInvoke.CvtColor(image, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                //normalizes brightness and increases contrast of the image
                CvInvoke.EqualizeHist(ugray, ugray);

                foreach (CascadeClassifier faceCascade in frontFaceCascades)
                {
                    Rect[] detectedFaces = faceCascade.detectMultiScale(ugray, 1.1, 3, 0, new Size(20, 20));

                    //faces.AddRange(detectedFaces);

                    foreach (Rect face in detectedFaces)
                    {
                        PersonFace personFace = new PersonFace(face);

                        personFaces.add(personFace);

                        //personFace.FaceRect = face;

                        //Get the region of interest on the faces

                        using (UMat faceRegion = new UMat(ugray, new System.Drawing.Rectangle(
                            face.x,
                            face.y,
                            face.width,
                            face.height)))
                        {
                            //Detect eyes
                            foreach (CascadeClassifier eyeCascade in eyesCascades)
                            {
                                Rect[] detectedEyes = eyeCascade.detectMultiScale(
                                    faceRegion,
                                    1.1,
                                    3,
                                    0,
                                    new Size(10, 10));

                                //List<Rectangle> eyes = new List<Rectangle>();

                                foreach (Rect eye in detectedEyes)
                                {
                                    //Ensure the eyes are in the upper half of the img region
                                    //if (eye.Y + eye.Height > faceRegion.Size.Height / 2)
                                    //continue;

                                    //eye.Offset(face.X, face.Y);
                                    personFace.AddEye(new Rect(eye.x + face.x, eye.y + face.y,
                                        eye.width, eye.height));
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

                                Rect[] detectedMouths = mouthCascade.detectMultiScale(
                                    faceRegion,
                                    1.1,
                                    3,
                                    0,
                                    new Size(25, 15));

                                foreach (Rect mouth in detectedMouths)
                                {
                                    //mouth.Offset(face.X, face.Y);
                                    //personFace.AddMouth(mouth);
                                    personFace.AddMouth(new Rect(mouth.x + face.x, mouth.y + face.y,
                                        mouth.width, mouth.height));
                                }
                            }

                            //Detect noses
                            ArrayList<Rect> noses = new ArrayList<Rect>();
                            foreach (CascadeClassifier noseCascade in nosesCascades)
                            {
                                Rect[] detectedNoses = noseCascade.detectMultiScale(
                                    faceRegion,
                                    1.1,
                                    10,
                                    0,
                                    new Size(25, 15));

                                foreach (Rect nose in detectedNoses)
                                {
                                    //nose.Offset(face.X, face.Y);
                                    noses.add(new Rect(nose.x + face.x, nose.y + face.y,
                                        nose.width, nose.height));
                                }

                            }

                            //personFace.NoseRects = noses.ToArray();

                        }
                    }

                }


            }

            return personFaces;
        }

    }
}
