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
using Tracer2Server.Projection;

namespace Tracer2Server.Trace
{
    class BorderPartPointGeo : BorderPart
    {
        public PointGeo m_oPG;

        public BorderPartPointGeo(PointGeo oPG)
        {
            m_oPG = oPG;
        }

        public override BorderPart ShallowCopy()
        {
            return (BorderPart)this.MemberwiseClone();
        }

        public override String GetTypName()
        {
            return "PointGeo";
        }

        public override PointGeo[] GetGeoPoints()
        {
            PointGeo[] aoPointGeo = new PointGeo[1];
            aoPointGeo[0] = m_oPG;
            return aoPointGeo;
        }

        public override string ToString()
        {
            return m_oPG.ToString();
        }

    }

}
