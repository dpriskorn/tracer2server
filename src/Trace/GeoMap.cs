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
using Tracer2Server.Tiles;
using Tracer2Server.WebServer;
using Tracer2Server.Projection;

namespace Tracer2Server.Trace
{
    class GeoMap
    {
        private TracerParam m_oTracerParam;

        private int m_nTileArraySize;
        private int m_nTileCenter;

        private PointGeo m_oInnerPoint;

        private int m_nResolution;
        private int m_nResolutionMask;
        private int m_nResolutionShift;

        private Point m_oFirstTileNr;
        private double m_dTileSize;

        private Object[,] m_aoData;

        public int m_nXYmax;
        private RectangleGeo m_oRectangleGeoAll;

        public GeoMap(TracerParam oTracerParam)
        {
            m_oTracerParam = oTracerParam;

            Tiler oTiler = m_oTracerParam.m_oTiler;

            m_oInnerPoint = oTracerParam.m_oServerEventArgs.oStartPoint;

            m_dTileSize = oTiler.dTileSize;
            m_nResolution = oTiler.nResolution;
            m_nResolutionMask = m_nResolution - 1;
            m_nResolutionShift = oTiler.nResolutionShift;

            m_nXYmax = 65536 - 1;
            m_nTileArraySize = (m_nXYmax + 1) / m_nResolution;
            m_nTileCenter = m_nTileArraySize / 2;

            m_aoData = new Object[m_nTileArraySize, m_nTileArraySize];
            m_oFirstTileNr = oTiler.GetTileNrByPointGeo(m_oInnerPoint);
            m_oFirstTileNr.X -= m_nTileCenter;
            m_oFirstTileNr.Y -= m_nTileCenter;

            m_oRectangleGeoAll = new RectangleGeo(
                (m_oFirstTileNr.X) * m_dTileSize,
                (m_oFirstTileNr.Y) * m_dTileSize,
                (m_oFirstTileNr.X + m_nTileArraySize) * m_dTileSize,
                (m_oFirstTileNr.Y + m_nTileArraySize) * m_dTileSize
                );
        }

        public byte GetPix(int nX, int nY)
        {
            if (nX < 0 || nX > m_nXYmax || nY < 0 || nY > m_nXYmax)
            {
                throw new Exception("Area to big");
            }

            int x = nX & m_nResolutionMask;
            int y = m_nResolutionMask - (nY & m_nResolutionMask);
            int nTX = nX >> m_nResolutionShift;
            int nTY = nY >> m_nResolutionShift;

            Object oData = m_aoData[nTX, nTY];
            if (oData == null)
            {
                GetTile(nTX, nTY);
                oData = m_aoData[nTX, nTY];
            }
            return ((byte[][])oData)[y][x];
        }

        private void GetTile(int nTX, int nTY)
        {
            Tile oTile;

            Point pointXY = new Point(
                nTX * m_nResolution,
                nTY * m_nResolution
                );

            RectangleGeo rectGeo = new RectangleGeo(
                (m_oFirstTileNr.X + nTX) * m_dTileSize,
                (m_oFirstTileNr.Y + nTY + 1) * m_dTileSize,
                (m_oFirstTileNr.X + nTX + 1) * m_dTileSize,
                (m_oFirstTileNr.Y + nTY) * m_dTileSize
                );

            oTile = new Tile(rectGeo, pointXY, m_nResolution);

            m_aoData[nTX, nTY] = (Object)m_oTracerParam.m_oFileCache.GetTile(oTile);
        }

        public PointGeo ConvertPointToGeo(PointD oPointD)
        {
            PointGeo oPointGeo = new PointGeo();

            double dLon = ((double)(oPointD.dX + 1) / (m_nResolution * m_nTileArraySize)) * (m_oRectangleGeoAll.dRight - m_oRectangleGeoAll.dLeft) + m_oRectangleGeoAll.dLeft;
            double dLat = (((double)(m_nResolution * m_nTileArraySize)) - oPointD.dY - 0.5) / (m_nResolution * m_nTileArraySize) * (m_oRectangleGeoAll.dTop - m_oRectangleGeoAll.dBottom) + m_oRectangleGeoAll.dBottom;

            if (double.IsNaN(oPointGeo.dLon) || double.IsNaN(oPointGeo.dLat))
            {
                return new PointGeo(dLat, dLon);
            }
            return new PointGeo(dLat, dLon);
        }

        public Point ConvertGeoToPoint(PointGeo point)
        {
            return new Point(
                (int)Math.Round((point.dLon - m_oRectangleGeoAll.dLeft) / (m_oRectangleGeoAll.dRight - m_oRectangleGeoAll.dLeft) * m_nResolution * m_nTileArraySize),
                (m_nResolution * m_nTileArraySize) - (int)Math.Round((point.dLat - m_oRectangleGeoAll.dBottom) / (m_oRectangleGeoAll.dTop - m_oRectangleGeoAll.dBottom) * m_nResolution * m_nTileArraySize)
                );
        }

    }

}
