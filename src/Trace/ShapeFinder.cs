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
using Tracer2Server.Tiles;
using Tracer2Server.Debug;
using Tracer2Server.WebServer;
using Tracer2Server.Trace.Fit;

namespace Tracer2Server.Trace
{
    class ShapeFinder
    {
        const byte AREA = 0;
        const byte BORDER = 1;

        private List<Point> m_listBorderPoints;
        private LinkedList<BorderPart> m_listBorderLines;

        private TracerParam m_oTracerParam;

        public ShapeFinder(TracerParam oTracerParam)
        {
            m_oTracerParam = oTracerParam;
        }

        public void GetStartPoint()
        {
            int nPosX = m_oTracerParam.m_oInnerPointXY.X;
            int nPosY = m_oTracerParam.m_oInnerPointXY.Y;

            if (m_oTracerParam.m_oGeoMap.GetPix(nPosX, nPosY) == BORDER)
            {
                throw new Exception("Can't find sarting point");
            }
            do
            {
                nPosY += 1;
            }
            while (m_oTracerParam.m_oGeoMap.GetPix(nPosX, nPosY) == AREA);
            Point pPointStart = new Point(nPosX, nPosY);

            LinkedList<BorderPart> listBorderLines = new LinkedList<BorderPart>();
            listBorderLines.AddLast(new BorderPartLine(m_oTracerParam.m_oInnerPointXY, pPointStart));
            m_oTracerParam.SetBorderLine(listBorderLines.ToList(), "Find starting point");

            m_oTracerParam.SetStartPoint(pPointStart);
        }

        public void GetBorderPoints()
        {
            m_listBorderPoints = new List<Point>(60000);

            GetPoints(1);

            m_oTracerParam.SetBorderPoints(m_listBorderPoints);

            m_oTracerParam.m_oShapeAreaXY = GetShapeArea();

            DebugStatic.oDebugBitmap.oGeoMap = m_oTracerParam.m_oGeoMap;
            DebugStatic.oDebugBitmap.MakeBitmap(m_oTracerParam.m_oInnerPointXY, m_oTracerParam.m_oShapeAreaXY);
        }

