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
using System.Globalization;
using Tracer2Server.Projection;
using Tracer2Server.Trace.Fit;

namespace Tracer2Server.Trace
{
    class BorderPartArc : BorderPart
    {
        // Ax^2 + Bxy + Cy^2 + Dx + Ey + F = 0
        double m_dA = Double.NaN;
        double m_dB = Double.NaN;
        double m_dC = Double.NaN;
        double m_dD = Double.NaN;
        double m_dE = Double.NaN;
        double m_dF = Double.NaN;

        PointD m_oEllipseCenter;
        double m_da = Double.NaN;
        double m_db = Double.NaN;
        double m_dTauDeg = Double.NaN;
        double m_dTauRad = Double.NaN;

        List<Point> m_listPoints = new List<Point>();
        List<Point> m_listCheckPoints = new List<Point>();

        bool m_bClockwise = true;
        double m_dAlphaStartDeg = Double.NaN;
        double m_dAlphaEndDeg = Double.NaN;

        public BorderPartLine m_oLineStart = null;
        public BorderPartLine m_oLineEnd = null;

        public bool isElipse()
        {
            return ((m_dB * m_dB - 4 * m_dA * m_dC) < 0);
        }

        public List<PointD> GetBorderPoints(int nPointsPerCircle)
        {
            List<PointD> listPointD = new List<PointD>();

            for (int i = 0; i < nPointsPerCircle; i++)
            {
                listPointD.Add(GetPointOfEllipse((Math.PI * 2 / nPointsPerCircle) * i));
            }
            return listPointD;
        }

        public List<PointD> GetBorderPoints(PointD oStart, PointD oEnd, int nPointsPerCircle)
        {
            List<PointD> listPointD = new List<PointD>();
            double dDiff;
            double dAlphaStart0 = GetAlpha0(oStart);
            double dAlphaEnd0 = GetAlpha0(oEnd);
            int nDirection = 0;

            bool bClockwise = isClockwise();

            if (bClockwise == true)
            {
                dDiff = dAlphaStart0 - dAlphaEnd0;
                nDirection = -1;
            }
            else
            {
                dDiff = dAlphaEnd0 - dAlphaStart0;
                nDirection = 1;
            }

            if (dDiff < 0)
            {
                dDiff += Math.PI * 2;
            }
            else if (dDiff > Math.PI * 2)
            {
                dDiff -= Math.PI * 2;
            }

            int nCount = (int)Math.Round((dDiff / (Math.PI * 2 / nPointsPerCircle)), MidpointRounding.AwayFromZero);
            if (nCount < 4)
            {
                nCount = 4;
            }
            dDiff /= nCount;
            for (int i = 1; i < nCount; i++)
            {
                listPointD.Add(GetPointOfEllipse(dAlphaStart0 + (i * dDiff * nDirection)));
            }
            return listPointD;
        }

        public bool Point2Ellipse(Ellipse oE, List<Point> listPoints, List<Point> listCheckPoints)
        {
            Point oPoint;
            double dAlphaStart;
            double dAlphaEnd;
            double dDiff;

            m_dA = oE.m_dA;
            m_dB = oE.m_dB;
            m_dC = oE.m_dC;
            m_dD = oE.m_dD;
            m_dE = oE.m_dE;
            m_dF = oE.m_dF;

            m_oEllipseCenter = oE.m_oEllipseCenter;
            m_da = oE.m_da;
            m_db = oE.m_db;
            m_dTauDeg = oE.m_dThetaDeg;
            m_dTauRad = oE.m_dThetaRad;

            m_listPoints = listPoints;
            m_listCheckPoints = listCheckPoints;

            if (m_listCheckPoints.Count() < 2)
            {
                return false;
            }
            oPoint = m_listCheckPoints[0];
            dAlphaStart = GetAlpha0(new PointD(oPoint.X, oPoint.Y));
            oPoint = m_listCheckPoints[1];
            dAlphaEnd = GetAlpha0(new PointD(oPoint.X, oPoint.Y));

            dDiff = dAlphaStart - dAlphaEnd;
            if (dDiff > Math.PI)
            {
                dDiff -= 2 * Math.PI;
            }
            if (dDiff < -Math.PI)
            {
                dDiff += 2 * Math.PI;
            }
            m_bClockwise = dDiff > 0;

            oPoint = m_listCheckPoints[0];
            m_dAlphaStartDeg = GetAlpha(new PointD(oPoint.X, oPoint.Y)) * 180 / Math.PI;
            oPoint = m_listCheckPoints[m_listCheckPoints.Count - 1];
            m_dAlphaEndDeg = GetAlpha(new PointD(oPoint.X, oPoint.Y)) * 180 / Math.PI;

            return isElipse() && isEllipseOk() && arePointsPartOfEllipse();
        }

