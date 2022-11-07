using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Interface_lab_1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private SubTheme subTheme1;
        private SubTheme subTheme2;

        private struct SubTheme
        {
            public SubTheme(List<Route> routes, bool number_of_subtheme)
            {
                this.routes = routes;
                this.number_of_subtheme = number_of_subtheme;
            }

            public List<Route> routes;
            public bool number_of_subtheme;

        }

        private struct Route
        {
            public Route(List<int> steps, double prob)
            {
                this.steps = steps;
                this.prob_of_route = prob;
            }

            public List<int> steps;
            public double prob_of_route;
        }

        private void ReadDataFromCSV(bool number_of_subtheme)
        {
            List<Route> routes = new List<Route>();
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "csv File|*.csv";
            if (opf.ShowDialog() == DialogResult.OK)
            {
                string[] lines = System.IO.File.ReadAllLines(opf.FileName);
                if(lines.Length == 0)
                {
                    MessageBox.Show("Пустой файл");
                    return;
                }
                try
                {
                    int first_root = 0;
                    foreach (string line in lines)
                    {
                        string[] data = line.Split(',');
                        string prob = data[0].Replace(".", ",");
                        double prob_of_route = Convert.ToDouble(prob);
                        List<int> steps = new List<int>();
                        if (line == lines[0]) first_root = Convert.ToInt32(data[1]);
                        else if (Convert.ToInt32(data[1]) != first_root)
                        {
                            MessageBox.Show("Все маршруты в подтемах должны начинаться с одного и того же номера шага диалога");
                            return;
                        }
                            for (var i = 1; i < data.Length; i++)
                            steps.Add(Convert.ToInt32(data[i]));
                        Route route = new Route(steps, prob_of_route);
                        routes.Add(route);
                    }
                    SubTheme subtheme = new SubTheme(routes, number_of_subtheme);
                    if (!number_of_subtheme)
                        subTheme1 = subtheme;
                    else
                        subTheme2 = subtheme;
                }
                catch(Exception ex)
                {
                    MessageBox.Show("В файле содержатся ошибки");
                }
            }
        }

        private void перваяПодтемаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReadDataFromCSV(false);
            перваяПодтемаToolStripMenuItem.Text += '*';
        }

        private void втораяПодтемаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReadDataFromCSV(true);
            втораяПодтемаToolStripMenuItem.Text += '*';
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(subTheme1.routes == null || subTheme2.routes == null)
            {
                MessageBox.Show("Одна или обе подтемы с маршрутами не введены");
                return;
            }

            try
            {
                int N = Convert.ToInt32(textBox1.Text);
                double P_err = Convert.ToDouble(textBox2.Text);
                double P_subtheme1 = Convert.ToDouble(textBox3.Text);
                double P_subtheme2 = 1 - P_subtheme1;
                var current_subtheme = new SubTheme();
                var current_route = new Route();
                double time1 = 0;
                double time2 = 0;
                var k1 = 0;
                var k2 = 0;
                List<double> times_of_subthemes = new List<double>();
                var rand = new Random();
                for (var i = 0; i < N; i++)
                {
                    double time_of_subtheme = 0;
                    var Generated_P_subtheme = rand.NextDouble();
                    if (Generated_P_subtheme <= P_subtheme1)
                        current_subtheme = subTheme1;
                    else
                        current_subtheme = subTheme2;
                    var Generated_P_route = rand.NextDouble();
                    for (var j = 0; j < current_subtheme.routes.Count; j++)
                    {
                        if (Generated_P_route <= current_subtheme.routes[j].prob_of_route)
                        {
                            current_route = current_subtheme.routes[j];
                            break;
                        }
                        else
                            Generated_P_route -= current_subtheme.routes[j].prob_of_route;
                    }
                    for (var index = 0; index < current_route.steps.Count; index++)
                    {
                        var Generated_time = rand.Next(5, 16) + rand.NextDouble();
                        if (!current_subtheme.number_of_subtheme)
                        {
                            time1 += Generated_time;
                            time_of_subtheme += Generated_time;
                        }
                        else
                        {
                            time2 += Generated_time;
                            time_of_subtheme += Generated_time;
                        }
                        if (rand.NextDouble() <= P_err)
                            index = 0;

                    }
                    times_of_subthemes.Add(time_of_subtheme);
                    if (!current_subtheme.number_of_subtheme)
                        k1++;
                    else
                        k2++;
                }
                var average_time1 = time1 / k1;
                var average_time2 = time2 / k2;
                var average_time = time1 + time2;
                average_time /= k1 + k2;

                textBox6.Text = average_time1.ToString();
                textBox5.Text = average_time2.ToString();
                textBox4.Text = average_time.ToString();

                //построение гистограммы
                chart1.Series[0].Points.Clear();
                var K = Convert.ToInt32(1 + 3.22 * Math.Log10(N));
                double min = double.MaxValue, max = double.MinValue;
                foreach (var item in times_of_subthemes)
                {
                    min = (min > item) ? (item) : (min);
                    max = (max < item) ? (item) : (max);
                }
                double[] intervals = new double[K + 1];
                for (var i = 0; i < K + 1; i++)
                    intervals[i] = min + (max - min) / K * i;
                int[] y = new int[K];
                double[] x = new double[K];
                for (var i = 0; i < intervals.Length - 1; i++)
                {
                    x[i] = (intervals[i + 1] + intervals[i]) / 2;
                    foreach (var j in times_of_subthemes)
                        if (j >= intervals[i] && j < intervals[i + 1])
                            y[i]++;
                }
                for (var i = 0; i < K; i++)
                    chart1.Series[0].Points.AddXY(x[i], y[i]);

            } catch(Exception ex)
            {
                MessageBox.Show("Неправильный ввод параметров");
            }
        }
    }
}
