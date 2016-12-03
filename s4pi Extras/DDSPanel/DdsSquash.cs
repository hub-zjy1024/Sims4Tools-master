/***************************************************************************
 *  Copyright (C) 2016 by Camille Marinetti                          *
 *                                                                         *
 *  This file is part of the Sims 4 Package Interface (s4pi)               *
 *                                                                         *
 *  s4pi is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  s4pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s4pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Drawing
{
    internal sealed class DdsSquash
    {
        public static byte[] CompressImage(byte[] pixelInput, int width, int height, Dds.DdsFormat format)
        {
           // byte[] blocks = new byte[(width * height) / (format == Dds.DdsFormat.ATI1 ? 2 : 1)];
            List<byte> blocks = new List<byte>();

            byte[] red = new byte[width * height];
            byte[] green = new byte[width * height];
            for (int i = 0; i < width * height; i++)
            {
                red[i] = pixelInput[i * 4];
                green[i] = pixelInput[(i * 4) + 1];
            }

            byte[,] r = new byte[height, width];
            byte[,] g = new byte[height, width];
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    r[h, w] = red[(h * width) + w];
                    if (format == Dds.DdsFormat.ATI2)
                    {
                        g[h, w] = green[(h * width) + w];
                    }
                }
            }

            int blockSize = format == Dds.DdsFormat.ATI1 ? 8 : 16;
            for (int i = 0; i < height; i += 4)
            {
                for (int j = 0; j < width; j += 4)
                {
                    byte[] unCompressedColor = new byte[16];
                    byte[] compressedColor;
                    if (format == Dds.DdsFormat.ATI2)
                    {
                        unCompressedColor = new byte[16];
                        for (int h = 0; h < 4; h++)
                        {
                            for (int w = 0; w < 4; w++)
                            {
                                unCompressedColor[(h * 4) + w] = g[i + h, j + w];
                            }
                        }
                        compressedColor = CompressColor(unCompressedColor);
                        blocks.AddRange(compressedColor);
                    }
                    for (int h = 0; h < 4; h++)
                    {
                        for (int w = 0; w < 4; w++)
                        {
                            unCompressedColor[(h * 4) + w] = r[i + h, j + w];
                        }
                    }
                    compressedColor = CompressColor(unCompressedColor);
                    blocks.AddRange(compressedColor);
                }
            }

            return blocks.ToArray();
        }

        internal static byte[] CompressColor(byte[] unCompressedColor)
        {
            byte minColor = Byte.MaxValue, maxColor = Byte.MinValue;
            bool bwmode = false;

            foreach (byte b in unCompressedColor)
            {
                if (b < minColor) minColor = b;
                if (b > maxColor) maxColor = b;
            }

            if (minColor <= 5 || maxColor >= 250)
            {
                byte minColor2 = Byte.MaxValue, maxColor2 = Byte.MinValue;
                foreach (byte b in unCompressedColor)
                {
                    if (b < minColor2 & b > 5) minColor2 = b;
                    if (b > maxColor2 & b < 250) maxColor2 = b;
                }
                if (minColor2 <= maxColor2)
                {
                    if (maxColor2 - minColor2 < maxColor - minColor)
                    {
                        bwmode = true;
                        minColor = minColor2;
                        maxColor = maxColor2;
                    }
                }
            }

            int padRange = bwmode ? 4 : 6;
            if (maxColor - minColor < padRange)
            {
                if (maxColor <= padRange) maxColor = (byte)(minColor + padRange);
                else minColor = (byte)(maxColor - padRange);
            }

            int[] ind = new int[16];

            if (bwmode)
            {
                byte[] color = new byte[4];
                int increment = (maxColor - minColor) / 5;
                int half = increment / 2;
                color[0] = (byte)(minColor + increment); // bit code 010
                color[1] = (byte)(minColor + (increment * 2)); // bit code 011
                color[2] = (byte)(minColor + (increment * 3)); // bit code 100
                color[3] = (byte)(minColor + (increment * 4)); // bit code 101

                for (int i = 0; i < unCompressedColor.Length; i++)
                {
                    if (unCompressedColor[i] <= 5) ind[i] = 6;
                    else if (unCompressedColor[i] >= 250) ind[i] = 7;
                    else if (unCompressedColor[i] <= minColor + half) ind[i] = 0;
                    else if (unCompressedColor[i] <= color[0] + half) ind[i] = 2;
                    else if (unCompressedColor[i] <= color[1] + half) ind[i] = 3;
                    else if (unCompressedColor[i] <= color[2] + half) ind[i] = 4;
                    else if (unCompressedColor[i] <= color[3] + half) ind[i] = 5;
                    else ind[i] = 1;
                }
            }
            else
            {
                byte[] color = new byte[6];
                int increment = (maxColor - minColor) / 7;
                int half = increment / 2;
                color[0] = (byte)(minColor + (increment * 6)); // bit code 010
                color[1] = (byte)(minColor + (increment * 5)); // bit code 011
                color[2] = (byte)(minColor + (increment * 4)); // bit code 100
                color[3] = (byte)(minColor + (increment * 3)); // bit code 101
                color[4] = (byte)(minColor + (increment * 2)); // bit code 110
                color[5] = (byte)(minColor + increment); // bit code 111

                for (int i = 0; i < unCompressedColor.Length; i++)
                {
                    if (unCompressedColor[i] >= maxColor - half) ind[i] = 0;
                    else if (unCompressedColor[i] >= color[0] - half) ind[i] = 2;
                    else if (unCompressedColor[i] >= color[1] - half) ind[i] = 3;
                    else if (unCompressedColor[i] >= color[2] - half) ind[i] = 4;
                    else if (unCompressedColor[i] >= color[3] - half) ind[i] = 5;
                    else if (unCompressedColor[i] >= color[4] - half) ind[i] = 6;
                    else if (unCompressedColor[i] >= color[5] - half) ind[i] = 7;
                    else ind[i] = 1;
                }
            }

            byte[] block = new byte[8];
            if (bwmode)
            {
                block[0] = minColor;
                block[1] = maxColor;
            }
            else
            {
                block[0] = maxColor;
                block[1] = minColor;
            }

            int tmp = ind[0] + (ind[1] << 3) + (ind[2] << 6) + (ind[3] << 9) + (ind[4] << 12) + (ind[5] << 15) + (ind[6] << 18) + (ind[7] << 21);
            int tmp2 = ind[8] + (ind[9] << 3) + (ind[10] << 6) + (ind[11] << 9) + (ind[12] << 12) + (ind[13] << 15) + (ind[14] << 18) + (ind[15] << 21);

            block[2] = (byte)(tmp & 0xFF);
            block[3] = (byte)((tmp & 0xFF00) >> 8);
            block[4] = (byte)((tmp & 0xFF0000) >> 16);
            block[5] = (byte)(tmp2 & 0xFF);
            block[6] = (byte)((tmp2 & 0xFF00) >> 8);
            block[7] = (byte)((tmp2 & 0xFF0000) >> 16);

            return block;
        }

        public static byte[] DecompressImage(byte[] blocks, int width, int height, Dds.DdsFormat format)
        {
            // Allocate room for decompressed output
            byte[] pixelOutput = new byte[width * height * 4];

            byte[,] c1 = new byte[height, width];
            byte[,] c2 = new byte[height, width];

            int blockSize = format == Dds.DdsFormat.ATI1 ? 8 : 16;
            int columnCounter = 0, rowCounter = 0;
            for (int i = 0; i < blocks.Length; i += blockSize)
            {
                byte[] compressedColor = new byte[8];
                Array.Copy(blocks, i, compressedColor, 0, 8);
                byte[] decompressedColor = DecompressColor(compressedColor);
                for (int h = 0; h < 4; h++)
                {
                    for (int w = 0; w < 4; w++)
                    {
                        c1[rowCounter + h, columnCounter + w] = decompressedColor[(h * 4) + w];
                    }
                }
                if (format == Dds.DdsFormat.ATI2)
                {
                    compressedColor = new byte[8];
                    Array.Copy(blocks, i + 8, compressedColor, 0, 8);
                    decompressedColor = DecompressColor(compressedColor);
                    for (int h = 0; h < 4; h++)
                    {
                        for (int w = 0; w < 4; w++)
                        {
                            c2[rowCounter + h, columnCounter + w] = decompressedColor[(h * 4) + w];
                        }
                    }
                }
                columnCounter += 4;
                if (columnCounter >= width)
                {
                    rowCounter += 4;
                    columnCounter = 0;
                }
            }

            byte[] red = new byte[width * height];
            byte[] green = new byte[width * height];
            byte[] blue = new byte[width * height];
            byte[] alpha = new byte[width * height];
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    red[(h * width) + w] = c1[h, w];
                    if (format == Dds.DdsFormat.ATI2)
                    {
                        red[(h * width) + w] = c2[h, w];
                        green[(h * width) + w] = c1[h, w];
                    }
                    else
                    {
                        red[(h * width) + w] = c1[h, w];
                        green[(h * width) + w] = c1[h, w];
                        blue[(h * width) + w] = c1[h, w];
                    }
                    alpha[(h * width) + w] = 255;
                }
            }
            for (int i = 0; i < width * height; i++)
            {
                pixelOutput[i * 4] = red[i];
                pixelOutput[(i * 4) + 1] = green[i];
                pixelOutput[(i * 4) + 2] = blue[i];
                pixelOutput[(i * 4) + 3] = alpha[i];
            }

            // Return our pixel data to caller..
            return pixelOutput;
        }

        internal static byte[] DecompressColor(byte[] compressedColor)
        {
            byte[] decompressed = new byte[16];
            byte[] color = new byte[8];
            color[0] = compressedColor[0];
            color[1] = compressedColor[1];
            if (color[0] > color[1])
            {
                color[2] = (byte)((6 * color[0] + 1 * color[1]) / 7.0f); // bit code 010
                color[3] = (byte)((5 * color[0] + 2 * color[1]) / 7.0f); // bit code 011
                color[4] = (byte)((4 * color[0] + 3 * color[1]) / 7.0f); // bit code 100
                color[5] = (byte)((3 * color[0] + 4 * color[1]) / 7.0f); // bit code 101
                color[6] = (byte)((2 * color[0] + 5 * color[1]) / 7.0f); // bit code 110
                color[7] = (byte)((1 * color[0] + 6 * color[1]) / 7.0f); // bit code 111
            }
            else
            {
                color[2] = (byte)((4 * color[0] + 1 * color[1]) / 5.0f); // bit code 010
                color[3] = (byte)((3 * color[0] + 2 * color[1]) / 5.0f); // bit code 011
                color[4] = (byte)((2 * color[0] + 3 * color[1]) / 5.0f); // bit code 100
                color[5] = (byte)((1 * color[0] + 4 * color[1]) / 5.0f); // bit code 101
                color[6] = 0;                                            // bit code 110
                color[7] = 255;                                          // bit code 111
            }

            byte[] ind = new byte[16];
            ind[0] = (byte)(compressedColor[2] & 0x07);
            ind[1] = (byte)((compressedColor[2] & 0x38) >> 3);
            ind[2] = (byte)(((compressedColor[2] & 0xC0) >> 6) + ((compressedColor[3] & 0x01) << 2));
            ind[3] = (byte)((compressedColor[3] & 0x0E) >> 1);
            ind[4] = (byte)((compressedColor[3] & 0x70) >> 4);
            ind[5] = (byte)(((compressedColor[3] & 0x80) >> 7) + ((compressedColor[4] & 0x03) << 1));
            ind[6] = (byte)((compressedColor[4] & 0x1C) >> 2);
            ind[7] = (byte)((compressedColor[4] & 0xE0) >> 5);
            ind[8] = (byte)(compressedColor[5] & 0x07);
            ind[9] = (byte)((compressedColor[5] & 0x38) >> 3);
            ind[10] = (byte)(((compressedColor[5] & 0xC0) >> 6) + ((compressedColor[6] & 0x01) << 2));
            ind[11] = (byte)((compressedColor[6] & 0x0E) >> 1);
            ind[12] = (byte)((compressedColor[6] & 0x70) >> 4);
            ind[13] = (byte)(((compressedColor[6] & 0x80) >> 7) + ((compressedColor[7] & 0x03) << 1));
            ind[14] = (byte)((compressedColor[7] & 0x1C) >> 2);
            ind[15] = (byte)((compressedColor[7] & 0xE0) >> 5);

            for (int i = 0; i < 16; i++)
            {
                decompressed[i] = color[ind[i]];
            }

            return decompressed;
        }
    }
}
