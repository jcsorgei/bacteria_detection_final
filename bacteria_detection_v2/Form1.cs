using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//Jozef Csorgei - 128253, Frantisek Bukor - 127989, Gabriel Cseh - ??????

namespace bacteria_detection_v2
{
    public partial class Form1 : Form
    {
        public int imgCount = 1;
        public string fileName;
        public double ThreshMin = 180;
        public double ThreshMax = 255;
        public int ThreshMode=0;
        public Image <Gray, byte> _01Markers;
        public Image<Gray, byte> _02Markers;
        public Image<Gray, byte> _03Markers;
        public Image<Gray, byte> markers;
        public Image<Bgr, byte> circleimg;
        public Image<Bgr, byte> img;
        public Image<Bgr, byte> original;
        public Image<Gray, byte> umat;
        public Image<Gray, int> labels;
        public CircleF[] circles;
        public bool count;


        public Form1()
        {
            InitializeComponent();
        }        
        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = original.ToBitmap();
            img = new Bitmap(pictureBox1.Image)
                .ToImage<Bgr, byte>();
            if (ThreshMode==0) {
                markers = new Bitmap(pictureBox1.Image)
                .ToImage<Gray, byte>()
                .ThresholdBinary(new Gray(ThreshMin), new Gray(255));
            }
            else if (ThreshMode==1) {
                markers = new Bitmap(pictureBox1.Image)
                .ToImage<Gray, byte>()
                .ThresholdBinaryInv(new Gray(ThreshMin), new Gray(255));
            }
            else  {
                markers = new Bitmap(pictureBox1.Image).ToImage<Gray, byte>();
                CvInvoke.Threshold(markers, markers, 0, 255,ThresholdType.Otsu);
            }
            Mat kernel1 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Ellipse, new Size(21, 21), new Point(10, 10));// csinálunk egy elipszist aminek a mérete 21 és a közepe 10
            CvInvoke.MorphologyEx(markers, markers, MorphOp.Erode, kernel1, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Reflect, new MCvScalar(1.0)); // egy erode transzformációt csinálunk a grayscale képen, hogy eltüntessük a kisebb imperfekciókat a képen
            labels = new Image<Gray, Int32>(markers.Size);
            markers.Data[10, 10,0] = 255; //berajzolunk egy újabb elemet a képre, hogy a watershed ezt az elemet nyújtsa ki a háttérnek
            CvInvoke.ConnectedComponents(markers, labels);
            CvInvoke.Watershed(img, labels);
            Image<Gray, Byte> umat = labels.Convert<Gray, Byte>();
            pictureBox1.Image = img.ToBitmap();
            pictureBox2.Image = umat.ToBitmap();
            count = false;
        }

        private void watershedImage(Bitmap img)
        {
            var srcImg = img
                .ToImage<Bgr, byte>();
            if (ThreshMode == 0)
            {
                markers = img
                .ToImage<Gray, byte>()
                .ThresholdBinary(new Gray(ThreshMin), new Gray(255));
            }
            else if (ThreshMode == 1)
            {
                markers = img
                .ToImage<Gray, byte>()
                .ThresholdBinaryInv(new Gray(ThreshMin), new Gray(255));
            }
            else
            {
                markers = img.ToImage<Gray, byte>();
                CvInvoke.Threshold(markers, markers, 0, 255, ThresholdType.Otsu);
            }

            Directory.CreateDirectory($".\\{imgCount}");

            _01Markers = markers;
            _01Markers.Save($".\\{imgCount}\\01_markers.png");
            Mat kernel1 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Ellipse, new Size(21, 21), new Point(10, 10));

            CvInvoke.MorphologyEx(markers, markers, MorphOp.Erode, kernel1, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Reflect, new MCvScalar(1.0)); 
            _02Markers = markers;
            _02Markers.Save($".\\{imgCount}\\02_markers.png");           
            var labels = new Image<Gray, Int32>(markers.Size);
            
            markers.Data[10, 10, 0] = 255;
            CvInvoke.ConnectedComponents(markers, labels);
            CvInvoke.Watershed(srcImg, labels);

            Image<Gray, Byte> umat = labels.Convert<Gray, Byte>();

            _03Markers = umat;
            _03Markers.Save($".\\{imgCount}\\03_markers.png");
            pictureBox2.Image = umat.ToBitmap();
            imgCount++;
        }

        private void watershedToolStripMenuItem_Click(object sender, EventArgs e) //Felső címsávból a kép megnyitása
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(openFileDialog1.FileName);
                original = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();
                fileName = openFileDialog1.FileName;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            ThreshMin = trackBar1.Value;
            button1_Click(sender, e);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _01Markers.Save("_01_mask_default.png");
            _02Markers.Save("_02_markers_morphologyex.png");
            _03Markers.Save("_03_segmented.png");
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                ThreshMode = 0;
                radioButton2.Checked = false;
                radioButton3.Checked = false;
                button1_Click(sender, e);
                trackBar1.Enabled = true;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                ThreshMode = 1;
                radioButton1.Checked = false;
                radioButton3.Checked = false;
                button1_Click(sender, e);
                trackBar1.Enabled = true;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                ThreshMode = 3;
                radioButton1.Checked = false;
                radioButton2.Checked = false;
                button1_Click(sender, e);
                trackBar1.Enabled = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            imgCount = 1;
            DirectoryInfo d = new DirectoryInfo(@"images");
            FileInfo[] images = d.GetFiles("*.png");

            foreach (var image in images)
            {
                var img = new Bitmap($".\\images\\{image.Name}");
                watershedImage(img);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (count == false)
            {

                circleimg = img;
                Image<Gray, Byte> umat = labels.Convert<Gray, Byte>();
                Gray cannyThreshold = new Gray(50);
                Gray cannyThresholdLinking = new Gray(30);
                Gray circleAccumulatorThreshold = new Gray(50);
                Image<Gray, Byte> cannyEdges = umat.Canny(cannyThreshold.Intensity, cannyThresholdLinking.Intensity);
                circles = circleimg.Convert<Gray, Byte>().HoughCircles(
                cannyThreshold,
                circleAccumulatorThreshold,
                0.3, //Resolution of the accumulator used to detect centers of the circles
                cannyEdges.Height / 10, //min distance 
                0, //min radius
                300 //max radius
                )[0]; //Get the circles from the first channel
                listBox1.Items.Add($"{Path.GetFileName(fileName)} - Circle count: {circles.Count()}");
                foreach (CircleF circle in circles)
                {
                    circleimg.Draw(circle, new Bgr(Color.Red), 4);
                }
                pictureBox1.Image = circleimg.ToBitmap();
                count = true;
            }
            else { }
        }
    }
}
