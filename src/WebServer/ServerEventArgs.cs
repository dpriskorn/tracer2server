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
using System.Drawing;
using Tracer2Server.Projection;

namespace Tracer2Server.WebServer
{
    public class ServerEventArgs
    {
        private string m_strRequest;

        private string m_strOrder;
        private string m_strLat;
        private string m_strLon;
        private string m_strName;
        private string m_strUrl;
        private string m_strTileSize;
        private string m_strResolution;
        private string m_strSkipBottom;
        private string m_strMode;
        private string m_strThreshold;
        private string m_strPointsPerCircle;

        private double m_dLat;
        private double m_dLon;
        private double m_dTileSize;
        private int m_nResolution;
        private int m_nSkipBottom;
        private int m_nThreshold;
        private int m_nPointsPerCircle;

        private string m_strResponse = "";
        private string m_strMime = "text/plain";
        private int m_nCode = 200;

        public string strOrder
        {
            get { return m_strOrder; }
        }

        public string strResponse
        {
            get { return m_strResponse; }
            set { m_strResponse = value; }
        }

        public string strMime
        {
            get { return m_strMime; }
            set { m_strMime = value; }
        }

        public int nCode
        {
            get { return m_nCode; }
            set { m_nCode = value; }
        }

        public double dLat
        {
            get { return m_dLat; }
        }

        public double dLon
        {
            get { return m_dLon; }
        }

        public string strName
        {
            get { return m_strName; }
        }

        public string strUrl
        {
            get { return m_strUrl; }
        }

        public double dTileSize
        {
            get { return m_dTileSize; }
        }

        public int nResolution
        {
            get { return m_nResolution; }
        }

        public int nSkipBottom
        {
            get { return m_nSkipBottom; }
        }

        public string strMode
        {
            get { return m_strMode; }
        }

        public int nThreshold
        {
            get { return m_nThreshold; }
        }

        public int nPointsPerCircle
        {
            get { return m_nPointsPerCircle; }
        }

        public PointGeo oStartPoint
        {
            get { return new PointGeo(m_dLat, m_dLon); }
        }

        public bool isModeBondary
        {
            get { return m_strMode.Equals("boundary"); }
        }

        private String GetString(String strParamName, String strDefault = "")
        {
            int startPos;
            int endPos;

            startPos = m_strRequest.IndexOf(strParamName);
            if (startPos < 0)
            {
                return strDefault;
            }
            endPos = m_strRequest.IndexOf("&trace", startPos + 1);

            if (endPos < 0)
            {
                endPos = m_strRequest.Length;
            }
            return m_strRequest.Substring(startPos + strParamName.Length, endPos - startPos - strParamName.Length).Trim();
        }

        public bool SetArgs(string buffer)
        {
            NumberFormatInfo oNFIDefault = System.Globalization.NumberFormatInfo.InvariantInfo;

            try
            {
                m_strRequest = buffer;

                m_strOrder = GetString("traceOrder=");
                m_strLat = GetString("&traceLat=", "0");
                m_strLon = GetString("&traceLon=", "0");
                m_strName = GetString("&traceName=");
                m_strUrl = GetString("&traceUrl=");
                m_strTileSize = GetString("&traceTileSize=", "0.0004");
                m_strResolution = GetString("&traceResolution=", "2048");
                m_strSkipBottom = GetString("&traceSkipBottom=", "0");
                m_strMode = GetString("&traceMode=", "boundary");
                m_strThreshold = GetString("&traceThreshold=", "127");
                m_strPointsPerCircle = GetString("&tracePointsPerCircle=", "16");

                m_dLat = double.Parse(m_strLat, oNFIDefault);
                m_dLon = double.Parse(m_strLon, oNFIDefault);
                m_dTileSize = double.Parse(m_strTileSize, oNFIDefault);
                m_nResolution = int.Parse(m_strResolution, oNFIDefault);
                m_nSkipBottom = int.Parse(m_strSkipBottom, oNFIDefault);
                m_nThreshold = int.Parse(m_strThreshold, oNFIDefault);
                m_nPointsPerCircle = int.Parse(m_strPointsPerCircle, oNFIDefault);

                if (m_nSkipBottom > m_nResolution / 10)
                {
                    m_nSkipBottom = m_nResolution / 10;
                }

                if (Math.Abs(m_dLat) > 90 || Math.Abs(m_dLon) > 180)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

    }

}
