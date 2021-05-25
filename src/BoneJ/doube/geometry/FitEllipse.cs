/**
* FitEllipse Copyright 2009 2010 Michael Doube
* Ported from org.doube.geometry.Centroid in the BoneJ project http://bonej.org/
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using gov.nist.math.jama;
using Tracer2Server.Trace.Fit;

/**
* Ellipse-fitting methods.
*
* @author Michael Doube
*
*/
namespace BoneJ.doube.geometry
{
    class FitEllipse
    {
        /**
          * Java port of Chernov's MATLAB implementation of the direct ellipse fit
          *
          * @param points
          * n * 2 array of 2D coordinates.
          * @return <p>
          * 6-element array, {a b c d f g}, which are the algebraic
          * parameters of the fitting ellipse: <i>ax</i><sup>2</sup> +
          * 2<i>bxy</i> + <i>cy</i><sup>2</sup> +2<i>dx</i> + 2<i>fy</i> +
          * <i>g</i> = 0. The vector <b>A</b> represented in the array is
          * normed, so that ||<b>A</b>||=1.
          * </p>
          *
          * @see <p>
          * <a href="http://www.mathworks.co.uk/matlabcentral/fileexchange/22684-ellipse-fit-direct-method"
          * >MATLAB script</a>
          * </p>
          */
        public static List<Ellipse> direct(List<Point> listPoints)
        {
            List<Ellipse> listEllipse = new List<Ellipse>();
            int nPoints = listPoints.Count;
            double[][] d1 = Matrix.CreateArray(nPoints, 3);
            for (int i = 0; i < nPoints; i++)
            {
                Point p = listPoints[i];
                double xixC = listPoints[i].X;
                double yiyC = listPoints[i].Y;
                d1[i][0] = xixC * xixC;
                d1[i][1] = xixC * yiyC;
                d1[i][2] = yiyC * yiyC;
            }
            Matrix D1 = new Matrix(d1);
            double[][] d2 = Matrix.CreateArray(nPoints, 3);
            for (int i = 0; i < nPoints; i++)
            {
                Point p = listPoints[i];

                d2[i][0] = p.X;
                d2[i][1] = p.Y;
                d2[i][2] = 1;
            }
            Matrix D2 = new Matrix(d2);

            Matrix S1 = D1.transpose().times(D1);

            Matrix S2 = D1.transpose().times(D2);

            Matrix S3 = D2.transpose().times(D2);

            Matrix T = (S3.inverse().times(-1)).times(S2.transpose());

            Matrix M = S1.plus(S2.times(T));

            double[][] m = M.getArray();
            double[][] n = new double[3][] { 
                new double[] {   m[2][0] / 2,   m[2][1] / 2,     m[2][2] / 2    },
                new double[] {   -m[1][0],      -m[1][1],        -m[1][2]       },
                new double[] {   m[0][0] / 2,   m[0][1] / 2,     m[0][2] / 2    }
            };

            Matrix N = new Matrix(n);

            EigenvalueDecomposition E = N.eig();

            // scaling
            Matrix eVec = E.getV();
            double[][] adVec = eVec.getArray();
            for (int h = 0; h < 3; h++)
            {
                double k = Math.Abs(adVec[2][h]);
                adVec[0][h] = adVec[0][h] / k;
                adVec[1][h] = adVec[1][h] / k;
                adVec[2][h] = adVec[2][h] / k;
            }
            eVec = new Matrix(adVec);

            Matrix R1 = eVec.getMatrix(0, 0, 0, 2);
            Matrix R2 = eVec.getMatrix(1, 1, 0, 2);
            Matrix R3 = eVec.getMatrix(2, 2, 0, 2);

            Matrix cond = (R1.times(4)).arrayTimes(R3).minus(R2.arrayTimes(R2));

            for (int i = 0; i < 3; i++)
            {
                double ff = cond.get(0, i);
                if (cond.get(0, i) > 0)
                {
                    Matrix A1 = eVec.getMatrix(0, 2, i, i);

                    Matrix A = new Matrix(6, 1);
                    A.setMatrix(0, 2, 0, 0, A1);
                    A.setMatrix(3, 5, 0, 0, T.times(A1));

                    double[] a = A.getColumnPackedCopy();
                    listEllipse.Add(new Ellipse(a));
                    listEllipse.Add(new Ellipse(a, true));

                    //double a4 = a[3] - 2 * a[0] * xC - a[1] * yC;
                    //double a5 = a[4] - 2 * a[2] * yC - a[1] * xC;
                    //double a6 = a[5] + a[0] * xC * xC + a[2] * yC * yC + a[1] * xC * yC
                    //                - a[3] * xC - a[4] * yC;
                    //A.set(3, 0, a4);
                    //A.set(4, 0, a5);
                    //A.set(5, 0, a6);
                    //A = A.times(1 / A.normF());
                }
            }
            return listEllipse;
        }

