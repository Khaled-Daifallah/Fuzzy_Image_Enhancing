using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Image_Enhance
{
    class ImageOPER
    {
        public const byte COLORED = 0;
        public const byte GRAY = 1;
        private Thread DrawingThread;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.StatusStrip strip;
        private Bar_Sta link;
        private UnsafeBitmap source;
        private EVector[][] E_Image,enhanced_IMAGE;
        private double[][] qX; // parts*x
        private double[][] qY; // parts*y
        private int M, N; // nb of partitions on X,Y
        private double psay;
        private byte imageType;
        private double[][][][] W; //  i*j*x*y
        private double[][] card;
        private EVector[][] means;
        private double[][] variences;
        private double[][] lambdas;
        private EVector[][] taws;
        private EVector[][][] E_Colored_Image, enhanced_C_IMAGE;
        private EVector[][] luminosity;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pic"></param>
        /// <param name="XpatM"></param>
        /// <param name="YpatN"></param>
        /// <param name="psay"></param>
        /// <param name="isColored"></param>

        public ImageOPER(UnsafeBitmap pic, int XpatM, int YpatN, double psay, bool? isColored, System.Windows.Forms.ToolStripStatusLabel statusLabel, System.Windows.Forms.ToolStripProgressBar progressBar,System.Windows.Forms.StatusStrip strip)
        {
            this.statusLabel = statusLabel;
            this.progressBar = progressBar;
            this.strip = strip;
            progressBar.Value = 0;
            //DrawingThread = new Thread(new ParameterizedThreadStart(doDrawing));
            link = new Bar_Sta("Initializing", 0);
            //DrawingThread.Start(link);
            doDrawing(link);
            source = pic;
            source.LockBitmap();
            if (isColored == null)
            {
                PixelData data;
                Random ra = new Random();
                data = source.GetPixel(ra.Next(source.Width), ra.Next(source.Height));
                if (data.red == data.green && data.red == data.blue)
                    imageType = GRAY;
                else
                    imageType = COLORED;
            }
            else if (isColored.Value)
                imageType = COLORED;
            else imageType = GRAY;
            M = XpatM; N = YpatN;
            this.psay = psay;
            ////////////
            W = new double[M][][][];
            card = new double[M][];
            for (int i = 0; i < M; i++)
            {
                W[i] = new double[N][][];
                card[i] = new double[N];
                for (int j = 0; j < N; j++)
                {
                    W[i][j] = new double[source.Width][];
                    for (int x = 0; x < source.Width; x++)
                        W[i][j][x] = new double[source.Height];
                }
            }
            ////////////
            link.status = "Filling Qx and Qy"; link.val = 3;
            //DrawingThread = new Thread(new ParameterizedThreadStart(doDrawing));
            //DrawingThread.Start(link);
            doDrawing(link);
            fillQ();
            link.status = "Filling W [i,j,x,y] "; link.val = 7;
            //DrawingThread = new Thread(new ParameterizedThreadStart(doDrawing));
            //DrawingThread.Start(link);
            doDrawing(link);
            fillW();
            if (imageType==GRAY)
            {
                imageType = GRAY;
                //////
                link.status = "Moving to E space"; link.val = 12;
                doDrawing(link);
                //////
                transformTo_Espace();
                //////
                link.status = "Calculating the MEANS"; link.val = 8;
                doDrawing(link);
                //////
                calculate_Means();
                //////
                link.status = "Calculating the VARIANCES"; link.val = 13;
                doDrawing(link);
                //////
                calculate_Variances();
                //////
                link.status = "Calculating the LAMBDAs and TAWs"; link.val = 17;
                doDrawing(link);
                //////
                calculate_Lambdas_Taws();
                //////
                link.status = "Computing the Enhanced Image"; link.val = 10;
                doDrawing(link);
                //////
            }
            else
            {
                imageType = COLORED;
                link.status = "Moving to E space"; link.val = 12;
                doDrawing(link);
                transformTo_E_Colored_Space();
                link.status = "Calculating the MEANS"; link.val = 8;
                doDrawing(link);
                calculate_Colored_Means();
                link.status = "Calculating the VARIANCES"; link.val = 13;
                doDrawing(link);
                calculate_Colored_Variances();
                link.status = "Calculating the LAMBDAs and TAWs"; link.val = 17;
                doDrawing(link);
                calculate_Lambdas_Taws();
                link.status = "Computing the Enhanced Image"; link.val = 10;
                doDrawing(link);
/*                UnsafeBitmap temp;
                temp = new UnsafeBitmap(source.Width, source.Height);
                temp.LockBitmap();
                transformTo_EspaceR();
                calculate_Means();
                calculate_Variances();
                calculate_Lambdas_Taws();
                temp = getEnhancedColoredImageR(temp,0);
                ////
                transformTo_EspaceG();
                calculate_Means();
                calculate_Variances();
                calculate_Lambdas_Taws();
                temp = getEnhancedColoredImageR(temp, 1);
                ////
                transformTo_EspaceB();
                calculate_Means();
                calculate_Variances();
                calculate_Lambdas_Taws();
                temp = getEnhancedColoredImageR(temp, 2);*/
            }
        }

        public byte ImageType
        {
            get { return imageType; }
        }

/*        private UnsafeBitmap getEnhancedColoredImageR(UnsafeBitmap goal,int color)
        {
            EVector acc;
            EVector temp;
            enhanced_IMAGE = new EVector[E_Image.Length][];
            for (int x = 0; x < E_Image.Length; x++)
            {
                enhanced_IMAGE[x] = new EVector[E_Image[x].Length];
                for (int y = 0; y < E_Image[x].Length; y++)
                {
                    acc = new EVector(0.0d);
                    for (int i = 0; i < M; i++)
                    {
                        for (int j = 0; j < N; j++)
                        {
                            temp = (lambdas[i][j] * (E_Image[x][y] + taws[i][j]));
                            temp = W[i][j][x][y] * temp;
                            acc = acc + temp;
                        }
                    }
                    enhanced_IMAGE[x][y] = acc;
                }
            }
            //return convertoToImage(enhanced_IMAGE);
            return convertoToColoredImage(enhanced_IMAGE, goal, color);
        }*/

        public UnsafeBitmap getEnhancedImage()
        {
            EVector acc;
            EVector temp;
            enhanced_IMAGE = new EVector[E_Image.Length][];
            for (int x = 0; x < E_Image.Length; x++)
            {
                enhanced_IMAGE[x] = new EVector[E_Image[x].Length];
                for (int y = 0; y < E_Image[x].Length; y++)
                {
                    acc = new EVector(0.0d);
                    for (int i = 0; i < M; i++)
                    {
                        for (int j = 0; j < N; j++)
                        {
                            temp = (lambdas[i][j] * (E_Image[x][y] + taws[i][j]));
                            temp = W[i][j][x][y] * temp;
                            acc = acc + temp;
                        }
                    }
                    enhanced_IMAGE[x][y] = acc;
                }
            }
            link.status = "DRAWING the Enhanced Image"; link.val = 20;
            doDrawing(link);
            return convertoToImage(enhanced_IMAGE);
        }

        public UnsafeBitmap getEnhancedColoredImage()
        {
            EVector acc1,acc2,acc3;
            EVector temp;
            enhanced_C_IMAGE = new EVector[E_Colored_Image.Length][][];
            for (int x = 0; x < E_Colored_Image.Length; x++)
            {
                enhanced_C_IMAGE[x] = new EVector[E_Colored_Image[x].Length][];
                for (int y = 0; y < E_Colored_Image[x].Length; y++)
                {
                    enhanced_C_IMAGE[x][y] = new EVector[3]; 
                    acc1 = new EVector(0.0d);
                    acc2 = new EVector(0.0d);
                    acc3 = new EVector(0.0d);
                    for (int i = 0; i < M; i++)
                    {
                        for (int j = 0; j < N; j++)
                        {
                            temp = (lambdas[i][j] * (E_Colored_Image[x][y][0] + taws[i][j]));
                            temp = W[i][j][x][y] * temp;
                            acc1 = acc1 + temp;
                            ////////
                            temp = (lambdas[i][j] * (E_Colored_Image[x][y][1] + taws[i][j]));
                            temp = W[i][j][x][y] * temp;
                            acc2 = acc2 + temp;
                            ////////
                            temp = (lambdas[i][j] * (E_Colored_Image[x][y][2] + taws[i][j]));
                            temp = W[i][j][x][y] * temp;
                            acc3 = acc3 + temp;
                        }
                    }
                    enhanced_C_IMAGE[x][y][0] = acc1;
                    enhanced_C_IMAGE[x][y][1] = acc2;
                    enhanced_C_IMAGE[x][y][2] = acc3;
                }
            }
            link.status = "DRAWING the Enhanced Image"; link.val = 20;
            doDrawing(link);
            return convertoToColoredImage(enhanced_C_IMAGE);
        }


/*        private UnsafeBitmap convertoToColoredImage(EVector[][] EImage,UnsafeBitmap target,int color)
        {
            UnsafeBitmap temp = target;// new UnsafeBitmap(EImage.Length, EImage[0].Length);
            //temp.LockBitmap();
            int hold;
            for (int x = 0; x < temp.Width; x++)
            {
                for (int y = 0; y < temp.Height; y++)
                {
                    hold = EImage[x][y].IntValue;
                    if(color==1)
                        temp.SetPixel(x, y, new PixelData((byte)hold, (byte)hold, (byte)hold));
                    else if(color==2)
                        temp.SetPixel(x, y, new PixelData(temp.GetPixel(x,y).red, (byte)hold, (byte)hold));
                    else
                        temp.SetPixel(x, y, new PixelData(temp.GetPixel(x, y).red, temp.GetPixel(x, y).green, (byte)hold));
                }
            }
            return temp;
        }*/

        private UnsafeBitmap convertoToImage(EVector[][] EImage)
        {
            UnsafeBitmap temp = new UnsafeBitmap(EImage.Length, EImage[0].Length);
            temp.LockBitmap();
            int hold;
            for (int x = 0; x < temp.Width; x++)
            {
                for (int y = 0; y < temp.Height; y++)
                {
                    hold = EImage[x][y].IntValue;
                    temp.SetPixel(x, y, new PixelData((byte)hold, (byte)hold, (byte)hold));
                }
            }
            link.status = "DONE !!"; link.val = 10;
            doDrawing(link);
            return temp;
        }

        private UnsafeBitmap convertoToColoredImage(EVector[][][] EImage)
        {
            UnsafeBitmap temp = new UnsafeBitmap(EImage.Length, EImage[0].Length);
            temp.LockBitmap();
            for (int x = 0; x < temp.Width; x++)
            {
                for (int y = 0; y < temp.Height; y++)
                {
                    temp.SetPixel(x, y, new PixelData((byte)EImage[x][y][0].IntValue, (byte)EImage[x][y][1].IntValue, (byte)EImage[x][y][2].IntValue));
                }
            }
            link.status = "DONE !!"; link.val = 10;
            doDrawing(link);
            return temp;
        }

        private void calculate_Lambdas_Taws()
        {
            lambdas = new double[M][];
            taws = new EVector[M][];
            EVector temp = new EVector(0.0d);
            for (int i = 0; i < M; i++)
            {
                lambdas[i] = new double[N];
                taws[i] = new EVector[N];
                for (int j = 0; j < N; j++)
                {
                    lambdas[i][j] = Math.Sqrt(1.0d / 3.0d) / variences[i][j];
                    taws[i][j] = temp - means[i][j];
                }
            }
        }

        private void transformTo_Espace()
        {
            E_Image = new EVector[source.Width][];
            //source.LockBitmap();
            for (int x = 0; x < source.Width; x++)
            {
                E_Image[x] = new EVector[source.Height];
                for (int y = 0; y < source.Height; y++)
                {
                    E_Image[x][y] = new EVector((int)source.GetPixel(x, y).red);
                }
            }
            source.UnlockBitmap();
        }

        private void transformTo_E_Colored_Space()
        {
            E_Colored_Image = new EVector[source.Width][][];
            luminosity = new EVector[source.Width][];
            //source.LockBitmap();
            for (int x = 0; x < source.Width; x++)
            {
                E_Colored_Image[x] = new EVector[source.Height][];
                luminosity[x] = new EVector[source.Height];
                for (int y = 0; y < source.Height; y++)
                {
                    E_Colored_Image[x][y] = new EVector[3];
                    E_Colored_Image[x][y][0] = new EVector((int)source.GetPixel(x, y).red);
                    E_Colored_Image[x][y][1] = new EVector((int)source.GetPixel(x, y).green);
                    E_Colored_Image[x][y][2] = new EVector((int)source.GetPixel(x, y).blue);
                    luminosity[x][y] = (1.0d / 3.0d) * (E_Colored_Image[x][y][0] + E_Colored_Image[x][y][1] + E_Colored_Image[x][y][2]);
                }
            }
            source.UnlockBitmap();
        }

        /*        private void transformTo_EspaceG()
                {
                    E_Image = new EVector[source.Width][];
                    source.LockBitmap();
                    for (int x = 0; x < source.Width; x++)
                    {
                        E_Image[x] = new EVector[source.Height];
                        for (int y = 0; y < source.Height; y++)
                        {
                            E_Image[x][y] = new EVector((int)source.GetPixel(x, y).green);
                        }
                    }
                    source.UnlockBitmap();
                }

                private void transformTo_EspaceB()
                {
                    E_Image = new EVector[source.Width][];
                    source.LockBitmap();
                    for (int x = 0; x < source.Width; x++)
                    {
                        E_Image[x] = new EVector[source.Height];
                        for (int y = 0; y < source.Height; y++)
                        {
                            E_Image[x][y] = new EVector((int)source.GetPixel(x, y).blue);
                        }
                    }
                    source.UnlockBitmap();
                }*/

        private void calculate_Means()
        {
            EVector temp;
            means = new EVector[M][];
            for (int i = 0; i < M; i++)
            {
                means[i] = new EVector[N];
                for (int j = 0; j < N; j++)
                {
                    temp = new EVector(0.0d);
                    for (int x = 0; x < E_Image.Length; x++)
                    {
                        for (int y = 0; y < E_Image[x].Length; y++)
                        {
                            temp = temp + ((W[i][j][x][y] / card[i][j]) * E_Image[x][y]);
                        }
                    }
                    means[i][j] = temp;
                }
            }
        }

        private void calculate_Colored_Means()
        {
            EVector temp;
            means = new EVector[M][];
            for (int i = 0; i < M; i++)
            {
                means[i] = new EVector[N];
                for (int j = 0; j < N; j++)
                {
                    temp = new EVector(0.0d);
                    for (int x = 0; x < luminosity.Length; x++)
                    {
                        for (int y = 0; y < luminosity[x].Length; y++)
                        {
                            temp = temp + ((W[i][j][x][y] / card[i][j]) * luminosity[x][y]);
                        }
                    }
                    means[i][j] = temp;
                }
            }
        }

        private void calculate_Variances()
        {
            double acc = 0;
            EVector temp; double temp2;
            variences = new double[M][];
            for (int i = 0; i < M; i++)
            {
                variences[i] = new double[N];
                for (int j = 0; j < N; j++)
                {
                    acc = 0;
                    for (int x = 0; x < E_Image.Length; x++)
                    {
                        for (int y = 0; y < E_Image[x].Length; y++)
                        {
                            temp = E_Image[x][y] - means[i][j];
                            temp2 = temp.RealValue;
                     //           Console.WriteLine(temp2);
                            temp2 = Math.Pow(temp2, 2);
                            temp2 *= W[i][j][x][y];
                            acc += temp2 / Card(i, j);
                        }
                    }
                    variences[i][j] = Math.Sqrt(acc);
                }
            }
        }

        private void calculate_Colored_Variances()
        {
            double acc = 0;
            EVector temp; double temp2;
            variences = new double[M][];
            for (int i = 0; i < M; i++)
            {
                variences[i] = new double[N];
                for (int j = 0; j < N; j++)
                {
                    acc = 0;
                    for (int x = 0; x < luminosity.Length; x++)
                    {
                        for (int y = 0; y < luminosity[x].Length; y++)
                        {
                            temp = luminosity[x][y] - means[i][j];
                            temp2 = temp.RealValue;
                            //           Console.WriteLine(temp2);
                            temp2 = Math.Pow(temp2, 2);
                            temp2 *= W[i][j][x][y];
                            acc += temp2 / Card(i, j);
                        }
                    }
                    variences[i][j] = Math.Sqrt(acc);
                }
            }
        }

        private void fillQ()
        {
            qX = new double[M][];
            qY = new double[N][];
            for (int i = 0; i < M; i++)
            {
                qX[i] = new double[source.Width];
                for (int x = 0; x < source.Width; x++)
                    qX[i][x] = C(i, M) * ((Math.Pow(x, i) * Math.Pow(source.Width - x, M - i)) / (Math.Pow(source.Width, M)));
            }
            for (int j = 0; j < N; j++)
            {
                qY[j] = new double[source.Height];
                for (int y = 0; y < source.Height; y++)
                    qY[j][y] = C(j, N) * ((Math.Pow(y, j) * Math.Pow(source.Height - y, N - j)) / (Math.Pow(source.Height, N)));
            }
        }

        private void fillW()
        {
            double[][] P=new double[M][];
            double total = 0.0d;
            for (int x = 0; x < source.Width; x++)
            {
                for (int y = 0; y < source.Height; y++)
                {
                    total = 0.0d;
                    for (int i = 0; i < M; i++)
                    {
                        P[i] = new double[N];
                        for (int j = 0; j < N; j++)
                        {
                            P[i][j] = qX[i][x] * qY[j][y];
                            //total +=  P[i][j];
                            total += Math.Pow(P[i][j], psay);
                        }
                    }
                    for (int i = 0; i < M; i++)
                    {
                        for (int j = 0; j < N; j++)
                        {
                            //W[i][j][x][y] = (Math.Pow(P[i][j], psay)) / (Math.Pow(total, psay));
                            W[i][j][x][y] = (Math.Pow(P[i][j], psay)) / total;
                            card[i][j] += W[i][j][x][y];
                        }
                    }
                }
            }
        }

        private double Card(int i,int j)
        {
            return card[i][j];
        }

        private static double C(int i, int m)
        {
            return Factorial(m) / (Factorial(i) * Factorial(m - i));
        }

        private static Int64 Factorial(int i)
        {
            Int64 acc = 1;
            for (int j = 2; j <= i; j++)
                acc *= j;
            return acc;
        }

        private void doDrawing(Bar_Sta link)
        {
            statusLabel.Text = link.status;
            progressBar.Increment(link.val);
            strip.Items[2].Text = progressBar.Value + "%";
            //progressBar.ProgressBar.
    /*        if (progressBar.Value + link.val <= 100)
                progressBar.Value += link.val;
            else progressBar.Value = 100;*/
//            progressBar.ProgressBar.Refresh();
  //          progressBar.ProgressBar.Update();
            strip.Update();
            strip.Refresh();
        }

    }
}
