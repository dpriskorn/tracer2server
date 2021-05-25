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
using System.Globalization;
using Tracer2Server.Projection;

namespace Tracer2Server.Trace
{
    public struct StraightLine
    {
        private double m_dm;        // slope (gradient)
        private double m_dc;        // y-intercept
        private double m_dx;        // x-intercept (if y-intercept == NaN)
        private double m_dAlpha;    // alpha (-180° < alpha <= 180°)

        public StraightLine(double dm, double dc, bool bRevert)
        {
            m_dm = dm;
            m_dc = dc;
            m_dx = double.NaN;
            if (double.IsNaN(m_dm))
            {
                m_dAlpha = double.NaN;
            }
            else if (double.IsInfinity(m_dm))
            {
                m_dAlpha = CheckAlpha(double.IsNegativeInfinity(m_dm) ? -90 : 90);
            }
            else
            {
                m_dAlpha = CheckAlpha(Math.Atan(dm) * 180 / Math.PI + (bRevert ? 180 : 0));
            }
        }

        public StraightLine(PointD oP1, PointD oP2)
        {
            m_dm = (oP2.dY - oP1.dY) / (oP2.dX - oP1.dX);
            m_dc = oP1.dY - (m_dm * oP1.dX);

            if (double.IsInfinity(m_dm) || double.IsNaN(m_dm))
            {
                m_dx = oP1.dX;
                m_dAlpha = CheckAlpha(oP1.dY > oP2.dY ? -90 : 90);
            }
            else
            {
                m_dx = double.NaN;
                m_dAlpha = CheckAlpha(Math.Atan((oP2.dY - oP1.dY) / (oP2.dX - oP1.dX)) * 180 / Math.PI + (oP1.dX > oP2.dX ? 180 : 0));
            }
        }

        //public StraightLine(PointD oP1, double dAlpha)
        //{
        //}

        static public double CheckAlpha(double dAlpha)
        {
            if (dAlpha > 180)
            {
                return dAlpha - 360;
            }
            if (dAlpha <= -180)
            {
                return dAlpha + 360;
            }
            return dAlpha;
        }

        public double dm
        {
            get { return m_dm; }
        }
        public double dc
        {
            get { return m_dc; }
        }
        public double dx
        {
            get { return m_dx; }
        }
        public double dAlpha
        {
            get { return m_dAlpha; }
        }
        public bool IsLine
        {
            get
            {
                return !(double.IsNaN(dx) || double.IsInfinity(dx) || double.IsNaN(dAlpha) || double.IsInfinity(dAlpha))
                    || !(double.IsNaN(dm) || double.IsInfinity(dm) || double.IsNaN(dc) || double.IsInfinity(dc));
            }
        }

        public static bool operator ==(StraightLine oLine1, StraightLine oLine2)
        {
            return
                oLine1.dm == oLine2.dm &&
                oLine1.dc == oLine2.dc &&
                oLine1.dx == oLine2.dx &&
                oLine1.dAlpha == oLine2.dAlpha;
        }

        public static bool operator !=(StraightLine oLine1, StraightLine oLine2)
        {
            return !(oLine1 == oLine2);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StraightLine)) return false;
            StraightLine oLineD = (StraightLine)obj;
            return
                oLineD.dm == this.dm &&
                oLineD.dc == this.dc &&
                oLineD.dx == this.dx &&
                oLineD.dAlpha == this.dAlpha &&
                oLineD.GetType().Equals(this.GetType());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "m={0:F3},	c={1:F3}, alpha={2:F3},	x={3:F3}", dm, dc, dAlpha, dx);
        }

        public PointD GetIntersectionPoint(StraightLine oLine)
        {
            double dx;
            double dy;

            if (!this.IsLine || !oLine.IsLine)
            {
                // data missing
                return new PointD(double.NaN, double.NaN);
            }

            if (double.IsInfinity(this.dm) || double.IsNaN(this.dm))
            {
                if (double.IsInfinity(oLine.dm) || double.IsNaN(oLine.dm))
                {
                    // no IntersectionPoint
                    return new PointD(double.NaN, double.NaN);
                }
                else
                {
                    dx = this.dx;
                    dy = oLine.dm * dx + oLine.dc;
                }
            }
            else if (double.IsInfinity(oLine.dm) || double.IsNaN(oLine.dm))
            {
                dx = oLine.dx;
                dy = this.dm * dx + this.dc;
            }
            else
            {
                dx = (oLine.m_dc - this.dc) / (this.dm - oLine.m_dm);
                dy = this.m_dm * dx + this.m_dc;
            }
            return new PointD(dx, dy);
        }

    }

}