        private bool isClockwise()
        {
            return m_bClockwise;
        }

        public bool isEllipseOk()
        {
            if (m_da > m_db * 2)
            {
                return false;
            }
            if (m_db > m_da * 2)
            {
                return false;
            }
            return true;
        }

        public bool arePointsPartOfEllipse()
        {
            foreach (Point oP in m_listCheckPoints)
            {
                if (isPointPartOfEllipse(oP) == false)
                {
                    return false;
                }
            }
            return true;
        }

        private double GetAlpha(Point oPoint)
        {
            double dAlphaRad = 0;

            if (oPoint.X == m_oEllipseCenter.dX)
            {
                dAlphaRad = m_oEllipseCenter.dY > oPoint.Y ? -Math.PI / 2 : Math.PI / 2;
            }
            else
            {
                dAlphaRad = Math.Atan((((double)oPoint.Y) - m_oEllipseCenter.dY) / (((double)oPoint.X) - m_oEllipseCenter.dX));
                if (oPoint.X < m_oEllipseCenter.dX)
                {
                    dAlphaRad += Math.PI;
                }
            }
            return dAlphaRad;
        }

        private double GetAlpha(PointD oPoint)
        {
            double dAlphaRad = 0;

            if (oPoint.dX == m_oEllipseCenter.dX)
            {
                dAlphaRad = m_oEllipseCenter.dY > oPoint.dY ? -Math.PI / 2 : Math.PI / 2;
            }
            else
            {
                dAlphaRad = Math.Atan((((double)oPoint.dY) - m_oEllipseCenter.dY) / (((double)oPoint.dX) - m_oEllipseCenter.dX));
                if (oPoint.dX < m_oEllipseCenter.dX)
                {
                    dAlphaRad += Math.PI;
                }
            }
            return dAlphaRad;
        }

        private double GetAlpha0(PointD oPoint)
        {
            double dAlphaRad = 0;
            double dAlphaRad0 = 0;
            double dr = Math.Sqrt(Math.Pow(oPoint.dX - m_oEllipseCenter.dX, 2) + Math.Pow(oPoint.dY - m_oEllipseCenter.dY, 2));

            dAlphaRad = GetAlpha(oPoint);
            dAlphaRad -= m_dTauRad;

            PointD oPoint0 = new PointD(dr * Math.Cos(dAlphaRad), dr * Math.Sin(dAlphaRad));

            // get alpha for smalest distance iterative
            dAlphaRad0 = 0;
            for (int i = 0; i < 20; i++)
            {
                dAlphaRad0 = Math.Atan(((Math.Pow(m_da, 2) - Math.Pow(m_db, 2)) * Math.Sin(dAlphaRad0) + (Math.Abs(oPoint0.dY * m_db))) / ((Math.Abs(oPoint0.dX * m_da))));
            }
            if (dAlphaRad < 0)
            {
                dAlphaRad += Math.PI * 2;
            }
            else if (dAlphaRad >= Math.PI * 2)
            {
                dAlphaRad -= Math.PI * 2;
            }
            if (dAlphaRad <= Math.PI)
            {
                if (dAlphaRad > Math.PI / 2)
                {
                    dAlphaRad0 = Math.PI - dAlphaRad0;
                }
            }
            else
            {
                if (dAlphaRad > Math.PI * 1.5)
                {
                    dAlphaRad0 = -dAlphaRad0;
                }
                else
                {
                    dAlphaRad0 += Math.PI;
                }
            }
            return dAlphaRad0;
        }

