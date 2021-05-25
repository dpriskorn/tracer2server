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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Tracer2Server.Trace;
using Tracer2Server.Debug;
using Tracer2Server.Projection;
using Tracer2Server.WebServer;

namespace Tracer2Server.Trace
{
    class Tracer
    {
        public PointGeo[] Trace(TracerParam oTracerParam)
        {
            List<BorderPart> listBorderPart;
            ShapeFinder oShapeFinder;
            PointGeo[] aoPointGeo = null;

            if (oTracerParam == null)
            {
                return null;
            }

            oShapeFinder = new ShapeFinder(oTracerParam);

            try
            {
                DebugStatic.oDebugWatch.StartWatch("GetStartPoint");
                oShapeFinder.GetStartPoint();
                DebugStatic.oDebugWatch.StopWatch("GetStartPoint");

                DebugStatic.oDebugWatch.StartWatch("GetBorderPoints");
                oShapeFinder.GetBorderPoints();
                DebugStatic.oDebugWatch.StopWatch("GetBorderPoints");

                DebugStatic.oDebugWatch.StartWatch("GetBorderLine");
                oShapeFinder.GetBorderLine();
                DebugStatic.oDebugWatch.StopWatch("GetBorderLine");

                DebugStatic.oDebugWatch.StartWatch("GetPoints");
                listBorderPart = oShapeFinder.GetPoints();
                DebugStatic.oDebugWatch.StopWatch("GetPoints");

                oTracerParam.Stop();

                if (listBorderPart != null)
                {
                    aoPointGeo = new PointGeo[listBorderPart.Count];
                    for (int i = 0; i < listBorderPart.Count; i++)
                    {
                        aoPointGeo[i] = ((BorderPartPointGeo)listBorderPart[i]).m_oPG;
                    }
                }
            }
            catch (Exception e)
            {
                oTracerParam.m_strError = "Exception: " + e.Message;
                oTracerParam.m_strStackTrace = e.StackTrace.ToString();
                oTracerParam.Stop();
                return null;
            }
            return aoPointGeo;
        }

    }

}
