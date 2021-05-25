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
using System.Drawing;
using Tracer2Server.WebServer;
using Tracer2Server.Trace;
using Tracer2Server.Projection;

namespace Tracer2Server.Tiles
{
    class Tiler
    {
        private TracerParam m_oTracerParam;

        private double m_dTileSize = 0.0002;
        private int m_nResolution = 1024;
        private int m_nResolutionShift = 10;
        private int m_nPreLoadSize = 1;

        public Tiler(TracerParam oTracerParam)
        {
            m_oTracerParam = oTracerParam;

            m_dTileSize = oTracerParam.m_oServerEventArgs.dTileSize;

            int i = 0;
            m_nResolution = oTracerParam.m_oServerEventArgs.nResolution;
            m_nResolution >>= 1;
            while (m_nResolution != 0)
            {
                i++;
                m_nResolution >>= 1;
            }
            m_nResolution = 1 << i;
            m_nResolutionShift = i;

            m_nPreLoadSize = m_nResolution > 1024 ? 1 : m_nResolution > 512 ? 2 : 3;
        }

        public double dTileSize
        {
            get { return m_dTileSize; }
        }
        public int nResolution
        {
            get { return m_nResolution; }
        }
        public int nResolutionShift
        {
            get { return m_nResolutionShift; }
        }

        public Tile[] GetTilesByTile(Tile oTile)
        {
            int minX = (int)Math.Round((oTile.oRectangle.dLeft / m_dTileSize));
            int minY = (int)Math.Round((oTile.oRectangle.dBottom / m_dTileSize));

            Point oPoint = oTile.oFirstPointXY;

            Tile[] tiles = new Tile[(m_nPreLoadSize * 2 + 1) * (m_nPreLoadSize * 2 + 1)];
            long ycount = 0;
            for (int y = -m_nPreLoadSize; y <= m_nPreLoadSize; y++)
            {
                for (int x = -m_nPreLoadSize; x <= m_nPreLoadSize; x++)
                {
                    Point oPointXY = new Point(
                        oPoint.X + (x * m_nResolution),
                        oPoint.Y + (y * m_nResolution)
                        );

                    if (oPointXY.X >= 0 && oPointXY.X <= m_oTracerParam.m_oGeoMap.m_nXYmax && oPointXY.X >= 0 && oPointXY.X <= m_oTracerParam.m_oGeoMap.m_nXYmax)
                    {
                        RectangleGeo oRectGeo = new RectangleGeo(
                            (minX + x) * m_dTileSize,
                            (minY + y + 1) * m_dTileSize,
                            (minX + x + 1) * m_dTileSize,
                            (minY + y) * m_dTileSize
                            );
                        tiles[ycount] = new Tile(oRectGeo, oPointXY, m_nResolution);
                    }
                    ycount++;
                }
            }
            return tiles;
        }

        public Point GetTileNrByPointGeo(PointGeo oPoint)
        {
            int minX = (int)Math.Floor(oPoint.dLon / m_dTileSize);
            int minY = (int)Math.Floor(oPoint.dLat / m_dTileSize);

            return new Point(minX, minY);
        }

    }

}
