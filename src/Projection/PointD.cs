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

using System.Drawing;
using System.Globalization;

namespace Tracer2Server.Projection
{
    public struct PointD
    {
        private double m_dX;
        private double m_dY;

        public PointD(double dX, double dY)
        {
            m_dX = dX;
            m_dY = dY;
        }

        public double dX
        {
            get { return m_dX; }
            //set { m_dX = value; }
        }

        public double dY
        {
            get { return m_dY; }
            //set { m_dY = value; }
        }

        public static PointD operator +(PointD oPoint1, PointD oPoint2)
        {
            return new PointD(oPoint1.dX + oPoint2.dX, oPoint1.dY + oPoint2.dY);
        }

        public static PointD operator +(PointD oPoint1, Point oPoint2)
        {
            return new PointD(oPoint1.dX + oPoint2.X, oPoint1.dY + oPoint2.Y);
        }

        public static explicit operator Point(PointD oPointD)
        {
            return new Point((int)oPointD.dX, (int)oPointD.dY);
        }

        public static explicit operator PointD(Point oPoint)
        {
            return new PointD(oPoint.X, oPoint.Y);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "(x={0}, y={1})", dX, dY);
        }

    }

}
