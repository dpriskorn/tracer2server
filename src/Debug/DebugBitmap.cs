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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Tracer2Server.Trace;
using System.IO;

namespace Tracer2Server.Debug
{
    class DebugBitmap
    {
        const byte BACKGROUND = 0;
        const byte PEN = 1;
        const byte OBJECT = 2;
        const byte BORDER = 3;
        const byte TRACK = 4;

        private Color[] m_acColor = { Color.White, Color.LightBlue, Color.LightGreen, Color.Yellow, Color.Blue };

        private GeoMap m_oGeoMap;
        private Bitmap m_oBitmap;
        private Rectangle m_oBitmapArea;

        static private string s_strDebugprefix = "debug//";

        public GeoMap oGeoMap
        {
            get
            {
                return m_oGeoMap;
            }
            set
            {
                m_oGeoMap = value;
            }
        }

        public DebugBitmap()
        {
            if (DebugStatic.bDebugBitmap)
            {
                DirectoryInfo diCache = new DirectoryInfo(s_strDebugprefix);
                try
                {
                    if (!diCache.Exists)
                    {
                        diCache.Create();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The process failed: {0}", e.ToString());
                }
                finally { }
            }
        }

        private void SetBitmapArea(Rectangle oShapeArea)
        {
            int xmin = oShapeArea.Left - 50;
            int xmax = oShapeArea.Right + 50;
            int ymin = PosY2Bitmap(oShapeArea.Bottom) - 50;
            int ymax = PosY2Bitmap(oShapeArea.Top) + 50;

            if (xmin < 0)
            {
                xmin = 0;
            }
            if (xmax > m_oGeoMap.m_nXYmax)
            {
                xmax = m_oGeoMap.m_nXYmax;
            }
            if (ymin < 0)
            {
                ymin = 0;
            }
            if (ymax > m_oGeoMap.m_nXYmax)
            {
                ymax = m_oGeoMap.m_nXYmax;
            }
            m_oBitmapArea = new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        public void MakeBitmap(Point oInnerPointXY, Rectangle oShapeArea)
        {
            int x;
            int y;
            byte ucData;

            if (!DebugStatic.bDebugBitmap) return;

            SetBitmapArea(oShapeArea);

            try
            {
                m_oBitmap = new Bitmap(m_oBitmapArea.Width, m_oBitmapArea.Height, PixelFormat.Format32bppArgb);

                for (x = 0; x < m_oBitmapArea.Width; x++)
                {
                    for (y = 0; y < m_oBitmapArea.Height; y++)
                    {
                        ucData = m_oGeoMap.GetPix(x + m_oBitmapArea.Left, PosY2Bitmap(y + m_oBitmapArea.Top));
                        if (ucData < 5)
                        {
                            m_oBitmap.SetPixel(x, y, m_acColor[ucData]);
                        }
                        else
                        {
                            m_oBitmap.SetPixel(x, y, Color.Black);
                        }
                    }
                }
                try
                {
                    m_oBitmap.SetPixel(oInnerPointXY.X - m_oBitmapArea.Left, PosY2Bitmap(oInnerPointXY.Y) - m_oBitmapArea.Top, Color.Black);
                }
                catch //( Exception e )
                {
                    // Start Pos in not inside of Shape
                }
                m_oBitmap.Save(s_strDebugprefix + "Base.bmp");
            }
            catch
            {
            }
        }

        public void MakeBitmap(string strName, BorderPart[] aoBorderLines)
        {
            if (!DebugStatic.bDebugBitmap) return;

            Bitmap oNewBitmap = (Bitmap)m_oBitmap.Clone();
            Color oGray = Color.FromArgb(255, 128, 128, 128);
            try
            {
                foreach (BorderPartLine l in aoBorderLines.ToArray())
                {
                    if (oNewBitmap.GetPixel(l.m_oPointStart.X - m_oBitmapArea.Left, PosY2Bitmap(l.m_oPointStart.Y) - m_oBitmapArea.Top).Equals(oGray))
                    {
                        oNewBitmap.SetPixel(l.m_oPointStart.X - m_oBitmapArea.Left, PosY2Bitmap(l.m_oPointStart.Y) - m_oBitmapArea.Top, Color.Black);
                    }
                    else
                    {
                        oNewBitmap.SetPixel(l.m_oPointStart.X - m_oBitmapArea.Left, PosY2Bitmap(l.m_oPointStart.Y) - m_oBitmapArea.Top, oGray);
                    }

                    if (oNewBitmap.GetPixel(l.m_oPointEnd.X - m_oBitmapArea.Left, PosY2Bitmap(l.m_oPointEnd.Y) - m_oBitmapArea.Top).Equals(oGray))
                    {
                        oNewBitmap.SetPixel(l.m_oPointEnd.X - m_oBitmapArea.Left, PosY2Bitmap(l.m_oPointEnd.Y) - m_oBitmapArea.Top, Color.Black);
                    }
                    else
                    {
                        oNewBitmap.SetPixel(l.m_oPointEnd.X - m_oBitmapArea.Left, PosY2Bitmap(l.m_oPointEnd.Y) - m_oBitmapArea.Top, oGray);
                    }
                }
                oNewBitmap.Save(s_strDebugprefix + strName + ".bmp");
            }
            catch
            {
            }
        }

        private int PosY2Bitmap(int y)
        {
            return m_oGeoMap.m_nXYmax - y;
        }

    }

}
