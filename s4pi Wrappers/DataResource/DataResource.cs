/***************************************************************************
 *  Copyright (C) 2014, 2016 by the Sims 4 Tools development team          *
 *                                                                         *
 *  Contributors:                                                          *
 *  ChaosMageX                                                             *
 *  granthes                                                               *
 *  Keyi Zhang, kz005@bucknell.edu                                         *
 *  Buzzler                                                                *
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

namespace s4pi.DataResource
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;
    using s4pi.Interfaces;
    using s4pi.Settings;
    using FieldDataTypeFlags = s4pi.DataResource.DataResourceFlags.FieldDataTypeFlags;

    public class DataResource : AResource
    {
        internal const uint NullOffset = 0x80000000;

        private const int recommendedApiVersion = 1;
        private static bool checking = Settings.Checking;

        #region Attributes

        private uint version = 0x100;
        private uint dataTablePosition;
        private uint structureTablePosition;
        private StructureList structureList;
        private DataList dataList;
        private byte[] rawData;

        #endregion

        #region Constructors

        public DataResource(int apiVersion, Stream s)
            : base(apiVersion, s)
        {
            if (this.stream == null)
            {
                this.stream = this.UnParse();
                this.OnResourceChanged(this, EventArgs.Empty);
            }
            this.stream.Position = 0;
            this.Parse(this.stream);
        }

        #endregion

        #region AResource

        public override int RecommendedApiVersion
        {
            get { return recommendedApiVersion; }
        }

        public override List<string> ContentFields
        {
            get { return GetContentFields(this.requestedApiVersion, this.GetType()); }
        }

        public void Parse(Stream s)
        {
            BinaryReader reader = new BinaryReader(s);

            uint magic = reader.ReadUInt32();

            if (checking)
            {
                if (magic != FOURCC("DATA"))
                {
                    throw new InvalidDataException(
                        string.Format("Expected magic tag 0x{0:X8}; read 0x{1:X8}; position 0x{2:X8}",
                            FOURCC("DATA"),
                            magic,
                            s.Position));
                }
            }

            this.version = reader.ReadUInt32();

            if (!reader.GetOffset(out this.dataTablePosition))
            {
                string message = string.Format("Invalid Data Table Position: 0x{0:X8}", this.dataTablePosition);
                throw new InvalidDataException(message);
            }

            int dataCount = reader.ReadInt32();
            if (!reader.GetOffset(out this.structureTablePosition))
            {
                string message = string.Format("Invalid Structure Table Position: 0x{0:X8}", this.StructureTablePosition);
                throw new InvalidDataException(message);
            }

            int structureCount = reader.ReadInt32();

            // Structure table
            this.structureList = new StructureList(this.OnResourceChanged,
                this.structureTablePosition,
                structureCount,
                reader);

            this.dataList = new DataList(this.OnResourceChanged, this, dataCount, reader);

            s.Position = 0;
            this.rawData = reader.ReadBytes((int)s.Length);
        }

        private const uint blank = 0;

        protected override Stream UnParse()
        {
            return new MemoryStream(this.rawData);
            Stream s = new MemoryStream();
            BinaryWriter w = new BinaryWriter(s);

            if (this.structureList == null)
            {
                this.structureList = new StructureList(this.OnResourceChanged);
            }
            if (this.dataList == null)
            {
                this.dataList = new DataList(this.OnResourceChanged, this);
            }

            // Write Header with blank offsets
            w.Write((uint)FOURCC("DATA"));
            w.Write(this.version);
            w.Write(blank); // data table offset
            w.Write(this.dataList.Count);
            w.Write(blank); // structure table offset
            w.Write(this.structureList.Count);

            // Padding between header and data table?
            // Need more information
            //Util.Padding(w);
            w.Write(blank);
            w.Write(blank);


            // Write Data Table with blank Name and Structure offsets
            this.dataTablePosition = (uint)s.Position;
            this.dataList.UnParse(w);

            // Write Structure Table blank Name offsets
            this.structureTablePosition = (uint)s.Position;
            this.structureList.UnParse(w);


            // Write Names and set Name positions in data
            foreach (Structure structure in this.structureList)
            {
                foreach (Field field in structure.FieldTable)
                {
                    if (string.IsNullOrEmpty(field.Name))
                    {
                        field.NamePosition = NullOffset;
                    }
                    else
                    {
                        field.NamePosition = (uint)s.Position;
                        w.WriteAsciiString(field.Name);
                    }
                }
                if (string.IsNullOrEmpty(structure.Name))
                {
                    structure.NamePosition = NullOffset;
                }
                else
                {
                    structure.NamePosition = (uint)s.Position;
                    w.WriteAsciiString(structure.Name);
                }
            }
            foreach (Data data in this.dataList)
            {
                if (string.IsNullOrEmpty(data.Name))
                {
                    data.NamePosition = NullOffset;
                }
                else
                {
                    data.NamePosition = (uint)s.Position;
                    w.WriteAsciiString(data.Name);
                }
            }

            // Go back, calculate and write offsets

            this.structureList.WriteOffsets(w);
            this.dataList.WriteOffsets(w);

            // write the data table offset
            w.BaseStream.Position = 0x08;
            w.Write(this.dataTablePosition - w.BaseStream.Position);
            w.BaseStream.Position = 16;
            w.Write(this.structureTablePosition - w.BaseStream.Position); // dirty hack

            return s;
        }

        #endregion

        #region Content Fields

        [ElementPriority(1)]
        public uint Version
        {
            get { return this.version; }
            set
            {
                if (this.version != value)
                {
                    this.version = value;
                    this.OnResourceChanged(this, EventArgs.Empty);
                }
            }
        }

        [ElementPriority(2)]
        public uint DataTablePosition
        {
            get { return this.dataTablePosition; }
            set
            {
                if (this.dataTablePosition != value)
                {
                    this.dataTablePosition = value;
                    this.OnResourceChanged(this, EventArgs.Empty);
                }
            }
        }

        [ElementPriority(3)]
        public uint StructureTablePosition
        {
            get { return this.structureTablePosition; }
            set
            {
                if (this.structureTablePosition != value)
                {
                    this.structureTablePosition = value;
                    this.OnResourceChanged(this, EventArgs.Empty);
                }
            }
        }

        [ElementPriority(4)]
        public StructureList StructureTable
        {
            get { return this.structureList; }
            set
            {
                if (this.structureList != value)
                {
                    this.structureList = value == null
                        ? new StructureList(this.OnResourceChanged)
                        : new StructureList(this.OnResourceChanged, value);
                    this.OnResourceChanged(this, EventArgs.Empty);
                }
            }
        }

        [ElementPriority(5)]
        public DataList DataTable
        {
            get { return this.dataList; }
            set
            {
                if (this.dataList != value)
                {
                    this.dataList = value == null
                        ? new DataList(this.OnResourceChanged, this)
                        : new DataList(this.OnResourceChanged, this, value);
                    this.OnResourceChanged(this, EventArgs.Empty);
                }
            }
        }

        #endregion

        public string Value
        {
            get { return this.ValueBuilder; }
        }

        #region Nested Types

        public class Structure : AHandlerElement, IEquatable<Structure>
        {
            private const int recommendedApiVersion = 1;

            private uint myPosition;

            #region Attributes

            private uint namePosition;
            private string name;
            private uint nameHash;
            private uint unknown08;
            private uint size;
            private uint fieldTablePosition;
            private uint fieldCount;
            private FieldList fieldList;

            #endregion

            #region Constructors

            public Structure(int apiVersion, EventHandler handler) : base(apiVersion, handler)
            {
            }

            public Structure(int apiVersion, EventHandler handler, BinaryReader r) : base(apiVersion, handler)
            {
                this.Parse(r);
            }

            #endregion

            #region Data I/O

            private void Parse(BinaryReader r)
            {
                this.myPosition = (uint)r.BaseStream.Position;

                r.GetOffset(out this.namePosition);

                this.name = r.GetAsciiString(this.namePosition);
                this.nameHash = r.ReadUInt32();
                this.unknown08 = r.ReadUInt32();
                this.size = r.ReadUInt32();
                r.GetOffset(out this.fieldTablePosition);
                this.fieldCount = r.ReadUInt32();
            }

            internal void ParseFieldTable(BinaryReader r)
            {
                if (this.fieldTablePosition == NullOffset)
                {
                    this.fieldList = new FieldList(this.handler);
                }
                else
                {
                    r.BaseStream.Position = this.fieldTablePosition;
                    this.fieldList = new FieldList(this.handler, this.fieldCount, r);
                }
            }

            public void UnParse(BinaryWriter w)
            {
                this.myPosition = (uint)w.BaseStream.Position;

                if (this.name == null)
                {
                    this.name = "";
                }
                this.nameHash = FNV32.GetHash(this.name);

                if (this.fieldList == null)
                {
                    this.fieldList = new FieldList(this.handler);
                }
                if (this.fieldList.Count == 0)
                {
                    this.fieldTablePosition = NullOffset;
                }

                w.Write((uint)0); // Name Offset
                w.Write(this.nameHash);
                w.Write(this.unknown08);
                w.Write(this.size);
                w.Write((uint)0); // Field Table Offset
                w.Write(this.fieldList.Count);
            }

            public void WriteOffsets(BinaryWriter w)
            {
                Stream s = w.BaseStream;
                s.Position = this.myPosition;

                w.Write(this.namePosition == NullOffset
                    ? this.namePosition
                    : this.namePosition - this.myPosition);

                s.Position += 12; // Name Hash, Unknown 08, and Size

                w.Write(this.fieldTablePosition == NullOffset
                    ? this.fieldTablePosition
                    : this.fieldTablePosition - this.myPosition - 0x10);

                this.fieldList.WriteOffsets(w);
            }

            #endregion

            #region AHandlerElement

            public override int RecommendedApiVersion
            {
                get { return recommendedApiVersion; }
            }

            public override List<string> ContentFields
            {
                get { return GetContentFields(this.requestedApiVersion, this.GetType()); }
            }

            #endregion

            #region IEquatable<Structure>

            public bool Equals(Structure other)
            {
                return this.name == other.name && this.nameHash == other.nameHash && this.unknown08 == other.unknown08
                       && this.size == other.size && this.fieldTablePosition == other.fieldTablePosition
                       && this.fieldList.Equals(other.fieldList);
            }

            public override bool Equals(object obj)
            {
                return obj is Structure && this.Equals(obj as Structure);
            }

            public override int GetHashCode()
            {
                return this.namePosition.GetHashCode() ^ this.name.GetHashCode() ^ this.nameHash.GetHashCode()
                       ^ this.unknown08.GetHashCode()
                       ^ this.size.GetHashCode() ^ this.fieldTablePosition.GetHashCode();
            }

            #endregion

            public uint GetPosition()
            {
                return this.myPosition;
            }

            #region Content Fields

            [ElementPriority(1)]
            public uint NamePosition
            {
                get { return this.namePosition; }
                set
                {
                    if (this.namePosition != value)
                    {
                        this.namePosition = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(2)]
            public string Name
            {
                get { return this.name; }
                set
                {
                    if (this.name != value)
                    {
                        this.name = value == null ? "" : value;
                        this.nameHash = FNV32.GetHash(this.name);
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(3)]
            public uint NameHash
            {
                get { return this.nameHash; }
                set
                {
                    if (this.nameHash != value)
                    {
                        this.nameHash = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(4)]
            public uint Unknown8
            {
                get { return this.unknown08; }
                set
                {
                    if (this.unknown08 != value)
                    {
                        this.unknown08 = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(5)]
            public uint Size
            {
                get { return this.size; }
                set
                {
                    if (this.size != value)
                    {
                        this.size = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(6)]
            public uint FieldTablePosition
            {
                get { return this.fieldTablePosition; }
                set
                {
                    if (this.fieldTablePosition != value)
                    {
                        this.fieldTablePosition = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(8)]
            public FieldList FieldTable
            {
                get { return this.fieldList; }
                set
                {
                    if (this.fieldList != value)
                    {
                        this.fieldList = value == null
                            ? new FieldList(this.handler)
                            : new FieldList(this.handler, value);
                        this.OnElementChanged();
                    }
                }
            }

            #endregion

            public string Value
            {
                get { return this.ValueBuilder; }
            }
        }

        public class StructureList : DependentList<Structure>
        {
            public StructureList(EventHandler handler) : base(handler)
            {
            }

            public StructureList(EventHandler handler, uint structureTablePosition, int structureCount, BinaryReader r)
                : base(handler)
            {
                this.elementHandler = handler;
                this.Capacity = structureCount;

                int i;
                Structure structure;
                Stream s = r.BaseStream;
                s.Position = structureTablePosition;
                for (i = 0; i < structureCount; i++)
                {
                    structure = new Structure(0, this.elementHandler, r);
                    this.Add(structure);
                }
                for (i = 0; i < structureCount; i++)
                {
                    structure = this[i];
                    structure.ParseFieldTable(r);
                }
            }

            public StructureList(EventHandler handler, IEnumerable<Structure> ilt) : base(handler, ilt)
            {
            }

            #region Data I/O

            public void UnParse(BinaryWriter w)
            {
                int i;
                int count = this.Count;
                Structure structure;
                // Write the structures
                for (i = 0; i < count; i++)
                {
                    structure = this[i];
                    structure.UnParse(w);
                }
                // Write the field tables
                Stream s = w.BaseStream;
                for (i = 0; i < count; i++)
                {
                    structure = this[i];
                    structure.FieldTablePosition = (uint)s.Position;
                    structure.FieldTable.UnParse(w);
                }
            }

            public void WriteOffsets(BinaryWriter w)
            {
                Structure structure;
                int count = this.Count;
                for (int i = 0; i < count; i++)
                {
                    structure = this[i];
                    structure.WriteOffsets(w);
                }
            }

            #endregion

            public Structure GetFromPosition(uint position)
            {
                if (position == NullOffset)
                {
                    return null;
                }
                for (int i = this.Count - 1; i >= 0; i--)
                {
                    var structure = this[i];
                    if (structure.GetPosition() == position)
                    {
                        return structure;
                    }
                }
                return null;
            }

            protected override Structure CreateElement(Stream s)
            {
                throw new NotImplementedException();
            }

            protected override void WriteElement(Stream s, Structure element)
            {
                throw new NotImplementedException();
            }
        }

        public class Field : AHandlerElement, IEquatable<Field>
        {
            private uint myPosition;

            #region Attributes

            private uint namePosition;
            private string name;
            private uint nameHash;
            private FieldDataTypeFlags dataType;
            private uint dataOffset;
            private uint positionOffset;

            #endregion

            #region Constructors

            public Field(int apiVersion, EventHandler handler) : base(apiVersion, handler)
            {
            }

            public Field(int apiVersion, EventHandler handler, BinaryReader r) : base(apiVersion, handler)
            {
                this.Parse(r);
            }

            #endregion

            #region Data I/O

            private void Parse(BinaryReader r)
            {
                this.myPosition = (uint)r.BaseStream.Position;

                r.GetOffset(out this.namePosition);

                this.name = r.GetAsciiString(this.namePosition);
                this.nameHash = r.ReadUInt32();
                this.dataType = (FieldDataTypeFlags)r.ReadUInt32();
                this.dataOffset = r.ReadUInt32();
                r.GetOffset(out this.positionOffset);
            }

            public void UnParse(BinaryWriter w)
            {
                this.myPosition = (uint)w.BaseStream.Position;

                if (this.name == null)
                {
                    this.name = "";
                }
                this.nameHash = FNV32.GetHash(this.name);

                w.Write((uint)0); // Name Offset
                w.Write(this.nameHash);
                w.Write((uint)this.dataType);
                w.Write(this.dataOffset);
                w.Write((uint)0); // Unknown 10 Offset
            }

            public void WriteOffsets(BinaryWriter w)
            {
                Stream s = w.BaseStream;
                s.Position = this.myPosition;

                w.Write(this.namePosition == NullOffset
                    ? this.namePosition
                    : this.namePosition - this.myPosition);
                s.Position += 12; // Name Hash, DataType, and Data Offset

                w.Write(this.positionOffset == NullOffset
                    ? this.positionOffset
                    : this.positionOffset - this.myPosition - 0x10);
            }

            #endregion

            #region AHandlerElement

            public override int RecommendedApiVersion
            {
                get { return recommendedApiVersion; }
            }

            public override List<string> ContentFields
            {
                get { return GetContentFields(this.requestedApiVersion, this.GetType()); }
            }

            #endregion

            #region IEquatable<Field>

            public bool Equals(Field other)
            {
                return this.name == other.name && this.nameHash == other.nameHash && this.dataType == other.dataType
                       && this.dataOffset == other.dataOffset && this.positionOffset == other.positionOffset;
            }

            public override bool Equals(object obj)
            {
                return obj is Field && this.Equals(obj as Field);
            }

            public override int GetHashCode()
            {
                return this.name.GetHashCode() ^ this.nameHash.GetHashCode() ^ this.dataType.GetHashCode()
                       ^ this.dataOffset.GetHashCode() ^ this.positionOffset.GetHashCode();
            }

            #endregion

            #region Content Fields

            [ElementPriority(1)]
            public uint NamePosition
            {
                get { return this.namePosition; }
                set
                {
                    if (this.namePosition != value)
                    {
                        this.namePosition = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(2)]
            public string Name
            {
                get { return this.name; }
                set
                {
                    if (this.name != value)
                    {
                        this.name = value == null ? "" : value;
                        this.nameHash = FNV32.GetHash(this.name);
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(3)]
            public uint NameHash
            {
                get { return this.nameHash; }
                set
                {
                    if (this.nameHash != value)
                    {
                        this.nameHash = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(4)]
            public FieldDataTypeFlags DataType
            {
                get { return this.dataType; }
                set
                {
                    if (this.dataType != value)
                    {
                        this.dataType = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(5)]
            public uint DataOffset
            {
                get { return this.dataOffset; }
                set
                {
                    if (this.dataOffset != value)
                    {
                        this.dataOffset = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(6)]
            public uint PositionOffset
            {
                get { return this.positionOffset; }
                set
                {
                    if (this.positionOffset != value)
                    {
                        this.positionOffset = value;
                        this.OnElementChanged();
                    }
                }
            }

            #endregion

            public string Value
            {
                get { return this.ValueBuilder; }
            }
        }

        public class FieldList : DependentList<Field>, IEquatable<FieldList>
        {
            public FieldList(EventHandler handler) : base(handler)
            {
            }

            public FieldList(EventHandler handler, uint fieldCount, BinaryReader r)
                : base(handler)
            {
                this.elementHandler = handler;
                this.Capacity = (int)fieldCount;

                for (int i = 0; i < fieldCount; i++)
                {
                    this.Add(new Field(0, this.elementHandler, r));
                }
            }

            public FieldList(EventHandler handler, IEnumerable<Field> ilt) : base(handler, ilt)
            {
            }

            #region Data I/O

            public void UnParse(BinaryWriter w)
            {
                foreach (Field field in this)
                {
                    field.UnParse(w);
                }
            }

            public void WriteOffsets(BinaryWriter w)
            {
                foreach (Field field in this)
                {
                    field.WriteOffsets(w);
                }
            }

            #endregion

            protected override Field CreateElement(Stream s)
            {
                throw new NotImplementedException();
            }

            protected override void WriteElement(Stream s, Field element)
            {
                throw new NotImplementedException();
            }

            public bool Equals(FieldList other)
            {
                if (other == null || this.Count != other.Count)
                {
                    return false;
                }
                for (int i = this.Count - 1; i >= 0; i--)
                {
                    if (!this[i].Equals(other[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public class Data : AHandlerElement, IEquatable<Data>
        {
            private uint myPosition;

            private DataResource owner;

            #region Attributes

            private uint namePosition;
            private string name;
            private uint nameHash;
            private uint structurePosition;
            private Structure structure;
            private FieldDataTypeFlags dataType;
            private uint fieldSize;
            private uint fieldPosition;
            private uint fieldCount;
            private bool isNull;
            private DataBlobHandler fieldData;

            #endregion

            #region Constructors

            public Data(int apiVersion, EventHandler handler, DataResource owner) : base(apiVersion, handler)
            {
                this.owner = owner;
            }

            public Data(int apiVersion, EventHandler handler, DataResource owner, BinaryReader r)
                : base(apiVersion, handler)
            {
                this.owner = owner;
                this.Parse(r);
            }

            #endregion

            #region Data I/O

            private void Parse(BinaryReader r)
            {
                this.myPosition = (uint)r.BaseStream.Position;

                r.GetOffset(out this.namePosition);

                this.name = r.GetAsciiString(this.namePosition);
                this.nameHash = r.ReadUInt32();
                if (r.GetOffset(out this.structurePosition))
                {
                    this.structure = this.owner.structureList.GetFromPosition(this.structurePosition);
                    if (this.structure == null)
                    {
                        throw new InvalidDataException(string.Format("Invalid Structure Position: 0x{0:X8}",
                            this.structurePosition));
                    }
                }
                else
                {
                    this.structure = null;
                }
                this.dataType = (FieldDataTypeFlags)r.ReadUInt32();
                this.fieldSize = r.ReadUInt32();
                if (!r.GetOffset(out this.fieldPosition))
                {
                    throw new InvalidDataException("Invalid Field Offset: 0x80000000");
                }
                this.fieldCount = r.ReadUInt32();
            }

            internal void ParseFieldData(uint length, Stream s)
            {
                s.Position = this.fieldPosition;
                this.fieldData = new DataBlobHandler(this.requestedApiVersion, this.handler, length, s);
            }

            public void UnParse(BinaryWriter w)
            {
                this.myPosition = (uint)w.BaseStream.Position;

                if (this.name == null)
                {
                    this.name = "";
                }
                this.nameHash = FNV32.GetHash(this.name);

                w.Write((uint)0); // Name Offset
                w.Write(this.nameHash);
                w.Write((uint)0); // Structure Offset
                w.Write((uint)this.dataType);
                w.Write(this.fieldSize);
                w.Write((uint)0); // Field Offset
                w.Write(this.fieldCount);
            }

            public void WriteOffsets(BinaryWriter w)
            {
                Stream s = w.BaseStream;
                s.Position = this.myPosition;

                w.Write(this.namePosition == NullOffset
                    ? this.namePosition
                    : this.namePosition - this.myPosition);
                s.Position += 4; // Name Hash

                if (this.structure == null)
                {
                    this.structurePosition = NullOffset;
                    w.Write(this.structurePosition);
                }
                else
                {
                    this.structurePosition = this.structure.GetPosition();
                    w.Write(this.structurePosition - this.myPosition - 0x08);
                }
                s.Position += 8; // data dataType and field size

                w.Write(this.fieldPosition == NullOffset
                    ? this.fieldPosition
                    : this.fieldPosition - this.myPosition - 0x14);
            }

            #endregion

            #region AHandlerElement

            public override int RecommendedApiVersion
            {
                get { return recommendedApiVersion; }
            }

            public override List<string> ContentFields
            {
                get { return GetContentFields(this.requestedApiVersion, this.GetType()); }
            }

            #endregion

            #region IEquatable<Data>

            public bool Equals(Data other)
            {
                return this.namePosition == other.namePosition && this.name == other.name
                       && this.nameHash == other.nameHash
                       && this.structurePosition == other.structurePosition && this.dataType == other.dataType
                       && this.fieldSize == other.fieldSize
                       && this.fieldPosition == other.fieldPosition && this.fieldCount == other.fieldCount
                       && this.isNull == other.isNull;
            }

            public override bool Equals(object obj)
            {
                return obj is Data && this.Equals((Data)obj);
            }

            public override int GetHashCode()
            {
                return this.namePosition.GetHashCode() ^ this.name.GetHashCode() ^ this.nameHash.GetHashCode()
                       ^ this.structurePosition.GetHashCode() ^ this.dataType.GetHashCode()
                       ^ this.fieldSize.GetHashCode()
                       ^ this.fieldPosition.GetHashCode() ^ this.fieldCount.GetHashCode() ^ this.isNull.GetHashCode();
            }

            #endregion

            #region Content Fields

            [ElementPriority(1)]
            public uint NamePosition
            {
                get { return this.namePosition; }
                set
                {
                    if (this.namePosition != value)
                    {
                        this.namePosition = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(2)]
            public string Name
            {
                get { return this.name; }
                set
                {
                    if (this.name != value)
                    {
                        this.name = value == null ? "" : value;
                        this.nameHash = FNV32.GetHash(this.name);
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(3)]
            public uint NameHash
            {
                get { return this.nameHash; }
                set
                {
                    if (this.nameHash != value)
                    {
                        this.nameHash = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(4)]
            public uint StructurePosition
            {
                get { return this.structurePosition; }
                set
                {
                    if (this.structurePosition != value)
                    {
                        this.structurePosition = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(5)]
            public FieldDataTypeFlags DataType
            {
                get 
                {
                    return this.dataType; 
                }
                set
                {
                    if (this.dataType != value)
                    {
                        this.dataType = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(6)]
            public uint FieldSize
            {
                get { return this.fieldSize; }
                set
                {
                    if (this.fieldSize != value)
                    {
                        this.fieldSize = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(7)]
            public uint FieldPosition
            {
                get { return this.fieldPosition; }
                set
                {
                    if (this.fieldPosition != value)
                    {
                        this.fieldPosition = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(8)]
            public uint FieldCount
            {
                get { return this.fieldCount; }
                set
                {
                    if (this.fieldCount != value)
                    {
                        this.fieldCount = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(9)]
            public bool IsNull
            {
                get { return this.isNull; }
                set
                {
                    if (this.isNull != value)
                    {
                        this.isNull = value;
                        this.OnElementChanged();
                    }
                }
            }

            [ElementPriority(10)]
            public DataBlobHandler FieldData
            {
                get { return this.fieldData; }
                set
                {
                    if (value == null)
                    {
                        throw new InvalidOperationException();
                    }
                    if (this.fieldData != value)
                    {
                        this.fieldData = new DataBlobHandler(this.requestedApiVersion, this.handler, value);
                        this.OnElementChanged();
                    }
                }
            }

            #endregion

            public string Value
            {
                get { return this.ValueBuilder; }
            }
        }

        public class DataList : DependentList<Data>
        {
            private class FieldPosComparer : IComparer<Data>
            {
                public static readonly FieldPosComparer Singleton = new FieldPosComparer();

                public FieldPosComparer()
                {
                }

                public int Compare(Data x, Data y)
                {
                    return (int)x.FieldPosition - (int)y.FieldPosition;
                }
            }

            private List<Data> fieldPosSorted;

            private DataResource owner;

            public DataList(EventHandler handler, DataResource owner) : base(handler)
            {
                this.owner = owner;
            }

            public DataList(EventHandler handler, DataResource owner, int dataCount, BinaryReader r)
                : base(handler)
            {
                this.elementHandler = handler;
                this.owner = owner;
                this.Capacity = dataCount;

                Stream s = r.BaseStream;
                int i;
                //long savedPos = s.Position;
                Data data;
                this.fieldPosSorted = new List<Data>(dataCount);
                s.Position = owner.dataTablePosition;
                for (i = 0; i < dataCount; i++)
                {
                    data = new Data(0, this.elementHandler, owner, r);
                    this.Add(data);
                    this.fieldPosSorted.Add(data);
                }

                // Go back and read the actual data
                this.fieldPosSorted.Sort(FieldPosComparer.Singleton);
                for (i = 0; i < dataCount; i++)
                {
                    data = this.fieldPosSorted[i];
                    var length = i < dataCount - 1
                        ? this.fieldPosSorted[i + 1].FieldPosition - data.FieldPosition
                        : owner.structureTablePosition - data.FieldPosition;
                    data.ParseFieldData(length, s);
                }
            }

            public DataList(EventHandler handler, DataResource owner, IEnumerable<Data> ilt) : base(handler, ilt)
            {
                this.owner = owner;
            }

            #region Data I/O

            public void UnParse(BinaryWriter writer)
            {
                int i;
                int count = this.Count;
                long previousPosition = writer.BaseStream.Position;

                // Write the headers 
                for (i = 0; i < count; i++)
                {
                    this[i].UnParse(writer);
                }

                Debug.WriteLine(writer.BaseStream.Position - previousPosition);

                // Padding between headers and data?
                // Note: this quick fix is still problematic for big files
                // The padding need to be fully explored
                //w.Write(Util.Zero32);
                Debug.WriteLine(writer.BaseStream.Position.ToString("X16"));
                writer.WriteZeroBytes(16 - (int)(writer.BaseStream.Position - previousPosition) % 16);


                // Write the data
                Stream s = writer.BaseStream;
                for (i = 0; i < count; i++)
                {
                    Data data = this[i];
                    data.FieldPosition = (uint)s.Position;
                    data.FieldData.UnParse(s);
                }
            }

            public void WriteOffsets(BinaryWriter w)
            {
                foreach (Data data in this)
                {
                    data.WriteOffsets(w);
                }
            }

            #endregion

            protected override Data CreateElement(Stream s)
            {
                throw new NotImplementedException();
            }

            protected override void WriteElement(Stream s, Data element)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }

    public class DataResourceHandler : AResourceHandler
    {
        public DataResourceHandler()
        {
            this.Add(typeof(DataResource), new List<string>(new string[] { "0x545AC67A", "0x02D5DF13" }));
        }
    }
}