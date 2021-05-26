using iTextSharp.text.pdf;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;

namespace MultipleChoiceMarker
{
    public partial class Viewer : MetroWindow
    {
        Function.mcData mcData;
        List<string[]> AnswerKeys = new List<string[]>();
        List<string[]> Review = new List<string[]>();
        List<string[]> Result = new List<string[]>();
        string projLocation = string.Empty;

        public Viewer(Function.mcData data, string ProjectLocation = "")
        {
            InitializeComponent();
            projLocation = ProjectLocation;
            mcData = data;
            label_projectName.Content = mcData.Subject;
            textBox_MarksForEachQuestion.Text = mcData.MarksForEachQuestion.ToString();
            textBox_CorrectPercentageToPass.Text = (mcData.CorrectPercentageToPass * 100).ToString();
            Binding();
            updateResult();
        }
        private void Binding()
        {
            AnswerKeys = new List<string[]>(mcData.AnswerKeys);
            for (int i = 0; i < mcData.NoOfQuestions; i++)
            {
                DataGridComboBoxColumn comboBoxClmn = new DataGridComboBoxColumn();
                comboBoxClmn.Header = "Question " + (i + 1).ToString();
                comboBoxClmn.SelectedValueBinding = new Binding("[" + i + "]");
                comboBoxClmn.ItemsSource = new ObservableCollection<string>() { "A", "B", "C", "D"};
                dataGrid_AnsKey.Columns.Add(comboBoxClmn);
            }
            dataGrid_AnsKey.ItemsSource = AnswerKeys;

            Review = new List<string[]>(mcData.StudentAnswer);

            DataGridTextColumn Class = new DataGridTextColumn();
            Class.Header = "Class";
            Class.Binding = new Binding("[0]");
            dataGrid_Review.Columns.Add(Class);
            DataGridTextColumn ClassNo = new DataGridTextColumn();
            ClassNo.Header = "Class Number";
            ClassNo.Binding = new Binding("[1]");
            dataGrid_Review.Columns.Add(ClassNo);

            for (int i = 0; i < mcData.NoOfQuestions; i++)
            {
                DataGridComboBoxColumn comboBoxClmn = new DataGridComboBoxColumn();
                comboBoxClmn.Header = "Question " + (i + 1).ToString();
                comboBoxClmn.SelectedValueBinding = new Binding("[" + (i + 2) + "]");
                comboBoxClmn.ItemsSource = new ObservableCollection<string>() { "A", "B", "C", "D", "X" };
                dataGrid_Review.Columns.Add(comboBoxClmn);
            }

            dataGrid_Review.ItemsSource = Review;
        }

        private void updateResult()
        {
            dataGrid_Result.ItemsSource = null;
            dataGrid_Result.Items.Clear();
            Result.Clear();
            foreach (string[] r in Review)
            {
                string[] result_tmp = new string[8];
                result_tmp[0] = r[0]; //class
                result_tmp[1] = r[1]; //class no.

                int attempt = 0;
                int correct = 0;

                for (int i = 2; i < mcData.NoOfQuestions + 2; i++)
                {
                    if (r[i] != "X")
                    {
                        attempt++;
                        if (r[i] == AnswerKeys[0][i - 2])
                            correct++;
                    }
                }

                result_tmp[2] = correct.ToString(); //no. of correct
                result_tmp[3] = (correct * mcData.MarksForEachQuestion).ToString(); //marks

                double percentage = (correct / (double)mcData.NoOfQuestions);
                result_tmp[4] = Math.Round(percentage, 2, MidpointRounding.AwayFromZero).ToString(); //correct percentage

                Result.Add(result_tmp);
            }
            dataGrid_Result.ItemsSource = Result;
        }

