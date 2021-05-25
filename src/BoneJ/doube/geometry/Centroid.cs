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

namespace BoneJ.doube.geometry
{
    class Centroid
    {
        /**
         * Find the centroid of an array in double[n][i] format, where n = number of
         * points and i = number of dimensions
         *
         * @param points
         * @return array containing centroid in i dimensions
         */
        public static double[] getCentroid(double[][] points)
        {
            int nDimensions = points[0].Length;

            switch (nDimensions)
            {
                case 1:
                    return getCentroid1D(points);
                case 2:
                    return getCentroid2D(points);
                case 3:
                    return getCentroid3D(points);
                default:
                    return getCentroidND(points);
            }
        }

        /**
         * Find the centroid of a set of points in double[n][1] format
         *
         * @param points
         * @return
         */
        private static double[] getCentroid1D(double[][] points)
        {
            double[] centroid = new double[1];
            double sumX = 0;
            int nPoints = points.Length;

            for (int n = 0; n < nPoints; n++)
            {
                sumX += points[n][0];
            }

            centroid[0] = sumX / nPoints;

            return centroid;
        }

        /**
         * Find the centroid of a set of points in double[n][2] format
         *
         * @param points
         * @return
         */
        private static double[] getCentroid2D(double[][] points)
        {
            double[] centroid = new double[2];
            double sumX = 0;
            double sumY = 0;
            int nPoints = points.Length;

            for (int n = 0; n < nPoints; n++)
            {
                sumX += points[n][0];
                sumY += points[n][1];
            }

            centroid[0] = sumX / nPoints;
            centroid[1] = sumY / nPoints;

            return centroid;
        }

        /**
         * Find the centroid of a set of points in double[n][3] format
         *
         * @param points
         * @return
         */
        private static double[] getCentroid3D(double[][] points)
        {
            double[] centroid = new double[3];
            double sumX = 0;
            double sumY = 0;
            double sumZ = 0;
            int nPoints = points.Length;

            for (int n = 0; n < nPoints; n++)
            {
                sumX += points[n][0];
                sumY += points[n][1];
                sumZ += points[n][2];
            }

            centroid[0] = sumX / nPoints;
            centroid[1] = sumY / nPoints;
            centroid[2] = sumZ / nPoints;

            return centroid;
        }

        /**
         * Find the centroid of a set of points in double[n][i] format
         *
         * @param points
         * @return
         */
        private static double[] getCentroidND(double[][] points)
        {
            int nPoints = points.Length;
            int nDimensions = points[0].Length;
            double[] centroid = new double[nDimensions];
            double[] sums = new double[nDimensions];

            for (int n = 0; n < nPoints; n++)
            {
                if (points[n].Length != nDimensions)
                    throw new ArgumentException("Number of dimensions must be equal");
                for (int i = 0; i < nDimensions; i++)
                {
                    sums[i] += points[n][i];
                }
            }

            for (int i = 0; i < nDimensions; i++)
            {
                centroid[i] = sums[i] / nPoints;
            }

            return centroid;
        }

        /**
         * Return the centroid of a 1D array, which is its mean value
         *
         * @param points
         * @return the mean value of the points
         */
        public static double getCentroid(double[] points)
        {
            int nPoints = points.Length;
            double sum = 0;
            for (int n = 0; n < nPoints; n++)
            {
                sum += points[n];
            }
            return sum / nPoints;
        }

        /**
         * Calculate the centroid of a list of 3D Points
         * @param points
         * @return
         */
        //public static double[] getCentroid(List<Point> points) {
        //        double xsum = 0;
        //        double ysum = 0;
        //        double zsum = 0;
        //        double n = points.Count;

        //        for (Point p : points) {
        //                xsum += p.x;
        //                ysum += p.y;
        //                zsum += p.z;
        //        }
        //        double[] centroid = { xsum / n, ysum / n, zsum / n };
        //        return centroid;
        //}
    }
}
