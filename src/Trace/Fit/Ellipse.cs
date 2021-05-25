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
using gov.nist.math.jama;

namespace Tracer2Server.Trace.Fit
{
    public class Ellipse
    {
        public double[] m_adResult;

        // Ax^2 + Bxy + Cy^2 + Dx + Ey + F = 0
        public double m_dA = Double.NaN;
        public double m_dB = Double.NaN;
        public double m_dC = Double.NaN;
        public double m_dD = Double.NaN;
        public double m_dE = Double.NaN;
        public double m_dF = Double.NaN;

        public PointD m_oEllipseCenter;
        public double m_da = Double.NaN;
        public double m_db = Double.NaN;
        public double m_dThetaDeg = Double.NaN;
        public double m_dThetaRad = Double.NaN;

        public Ellipse(double[] adResult, bool bSwap = false)
        {
            m_adResult = adResult;

            // ellipse coefficients
            m_dA = adResult[0];
            m_dB = adResult[1];
            m_dC = adResult[2];
            m_dD = adResult[3];
            m_dE = adResult[4];
            m_dF = adResult[5];

            double h = (2 * m_dC * m_dD - m_dE * m_dB) / (m_dB * m_dB - 4 * m_dA * m_dC);
            double k = (-2 * m_dA * h - m_dD) / (m_dB);

            m_oEllipseCenter = new PointD(h, k);

            double[,] oM0 = new double[3,3];
            double[,] oM = new double[2,2];

            oM0[0, 0] = m_dF;
            oM0[1, 0] = m_dD / 2;
            oM0[2, 0] = m_dE / 2;
            oM0[0, 1] = m_dD / 2;
            oM0[1, 1] = m_dA;
            oM0[2, 1] = m_dB / 2;
            oM0[0, 2] = m_dE / 2;
            oM0[1, 2] = m_dB / 2;
            oM0[2, 2] = m_dC;
            double dM0det = GetDet(oM0);

            oM[0, 0] = m_dA;
            oM[1, 0] = m_dB / 2;
            oM[0, 1] = m_dB / 2;
            oM[1, 1] = m_dC;
            double dMdet = GetDet(oM);

            double[][] n = new double[2][] { 
                new double[] {   m_dA,      m_dB / 2    },
                new double[] {   m_dB / 2,  m_dC        }
            };
            Matrix N = new Matrix(n);
            EigenvalueDecomposition E = N.eig();

            double[] d = E.getRealEigenvalues();
            //if (Math.Abs(d[0] - m_dA) > Math.Abs(d[1] - m_dC))
            if (bSwap)
            {
                double oX = d[0];
                d[0] = d[1];
                d[1] = oX;
            }

            m_da = Math.Sqrt(-dM0det / (dMdet * d[0]));
            m_db = Math.Sqrt(-dM0det / (dMdet * d[1]));
            m_dThetaRad = Math.Atan(m_dB / (m_dA - m_dC)) / 2;
            m_dThetaDeg = m_dThetaRad * 180 / Math.PI;
        }

        public void MoveCenter(PointD oPoint)
        {
            m_oEllipseCenter += oPoint;
        }

        public void MoveCenter(Point oPoint)
        {
            m_oEllipseCenter += oPoint;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "A={0}, B={1}, C={2}, D={3}, E={4}, F={5}, a={6}, b={7}, t={8} " + m_oEllipseCenter.ToString(), m_dA, m_dB, m_dC, m_dD, m_dE, m_dF, m_da, m_db, m_dThetaDeg);
        }

        private double GetDet(double[,] oMatrix)
        {
            int nDim = oMatrix.GetLength(0);

            if (nDim == 1)
            {
                return oMatrix[0, 0];
            }
            else if (nDim == 2)
            {
                return oMatrix[0, 0] * oMatrix[1, 1] - oMatrix[0, 1] * oMatrix[1, 0];
            }
            else if (nDim == 3)
            {
                double dRet;
                dRet = oMatrix[0, 0] * oMatrix[1, 1] * oMatrix[2, 2] + oMatrix[0, 1] * oMatrix[1, 2] * oMatrix[2, 0] + oMatrix[0, 2] * oMatrix[1, 0] * oMatrix[2, 1];
                dRet -= oMatrix[0, 2] * oMatrix[1, 1] * oMatrix[2, 0] + oMatrix[0, 1] * oMatrix[1, 0] * oMatrix[2, 2] + oMatrix[0, 0] * oMatrix[1, 2] * oMatrix[2, 1];
                return dRet;
            }
            return 0;
        }

    }

}