        private void button_updateResult_Click(object sender, RoutedEventArgs e)
        {
            updateResult();
        }
        private iTextSharp.text.Document WriteResult(iTextSharp.text.Document doc)
        {
            //Overall
            doc.Add(BriefReport(getStudentMark()));

            //Classes
            List<string> Classes = new List<string>();
            for (int i = 0; i < NoOfStudent(); i++)
                if (!Classes.Contains(Review[i][0]))
                    Classes.Add(Review[i][0]);

            Classes.Sort();
            foreach (string Class in Classes)
            {
                List<double> marks = getStudentMark(Class);
                doc.Add(BriefReport(marks, "Brief Report For Class " + Class));
            }

            //Chart
            var PassPercentage = iTextSharp.text.Image.GetInstance(ChartToByte(PassPercentageChart()));
            var AverageMarkOfClasses = iTextSharp.text.Image.GetInstance(ChartToByte(MeanMarkOfClassesChart()));
            PassPercentage.ScalePercent(50f);
            AverageMarkOfClasses.ScalePercent(50f);

            doc.Add(PassPercentage);
            doc.NewPage();
            doc.Add(AverageMarkOfClasses);
            doc.NewPage();
            doc.Add(QuestionAnalysis());
            doc.Add(new iTextSharp.text.Paragraph("Note: Students who did not answer the question are not included in the above table"));

            return doc;
        }
        private PdfPTable BriefReport(List<double> marks, string title = "Brief Report For Whole Form")
        {
            PdfPTable table = new PdfPTable(8);
            table.WidthPercentage = 100;
            
            PdfPCell cell = new PdfPCell(new iTextSharp.text.Phrase(title)) { Colspan = 8, HorizontalAlignment = 1};
            table.AddCell(cell);

            PdfPCell cell_noOfStudents = new PdfPCell(new iTextSharp.text.Phrase("Number of students"));
            PdfPCell cell_mean = new PdfPCell(new iTextSharp.text.Phrase("Mean"));
            PdfPCell cell_median = new PdfPCell(new iTextSharp.text.Phrase("Median"));
            PdfPCell cell_mode = new PdfPCell(new iTextSharp.text.Phrase("Mode"));
            PdfPCell cell_sd = new PdfPCell(new iTextSharp.text.Phrase("S.D."));
            PdfPCell cell_highest = new PdfPCell(new iTextSharp.text.Phrase("Highest"));
            PdfPCell cell_lowest = new PdfPCell(new iTextSharp.text.Phrase("Lowest"));
            PdfPCell cell_passpercentage = new PdfPCell(new iTextSharp.text.Phrase("Pass Percentage"));

            table.AddCell(cell_noOfStudents);
            table.AddCell(cell_mean);
            table.AddCell(cell_median);
            table.AddCell(cell_mode);
            table.AddCell(cell_sd);
            table.AddCell(cell_highest);
            table.AddCell(cell_lowest);
            table.AddCell(cell_passpercentage);

            //Next Row
            double higest = HighestMark(marks);
            double lowest = LowestMark(marks);
            double mean = Mean(marks);
            double median = Median(marks);
            double mode = Mode(marks);
            double sd = StandardDeviation(marks);
            double passpercentage = PassPercentage(marks);

            table.AddCell(new PdfPCell(new iTextSharp.text.Phrase(marks.Count().ToString())));
            table.AddCell(new PdfPCell(new iTextSharp.text.Phrase(mean.ToString("N2"))));
            table.AddCell(new PdfPCell(new iTextSharp.text.Phrase(median.ToString("N2"))));
            if(Mode(marks) > 0)
                table.AddCell(new PdfPCell(new iTextSharp.text.Phrase(mode.ToString("N2"))));
            else
                table.AddCell(new PdfPCell(new iTextSharp.text.Phrase("-")));
            table.AddCell(new PdfPCell(new iTextSharp.text.Phrase(sd.ToString("N2"))));
            table.AddCell(new PdfPCell(new iTextSharp.text.Phrase(higest.ToString("N2"))));
            table.AddCell(new PdfPCell(new iTextSharp.text.Phrase(lowest.ToString("N2"))));
            table.AddCell(new PdfPCell(new iTextSharp.text.Phrase((passpercentage * 100).ToString("N2") + "%")));
            return table;
        }
        private List<double> getStudentMark(string Class = "")
        {
            List<double> markList = new List<double>();
            if (Class.Length > 0)
            {
                for (int i = 0; i < NoOfStudent(); i++)
                {
                    if(Result[i][0] == Class)
                        markList.Add(Convert.ToDouble(Result[i][3]));
                }
                
            }
            else
            {
                for (int i = 0; i < NoOfStudent(); i++)
                {
                    markList.Add(Convert.ToDouble(Result[i][3]));
                }
            }
            return markList;
        }
        private int NoOfQuestion()
        {
            return mcData.NoOfQuestions;
        }
        private int NoOfStudent()
        {
            return Review.Count();
        }
        private double Mean(List<double> list)
        {
            double avg = list.Average();
            return Math.Round(avg, 2, MidpointRounding.AwayFromZero);
        }
        private double Median(List<double> list)
        {
            double med = 0;
            list.Sort();
            int size = list.Count();
            
            if (size % 2 != 0)
                med = Math.Round(list[size / 2], 2, MidpointRounding.AwayFromZero);
            else {
                med = Math.Round((list[size / 2 - 1] + list[size / 2]) / 2f, 2, MidpointRounding.AwayFromZero);
            }
            return med;
        }
        private double Mode(List<double> list)
        {
            var counter = new Dictionary<double, int>();
            foreach (double m in list)
            {
                if (counter.ContainsKey(m))
                    counter[m]++;
                else
                    counter.Add(m, 1);
            }

            double mode = 0;
            int highest = 0;
            foreach (KeyValuePair<double, int> pair in counter)
            {
                if (pair.Value > highest)
                {
                    mode = pair.Key;
                    highest = pair.Value;
                }
            }
            if (highest == 0) return -1.0;
            return Math.Round(mode, 2, MidpointRounding.AwayFromZero);
        }

