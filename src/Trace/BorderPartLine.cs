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

namespace Tracer2Server.Trace
{
    class BorderPartLine : BorderPart
    {
        public Point m_oPointStart;
        public Point m_oPointEnd;
        public int m_nSize;
        public int[] m_nPartLength = new int[2] { 0, 0 };
        public Point m_oDirection;
        public Point m_oDirectionConnect;

        public double m_dm;
        public double m_dc;
        public double m_dAlpha;

        public int nDeltaX
        {
            get { return Math.Abs(m_oPointStart.X - m_oPointEnd.X); }
        }
        public int nDeltaY
        {
            get { return Math.Abs(m_oPointStart.Y - m_oPointEnd.Y); }
        }
        public double dLength
        {
            get { return Math.Sqrt(nDeltaX * nDeltaX + nDeltaY * nDeltaY); }
        }

        public BorderPartLine(Point oPointStart, Point oPointEnd)
        {
            m_oPointStart = oPointStart;
            m_oPointEnd = oPointEnd;

            m_nSize = 2;
            m_nPartLength[0] = 2;

            m_oDirection = new Point(m_oPointEnd.X - m_oPointStart.X, m_oPointEnd.Y - m_oPointStart.Y);
        }

        public bool CombineSameLine1(BorderPartLine oLine)
        {
            if (m_oDirection != oLine.m_oDirection)
            {
                return false;
            }
            m_nSize += oLine.m_nSize - 1;
            m_nPartLength[0] = m_nSize;
            if (m_oPointEnd == oLine.m_oPointStart)
            {
                m_oPointEnd = oLine.m_oPointEnd;
            }
            else
            {
                m_oPointStart = oLine.m_oPointStart;
            }
            return true;
        }

        public bool CombineLine(BorderPartLine oLine)
        {
            if (m_oPointEnd == oLine.m_oPointStart)
            {
                m_oPointEnd = oLine.m_oPointEnd;
            }
            else if (m_oPointStart == oLine.m_oPointEnd)
            {
                m_oPointStart = oLine.m_oPointStart;
            }
            else
            {
                return false;
            }
            m_nSize += oLine.m_nSize - 1;
            CalcLine();
            return true;
        }

        public BorderPartLine SplitLine(Point oPointSplit1, Point oPointSplit2)
        {
            m_nSize = 2;

            BorderPartLine oMyLine = new BorderPartLine(oPointSplit2, m_oPointEnd);

            m_oPointEnd = oPointSplit1;

            oMyLine.CalcLine();
            CalcLine();
            return oMyLine;
        }

        public bool CombineSameLinexx(BorderPartLine oLine1, BorderPartLine oLine2)
        {
            m_nSize += oLine1.m_nSize + oLine2.m_nSize - 2;
            if (m_oPointEnd == oLine1.m_oPointStart)
            {
                m_oPointEnd = oLine2.m_oPointEnd;
            }
            else if (m_oPointStart == oLine1.m_oPointEnd)
            {
                m_oPointStart = oLine2.m_oPointStart;
            }
            else
            {
                return false;
            }
            CalcLine();
            return true;
        }