        /**
         * Create an array of (x,y) coordinates on an ellipse of radii (a,b) and
         * rotated r radians. Random noise is added if noise > 0.
         *
         * @param a
         * One semi-axis length
         * @param b
         * The other semi-axis length
         * @param r
         * Angle of rotation
         * @param c
         * centroid x
         * @param d
         * centroid y
         * @param noise
         * intensity of random noise
         * @param n
         * Number of points
         * @return
         */
        public static double[][] testEllipse(double a, double b, double r, double c, double d, double noise, int n)
        {
            Random ran = new Random();
            double[][] points = Matrix.CreateArray(n, 2);
            double increment = 2 * Math.PI / n;
            double alpha = 0;
            for (int i = 0; i < n; i++)
            {
                points[i][0] = a * Math.Cos(alpha) + ran.NextDouble() * noise;
                points[i][1] = b * Math.Sin(alpha) + ran.NextDouble() * noise;
                alpha += increment;
            }
            double sinR = Math.Sin(r);
            double cosR = Math.Cos(r);
            for (int i = 0; i < n; i++)
            {
                double x = points[i][0];
                double y = points[i][1];
                points[i][0] = x * cosR - y * sinR + c;
                points[i][1] = x * sinR + y * cosR + d;
            }
            return points;
        }

        /**
         * <p>
         * Convert variables a, b, c, d, f, g from the general ellipse equation ax²
         * + bxy + cy² +dx + fy + g = 0 into useful geometric parameters semi-axis
         * lengths, centre and angle of rotation.
         * </p>
         *
         * @see <p>
         * Eq. 19-23 at <a
         * href="http://mathworld.wolfram.com/Ellipse.html">Wolfram Mathworld
         * Ellipse</a>.
         * </p>
         *
         * @param ellipse
         * <p>
         * array containing a, b, c, d, f, g of the ellipse equation.
         * </p>
         * @return <p>
         * array containing centroid coordinates, axis lengths and angle of
         * rotation of the ellipse specified by the input variables.
         * </p>
         */
        public static double[] varToDimensions(double[] ellipse)
        {
            double a = ellipse[0];
            double b = ellipse[1] / 2;
            double c = ellipse[2];
            double d = ellipse[3] / 2;
            double f = ellipse[4] / 2;
            double g = ellipse[5];

            // centre
            double cX = (c * d - b * f) / (b * b - a * c);
            double cY = (a * f - b * d) / (b * b - a * c);

            // semiaxis length
            double af = 2 * (a * f * f + c * d * d + g * b * b - 2 * b * d * f - a * c * g);

            double aL = Math.Sqrt((af) / ((b * b - a * c) * (Math.Sqrt((a - c) * (a - c) + 4 * b * b) - (a + c))));

            double bL = Math.Sqrt((af) / ((b * b - a * c) * (-Math.Sqrt((a - c) * (a - c) + 4 * b * b) - (a + c))));
            double phi = 0;
            if (b == 0)
            {
                if (a <= c)
                    phi = 0;
                else if (a > c)
                    phi = Math.PI / 2;
            }
            else
            {
                if (a < c)
                    phi = Math.Atan(2 * b / (a - c)) / 2;
                else if (a > c)
                    phi = Math.Atan(2 * b / (a - c)) / 2 + Math.PI / 2;
            }
            double[] dimensions = { cX, cY, aL, bL, phi };
            return dimensions;
        }

    }

}
