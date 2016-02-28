/***************************************************************************
 *  Copyright (C) 2014, 2016 by the Sims 4 Tools development team          *
 *                                                                         *
 *  Contributors:                                                          *
 *  Keyi Zhang, kz005@bucknell.edu                                         *
 *  pbox (using info from velocitygrass)                                   *
 *                                                                         *
 *  This file is part of the Sims 4 Package Interface (s4pi)               *
 *                                                                         *
 *  s4pi is free software: you can redistribute it and/or modify           *
 *  it under the terms of the GNU General Public License as published by   *
 *  the Free Software Foundation, either version 3 of the License, or      *
 *  (at your option) any later version.                                    *
 *                                                                         *
 *  s4pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s4pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace s4pi.DataResource
{
    public static class DataResourceFlags
    {
        public enum FieldDataTypeFlags : uint
        {
            Boolean = 0x00000000,
            String = 0x00000001, // only used as dataType in data tables which are referenced by fields of dataType 0x0000000B
            Int16 = 0x00000006,
            Tag = 0x00000007,
            TagValue = 0x00000008,
            CasModifierInstance = 0x00000009,
            Float = 0x0000000A,
            StringOffset = 0x0000000B,
            ModeName = 0x0000000C,
            DataOffset = 0x0000000D,
            DataOffsetList = 0x0000000E,
            TwoFloats = 0x0000000F,
            RGBColor = 0x00000010,
            ARGBColor = 0x00000011,
            Instance = 0x00000012,
            TGI = 0x00000013,
            StringKeyHash = 0x00000014
        }

        public static Dictionary<FieldDataTypeFlags, int> DataSizeTable
        {
            get
            {
                return new Dictionary<FieldDataTypeFlags, int>()
                {
                    {FieldDataTypeFlags.Boolean , 4},
                    {FieldDataTypeFlags.Int16 , 4},
                    {FieldDataTypeFlags.Tag , 4},
                    {FieldDataTypeFlags.TagValue , 8},
                    {FieldDataTypeFlags.CasModifierInstance , 8},
                    {FieldDataTypeFlags.Float , 4},
                    {FieldDataTypeFlags.StringOffset , 8},
                    {FieldDataTypeFlags.ModeName , 8},
                    {FieldDataTypeFlags.DataOffset, 4},
                    {FieldDataTypeFlags.DataOffsetList, 8},
                    {FieldDataTypeFlags.TwoFloats, 8},
                    {FieldDataTypeFlags.RGBColor, 12},
                    {FieldDataTypeFlags.ARGBColor, 16},
                    {FieldDataTypeFlags.Instance, 8},
                    {FieldDataTypeFlags.TGI, 16},
                    {FieldDataTypeFlags.StringKeyHash, 4}
                };
            }
        }
    }
}