        private double HighestMark(List<double> list)
        {
            double highest = 0;
            foreach (double m in list)
                if (m > highest)
                    highest = m;
            return Math.Round(highest, 2, MidpointRounding.AwayFromZero);
        }

        private double LowestMark(List<double> list)
        {
            double lowest = list[0];
            foreach (double m in list)
                if (m < lowest)
                    lowest = m;
            return Math.Round(lowest, 2, MidpointRounding.AwayFromZero);
        }

        private double PassPercentage(List<double> list)
        {
            double fullMark = mcData.NoOfQuestions * mcData.MarksForEachQuestion;
            double passmark = mcData.CorrectPercentageToPass * fullMark;
            int noOfStudentPass = 0;
            foreach (double m in list)
                if (m > passmark)
                    noOfStudentPass++;
            return Math.Round(noOfStudentPass / (double)NoOfStudent(), 4, MidpointRounding.AwayFromZero);
        }

        private double StandardDeviation(List<double> list)
        {
            double average = list.Average();
            double sumOfSquaresOfDifferences = list.Select(val => (val - average) * (val - average)).Sum();
            double sd = Math.Sqrt(sumOfSquaresOfDifferences / list.Count);
            return sd;
        }

        private Chart NewChart(string Title, string AxisXTitle, string AxisYTitle, SeriesChartType CharType)
        {
            var chart = new Chart
            {
                Width = 1024,
                Height = 768,
                AntiAliasing = AntiAliasingStyles.All,
                TextAntiAliasingQuality = TextAntiAliasingQuality.High
            };
            chart.Titles.Add(Title);
            chart.Titles[0].Font = new System.Drawing.Font("Arial", 25f);
            chart.ChartAreas.Add("");
            chart.ChartAreas[0].AxisX.Title = AxisXTitle;
            chart.ChartAreas[0].AxisY.Title = AxisYTitle;
            chart.ChartAreas[0].AxisX.TitleFont = new System.Drawing.Font("Arial", 20f);
            chart.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Arial", 20f);
            chart.ChartAreas[0].AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 20f);
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = -90;
            chart.ChartAreas[0].AxisY.LabelStyle.Font = new System.Drawing.Font("Arial", 20f);
            chart.ChartAreas[0].Area3DStyle.Enable3D = true;
            chart.ChartAreas[0].BackColor = System.Drawing.Color.White;
            chart.Series.Add("");
            chart.Series[0].ChartType = CharType;
            chart.Series[0].Font = new System.Drawing.Font("Arial", 20f);
            return chart;
        }

