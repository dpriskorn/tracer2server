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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;
using Tracer2Server.Trace;
using System.Globalization;
using Tracer2Server.Debug;
using Tracer2Server.Projection;
using System.Reflection;

namespace Tracer2Server.WebServer
{
    class Server
    {
        private TcpListener m_oTcpListener;
        private int m_nPort;
        private bool m_bStarted = false;

        public Server(int nPort)
        {
            m_nPort = nPort;
        }

        public int nPort
        {
            get { return m_nPort; }
        }

        public void Start()
        {
            IPAddress oIpAddress;

            if (m_bStarted)
            {
                return;
            }

            m_bStarted = true;

            IPHostEntry host = Dns.GetHostEntry("localhost");
            if (OperationSystem.isWindows)
            {
                if (host.AddressList.Length > 1)
                {
                    oIpAddress = host.AddressList[1];
                }
                else
                {
                    oIpAddress = host.AddressList[0];
                }
            }
            else
            {
                oIpAddress = host.AddressList[0];
            }
            m_oTcpListener = new TcpListener(oIpAddress, m_nPort);
            //m_oTcpListener = new TcpListener(m_nPort);
            m_oTcpListener.Start();
            Thread th = new Thread(new ThreadStart(StartListen));
            th.IsBackground = true;
            th.Start();
        }

        private void SendHeader(string strHttpVersion, int nBytesLength, string strStatusCode, string strMime, ref Socket r_oSocket)
        {
            StringBuilder buffer = new StringBuilder();
            buffer = buffer.Append(strHttpVersion + strStatusCode + "\r\n");
            buffer = buffer.Append("Server: OSM.WebServer\r\n");
            buffer = buffer.Append(string.Format("Content-Type: {0}\r\n", strMime));
            buffer = buffer.Append("Accept-Ranges: bytes\r\n");
            buffer = buffer.Append("Content-Length: " + nBytesLength + "\r\n\r\n");
            byte[] sendData = Encoding.ASCII.GetBytes(buffer.ToString());
            SendToBrowser(sendData, ref r_oSocket);
        }

        private void SendToBrowser(String strData, ref Socket r_oSocket)
        {
            SendToBrowser(Encoding.ASCII.GetBytes(strData), ref r_oSocket);
        }

        private void SendToBrowser(byte[] aucSendData, ref Socket r_oSocket)
        {
            try
            {
                if (r_oSocket.Connected)
                {
                    r_oSocket.Send(aucSendData, aucSendData.Length, 0);
                }
            }
            catch
            {
            }
        }

        private void StartListen()
        {
            ServerEventArgs oServerEventArgs;
            TracerParam oTracerParam;

            while (true)
            {
                Socket oSocket = m_oTcpListener.AcceptSocket();
                if (oSocket.Connected)
                {
                    byte[] receive = new byte[1024];
                    int i = oSocket.Receive(receive, receive.Length, 0);
                    string buffer = Encoding.ASCII.GetString(receive);
                    if (buffer.Substring(0, 3) != "GET")
                    {
                        oSocket.Close();
                        return;
                    }

                    int startPos = buffer.LastIndexOf("HTTP");
                    string strHttpVersion = buffer.Substring(startPos, 8);
                    string strRequest = buffer.Substring(0, startPos - 1);
                    strRequest.Replace("\\", "/");

                    oServerEventArgs = new ServerEventArgs();
                    if (oServerEventArgs.SetArgs(strRequest) == true)
                    {
                        oTracerParam = new TracerParam(oServerEventArgs);
                        GetContent(oTracerParam);
                    }
                    else
                    {
                        oTracerParam = new TracerParam(oServerEventArgs);
                        oTracerParam.m_oServerEventArgs.nCode = 400;
                        oTracerParam.SetError("The request cannot be fulfilled due to bad syntax.");
                        oTracerParam.Stop();

                        oTracerParam.m_oServerEventArgs.strResponse = "&traceError=" + oTracerParam.m_strError;
                    }
                    byte[] bytes = Encoding.UTF8.GetBytes(oTracerParam.m_oServerEventArgs.strResponse);
                    SendHeader(strHttpVersion, bytes.Length, string.Format(" {0} OK", oServerEventArgs.nCode), oServerEventArgs.strMime, ref oSocket);
                    SendToBrowser(bytes, ref oSocket);
                    oSocket.Close();
                }
            }
        }

        private void GetContent(TracerParam oTracerParam)
        {
            Console.WriteLine("- {0}/{1}/{2}", oTracerParam.m_oServerEventArgs.strOrder, oTracerParam.m_oServerEventArgs.dLat, oTracerParam.m_oServerEventArgs.dLon);

            switch (oTracerParam.m_oServerEventArgs.strOrder)
            {
                case "GetTrace":
                    try
                    {
                        oTracerParam.m_oServerEventArgs.strResponse = GetTrace(oTracerParam);
                    }
                    catch (Exception e)
                    {
                        oTracerParam.m_strError = "Exception: " + e.Message;
                        oTracerParam.m_strStackTrace = e.StackTrace.ToString();
                        oTracerParam.m_oServerEventArgs.strResponse = "&traceError=" + oTracerParam.m_strError;
                        oTracerParam.Stop();
                    }
                    break;
                case "GetVersion":
                    Version Version = Assembly.GetCallingAssembly().GetName().Version;
                    oTracerParam.m_oServerEventArgs.strResponse = Version.Major.ToString() + ":" + Version.Minor.ToString() + ":" + Version.Build.ToString() + ":" + Version.Revision.ToString();
                    break;
                default:
                    oTracerParam.m_oServerEventArgs.nCode = 404;
                    break;
            }
        }

        private string GetTrace(TracerParam oTracerParam)
        {
            NumberFormatInfo oNFIDefault = System.Globalization.NumberFormatInfo.InvariantInfo;
            Tracer oTracer = new Tracer();

            DebugStatic.oDebugWatch.StartWatch("TraceSimple");
            PointGeo[] aoPointGeo = oTracer.Trace(oTracerParam);
            DebugStatic.oDebugWatch.StopWatch("TraceSimple");

            if (aoPointGeo == null)
            {
                return "&traceError=" + oTracerParam.m_strError;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("tracePoints=");
            foreach (PointGeo oPointGeo in aoPointGeo)
            {
                sb.Append(string.Format(oNFIDefault, "({0:F8}:{1:F8})", oPointGeo.dLat, oPointGeo.dLon));
            }
            if (oTracerParam.m_oServerEventArgs.strMode == "match color")
            {
                sb.Append("&traceColorARGB=");
                sb.Append("(");
                sb.Append(oTracerParam.m_oInnerColor.A.ToString());
                sb.Append(":");
                sb.Append(oTracerParam.m_oInnerColor.R.ToString());
                sb.Append(":");
                sb.Append(oTracerParam.m_oInnerColor.G.ToString());
                sb.Append(":");
                sb.Append(oTracerParam.m_oInnerColor.B.ToString());
                sb.Append(")");
            }
            return sb.ToString();
        }

    }

}
