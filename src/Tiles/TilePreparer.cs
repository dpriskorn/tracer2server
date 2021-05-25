/**
 *  Tracer2Server - Server for the JOSM plugin Tracer2
 *  
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Threading;
using Tracer2Server.Trace;
using Tracer2Server.WebServer;

namespace Tracer2Server.Tiles
{
    class TilePreparer
    {
        const byte AREA = 0;
        const byte BORDER = 1;

        public byte[][] Prepare(Bitmap oBitmap, TracerParam oTracerParam)
        {
            int nHeight = oBitmap.Height;
            int nWidth = oBitmap.Width;
            BitmapData oBitmapData = oBitmap.LockBits(new Rectangle(0, 0, nWidth, nHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int nBitmapStride = oBitmapData.Stride;
            int nThreshold = oTracerParam.m_oServerEventArgs.nThreshold;
            byte[][] aucResult;

            aucResult = new byte[nHeight][];
            for (int x = 0; x < aucResult.Length; x++)
            {
                aucResult[x] = new byte[nWidth];
            }

            unsafe
            {
                byte* pucBitmapData = (byte*)(void*)oBitmapData.Scan0;
                int nYpos = 0;
                byte[] aucRow;

                switch (oTracerParam.m_oServerEventArgs.strMode)
                {
                    case "boundary":
                        for (int y = 0; y < nHeight; y++)
                        {
                            aucRow = aucResult[y];
                            for (int x = 0; x < nWidth; x++)
                            {
                                if (pucBitmapData[x * 3 + 2 + nYpos] < nThreshold && pucBitmapData[x * 3 + 1 + nYpos] < nThreshold && pucBitmapData[x * 3 + 0 + nYpos] < nThreshold)
                                    aucRow[x] = BORDER;
                                else
                                    aucRow[x] = AREA;
                            }
                            nYpos += nBitmapStride;
                        }
                        break;
                    case "match color":
                        int nReTry = 1600; // 16 seconds
                        while ((nReTry > 0) && (oTracerParam.m_oInnerColor == Color.Black))
                        {
                            Thread.Sleep(10);
                            nReTry--;
                        }
                        if (nReTry > 0)
                        {
                            int nR = oTracerParam.m_oInnerColor.R;
                            int nG = oTracerParam.m_oInnerColor.G;
                            int nB = oTracerParam.m_oInnerColor.B;
                            int nRx;
                            int nGx;
                            int nBx;

                            for (int y = 0; y < nHeight; y++)
                            {
                                aucRow = aucResult[y];
                                for (int x = 0; x < nWidth; x++)
                                {
                                    nBx = pucBitmapData[x * 3 + 0 + nYpos] - nB;
                                    nGx = pucBitmapData[x * 3 + 1 + nYpos] - nG;
                                    nRx = pucBitmapData[x * 3 + 2 + nYpos] - nR;

                                    nBx = nBx < 0 ? -nBx : nBx;
                                    nGx = nGx < 0 ? -nGx : nGx;
                                    nRx = nRx < 0 ? -nRx : nRx;

                                    if (nBx + nGx + nRx > nThreshold)
                                        aucRow[x] = BORDER;
                                    else
                                        aucRow[x] = AREA;
                                }
                                nYpos = nYpos + nBitmapStride;
                            }
                        }
                        else
                        {
                            aucResult = null;
                        }

                        break;
                    default:
                        aucResult = null;
                        break;
                }
            }
            oBitmap.UnlockBits(oBitmapData);

            return aucResult;
        }

    }

}
