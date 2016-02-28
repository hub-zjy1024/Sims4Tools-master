/***************************************************************************
 *  Copyright (C) 2014, 2016 by the Sims 4 Tools development team          *
 *                                                                         *
 *  Contributors:                                                          *
 *  Keyi Zhang, kz005@bucknell.edu                                         *
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

namespace s4pi.DataResource
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    internal static class Extensions
    {
        public static string GetAsciiString(this BinaryReader reader, long nameOffset)
        {
            if (nameOffset == DataResource.NullOffset)
            {
                return "";
            }
            long startPosition = reader.BaseStream.Position;
            reader.BaseStream.Position = nameOffset;
            List<byte> characters = new List<byte>();
            byte c = reader.ReadByte();
            while (c != 0x00)
            {
                characters.Add(c);
                c = reader.ReadByte();
            }

            reader.BaseStream.Position = startPosition;
            return Encoding.ASCII.GetString(characters.ToArray());
        }

        public static void WriteAsciiString(this BinaryWriter writer, string str)
        {
            byte[] array = new byte[str.Length + 1];
            Encoding.ASCII.GetBytes(str, 0, str.Length, array, 0);
            array[str.Length] = 0;
            writer.Write(array);
        }

        public static bool GetOffset(this BinaryReader reader, out uint offset)
        {
            offset = reader.ReadUInt32();
            if (offset == DataResource.NullOffset)
            {
                return false;
            }
            offset += (uint)reader.BaseStream.Position - 4;

            return true;
        }

        public static void WriteZeroBytes(this BinaryWriter writer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                writer.Write((byte)0);
            }
        }
    }
}