        private Chart MeanMarkOfClassesChart()
        {
            var query = (from o in Result.AsEnumerable()
                        group o by new
                        {
                            Class = o[0]
                        }
            into g
                        select new
                        {
                            Class = g.Key.Class,
                            Avg = g.Average(x => double.Parse(x[4]))
                        } ).OrderBy(c => c.Class);
            Chart chart = NewChart("The average mark of each class", "Class", "Average Mark", SeriesChartType.Column);

            foreach (var q in query)
            {
                chart.Series[0].Points.AddXY(q.Class, q.Avg);
            }

            return chart;
        }

        private Chart PassPercentageChart()
        {
            int pass = 0, fail = 0;
            for(int i = 0; i < Result.Count; i++)
            {
                double percentage = double.Parse(Result[i][4]);
                if (percentage >= mcData.CorrectPercentageToPass)
                    pass++;
                else
                    fail++;
            }
            Chart chart = NewChart("The pass percentage", "", "", SeriesChartType.Pie);
            chart.Series[0].IsValueShownAsLabel = true;
            chart.Series[0].Points.AddXY("Pass", pass);
            chart.Series[0].Points.AddXY("Fail", fail);

            chart.Series[0]["PieLabelStyle"] = "Outside";
            chart.Series[0].Label = "#VALX: #PERCENT"; //http://blogs.msdn.com/b/alexgor/archive/2008/11/11/microsoft-chart-control-how-to-using-keywords.aspx
            return chart;
        }

        private byte[] ChartToByte(Chart chart)
        {
            using (var chartimage = new MemoryStream())
            {
                chart.SaveImage(chartimage, ChartImageFormat.Png);
                return chartimage.GetBuffer();
            }
        }

        private Dictionary<int, double[]> PercentageForEachQuestion()
        {
            Dictionary<int, double[]> PercentageCorrectForEachQuestion = new Dictionary<int, double[]>();
            int count = 0;
            foreach(string ans in AnswerKeys[0])
            {
                int A = 0;
                int B = 0;
                int C = 0;
                int D = 0;
                for (int i = 0; i < NoOfStudent(); i++)
                {
                    string st_ans = Review[i][count + 2];
                    switch (st_ans)
                    {
                        case "A":
                            A++;
                            break;
                        case "B":
                            B++;
                            break;
                        case "C":
                            C++;
                            break;
                        case "D":
                            D++;
                            break;
                    }
                }
                double Apercentage = Math.Round(A / (double)NoOfStudent(), 4, MidpointRounding.AwayFromZero);
                double Bpercentage = Math.Round(B / (double)NoOfStudent(), 4, MidpointRounding.AwayFromZero);
                double Cpercentage = Math.Round(C / (double)NoOfStudent(), 4, MidpointRounding.AwayFromZero);
                double Dpercentage = Math.Round(D / (double)NoOfStudent(), 4, MidpointRounding.AwayFromZero);
                double[] percentage = new double[] { Apercentage, Bpercentage, Cpercentage, Dpercentage };
                PercentageCorrectForEachQuestion.Add(count + 1, percentage);//Question No. , Correct Percentage
                count++;
            }
            return PercentageCorrectForEachQuestion;
        }

