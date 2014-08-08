using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace Image_Resizing
{
    public struct MyColor
    {
        public byte red, green, blue;
    }
    public struct Point
    {
        public int value;
        public string position;
    }
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static MyColor[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            MyColor[,] Buffer = new MyColor[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[0];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[2];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(MyColor[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(MyColor[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Calculate energy between the given two pixels
        /// </summary>
        /// <param name="Pixel1">First pixel color</param>
        /// <param name="Pixel2">Second pixel color</param>
        /// <returns>Energy between the 2 pixels</returns>
        private static int CalculatePixelsEnergy(MyColor Pixel1, MyColor Pixel2)
        {
            int Energy = Math.Abs(Pixel1.red - Pixel2.red) + Math.Abs(Pixel1.green - Pixel2.green) + Math.Abs(Pixel1.blue - Pixel2.blue);
            return Energy;
        }

        private static MyColor CalculatePixelsAvg(MyColor Pixel1, MyColor Pixel2)
        {
            MyColor Avg;
            double Red = Pixel1.red + Pixel2.red;
            Red /= 2;
            Avg.red = (byte)Red;
            double Blue = Pixel1.blue + Pixel2.blue;
            Blue /= 2;
            Avg.blue = (byte)Blue;
            double Green = Pixel1.green + Pixel2.green;
            Green /= 2;
            Avg.green = (byte)Green;
            return Avg;
        }
        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(MyColor[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[0] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[2] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }
        private static int[] UsedEnergyIndexH, UsedEnergyIndexC;
        private static void ShiftLeft(MyColor[,] ImageMatrix, int width, int Row, int elementIndex)
        {
            for (int i = elementIndex; i < width - 1; i++)
                ImageMatrix[Row, i] = ImageMatrix[Row, i + 1];
        }
        private static void ShiftRight(MyColor[,] ImageMatrix, int width, int Row, int elementIndex)
        {
            for (int i = width - 1; i > elementIndex; i--)
                ImageMatrix[Row, i] = ImageMatrix[Row, i - 1];
        }
        private static void ShiftUp(MyColor[,] ImageMatrix, int height, int Col, int elementIndex)
        {
            for (int i = elementIndex; i < height - 1; i++)
                ImageMatrix[i, Col] = ImageMatrix[i + 1, Col];
        }
        private static void ShiftDown(MyColor[,] ImageMatrix, int height, int Col, int elementIndex)
        {
            for (int i = height - 1; i > elementIndex; i--)
                ImageMatrix[i, Col] = ImageMatrix[i - 1, Col];

        }
        private static void ShiftLeft2(int[,] Energy, int width, int Row, int elementIndex)
        {
            for (int i = elementIndex; i < width - 1; i++)
                Energy[Row, i] = Energy[Row, i + 1];
        }
        private static void ShiftUp2(int[,] Energy, int height, int Col, int elementIndex)
        {
            for (int i = elementIndex; i < height - 1; i++)
                Energy[i, Col] = Energy[i + 1, Col];
        }
        private static void ShiftLeft3(Point[,] Cost, int width, int Row, int elementIndex)
        {
            for (int i = elementIndex; i < width - 1; i++)
                Cost[Row, i] = Cost[Row, i + 1];
        }
        private static void ShiftUp3(Point[,] Cost, int height, int Col, int elementIndex)
        {
            for (int i = elementIndex; i < height - 1; i++)
                Cost[i, Col] = Cost[i + 1, Col];
        }
        private static int MinRow, MinCol;
        private static int GetMinRow(Point[,] Cost, int W, int H)
        {
            int Row = -1;
            for (int i = 5; i < H; i++)
            {
                if (Cost[i, W - 1].value < MinRow)
                {
                    MinRow = Cost[i, W - 1].value;
                    Row = i;
                }
            }
            return Row;
        }
        private static int GetMinCol(Point[,] Cost, int W, int H)
        {
            int Col = -1;
            for (int i = 5; i < W; i++)
            {
                if (Cost[H - 1, i].value < MinCol)
                {
                    MinCol = Cost[H - 1, i].value;
                    Col = i;
                }
            }
            return Col;
        }
        public static MyColor[,] Resize(MyColor[,] OldImageMatrix, int WPlus, int HPlus, int WSub, int HSub)
        {
            int Height = OldImageMatrix.GetLength(0)+5;
            int Width = OldImageMatrix.GetLength(1)+5;
            MyColor[,] ImageMatrix = new MyColor[Height + HPlus+5, Width + WPlus+5];
            UsedEnergyIndexC = new int[Width + WPlus+5];
            UsedEnergyIndexH = new int[Height + HPlus+5];
            for (int y = 5; y < Height; y++)
            {
                for (int x = 5; x < Width; x++)
                {
                    ImageMatrix[y, x] = OldImageMatrix[y-5, x-5];
                }
            }
            int New_W_Sub = WSub, New_H_Sub = HSub, New_W_Plus = WPlus, New_H_Plus = HPlus, New_Height = Height, New_Width = Width;
            int[] RowIndex = new int[New_Width+WPlus +5], ColIndex = new int[New_Height+HPlus+5];
            int[,] Energy = new int[New_Height+5, New_Width+5];
            Point[,] ColCost = new Point[New_Height+5, New_Width+5], RowCost = new Point[New_Height+5, New_Width+5];
            bool LastWasRow = true; 
            while (New_H_Sub > 0 || New_W_Sub > 0 || New_H_Plus > 0 || New_W_Plus > 0)
            {
                for (int i = 5; i < Width + WPlus; i++)
                    UsedEnergyIndexC[i] = i;
                for (int i = 5; i < Height + HPlus; i++)
                    UsedEnergyIndexH[i] = i;
                if (New_H_Sub > 0 && New_W_Sub > 0)
                {
                    if (New_H_Sub == HSub && New_W_Sub == WSub)//First Time
                    {
                        Energy = calculateEnergy(ImageMatrix, New_Height, New_Width);
                        ColCost = calculateCostVertically(Energy, ImageMatrix, New_Height, New_Width);
                        RowCost = calculateCostHorizontally(Energy, ImageMatrix, New_Height, New_Width);
                    }
                    else
                    {
                        Energy = calculateEnergy2(ImageMatrix, Energy, New_Height, New_Width, RowIndex, ColIndex, LastWasRow);
                        ColCost = calculateCostVertically2(Energy, ImageMatrix, ColCost, New_Height, New_Width, ColIndex[5]);
                        RowCost = calculateCostHorizontally2(Energy, ImageMatrix, RowCost, New_Height, New_Width, RowIndex[5]);
                    }
                    int Row, Col, Old_Width = New_Width, Old_Height = New_Height;
                    MinCol = MinRow = int.MaxValue;
                    Row = GetMinRow(RowCost, Old_Width, Old_Height);
                    Col = GetMinCol(ColCost, Old_Width, Old_Height);
                    if (MinRow <= MinCol)
                    {
                        int ToBeDeleted = Row;
                        int Next = 0;
                        LastWasRow = true;
                        for (int i = Old_Width - 1; i >= 5; i--)
                        {
                            string Pos = RowCost[ToBeDeleted, i].position;
                            if (Pos == "SW") Next = ToBeDeleted + 1;
                            else if (Pos == "NW") Next = ToBeDeleted - 1;
                            else Next = ToBeDeleted;
                            RowIndex[i] = ToBeDeleted;
                            ShiftUp(ImageMatrix, New_Height, i, ToBeDeleted);
                            ShiftUp2(Energy, New_Height, i, ToBeDeleted);
                            ShiftUp3(RowCost, New_Height, i, ToBeDeleted);
                            ToBeDeleted = Next;
                        }
                        New_H_Sub--;
                        New_Height--;
                    }
                    else
                    {
                        int ToBeDeleted = Col;
                        int Next = 0;
                        LastWasRow = false;
                        for (int i = Old_Height - 1; i >= 5; i--)
                        {
                            string Pos = ColCost[i, ToBeDeleted].position;
                            if (Pos == "NE") Next = ToBeDeleted + 1;
                            else if (Pos == "NW") Next = ToBeDeleted - 1;
                            else Next = ToBeDeleted;
                            ColIndex[i] = ToBeDeleted;
                            ShiftLeft(ImageMatrix, New_Width, i, ToBeDeleted);
                            ShiftLeft2(Energy, New_Width, i, ToBeDeleted);
                            ShiftLeft3(ColCost, New_Width, i, ToBeDeleted);
                            ToBeDeleted = Next;
                        }
                        New_W_Sub--;
                        New_Width--;
                    }
                }
                else if (New_H_Sub > 0 || New_H_Plus > 0)
                {
                    if (New_H_Sub == HSub || New_H_Plus > 0)//First Time or add
                    {
                        Energy = calculateEnergy(ImageMatrix, New_Height, New_Width);
                        RowCost = calculateCostHorizontally(Energy, ImageMatrix, New_Height, New_Width);
                    }
                    else
                    {
                        Energy = calculateEnergy2(ImageMatrix, Energy, New_Height, New_Width, RowIndex, ColIndex, LastWasRow);
                        RowCost = calculateCostHorizontally2(Energy, ImageMatrix, RowCost, New_Height, New_Width, RowIndex[5]);
                    }
                    int Row, Old_Width = New_Width, Old_Height = New_Height;
                    do
                    {
                        MinCol = MinRow = int.MaxValue;
                        Row = GetMinRow(RowCost, Old_Width, Old_Height);
                        if (Row == -1)
                        {
                            break;
                        }
                        if (New_H_Plus > 0)
                        {
                            RowCost[Row, Old_Width - 1].value = int.MaxValue;
                        }
                        int ToBeDeleted = Row;
                        int Next = 0;
                        LastWasRow = true;
                        if (New_H_Plus > 0)
                        {
                            for (int i = Row; i <= New_Height; i++)
                                UsedEnergyIndexH[i]++;
                        }
                        for (int i = Old_Width - 1; i >= 5; i--)
                        {
                            string Pos = RowCost[ToBeDeleted, i].position;
                            if (Pos == "SW") Next = ToBeDeleted + 1;
                            else if (Pos == "NW") Next = ToBeDeleted - 1;
                            else Next = ToBeDeleted;
                            RowIndex[i] = ToBeDeleted;
                            if (New_H_Plus > 0)
                            {
                                ToBeDeleted = UsedEnergyIndexH[Row];
                                ShiftDown(ImageMatrix, New_Height + 1, i, ToBeDeleted);
                                if (ToBeDeleted - 1 >= 5 && ToBeDeleted + 1 < New_Height + 1)
                                    ImageMatrix[ToBeDeleted, i] = CalculatePixelsAvg(ImageMatrix[ToBeDeleted + 1, i], ImageMatrix[ToBeDeleted - 1, i]);
                                else if (ToBeDeleted - 1 >= 5)
                                    ImageMatrix[ToBeDeleted, i] = ImageMatrix[ToBeDeleted - 1, i];
                                else if (ToBeDeleted + 1 < New_Height + 1)
                                    ImageMatrix[ToBeDeleted, i] = ImageMatrix[ToBeDeleted + 1, i];
                            }
                            else
                            {
                                ShiftUp(ImageMatrix, New_Height, i, ToBeDeleted);
                                ShiftUp2(Energy, New_Height, i, ToBeDeleted);
                                ShiftUp3(RowCost, New_Height, i, ToBeDeleted);
                            }
                            ToBeDeleted = Next;
                        }
                        if (New_H_Plus > 0)
                        {
                            New_H_Plus--;
                            New_Height++;
                        }
                        else
                        {
                            New_H_Sub--;
                            New_Height--;
                        }
                    } while (New_H_Plus > 0);
                }
                else if (New_W_Sub > 0 || New_W_Plus > 0)
                {
                    if (New_W_Sub == WSub || New_W_Plus > 0)//First time or add
                    {
                        Energy = calculateEnergy(ImageMatrix, New_Height, New_Width);
                        ColCost = calculateCostVertically(Energy, ImageMatrix, New_Height, New_Width);
                    }
                    else
                    {
                        Energy = calculateEnergy2(ImageMatrix, Energy, New_Height, New_Width, RowIndex, ColIndex, LastWasRow);
                        ColCost = calculateCostVertically2(Energy, ImageMatrix, ColCost, New_Height, New_Width, ColIndex[5]);
                    }
                    int Col, Old_Width = New_Width, Old_Height = New_Height;
                    do
                    {
                        MinCol = int.MaxValue;
                        Col = GetMinCol(ColCost, Old_Width, Old_Height);
                        if (Col == -1)
                        {
                            break;
                        }
                        if (New_W_Plus > 0)
                        {
                            ColCost[Old_Height - 1, Col].value = int.MaxValue;
                        }
                        int ToBeDeleted = Col;
                        int Next = 0;
                        LastWasRow = false;
                        if (New_W_Plus > 0)
                        {
                            for (int i = Col; i <= New_Width; i++)
                                UsedEnergyIndexC[i]++;
                        }
                        for (int i = Old_Height - 1; i >= 5; i--)
                        {
                            string Pos = ColCost[i, ToBeDeleted].position;
                            if (Pos == "NE") Next = ToBeDeleted + 1;
                            else if (Pos == "NW") Next = ToBeDeleted - 1;
                            else Next = ToBeDeleted;
                            ColIndex[i] = ToBeDeleted;
                            if (New_W_Plus > 0)
                            {
                                ToBeDeleted = UsedEnergyIndexC[Col];
                                ShiftRight(ImageMatrix, New_Width + 1, i, ToBeDeleted);
                                if (ToBeDeleted - 1 >= 5 && ToBeDeleted + 1 < New_Width + 1)
                                    ImageMatrix[i, ToBeDeleted] = CalculatePixelsAvg(ImageMatrix[i, ToBeDeleted + 1], ImageMatrix[i, ToBeDeleted - 1]);
                                else if (ToBeDeleted - 1 >= 5)
                                    ImageMatrix[i, ToBeDeleted] = ImageMatrix[i, ToBeDeleted - 1];
                                else if (ToBeDeleted + 1 < New_Width + 1)
                                    ImageMatrix[i, ToBeDeleted] = ImageMatrix[i, ToBeDeleted + 1];
                            }
                            else
                            {
                                ShiftLeft(ImageMatrix, New_Width, i, ToBeDeleted);
                                ShiftLeft2(Energy, New_Width, i, ToBeDeleted);
                                ShiftLeft3(ColCost, New_Width, i, ToBeDeleted);
                            }
                            ToBeDeleted = Next;
                        }
                        if (New_W_Plus > 0)
                        {
                            New_W_Plus--;
                            New_Width++;
                        }
                        else
                        {
                            New_W_Sub--;
                            New_Width--;
                        }
                    } while (New_W_Plus > 0);
                }
            }
            MyColor[,] FinalImageMatrix = new MyColor[New_Height-5, New_Width-5];
            for (int y = 0; y < New_Height-5; y++)
            {
                for (int x = 0; x < New_Width-5; x++)
                {
                    FinalImageMatrix[y, x] = ImageMatrix[y+5, x+5];
                }
            }
            return FinalImageMatrix;
        }

        private static int[,] calculateEnergy(MyColor[,] Data, int height, int width)
        {
            int[,] Energy = new int[height+5, width+5];
            int X, Y, E;
            for (int y = 5; y < height; y++)
            {
                for (int x = 5; x < width; x++)
                {
                    E = 0;
                    for (int _y = -1; _y <= 1; _y++)
                    {
                        for (int _x = -1; _x <= 1; _x++)
                        {
                            Y = y + _y;
                            X = x + _x;
                            if (X >= 5 && X < width && Y >= 5 && Y < height)
                            {
                                E += CalculatePixelsEnergy(Data[y, x], Data[Y, X]);
                            }
                        }
                    }
                    Energy[y, x] = E;
                }
            }
            return Energy;
        }
        private static Point[,] calculateCostVertically(int[,] Energy, MyColor[,] Data, int height, int width)
        {
            Point[,] Cost = new Point[height+5, width+5];
            for (int y = 5; y < height; y++)
            {
                for (int x = 5; x < width ; x++)
                {
                    int oldleft = Energy[y, x - 1], oldright = Energy[y, x + 1], newleft = oldleft, newright = oldright;

                    newright -= (CalculatePixelsEnergy(Data[y, x + 1], Data[y - 1, x + 2]) + CalculatePixelsEnergy(Data[y, x + 1], Data[y + 1, x + 2]) + CalculatePixelsEnergy(Data[y, x + 1], Data[y, x]));
                    newright += (CalculatePixelsEnergy(Data[y, x + 1], Data[y - 1, x - 2]) + CalculatePixelsEnergy(Data[y, x + 1], Data[y, x - 2]) + CalculatePixelsEnergy(Data[y, x + 1], Data[y + 1, x - 2]));

                    newleft -= (CalculatePixelsEnergy(Data[y, x - 1], Data[y, x]));
                    newleft += (CalculatePixelsEnergy(Data[y, x - 1], Data[y, x + 1]));
                    if (x == 5)
                        Energy[y - 1, x - 1] = int.MaxValue;
                    else if (x == width -1)
                        Energy[y - 1, x + 1] = int.MaxValue;
                    Cost[y , x ].value = Math.Abs((oldright + oldleft) - (newright + newleft)) + Math.Min(Energy[y - 1, x - 1], Math.Min(Energy[y - 1, x], Energy[y - 1, x + 1]));
                    if (Energy[y - 1, x - 1] <= Energy[y - 1, x] && Energy[y - 1, x - 1] <= Energy[y - 1, x + 1])
                        Cost[y , x ].position = "NW";
                    else if (Energy[y - 1, x] <= Energy[y - 1, x - 1] && Energy[y - 1, x] <= Energy[y - 1, x + 1])
                        Cost[y , x ].position = "N";
                    else if (Energy[y - 1, x + 1] <= Energy[y - 1, x - 1] && Energy[y - 1, x + 1] <= Energy[y - 1, x])
                        Cost[y , x ].position = "NE";
                    if (x == 5)
                        Energy[y - 1, x - 1] = 0;
                    else if (x == width -1)
                        Energy[y - 1, x + 1] = 0;
                }
            }
            return Cost;
        }
        private static Point[,] calculateCostHorizontally(int[,] Energy, MyColor[,] Data, int height, int width)
        {
            Point[,] Cost = new Point[height+5, width+5];
            for (int y = height -1; y >= 5; y--)
            {
                for (int x = 5; x < width ; x++)
                {
                    int oldleft = Energy[y + 1, x], oldright = Energy[y - 1, x], newleft = oldleft, newright = oldright;

                    newright -= (CalculatePixelsEnergy(Data[y - 1, x], Data[y, x]) + CalculatePixelsEnergy(Data[y - 1, x], Data[y - 2, x - 1]) + CalculatePixelsEnergy(Data[y - 1, x], Data[y - 2, x + 1]));
                    newright += (CalculatePixelsEnergy(Data[y - 1, x], Data[y + 1, x - 1]) + CalculatePixelsEnergy(Data[y - 1, x], Data[y + 1, x]) + CalculatePixelsEnergy(Data[y - 1, x], Data[y + 1, x + 1]));

                    newleft -= (CalculatePixelsEnergy(Data[y + 1, x], Data[y, x]));
                    newleft += (CalculatePixelsEnergy(Data[y + 1, x], Data[y - 1, x]));
                    if (y == 5)
                        Energy[y - 1, x - 1] = int.MaxValue;
                    else if (y == height -1)
                        Energy[y + 1, x - 1] = int.MaxValue;

                    Cost[y , x ].value = Math.Abs((oldright + oldleft) - (newright + newleft)) + Math.Min(Energy[y - 1, x - 1], Math.Min(Energy[y, x - 1], Energy[y + 1, x - 1]));
                    if (Energy[y - 1, x - 1] <= Energy[y, x - 1] && Energy[y - 1, x - 1] <= Energy[y + 1, x - 1])
                        Cost[y , x ].position = "NW";
                    else if (Energy[y, x - 1] <= Energy[y - 1, x - 1] && Energy[y, x - 1] <= Energy[y + 1, x - 1])
                        Cost[y , x ].position = "W";
                    else if (Energy[y + 1, x - 1] <= Energy[y - 1, x - 1] && Energy[y + 1, x - 1] <= Energy[y, x - 1])
                        Cost[y , x ].position = "SW";
                    if (y == 5)
                        Energy[y - 1, x - 1] = 0;
                    else if (y == height -1)
                        Energy[y + 1, x - 1] = 0;
                }
            }
            return Cost;
        }

        private static int[,] calculateEnergy2(MyColor[,] Data, int[,] OldEnergy, int height, int width, int[] Row, int[] Col, bool LastWasRow)
        {
            int[,] Energy = OldEnergy;
            int E, Y, X;
            if (!LastWasRow)
            {
                int y = 5;
                for (int i = 5; i < height - 1; i++)
                {
                    int x = Col[i];

                    for (int XX = x - 2; XX < x + 2; XX++)
                    {
                        if (XX < 5 || XX >= width)
                            continue;
                        E = 0;
                        for (int _y = -1; _y <= 1; _y++)
                        {
                            for (int _x = -1; _x <= 1; _x++)
                            {
                                Y = y + _y;
                                X = XX + _x;
                                if (X >= 5 && X < width && Y >= 5 && Y < height)
                                {
                                    E += CalculatePixelsEnergy(Data[y, XX], Data[Y, X]);
                                }
                            }
                        }
                        Energy[y, XX] = E;
                    }
                    y++;
                }
            }
            else
            {
                int x = 5;
                for (int i = 5; i < width - 1; i++)
                {
                    int y = Row[i];

                    for (int YY = y - 2; YY < y + 2; YY++)
                    {
                        if (YY < 5 || YY >= height)
                            continue;
                        E = 0;
                        for (int _y = -1; _y <= 1; _y++)
                        {
                            for (int _x = -1; _x <= 1; _x++)
                            {
                                Y = YY + _y;
                                X = x + _x;
                                if (X >= 5 && X < width && Y >= 5 && Y < height)
                                {
                                    E += CalculatePixelsEnergy(Data[YY, x], Data[Y, X]);
                                }
                            }
                        }
                        Energy[YY, x] = E;
                    }
                    x++;
                }
            }
            return Energy;
        }
        private static Point[,] calculateCostVertically2(int[,] Energy, MyColor[,] Data, Point[,] OldCost, int height, int width, int X)
        {
            Point[,] Cost = OldCost;
            for (int y = 5; y < height; y++)
            {
                for (int _x = -(y - 5); _x <= (y - 5); _x++)
                {
                    int x = X + 5 + _x;
                    if (x < 5 || x >= width)
                        continue;
                    int oldleft = Energy[y, x - 1], oldright = Energy[y, x + 1], newleft = oldleft, newright = oldright;

                    newright -= (CalculatePixelsEnergy(Data[y, x + 1], Data[y - 1, x + 2]) + CalculatePixelsEnergy(Data[y, x + 1], Data[y + 1, x + 2]) + CalculatePixelsEnergy(Data[y, x + 1], Data[y, x]));
                    newright += (CalculatePixelsEnergy(Data[y, x + 1], Data[y - 1, x - 2]) + CalculatePixelsEnergy(Data[y, x + 1], Data[y, x - 2]) + CalculatePixelsEnergy(Data[y, x + 1], Data[y + 1, x - 2]));

                    newleft -= (CalculatePixelsEnergy(Data[y, x - 1], Data[y, x]));
                    newleft += (CalculatePixelsEnergy(Data[y, x - 1], Data[y, x + 1]));
                    if (x == 5)
                        Energy[y - 1, x - 1] = int.MaxValue;
                    else if (x == width -1)
                        Energy[y - 1, x + 1] = int.MaxValue;
                    Cost[y , x ].value = Math.Abs((oldright + oldleft) - (newright + newleft)) + Math.Min(Energy[y - 1, x - 1], Math.Min(Energy[y - 1, x], Energy[y - 1, x + 1]));
                    if (Energy[y - 1, x - 1] <= Energy[y - 1, x] && Energy[y - 1, x - 1] <= Energy[y - 1, x + 1])
                        Cost[y , x ].position = "NW";
                    else if (Energy[y - 1, x] <= Energy[y - 1, x - 1] && Energy[y - 1, x] <= Energy[y - 1, x + 1])
                        Cost[y , x ].position = "N";
                    else if (Energy[y - 1, x + 1] <= Energy[y - 1, x - 1] && Energy[y - 1, x + 1] <= Energy[y - 1, x])
                        Cost[y , x ].position = "NE";
                    if (x == 5)
                        Energy[y - 1, x - 1] = 0;
                    else if (x == width -1)
                        Energy[y - 1, x + 1] = 0;
                }
            }
            return Cost;
        }
        private static Point[,] calculateCostHorizontally2(int[,] Energy, MyColor[,] Data, Point[,] OldCost, int height, int width, int Y)
        {
            Point[,] Cost = OldCost;

            for (int x = 5; x < width ; x++)
            {
                for (int _y = -(x - 5); _y <= (x - 5); _y++)
                {
                    int y = Y + 5 + _y;
                    if (y < 5 || y >= height )
                        continue;
                    int oldleft = Energy[y + 1, x], oldright = Energy[y - 1, x], newleft = oldleft, newright = oldright;

                    newright -= (CalculatePixelsEnergy(Data[y - 1, x], Data[y, x]) + CalculatePixelsEnergy(Data[y - 1, x], Data[y - 2, x - 1]) + CalculatePixelsEnergy(Data[y - 1, x], Data[y - 2, x + 1]));
                    newright += (CalculatePixelsEnergy(Data[y - 1, x], Data[y + 1, x - 1]) + CalculatePixelsEnergy(Data[y - 1, x], Data[y + 1, x]) + CalculatePixelsEnergy(Data[y - 1, x], Data[y + 1, x + 1]));

                    newleft -= (CalculatePixelsEnergy(Data[y + 1, x], Data[y, x]));
                    newleft += (CalculatePixelsEnergy(Data[y + 1, x], Data[y - 1, x]));
                    if (y == 5)
                        Energy[y - 1, x - 1] = int.MaxValue;
                    else if (y == height -1)
                        Energy[y + 1, x - 1] = int.MaxValue;

                    Cost[y , x ].value = Math.Abs((oldright + oldleft) - (newright + newleft)) + Math.Min(Energy[y - 1, x - 1], Math.Min(Energy[y, x - 1], Energy[y + 1, x - 1]));
                    if (Energy[y - 1, x - 1] <= Energy[y, x - 1] && Energy[y - 1, x - 1] <= Energy[y + 1, x - 1])
                        Cost[y , x ].position = "NW";
                    else if (Energy[y, x - 1] <= Energy[y - 1, x - 1] && Energy[y, x - 1] <= Energy[y + 1, x - 1])
                        Cost[y , x ].position = "W";
                    else if (Energy[y + 1, x - 1] <= Energy[y - 1, x - 1] && Energy[y + 1, x - 1] <= Energy[y, x - 1])
                        Cost[y , x ].position = "SW";
                    if (y == 5)
                        Energy[y - 1, x - 1] = 0;
                    else if (y == height -1)
                        Energy[y + 1, x - 1] = 0;
                }
            }
            return Cost;
        }

    }
}
