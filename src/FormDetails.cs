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
using System.Drawing.Imaging;
using System.Diagnostics;
using Tracer2Server.Trace;
using Tracer2Server.Tiles;
using Tracer2Server.Projection;
using System.Management;

namespace Tracer2Server
{
    partial class FormDetails : Form
    {
        public TracerParam m_oTracerParam = null;
        private Bitmap m_oBitmapOverview;
        private Bitmap m_oBitmapOverviewSmal;
        private Bitmap m_oBitmapResultSmal;
        private Bitmap m_oBitmapResultPart;
        private Bitmap m_oBitmapHiliteSmal;
        private Bitmap m_oBitmapAll;
        private Bitmap m_oBitmapPart;

        private Object m_oHilitePart;
        private BorderPart[] m_aoBorderPart;

        private Rectangle m_oRectangleAll;
        private Rectangle m_oRectanglePart;
        private Point m_oPointCenterPart;
        private double m_dZoom = 1.0;

        private Color m_oColorData = Color.Yellow;
        private Color m_oColorHilite = Color.Red;
        private Color m_oColorArea = Color.Blue;
        private Color m_oColorTile = Color.Gray;

        public FormDetails(TracerParam oTracerParam)
        {
            InitializeComponent();

            m_oTracerParam = oTracerParam;

            FormMain.AddFormDetails(this);

            this.Text = m_oTracerParam.m_strResult;

            textBoxName.Text = m_oTracerParam.m_oServerEventArgs.strName;
            textBoxURL.Text = m_oTracerParam.m_oServerEventArgs.strUrl;
            textBoxLat.Text = m_oTracerParam.m_oServerEventArgs.dLat.ToString();
            textBoxLon.Text = m_oTracerParam.m_oServerEventArgs.dLon.ToString();
            textBoxTileSize.Text = m_oTracerParam.m_oServerEventArgs.dTileSize.ToString();
            textBoxResolution.Text = m_oTracerParam.m_oServerEventArgs.nResolution.ToString();
            textBoxSkipBottom.Text = m_oTracerParam.m_oServerEventArgs.nSkipBottom.ToString();
            textBoxMode.Text = m_oTracerParam.m_oServerEventArgs.strMode;
            textBoxThreshold.Text = m_oTracerParam.m_oServerEventArgs.nThreshold.ToString();
            textBoxPointsPerCircle.Text = m_oTracerParam.m_oServerEventArgs.nPointsPerCircle.ToString();
            textBoxOrder.Text = m_oTracerParam.m_oServerEventArgs.strOrder;

            textBoxResponse.Text = m_oTracerParam.m_oServerEventArgs.strResponse;
            textBoxMime.Text = m_oTracerParam.m_oServerEventArgs.strMime;
            textBoxCode.Text = m_oTracerParam.m_oServerEventArgs.nCode.ToString();

            textBoxOutput.Text = m_oTracerParam.m_strOutput;

            textBoxError.Text = m_oTracerParam.m_strError;
            textBoxStackTrace.Text = m_oTracerParam.m_strStackTrace;

            Size oSize = m_oTracerParam.m_oShapeAreaXY.Size;
            int nSize = m_oTracerParam.m_oTiler.nResolution - 256;
            if (nSize < oSize.Width)
            {
                nSize = oSize.Width;
            }
            if (nSize < oSize.Height)
            {
                nSize = oSize.Height;
            }
            Point oCenter = new Point(m_oTracerParam.m_oShapeAreaXY.Location.X + (oSize.Width - nSize) / 2, m_oTracerParam.m_oShapeAreaXY.Location.Y + (oSize.Height - nSize) / 2);
            m_oRectangleAll = new Rectangle(oCenter, new Size(nSize, nSize));
            m_oRectangleAll.Inflate(128, 128);

            //if (GetAvailableMemory() - 100000000 > (m_oRectangleAll.Width * m_oRectangleAll.Height * 4))
            //{
            //    GetBitmapAll();
            //}
            m_oBitmapResultSmal = new Bitmap(512, 512);
            m_oBitmapResultPart = new Bitmap(512, 512);
            m_oBitmapHiliteSmal = new Bitmap(512, 512);
            m_oBitmapAll = new Bitmap(512, 512);

            int i = 0;
            foreach (string str in m_oTracerParam.m_listBorderStepsName)
            {
                listBoxSteps.Items.Add("" + i + " " + str);
                i++;
            }
            if (listBoxSteps.Items.Count > 0)
            {
                listBoxSteps.SetSelected(listBoxSteps.Items.Count - 1, true);
            }
            if (listBoxParts.Items.Count > 0)
            {
                listBoxParts.SetSelected(0, true);
            }

            checkBoxShowBitmap.Checked = m_oBitmapOverview != null;

            buttonColorData.BackColor = m_oColorData;
            buttonColorHilite.BackColor = m_oColorHilite;
            buttonColorArea.BackColor = m_oColorArea;
            buttonColorTile.BackColor = m_oColorTile;

            comboBoxZoom.SelectedIndex = 2;
        }