        private PdfPTable QuestionAnalysis()
        {
            PdfPTable table = new PdfPTable(7);
            table.WidthPercentage = 100;
            PdfPCell title = new PdfPCell(new iTextSharp.text.Phrase("Question Analysis"));
            PdfPCell cell_A = new PdfPCell(new iTextSharp.text.Phrase("A"));
            PdfPCell cell_B = new PdfPCell(new iTextSharp.text.Phrase("B"));
            PdfPCell cell_C = new PdfPCell(new iTextSharp.text.Phrase("C"));
            PdfPCell cell_D = new PdfPCell(new iTextSharp.text.Phrase("D"));
            PdfPCell cell_correctAns = new PdfPCell(new iTextSharp.text.Phrase("Correct Answer"));
            PdfPCell cell_correctPercentage = new PdfPCell(new iTextSharp.text.Phrase("Correct Percentage"));
            table.AddCell(title);
            table.AddCell(cell_A);
            table.AddCell(cell_B);
            table.AddCell(cell_C);
            table.AddCell(cell_D);
            table.AddCell(cell_correctAns);
            table.AddCell(cell_correctPercentage);

            for (int i = 0; i < NoOfQuestion(); i++)
            {
                PdfPCell cell_QuestionNo = new PdfPCell(new iTextSharp.text.Phrase("Q" + (i + 1).ToString()));
                table.AddCell(cell_QuestionNo);
                Dictionary<int, double[]> Percentage = PercentageForEachQuestion();
                table.AddCell((Percentage[i + 1][0] * 100).ToString("N2") + "%"); //A
                table.AddCell((Percentage[i + 1][1] * 100).ToString("N2") + "%"); //B
                table.AddCell((Percentage[i + 1][2] * 100).ToString("N2") + "%"); //C
                table.AddCell((Percentage[i + 1][3] * 100).ToString("N2") + "%"); //D
                table.AddCell(AnswerKeys[0][i]); //CORRECT ANS

                double correctPercentage = 0;
                switch (AnswerKeys[0][i])
                {
                    case "A":
                        correctPercentage = Percentage[i + 1][0];
                        break;
                    case "B":
                        correctPercentage = Percentage[i + 1][1];
                        break;
                    case "C":
                        correctPercentage = Percentage[i + 1][2] ;
                        break;
                    case "D":
                        correctPercentage = Percentage[i + 1][3];
                        break;
                }
                table.AddCell((correctPercentage * 100).ToString("N2") + "%"); //A
            }
            return table;
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            updateResult();
            mcData.AnswerKeys = AnswerKeys;
            mcData.StudentAnswer = Review;

            if (projLocation == "")
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "XML File|*.xml";
                saveFileDialog.Title = "Save xml File";
                saveFileDialog.ShowDialog();

                if (saveFileDialog.FileName != "")
                    Function.WriteToXmlFile(saveFileDialog.FileName, mcData);
            }
            else
            {
                Function.WriteToXmlFile(projLocation, mcData);
            }
        }

        private void Button_Open_Click(object sender, RoutedEventArgs e)
        {
            Function.mcData data;
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            openFileDialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            openFileDialog.Filter = "XML Files|*.xml";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    data = Function.ReadFromXmlFile<Function.mcData>(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    messageBox("Error while reading from the xml file", "Error!");
                    return;
                }
                Viewer viewer = new Viewer(data, openFileDialog.FileName);
                viewer.Show();
                this.Close();
            }
        }
        private void Button_Generate_Report_Click(object sender, RoutedEventArgs e)
        {
            updateResult();
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = "PDF file|*.pdf";
            sfd.Title = "Save an pdf File";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileStream fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write, FileShare.None);
                iTextSharp.text.Document doc = new iTextSharp.text.Document();
                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();
                doc = WriteResult(doc);
                doc.Close();
            }
        }
        private async void messageBox(string Message, string Title)
        {
            await this.ShowMessageAsync(Title, Message);
        }

        private async void label_projectName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string projName = await this.ShowInputAsync("Project Name", "Please provide a new project name");
            if (projName != null)
            {
                mcData.Subject = projName;
                label_projectName.Content = projName;
            }
        }

        private void button_SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!Function.isDouble(textBox_MarksForEachQuestion.Text) || !Function.isDouble(textBox_CorrectPercentageToPass.Text))
            {
                messageBox("Please ensure the marks for each question and correct percentage to pass are entered correctly", "Error");
                textBox_MarksForEachQuestion.Text = mcData.MarksForEachQuestion.ToString();
                textBox_CorrectPercentageToPass.Text = (mcData.CorrectPercentageToPass * 100).ToString();
            }
            else
            {
                mcData.MarksForEachQuestion = double.Parse(textBox_MarksForEachQuestion.Text);
                mcData.CorrectPercentageToPass = double.Parse(textBox_CorrectPercentageToPass.Text) / 100.0;
                updateResult();
            }
        }
    }
}
