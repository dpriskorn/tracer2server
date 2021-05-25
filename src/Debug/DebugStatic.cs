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

namespace Tracer2Server.Debug
{
    static class DebugStatic
    {
        static private bool s_bDebug = true;
        static private bool s_bDebugBitmap = !true;

        static private DebugWatch s_oDebugWatch = new DebugWatch();
        static private DebugBitmap s_oDebugBitmap = new DebugBitmap();

        static public bool bDebug
        {
            get
            {
                return s_bDebug;
            }
        }
        static public bool bDebugBitmap
        {
            get
            {
                return s_bDebugBitmap;
            }
        }
        static public DebugWatch oDebugWatch
        {
            get
            {
                return s_oDebugWatch;
            }
        }
        static public DebugBitmap oDebugBitmap
        {
            get
            {
                return s_oDebugBitmap;
            }
        }

    }

}
