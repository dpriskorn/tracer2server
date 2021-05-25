using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using BoneJ.doube.geometry;

namespace Tracer2Server.Trace.Fit
{
    class EllipseFit
    {
        public List<Ellipse> m_listEllipse = new List<Ellipse>();
        public Point m_oPointCenter;

        public void Fit(List<Point> listPoints)
        {
            if (listPoints == null || listPoints.Count < 10)
            {
                return;
            }
            CalcCenter(listPoints);

            FitCenter(listPoints, m_oPointCenter);
            CalcEllipse(listPoints, new Point(0,0));
        }

        public void FitCenter(List<Point> listPoints, Point oPointCenter)
        {
            List<Point> listPointsX = new List<Point>();

            listPointsX = new List<Point>();
            foreach (Point oP in listPoints)
            {
                listPointsX.Add(new Point(oP.X - oPointCenter.X, oP.Y - oPointCenter.Y));
            }
            CalcEllipse(listPointsX, oPointCenter);
        }

        private void CalcEllipse(List<Point> listPoints, Point oPointCenter)
        {
            EllipseFit oEllipseFit = new EllipseFit();

            List<Ellipse> listFE = FitEllipse.direct(listPoints);

            foreach (Ellipse oEllipse in listFE)
            {
                oEllipse.MoveCenter(oPointCenter);
                m_listEllipse.Add(oEllipse);
            }
        }

        private void CalcCenter(List<Point> listPoints)
        {
            int nSX = 0;
            int nSY = 0;

            int nPoints = listPoints.Count;

            foreach (Point oP in listPoints)
            {
                nSX += oP.X;
                nSY += oP.Y;
            }
            m_oPointCenter = new Point(nSX / nPoints, nSY / nPoints);
        }
    }

}