        private Rectangle GetShapeArea()
        {
            int xmin = m_listBorderPoints[0].X;
            int xmax = m_listBorderPoints[0].X;
            int ymin = m_listBorderPoints[0].Y;
            int ymax = m_listBorderPoints[0].Y;

            foreach (Point oP in m_listBorderPoints)
            {
                if (xmin > oP.X)
                {
                    xmin = oP.X;
                }
                else if (xmax < oP.X)
                {
                    xmax = oP.X;
                }
                if (ymin > oP.Y)
                {
                    ymin = oP.Y;
                }
                else if (ymax < oP.Y)
                {
                    ymax = oP.Y;
                }
            }
            return new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        private void GetPoints(int nRotation)
        {
            int nDirection = (int)DIRECTION_XY.DOWN;
            int nStartPosX = m_oTracerParam.m_oStartPointXY.X;
            int nStartPosY = m_oTracerParam.m_oStartPointXY.Y;
            int nPosX = nStartPosX;
            int nPosY = nStartPosY;
            byte ucData;
            int nCount;
            do
            {
                m_listBorderPoints.Add(new Point(nPosX, nPosY));
                nCount = 9;
                do
                {
                    if (nCount-- == 0)
                    {
                        return;
                    }
                    nDirection = (nDirection + nRotation) & 0x7;
                    ucData = m_oTracerParam.m_oGeoMap.GetPix(nPosX + MyDirection.m_aoDirX[nDirection], nPosY + MyDirection.m_aoDirY[nDirection]);
                } while (ucData == AREA);
                nPosX += MyDirection.m_aoDirX[nDirection];
                nPosY += MyDirection.m_aoDirY[nDirection];
                nDirection = (nDirection - (3 * nRotation)) & 0x7;
            } while (m_listBorderPoints.Count() < 100000 && ((nPosX != nStartPosX) || (nPosY != nStartPosY)));
        }

        public void GetBorderLine()
        {
            string strStep;
            int nTurns;

            BorderPartLine oLine;
            BorderPartLine oLineNew;
            Point oPointStart;
            LinkedListNode<BorderPart> oLineNodeStart;
            LinkedListNode<BorderPart> oLineNode;
            LinkedListNode<BorderPart> oLineNode1;
            LinkedListNode<BorderPart> oLineNode2;
            LinkedListNode<BorderPart> oLineNodeBack1;
            LinkedListNode<BorderPart> oLineNodeBack2;

            m_listBorderLines = new LinkedList<BorderPart>();

            strStep = "Combine points with same direction";
            DebugStatic.oDebugWatch.StartWatch(strStep);
            Point oPointEnd;
            oPointStart = m_listBorderPoints[m_listBorderPoints.Count() - 1];
            oPointEnd = m_listBorderPoints[0];
            oLine = new BorderPartLine(oPointStart, oPointEnd);
            oPointStart = oPointEnd;
            nTurns = m_listBorderPoints.Count();
            for (int i = 1; i < nTurns; i++)
            {
                oPointEnd = m_listBorderPoints[i];
                oLineNew = new BorderPartLine(oPointStart, oPointEnd);
                if (oLine.CombineSameLine1(oLineNew) == false)
                {
                    m_listBorderLines.AddLast(oLine);
                    oLine = oLineNew;
                }
                oPointStart = oPointEnd;
            }
            if (((BorderPartLine)m_listBorderLines.First.Value).CombineSameLine1(oLine) == false)
            {
                m_listBorderLines.AddLast(oLine);
            }
            DebugStatic.oDebugWatch.StopWatch(strStep, m_listBorderLines.Count().ToString());
            DebugStatic.oDebugBitmap.MakeBitmap(strStep, m_listBorderLines.ToArray());
            m_oTracerParam.SetBorderLine(m_listBorderLines.ToList(), strStep);

            if (m_listBorderLines.Count < 3)
            {
                // no area
                m_oTracerParam.m_strError = "No area found.";
                m_listBorderLines = new LinkedList<BorderPart>();
                return;
            }

            // calc line
            foreach (BorderPart oBorderPart in m_listBorderLines)
            {
                ((BorderPartLine)oBorderPart).CalcLine();
            }

            //strStep = "Remove corner line (size=2)";
            //DebugStatic.oDebugFile.StartWatch(strStep);
            //oLineNode = m_listBorderLines.First;
            //nTurns = m_listBorderLines.Count();
            //for (int i = 0; i < nTurns * 2; i++)
            //{
            //    if (((BorderPartLine)oLineNode.Value).m_nSize == 2)
            //    {
            //        oLineNode1 = GetNextNode(oLineNode);
            //        oLineNodeBack1 = GetPreviousNode(oLineNode);
            //        double dA1 = ((BorderPartLine)oLineNode.Value).GetDiffAlphaAbs((BorderPartLine)oLineNode1.Value);
            //        double dA2 = ((BorderPartLine)oLineNode.Value).GetDiffAlphaAbs((BorderPartLine)oLineNodeBack1.Value);
            //        double dA3 = ((BorderPartLine)oLineNode1.Value).GetDiffAlphaAbs((BorderPartLine)oLineNodeBack1.Value);
            //        if (/*((BorderPartLine)oLineNode1.Value).m_nSize > 2 && ((BorderPartLine)oLineNodeBack1.Value).m_nSize > 2 &&*/ dA1 > 40 && dA1 < 50 && dA2 > 40 && dA2 < 50 && dA3 > 80)
            //        {
            //            m_listBorderLines.Remove(oLineNode.Value);
            //            oLineNode = oLineNodeBack1;
            //        }
            //    }
            //    oLineNode = GetNextNode(oLineNode);
            //}
            //DebugStatic.oDebugFile.StopWatch(strStep, m_listBorderLines.Count().ToString());
            //DebugStatic.oDebugBitmap.MakeBitmap(strStep, m_listBorderLines.ToArray());
            //m_oTracerParam.SetBorderLine(m_listBorderLines.ToList(), strStep);

            strStep = "Split corner line";
            DebugStatic.oDebugWatch.StartWatch(strStep);
            oLineNode = m_listBorderLines.First;
            nTurns = m_listBorderLines.Count();
            for (int i = 0; i < nTurns * 2; i++)
            {
                if (((BorderPartLine)oLineNode.Value).m_nSize > 2)
                {
                    oLineNode1 = GetNextNode(oLineNode);
                    oLineNode2 = GetNextNode(oLineNode1);
                    oLineNodeBack1 = GetPreviousNode(oLineNode);
                    oLineNodeBack2 = GetPreviousNode(oLineNodeBack1);

                    if (((BorderPartLine)oLineNode1.Value).m_nSize == 2 && ((BorderPartLine)oLineNodeBack1.Value).m_nSize == 2 && ((BorderPartLine)oLineNode2.Value).m_nSize > 2 && ((BorderPartLine)oLineNodeBack2.Value).m_nSize > 2)
                    {
                        double dA1 = ((BorderPartLine)oLineNode.Value).GetDiffAlphaAbs((BorderPartLine)oLineNode2.Value);
                        double dA2 = ((BorderPartLine)oLineNode.Value).GetDiffAlphaAbs((BorderPartLine)oLineNodeBack2.Value);
                        double dA3 = ((BorderPartLine)oLineNode1.Value).GetDiffAlphaAbs((BorderPartLine)oLineNodeBack1.Value);
                        if (dA1 < 1 && dA2 < 1 && dA3 > 80 && dA3 < 100)
                        {
                            int nLength1 = ((BorderPartLine)oLineNode2.Value).m_nSize;
                            int nLength2 = ((BorderPartLine)oLineNodeBack2.Value).m_nSize;
                            int nPos1 = m_listBorderPoints.IndexOf(((BorderPartLine)oLineNode.Value).m_oPointStart);
                            int nPos2 = m_listBorderPoints.IndexOf(((BorderPartLine)oLineNode.Value).m_oPointEnd);
                            //int nCount = 1 + (int)( (double)((((BorderPartLine)oLineNode.Value).m_nSize - 3) * nPos1 / (nPos1 + nPos2)));

                            //nPos1 = nPos1 + nCount;
                            nPos1++;
                            if (nPos1 >= m_listBorderPoints.Count)
                            {
                                nPos1 -= m_listBorderPoints.Count;
                            }
                            //nPos2 = nPos1 + 1;
                            nPos2--;
                            //if (nPos2 >= m_listBorderPoints.Count)
                            //{
                            //    nPos2 -= m_listBorderPoints.Count;
                            //}
                            if (nPos2 < 0)
                            {
                                nPos2 += m_listBorderPoints.Count;
                            }

                            // m_listBorderLines.Remove(oLineNode.Value);
                            m_listBorderLines.AddAfter(oLineNode, ((BorderPartLine)oLineNode.Value).SplitLine(m_listBorderPoints[nPos1], m_listBorderPoints[nPos2]));
                            oLineNode = oLineNodeBack1;
                        }
                    }
                }
                oLineNode = GetNextNode(oLineNode);
            }
            DebugStatic.oDebugWatch.StopWatch(strStep, m_listBorderLines.Count().ToString());
            DebugStatic.oDebugBitmap.MakeBitmap(strStep, m_listBorderLines.ToArray());
            m_oTracerParam.SetBorderLine(m_listBorderLines.ToList(), strStep);


            strStep = "Combine line with same length";
            DebugStatic.oDebugWatch.StartWatch(strStep);
            oLineNode = m_listBorderLines.First;
            nTurns = m_listBorderLines.Count();
            for (int i = 0; i < nTurns; i++)
            {
                int nTemp = ((BorderPartLine)oLineNode.Value).m_nSize;
                oLineNodeBack1 = GetPreviousNode(oLineNode);
                oLineNode1 = GetNextNode(oLineNode);
                oLineNode2 = GetNextNode(oLineNode1);
                while (((BorderPartLine)oLineNode2.Value).m_nSize == nTemp && (((BorderPartLine)oLineNodeBack1.Value).m_oDirection == ((BorderPartLine)oLineNode1.Value).m_oDirection) && (((BorderPartLine)oLineNode.Value).CombineSameLine((BorderPartLine)oLineNode1.Value, (BorderPartLine)oLineNode2.Value) == true))
                {
                    m_listBorderLines.Remove(oLineNode1.Value);
                    m_listBorderLines.Remove(oLineNode2.Value);
                    oLineNode1 = GetNextNode(oLineNode);
                    oLineNode2 = GetNextNode(oLineNode1);
                }

                if (i < 2)
                {
                    oLineNodeBack1 = GetNextNode(oLineNode);
                    oLineNode1 = GetPreviousNode(oLineNode);
                    oLineNode2 = GetPreviousNode(oLineNode1);
                    while (((BorderPartLine)oLineNode2.Value).m_nSize == nTemp && (((BorderPartLine)oLineNodeBack1.Value).m_oDirection == ((BorderPartLine)oLineNode1.Value).m_oDirection) && (((BorderPartLine)oLineNode.Value).CombineSameLine((BorderPartLine)oLineNode1.Value, (BorderPartLine)oLineNode2.Value) == true))
                    {
                        m_listBorderLines.Remove(oLineNode1.Value);
                        m_listBorderLines.Remove(oLineNode2.Value);
                        oLineNode1 = GetPreviousNode(oLineNode);
                        oLineNode2 = GetPreviousNode(oLineNode1);
                    }
                }
                oLineNode = GetNextNode(oLineNode);
            }
            DebugStatic.oDebugWatch.StopWatch(strStep, m_listBorderLines.Count().ToString());
            DebugStatic.oDebugBitmap.MakeBitmap(strStep, m_listBorderLines.ToArray());
            m_oTracerParam.SetBorderLine(m_listBorderLines.ToList(), strStep);


            //for (int s = 2; s < 5; s++)
            for (int s = 5; s > 1; s--)
            {
                strStep = "Combine line with same alpha (size=" + s + ")";
                DebugStatic.oDebugWatch.StartWatch(strStep);
                oLineNode = m_listBorderLines.First;
                nTurns = m_listBorderLines.Count();
                for (int i = 0; i < nTurns; i++)
                {
                    oLineNode1 = GetNextNode(oLineNode);
                    oLineNode2 = GetNextNode(oLineNode1);
                    if (((BorderPartLine)oLineNode1.Value).m_nSize == s)
                    {
                        if (((BorderPartLine)oLineNode1.Value).IsSameAlpha((BorderPartLine)oLineNode.Value) && ((BorderPartLine)oLineNode1.Value).IsSameAlpha((BorderPartLine)oLineNode2.Value))
                        {
                            double dA1 = ((BorderPartLine)oLineNode1.Value).GetDiffAlphaAbs((BorderPartLine)oLineNode.Value);
                            double dA2 = ((BorderPartLine)oLineNode1.Value).GetDiffAlphaAbs((BorderPartLine)oLineNode2.Value);
                            if (dA1 < dA2 || (dA1 == dA2 && ((BorderPartLine)oLineNode.Value).m_nSize > ((BorderPartLine)oLineNode2.Value).m_nSize))
                            {
                                if (((BorderPartLine)oLineNode.Value).IsSameAlpha((BorderPartLine)oLineNode1.Value) == true)
                                {
                                    if (((BorderPartLine)oLineNode.Value).CombineLine((BorderPartLine)oLineNode1.Value))
                                    {
                                        m_listBorderLines.Remove(oLineNode1.Value);
                                    }
                                }
                            }
                            else
                            {
                                if (((BorderPartLine)oLineNode1.Value).IsSameAlpha((BorderPartLine)oLineNode2.Value) == true)
                                {
                                    if (((BorderPartLine)oLineNode1.Value).CombineLine((BorderPartLine)oLineNode2.Value))
                                    {
                                        m_listBorderLines.Remove(oLineNode2.Value);
                                    }
                                }
                            }
                        }
                    }
                    oLineNode = GetNextNode(oLineNode);
                }
                DebugStatic.oDebugWatch.StopWatch(strStep, m_listBorderLines.Count().ToString());
                DebugStatic.oDebugBitmap.MakeBitmap(strStep, m_listBorderLines.ToArray());
                m_oTracerParam.SetBorderLine(m_listBorderLines.ToList(), strStep);
            }

            for (int s = 4; s > 1; s--)
            {
                strStep = "Combine (size=" + s + ")";
                DebugStatic.oDebugWatch.StartWatch(strStep);
                oLineNode = m_listBorderLines.First;
                nTurns = m_listBorderLines.Count() * 4;
                for (int i = 0; i < nTurns; i++)
                {
                    oLineNode1 = GetNextNode(oLineNode);
                    oLineNode2 = GetNextNode(oLineNode1);
                    if (((BorderPartLine)oLineNode.Value).m_nSize > s && ((BorderPartLine)oLineNode.Value).m_nSize > ((BorderPartLine)oLineNode1.Value).m_nSize && ((BorderPartLine)oLineNode2.Value).m_nSize > s && ((BorderPartLine)oLineNode2.Value).m_nSize > ((BorderPartLine)oLineNode1.Value).m_nSize)
                    {
                        if (((BorderPartLine)oLineNode.Value).IsSameAlpha((BorderPartLine)oLineNode1.Value) && ((BorderPartLine)oLineNode.Value).IsSameAlpha((BorderPartLine)oLineNode2.Value) && ((BorderPartLine)oLineNode1.Value).IsSameAlpha((BorderPartLine)oLineNode2.Value))
                        {
                            if (((BorderPartLine)oLineNode.Value).CombineSameLinexx((BorderPartLine)oLineNode1.Value, (BorderPartLine)oLineNode2.Value) == true)
                            {
                                m_listBorderLines.Remove(oLineNode1.Value);
                                m_listBorderLines.Remove(oLineNode2.Value);
                            }
                        }
                    }
                    oLineNode = GetNextNode(oLineNode);
                }
                DebugStatic.oDebugWatch.StopWatch(strStep, m_listBorderLines.Count().ToString());
                DebugStatic.oDebugBitmap.MakeBitmap(strStep, m_listBorderLines.ToArray());
                m_oTracerParam.SetBorderLine(m_listBorderLines.ToList(), strStep);
            }


            strStep = "Combine line with same alpha";
            DebugStatic.oDebugWatch.StartWatch(strStep);
            oLineNode = m_listBorderLines.First;
            nTurns = m_listBorderLines.Count();
            for (int z = 0; z < 3; z++)
            {
                double dPixel = 1.0 + 0.5 * z;

                for (int i = 0; i < nTurns * 2; i++)
                {
                    oLineNode1 = GetNextNode(oLineNode);
                    oLineNode2 = GetNextNode(oLineNode1);

                    double dA1 = ((BorderPartLine)oLineNode1.Value).GetDiffAlphaAbs((BorderPartLine)oLineNode.Value);
                    double dA2 = ((BorderPartLine)oLineNode1.Value).GetDiffAlphaAbs((BorderPartLine)oLineNode2.Value);
                    if (dA1 < dA2 || (dA1 == dA2 && ((BorderPartLine)oLineNode.Value).m_nSize > ((BorderPartLine)oLineNode2.Value).m_nSize))
                    {
                        if (((BorderPartLine)oLineNode.Value).CombineSameAlpha((BorderPartLine)oLineNode1.Value, dPixel) == true)
                        {
                            m_listBorderLines.Remove(oLineNode1.Value);
                        }
                        else if (((BorderPartLine)oLineNode1.Value).CombineSameAlpha((BorderPartLine)oLineNode2.Value, dPixel) == true)
                        {
                            m_listBorderLines.Remove(oLineNode2.Value);
                        }
                        else
                        {
                            oLineNode = GetNextNode(oLineNode);
                        }
                    }
                    else
                    {
                        if (((BorderPartLine)oLineNode1.Value).CombineSameAlpha((BorderPartLine)oLineNode2.Value, dPixel) == true)
                        {
                            m_listBorderLines.Remove(oLineNode2.Value);
                        }
                        else if (((BorderPartLine)oLineNode.Value).CombineSameAlpha((BorderPartLine)oLineNode1.Value, dPixel) == true)
                        {
                            m_listBorderLines.Remove(oLineNode1.Value);
                        }
                        else
                        {
                            oLineNode = GetNextNode(oLineNode);
                        }
                    }
                }
            }
            DebugStatic.oDebugWatch.StopWatch(strStep, m_listBorderLines.Count().ToString());
            DebugStatic.oDebugBitmap.MakeBitmap(strStep, m_listBorderLines.ToArray());
            m_oTracerParam.SetBorderLine(m_listBorderLines.ToList(), strStep);


            strStep = "Combine corner line with same alpha";
            DebugStatic.oDebugWatch.StartWatch(strStep);
            oLineNode = m_listBorderLines.First;
            nTurns = m_listBorderLines.Count();
            for (int i = 0; i < nTurns; i++)
            {
                double dPixel = 4.0;
                oLineNode1 = GetNextNode(oLineNode);
                oLineNode2 = GetNextNode(oLineNode1);

                double dA3 = ((BorderPartLine)oLineNode.Value).GetDiffAlphaAbs((BorderPartLine)oLineNode2.Value);
                if (dA3 > 50 && dA3 < 130)
                {
                    double dA1 = ((BorderPartLine)oLineNode1.Value).GetDiffAlphaAbs((BorderPartLine)oLineNode.Value);
                    double dA2 = ((BorderPartLine)oLineNode1.Value).GetDiffAlphaAbs((BorderPartLine)oLineNode2.Value);
                    if (dA1 < dA2)
                    {
                        if (((BorderPartLine)oLineNode.Value).CombineSameAlpha((BorderPartLine)oLineNode1.Value, dPixel) == true)
                        {
                            m_listBorderLines.Remove(oLineNode1.Value);
                        }
                    }
                    else
                    {
                        if (((BorderPartLine)oLineNode1.Value).CombineSameAlpha((BorderPartLine)oLineNode2.Value, dPixel) == true)
                        {
                            m_listBorderLines.Remove(oLineNode2.Value);
                        }
                    }
                }
                oLineNode = GetNextNode(oLineNode);
            }
            DebugStatic.oDebugWatch.StopWatch(strStep, m_listBorderLines.Count().ToString());
            DebugStatic.oDebugBitmap.MakeBitmap(strStep, m_listBorderLines.ToArray());
            m_oTracerParam.SetBorderLine(m_listBorderLines.ToList(), strStep);


            strStep = "Remove corner line (size=2-6)";
            DebugStatic.oDebugWatch.StartWatch(strStep);
            oLineNode = m_listBorderLines.First;
            nTurns = m_listBorderLines.Count();
            for (int i = 0; i < nTurns; i++)
            {
                if (((BorderPartLine)oLineNode.Value).m_nSize < 7)
                {
                    oLineNode1 = GetNextNode(oLineNode);
                    oLineNodeBack1 = GetPreviousNode(oLineNode);
                    //double dA1 = ((BorderPartLine)oLineNode.Value).GetDiffAlphaAbs((BorderPartLine)oLineNode1.Value);
                    //double dA2 = ((BorderPartLine)oLineNode.Value).GetDiffAlphaAbs((BorderPartLine)oLineNodeBack1.Value);
                    double dA3 = ((BorderPartLine)oLineNode1.Value).GetDiffAlphaAbs((BorderPartLine)oLineNodeBack1.Value);
                    //if (dA1 > 20 && dA1 < 70 && dA2 > 20 && dA2 < 70 && dA3 > 60)
                    if (dA3 > 50 && dA3 < 130)
                    {
                        m_listBorderLines.Remove(oLineNode.Value);
                        oLineNode = oLineNodeBack1;
                    }
                }
                oLineNode = GetNextNode(oLineNode);
            }
            DebugStatic.oDebugWatch.StopWatch(strStep, m_listBorderLines.Count().ToString());
            DebugStatic.oDebugBitmap.MakeBitmap(strStep, m_listBorderLines.ToArray());
            m_oTracerParam.SetBorderLine(m_listBorderLines.ToList(), strStep);


            // combine line to arc
            if (m_oTracerParam.m_oServerEventArgs.nPointsPerCircle < 8) return;
            strStep = "Convert line to arc";
            DebugStatic.oDebugWatch.StartWatch(strStep);
            oLineNode = m_listBorderLines.First;
            oLineNode1 = oLineNode;
            nTurns = m_listBorderLines.Count();
            if (nTurns < 5) return;
            LinkedList<BorderPart> listBorderArc = new LinkedList<BorderPart>();
            // find start
            for (int i = 0; i < nTurns; i++)
            {
                oLineNode1 = GetNextNode(oLineNode);
                if (((BorderPartLine)oLineNode.Value).GetDiffAlphaAbs((BorderPartLine)oLineNode1.Value) >= 45)
                {
                    i = nTurns * 3;
                }
                oLineNode = oLineNode1;
            }
            BorderPartArc oBorderPartArc = new BorderPartArc();
            oLineNodeStart = oLineNode;
            oLineNode1 = GetNextNode(oLineNode);
            List<BorderPartLine> listLine;
            do
            {
                int nMinCount = 0;
                double dAlpha = 0;
                if (((BorderPartLine)oLineNode.Value).GetDiffAlphaAbs((BorderPartLine)oLineNode1.Value) < 45)
                {
                    int nDir = ((BorderPartLine)oLineNode.Value).GetDiffAlphaAbs((BorderPartLine)oLineNode1.Value) > 0 ? 1 : -1;
                    listLine = new List<BorderPartLine>();
                    listLine.Add((BorderPartLine)oLineNode.Value);
                    listLine.Add((BorderPartLine)oLineNode1.Value);

                    double dAlphaPart;
                    while (oLineNode1 != oLineNodeStart)
                    {
                        oLineNode2 = GetNextNode(oLineNode1);
                        if (oLineNode2 == oLineNodeStart || oLineNode2.Value.GetType() != typeof(BorderPartLine))
                        {
                            break;
                        }
                        dAlphaPart = ((BorderPartLine)oLineNode1.Value).GetDiffAlphaAbs((BorderPartLine)oLineNode2.Value) * nDir;
                        if (dAlphaPart <= 0 || dAlphaPart >= 45)
                        {
                            break;
                        }
                        listLine.Add((BorderPartLine)oLineNode2.Value);
                        oLineNode1 = oLineNode2;

                        if (dAlpha < 40) nMinCount++;
                        dAlpha += dAlphaPart;
                    }
                    if (dAlpha >= 40)
                    {
                        if (nMinCount < 5) nMinCount = 5;

                        EllipseFit oEllipseFit;
                        bool bFound = false;
                        while (bFound == false && listLine.Count >= nMinCount)
                        {
                            List<Point> listCheckPoints = new List<Point>();
                            listCheckPoints.Add(listLine[0].m_oPointStart);
                            foreach (BorderPartLine oBPL in listLine)
                            {
                                listCheckPoints.Add(GetPointInTehMiddle(oBPL.m_oPointStart, oBPL.m_oPointEnd, 0.5));
                                listCheckPoints.Add(oBPL.m_oPointEnd);
                            }
                            oBorderPartArc = new BorderPartArc();
                            List<Point> listPoints = GetPointListMax(listLine[0].m_oPointStart, listLine[listLine.Count - 1].m_oPointEnd, 2);

                            //string strTemp = "EllipseFit2";
                            //DebugStatic.oDebugWatch.StartWatch(strTemp);
                            oEllipseFit = new EllipseFit();
                            oEllipseFit.Fit(listPoints);
                            //DebugStatic.oDebugWatch.StopWatch(strTemp);

                            foreach (Ellipse oE in oEllipseFit.m_listEllipse)
                            {
                                if (oBorderPartArc.Point2Ellipse(oE, listPoints, listCheckPoints) == true)
                                {
                                    bFound = true;
                                    break;
                                }
                            }
                            if (bFound == false) listLine.RemoveAt(listLine.Count - 1);
                        }
                        if (listLine.Count >= nMinCount)
                        {
                            if (listLine.Count == m_listBorderLines.Count())
                            {
                                m_listBorderLines.Clear();
                                m_listBorderLines.AddFirst(oBorderPartArc);
                                oLineNode = m_listBorderLines.First;
                            }
                            else
                            {
                                oLineNode = GetPreviousNode(oLineNode);
                                m_listBorderLines.AddAfter(oLineNode, oBorderPartArc);
                                foreach (BorderPartLine oBPL in listLine)
                                {
                                    m_listBorderLines.Remove(oBPL);
                                }
                                oLineNode = GetNextNode(oLineNode);
                                oBorderPartArc.m_oLineStart = listLine[0];
                                oBorderPartArc.m_oLineEnd = listLine[listLine.Count - 1];
                            }
                        }
                    }
                }
                oLineNode = GetNextNode(oLineNode);
                oLineNode1 = GetNextNode(oLineNode);
            }
            while (oLineNode != oLineNodeStart && oLineNode.Value.GetType() == typeof(BorderPartLine) && oLineNode1 != oLineNodeStart && oLineNode1.Value.GetType() == typeof(BorderPartLine));

            DebugStatic.oDebugWatch.StopWatch(strStep, m_listBorderLines.Count().ToString());
            DebugStatic.oDebugBitmap.MakeBitmap(strStep, m_listBorderLines.ToArray());
            m_oTracerParam.SetBorderLine(m_listBorderLines.ToList(), strStep);
        }

        public List<BorderPart> GetPoints()
        {
            List<BorderPart> listBorderPart = new List<BorderPart>();
            StraightLine oStraightLine1;
            StraightLine oStraightLine2;
            PointD oPointD;
            BorderPart oBorderPart;
            BorderPartLine oBorderPartLine;
            BorderPartArc oBorderPartArc;
            List<PointD> listPointD;
            string strStep = "Convert to geographic points";

            if (m_listBorderLines.Count() == 1)
            {
                // only one ellipse
                listBorderPart = new List<BorderPart>();
                listPointD = ((BorderPartArc)m_listBorderLines.First.Value).GetBorderPoints(m_oTracerParam.m_oServerEventArgs.nPointsPerCircle);
                foreach (PointD oPD in listPointD)
                {
                    if (double.IsNaN(oPD.dX) || double.IsNaN(oPD.dY))
                    {
                        m_oTracerParam.m_strError = "Point is not a nummber: " + oPD.ToString();
                        m_oTracerParam.SetBorderLine(listBorderPart, strStep);
                        return null;
                    }
                    listBorderPart.Add(new BorderPartPointGeo(m_oTracerParam.m_oGeoMap.ConvertPointToGeo(oPD)));
                }
                m_oTracerParam.SetBorderLine(listBorderPart, strStep);
                return listBorderPart;
            }

            // get first line
            oBorderPart = m_listBorderLines.Last();
            if (oBorderPart.GetType() == typeof(BorderPartLine))
            {
                oBorderPartLine = (BorderPartLine)oBorderPart;
            }
            else
            {
                oBorderPartLine = ((BorderPartArc)oBorderPart).m_oLineEnd;
            }
            oStraightLine1 = CalcStraightLine(oBorderPartLine);

            // get all points
            foreach (BorderPart oBP in m_listBorderLines)
            {
                if (oBP.GetType() == typeof(BorderPartLine))
                {
                    oBorderPartLine = (BorderPartLine)oBP;
                    oStraightLine2 = CalcStraightLine(oBorderPartLine);

                    oPointD = GetIntersectionPoint(oStraightLine1, oStraightLine2);
                    if (double.IsNaN(oPointD.dX) || double.IsNaN(oPointD.dY))
                    {
                        m_oTracerParam.m_strError = "No intersection point: " + oStraightLine1.ToString() + " " + oStraightLine2.ToString();
                        m_oTracerParam.SetBorderLine(listBorderPart, strStep);
                        return null;
                    }
                    listBorderPart.Add(new BorderPartPointGeo(m_oTracerParam.m_oGeoMap.ConvertPointToGeo(oPointD)));
                    oStraightLine1 = oStraightLine2;
                }
                else
                {
                    oBorderPartArc = (BorderPartArc)oBP;
                    oBorderPartLine = oBorderPartArc.m_oLineStart;
                    oStraightLine2 = CalcStraightLine(oBorderPartLine);

                    // get start point
                    oPointD = GetIntersectionPoint(oStraightLine1, oStraightLine2);
                    if (double.IsNaN(oPointD.dX) || double.IsNaN(oPointD.dY))
                    {
                        m_oTracerParam.m_strError = "No intersection point: " + oStraightLine1.ToString() + " " + oStraightLine2.ToString();
                        m_oTracerParam.SetBorderLine(listBorderPart, strStep);
                        return null;
                    }
                    listBorderPart.Add(new BorderPartPointGeo(m_oTracerParam.m_oGeoMap.ConvertPointToGeo(oPointD)));

                    // get end point
                    oBorderPartLine = oBorderPartArc.m_oLineEnd;
                    oStraightLine1 = CalcStraightLine(oBorderPartLine);

                    oBorderPart = GetNextNode(m_listBorderLines.Find(oBP)).Value;
                    if (oBorderPart.GetType() == typeof(BorderPartLine))
                    {
                        oBorderPartLine = (BorderPartLine)oBorderPart;
                    }
                    else
                    {
                        oBorderPartLine = ((BorderPartArc)oBorderPart).m_oLineStart;
                    }
                    oStraightLine2 = CalcStraightLine(oBorderPartLine);
                    PointD oPointD2 = GetIntersectionPoint(oStraightLine1, oStraightLine2);
                    if (double.IsNaN(oPointD2.dX) || double.IsNaN(oPointD2.dY))
                    {
                        m_oTracerParam.m_strError = "No intersection point: " + oStraightLine1.ToString() + " " + oStraightLine2.ToString();
                        m_oTracerParam.SetBorderLine(listBorderPart, strStep);
                        return null;
                    }

                    // get ellipse points
                    listPointD = oBorderPartArc.GetBorderPoints(oPointD, oPointD2, m_oTracerParam.m_oServerEventArgs.nPointsPerCircle);
                    foreach (PointD oPD in listPointD)
                    {
                        if (double.IsNaN(oPD.dX) || double.IsNaN(oPD.dY))
                        {
                            m_oTracerParam.m_strError = "Point is not a nummber: " + oPD.ToString();
                            m_oTracerParam.SetBorderLine(listBorderPart, strStep);
                            return null;
                        }
                        listBorderPart.Add(new BorderPartPointGeo(m_oTracerParam.m_oGeoMap.ConvertPointToGeo(oPD)));
                    }
                }
            }
            m_oTracerParam.SetBorderLine(listBorderPart, strStep);
            if (listBorderPart.Count <= 2)
            {
                // no area
                m_oTracerParam.m_strError = "No area found.";
                return null;
            }
            return listBorderPart;
        }

        private PointD GetIntersectionPoint(StraightLine oLine1, StraightLine oLine2)
        {
            PointD oPointD = oLine1.GetIntersectionPoint(oLine2);

            if (oPointD.dX < 0 || oPointD.dX > m_oTracerParam.m_oGeoMap.m_nXYmax || oPointD.dY < 0 || oPointD.dY > m_oTracerParam.m_oGeoMap.m_nXYmax)
            {
                return new PointD(double.NaN, double.NaN);
            }
            if (double.IsNaN(oPointD.dX))
            {
                return new PointD(double.NaN, double.NaN);
            }
            return oPointD;
        }

        private Point GetPointInTehMiddle(Point oPointStart, Point oPointEnd, double dPos)
        {
            int nStart = m_listBorderPoints.IndexOf(oPointStart);
            int nEnd = m_listBorderPoints.IndexOf(oPointEnd);
            int nCount;
            int nPos;

            if (nStart > nEnd)
            {
                nCount = m_listBorderPoints.Count() - nStart + nEnd;
            }
            else
            {
                nCount = nEnd - nStart;
            }
            nCount = (int)(dPos * nCount);
            nPos = nStart + nCount;
            if (nPos >= m_listBorderPoints.Count())
            {
                nPos = nPos - m_listBorderPoints.Count();
            }
            return m_listBorderPoints.ElementAt(nPos);
        }

        private List<Point> GetPointListMax(Point oPointStart, Point oPointEnd, int nRemove)
        {
            List<Point> listPoint;

            if (oPointStart != oPointEnd)
            {
                listPoint = GetPointList(oPointStart, oPointEnd);
                if (listPoint.Count > nRemove * 2)
                {
                    listPoint.RemoveRange(0, nRemove);
                    listPoint.RemoveRange(listPoint.Count - nRemove, nRemove);
                }
            }
            else
            {
                listPoint = new List<Point>(m_listBorderPoints.ToArray());
            }

            if (listPoint.Count > 700)
            {
                int nStep = listPoint.Count / 500;
                List<Point> listPoint2 = new List<Point>();
                for (int i = 0; i < listPoint.Count - 1; i += nStep)
                {
                    listPoint2.Add(listPoint[i]);
                }
                return listPoint2;
            }
            return listPoint;
        }

        private List<Point> GetPointList(Point oPointStart, Point oPointEnd)
        {
            int nStart = -1;
            int nEnd = -1;
            List<Point> listPoint = new List<Point>();

            nStart = m_listBorderPoints.IndexOf(oPointStart);
            nEnd = m_listBorderPoints.IndexOf(oPointEnd);
            for (int i = 0; i < m_listBorderPoints.Count(); i++)
            {
                if (m_listBorderPoints[i] == oPointStart)
                {
                    nStart = i;
                }
                if (m_listBorderPoints[i] == oPointEnd)
                {
                    nEnd = i;
                }
            }

            if (nStart > nEnd)
            {
                for (int i = nStart; i < m_listBorderPoints.Count(); i++)
                {
                    listPoint.Add(m_listBorderPoints[i]);
                }
                for (int i = 0; i <= nEnd; i++)
                {
                    listPoint.Add(m_listBorderPoints[i]);
                }
            }
            else
            {
                for (int i = nStart; i <= nEnd; i++)
                {
                    listPoint.Add(m_listBorderPoints[i]);
                }
            }
            return listPoint;
        }

        private StraightLine CalcStraightLine(BorderPartLine oLine)
        {
            double dm = double.NaN;
            double dc = double.NaN;
            bool bRevert;

            StraightLine oStraightLineFit;
            StraightLine oStraightLine;

            List<Point> listPoints = GetPointList(oLine.m_oPointStart, oLine.m_oPointEnd);
            LineFit oLineFit = new LineFit();
            oLineFit.Fit(listPoints);
            dm = oLineFit.m_dm;
            dc = oLineFit.m_dc;
            bRevert = oLineFit.m_bRevert;

            oStraightLineFit = new StraightLine(dm, dc, bRevert);
            oStraightLine = new StraightLine((PointD)oLine.m_oPointStart, (PointD)oLine.m_oPointEnd);

            if (oStraightLineFit.IsLine)
            {
                // check alpha
                double dDif = StraightLine.CheckAlpha(oStraightLine.dAlpha - oStraightLineFit.dAlpha);
                if ((dDif < 20 && dDif > -20) || dDif > 180 - 20 || dDif < -180 + 20)
                {
                    oStraightLine = oStraightLineFit;
                }
                else
                {
                    oStraightLineFit = oStraightLine;
                }
            }
            return oStraightLine;
        }

        private LinkedListNode<BorderPart> GetNextNode(LinkedListNode<BorderPart> oNode)
        {
            LinkedListNode<BorderPart> oNext = oNode.Next;
            if (oNext == null)
            {
                oNext = m_listBorderLines.First;
            }
            return oNext;
        }

        private LinkedListNode<BorderPart> GetPreviousNode(LinkedListNode<BorderPart> oNode)
        {
            LinkedListNode<BorderPart> oPrevious = oNode.Previous;
            if (oPrevious == null)
            {
                oPrevious = m_listBorderLines.Last;
            }
            return oPrevious;
        }

    }

}
