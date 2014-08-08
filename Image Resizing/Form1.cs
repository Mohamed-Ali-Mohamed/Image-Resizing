using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Image_Resizing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        MyColor[,] ImageMatrix;
        string OpenedFilePath;
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            label8.Text = "";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                int Height = ImageMatrix.GetLength(0);
                int Width = ImageMatrix.GetLength(1);
                textBox1.Text = Height.ToString();
                textBox2.Text = Width.ToString();
            }

        }

        private void Rersize_Click(object sender, EventArgs e)
        {
            label8.Text = "";
            if(OpenedFilePath==null)
                return;
            ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
            if (txtWidth.Text == "" || txtHeight.Text == "")
                return;
            for (int i = 0; i < txtHeight.Text.Length; i++)
                if (txtHeight.Text[i] < '0' || txtHeight.Text[i] > '9') return;
            for (int i = 0; i < txtWidth.Text.Length; i++)
                if (txtWidth.Text[i] < '0' || txtWidth.Text[i] > '9') return;
            int W = int.Parse(txtWidth.Text);
            int H = int.Parse(txtHeight.Text);
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);
            int WPlus=0, HPlus=0, WSub=0, HSub=0;
            if (W >= Width)
                WPlus = W - Width;
            else
                WSub = Width - W;
            if (H >= Height)
                HPlus = H - Height;
            else
                HSub = Height - H;
            int Start = System.Environment.TickCount;
            MyColor[,] resizedImage = ImageOperations.Resize(ImageMatrix, WPlus, HPlus, WSub, HSub);
            int End = System.Environment.TickCount;
            ImageOperations.DisplayImage(resizedImage, pictureBox2);
            double Time = End - Start;
            Time /= 1000;
            label8.Text = (Time).ToString();
            label8.Text += " s";
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }
    }
}
