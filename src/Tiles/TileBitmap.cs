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
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using Tracer2Server.Debug;
using Tracer2Server.WebServer;
using Tracer2Server.Trace;

namespace Tracer2Server.Tiles
{
    class TileBitmap
    {
        private TracerParam m_oTracerParam;

        public Tile m_oTile;
        public string m_strFileName;
        private byte[][] m_aucData;
        private bool m_bClearData = false;

        private Thread m_oThread;
        private Object m_oLock = new Object();

        public TimeSpan m_oTimeSpan;
        public bool m_bDownloadViaWms = false;

        public TileBitmap(Tile oTile, string strFileName, TracerParam oTracerParam)
        {
            m_oTile = oTile.Clone();
            m_strFileName = (string)strFileName.Clone();
            m_oTracerParam = oTracerParam;

            m_oThread = new Thread(ThreadReadData);
            m_oThread.Start(this);
        }

        public override string ToString()
        {
            return "Bitmap " + m_oTile.ToString();
        }

        public void ClearData()
        {
            lock (m_oLock)
            {
                m_aucData = null;
                m_bClearData = true;
            }
        }

        public byte[][] GetData()
        {
            byte[][] aucData = null;

            lock (m_oLock)
            {
                if (m_aucData != null)
                {
                    return (byte[][])m_aucData.Clone();
                }
            }

            DebugStatic.oDebugWatch.StartWatch("GetData");
            int nReTry = 1500;  // 15 seconds
            while (aucData == null && nReTry != 0)
            {
                Thread.Sleep(10);
                lock (m_oLock)
                {
                    if (m_aucData != null)
                    {
                        m_oThread = null;
                        aucData = (byte[][])m_aucData.Clone();
                    }
                    else
                    {
                        nReTry--;
                    }
                }
            }

            if (nReTry == 0)
            {
                try
                {
                    m_oThread.Abort();
                    m_oThread = null;
                }
                catch
                {
                }
            }
            DebugStatic.oDebugWatch.StopWatch("GetData");

            return aucData;
        }

        public void SetData(byte[][] aoData)
        {
            lock (m_oLock)
            {
                if (m_aucData == null && aoData != null)
                {
                    m_aucData = aoData;
                }
            }
            Thread.Sleep(10);
            lock (m_oLock)
            {
                if (m_bClearData == true)
                {
                    m_aucData = null;
                }
            }
        }

        public static void ThreadReadData(object oObject)
        {
            TileDownloader m_oTileDownloader;
            TileBitmap oTileBitmap = (TileBitmap)oObject;
            TilePreparer oPreparer = new TilePreparer();
            byte[][] aoData = null;
            Bitmap oBitmap = null;
            Stopwatch oStopWatch;
            string strTemp;

            oStopWatch = new Stopwatch();
            oStopWatch.Start();

            int nReTry = 3;
            while ((aoData == null) && (nReTry > 0) && (File.Exists(oTileBitmap.m_strFileName)))
            {
                try
                {
                    oBitmap = new Bitmap(oTileBitmap.m_strFileName);
                    oTileBitmap.GetColor(oBitmap);
                    aoData = oPreparer.Prepare(oBitmap, oTileBitmap.m_oTracerParam);
                    oBitmap = null;
                }
                catch
                {
                    Thread.Sleep(10);
                    nReTry--;
                }
            }

            if (aoData == null)
            {
                strTemp = "TileDownloader " + oTileBitmap.m_oTile.ToString();
                DebugStatic.oDebugWatch.StartWatch(strTemp);
                m_oTileDownloader = new TileDownloader(oTileBitmap.m_oTracerParam.m_oServerEventArgs);
                oBitmap = m_oTileDownloader.Download(oTileBitmap.m_oTile);
                DebugStatic.oDebugWatch.StopWatch(strTemp);

                if (oBitmap != null)
                {
                    oTileBitmap.GetColor(oBitmap);
                    aoData = oPreparer.Prepare(oBitmap, oTileBitmap.m_oTracerParam);
                    oTileBitmap.m_bDownloadViaWms = true;
                }
            }

            oTileBitmap.SetData(aoData);

            if (oTileBitmap.m_bDownloadViaWms == true)
            {
                strTemp = "Bitmap.Save " + oTileBitmap.m_oTile.ToString();
                DebugStatic.oDebugWatch.StartWatch(strTemp);
                if (!File.Exists(oTileBitmap.m_strFileName))
                {
                    oBitmap.Save(oTileBitmap.m_strFileName);
                    oBitmap = null;
                }
                DebugStatic.oDebugWatch.StopWatch(strTemp);
            }

            oStopWatch.Stop();
            oTileBitmap.m_oTimeSpan = oStopWatch.Elapsed;
            oTileBitmap.m_oTracerParam.AddNewTile(oTileBitmap);
        }

        public void GetColor(Bitmap oBitmap)
        {
            Point oInnerPointXY = m_oTracerParam.m_oInnerPointXY;
            Tile oTile = m_oTile;

            if (oTile.oFirstPointXY.X <= oInnerPointXY.X && oTile.oFirstPointXY.Y <= oInnerPointXY.Y)
            {
                int x = oInnerPointXY.X - oTile.oFirstPointXY.X;
                int y = oInnerPointXY.Y - oTile.oFirstPointXY.Y;

                if (x < oTile.nResolution && y < oTile.nResolution)
                {
                    y = (oTile.nResolution - 1) - y;

                    Color oColor = oBitmap.GetPixel(x, y);
                    m_oTracerParam.m_oInnerColor = oColor;
                    //oBitmap.SetPixel(x, y, Color.White);
                    //oBitmap.Save(m_strFileName);
                }
            }
            return;
        }

    }

}
