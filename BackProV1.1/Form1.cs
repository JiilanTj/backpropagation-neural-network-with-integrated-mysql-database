using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using MySql.Data.MySqlClient;

namespace BackProV1._1
{
    public partial class Form1 : Form
    {
     
        float miu;
        double error;
        DataTable table = new DataTable();
        int hidden_node=5, in_node = 2, out_node =2, jumlah_pattern=20; //inisialisasi jumlah node dan data input
        double[,] input, output, out_error, v, w, y_output, hid_err; //input, output, perhitungan error,dan bobot
    
        double[] hidden, output_error, hidden_error, error_rata;

        int iterasi;
        Random nilai1 = new Random();
        Random nilai2 = new Random();
        MySqlConnection connection;


        public Form1()
        {
            InitializeComponent();
        }

        public IEnumerable<string> ReadLines(Func<Stream> streamProvider,
                                     Encoding encoding)
        {
            using (var stream = streamProvider())
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

       private void button1_Click(object sender, EventArgs e)
{
    string connectionString = "server=localhost;database=datalog;user=root;";
    string tableName = "tb_sensor";
    using (MySqlConnection connection = new MySqlConnection(connectionString))
    {
        connection.Open();

        // Query SQL untuk mengambil data dari tabel
        string query = $"SELECT no, daya_total, input2, output1, output2 FROM {tableName}";
        MySqlCommand command = new MySqlCommand(query, connection);
        MySqlDataReader reader = command.ExecuteReader();

        List<string> source = new List<string>();
        while (reader.Read())
        {
            string column1 = reader.GetString(0);
            string column2 = reader.GetString(1);
            string column3 = reader.GetString(2);
            string column4 = reader.GetString(3);
            string column5 = reader.GetString(4);

           

                    string row = $"{column1}/{column2}/{column3}/{column4}/{column5}";
            source.Add(row);
        }

                if (source.Count > 0)
                {
                    double[] data = source[0].Split('/').Take(36).Select(Double.Parse).ToArray();
                    textBox4.Text = data[1].ToString(); // Mengisi textBox4 dengan nilai daya_total dari database
                    textBox5.Text = data[1].ToString(); // Mengisi textBox4 dengan nilai daya_total dari database
                }
                else
                {
                    textBox4.Text = ""; // Jika tidak ada data dari database, kosongkan textBox4
                    textBox5.Text = ""; // Jika tidak ada data dari database, kosongkan textBox4
                }


                string[] lines = source.ToArray();
        dataGridView1.ColumnCount = in_node + out_node + 1;
        dataGridView1.Columns[0].Name = "No";
        dataGridView1.Columns[0].Width = 30;
        for (int i = 1; i <= in_node; i++)
        {
            dataGridView1.Columns[i].Name = "input " + i.ToString();
            dataGridView1.Columns[i].Width = 50;
        }
        for (int i = 1; i <= out_node; i++)
        {
            dataGridView1.Columns[i + in_node].Name = "output " + i.ToString();
            dataGridView1.Columns[i + in_node].Width = 50;
        }

        int countData = 0;
        jumlah_pattern = Convert.ToInt32(lines[lines.Length - 1].Split('/')[0]) + 1;
        input = new double[in_node + 1, jumlah_pattern];
        output = new double[out_node + 1, jumlah_pattern];

        foreach (string line in lines)
        {
            string[] datax = line.Split('/');
            double in1 = Convert.ToDouble(datax[1]);
            double in2 = Convert.ToDouble(datax[2]);
            int target1 = Convert.ToInt16(datax[3]);
            int target2 = Convert.ToInt16(datax[4]);

            input[0, countData] = in1;
            input[1, countData] = in2;
            output[0, countData] = target1;
            output[1, countData] = target2;
            countData++;

            string[] row = new string[5];
            row[0] = datax[0];
            row[1] = datax[1];
            row[2] = datax[2];
            row[3] = datax[3];
            row[4] = datax[4];
            dataGridView1.Rows.Add(row);
        }
    }
}


        private void Form1_Load(object sender, EventArgs e)
        {
   
           
           
        }

        void ReadValue()
        {
            hidden_node = Convert.ToInt16(textBox1.Text) ;
            miu = Convert.ToSingle(textBox2.Text);
            error = Convert.ToDouble(textBox3.Text);
        }

        

        

        private void timer1_Tick(object sender, EventArgs e)
        {
            int jumlah_hidden = Convert.ToInt32(textBox1.Text);
            double miu_nol = double.Parse(textBox2.Text.Replace('.', ','));
            double error_limit = double.Parse(textBox3.Text.Replace('.', ','));
            double tmp_hidden1 = new double(), tmp_hidden2 = new double(), tmp_e_hidden = new double();

            for (int p = 0; p < jumlah_pattern; p++)
            {
                //INPUT - HIDDEN
                for (int i = 0; i < jumlah_hidden; i++)
                {
                    tmp_hidden1 = 0;
                    for (int j = 0; j < in_node; j++)
                    {
                        tmp_hidden1 = tmp_hidden1 + input[j, p] * w[j, i];
                    }
                    hidden[i] = (1 / (1 + Math.Exp(-1.0 * (tmp_hidden1 + 1))));
                }
                //HIDDEN - OUTPUT
                for (int i = 0; i < out_node; i++)
                {
                    tmp_hidden2 = 0;
                    for (int j = 0; j < jumlah_hidden; j++)
                    {
                        tmp_hidden2 = tmp_hidden2 + hidden[j] * v[j, i];
                    }
                    y_output[i, p] = (1 / (1 + Math.Exp(-1.0 * (tmp_hidden2 + 1))));
                }
                //hitung error output
                for (int i = 0; i < out_node; i++)
                {
                    output_error[i] = (output[i, p] - y_output[i, p]) * (y_output[i, p] * (1 - y_output[i, p]));
                    out_error[i, p] = output_error[i];
               
                }
                for (int i = 0; i < jumlah_hidden; i++)
                {
                    //compute error signal for hidden units
                    tmp_e_hidden = 0;
                    for (int j = 0; j < out_node; j++)
                    {
                        tmp_e_hidden = tmp_e_hidden + output_error[j] * v[i, j];
                    }
                    hidden_error[i] = (hidden[i] * (1 - hidden[i])) * tmp_e_hidden;
                    hid_err[i, p] = hidden_error[i];
                }
                for (int i = 0; i < jumlah_hidden; i++)
                {
                    //setting weight dari output ke hidden
                    for (int j = 0; j < out_node; j++)
                    {
                        v[i, j] = v[i, j] + miu_nol * output_error[j] * hidden[i];
                    }
                }
                for (int i = 0; i < in_node; i++)
                {
                    for (int j = 0; j < jumlah_hidden; j++)
                    {
                        w[i, j] = w[i, j] + miu_nol * hidden_error[j] * input[i, p];
                    }
                }
                error = error + Math.Abs(out_error[in_node - 1, p]);
            
            }
            error = error / jumlah_pattern;
            iterasi = iterasi + 1;
            textBox10.Text = error.ToString();
            textBox11.Text = iterasi.ToString();
            Console.WriteLine("Jumlah Iterasi: {0}\tLearning Rate: {1}\tError Limit: {2}\tError: {3}", iterasi, miu_nol, error_limit, error);
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Add(String.Format("Err[{0}] - {1}", listBox3.Items.Count + 1, error));
            chart1.Series["Error"].Points.AddXY(iterasi, error);
            for (int i = 0; i < in_node; i++)
            {
                for (int j = 0; j < jumlah_hidden; j++)
                {
                    listBox1.Items.Add(String.Format("W[{0},{1}]={2}", i, j, w[i, j]));
                }
            }
            for (int i = 0; i < jumlah_hidden; i++)
            {
                for (int j = 0; j < out_node; j++)
                {
                    listBox2.Items.Add(String.Format("V[{0},{1}]={2}", i, j, v[i, j]));
                }
            }
            if (error < error_limit)
            {
                timer1.Enabled = false;
                button3.Enabled = true;
            }
            
        }
       

        private async void button2_Click(object sender, EventArgs e)
        {

           double[,] y_inp2 = new double[40, 40];
           double[,] y_out = new double[40, 40];
           double[,] y_out2 = new double[40, 40];
           double[] HiddenNode = new double[40];
           double threshold = 0.5;
           y_inp2[0, 0] = Convert.ToDouble(textBox4.Text);
           y_inp2[0, 1] = Convert.ToDouble(textBox5.Text);
           int jumlah_hidden = Convert.ToInt32(textBox1.Text);
           double temp_hidden1, temp_hidden2;
           double jml_input = 2;
           double jml_output = 2;


            for (int i = 0; i < jumlah_hidden; i++)
           {
               temp_hidden1 = 0;
               for (int j = 0; j < jml_input; j++)
               {
                   temp_hidden1 = temp_hidden1 + y_inp2[0, j] * w[j, i];
               }
               HiddenNode[i] = (1 / (1 + Math.Exp(-1.0 * (temp_hidden1 + 1))));
           }
           for (int i = 0; i < jml_output; i++)
           {
               temp_hidden2 = 0;
               for (int j = 0; j < jumlah_hidden; j++)
               {
                   temp_hidden2 = temp_hidden2 + HiddenNode[j] * v[j, i];
               }
               y_out[0, i] = (1 / (1 + Math.Exp(-1.0 * (temp_hidden2 + 1))));
               if (y_out[0, i] >= threshold)
               {
                   y_out2[0, i] = 1;
               }
               else
               {
                   y_out2[0, i] = 0;
               }

               textBox6.Text = y_out[0, 0].ToString();
               textBox7.Text = y_out[0, 1].ToString();
               textBox8.Text = y_out2[0, 1].ToString();
                textBox9.Text = y_out2[0, 1].ToString();

                await Task.Delay(2000);

                // Mendapatkan nilai yang akan diperiksa
                double nilai1 = Convert.ToDouble(textBox4.Text);
                double nilai2 = Convert.ToDouble(textBox5.Text);

                // Menandakan apakah nilai ditemukan atau tidak
                bool ditemukan1 = false;
                bool ditemukan2 = false;

                // Memeriksa nilai pada dataGridView1
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    // Mengambil nilai dari kolom "input 1"
                    double nilaiKolom = Convert.ToDouble(row.Cells["input 1"].Value);

                    // Membandingkan nilai dengan nilai pada textBox4
                    if (nilaiKolom == nilai1)
                    {
                        ditemukan1 = true;
                        break;
                    }
                }

                // Memeriksa nilai pada dataGridView1
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    // Mengambil nilai dari kolom "input 1"
                    double nilaiKolom = Convert.ToDouble(row.Cells["input 1"].Value);

                    // Membandingkan nilai dengan nilai pada textBox5
                    if (nilaiKolom == nilai2)
                    {
                        ditemukan2 = true;
                        break;
                    }
                }

                // Menampilkan hasil ke textBox6, textBox7, textBox8, textBox9
                textBox6.Text = ditemukan1.ToString();
                textBox7.Text = ditemukan2.ToString();
                textBox8.Text = ditemukan1.ToString();
                textBox9.Text = ditemukan2.ToString();

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int jumlah_hidden = Convert.ToInt32(textBox1.Text);
            double miu_nol = double.Parse(textBox2.Text.Replace('.',','));
            double error_limit = double.Parse(textBox3.Text.Replace('.', ','));
            Console.WriteLine("Jumlah Hidden: {0}\tLearning Rate: {1}\tError Limit: {2}", jumlah_hidden, miu_nol, error_limit);
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            hidden = new double[jumlah_hidden];
            iterasi = 0;
            error = 0;
            error_rata = new double[] { };
            hidden_error = new double[jumlah_hidden];
            hid_err = new double[jumlah_hidden, jumlah_pattern];
            output_error= new double[out_node];
            out_error = new double[out_node,jumlah_pattern];
            y_output = new double[out_node, jumlah_pattern];
            var rand = new Random();
            //random 
            w = new double[in_node, jumlah_hidden];
            v = new double[jumlah_hidden, out_node];
            for (int i = 0; i < in_node; i++)
            {
                for (int j = 0; j < jumlah_hidden; j++)
                {
                    w[i, j] = rand.NextDouble();
                    listBox1.Items.Add(String.Format("W[{0},{1}]={2}", i, j, w[i, j]));
                }
            }
            for (int i = 0; i < jumlah_hidden; i++)
            {
                for (int j = 0; j < out_node; j++)
                {
                    v[i, j] = rand.NextDouble();
                    listBox2.Items.Add(String.Format("V[{0},{1}]={2}", i, j, v[i, j]));
                }
            }
            timer1.Enabled = true;
            button3.Enabled = false;
            

        }



    }
}
