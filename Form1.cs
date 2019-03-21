using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace Image_Enhance
{
    public partial class Form1 : Form
    {
        private Image g;
        private UnsafeBitmap load,result;
        private ImageOPER core;
        public Form1()
        {
            InitializeComponent();
            
/*            bool colored = true;
            UnsafeBitmap load = new UnsafeBitmap(new Bitmap(@"I:\FuzzyLogic_ASSA\color2.bmp"));
            ImageOPER work = new ImageOPER(load, 5, 5, 1,colored);
            UnsafeBitmap res ;
            if (!colored)
                res = work.getEnhancedImage();
            else
                res = work.getEnhancedColoredImage();
            res.UnlockBitmap();
            res.Bitmap.Save(@"I:\FuzzyLogic_ASSA\Cres2.bmp");*/
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                textBox1.Enabled = textBox2.Enabled = textBox3.Enabled = button2.Enabled = true;
                g = new Bitmap(openFileDialog1.FileName);
                pictureBox1.BackgroundImageLayout = ImageLayout.Center;
                tabControl1.SelectedIndex = 0;
                if (tabControl1.TabPages.Count > 1)
                {
                    tabControl1.TabPages.RemoveAt(1);
                }
                if (g.Width > pictureBox1.Width || g.Height > pictureBox1.Height)
                {
                    MessageBox.Show(this, "The image which you have chosen is larger than the panel , and it will be scaled to fit on the panel", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
                }
                pictureBox1.BackgroundImage = g;
                toolStripStatusLabel1.Text = "Please load an Image";
                toolStripStatusLabel2.Text = "0%";
                toolStripProgressBar1.Value=0;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0 && textBox2.Text.Length > 0 && textBox3.Text.Length > 0)
            {
                int xP, yP, psay;
                try
                {
                    xP = int.Parse(textBox1.Text);
                    yP = int.Parse(textBox2.Text);
                    psay = int.Parse(textBox3.Text);
                }
                catch (Exception)
                {
                    showError1();
                    return;
                }
                if (xP <= 0 || yP <= 0)
                {
                    showError1();
                    return;
                }
                ///////
                /// here will be the code
                /// of the core + creating the tab and the picBox in it.
                ///////
                Nullable<bool> colred;
                if (radioButton1.Checked == true)
                    colred = null;
                else if (radioButton2.Checked == true)
                    colred = true;
                else
                    colred = false;
                load = new UnsafeBitmap((Bitmap)g);
                core = new ImageOPER(load, xP, yP, psay, colred,toolStripStatusLabel1,toolStripProgressBar1,statusStrip1);
                if (core.ImageType == ImageOPER.GRAY)
                {
                    result = core.getEnhancedImage();
                }
                else
                {
                    result = core.getEnhancedColoredImage();
                }
                result.UnlockBitmap();
                //////
                TabPage myPage = new TabPage("Image Enhance Result");
                tabControl1.TabPages.Add(myPage);
                PictureBox pic = new PictureBox();
                pic.Location = new System.Drawing.Point(6, 6);
                pic.Name = "pictureBox1";
                pic.Size = new System.Drawing.Size(623, 425);
                pic.TabIndex = 0;
                pic.TabStop = false;
                pic.BackgroundImageLayout = pictureBox1.BackgroundImageLayout;
                pic.BackgroundImage = result.Bitmap;
                myPage.Controls.Add(pic);
                tabControl1.SelectTab(myPage);
            }
            else
            {
                showError1();
                return;
            }
        }

        private void showError1()
        {
            MessageBox.Show(this, "Please Check the values of X_Partitions && Y_Partitions && Psay, X && Y must be possitive integers, psay must be positive real value", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }
}
