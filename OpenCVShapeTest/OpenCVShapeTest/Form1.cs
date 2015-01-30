using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

namespace OpenCVShapeTest
{
    public partial class Form1 : Form
    {
        VideoCapture capture;
        int frameCount;
        List<List<Point2d>> TrackPoints;
        String fileName;
        Point2f[] goodPoints;

        const int BOXSIZE = 10;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*

            VideoCapture capture = new VideoCapture(0);

            int sleepTime = (int)Math.Round(1000 / capture.Fps);

            using (Window window = new Window("capture"))
            {
                
                // Frame image buffer
                Mat image = new Mat();

                // When the movie playback reaches end, Mat.data becomes NULL.
                while (true)
                {
                    capture.Read(image); // same as cvQueryFrame
                    if (image.Empty())
                        break;

                    window.ShowImage(image);
                    Cv2.WaitKey(sleepTime);
                }
                 
            }
             * */
        }


        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "F:\\Documents\\Projects\\Final Year Project\\NN\\videos without chagnges";
            openFileDialog1.Filter = "Media Files|*.mpg;*.avi;*.wma;*.mov;*.wav;*.mp2;*.mp3|All Files|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDialog1.FileName;
                readFromFile();
               
            }
            MessageBox.Show("Error: Could not read file from disk. Original error: " + frameCount);
        }

        private void readFromFile()
        {
            try
            {
                capture = new VideoCapture(fileName);

                timer1_Tick(null, null);
                trackBar1.Maximum = capture.FrameCount;
                frameCount = 0;
                trackBar1.Value = 0;
                label3.Text = "Frame #" + frameCount;

                Console.WriteLine(capture.FrameCount);
                MessageBox.Show("Frame Count: " + capture.FrameCount);
            }
            catch (Exception ex)
            {
               MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }
            readFile();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Mat image = new Mat();

            capture.Read(image); // same as cvQueryFrame
            frameCount++;

            if (frameCount >= capture.FrameCount)
            {
                timer1.Stop();
                trackBar1.Value = capture.FrameCount;
                return;
            }


            if (checkBox1.Checked)
            {
                goodPoints = Cv2.GoodFeaturesToTrack(image.CvtColor(ColorConversion.RgbaToGray), 25, 0.01, 100, image.CvtColor(ColorConversion.RgbaToGray), 3, false, 0.04);

                Console.WriteLine(goodPoints.Length);
                for (int i = 0; i < goodPoints.Length; i++)
                {
                    image.Circle(goodPoints[i], 5, new Scalar(0, 255, 255), 5);
                    //MessageBox.Show(goodPoints[i].ToString());
                }
            }
            //  Cv2.ImShow("temp", image);
            label3.Text = "Frame #" + frameCount;
            Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);

            System.Drawing.Size newSize = new System.Drawing.Size(pictureBox1.Width, image.Height * pictureBox1.Width / image.Width);

            Bitmap newImage = new Bitmap((Image)bitmap, newSize);
            image.Dispose();
            bitmap.Dispose();

            pictureBox1.Width = newImage.Width;
            pictureBox1.Height = newImage.Height;
            pictureBox1.Image = newImage;

            pictureBox1.Refresh();
            pictureBox1_MouseMove(null, null);
            pictureBox2.Refresh();

            trackBar1.Value = frameCount;


        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Start();

            button3.Enabled = false;
            button5.Enabled = false;
            button1.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();

            button3.Enabled = true;
            button5.Enabled = true;
            button1.Enabled = true;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (frameCount > 0)
            {
                capture.Set(CaptureProperty.PosFrames, frameCount - 2);
                frameCount -= 2;
                timer1_Tick(null, null);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            timer1_Tick(null, null);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Drawing.Point point = pictureBox1.PointToClient(Cursor.Position);

            //System.Windows.Forms.MessageBox.Show(""+comboBox1.SelectedIndex);
            if (!comboBox1.SelectedIndex.Equals(-1))
            {
                TrackPoints[comboBox1.SelectedIndex][frameCount] = new Point2d((point.X * 10000) / pictureBox1.Image.Width, (point.Y * 10000) / pictureBox1.Image.Height);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Select a valid point to track");
            }
            pictureBox1.Refresh();
        }


        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                Point2d point = new Point2d();
                try
                {
                    point = TrackPoints[comboBox1.SelectedIndex][frameCount];

                }
                catch (Exception)
                {
                    point = new Point2d(-1, -1);
                }

                //System.Windows.Forms.MessageBox.Show(point.ToString());

                if (point.X == -1 && point.Y == -1)
                {
                    //System.Windows.Forms.MessageBox.Show(" ");
                    System.Drawing.Point holdPoint = pictureBox1.PointToClient(Cursor.Position);
                    point = new Point2d(holdPoint.X, holdPoint.Y);
                }
                else
                {
                    point.X = (point.X * pictureBox1.Image.Width) / 10000;
                    point.Y = (point.Y * pictureBox1.Image.Height) / 10000;
                }

                int x = (int)point.X - BOXSIZE;
                if (x < 0)
                {
                    x = 0;
                }

                int y = (int)point.Y - BOXSIZE;
                if (y < 0)
                {
                    y = 0;
                }

                e.Graphics.DrawRectangle(
                new Pen(Color.Red, 2f),
                (int)point.X - BOXSIZE, (int)point.Y - BOXSIZE, 2 * BOXSIZE, 2 * BOXSIZE);


                Rectangle cropRect = new Rectangle(x, y, BOXSIZE * 2, BOXSIZE * 2);
                Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(pictureBox1.Image, new Rectangle(0, 0, target.Width, target.Height),
                                     cropRect,
                                     GraphicsUnit.Pixel);
                }

                System.Drawing.Size newSize = new System.Drawing.Size(pictureBox2.Width, pictureBox2.Height);

                Bitmap newImage = new Bitmap((Image)target, newSize);
                pictureBox2.Image = newImage;

            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            pictureBox1.Refresh();
            pictureBox2.Refresh();
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                e.Graphics.DrawRectangle(
                new Pen(Color.Red, 2f),
                pictureBox2.Width / 2 - 2, pictureBox2.Height / 2 - 2, 4, 4);
            }
        }
        private void readFile()
        {
            string csvFileName = System.IO.Path.ChangeExtension(fileName, "csv");
            Console.WriteLine(csvFileName);
            comboBox1.Items.Clear();

            try
            {
                string[] lines = System.IO.File.ReadAllLines(csvFileName);
                Console.WriteLine(lines.Length);
                TrackPoints = new List<List<Point2d>>();
                foreach (string l in lines)
                {
                    Console.WriteLine(l);
                    string[] points = l.Split(',');
                    comboBox1.Items.Add(points[0]);
                    List<Point2d> list = new List<Point2d>();
                    for (int i = 0; i < points.Length - 2; i += 2)
                    {

                        list.Add(new Point2d(Int32.Parse(points[i + 1]), Int32.Parse(points[i + 2])));
                    }
                    TrackPoints.Add(list);
                }

            }
            catch (Exception e)
            {
                System.IO.File.Create(csvFileName);
            }
        }
        private void saveTOFIle()
        {
            string csvFileName = System.IO.Path.ChangeExtension(fileName, "csv");
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(csvFileName, false))
            {
                int count = 0;
                foreach (List<Point2d> l in TrackPoints)
                {
                    file.Write(comboBox1.Items[count++] + ",");
                    foreach (Point2d point in l)
                    {
                        file.Write(point.X + "," + point.Y + ",");
                    }
                    file.WriteLine();
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            saveTOFIle();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.BackColor = Color.White;
            if (textBox1.Text == "" || comboBox1.Items.Contains(textBox1.Text))
            {
                textBox1.BackColor = Color.Red;
                return;
            }

            comboBox1.Items.Add(textBox1.Text);
            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
            comboBox1_SelectedIndexChanged(null, null);

            List<Point2d> list = new List<Point2d>();
            for (int i = 0; i < capture.FrameCount; i++)
            {
                list.Add(new Point2d(-1, -1));
            }
            if (TrackPoints == null)
            {
                TrackPoints = new List<List<Point2d>>();
            }
            TrackPoints.Add(list);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine(comboBox1.SelectedIndex);
        }

        private void pictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.A)
            {
                Console.WriteLine("asdf");
            }
        }

        private void trackBar1_Scroll_1(object sender, EventArgs e)
        {
            capture.Set(CaptureProperty.PosFrames, trackBar1.Value - 1);
            frameCount = trackBar1.Value - 1;
            timer1_Tick(null, null);

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            capture.Set(CaptureProperty.PosFrames, frameCount - 1);
            frameCount = frameCount - 1;
            timer1_Tick(null, null);

        }

        private void Histogram_Click(object sender, EventArgs e)
        {
            CvMat a;
            ////Cv.CalcArrHist(pictureBox1.Image,a);
        }
    }
}