        private void FormDetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_oBitmapOverview != null)
            {
                m_oBitmapOverview.Dispose();
                m_oBitmapOverview = null;
            }
            if (m_oBitmapOverviewSmal != null)
            {
                m_oBitmapOverviewSmal.Dispose();
                m_oBitmapOverviewSmal = null;
            }
            if (m_oBitmapResultSmal != null)
            {
                m_oBitmapResultSmal.Dispose();
                m_oBitmapResultSmal = null;
            }
            if (m_oBitmapHiliteSmal != null)
            {
                m_oBitmapHiliteSmal.Dispose();
                m_oBitmapHiliteSmal = null;
            }
            if (m_oBitmapAll != null)
            {
                m_oBitmapAll.Dispose();
                m_oBitmapAll = null;
            }
            GC.Collect();

            FormMain.RemoveFormDetails(this);
        }

        private static long GetAvailableMemory()
        {
            long availableMemory;

            try
            {
                using (PerformanceCounter perfCounter = new PerformanceCounter("Memory", "Available Bytes"))
                {
                    availableMemory = Convert.ToInt64(perfCounter.NextValue());
                    return availableMemory;
                }
            }
            catch
            {
            }

            try
            {
                ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
                ManagementObjectCollection results = searcher.Get();

                foreach (ManagementObject result in results)
                {
                    availableMemory = Convert.ToInt64(result["FreePhysicalMemory"]);
                    return availableMemory * 1024;
                }
            }
            catch
            {
            }
            return 0;
        }

        private void GetBitmapAll()
        {
            m_oBitmapOverview = new Bitmap(m_oRectangleAll.Width, m_oRectangleAll.Height);
            List<TileBitmap> listTileBitmap = m_oTracerParam.m_oFileCache.listTileBitmap;

            Graphics g = Graphics.FromImage(m_oBitmapOverview);
            foreach (TileBitmap oTB in listTileBitmap)
            {
                Rectangle oRect = new Rectangle(oTB.m_oTile.oFirstPointXY, new Size(oTB.m_oTile.nResolution, oTB.m_oTile.nResolution));
                int posX = oRect.Left - m_oRectangleAll.Left;
                int posY = m_oRectangleAll.Bottom - oRect.Top - oTB.m_oTile.nResolution + 1;

                Rectangle oR = new Rectangle(oRect.X - oTB.m_oTile.oFirstPointXY.X, oRect.Y - oTB.m_oTile.oFirstPointXY.Y, oRect.Width, oRect.Height);

                try
                {
                    Bitmap oBitmap = new Bitmap(oTB.m_strFileName);
                    g.DrawImage(oBitmap, posX, posY, oR, GraphicsUnit.Pixel);
                }
                catch
                {
                }
            }
            g.Dispose();
            m_oBitmapOverviewSmal = ResizeImage(m_oBitmapOverview, 512, 512);
        }

        private void GetBitmapAllLine()
        {
            Graphics gs = null;
            Graphics gp = null;

            Pen oPen = new Pen(m_oColorData);
            oPen.Width = 1;

            m_oBitmapResultPart.MakeTransparent();
            gp = Graphics.FromImage(m_oBitmapResultPart);
            gp.Clear(Color.Transparent);
            
            m_oBitmapResultSmal.MakeTransparent();
            gs = Graphics.FromImage(m_oBitmapResultSmal);
            gs.Clear(Color.Transparent);

            if (m_aoBorderPart != null && m_aoBorderPart.Count() > 0)
            {
                Point[] aoPoints;
                PointGeo[] aoPointGeo;
                PointGeo oPGLast = new PointGeo(double.NaN, double.NaN);

                if (checkBoxTileBorder.Checked == true)
                {
                    List<TileBitmap> listTileBitmap = m_oTracerParam.m_oFileCache.listTileBitmap;

                    Pen oPenx = new Pen(m_oColorTile);
                    oPenx.Width = 1;
                    foreach (TileBitmap oTB in listTileBitmap)
                    {
                        Rectangle oRect = new Rectangle(oTB.m_oTile.oFirstPointXY, new Size(oTB.m_oTile.nResolution, oTB.m_oTile.nResolution));
                        DrawRect(oRect, gs, gp, oPenx);
                    }
                }

                aoPointGeo = m_aoBorderPart[m_aoBorderPart.Length - 1].GetGeoPoints();
                if (aoPointGeo != null && aoPointGeo.Count() > 0)
                {
                    oPGLast = aoPointGeo[aoPointGeo.Count() - 1];
                }
                foreach (BorderPart oLine in m_aoBorderPart)
                {
                    aoPoints = oLine.GetPoints();
                    aoPointGeo = oLine.GetGeoPoints();

                    if (checkBoxShowLine.Checked == true)
                    {
                        Point oPLast;
                        if (aoPoints.Length > 0)
                        {
                            oPLast = aoPoints[aoPoints.Length - 1];
                            foreach (Point oP in aoPoints)
                            {
                                DrawLine(oPLast, oP, gs, gp, oPen);
                                oPLast = oP;
                            }
                        }

                        foreach (PointGeo oPG in aoPointGeo)
                        {
                            DrawLine(m_oTracerParam.m_oGeoMap.ConvertGeoToPoint(oPGLast), m_oTracerParam.m_oGeoMap.ConvertGeoToPoint(oPG), gs, gp, oPen);
                            oPGLast = oPG;
                        }
                    }

                    if (checkBoxShowPoint.Checked == true)
                    {
                        foreach (Point oP in aoPoints)
                        {
                            DrawPoint(oP, gs, gp, oPen);
                        }
                        foreach (PointGeo oPG in aoPointGeo)
                        {
                            DrawPoint(m_oTracerParam.m_oGeoMap.ConvertGeoToPoint(oPG), gs, gp, oPen);
                        }
                    }
                }
            }
            gs.Dispose();
            gp.Dispose();
        }

        private void AddBitmapHilite(bool bAdd = true)
        {
            Graphics gs = null;
            Graphics gp = null;

            Pen oPen;
            oPen = new Pen(bAdd ? m_oColorHilite : m_oColorData);
            oPen.Width = 1;

            m_oBitmapResultPart.MakeTransparent();
            gp = Graphics.FromImage(m_oBitmapResultPart);
            //gp.Clear(Color.Transparent);
            
            m_oBitmapHiliteSmal.MakeTransparent();
            gs = Graphics.FromImage(m_oBitmapHiliteSmal);
            gs.Clear(Color.Transparent);

            if (m_oHilitePart != null)
            {
                Point[] aoPoints = ((BorderPart)m_oHilitePart).GetPoints();
                PointGeo[] aoPointsGeo = ((BorderPart)m_oHilitePart).GetGeoPoints();

                if (checkBoxShowLine.Checked == true)
                {
                    Point oPLast;
                    if (aoPoints.Length > 0)
                    {
                        oPLast = aoPoints[aoPoints.Length - 1];
                        foreach (Point oP in aoPoints)
                        {
                            DrawLine(oPLast, oP, gs, gp, oPen);
                            oPLast = oP;
                        }
                    }
                }

                if (checkBoxShowPoint.Checked == true)
                {
                    foreach (Point oP in aoPoints)
                    {
                        DrawPoint(oP, gs, gp, oPen);
                    }
                    foreach (PointGeo oPG in aoPointsGeo)
                    {
                        DrawPoint(m_oTracerParam.m_oGeoMap.ConvertGeoToPoint(oPG), gs, gp, oPen);
                    }
                }
            }

            oPen = new Pen(m_oColorArea);
            oPen.Width = 1;

            DrawRect(m_oRectanglePart, gs, null, oPen);

            gs.Dispose();
            gp.Dispose();

            ShowAll();
        }

        private void DrawPoint(Point oPoint, Graphics gs, Graphics gp, Pen oPen)
        {
            Rectangle oRect;

            if (gs != null)
            {
                oRect = new Rectangle((oPoint.X - m_oRectangleAll.Left) * 512 / m_oRectangleAll.Width - 2, (m_oRectangleAll.Bottom - oPoint.Y) * 512 / m_oRectangleAll.Height - 2, 4, 4);
                gs.DrawRectangle(oPen, oRect);
            }
            if (gp != null)
            {
                oRect = new Rectangle(256 + (int)((oPoint.X - m_oPointCenterPart.X) * m_dZoom) - 2, 256 + (int)((m_oPointCenterPart.Y - oPoint.Y) * m_dZoom) - 2, 4, 4);
                gp.DrawRectangle(oPen, oRect);
            }
        }

        private void DrawLine(Point oPointStart, Point oPointEnd, Graphics gs, Graphics gp, Pen oPen)
        {
            Point oP1;
            Point oP2;

            if (gs != null)
            {
                oP1 = new Point((oPointStart.X - m_oRectangleAll.Left) * 512 / m_oRectangleAll.Width, (m_oRectangleAll.Bottom - oPointStart.Y) * 512 / m_oRectangleAll.Height);
                oP2 = new Point((oPointEnd.X - m_oRectangleAll.Left) * 512 / m_oRectangleAll.Width, (m_oRectangleAll.Bottom - oPointEnd.Y) * 512 / m_oRectangleAll.Height);
                gs.DrawLine(oPen, oP1, oP2);
            }
            if (gp != null)
            {
                oP1 = new Point(256 + (int)((oPointStart.X - m_oPointCenterPart.X) * m_dZoom), 256 + (int)((m_oPointCenterPart.Y - oPointStart.Y) * m_dZoom));
                oP2 = new Point(256 + (int)((oPointEnd.X - m_oPointCenterPart.X) * m_dZoom), 256 + (int)((m_oPointCenterPart.Y - oPointEnd.Y) * m_dZoom));
                gp.DrawLine(oPen, oP1, oP2);
            }
        }

        private void DrawRect(Rectangle oRectangle, Graphics gs, Graphics gp, Pen oPen)
        {
            Point oP1 = new Point(oRectangle.Left + oRectangle.Width, oRectangle.Top);
            Point oP2 = new Point(oRectangle.Left, oRectangle.Top + oRectangle.Height);
            Point oP3 = new Point(oRectangle.Left + oRectangle.Width, oRectangle.Top + oRectangle.Height);
            DrawLine(oRectangle.Location, oP1, gs, gp, oPen);
            DrawLine(oRectangle.Location, oP2, gs, gp, oPen);
            DrawLine(oP1, oP3, gs, gp, oPen);
            DrawLine(oP2, oP3, gs, gp, oPen);
        }

        private void ShowAll()
        {
            Rectangle oR = new Rectangle(0, 0, 512, 512);
            Graphics g = Graphics.FromImage(m_oBitmapAll);
            g.Clear(Color.Black);
            if (m_oBitmapOverviewSmal != null)
            {
                g.DrawImage(m_oBitmapOverviewSmal, 0, 0, oR, GraphicsUnit.Pixel);
            }
            g.DrawImage(m_oBitmapResultSmal, 0, 0, oR, GraphicsUnit.Pixel);
            g.DrawImage(m_oBitmapHiliteSmal, 0, 0, oR, GraphicsUnit.Pixel);
            g.Dispose();
            pictureBox1.Image = m_oBitmapAll;
        }

        private void ShowPart()
        {
            Rectangle oRect;
            m_oBitmapPart = new Bitmap(512, 512);
            Graphics g = Graphics.FromImage(m_oBitmapPart);
            g.Clear(Color.Black);
            oRect = new Rectangle(m_oRectanglePart.X - m_oRectangleAll.X, m_oRectangleAll.Bottom - m_oRectanglePart.Bottom, m_oRectanglePart.Width, m_oRectanglePart.Height);
            if (m_oBitmapOverview != null)
            {
                Bitmap oBitmapPart = new Bitmap((int)(512 / m_dZoom), (int)(512 / m_dZoom));
                Graphics gp = Graphics.FromImage(oBitmapPart);
                gp.DrawImage(m_oBitmapOverview, 0, 0, oRect, GraphicsUnit.Pixel);
                g.DrawImage(ResizeImage(oBitmapPart, 512, 512), 0, 0, new Rectangle(0, 0, 512, 512), GraphicsUnit.Pixel);
            }
            g.DrawImage(m_oBitmapResultPart, 0, 0, new Rectangle(0, 0, 512, 512), GraphicsUnit.Pixel);
            g.Dispose();

            pictureBox2.Image = m_oBitmapPart;
        }

        public static System.Drawing.Bitmap ResizeImage(System.Drawing.Bitmap value, int newWidth, int newHeight)
        {
            System.Drawing.Bitmap resizedImage = new System.Drawing.Bitmap(newWidth, newHeight);
            System.Drawing.Graphics.FromImage((System.Drawing.Image)resizedImage).DrawImage(value, 0, 0, newWidth, newHeight);
            return (resizedImage);
        }

        private void SetHilitePart(Object oHilitePart)
        {
            if (m_oHilitePart != null)
            {
                AddBitmapHilite(false);
                m_oHilitePart = null;
            }
            if (oHilitePart != null)
            {
                m_oHilitePart = oHilitePart;
                AddBitmapHilite();
            }
        }

        private void listBoxSteps_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            int nPos = listBoxSteps.SelectedIndex;
            if (nPos >= 0)
            {
                int i = 0;
                SetHilitePart(null);
                listBoxParts.Items.Clear();
                if (nPos < m_oTracerParam.m_listBorderSteps.Count)
                {
                    foreach (BorderPart oLine in m_oTracerParam.m_listBorderSteps[nPos])
                    {
                        listBoxParts.Items.Add("" + i + " " + oLine.GetTypName() + " " + oLine.ToString());
                        i++;
                    }
                    m_aoBorderPart = m_oTracerParam.m_listBorderSteps.ElementAt(nPos);
                    GetBitmapAllLine();
                    AddBitmapHilite();
                }
                ShowPart();
            }

            this.Cursor = Cursors.Default;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            m_oPointCenterPart = new Point(m_oRectangleAll.Left + m_oRectangleAll.Width * e.X / 512, m_oRectangleAll.Bottom - m_oRectangleAll.Height * e.Y / 512);
            SetRectanglePart();
            GetBitmapAllLine();
            AddBitmapHilite();
            ShowPart();

            this.Cursor = Cursors.Default;
        }

        private void listBoxParts_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            int nPos = listBoxSteps.SelectedIndex;

            if (nPos >= 0)
            {
                Point oPoint = new Point();
                if (nPos < m_oTracerParam.m_listBorderSteps.Count)
                {
                    BorderPart[] aoLine = m_oTracerParam.m_listBorderSteps[nPos];
                    int nPosPart = listBoxParts.SelectedIndex;
                    if (nPosPart >= 0 && nPosPart < aoLine.Length)
                    {
                        BorderPart oLine = aoLine[nPosPart];
                        SetHilitePart(oLine);
                        if (oLine.GetType() == typeof(BorderPartPointGeo))
                        {
                            oPoint = m_oTracerParam.m_oGeoMap.ConvertGeoToPoint(oLine.GetGeoPoints()[0]);
                        }
                        else
                        {
                            oPoint = oLine.GetCenterPoint();
                        }
                    }
                    else
                    {
                        SetHilitePart(null);
                    }
                }
                else
                {
                    SetHilitePart(null);
                }
                if (m_oHilitePart != null)
                {
                    m_oPointCenterPart = new Point(oPoint.X, oPoint.Y);
                    SetRectanglePart();
                }
                GetBitmapAllLine();
                AddBitmapHilite();
                ShowPart();
            }

            this.Cursor = Cursors.Default;
        }

        private void ShowMessageMemory(long lFree)
        {
            string strMessage;
            string strCaption;

            strCaption = "Not enough memory.";
            strMessage = "Not enough memory for this operation. \nFree = " +
                (lFree / 1000000).ToString() + "MByte \nRequired = " +
                (((m_oRectangleAll.Width * m_oRectangleAll.Height * 4) + 100000000) / 1000000).ToString() + "MByte";

            MessageBox.Show(strMessage, strCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void checkBoxShowBitmap_CheckedChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            if (checkBoxShowBitmap.Checked)
            {
                if (m_oBitmapOverview == null)
                {
                    long lMemory = GetAvailableMemory();
                    if (lMemory - 100000000 > (m_oRectangleAll.Width * m_oRectangleAll.Height * 4))
                    {
                        GetBitmapAll();
                        ShowAll();
                        ShowPart();
                    }
                    else
                    {
                        ShowMessageMemory(lMemory);
                        checkBoxShowBitmap.Checked = false;
                    }
                }
            }
            else
            {
                if (m_oBitmapOverview != null)
                {
                    m_oBitmapOverview.Dispose();
                    m_oBitmapOverview = null;

                    if (m_oBitmapOverviewSmal != null)
                    {
                        m_oBitmapOverviewSmal.Dispose();
                        m_oBitmapOverviewSmal = null;
                    }
                    GC.Collect();
                    ShowAll();
                    ShowPart();
                }
            }

            this.Cursor = Cursors.Default;
        }

        private void checkBoxTileBorder_CheckedChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            GetBitmapAllLine();
            AddBitmapHilite();
            ShowAll();
            ShowPart();

            this.Cursor = Cursors.Default;
        }

        private void checkBoxShowLine_CheckedChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            GetBitmapAllLine();
            AddBitmapHilite();
            ShowAll();
            ShowPart();

            this.Cursor = Cursors.Default;
        }

        private void checkBoxShowPoint_CheckedChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            GetBitmapAllLine();
            AddBitmapHilite();
            ShowAll();
            ShowPart();

            this.Cursor = Cursors.Default;
        }

        private void buttonColorHilite_Click(object sender, EventArgs e)
        {
            ColorDialog oColorDialog = new ColorDialog();
            oColorDialog.FullOpen = true;
            oColorDialog.Color = m_oColorHilite;
            if (oColorDialog.ShowDialog() == DialogResult.OK)
            {
                m_oColorHilite = oColorDialog.Color;
                buttonColorHilite.BackColor = oColorDialog.Color;

                this.Cursor = Cursors.WaitCursor;

                GetBitmapAllLine();
                AddBitmapHilite();
                ShowAll();
                ShowPart();

                this.Cursor = Cursors.Default;
            }
        }

        private void buttonColorArea_Click(object sender, EventArgs e)
        {
            ColorDialog oColorDialog = new ColorDialog();
            oColorDialog.FullOpen = true;
            oColorDialog.Color = m_oColorArea;
            if (oColorDialog.ShowDialog() == DialogResult.OK)
            {
                m_oColorArea = oColorDialog.Color;
                buttonColorArea.BackColor = oColorDialog.Color;

                this.Cursor = Cursors.WaitCursor;

                GetBitmapAllLine();
                AddBitmapHilite();
                ShowAll();
                ShowPart();

                this.Cursor = Cursors.Default;
            }
        }

        private void buttonColorTile_Click(object sender, EventArgs e)
        {
            ColorDialog oColorDialog = new ColorDialog();
            oColorDialog.FullOpen = true;
            oColorDialog.Color = m_oColorTile;
            if (oColorDialog.ShowDialog() == DialogResult.OK)
            {
                m_oColorTile = oColorDialog.Color;
                buttonColorTile.BackColor = oColorDialog.Color;

                this.Cursor = Cursors.WaitCursor;

                GetBitmapAllLine();
                AddBitmapHilite();
                ShowAll();
                ShowPart();

                this.Cursor = Cursors.Default;
            }
        }

        private void buttonColorData_Click(object sender, EventArgs e)
        {
            ColorDialog oColorDialog = new ColorDialog();
            oColorDialog.FullOpen = true;
            oColorDialog.Color = m_oColorData;
            if (oColorDialog.ShowDialog() == DialogResult.OK)
            {
                m_oColorData = oColorDialog.Color;
                buttonColorData.BackColor = oColorDialog.Color;

                this.Cursor = Cursors.WaitCursor;

                GetBitmapAllLine();
                AddBitmapHilite();
                ShowAll();
                ShowPart();

                this.Cursor = Cursors.Default;
            }
        }

        private void comboBoxZoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            int nPos = comboBoxZoom.SelectedIndex;
            double[] adZoom = new double[] { 4, 2, 1, 0.5, 0.25 };

            if (nPos >= 0 && nPos < adZoom.Length)
            {
                double dZoom = adZoom[nPos];
                if (dZoom != m_dZoom)
                {
                    m_dZoom = dZoom;
                    SetRectanglePart();

                    GetBitmapAllLine();
                    AddBitmapHilite();
                    ShowPart();
                }
            }

            this.Cursor = Cursors.Default;
        }

        private void SetRectanglePart()
        {
            m_oRectanglePart = new Rectangle(m_oPointCenterPart.X - (int)(256 / m_dZoom), m_oPointCenterPart.Y - (int)(256 / m_dZoom), (int)(512 / m_dZoom), (int)(512 / m_dZoom));
        }

    }

}