        private bool isPointPartOfEllipse(Point oPoint)
        {
            double dAlphaRad = 0;
            double dr = Math.Sqrt(Math.Pow(oPoint.X - m_oEllipseCenter.dX, 2) + Math.Pow(oPoint.Y - m_oEllipseCenter.dY, 2));

            dAlphaRad = GetAlpha(oPoint);
            dAlphaRad -= m_dTauRad;

            PointD oPoint0 = new PointD(dr * Math.Cos(dAlphaRad), dr * Math.Sin(dAlphaRad));

            // get alpha for smalest distance iterative
            dAlphaRad = 0;
            for (int i = 0; i < 20; i++)
            {
                dAlphaRad = Math.Atan(((Math.Pow(m_da, 2) - Math.Pow(m_db, 2)) * Math.Sin(dAlphaRad) + (Math.Abs(oPoint0.dY * m_db))) / ((Math.Abs(oPoint0.dX * m_da))));
            }

            double d0 = GetDistanceToEllipse0(oPoint0, dAlphaRad);
            if (d0 >= 3)
            {
                return false;
            }
            return true;
        }

        private double GetDistanceToEllipse0(PointD oPoint, double dAlphaRad)
        {
            double x = m_da * Math.Cos(dAlphaRad);
            double y = m_db * Math.Sin(dAlphaRad);

            return Math.Sqrt(Math.Pow(Math.Abs(x) - Math.Abs(oPoint.dX), 2) + Math.Pow(Math.Abs(y) - Math.Abs(oPoint.dY), 2));
        }

        private PointD GetPointOfEllipse(double dAlphaRad)
        {
            double x = m_oEllipseCenter.dX + Math.Cos(m_dTauRad) * m_da * Math.Cos(dAlphaRad) - Math.Sin(m_dTauRad) * m_db * Math.Sin(dAlphaRad);
            double y = m_oEllipseCenter.dY + Math.Sin(m_dTauRad) * m_da * Math.Cos(dAlphaRad) + Math.Cos(m_dTauRad) * m_db * Math.Sin(dAlphaRad);

            return new PointD(x, y);
        }


        public override BorderPart ShallowCopy()
        {
            return (BorderPart)this.MemberwiseClone();
        }

        public override String GetTypName()
        {
            return "Arc";
        }

        public override Point[] GetPoints()
        {
            Point[] aoPoints = new Point[m_listPoints.Count() + 1];
            int i = 0;

            aoPoints[i++] = new Point((int)m_oEllipseCenter.dX, (int)m_oEllipseCenter.dY);

            foreach (Point oP in m_listPoints)
            {
                aoPoints[i++] = oP;
            }
            return aoPoints;
        }

        public override Point GetCenterPoint()
        {
            return new Point((int)m_oEllipseCenter.dX, (int)m_oEllipseCenter.dY);
        }

        public override string ToString()
        {
            string strCenter = string.Format(CultureInfo.CurrentCulture, "X={0:D}, Y={1:D}, ", (int)m_oEllipseCenter.dX, (int)m_oEllipseCenter.dY);
            string strEllipse = string.Format(CultureInfo.CurrentCulture, "a={0:D}, b={1:D}, τ={2:F1}, ", (int)m_da, (int)m_db, m_dTauDeg);
            string strAlpha = string.Format(CultureInfo.CurrentCulture, "α1={0:F1}, α2={1:F1}", m_dAlphaStartDeg, m_dAlphaEndDeg);
            return strCenter + strEllipse + strAlpha;
        }

    }

}
