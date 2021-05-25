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

using System.Globalization;

namespace Tracer2Server.Projection
{
    public struct RectangleGeo
    {
        private PointGeo m_oTopLeft;
        private PointGeo m_oBottomRight;

        public double dTop
        {
            get { return m_oTopLeft.dLat; }
        }

        public double dLeft
        {
            get { return m_oTopLeft.dLon; }
        }

        public double dBottom
        {
            get { return m_oBottomRight.dLat; }
        }

        public double dRight
        {
            get { return m_oBottomRight.dLon; }
        }

        public RectangleGeo(PointGeo topLeft, PointGeo bottomRight)
        {
            this.m_oTopLeft = topLeft;
            this.m_oBottomRight = bottomRight;
        }

        public RectangleGeo(double dLeft, double dTop, double dRight, double dBottom)
        {
            m_oTopLeft = new PointGeo(dTop, dLeft);
            m_oBottomRight = new PointGeo(dBottom, dRight);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0:F4} {1:F4} {2:F4} {3:F4}", dLeft, dTop, dRight, dBottom);
        }

    }

}
