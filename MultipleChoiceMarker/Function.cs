using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace MultipleChoiceMarker
{
    public static class Function
    {
        public class mcData
        {
            public string Subject;
            public int NoOfQuestions;
            public double MarksForEachQuestion;
            public double CorrectPercentageToPass;
            public List<string[]> StudentAnswer;
            public List<string[]> AnswerKeys;
        }
        public class Choice
        {
            public string choice { get; set; }
        }

        public static String[] GetFilesFrom(String searchFolder, String[] filters)
        {
            List<String> files = new List<String>();
            foreach (var filter in filters)
            {
                files.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), SearchOption.TopDirectoryOnly));
            }
            return files.ToArray();
        }
        public static bool isInt(string s)
        {
            bool result = false;
            int r = 0;
            result = int.TryParse(s, out r);
            return result;
        }
        public static bool isDouble(string s)
        {
            bool result = false;
            double r = 0;
            result = double.TryParse(s, out r);
            return result;
        }
        public static bool isWithinRange(double value, double low, double high)
        {
            if (value >= low && value <= high) 
                return true;
            else
                return false;
        }
        public static bool isChoice(string choice)
        {
            if (!(choice == "A" || choice == "B" || choice == "C" || choice == "D"))
                return false;
            else
                return true;
        }
        

        public static Bitmap[] findRectangle(Bitmap image)
        {
            Bitmap[] r = new Bitmap[4];
            FiltersSequence commonSeq = new FiltersSequence();
            commonSeq.Add(Grayscale.CommonAlgorithms.BT709); //Grayscale
            commonSeq.Add(new BradleyLocalThresholding()); //Grayscale > Black or White
            commonSeq.Add(new Invert()); //black to white, white to black
            Bitmap FilteredImage = commonSeq.Apply(image);

            BlobCounter blobCounter = new BlobCounter();
            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 50;
            blobCounter.MinWidth = 50;
            blobCounter.ObjectsOrder = ObjectsOrder.YX;
            blobCounter.ProcessImage(FilteredImage);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
            int count = 0;
            foreach (var blob in blobs)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blob);
                List<IntPoint> cornerPoints;
                if (shapeChecker.IsQuadrilateral(edgePoints, out cornerPoints) && blob.Rectangle.Width > 500 && blob.Rectangle.Height > 500)
                {
                    if (count >= 4) break; //error
                    r[count] = CropImage(FilteredImage, blob.Rectangle);
                    count++;
                }
            }

            return r;
        }
        public static Bitmap CropImage(Bitmap image, Rectangle rect)
        {
            // create filter
            Crop filter = new Crop(rect);
            // apply the filter
            Bitmap newImage = filter.Apply(image);
            return newImage;
        }
        public static string getClass(Bitmap img)
        {
            Bitmap form_img = CropImage(img, new Rectangle(0, 0, img.Width, img.Height / 5));
            int form = GetChoice(form_img, 6, ObjectsOrder.YX, true);
            Bitmap class_img = CropImage(img, new Rectangle(0, img.Height / 5, img.Width, img.Height / 5));
            int st_classid = GetChoice(class_img, 6, ObjectsOrder.YX, true);
            string st_class = "";
            switch (st_classid)
            {
                case 1:
                    st_class = "A";
                    break;
                case 2:
                    st_class = "B";
                    break;
                case 3:
                    st_class = "C";
                    break;
                case 4:
                    st_class = "D";
                    break;
                case 5:
                    st_class = "E";
                    break;
                case 6:
                    st_class = "F";
                    break;
            }

            return form.ToString() + st_class.ToString();
        }
        public static int getClassNo(Bitmap img)
        {
            int classno = 0;
            Bitmap classno_img = CropImage(img, new Rectangle(0, img.Height / 5 * 2, img.Width, img.Height / 5 * 3));
            classno = GetChoice(classno_img, 45, ObjectsOrder.YX, true);
            return classno;
        }

        public static string[] getStudentAns(Bitmap img0, Bitmap img1, Bitmap img2, Bitmap img3, int noOfQuestions)
        {
            string[] answers = new string[noOfQuestions + 2];
            Bitmap[] questionShot = new Bitmap[60];
            for (int i = 0; i <= noOfQuestions; i++)
            {
                if (i >= 20) break;
                questionShot[i] = CropImage(img1, new Rectangle(0, i * img1.Height / 20, img1.Width, img1.Height / 20)); //divide into 20 parts
            }
            if(noOfQuestions > 20)
                for (int i = 20; i <= noOfQuestions - 1; i++)
                {
                    questionShot[i] = CropImage(img2, new Rectangle(0, (i - 20) * img2.Height / 20, img2.Width, img2.Height / 20));
                }
            if (noOfQuestions > 40)
                for (int i = 40; i <= noOfQuestions - 1; i++)
                {
                    questionShot[i] = CropImage(img3, new Rectangle(0, (i - 40) * img3.Height / 20, img3.Width, img3.Height / 20));
                }
            for (int i = 0; i <= noOfQuestions - 1; i++)
            {
                answers[0] = getClass(img0);
                answers[1] = getClassNo(img0).ToString();
                int ans = GetChoice(questionShot[i], 4);
                char choice = 'X';
                switch (ans)
                {
                    case 1:
                        choice = 'A';
                        break;
                    case 2:
                        choice = 'B';
                        break;
                    case 3:
                        choice = 'C';
                        break;
                    case 4:
                        choice = 'D';
                        break;
                }
                answers[i + 2] = choice.ToString();
            }
            return answers;
        }

        public static int GetChoice(Bitmap row, int noOfChoicesProvieded, ObjectsOrder Order = ObjectsOrder.XY, bool notAnAnswer = false)
        {
            BlobCounter blobCounter = new BlobCounter();

            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 50;
            blobCounter.MinWidth = 50;
            blobCounter.ObjectsOrder = Order;
            blobCounter.ProcessImage(row);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            int i = 1;
            double minfullness = 0;
            int choice = 0;
            int count = 0;
            int blacken = 0;
            foreach (var blob in blobs)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blob);
                AForge.Point center;
                float radius;
                if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                {
                    if (blob.Fullness >= .5 && blob.Fullness > minfullness)
                    {
                        choice = i;
                        blacken++;
                        minfullness = blob.Fullness;
                    }
                    i++;
                    count++;
                }
            }
            if (blacken > 1 && !notAnAnswer) choice = 0; //blackened more than one circle
            if (count > noOfChoicesProvieded) choice = 0; //error (found extra circle)
            return choice;
        }
        public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter writer = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                writer = new StreamWriter(filePath, append);
                serializer.Serialize(writer, objectToWrite);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        public static T ReadFromXmlFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                reader = new StreamReader(filePath);
                return (T)serializer.Deserialize(reader);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
    }
}