        public bool CombineSameLine(BorderPartLine oLine1, BorderPartLine oLine2)
        {
            if ((oLine1.m_nSize != 2) || (m_oDirection != oLine2.m_oDirection))
            {
                return false;
            }
            if (!m_oDirectionConnect.IsEmpty && (m_oDirectionConnect != oLine1.m_oDirection))
            {
                return false;
            }
            if (!oLine2.m_oDirectionConnect.IsEmpty && (oLine2.m_oDirectionConnect != oLine1.m_oDirection))
            {
                return false;
            }
            if ((m_oPointEnd != oLine1.m_oPointStart && m_oPointStart != oLine1.m_oPointEnd) || (oLine2.m_oPointEnd != oLine1.m_oPointStart && oLine2.m_oPointStart != oLine1.m_oPointEnd))
            {
                return false;
            }

            if (m_nPartLength[1] != 0)
            {
                if (oLine2.m_nPartLength[1] != 0)
                {
                    if (m_nPartLength[0] != oLine2.m_nPartLength[0] || m_nPartLength[1] != oLine2.m_nPartLength[1])
                    {
                        return false;
                    }
                }
                else
                {
                    if (oLine2.m_nPartLength[0] != m_nPartLength[0] && oLine2.m_nPartLength[0] != m_nPartLength[1])
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (oLine2.m_nPartLength[1] != 0)
                {
                    if (m_nPartLength[0] != oLine2.m_nPartLength[0] && m_nPartLength[0] != oLine2.m_nPartLength[1])
                    {
                        return false;
                    }
                    m_nPartLength[0] = oLine2.m_nPartLength[0];
                    m_nPartLength[1] = oLine2.m_nPartLength[1];
                }
                else
                {
                    if (Math.Abs(m_nPartLength[0] - oLine2.m_nPartLength[0]) > 1)
                    {
                        return false;
                    }
                    if (m_nPartLength[0] != oLine2.m_nPartLength[0])
                    {
                        m_nPartLength[1] = oLine2.m_nPartLength[0];
                        SortLength();
                    }
                    m_oDirectionConnect = oLine1.m_oDirection;
                }
            }

            m_nSize += oLine2.m_nSize;
            if (m_oPointEnd == oLine1.m_oPointStart)
            {
                m_oPointEnd = oLine2.m_oPointEnd;
            }
            else
            {
                m_oPointStart = oLine2.m_oPointStart;
            }
            CalcLine();
            return true;
        }

        public bool CombineSameAlpha(BorderPartLine oLine1, double dPixel)
        {
            double dAlphaDiffMax;

            //dAlphaDiffMax = (100.0 / dLength) + (100.0 / oLine1.dLength);
            dAlphaDiffMax = (Math.Atan(dPixel / dLength) + Math.Atan(dPixel / oLine1.dLength)) * 180 / Math.PI;
            //dAlphaDiffMax = (Math.Atan(1.1 / dLength) + Math.Atan(1.1 / oLine1.dLength)) * 180 / Math.PI;

            if (Math.Abs(CheckAlpha(m_dAlpha - oLine1.m_dAlpha)) >= dAlphaDiffMax)
            {
                return false;
            }
            if (m_oPointEnd == oLine1.m_oPointStart)
            {
                m_oPointEnd = oLine1.m_oPointEnd;
            }
            else if (m_oPointStart == oLine1.m_oPointEnd)
            {
                m_oPointStart = oLine1.m_oPointStart;
            }
            else
            {
                return false;
            }
            m_nSize += oLine1.m_nSize;
            CalcLine();
            return true;
        }

        private void SortLength()
        {
            if (m_nPartLength[0] > m_nPartLength[1])
            {
                int nTemp = m_nPartLength[0];
                m_nPartLength[0] = m_nPartLength[1];
                m_nPartLength[1] = nTemp;
            }
        }

        public void CalcLine()
        {
            m_dm = ((double)(m_oPointEnd.Y - m_oPointStart.Y)) / (m_oPointEnd.X - m_oPointStart.X);
            m_dc = (double)m_oPointStart.Y - (m_dm * m_oPointStart.X);

            if (double.IsInfinity(m_dm) || double.IsNaN(m_dm))
            {
                this.m_dAlpha = CheckAlpha(m_oPointStart.Y > m_oPointEnd.Y ? -90 : 90);
            }
            else
            {
                this.m_dAlpha = CheckAlpha(Math.Atan(((double)(m_oPointEnd.Y - m_oPointStart.Y)) / (m_oPointEnd.X - m_oPointStart.X)) * 180 / Math.PI + (m_oPointStart.X > m_oPointEnd.X ? 180 : 0));
            }
        }

        public double CheckAlpha(double dAlpha)
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

        //public bool IsSameDirection(BorderPartLine oLine1, BorderPartLine oLine2)
        //{
        //    return Math.Abs(oLine1.m_oDirection.X - oLine2.m_oDirection.X) + Math.Abs(oLine1.m_oDirection.Y - oLine2.m_oDirection.Y) <= 1;
        //}

        public double GetDiffAlpha(BorderPartLine oLine1)
        {
            return CheckAlpha(m_dAlpha - oLine1.m_dAlpha);
        }

        public double GetDiffAlphaAbs(BorderPartLine oLine1)
        {
            return Math.Abs(CheckAlpha(m_dAlpha - oLine1.m_dAlpha));
        }

        public bool IsSameAlpha(BorderPartLine oLine1)
        {
            double dAlphaDiffMax;

            //dAlphaDiffMax = (60.0 / dLength) + (60.0 / oLine1.dLength);
            dAlphaDiffMax = (Math.Atan(1.1 / dLength) + Math.Atan(1.1 / oLine1.dLength)) * 180 / Math.PI;

            return GetDiffAlphaAbs(oLine1) <= dAlphaDiffMax;
        }

        public bool IsSameAlpha(BorderPartLine oLine1, double dPixel)
        {
            double dAlphaDiffMax;

            //dAlphaDiffMax = (60.0 / dLength) + (60.0 / oLine1.dLength);
            dAlphaDiffMax = (Math.Atan(dPixel / dLength) + Math.Atan(dPixel / oLine1.dLength)) * 180 / Math.PI;

            return GetDiffAlphaAbs(oLine1) <= dAlphaDiffMax;
        }


        public override BorderPart ShallowCopy()
        {
            return (BorderPart)this.MemberwiseClone();
        }

        public override String GetTypName()
        {
            return "Line";
        }

        public override Point[] GetPoints()
        {
            Point[] aoPoints = new Point[2];

            aoPoints[0] = m_oPointStart;
            aoPoints[1] = m_oPointEnd;
            return aoPoints;
        }

        public override Point GetCenterPoint()
        {
            return new Point((m_oPointStart.X + m_oPointEnd.X) / 2, (m_oPointStart.Y + m_oPointEnd.Y) / 2);
        }

        public override string ToString()
        {
            string strStart = string.Format(CultureInfo.CurrentCulture, "P1.X={0:D}, P1.Y={1:D}, ", m_oPointStart.X, m_oPointStart.Y);
            string strEnd = string.Format(CultureInfo.CurrentCulture, "P2.X={0:D}, P2.Y={1:D}", m_oPointEnd.X, m_oPointEnd.Y);
            return strStart + strEnd;
        }

    }

}
