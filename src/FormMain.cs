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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Tracer2Server.Trace;
using Tracer2Server.WebServer;

namespace Tracer2Server
{
    partial class FormMain : Form
    {
        static private List<FormDetails> s_listFormDetails = new List<FormDetails>();
        static private Object s_oLock = new object();

        static public void AddFormDetails(FormDetails oFormDetails)
        {
            if (oFormDetails != null)
            {
                lock (s_oLock)
                {
                    s_listFormDetails.Add(oFormDetails);
                }
            }
        }

        static public void RemoveFormDetails(FormDetails oFormDetails)
        {
            if (oFormDetails != null)
            {
                lock (s_oLock)
                {
                    s_listFormDetails.Remove(oFormDetails);
                }
            }
        }

        static public bool ExistFormDetails(TracerParam oTracerParam)
        {
            if (oTracerParam != null)
            {
                lock (s_oLock)
                {
                    foreach (FormDetails oFD in s_listFormDetails)
                    {
                        if (oFD.m_oTracerParam == oTracerParam)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private Thread m_oThread = null;

        public delegate void AddListItem(TracerParam oServerEvent, EventType oEventType);
        public AddListItem myDelegate;

        private List<TracerParam> m_listTracerParam = new List<TracerParam>();
        private TracerParam m_oTracerParam = null;

        public FormMain()
        {
            InitializeComponent();

            ServerEventHandler handler = new ServerEventHandler(OnHandlerTracerParam);
            TracerParam.s_oEventHandler += handler;

            myDelegate = new AddListItem(ThreadProcUnsafe);
        }

        public void OnHandlerTracerParam(object sender, EventType oServerEvent)
        {
            try
            {
                if (this.textBox.InvokeRequired)
                {
                    // If Invoke needed then call Methode via Invoke
                    this.textBox.Invoke(this.myDelegate, new Object[] { sender, oServerEvent });
                    return;
                }
                ThreadProcUnsafe((TracerParam)sender, oServerEvent);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n\r" + e.StackTrace.ToString(), "Exception: OnHandlerTracerParam", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ThreadProcUnsafe(TracerParam oTracerParam, EventType oEventType)
        {
            try
            {
                m_oTracerParam = oTracerParam;
                textBox.Text = oTracerParam.m_strOutput;
                textBox.Select(oTracerParam.m_strOutput.Length, 0);
                textBox.ScrollToCaret();
                labelTextBox.Text = oTracerParam.m_strHeadLine;

                if (oEventType == EventType.Stop)
                {
                    if (!m_listTracerParam.Contains(oTracerParam))
                    {
                        listBox.BeginUpdate();
                        listBox.Items.Add(oTracerParam.m_strResult);
                        if (listBox.Items.Count > 6)
                        {
                            if (listBox.SelectedIndex == 0)
                            {
                                buttonDetails.Enabled = false;
                            }
                            listBox.Items.RemoveAt(0);
                        }
                        listBox.EndUpdate();

                        m_listTracerParam.Add(oTracerParam);
                        if (m_listTracerParam.Count > 6)
                        {
                            m_listTracerParam.RemoveAt(0);
                        }
                    }
                }
                //Console.WriteLine(oEventType.ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n\r" + e.StackTrace.ToString(), "Exception: ThreadProcUnsafe", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            m_oThread = new Thread(ThreadReadData);
            m_oThread.Start(this);
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_oThread.Abort();
            m_oThread = null;
        }

        public static void ThreadReadData(object oObject)
        {
            Server webServer = new Server(49243);
            webServer.Start();
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int nPos = listBox.SelectedIndex;
                if (nPos >= 0)
                {
                    TracerParam oTracerParam = m_listTracerParam[nPos];

                    m_oTracerParam = oTracerParam;
                    textBox.Text = oTracerParam.m_strOutput;
                    labelTextBox.Text = oTracerParam.m_strHeadLine;
                    buttonDetails.Enabled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\r" + ex.StackTrace.ToString(), "Exception: listBox_SelectedIndexChanged", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void buttonDetails_Click(object sender, EventArgs e)
        {
            int nPos = listBox.SelectedIndex;

            try
            {
                if (nPos >= 0)
                {
                    TracerParam oTracerParam = m_listTracerParam[nPos];

                    if (ExistFormDetails(oTracerParam) == false)
                    {
                        FormDetails oFormDetails = new FormDetails(oTracerParam);
                        oFormDetails.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\r" + ex.StackTrace.ToString(), "Exception: buttonDetails_Click", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void listBox_DoubleClick(object sender, EventArgs e)
        {
            buttonDetails_Click(sender, e);
        }

    }

}
