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
using System.Globalization;
using System.Net;
using System.Drawing;
using System.IO;
using Tracer2Server.WebServer;
using System.Reflection;

namespace Tracer2Server.Tiles
{
    public class TileDownloader
    {
        string m_strUrl;
        int m_nTreshold;
        int m_nSkipBottom;

        public TileDownloader(ServerEventArgs oServerEventArgs)
        {
            m_nSkipBottom = 0;

            m_strUrl = oServerEventArgs.strUrl;
            int startPos = m_strUrl.IndexOf("wms:");
            if (startPos == 0)
            {
                m_strUrl = m_strUrl.Remove(0, 4);
            }
            startPos = m_strUrl.IndexOf("EPSG:");
            if (startPos >= 0)
            {
                string strTemp = m_strUrl.Substring(startPos, 9);
                m_strUrl = m_strUrl.Replace("SRS={proj(" + strTemp + ")}", "SRS=" + strTemp);
            }
            m_strUrl = m_strUrl.Replace("SRS={proj}", "SRS=EPSG:4326");

            m_nTreshold = oServerEventArgs.nThreshold;
            m_nSkipBottom = oServerEventArgs.nSkipBottom;
        }

        public Bitmap Download(Tile oTile)
        {
            NumberFormatInfo oNFIDefault = System.Globalization.NumberFormatInfo.InvariantInfo;
            double dSkipBottom = (double)m_nSkipBottom / oTile.nResolution * (oTile.oRectangle.dTop - oTile.oRectangle.dBottom);

            string strBox = oTile.oRectangle.dLeft.ToString("F4", oNFIDefault) + ","
                + oTile.oRectangle.dBottom.ToString("F4", oNFIDefault) + ","
                + oTile.oRectangle.dRight.ToString("F4", oNFIDefault) + ","
                + (oTile.oRectangle.dTop + dSkipBottom).ToString("F4", oNFIDefault);
            string strWidth = oTile.nResolution.ToString();
            string strHeight = (oTile.nResolution + m_nSkipBottom).ToString();

            string strUrl = m_strUrl;
            strUrl = strUrl.Replace("{bbox}", strBox);
            strUrl = strUrl.Replace("{width}", strWidth);
            strUrl = strUrl.Replace("{height}", strHeight);

            using (WebClient wc = new WebClient())
            {
                try
                {
                    Version Version = Assembly.GetCallingAssembly().GetName().Version;
                    wc.Headers.Add("user-agent", "Tracer2Server/" + Version.Major.ToString() + "." + Version.Minor.ToString());

                    byte[] data = wc.DownloadData(strUrl);
                    using (Bitmap b = new Bitmap(oTile.nResolution, oTile.nResolution, System.Drawing.Imaging.PixelFormat.Format24bppRgb))
                    {
                        try
                        {
                            using (Graphics g = Graphics.FromImage(b))
                            {
                                using (MemoryStream ms = new MemoryStream(data))
                                {
                                    using (Bitmap tileBitmap = new Bitmap(ms))
                                    {
                                        g.DrawImage(tileBitmap, new Rectangle(new Point(), b.Size), new Rectangle(new Point(0, m_nSkipBottom), b.Size), GraphicsUnit.Pixel);
                                    }
                                }
                            }
                            return (Bitmap)b.Clone();
                        }
                        catch //( Exception e )
                        {
                            return null;
                        }
                    }
                }
                catch //( Exception e )
                {
                    return null;
                }
            }
        }

    }

}
