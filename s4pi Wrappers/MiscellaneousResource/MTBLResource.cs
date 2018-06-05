/***************************************************************************
 *  Copyright (C) 2014, 2016 by the Sims 4 Tools development team          *
 *                                                                         *
 *  Contributors:                                                          *
 *  Inge Jones                                                             *
 *  pbox                                                                   *
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

using s4pi.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace s4pi.Miscellaneous
{
    public class MTBLResource : AResource
    {
        const int recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

        static bool checking = s4pi.Settings.Settings.Checking;

        #region Attributes
        uint version;
        MTBLEntryList entryList;
        #endregion

        public MTBLResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, EventArgs.Empty); } stream.Position = 0; Parse(stream); }

        public class MTBLEntryList : DependentList<MTBLEntry>
        {
                public MTBLEntryList(EventHandler handler, long maxSize = -1)
                    : base(handler, maxSize)
                {
                }

                public MTBLEntryList(EventHandler handler, IEnumerable<MTBLEntry> ilt, long maxSize = -1)
                    : base(handler, ilt, maxSize)
                {
                }

                public MTBLEntryList(EventHandler handler, Stream s, long maxSize = -1)
                    : base(handler, s, maxSize)
                {
                }

                protected override MTBLEntry CreateElement(Stream s)
                {
                    return new MTBLEntry(recommendedApiVersion, this.elementHandler, s);
                }

                protected override void WriteElement(Stream s, MTBLEntry element)
                {
                    element.UnParse(s);
                }

        }

        public class MTBLEntry : AHandlerElement, IEquatable<MTBLEntry>
        {
            long offset = 0;
            ulong modelIID = 0x0000000000000000;
            ulong baseFileNameHash = 0x0000000000000000;
            WidthAndMappingFlags widthAndMappingFlags;
            byte minimumWallHeight;
            byte numberOfLevels;
            byte unused;
            float thumbnailBoundsMinX;
            float thumbnailBoundsMinZ;
            float thumbnailBoundsMinY;
            float thumbnailBoundsMaxX;
            float thumbnailBoundsMaxZ;
            float thumbnailBoundsMaxY;
            ModelFlags modelFlags;
            ulong vfxHash = 0x0000000000000000;

            public MTBLEntry(int apiVersion, EventHandler handler, MTBLEntry other)
                : this(apiVersion, handler, other.modelIID, other.baseFileNameHash, other.widthAndMappingFlags, other.minimumWallHeight, other.numberOfLevels, other.unused, other.thumbnailBoundsMinX, other.thumbnailBoundsMinZ, other.thumbnailBoundsMinY, other.thumbnailBoundsMaxX, other.thumbnailBoundsMaxZ, other.thumbnailBoundsMaxY, other.modelFlags, other.vfxHash)
            {
            }
            public MTBLEntry(int apiVersion, EventHandler handler)
                : base(apiVersion, handler)
            {
            }
            public MTBLEntry(int apiVersion, EventHandler handler, Stream s)
                : this(apiVersion, handler)
            {
                this.Parse(s);
            }
            public MTBLEntry(int apiVersion, EventHandler handler, ulong modelIID, ulong baseFileNameHash, WidthAndMappingFlags widthAndMappingFlags, byte minimumWallHeight, byte numberOfLevels, byte unused, float thumbnailBoundsMinX, float thumbnailBoundsMinZ, float thumbnailBoundsMinY, float thumbnailBoundsMaxX, float thumbnailBoundsMaxZ, float thumbnailBoundsMaxY, ModelFlags modelFlags, ulong vfxHash)
                : base(apiVersion, handler)
            {
                this.modelIID = modelIID;
                this.baseFileNameHash = baseFileNameHash;
                this.widthAndMappingFlags = widthAndMappingFlags;
                this.minimumWallHeight = minimumWallHeight;
                this.numberOfLevels = numberOfLevels;
                this.unused = unused;
                this.thumbnailBoundsMinX = thumbnailBoundsMinX;
                this.thumbnailBoundsMinZ = thumbnailBoundsMinZ;
                this.thumbnailBoundsMinY = thumbnailBoundsMinY;
                this.thumbnailBoundsMaxX = thumbnailBoundsMaxX;
                this.thumbnailBoundsMaxZ = thumbnailBoundsMaxZ;
                this.thumbnailBoundsMaxY = thumbnailBoundsMaxY;
                this.modelFlags = modelFlags;
                this.vfxHash = vfxHash;
            }

            [ElementPriority(0)]
            public long Offset
            {
                get { return offset; }
            }
            [ElementPriority(1)]
            public ulong ModelIID
            {
                get { return modelIID; }
                set { modelIID = value; OnElementChanged(); }
            }
            [ElementPriority(2)]
            public ulong BaseFileNameHash
            {
                get { return baseFileNameHash; }
                set { this.baseFileNameHash = value; OnElementChanged(); }
            }
            [ElementPriority(3)]
            public WidthAndMappingFlags WidthAndMappingFlags
            {
                get { return widthAndMappingFlags; }
                set { this.widthAndMappingFlags = value; OnElementChanged(); }
            }
            [ElementPriority(4)]
            public byte MinimumWallHeight
            {
                get { return minimumWallHeight; }
                set { this.minimumWallHeight = value; OnElementChanged(); }
            }
            [ElementPriority(5)]
            public byte NumberOfLevels
            {
                get { return numberOfLevels; }
                set { this.numberOfLevels = value; OnElementChanged(); }
            }
            [ElementPriority(6)]
            public byte Unused
            {
                get { return unused; }
                set { this.unused = value; OnElementChanged(); }
            }
            [ElementPriority(7)]
            public float ThumbnailBoundsMinX
            {
                get { return thumbnailBoundsMinX; }
                set { this.thumbnailBoundsMinX = value; OnElementChanged(); }
            }

            [ElementPriority(8)]
            public float ThumbnailBoundsMaxX
            {
                get { return thumbnailBoundsMaxX; }
                set { this.thumbnailBoundsMaxX = value; OnElementChanged(); }
            }
            [ElementPriority(9)]
            public float ThumbnailBoundsMinY
            {
                get { return thumbnailBoundsMinY; }
                set { this.thumbnailBoundsMinY = value; OnElementChanged(); }
            }
            [ElementPriority(10)]
            public float ThumbnailBoundsMaxY
            {
                get { return thumbnailBoundsMaxY; }
                set { this.thumbnailBoundsMaxY = value; OnElementChanged(); }
            }
            [ElementPriority(11)]
            public float ThumbnailBoundsMinZ
            {
                get { return thumbnailBoundsMinZ; }
                set { this.thumbnailBoundsMinZ = value; OnElementChanged(); }
            }
            [ElementPriority(12)]
            public float ThumbnailBoundsMaxZ
            {
                get { return thumbnailBoundsMaxZ; }
                set { this.thumbnailBoundsMaxZ = value; OnElementChanged(); }
            }

            [ElementPriority(13)]
            public ModelFlags ModelFlags
            {
                get { return modelFlags; }
                set { this.modelFlags = value; OnElementChanged(); }
            }

            [ElementPriority(14)]
            public ulong VfxHash
            {
                get { return vfxHash; }
                set { this.vfxHash = value; OnElementChanged(); }
            }

            public override int RecommendedApiVersion
            {
                get { return recommendedApiVersion; }
            }
 
            public override List<string> ContentFields
            {
                get { return GetContentFields(0, GetType()); }
            }
 
            void Parse(Stream s)
            {
                var br = new BinaryReader(s);
                this.offset = s.Position;
                this.modelIID = br.ReadUInt64();
                this.baseFileNameHash = br.ReadUInt64();
                this.widthAndMappingFlags = (WidthAndMappingFlags)br.ReadByte();
                this.minimumWallHeight = br.ReadByte();
                this.numberOfLevels = br.ReadByte();
                this.unused = br.ReadByte();
                this.thumbnailBoundsMinX = br.ReadSingle();
                this.thumbnailBoundsMinZ = br.ReadSingle();
                this.thumbnailBoundsMinY = br.ReadSingle();
                this.thumbnailBoundsMaxX = br.ReadSingle();
                this.thumbnailBoundsMaxZ = br.ReadSingle();
                this.thumbnailBoundsMaxY = br.ReadSingle();
                this.modelFlags = (ModelFlags)br.ReadUInt32();
                this.vfxHash = br.ReadUInt64();

            }
            public string Value { get { return ValueBuilder; } }

            public void UnParse(Stream s)
            {
                var bw = new BinaryWriter(s);
                bw.Write(this.modelIID);
                bw.Write(this.baseFileNameHash);
                bw.Write((byte)this.widthAndMappingFlags);
                bw.Write(this.minimumWallHeight);
                bw.Write(this.numberOfLevels);
                bw.Write(this.unused);
                bw.Write(this.thumbnailBoundsMinX);
                bw.Write(this.thumbnailBoundsMinZ);
                bw.Write(this.thumbnailBoundsMinY);
                bw.Write(this.thumbnailBoundsMaxX);
                bw.Write(this.thumbnailBoundsMaxZ);
                bw.Write(this.thumbnailBoundsMaxY);
                bw.Write((uint)this.modelFlags);
                bw.Write(this.vfxHash);

            }
            public bool Equals(MTBLEntry other)
            {
                return this.modelIID == other.modelIID 
                    && this.baseFileNameHash == other.baseFileNameHash
                    && this.widthAndMappingFlags == other.widthAndMappingFlags
                    && this.minimumWallHeight == other.minimumWallHeight
                    && this.numberOfLevels == other.numberOfLevels
                    && this.unused == other.unused
                    && this.thumbnailBoundsMinX == other.thumbnailBoundsMinX
                    && this.thumbnailBoundsMinZ == other.thumbnailBoundsMinZ
                    && this.thumbnailBoundsMinY == other.thumbnailBoundsMinY
                    && this.thumbnailBoundsMaxX == other.thumbnailBoundsMaxX
                    && this.thumbnailBoundsMaxZ == other.thumbnailBoundsMaxZ
                    && this.thumbnailBoundsMaxY == other.thumbnailBoundsMaxY
                    && this.modelFlags == other.modelFlags
                    && this.vfxHash == other.vfxHash;
            }

        }

        #region Data I/O
        void Parse(Stream s)
        {
            s.Position = 0;
            BinaryReader r = new BinaryReader(s);

            uint magic = r.ReadUInt32();
            if (checking) if (magic != FOURCC("MTBL"))
                    throw new InvalidDataException(String.Format("Expected magic tag 0x{0:X8}; read 0x{1:X8}; position 0x{2:X8}",
                        FOURCC("MTBL"), magic, s.Position));

            version = r.ReadUInt32();
            this.EntryList= new MTBLEntryList(this.OnResourceChanged, s);
        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();

            BinaryWriter w = new BinaryWriter(ms);

            w.Write((uint)FOURCC("MTBL"));

            w.Write(version);

            if (entryList == null)
            {
                entryList = new MTBLEntryList(this.OnResourceChanged);
            }
            this.entryList.UnParse(ms);

            return ms;
        }
        #endregion

        #region Content Fields
        [ElementPriority(1)]
        public uint Version { get { return version; } set { if (version != value) { version = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        public MTBLEntryList EntryList
        {
            get { return entryList; }
            set { entryList = value; this.OnResourceChanged(this, EventArgs.Empty); }
        }
        public string Value { get { return ValueBuilder; } }

        #endregion
    }

    /// <summary>
    /// ResourceHandler for MTBLResource wrapper
    /// </summary>
    public class MTBLResourceHandler : AResourceHandler
    {
        public MTBLResourceHandler()
        {
            this.Add(typeof(MTBLResource), new List<string>(new string[] { "0x81CA1A10", }));
        }
    }
}
