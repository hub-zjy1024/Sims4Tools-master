/***************************************************************************
 *  Copyright (C) 2009, 2016 by the Sims 4 Tools development team          *
 *                                                                         *
 *  Contributors:                                                          *
 *  Peter Jones                                                            *
 *  Keyi Zhang                                                             *
 *  Cmar                                                                   *
 *  Buzzler                                                                *
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

namespace StblResource
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using s4pi.Interfaces;

    public class StringEntry : AHandlerElement, IEquatable<StringEntry>
    {
        private uint keyHash;
        private byte flags;
        private string stringValue;

        public StringEntry(int apiVersion, EventHandler handler)
            : base(apiVersion, handler)
        {
        }

        public StringEntry(int apiVersion, EventHandler handler, Stream s)
            : base(apiVersion, handler)
        {
            this.Parse(s);
            this.UpdateEntrySize();
        }

        internal uint EntrySize { get; private set; }

        #region AHandlerElement Members

        public override int RecommendedApiVersion
        {
            get { return StblResource.recommendedApiVersion; }
        }

        public override List<string> ContentFields
        {
            get { return GetContentFields(this.requestedApiVersion, this.GetType()); }
        }

        public override AHandlerElement Clone(EventHandler handler)
        {
            StringEntry clone = new StringEntry(this.RecommendedApiVersion, handler)
                                {
                                    keyHash = this.keyHash,
                                    flags = this.flags,
                                    stringValue = this.stringValue
                                };
            return clone;
        }

        #endregion

        #region Data I/O

        public void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            this.keyHash = r.ReadUInt32();
            this.flags = r.ReadByte();
            ushort length = r.ReadUInt16();
            this.stringValue = Encoding.UTF8.GetString(r.ReadBytes(length));
        }

        public void UnParse(Stream s)
        {
            BinaryWriter w = new BinaryWriter(s);
            w.Write(this.keyHash);
            w.Write(this.flags);
            byte[] str = Encoding.UTF8.GetBytes(this.StringValue);
            w.Write((ushort)str.Length);
            w.Write(str);
        }

        #endregion

        public bool Equals(StringEntry other)
        {
            return this.keyHash == other.keyHash && this.flags == other.flags
                   && string.CompareOrdinal(this.StringValue, other.StringValue) == 0;
        }

        [ElementPriority(0)]
        public uint KeyHash
        {
            get { return this.keyHash; }
            set
            {
                if (this.keyHash != value)
                {
                    this.OnElementChanged();
                    this.keyHash = value;
                }
            }
        }

        [ElementPriority(1)]
        public byte Flags
        {
            get { return this.flags; }
            set
            {
                if (this.flags != value)
                {
                    this.OnElementChanged();
                    this.flags = value;
                }
            }
        }

        [ElementPriority(2)]
        public string StringValue
        {
            get { return this.stringValue ?? string.Empty; }
            set
            {
                if (string.CompareOrdinal(this.StringValue, value) != 0)
                {
                    this.OnElementChanged();
                    this.stringValue = value;
                    this.UpdateEntrySize();
                }
            }
        }

        public string Value
        {
            get
            {
                return string.Format("Key 0x{0:X8}, Flags 0x{1:X2} : {2}",
                    this.keyHash,
                    this.flags,
                    this.StringValue);
            }
        }

        private void UpdateEntrySize()
        {
            this.EntrySize = (uint)Encoding.UTF8.GetByteCount(this.StringValue);
        }
    }
}