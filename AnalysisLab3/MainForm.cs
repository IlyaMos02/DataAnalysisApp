using AnalysisLab3.Computing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AnalysisLab3
{
    public partial class MainForm : Form
    {
        Compute compute;
        double alfa = 0.05;

        public MainForm()
        {
            InitializeComponent();
            compute = new Compute();
            ResolveTask1();
            ResolveTask2();
            ResolveTask22();
            ResolveTask3();
            ResolveTask4();
        }       

        private void btnFileDepend_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\Users\GameMax\source\repos\AnalysisLab3\AnalysisLab3\Data\data_lab3\dep";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {               
                var fileStream = openFileDialog.OpenFile();

                Dictionary<string, List<double>> data = GetDepDataFromFile(fileStream);
                                
                DataIntoGridView(dataGridDependFirst, data);
                CharacteristicIntoGridView(dataGridDepCharact, data);
                SetDepLabels(data);
            }
        }
       
        private void btnFileIndepend_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\Users\GameMax\source\repos\AnalysisLab3\AnalysisLab3\Data\data_lab3\indep";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var fileStream = openFileDialog.OpenFile();

                Dictionary<string, List<double>> data = GetIndepDataFromFile(fileStream);

                DataIntoGridView(dataGridIndependFirst, data);
                CharacteristicIntoGridView(dataGridIndepCharact, data);
                SetIndepLabels(data);
            }
        }      

        private Dictionary<string, List<double>> GetDepDataFromFile(Stream fileStream)
        {
            string[] fileContent;
            Dictionary<string, List<double>> data = new Dictionary<string, List<double>>();

            using (StreamReader reader = new StreamReader(fileStream))
            {
                fileContent = reader.ReadToEnd().Replace("\n", "").Replace(".", ",").Split('\r');
            }

            data.Add("x", new List<double>());
            data.Add("y", new List<double>());
            data.Add("z", new List<double>());
            foreach (var numb in fileContent)
            {
                if (!string.IsNullOrEmpty(numb))
                {
                    data["x"].Add(Convert.ToDouble(numb.Split(' ')[0]));
                    data["y"].Add(Convert.ToDouble(numb.Split(' ')[1]));
                    data["z"].Add(Convert.ToDouble(numb.Split(' ')[0]) - Convert.ToDouble(numb.Split(' ')[1]));
                }
            }

            return data;
        }
        private Dictionary<string, List<double>> GetIndepDataFromFile(Stream fileStream)
        {
            string[] fileContent;
            Dictionary<string, List<double>> data = new Dictionary<string, List<double>>();

            using (StreamReader reader = new StreamReader(fileStream))
            {
                fileContent = reader.ReadToEnd().Replace("\n", "").Replace(".", ",").Split('\r');
            }

            data.Add("x", new List<double>());
            data.Add("y", new List<double>());            
            foreach (var numb in fileContent)
            {
                if (!string.IsNullOrEmpty(numb))
                {
                    if (Convert.ToDouble(numb.Split(' ')[1]) == 0)
                        data["x"].Add(Convert.ToDouble(numb.Split(' ')[0]));
                    else
                        data["y"].Add(Convert.ToDouble(numb.Split(' ')[0]));                    
                }
            }

            return data;
        }

        private void DataIntoGridView(DataGridView dataGridFirst, Dictionary<string, List<double>> data)
        {
            int i = 0;
            dataGridFirst.Columns.Clear();
            dataGridFirst.Rows.Clear();
            foreach(var key in data.Keys)
            {
                DataGridViewColumn column = new DataGridViewColumn();
                DataGridViewCell cell = new DataGridViewTextBoxCell();
                column.DataPropertyName = key;
                column.HeaderText = key;
                column.CellTemplate = cell;
                column.Name = key;
                dataGridFirst.Columns.Add(column);               
            }
            string[] keys = data.Keys.ToArray();
            i = 0;
            foreach(var item in data.First().Value)
            {
                dataGridFirst.Rows.Add(new DataGridViewRow());

                if (data.ContainsKey("z"))
                    dataGridFirst.Rows[i].SetValues(data[keys[0]][i], data[keys[1]][i], data["z"][i]);
                else
                    dataGridFirst.Rows[i].SetValues(data[keys[0]][i], data[keys[1]][i]);
                i++;
            }                       
        }        

        private void CharacteristicIntoGridView(DataGridView dataGridFirst, Dictionary<string, List<double>> data)
        {
            dataGridFirst.DataSource = (
                from kvp in data
                select new
                {
                    Selection = kvp.Key,
                    Mean = Math.Round(compute.getMean(kvp.Value), 4),
                    MeanInterval = compute.getMeanInterval(kvp.Value),
                    Meadian = Math.Round(compute.getMedian(kvp.Value), 4),
                    MeadianInterval = compute.getMedianInterval(kvp.Value),
                    Variance = Math.Round(compute.getUnshiftedVariance(kvp.Value), 4),
                    StandartDeviation = Math.Round(compute.getStandartDeviation(kvp.Value), 4),
                    Skewness = Math.Round(double.IsNaN(compute.getSecondCoefficientOfSkewness(kvp.Value)) ? 0 : compute.getSecondCoefficientOfSkewness(kvp.Value), 4),
                    Kurtosis = Math.Round(double.IsNaN(compute.getSecondCoefficientOfKurtosis(kvp.Value)) ? 0 : compute.getSecondCoefficientOfKurtosis(kvp.Value), 4),
                    Normal = compute.IsNormalDist(kvp.Value).ToString()
                }
                ).ToList();
        }        

        private void SetDepLabels(Dictionary<string, List<double>> data)
        {
            double t = Math.Abs(compute.DepCriteriaMean(data));
            double StudentT = compute.getStudent(1.0 - alfa / 2.0, data["x"].Count + data["y"].Count - 2.0);
            double f, v1, v2;
            double rank = compute.DepRank(data);
            if (compute.getStandartDeviation(data["x"]) >= compute.getStandartDeviation(data["y"]))
            {
                f = compute.getUnshiftedVariance(data["x"]) / compute.getUnshiftedVariance(data["y"]);
                v1 = data["x"].Count - 1;
                v2 = data["y"].Count - 1;
            }
            else
            {
                f = compute.getUnshiftedVariance(data["y"]) / compute.getUnshiftedVariance(data["x"]);
                v1 = data["y"].Count - 1;
                v2 = data["x"].Count - 1;
            }
            double FisherF = compute.getFisher(1.0 - alfa, v1, v2);

            lblDepModuleT.Text = "| t | = " + (double.IsNaN(t) ? 0 : t);
            lblDepQuantileT.Text = "Student t = " + StudentT;

            if (t <= StudentT || double.IsNaN(t) )
                lblDepMeanResult.Text = "Середні значення вибірок рівні";
            else
                lblDepMeanResult.Text = "Середні значення вибірок суттєво різняться";

            lblDepF.Text = "f = " + (double.IsNaN(f) ? 0 : f);
            lblDepFisher.Text = "Fisher f = " + FisherF;

            if (f <= FisherF || double.IsNaN(f) )
                lblDepVarianceResult.Text = "Дисперсії вибірок збігаються";
            else
                lblDepVarianceResult.Text = "Дисперсіх вибірок відмінні";

            lblDepRank.Text = "rank = " + (double.IsNaN(rank) ? 0 : rank);
            lblDepNorm.Text = "Quantile norm = " + compute.getNormal(1.0 - alfa / 2.0).ToString();
            if (rank <= compute.getNormal(1.0 - alfa / 2.0) || double.IsNaN(rank))
                lblDepRankResult.Text = "Зсуву у функціях розподілу немає";
            else
                lblDepRankResult.Text = "Функції розподілу зсунені одна відносно іншої";
        }
        private void SetIndepLabels(Dictionary<string, List<double>> data)
        {                         
            double f, v1, v2;
            double rank = compute.IndepRank(data);
            if (compute.getStandartDeviation(data["x"]) >= compute.getStandartDeviation(data["y"]))
            {
                f = compute.getUnshiftedVariance(data["x"]) / compute.getUnshiftedVariance(data["y"]);
                v1 = data["x"].Count - 1;
                v2 = data["y"].Count - 1;
            }
            else
            {
                f = compute.getUnshiftedVariance(data["y"]) / compute.getUnshiftedVariance(data["x"]);
                v1 = data["y"].Count - 1;
                v2 = data["x"].Count - 1;
            }
            double FisherF = compute.getFisher(1.0 - alfa, v1, v2);

            double t;
            double v;
            if (f <= FisherF)
            {
                t = Math.Abs(compute.IndepCriteriaMeanNegative(data));
                v = compute.getIndepStudentV(data);
            }
            else
            {
                t = Math.Abs(compute.IndepCriteriaMean(data));
                v = data["x"].Count + data["y"].Count - 2.0;
            }
            double StudentT = compute.getStudent(1.0 - alfa / 2.0, v);

            lblIndepModuleT.Text = "| t | = " + (double.IsNaN(t)? 0 : t);
            lblIndepQuantileT.Text = "Student t = " + StudentT;

            if (t <= StudentT || double.IsNaN(t))
                lblIndepMeanResult.Text = "Середні значення вибірок рівні";
            else
                lblIndepMeanResult.Text = "Середні значення вибірок суттєво різняться";

            lblIndepF.Text = "f = " + (double.IsNaN(f) ? 0 : f);
            lblIndepFisherF.Text = "Fisher f = " + FisherF;

            if (f <= FisherF || double.IsNaN(f))
                lblIndepVarianceResult.Text = "Дисперсії вибірок збігаються";
            else
                lblIndepVarianceResult.Text = "Дисперсіх вибірок відмінні";

            lblIndepRank.Text = "rank = " + (double.IsNaN(rank) ? 0 : rank);
            lblIndepNorm.Text = "Quantile norm = " + compute.getNormal(1.0 - alfa / 2.0).ToString();
            if (rank <= compute.getNormal(1.0 - alfa / 2.0) || double.IsNaN(rank))
                lblIndepRankResult.Text = "Зсуву у функціях розподілу немає";
            else
                lblIndepRankResult.Text = "Функції розподілу зсунені одна відносно іншої";
        }

        //Task 1 Independent

        private void ResolveTask1()
        {
            string path = @"C:\Users\GameMax\source\repos\AnalysisLab3\AnalysisLab3\Data\data_lab3_tasks\readingspeed.txt";
            string[] fileContent;
            Dictionary<string, List<double>> data = new Dictionary<string, List<double>>();

            using (StreamReader sr = new StreamReader(path))
            {
                fileContent = sr.ReadToEnd().Replace("\n", "").Split('\r');
            }

            data.Add("DRA", new List<double>());
            data.Add("SC", new List<double>());
            foreach(var numb in fileContent)
            {
                if (!string.IsNullOrEmpty(numb))
                {
                    if (numb.Split(' ')[1] == "DRA")
                        data["DRA"].Add(Convert.ToDouble(numb.Split(' ')[0]));
                    else
                        data["SC"].Add(Convert.ToDouble(numb.Split(' ')[0]));
                }
            }

            IndepTaskDataIntoGridView(dataGrid1Task1, dataGrid2Task1, data);
            CharacteristicIntoGridView(dataGridCharactTask1, data);
            SetIndepTaskLabels(lblTask1ModuleT, lblTask1QuantileT, lblTask1MeanResult, lblTask1F, lblTask1Fisher, lblTask1VarianceResult, lblTask1Rank, lblTask1Norm, lblTask1RankResult, data);
        }
        

        //Task 2 Independent

        private void ResolveTask2()
        {
            string path = @"C:\Users\GameMax\source\repos\AnalysisLab3\AnalysisLab3\Data\data_lab3_tasks\water.txt";
            string[] fileContent;
            Dictionary<string, List<double>> data = new Dictionary<string, List<double>>();

            using (StreamReader sr = new StreamReader(path))
            {
                fileContent = sr.ReadToEnd().Replace("\n", "").Split('\r');
            }

            data.Add("south", new List<double>());
            data.Add("north", new List<double>());
            foreach (var numb in fileContent)
            {
                if (!string.IsNullOrEmpty(numb))
                {
                    if(numb.Split('\t')[0]=="South")
                        data["south"].Add(Convert.ToDouble(numb.Split('\t')[2]));
                    else
                        data["north"].Add(Convert.ToDouble(numb.Split('\t')[2]));
                }
            }

            IndepTaskDataIntoGridView(dataGrid1Task2, dataGrid2Task2, data);
            CharacteristicIntoGridView(dataGridCharactTask2, data);
            SetIndepTaskLabels(lblTask2ModuleT, lblTask2QuantileT, lblTask2MeanResult, lblTask2F, lblTask2Fisher, lblTask2VarianceResult, lblTask2Rank, lblTask2Norm, lblTask2RankResult, data);
        }

        private void ResolveTask22()
        {
            string path = @"C:\Users\GameMax\source\repos\AnalysisLab3\AnalysisLab3\Data\data_lab3_tasks\water.txt";
            string[] fileContent;
            Dictionary<string, List<double>> data = new Dictionary<string, List<double>>();

            using (StreamReader sr = new StreamReader(path))
            {
                fileContent = sr.ReadToEnd().Replace("\n", "").Split('\r');
            }

            data.Add("south", new List<double>());
            data.Add("north", new List<double>());
            foreach (var numb in fileContent)
            {
                if (!string.IsNullOrEmpty(numb))
                {
                    if (numb.Split('\t')[0] == "South")
                        data["south"].Add(Convert.ToDouble(numb.Split('\t')[3]));
                    else
                        data["north"].Add(Convert.ToDouble(numb.Split('\t')[3]));
                }
            }

            IndepTaskDataIntoGridView(dataGrid1Task22, dataGrid2Task22, data);
            CharacteristicIntoGridView(dataGridCharactTask22, data);
            SetIndepTaskLabels(lblTask22ModuleT, lblTask22QuantileT, lblTask22MeanResult, lblTask22F, lblTask22Fisher, lblTask22VarianceResult, lblTask22Rank, lblTask22Norm, lblTask22RankResult, data);
        }

        //Task 3 Dependent

        private void ResolveTask3()
        {
            string path = @"C:\Users\GameMax\source\repos\AnalysisLab3\AnalysisLab3\Data\data_lab3_tasks\ADHD.txt";
            string[] fileContent;
            Dictionary<string, List<double>> data = new Dictionary<string, List<double>>();

            using (StreamReader sr = new StreamReader(path))
            {
                fileContent = sr.ReadToEnd().Replace("\n", "").Split('\r');
            }

            data.Add("plac", new List<double>());
            data.Add("methy", new List<double>());
            data.Add("z", new List<double>());
            foreach (var numb in fileContent)
            {
                if (!string.IsNullOrEmpty(numb))
                {
                    data["plac"].Add(Convert.ToDouble(numb.Split(' ')[0]));
                    data["methy"].Add(Convert.ToDouble(numb.Split(' ')[1]));
                    data["z"].Add(Convert.ToDouble(numb.Split(' ')[0]) - Convert.ToDouble(numb.Split(' ')[1]));
                }
            }

            DataIntoGridView(dataGrid1Task3, data);
            CharacteristicIntoGridView(dataGridCharactTask3, data);
            SetDepTaskLabels(lblTask3ModuleT, lblTask3QuantileT, lblTask3MeanResult, lblTask3F, lblTask3Fisher, lblTask3VarianceResult, lblTask3Rank, lblTask3Norm, lblTask3RankResult, data);
        }

        //Task 4 Depend

        private void ResolveTask4()
        {
            string path = @"C:\Users\GameMax\source\repos\AnalysisLab3\AnalysisLab3\Data\data_lab3_tasks\wines.txt";
            string[] fileContent;
            Dictionary<string, List<double>> data = new Dictionary<string, List<double>>();

            using (StreamReader sr = new StreamReader(path))
            {
                fileContent = sr.ReadToEnd().Replace("\n", "").Replace(".", ",").Split('\r');
            }

            data.Add("shard", new List<double>());
            data.Add("cabern", new List<double>());
            data.Add("z", new List<double>());
            foreach (var numb in fileContent)
            {
                if (!string.IsNullOrEmpty(numb))
                {
                    data["shard"].Add(Convert.ToDouble(numb.Split('\t')[1]));
                    data["cabern"].Add(Convert.ToDouble(numb.Split('\t')[2]));
                    data["z"].Add(Convert.ToDouble(numb.Split('\t')[1]) - Convert.ToDouble(numb.Split('\t')[2]));
                }
            }

            DataIntoGridView(dataGrid1Task4, data);
            CharacteristicIntoGridView(dataGridCharactTask4, data);
            SetDepTaskLabels(lblTask4ModuleT, lblTask4QuantileT, lblTask4MeanResult, lblTask4F, lblTask4Fisher, lblTask4VarianceResult, lblTask4Rank, lblTask4Norm, lblTask4RankResult, data);
        }

        //Addition

        private void SetIndepTaskLabels(Label lblModuleT, Label lblQuantileT, Label lblMeanResult, Label lblF, Label lblFisher, Label lblVarianceResult, Label lblRank, Label lblNorm, Label lblRankResult, Dictionary<string, List<double>> data)
        {
            string[] keys = data.Keys.ToArray();
           
            double f, v1, v2;
            double rank = compute.IndepRank(data);
            if (compute.getStandartDeviation(data[keys[0]]) >= compute.getStandartDeviation(data[keys[1]]))
            {
                f = compute.getUnshiftedVariance(data[keys[0]]) / compute.getUnshiftedVariance(data[keys[1]]);
                v1 = data[keys[0]].Count - 1;
                v2 = data[keys[1]].Count - 1;
            }
            else
            {
                f = compute.getUnshiftedVariance(data[keys[1]]) / compute.getUnshiftedVariance(data[keys[0]]);
                v1 = data[keys[1]].Count - 1;
                v2 = data[keys[0]].Count - 1;
            }
            double FisherF = compute.getFisher(1.0 - alfa, v1, v2);

            double t;
            double v;
            if (f <= FisherF)
            {
                t = Math.Abs(compute.IndepCriteriaMeanNegative(data));
                v = compute.getIndepStudentV(data);
            }
            else
            {
                t = Math.Abs(compute.IndepCriteriaMean(data));
                v = data[keys[0]].Count + data[keys[1]].Count - 2.0;
            }
            double StudentT = compute.getStudent(1.0 - alfa / 2.0, v);

            lblModuleT.Text = "| t | = " + (double.IsNaN(t) ? 0 : t);
            lblQuantileT.Text = "Student t = " + StudentT;

            if (t <= StudentT || double.IsNaN(t))
                lblMeanResult.Text = "Середні значення вибірок рівні";
            else
                lblMeanResult.Text = "Середні значення вибірок суттєво різняться";

            lblF.Text = "f = " + (double.IsNaN(f) ? 0 : f);
            lblFisher.Text = "Fisher f = " + FisherF;

            if (f <= FisherF || double.IsNaN(f))
                lblVarianceResult.Text = "Дисперсії вибірок збігаються";
            else
                lblVarianceResult.Text = "Дисперсіх вибірок відмінні";

            lblRank.Text = "rank = " + (double.IsNaN(rank) ? 0 : rank);
            lblNorm.Text = "Quantile norm = " + compute.getNormal(1.0 - alfa / 2.0).ToString();
            if (rank <= compute.getNormal(1.0 - alfa / 2.0) || double.IsNaN(rank))
                lblRankResult.Text = "Зсуву у функціях розподілу немає";
            else
                lblRankResult.Text = "Функції розподілу зсунені одна відносно іншої";
        }

        private void SetDepTaskLabels(Label lblModuleT, Label lblQuantileT, Label lblMeanResult, Label lblF, Label lblFisher, Label lblVarianceResult, Label lblRank, Label lblNorm, Label lblRankResult, Dictionary<string, List<double>> data)
        {
            string[] keys = data.Keys.ToArray();

            double t = Math.Abs(compute.DepCriteriaMean(data));
            double StudentT = compute.getStudent(1.0 - alfa / 2.0, data[keys[0]].Count + data[keys[1]].Count - 2.0);
            double f, v1, v2;
            double rank = compute.DepRank(data);
            if (compute.getStandartDeviation(data[keys[0]]) >= compute.getStandartDeviation(data[keys[1]]))
            {
                f = compute.getUnshiftedVariance(data[keys[0]]) / compute.getUnshiftedVariance(data[keys[1]]);
                v1 = data[keys[0]].Count - 1;
                v2 = data[keys[1]].Count - 1;
            }
            else
            {
                f = compute.getUnshiftedVariance(data[keys[1]]) / compute.getUnshiftedVariance(data[keys[0]]);
                v1 = data[keys[1]].Count - 1;
                v2 = data[keys[0]].Count - 1;
            }
            double FisherF = compute.getFisher(1.0 - alfa, v1, v2);

            lblModuleT.Text = "| t | = " + (double.IsNaN(t) ? 0 : t);
            lblQuantileT.Text = "Student t = " + StudentT;

            if (t <= StudentT || double.IsNaN(t))
                lblMeanResult.Text = "Середні значення вибірок рівні";
            else
                lblMeanResult.Text = "Середні значення вибірок суттєво різняться";

            lblF.Text = "f = " + (double.IsNaN(f) ? 0 : f);
            lblFisher.Text = "Fisher f = " + FisherF;

            if (f <= FisherF || double.IsNaN(f))
                lblVarianceResult.Text = "Дисперсії вибірок збігаються";
            else
                lblVarianceResult.Text = "Дисперсіх вибірок відмінні";

            lblRank.Text = "rank = " + (double.IsNaN(rank) ? 0 : rank);
            lblNorm.Text = "Quantile norm = " + compute.getNormal(1.0 - alfa / 2.0).ToString();
            if (rank <= compute.getNormal(1.0 - alfa / 2.0) || double.IsNaN(rank))
                lblRankResult.Text = "Зсуву у функціях розподілу немає";
            else
                lblRankResult.Text = "Функції розподілу зсунені одна відносно іншої";
        }

        private void IndepTaskDataIntoGridView(DataGridView dataGridFirst, DataGridView dataGridSecond, Dictionary<string, List<double>> data)
        {
            int i = 0;
            dataGridFirst.Columns.Clear();
            dataGridFirst.Rows.Clear();
            dataGridSecond.Columns.Clear();
            dataGridSecond.Rows.Clear();

            string[] keys = data.Keys.ToArray();
            DataGridViewColumn column1 = new DataGridViewColumn();
            DataGridViewCell cell1 = new DataGridViewTextBoxCell();
            column1.DataPropertyName = keys[0];
            column1.HeaderText = keys[0];
            column1.CellTemplate = cell1;
            column1.Name = keys[0];
            dataGridFirst.Columns.Add(column1);

            DataGridViewColumn column2 = new DataGridViewColumn();
            DataGridViewCell cell2 = new DataGridViewTextBoxCell();
            column2.DataPropertyName = keys[1];
            column2.HeaderText = keys[1];
            column2.CellTemplate = cell2;
            column2.Name = keys[1];
            dataGridSecond.Columns.Add(column2);

            foreach (var item in data[keys[0]])
            {
                dataGridFirst.Rows.Add(item);
            }

            foreach (var item in data[keys[1]])
            {
                dataGridSecond.Rows.Add(item);
            }
        }
    }
}
