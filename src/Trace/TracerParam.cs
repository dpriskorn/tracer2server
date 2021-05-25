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
using System.Diagnostics;
using Tracer2Server;
using Tracer2Server.Tiles;
using Tracer2Server.Projection;
using Tracer2Server.WebServer;

namespace Tracer2Server.Trace
{
    delegate void ServerEventHandler(object sender, EventType e);

    public enum EventType { Start, AddTile, AddBorderPoints, AddBorderStep, Stop };

    class TracerParam : EventArgs
    {
        static public event ServerEventHandler s_oEventHandler;
        static private Object s_oLock = new Object();

        public ServerEventArgs m_oServerEventArgs;

        public Tiler m_oTiler;
        public GeoMap m_oGeoMap;
        internal TileCache m_oFileCache;

        public PointGeo m_oInnerPointGeo;
        public Point m_oInnerPointXY;
        public Color m_oInnerColor;
        public Rectangle m_oShapeAreaXY;

        public TimeSpan m_oTimeSpan;
        private Stopwatch m_oStopWatch;

        public List<TileBitmap> m_listTileBitmap = new List<TileBitmap>();
        private Object m_oLock = new Object();

        public string m_strHeadLine = "";
        public string m_strResult = "";
        public string m_strOutput = "";
        public string m_strError = "";
        public string m_strStackTrace = "";

        internal Point m_oStartPointXY;
        internal List<Point> m_listBorderPoints;
        internal List<BorderPart[]> m_listBorderSteps = new List<BorderPart[]>();
        internal List<String> m_listBorderStepsName = new List<string>();

        public TracerParam(ServerEventArgs args)
        {
            m_oServerEventArgs = args;

            m_oTiler = new Tiler(this);
            m_oGeoMap = new GeoMap(this);
            m_oFileCache = new TileCache(this);

            m_oInnerPointGeo = args.oStartPoint;
            m_oInnerPointXY = m_oGeoMap.ConvertGeoToPoint(m_oInnerPointGeo);
            m_oInnerColor = Color.Black;

            m_oStopWatch = new Stopwatch();
            m_oStopWatch.Start();

            m_strHeadLine = "Trace  " + m_oInnerPointGeo.ToString();
            FireEvent(EventType.Start);
        }

        public void AddNewTile(TileBitmap oTileBitmap)
        {
            if (oTileBitmap != null)
            {
                lock (m_oLock)
                {
                    string strTime = string.Format("{0:00}.{1:000}", oTileBitmap.m_oTimeSpan.Seconds, oTileBitmap.m_oTimeSpan.Milliseconds);
                    if (oTileBitmap.m_bDownloadViaWms == true)
                    {
                        m_strOutput += "Download tile  Rect=" + oTileBitmap.m_oTile.ToString() + "  Time=" + strTime + "s\r\n";
                    }
                    else
                    {
                        m_strOutput += "Load tile  Rect=" + oTileBitmap.m_oTile.ToString() + "  Time=" + strTime + "s\r\n";
                    }
                    m_listTileBitmap.Add(oTileBitmap);

                    FireEvent(EventType.AddTile);
                }
            }
        }

        public void SetStartPoint(Point oStartPointXY)
        {
            if (oStartPointXY != null)
            {
                lock (m_oLock)
                {
                    m_oStartPointXY = oStartPointXY;
                }
            }
        }

        public void SetBorderPoints(List<Point> listBorderPoints)
        {
            if (listBorderPoints != null)
            {
                lock (m_oLock)
                {
                    m_listBorderPoints = listBorderPoints;

                    m_strOutput += "Get border points -> " + listBorderPoints.Count + " points\r\n";

                    FireEvent(EventType.AddBorderPoints);
                }
            }
        }

        public void SetBorderLine(List<BorderPart> listBorderParts, string strName)
        {
            int nLineCont = 0;
            int nArcCount = 0;
            int nPointGeoCount = 0;
            string strTemp = "";

            if (listBorderParts != null)
            {
                lock (m_oLock)
                {
                    List<BorderPart> listBorderPartsCopy = new List<BorderPart>();
                    foreach (BorderPart oBP in listBorderParts)
                    {
                        if (oBP.GetType() == typeof(BorderPartLine))
                        {
                            nLineCont++;
                        }
                        else if (oBP.GetType() == typeof(BorderPartArc))
                        {
                            nArcCount++;
                        }
                        else if (oBP.GetType() == typeof(BorderPartPointGeo))
                        {
                            nPointGeoCount++;
                        }
                        else
                        {
                            continue;
                        }
                        listBorderPartsCopy.Add(oBP.ShallowCopy());
                    }
                    m_listBorderSteps.Add((BorderPart[])listBorderPartsCopy.ToArray());

                    if (nLineCont > 0)
                    {
                        strTemp += nLineCont.ToString() + " lines ";
                    }
                    if (nArcCount > 0)
                    {
                        strTemp += nArcCount.ToString() + " arc ";
                    }
                    if (nPointGeoCount > 0)
                    {
                        strTemp += nPointGeoCount.ToString() + " points ";
                    }
                    strTemp = strName + " -> " + strTemp.Trim();
                    m_listBorderStepsName.Add(strTemp);

                    m_strOutput += strTemp + "\r\n";

                    FireEvent(EventType.AddBorderStep);
                }
            }
        }

        public void SetError(string strError)
        {
            lock (m_oLock)
            {
                m_strOutput += strError + "\r\n";
                m_strError = strError;
            }
        }

        public void Stop()
        {
            lock (m_oLock)
            {
                m_oStopWatch.Stop();
                m_oTimeSpan = m_oStopWatch.Elapsed;

                if (m_strError.Equals(""))
                {
                    m_strResult = m_oServerEventArgs.strOrder + " " + m_oInnerPointGeo.ToString() + " finishes -> " + m_listBorderSteps[m_listBorderSteps.Count - 1].Length + " points";
                }
                else
                {
                    m_strResult = m_oServerEventArgs.strOrder + " " + m_oInnerPointGeo.ToString() + " error -> " + m_strError;
                }
                string strTime = string.Format("{0:00}.{1:000}", m_oTimeSpan.Seconds, m_oTimeSpan.Milliseconds);
                m_strOutput += "Request finishes  Time=" + strTime + "s\r\n";

                m_oFileCache.ClearAllData();
                GC.Collect();

                FireEvent(EventType.Stop);
            }
        }

        private void FireEvent(EventType oEventType)
        {
            if (s_oEventHandler != null)
            {
                s_oEventHandler(this, oEventType);
            }
        }

        static private void FireEvent(EventType oEventType, TracerParam oTracerParam)
        {
            lock (s_oLock)
            {
                if (s_oEventHandler != null)
                {
                    s_oEventHandler(oTracerParam, oEventType);
                }
            }
        }

    }

}
