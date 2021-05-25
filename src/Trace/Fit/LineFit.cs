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

namespace Tracer2Server.Trace.Fit
{
    class LineFit
    {
        public double m_dm = double.NaN;
        public double m_dc = double.NaN;
        public bool m_bRevert;

        public void Fit(List<Point> listPoints)
        {
            long nSX = 0;
            long nSX2 = 0;
            long nSY = 0;
            long nSY2 = 0;
            long nSXY = 0;
            int n;

            double dSX = 0;
            double dSX2 = 0;
            double dSY = 0;
            double dSY2 = 0;
            double dSXY = 0;

            n = 0;
            foreach (Point oPoint in listPoints)
            {
                nSX += oPoint.X;
                nSX2 += ((long)oPoint.X) * oPoint.X;
                nSY += oPoint.Y;
                nSY2 += ((long)oPoint.Y) * oPoint.Y;
                nSXY += ((long)oPoint.X) * oPoint.Y;
                n++;
            }

            dSX = (double)nSX;
            dSX2 = (double)nSX2;
            dSY = (double)nSY;
            dSY2 = (double)nSY2;
            dSXY = (double)nSXY;
            try
            {
                Point oPointStart = listPoints[0];
                Point oPointEnd = listPoints[listPoints.Count - 1];

                m_dc = ((dSY * dSX2) - (dSXY * dSX)) / ((n * dSX2) - (dSX * dSX));

                m_dm = ((dSXY * n) - (dSX * dSY)) / ((dSX2 * n) - (dSX * dSX));

                if (Math.Abs(2 * (oPointStart.X - oPointEnd.X)) > Math.Abs(oPointStart.Y - oPointEnd.Y))
                {
                    m_bRevert = oPointStart.X > oPointEnd.X;
                }
                else
                {
                    m_bRevert = (oPointStart.X < oPointEnd.X) ^ (m_dm > 0);
                }
            }
            catch
            {
                m_dm = double.NaN;
                m_dc = double.NaN;
                m_bRevert = false;
            }
        }

    }

}
