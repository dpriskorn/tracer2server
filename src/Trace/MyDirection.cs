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

namespace Tracer2Server.Trace
{
    public enum DIRECTION_XY
    {
        UP,
        UP_RIGHT,
        RIGHT,
        DOWN_RIGHT,
        DOWN,
        DOWN_LEFT,
        LEFT,
        UP_LEFT,
        None
    }


    class MyDirection
    {
        static public int[] m_aoDirX = new int[8] { 0, 1, 1, 1, 0, -1, -1, -1 };
        static public int[] m_aoDirY = new int[8] { 1, 1, 0, -1, -1, -1, 0, 1 };

        static public DIRECTION_XY[,] m_aoDirXY = new DIRECTION_XY[3, 3] { 
                {DIRECTION_XY.UP_LEFT, DIRECTION_XY.UP, DIRECTION_XY.UP_RIGHT},
                {DIRECTION_XY.LEFT, DIRECTION_XY.None, DIRECTION_XY.RIGHT}, 
                {DIRECTION_XY.DOWN_LEFT, DIRECTION_XY.DOWN, DIRECTION_XY.DOWN_RIGHT}
        };

        static public DIRECTION_XY GetDirection(Point oPoint)
        {
            int x = oPoint.X;
            int y = oPoint.Y;

            if (x < -1) x = -1;
            if (x > 1) x = 1;
            if (y < -1) y = -1;
            if (y > 1) y = 1;

            return m_aoDirXY[y + 1, x + 1];
        }

        static public Point GetDirection(DIRECTION_XY oDir)
        {
            return new Point(m_aoDirX[(int)oDir], m_aoDirY[(int)oDir]);
        }

        static public int GetRotation(Point oDirStart, Point oDirEnd)
        {
            int nStart = (int)GetDirection(oDirStart);
            int nEnd = (int)GetDirection(oDirEnd);
            int nRet = nEnd - nStart;

            if (nRet < -4) nRet += 8;
            if (nRet > 4) nRet -= 8;

            return nRet;
        }

    }

}
