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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Tracer2Server.WebServer;
using Tracer2Server.Trace;
using System.Windows.Forms;

namespace Tracer2Server.Tiles
{
    class TileCache
    {
        static private string s_strCacheprefix;

        private TracerParam m_oTracerParam;
        private string m_strCachePath = null;
        private List<TileBitmap> m_listTileBitmap = new List<TileBitmap>();
        private Object m_oLock = new Object();


        static TileCache()
        {
            if (OperationSystem.isWindows)
            {
                s_strCacheprefix = Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + System.IO.Path.DirectorySeparatorChar + "Tracer2Server" + System.IO.Path.DirectorySeparatorChar + "cache" + System.IO.Path.DirectorySeparatorChar;
            }
            else
            {
                s_strCacheprefix = Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + System.IO.Path.DirectorySeparatorChar + "Tracer2Server" + System.IO.Path.DirectorySeparatorChar + "cache" + System.IO.Path.DirectorySeparatorChar;
            }

            DirectoryInfo diCache = new DirectoryInfo(s_strCacheprefix);
            try
            {
                if (diCache.Exists)
                {
                    foreach (DirectoryInfo di in diCache.GetDirectories())
                    {
                        DeleteDir(di);
                    }
                    return;
                }
                else
                {
                    diCache.Create();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }
        }

        static bool DeleteDir(DirectoryInfo oDirectoryInfo)
        {
            bool bDeleteDir = true;

            try
            {
                foreach (DirectoryInfo oSubDirectoryInfo in oDirectoryInfo.GetDirectories())
                {
                    if (DeleteDir(oSubDirectoryInfo) == false)
                    {
                        bDeleteDir = false;
                    }
                }
                foreach (FileInfo oFileInfo in oDirectoryInfo.GetFiles())
                {
                    if (oFileInfo.CreationTime < DateTime.Now.AddMonths(-1))
                    {
                        oFileInfo.Delete();
                    }
                    else
                    {
                        bDeleteDir = false;
                    }
                }
                if (bDeleteDir == true)
                {
                    oDirectoryInfo.Delete(false);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return bDeleteDir;
        }


        public TileCache(TracerParam oTracerParam)
        {
            m_oTracerParam = oTracerParam;

            ServerEventArgs oServerEventArgs = oTracerParam.m_oServerEventArgs;
            int nHashCode = (oServerEventArgs.strUrl + oServerEventArgs.dTileSize + oServerEventArgs.nResolution).GetHashCode();
            string strPath = GetValidFileName(nHashCode.ToString("X"));
            m_strCachePath = s_strCacheprefix + strPath + System.IO.Path.DirectorySeparatorChar;
            Directory.CreateDirectory(m_strCachePath);
        }

        public List<TileBitmap> listTileBitmap
        {
            get
            {
                List<TileBitmap> oList = new List<TileBitmap>();
                lock (m_oLock)
                {
                    foreach (TileBitmap oTB in m_listTileBitmap)
                    {
                        oList.Add(oTB);
                    }
                }
                return oList;
            }
        }

        public void ClearAllData()
        {
            lock (m_oLock)
            {
                foreach (TileBitmap oTB in m_listTileBitmap)
                {
                    oTB.ClearData();
                }
            }
        }

        public string GetValidFileName(string strFilename)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"[{0}]+", invalidChars);

            var reservedWords = new[] {
                "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
                "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
                "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            };

            var sanitisedNamePart = Regex.Replace(strFilename, invalidReStr, "_");
            foreach (var reservedWord in reservedWords)
            {
                var reservedWordPattern = string.Format("^{0}\\.", reservedWord);
                sanitisedNamePart = Regex.Replace(sanitisedNamePart, reservedWordPattern, "_reservedWord_.", RegexOptions.IgnoreCase);
            }
            return sanitisedNamePart;
        }

        private string GetFileName(Tile oTile)
        {
            NumberFormatInfo oNFIDefault = System.Globalization.NumberFormatInfo.InvariantInfo;
            return m_strCachePath +
                oTile.oRectangle.dLeft.ToString("F4", oNFIDefault) + "_" +
                oTile.oRectangle.dBottom.ToString("F4", oNFIDefault) + "_" +
                oTile.oRectangle.dRight.ToString("F4", oNFIDefault) + "_" +
                oTile.oRectangle.dTop.ToString("F4", oNFIDefault) + ".BMP";
        }

        private TileBitmap GetCacheBitmap(Tile oTile)
        {
            string strFileName = GetFileName(oTile);
            TileBitmap oTileBitmap = null;

            lock (m_oLock)
            {
                foreach (TileBitmap oTB in m_listTileBitmap)
                {
                    if (oTB.m_strFileName.Equals(strFileName))
                    {
                        oTileBitmap = oTB;
                        break;
                    }
                }
            }
            return oTileBitmap;
        }

        public byte[][] GetTile(Tile oTile)
        {
            TileBitmap oNewTB;

            lock (m_oLock)
            {
                Tile[] aoTile = m_oTracerParam.m_oTiler.GetTilesByTile(oTile);

                if (GetCacheBitmap(oTile) == null)
                {
                    oNewTB = new TileBitmap(oTile, GetFileName(oTile), m_oTracerParam);
                    m_listTileBitmap.Add(oNewTB);
                }
                foreach (Tile oT in aoTile)
                {
                    if (oT != null && GetCacheBitmap(oT) == null)
                    {
                        Thread.Sleep(10);
                        oNewTB = new TileBitmap(oT, GetFileName(oT), m_oTracerParam);
                        m_listTileBitmap.Add(oNewTB);
                    }
                }
            }
            byte[][] aucData = GetCacheBitmap(oTile).GetData();
            if (aucData == null)
            {
                throw new Exception("Can't load Tile: " + oTile.ToString());
            }
            return aucData;
        }

    }

}
