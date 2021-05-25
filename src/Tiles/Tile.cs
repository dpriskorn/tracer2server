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
using Tracer2Server.Projection;

namespace Tracer2Server.Tiles
{
    public class Tile
    {
        private RectangleGeo m_oRectangle;
        private Point m_oFirstPointXY;
        private int m_nResolution;

        public RectangleGeo oRectangle
        {
            get { return new RectangleGeo(m_oRectangle.dLeft, m_oRectangle.dTop, m_oRectangle.dRight, m_oRectangle.dBottom); }
        }
        public Point oFirstPointXY
        {
            get { return new Point(m_oFirstPointXY.X, m_oFirstPointXY.Y); }
        }
        public int nResolution
        {
            get { return m_nResolution; }
        }

        public Tile(RectangleGeo oRectangle, Point oFirstPointXY, int nResolution)
        {
            this.m_oRectangle = oRectangle;
            this.m_oFirstPointXY = oFirstPointXY;
            this.m_nResolution = nResolution;
        }

        public Tile Clone()
        {
            return new Tile(this.oRectangle, this.oFirstPointXY, this.m_nResolution);
        }

        public override string ToString()
        {
            return m_oRectangle.ToString();
        }

    }

}
