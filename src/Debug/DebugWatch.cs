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
using System.Diagnostics;
using System.Globalization;

namespace Tracer2Server.Debug
{
    class DebugWatch
    {
        class MyStopwatch
        {
            private string m_strName;
            private Stopwatch m_oStopWatch;
            private TimeSpan m_oTimeSpan;

            public string strName
            {
                get
                {
                    return m_strName;
                }
            }

            public MyStopwatch(string strName)
            {
                this.m_strName = strName;
                this.m_oStopWatch = new Stopwatch();
                this.m_oTimeSpan = new TimeSpan();

                this.m_oStopWatch.Start();
            }

            public bool Stop()
            {
                if (m_oStopWatch != null)
                {
                    this.m_oTimeSpan = m_oStopWatch.Elapsed;
                    m_oStopWatch = null;
                    return true;
                }
                return false;
            }

            public override string ToString()
            {
                return string.Format(CultureInfo.CurrentCulture, "{0:00}:{1:00}:{2:00}.{3:000}",
                        m_oTimeSpan.Hours, m_oTimeSpan.Minutes, m_oTimeSpan.Seconds, m_oTimeSpan.Milliseconds);
            }
        }

        List<MyStopwatch> m_listMyStopwatch = null;
        object oLockMyStopwatch = new object();

        public DebugWatch()
        {
            if (DebugStatic.bDebug)
            {
                m_listMyStopwatch = new List<MyStopwatch>();
            }
        }

        public void StartWatch(string strText)
        {
            if (DebugStatic.bDebug && m_listMyStopwatch != null)
            {
                lock (oLockMyStopwatch)
                {
                    MyStopwatch oMyStopwatch = new MyStopwatch(strText);
                    for (int i = m_listMyStopwatch.Count(); i > 0; i--)
                    {
                        Console.Write(" ");
                    }
                    Console.WriteLine("Watch: " + strText);
                    m_listMyStopwatch.Add(oMyStopwatch);
                }
            }
        }

        public void StopWatch(string strText, string strInfo = null)
        {
            if (DebugStatic.bDebug && m_listMyStopwatch != null)
            {
                lock (oLockMyStopwatch)
                {
                    foreach (MyStopwatch oMyStopwatch in m_listMyStopwatch)
                    {
                        if (oMyStopwatch.strName.Equals(strText))
                        {
                            oMyStopwatch.Stop();
                            m_listMyStopwatch.Remove(oMyStopwatch);
                            for (int i = m_listMyStopwatch.Count(); i > 0; i--)
                            {
                                Console.Write(" ");
                            }
                            if (strInfo == null)
                            {
                                Console.WriteLine("Watch: " + strText + " " + oMyStopwatch.ToString());
                            }
                            else
                            {
                                Console.WriteLine("Watch: " + strText + " " + strInfo + " " + oMyStopwatch.ToString());
                            }
                            return;
                        }
                    }
                }
            }
        }

    }

}
