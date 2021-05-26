using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace MultipleChoiceMarker
{

    public partial class MainWindow : MetroWindow
    {
        List<string[]> AnswerKeys = new List<string[]>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Init()
        {
            this.IsEnabled = true;
            dataGrid_AnsKey.ItemsSource = null;
            dataGrid_AnsKey.Items.Clear();
            dataGrid_AnsKey.Columns.Clear();
            AnswerKeys.Clear();
        }

        private void button_Browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.ShowNewFolderButton = false;
            fbd.Description = "Select the folder that contains the scanned documents";
            fbd.SelectedPath = System.Windows.Forms.Application.StartupPath;
            System.Windows.Forms.DialogResult result = fbd.ShowDialog();
            string path = fbd.SelectedPath;
            textBox_Directory.Text = path;
        }

        private void button_Next_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate()) return;

            subject = textBox_Subject.Text;
            directory = textBox_Directory.Text;
            marksPerQuestion = double.Parse(textBox_Marks.Text);
            NoOfQuestions = comboBox_NumberOfQuestions.SelectedIndex + 1;
            percentageToPass = double.Parse(textBox_CorrectPercentageToPass.Text) / 100.0;

            FirstStep.Visibility = Visibility.Hidden;
            SecondStep.Visibility = Visibility.Visible;


            for (int i = 0; i <= comboBox_NumberOfQuestions.SelectedIndex; i++)
            {
                DataGridComboBoxColumn comboBoxClmn = new DataGridComboBoxColumn();
                comboBoxClmn.Header = "Question " + (i + 1).ToString();
                comboBoxClmn.SelectedValueBinding = new Binding("[" + i + "]");
                comboBoxClmn.ItemsSource = new ObservableCollection<string>() { "A", "B", "C", "D" };
                dataGrid_AnsKey.Columns.Add(comboBoxClmn);
            }

            string[] value;
            value = new string[NoOfQuestions];
            AnswerKeys.Add(value);
            dataGrid_AnsKey.ItemsSource = AnswerKeys;

        }

        private bool Validate()
        {
            if (!Function.isDouble(textBox_Marks.Text) || !Function.isWithinRange(double.Parse(textBox_Marks.Text), 1, 100))
            {
                messageBox("Please enter the marks given to each question properly", "Error");
                return false;
            }
            if (comboBox_NumberOfQuestions.SelectedIndex == -1)
            {
                messageBox("Please select the number of questions", "Error");
                return false;
            }
            if (textBox_Subject.Text.Length == 0)
            {
                messageBox("Please provide a subject for the project", "Error");
                return false;
            }
            if (textBox_CorrectPercentageToPass.Text.Length == 0 || (!Function.isDouble(textBox_CorrectPercentageToPass.Text) || !Function.isWithinRange(double.Parse(textBox_CorrectPercentageToPass.Text), 1, 100)))
            {
                messageBox("Please enter the passing percentage properly", "Error");
                return false;
            }
            if (!Directory.Exists(textBox_Directory.Text))
            {
                messageBox("Please input a correct directory", "Error");
                return false;
            }
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            var files = Function.GetFilesFrom(textBox_Directory.Text, filters);
            int filesCount = files.Count();
            if(filesCount == 0)
            {
                messageBox("No images were found in the directory provided", "Error");
                return false;
            }
            return true;
        }

        private void button_Previous_Click(object sender, RoutedEventArgs e)
        {
            dataGrid_AnsKey.ItemsSource = null;
            dataGrid_AnsKey.Items.Clear();
            dataGrid_AnsKey.Columns.Clear();
            AnswerKeys.Clear();
            
            SecondStep.Visibility = Visibility.Hidden;
            FirstStep.Visibility = Visibility.Visible;
        }

        private void button_Process_Click(object sender, RoutedEventArgs e)
        {
            if (!checkAnswerKey())
            {
                messageBox("Some of the answer keys are missing", "Error");
                return;
            }
            Process();
        }

        private bool checkAnswerKey()
        {
            foreach (string a in AnswerKeys[0])
            {
                if (!Function.isChoice(a))
                    return false;
            }
            return true;
        }
        ProgressDialogController controller;
        BackgroundWorker bw = new BackgroundWorker();
        string subject;
        string directory;
        double marksPerQuestion;
        int NoOfQuestions;
        double percentageToPass;

        private async void Process()
        {
            this.IsEnabled = false;
            controller = await this.ShowProgressAsync("Please wait a while", "Scanning the directory...", false);
            controller.SetIndeterminate();
            
            bw.DoWork += new DoWorkEventHandler(Work);
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkCompleted);
            bw.RunWorkerAsync();
        }

        private void WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            controller.CloseAsync();
            Function.mcData mcData = (Function.mcData) e.Result;
            Viewer viewer = new Viewer(mcData);
            viewer.Show();

            this.Close();
        }
        int progress = 0;
        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    controller.SetProgress(e.ProgressPercentage / 100f);   
                })
            );
        }

        private void Work(object sender, DoWorkEventArgs e)
        {
            var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            var files = Function.GetFilesFrom(directory, filters);
            int filesCount = files.Count();

            string[] st_class = new string[filesCount];
            int[] st_no = new int[filesCount];
            List<string[]> st_ans = new List<string[]>();

            for (int i = 0; i < files.Count(); i++)
            {
                Bitmap timage = (Bitmap)Bitmap.FromFile(files[i]);
                Bitmap[] r = Function.findRectangle(timage);
                Bitmap upperpart, ans_part1, ans_part2, ans_part3;
                upperpart = r[0];
                ans_part1 = r[1];
                ans_part2 = r[2];
                ans_part3 = r[3];


                st_ans.Add(Function.getStudentAns(upperpart, ans_part1, ans_part2, ans_part3, NoOfQuestions));
                double percentage = ((i + 1) / (double)filesCount) * 100;
                bw.ReportProgress((int)percentage);
            }

            Function.mcData data = new Function.mcData();
            data.Subject = subject;
            data.NoOfQuestions = NoOfQuestions;
            data.MarksForEachQuestion = marksPerQuestion;
            data.CorrectPercentageToPass = percentageToPass;
            data.StudentAnswer = st_ans;
            data.AnswerKeys = AnswerKeys;
            e.Result = data;
        }

        private async void messageBox(string Message, string Title)
        {
            await this.ShowMessageAsync(Title, Message);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Function.mcData data;
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            openFileDialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            openFileDialog.Filter = "XML Files|*.xml";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try {
                    data = Function.ReadFromXmlFile<Function.mcData>(openFileDialog.FileName);
                }catch(Exception ex)
                {
                    messageBox("Error while reading from the xml file", "Error!");
                    return;
                }
                Viewer viewer = new Viewer(data, openFileDialog.FileName);
                viewer.Show();
                this.Close();
            }
        }
    }
   
}
