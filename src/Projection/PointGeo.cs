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
using System;

namespace Tracer2Server.Projection
{
    public struct PointGeo
    {
        private double m_dLat;
        public double m_dLon;

        public PointGeo(double dLat, double dLon)
        {
            m_dLat = dLat;
            m_dLon = dLon;
        }

        public double dLat
        {
            get { return m_dLat; }
            //set { m_dLat = value; }
        }

        public double dLon
        {
            get { return m_dLon; }
            //set { m_dLon = value; }
        }

        public static explicit operator PointGeo(PointD oPointD)
        {
            return new PointGeo((int)oPointD.dX, (int)oPointD.dY);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "(Lat={0:F6}, Lon={1:F6})", dLat, dLon);
        }

    }

}
