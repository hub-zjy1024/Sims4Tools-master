/***************************************************************************
 *  Copyright (C) 2017 by Peter L Jones                                    *
 *  pljones@users.sf.net                                                   *
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
using System.IO;
using System.Xml;
using System.Linq;
using s4pi.Interfaces;

namespace DWorldResource
{
    public class DWorldResource : AResource
    {
        static bool checking = s4pi.Settings.Settings.Checking;
        const Int32 recommendedApiVersion = 1;

        #region Attributes
        private OMGSChunk objectManager;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the resource
        /// </summary>
        /// <param name="APIversion">Requested API version</param>
        /// <param name="s">Data stream to use, or null to create from scratch</param>
        public DWorldResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); dirty = true; } stream.Position = 0; Parse(stream); }
        #endregion

        #region Data I/O
        void Parse(Stream s)
        {
            objectManager = (OMGSChunk)TagLengthValue.TagLengthValueFactory(requestedApiVersion, OnResourceChanged, s);
            if (checking) if (!objectManager.tag.Equals((uint)FOURCC("OMGS")))
                    throw new InvalidDataException(String.Format("Unexpected Tag read; expected 'OMGS'; read '{0}'", FOURCC(objectManager.tag)));
        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();

            if (objectManager == null) objectManager = new OMGSChunk(requestedApiVersion, OnResourceChanged);
            objectManager.UnParse(ms);

            ms.Flush();
            return ms;
        }
        #endregion

        #region Sub-types
        public abstract class TagLengthValue : AHandlerElement, IEquatable<TagLengthValue>
        {
            #region Attributes
            internal UInt32 tag;
            #endregion

            #region Constructors
            public TagLengthValue(int apiVersion, EventHandler handler, UInt32 tag) : base(apiVersion, handler) { this.tag = tag; }
            #endregion

            #region Data I/O
            public static TagLengthValue TagLengthValueFactory(int apiVersion, EventHandler handler, Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                UInt32 tag = r.ReadUInt32();
                UInt32 length = r.ReadUInt32();

                TagLengthValue chunk = null;

                long pos = s.Position;
                switch (FOURCC(tag))
                {
                    case "OMGS": chunk = new OMGSChunk(apiVersion, handler, s, length); break;
                    case "OMGR": chunk = new OMGRChunk(apiVersion, handler, s, length); break;
                    case "LOT ": chunk = new LOT_Chunk(apiVersion, handler, s, length); break;
                    case "OBJ ": chunk = new OBJ_Chunk(apiVersion, handler, s, length); break;
                    case "ID  ": chunk = new UInt64Chunk(apiVersion, handler, tag, s); break;
                    case "REFS": chunk = new UnusedChunk(apiVersion, handler, tag, s, length); break;
                    case "SIZX": chunk = new UInt32Chunk(apiVersion, handler, tag, s); break;
                    case "SIZZ": chunk = new UInt32Chunk(apiVersion, handler, tag, s); break;
                    case "POS ": chunk = new VertexChunk(apiVersion, handler, tag, s); break;
                    case "ROT ":
                        if (length == sizeof(Single))
                            chunk = new SingleChunk(apiVersion, handler, tag, s);
                        else if (length == 4 * sizeof(Single))
                            chunk = new QuaternionChunk(apiVersion, handler, tag, s);
                        else
                            throw new InvalidDataException(String.Format("'ROT ' with unknown length: '0x{0:X8}'; at 0x{1:X8}", length, s.Position));
                        break;
                    case "MODL": chunk = new TGIBlockChunk(apiVersion, handler, tag, s); break;
                    case "LEVL": chunk = new Int32Chunk(apiVersion, handler, tag, s); break;
                    case "SCAL": chunk = new SingleChunk(apiVersion, handler, tag, s); break;
                    case "SCRP": chunk = new StringChunk(apiVersion, handler, tag, s, length); break;
                    case "TRES": chunk = new UnusedChunk(apiVersion, handler, tag, s, length); break;
                    case "DEFG": chunk = new UInt32Chunk(apiVersion, handler, tag, s); break;
                    case "DGUD": chunk = new UInt64Chunk(apiVersion, handler, tag, s); break;
                    case "MLOD": chunk = new ByteChunk(apiVersion, handler, tag, s); break;
                    case "PTID": chunk = new UInt64Chunk(apiVersion, handler, tag, s); break;
                    case "SLOT": chunk = new UInt32Chunk(apiVersion, handler, tag, s); break;
                    default:
                        if (checking)
                            throw new InvalidDataException(String.Format("Unknown Tag read: '{0}'; at 0x{1:X8}", FOURCC(tag), s.Position));
                        s.Position += length;
                        break;
                }
                if (checking) if (s.Position != pos + length)
                        throw new InvalidDataException(String.Format("Invalid chunk data length: 0x{0:X8} bytes read; 0x{1:X8} bytes expected; at 0x{2:X8}", s.Position - pos, length, s.Position));

                return chunk;
            }

            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(tag);
                long pos = s.Position;
                w.Write((UInt32)0);

                this.WriteTLVData(s);

                long newPos = s.Position;
                s.Seek(pos, SeekOrigin.Begin);
                w.Write((UInt32)(newPos - pos - sizeof(UInt32)));
                s.Seek(newPos, SeekOrigin.Begin);
            }

            protected abstract void WriteTLVData(Stream s);
            #endregion

            #region IEquatable<TagLengthValue> Members
            public abstract bool Equals(TagLengthValue other);
            public override bool Equals(object obj) { return obj as TagLengthValue != null ? this.Equals(obj as TagLengthValue) : false; }
            public abstract override int GetHashCode();
            #endregion

            #region AApiVersionedFields
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            public string Value { get { return this.ValueBuilder; } }
        }

        public class TLVList<T> : DependentList<T>
            where T : TagLengthValue, IEquatable<T>
        {
            #region Attributes
            UInt32 expectedLength;
            #endregion

            #region Constructors
            public TLVList(EventHandler handler, UInt32 expectedLength = 0)
                : base(handler)
            {
                this.expectedLength = expectedLength;
            }
            public TLVList(EventHandler handler, Stream s, UInt32 expectedLength)
                : this(null, expectedLength)
            {
                elementHandler = handler;
                Parse(s);
                this.handler = handler;
            }
            public TLVList(EventHandler handler, IEnumerable<T> collection)
                : this(null, 0)
            {
                elementHandler = handler;
                this.AddRange(collection);
                this.handler = handler;
            }
            #endregion

            #region Data I/O
            protected override void Parse(Stream s)
            {
                long maxPos = s.Position + expectedLength;
                this.Clear();
                while (s.Position < maxPos)
                    Add((T)TagLengthValue.TagLengthValueFactory(0, handler, s));
            }
            public override void UnParse(Stream s) { foreach (var element in this) element.UnParse(s); }

            protected override T CreateElement(Stream s) { throw new InvalidOperationException(); }
            protected override void WriteElement(Stream s, T element) { throw new InvalidOperationException(); }
            #endregion

            #region AHandlerList<T>
            public T this[String index]
            {
                get
                {
                    int i = this.FindIndex(t => t.tag == (uint)FOURCC(index));
                    return (i < 0) ? null : base[i];
                }
                set
                {
                    int i = this.FindIndex(t => t.tag == (uint)FOURCC(index));
                    if (i < 0)
                    {
                        base.Add(value);
                        OnListChanged();
                    }
                    else
                    {
                        if (!this[i].Equals(value))
                        {
                            base[i] = value;
                            OnListChanged();
                        }
                    }
                }
            }
            #endregion
        }

        public class TLVList : DependentList<TagLengthValue>
        {
            #region Attributes
            UInt32 expectedLength;
            IEnumerable<String> validTags;
            #endregion

            #region Constructors
            public TLVList(EventHandler handler, UInt32 expectedLength = 0, IEnumerable<String> validTags = null)
                : base(handler)
            {
                this.expectedLength = expectedLength;
                this.validTags = validTags;
            }
            public TLVList(EventHandler handler, Stream s, UInt32 expectedLength, IEnumerable<String> validTags = null)
                : this(null, expectedLength, validTags)
            {
                elementHandler = handler;
                Parse(s);
                this.handler = handler;
            }
            public TLVList(EventHandler handler, IEnumerable<TagLengthValue> collection, IEnumerable<String> validTags = null)
                : this(null, 0, validTags)
            {
                elementHandler = handler;
                this.AddRange(collection);
                this.handler = handler;
            }
            #endregion

            #region Data I/O
            protected override void Parse(Stream s)
            {
                long maxPos = s.Position + expectedLength;
                this.Clear();
                while (s.Position < maxPos)
                    Add(TagLengthValue.TagLengthValueFactory(0, handler, s));
            }
            public override void UnParse(Stream s) { foreach (var element in this) element.UnParse(s); }

            protected override TagLengthValue CreateElement(Stream s) { throw new InvalidOperationException(); }
            protected override void WriteElement(Stream s, TagLengthValue element) { throw new InvalidOperationException(); }
            #endregion

            #region DependentList<T>
            public override void Add(TagLengthValue item)
            {
                if (!validTags.Contains(FOURCC(item.tag)))
                    throw new InvalidOperationException(String.Format("Invalid item tag: '{0}'; expected one of: ('{1}')", FOURCC(item.tag), String.Join("', '", validTags)));
                base.Add(item);
            }
            #endregion

            #region AHandlerList<T>
            public TagLengthValue this[String index]
            {
                get
                {
                    int i = this.FindIndex(t => t.tag == (uint)FOURCC(index));
                    return (i < 0) ? null : base[i];
                }
                set
                {
                    if (!validTags.Contains(index))
                        throw new InvalidOperationException(String.Format("Invalid item tag: '{0}'; expected one of: ('{1}')", index, String.Join("', '", validTags)));

                    int i = this.FindIndex(t => t.tag == (uint)FOURCC(index));
                    if (i < 0)
                    {
                        base.Add(value);
                        OnListChanged();
                    }
                    else
                    {
                        if (!this[i].Equals(value))
                        {
                            base[i] = value;
                            OnListChanged();
                        }
                    }
                }
            }
            #endregion
        }

        public class OMGSChunk : TagLengthValue, IEquatable<OMGSChunk>
        {
            private const String _tag = "OMGS";

            #region Attributes
            private TLVList<OMGRChunk> omgsChunks;
            #endregion

            #region Constructors
            public OMGSChunk(int apiVersion, EventHandler handler) : base(apiVersion, handler, (uint)FOURCC(_tag)) { omgsChunks = new TLVList<OMGRChunk>(handler); }
            public OMGSChunk(int apiVersion, EventHandler handler, Stream s, UInt32 expectedLength) : base(apiVersion, handler, (uint)FOURCC(_tag)) { omgsChunks = new TLVList<OMGRChunk>(handler, s, expectedLength); }
            public OMGSChunk(int apiVersion, EventHandler handler, OMGSChunk basis) : base(apiVersion, handler, (uint)FOURCC(_tag)) { omgsChunks = new TLVList<OMGRChunk>(handler, basis.omgsChunks); }
            #endregion

            #region Data I/O
            protected override void WriteTLVData(Stream s) { omgsChunks.UnParse(s); }
            #endregion

            #region IEquatable<OMGSChunk> Members
            public bool Equals(OMGSChunk other) { return tag.Equals(other.tag) && omgsChunks.Equals(other.omgsChunks); }
            public override bool Equals(TagLengthValue other) { return other is OMGSChunk != null && Equals(other as OMGSChunk); }
            public override int GetHashCode() { return tag.GetHashCode() ^ omgsChunks.GetHashCode(); }
            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public TLVList<OMGRChunk> OMGRChunks { get { return omgsChunks; } set { if (!omgsChunks.Equals(value)) { omgsChunks = new TLVList<OMGRChunk>(handler, value); OnElementChanged(); } } }
            #endregion
        }

        public class OMGRChunk : TagLengthValue, IEquatable<OMGRChunk>
        {
            private const String _tag = "OMGR";
            private static IEnumerable<String> validTags = new[] { "ID  ", "LOT ", "OBJ ", "REFS" };

            #region Attributes
            private TLVList omgrChunks;
            #endregion

            #region Constructors
            public OMGRChunk(int apiVersion, EventHandler handler) : base(apiVersion, handler, (uint)FOURCC(_tag)) { omgrChunks = new TLVList(handler, 0, validTags); }
            public OMGRChunk(int apiVersion, EventHandler handler, Stream s, UInt32 expectedLength) : base(apiVersion, handler, (uint)FOURCC(_tag)) { omgrChunks = new TLVList(handler, s, expectedLength, validTags); }
            public OMGRChunk(int apiVersion, EventHandler handler, OMGRChunk basis) : base(apiVersion, handler, (uint)FOURCC(_tag)) { omgrChunks = new TLVList(handler, basis.omgrChunks, validTags); }
            #endregion

            #region Data I/O
            protected override void WriteTLVData(Stream s) { omgrChunks.UnParse(s); }
            #endregion

            #region IEquatable<OMGRChunk> Members
            public bool Equals(OMGRChunk other) { return tag.Equals(other.tag) && omgrChunks.Equals(other.omgrChunks); }
            public override bool Equals(TagLengthValue other) { return other is OMGRChunk != null && Equals(other as OMGRChunk); }
            public override int GetHashCode() { return tag.GetHashCode() ^ omgrChunks.GetHashCode(); }
            #endregion

            #region AApiVersionedFields
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()).Where(tag => !validTags.Contains(tag) || omgrChunks[tag] != null).ToList(); } }
            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public UInt64 ID { get { return ((UInt64Chunk)omgrChunks["ID  "]).Data; } set { if (ID != value) { ((UInt64Chunk)omgrChunks["ID  "]).Data = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            //public LOT_Chunk LOT { get { return ((LOT_Chunk)omgrChunks["LOT "]); } set { if (LOT != value) { omgrChunks["LOT "] = new LOT_Chunk(requestedApiVersion, handler, value); OnElementChanged(); } } }
            public TLVList<LOT_Chunk> LOT
            {
                get
                {
                    IEnumerable<LOT_Chunk> itlv = omgrChunks.FindAll(t => t.tag == (uint)FOURCC("LOT ")).Select(t => (LOT_Chunk)t);
                    return new TLVList<LOT_Chunk>(handler, itlv);
                }
                set
                {
                    if (!LOT.Equals(value))
                    {
                        omgrChunks.RemoveAll(t => t.tag.Equals(FOURCC("LOT ")));
                        value.ForEach(l => omgrChunks.Add(l));
                        OnElementChanged();
                    }
                }
            }
            [ElementPriority(3)]
            public TLVList<OBJ_Chunk> OBJ
            {
                get
                {
                    IEnumerable<OBJ_Chunk> itlv = omgrChunks.FindAll(t => t.tag == (uint)FOURCC("OBJ ")).Select(t => (OBJ_Chunk)t);
                    return new TLVList<OBJ_Chunk>(handler, itlv);
                }
                set
                {
                    if (!OBJ.Equals(value))
                    {
                        omgrChunks.RemoveAll(t => t.tag.Equals(FOURCC("OBJ ")));
                        value.ForEach(o => omgrChunks.Add(o));
                        OnElementChanged();
                    }
                }
            }
            // REFS is "Unused".
            #endregion
        }

        public class LOT_Chunk : TagLengthValue, IEquatable<LOT_Chunk>
        {
            private const String _tag = "LOT ";
            private static IEnumerable<String> validTags = new[] { "SIZX", "SIZZ", "POS ", "ROT " };

            #region Attributes
            private TLVList lotChunks;
            #endregion

            #region Constructors
            public LOT_Chunk(int apiVersion, EventHandler handler) : base(apiVersion, handler, (uint)FOURCC(_tag))
            {
                lotChunks = new TLVList(handler, new TagLengthValue[] {
                    new UInt32Chunk(apiVersion, handler, (uint)FOURCC("SIZX")),
                    new UInt32Chunk(apiVersion, handler, (uint)FOURCC("SIZX")),
                    new VertexChunk(apiVersion, handler, (uint)FOURCC("POS ")),
                    new SingleChunk(apiVersion, handler, (uint)FOURCC("ROT ")),
                }, validTags);
            }
            public LOT_Chunk(int apiVersion, EventHandler handler, Stream s, UInt32 expectedLength) : base(apiVersion, handler, (uint)FOURCC(_tag)) { lotChunks = new TLVList(handler, s, expectedLength, validTags); }
            public LOT_Chunk(int apiVersion, EventHandler handler, LOT_Chunk basis) : base(apiVersion, handler, (uint)FOURCC(_tag)) { lotChunks = new TLVList(handler, basis.lotChunks, validTags); }
            #endregion

            #region Data I/O
            protected override void WriteTLVData(Stream s) { lotChunks.UnParse(s); }
            #endregion

            #region IEquatable<LOT_Chunk> Members
            public bool Equals(LOT_Chunk other) { return tag.Equals(other.tag) && lotChunks.Equals(other.lotChunks); }
            public override bool Equals(TagLengthValue other) { return other is LOT_Chunk != null && Equals(other as LOT_Chunk); }
            public override int GetHashCode() { return tag.GetHashCode() ^ lotChunks.GetHashCode(); }
            #endregion

            #region AApiVersionedFields
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()).Where(tag => !validTags.Contains(tag) || lotChunks[tag] != null).ToList(); } }
            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public UInt32 SIZX { get { return ((UInt32Chunk)lotChunks["SIZX"]).Data; } set { if (SIZX != value) { ((UInt32Chunk)lotChunks["SIZX"]).Data = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public UInt32 SIZZ { get { return ((UInt32Chunk)lotChunks["SIZZ"]).Data; } set { if (SIZZ != value) { ((UInt32Chunk)lotChunks["SIZZ"]).Data = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public Vertex POS { get { return ((VertexChunk)lotChunks["POS "]).Data; } set { if (POS != value) { ((VertexChunk)lotChunks["POS "]).Data = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public Single ROT { get { return ((SingleChunk)lotChunks["ROT "]).Data; } set { if (ROT != value) { ((SingleChunk)lotChunks["ROT "]).Data = value; OnElementChanged(); } } }
            #endregion
        }

        public class OBJ_Chunk : TagLengthValue, IEquatable<OBJ_Chunk>
        {
            private const String _tag = "OBJ ";
            private static IEnumerable<String> validTags = new[] { "ID  ", "POS ", "ROT ", "MODL", "LEVL", "SCAL", "SCRP", "TRES", "DEFG", "DGUD", "MLOD", "PTID", "SLOT" };

            #region Attributes
            private TLVList objChunks;
            #endregion

            #region Constructors
            public OBJ_Chunk(int apiVersion, EventHandler handler)
                : base(apiVersion, handler, (uint)FOURCC(_tag))
            {
                objChunks = new TLVList(handler, new TagLengthValue[] {
                    new UInt64Chunk(apiVersion, handler, (uint)FOURCC("ID  ")),
                    new VertexChunk(apiVersion, handler, (uint)FOURCC("POS ")),
                    new QuaternionChunk(apiVersion, handler, (uint)FOURCC("ROT ")),
                    new TGIBlockChunk(apiVersion, handler, (uint)FOURCC("MODL")),
                    new UInt32Chunk(apiVersion, handler, (uint)FOURCC("SIZX")),
                    new Int32Chunk(apiVersion, handler, (uint)FOURCC("LEVL")),
                    new SingleChunk(apiVersion, handler, (uint)FOURCC("SCAL")),
                    new StringChunk(apiVersion, handler, (uint)FOURCC("SCRP")),
                    new UInt32Chunk(apiVersion, handler, (uint)FOURCC("DEFG")),
                    new UInt64Chunk(apiVersion, handler, (uint)FOURCC("DGUD")),
                    new ByteChunk(apiVersion, handler, (uint)FOURCC("MLOD")),
                    new UInt64Chunk(apiVersion, handler, (uint)FOURCC("PTID")),
                    new UInt32Chunk(apiVersion, handler, (uint)FOURCC("SLOT")),
                }, validTags);
            }
            public OBJ_Chunk(int apiVersion, EventHandler handler, Stream s, UInt32 expectedLength) : base(apiVersion, handler, (uint)FOURCC(_tag)) { objChunks = new TLVList(handler, s, expectedLength, validTags); }
            public OBJ_Chunk(int apiVersion, EventHandler handler, OBJ_Chunk basis) : base(apiVersion, handler, (uint)FOURCC(_tag)) { objChunks = new TLVList(handler, basis.objChunks, validTags); }
            #endregion

            #region Data I/O
            protected override void WriteTLVData(Stream s) { objChunks.UnParse(s); }
            #endregion

            #region IEquatable<OBJ_Chunk> Members
            public bool Equals(OBJ_Chunk other) { return tag.Equals(other.tag) && objChunks.Equals(other.objChunks); }
            public override bool Equals(TagLengthValue other) { return other is OBJ_Chunk != null && Equals(other as OBJ_Chunk); }
            public override int GetHashCode() { return tag.GetHashCode() ^ objChunks.GetHashCode(); }
            #endregion

            #region AApiVersionedFields
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()).Where(tag => !validTags.Contains(tag) || objChunks[tag] != null).ToList(); } }
            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public UInt64 ID { get { return ((UInt64Chunk)objChunks["ID  "]).Data; } set { if (ID != value) { ((UInt64Chunk)objChunks["ID  "]).Data = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public Vertex POS { get { return ((VertexChunk)objChunks["POS "]).Data; } set { if (!POS.Equals(value)) { ((VertexChunk)objChunks["POS "]).Data = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public Quaternion ROT { get { return ((QuaternionChunk)objChunks["ROT "]).Data; } set { if (!ROT.Equals(value)) { ((QuaternionChunk)objChunks["ROT "]).Data = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public TGIBlock MODL { get { return ((TGIBlockChunk)objChunks["MODL"]).Data; } set { if (!MODL.Equals(value)) { ((TGIBlockChunk)objChunks["MODL"]).Data = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public Int32 LEVL { get { return ((Int32Chunk)objChunks["LEVL"]).Data; } set { if (LEVL != value) { ((Int32Chunk)objChunks["LEVL"]).Data = value; OnElementChanged(); } } }
            [ElementPriority(6)]
            public Single SCAL { get { return ((SingleChunk)objChunks["SCAL"]).Data; } set { if (SCAL != value) { ((SingleChunk)objChunks["SCAL"]).Data = value; OnElementChanged(); } } }
            [ElementPriority(7)]
            public String SCRP { get { return ((StringChunk)objChunks["SCRP"]).Data; } set { if (SCRP != value) { ((StringChunk)objChunks["SCRP"]).Data = value; OnElementChanged(); } } }
            // TRES is "Unused".
            [ElementPriority(8)]
            public UInt32 DEFG { get { return ((UInt32Chunk)objChunks["DEFG"]).Data; } set { if (DEFG != value) { ((UInt32Chunk)objChunks["DEFG"]).Data = value; OnElementChanged(); } } }
            [ElementPriority(9)]
            public UInt64 DGUD { get { return ((UInt64Chunk)objChunks["DGUD"]).Data; } set { if (DGUD != value) { ((UInt64Chunk)objChunks["DGUD"]).Data = value; OnElementChanged(); } } }
            [ElementPriority(10)]
            public Byte MLOD { get { return ((ByteChunk)objChunks["MLOD"]).Data; } set { if (MLOD != value) { ((ByteChunk)objChunks["MLOD"]).Data = value; OnElementChanged(); } } }
            [ElementPriority(11)]
            public UInt64 PTID { get { return ((UInt64Chunk)objChunks["PTID"]).Data; } set { if (PTID != value) { ((UInt64Chunk)objChunks["PTID"]).Data = value; OnElementChanged(); } } }
            [ElementPriority(12)]
            public UInt32 SLOT { get { return ((UInt32Chunk)objChunks["SLOT"]).Data; } set { if (SLOT != value) { ((UInt32Chunk)objChunks["SLOT"]).Data = value; OnElementChanged(); } } }
            #endregion
        }

        public class UnusedChunk : TagLengthValue
        {
            #region Attributes
            private byte[] data;
            #endregion

            #region Constructors
            public UnusedChunk(int apiVersion, EventHandler handler, UInt32 tag) : base(apiVersion, handler, tag) { }
            public UnusedChunk(int apiVersion, EventHandler handler, UInt32 tag, Stream s, UInt32 expectedLength) : this(apiVersion, handler, tag) { Parse(s, expectedLength); }
            #endregion

            #region Data I/O
            private void Parse(Stream s, UInt32 expectedLength)
            {
                data = new byte[expectedLength];
                s.Read(data, 0, (int)expectedLength);
            }
            protected override void WriteTLVData(Stream s) { new BinaryWriter(s).Write(data); }
            #endregion

            #region IEquatable<TagLengthValue> Members
            public bool Equals(UnusedChunk other) { return tag.Equals(other.tag) && data.Equals(other.data); }
            public override bool Equals(TagLengthValue obj) { return obj as UnusedChunk != null ? this.Equals(obj as UnusedChunk) : false; }
            public override int GetHashCode() { return tag.GetHashCode() ^ data.GetHashCode(); }
            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public virtual BinaryReader Data
            {
                get { MemoryStream ms = new MemoryStream(); UnParse(ms); return new BinaryReader(ms); }
                set
                {
                    if (value.BaseStream.CanSeek) { value.BaseStream.Position = 0; Parse(value.BaseStream, (UInt32)value.BaseStream.Length); }
                    else
                    {
                        MemoryStream ms = new MemoryStream();
                        byte[] buffer = new byte[1024 * 1024];
                        for (int read = value.BaseStream.Read(buffer, 0, buffer.Length); read > 0; read = value.BaseStream.Read(buffer, 0, buffer.Length))
                            ms.Write(buffer, 0, read);
                        ms.Flush();
                        ms.Position = 0;
                        Parse(ms, (UInt32)ms.Length);
                    }
                    OnElementChanged();
                }
            }
            #endregion
        }

        public class UInt64Chunk : TagLengthValue
        {
            #region Attributes
            UInt64 data = 0;
            #endregion

            #region Constructors
            public UInt64Chunk(int apiVersion, EventHandler handler, UInt32 tag) : base(apiVersion, handler, tag) { }
            public UInt64Chunk(int apiVersion, EventHandler handler, UInt32 tag, Stream s) : this(apiVersion, handler, tag) { Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s) { data = new BinaryReader(s).ReadUInt64(); }
            protected override void WriteTLVData(Stream s) { new BinaryWriter(s).Write(data); }
            #endregion

            #region IEquatable<TagLengthValue> Members
            public bool Equals(UInt64Chunk other) { return tag.Equals(other.tag) && data.Equals(other.data); }
            public override bool Equals(TagLengthValue obj) { return obj as UInt64Chunk != null ? this.Equals(obj as UInt64Chunk) : false; }
            public override int GetHashCode() { return tag.GetHashCode() ^ data.GetHashCode(); }
            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public UInt64 Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            #endregion
        }

        public class UInt32Chunk : TagLengthValue
        {
            #region Attributes
            UInt32 data = 0;
            #endregion

            #region Constructors
            public UInt32Chunk(int apiVersion, EventHandler handler, UInt32 tag) : base(apiVersion, handler, tag) { }
            public UInt32Chunk(int apiVersion, EventHandler handler, UInt32 tag, Stream s) : this(apiVersion, handler, tag) { Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s) { data = new BinaryReader(s).ReadUInt32(); }
            protected override void WriteTLVData(Stream s) { new BinaryWriter(s).Write(data); }
            #endregion

            #region IEquatable<TagLengthValue> Members
            public bool Equals(UInt32Chunk other) { return data.Equals(other.data); }
            public override bool Equals(TagLengthValue obj) { return obj as UInt32Chunk != null ? this.Equals(obj as UInt32Chunk) : false; }
            public override int GetHashCode() { return tag.GetHashCode() ^ data.GetHashCode(); }
            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public UInt32 Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            #endregion
        }

        public class ByteChunk : TagLengthValue
        {
            #region Attributes
            Byte data = 0;
            #endregion

            #region Constructors
            public ByteChunk(int apiVersion, EventHandler handler, UInt32 tag) : base(apiVersion, handler, tag) { }
            public ByteChunk(int apiVersion, EventHandler handler, UInt32 tag, Stream s) : this(apiVersion, handler, tag) { Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s) { data = new BinaryReader(s).ReadByte(); }
            protected override void WriteTLVData(Stream s) { new BinaryWriter(s).Write(data); }
            #endregion

            #region IEquatable<TagLengthValue> Members
            public bool Equals(ByteChunk other) { return data.Equals(other.data); }
            public override bool Equals(TagLengthValue obj) { return obj as ByteChunk != null ? this.Equals(obj as ByteChunk) : false; }
            public override int GetHashCode() { return tag.GetHashCode() ^ data.GetHashCode(); }
            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public Byte Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            #endregion
        }

        public class Int32Chunk : TagLengthValue
        {
            #region Attributes
            Int32 data = 0;
            #endregion

            #region Constructors
            public Int32Chunk(int apiVersion, EventHandler handler, UInt32 tag) : base(apiVersion, handler, tag) { }
            public Int32Chunk(int apiVersion, EventHandler handler, UInt32 tag, Stream s) : this(apiVersion, handler, tag) { Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s) { data = new BinaryReader(s).ReadInt32(); }
            protected override void WriteTLVData(Stream s) { new BinaryWriter(s).Write(data); }
            #endregion

            #region IEquatable<TagLengthValue> Members
            public bool Equals(Int32Chunk other) { return data.Equals(other.data); }
            public override bool Equals(TagLengthValue obj) { return obj as Int32Chunk != null ? this.Equals(obj as Int32Chunk) : false; }
            public override int GetHashCode() { return tag.GetHashCode() ^ data.GetHashCode(); }
            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public Int32 Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            #endregion
        }

        public class SingleChunk : TagLengthValue
        {
            #region Attributes
            Single data = 0;
            #endregion

            #region Constructors
            public SingleChunk(int apiVersion, EventHandler handler, UInt32 tag) : base(apiVersion, handler, tag) { }
            public SingleChunk(int apiVersion, EventHandler handler, UInt32 tag, Stream s) : this(apiVersion, handler, tag) { Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s) { data = new BinaryReader(s).ReadSingle(); }
            protected override void WriteTLVData(Stream s) { new BinaryWriter(s).Write(data); }
            #endregion

            #region IEquatable<TagLengthValue> Members
            public bool Equals(SingleChunk other) { return data.Equals(other.data); }
            public override bool Equals(TagLengthValue obj) { return obj as SingleChunk != null ? this.Equals(obj as SingleChunk) : false; }
            public override int GetHashCode() { return tag.GetHashCode() ^ data.GetHashCode(); }
            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public Single Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            #endregion
        }

        public class VertexChunk : TagLengthValue
        {
            #region Attributes
            Vertex data;
            #endregion

            #region Constructors
            public VertexChunk(int apiVersion, EventHandler handler, UInt32 tag) : base(apiVersion, handler, tag) { }
            public VertexChunk(int apiVersion, EventHandler handler, UInt32 tag, Stream s) : this(apiVersion, handler, tag) { Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s) { data = new Vertex(requestedApiVersion, handler, s); }
            protected override void WriteTLVData(Stream s) { data.UnParse(s); }
            #endregion

            #region IEquatable<TagLengthValue> Members
            public bool Equals(VertexChunk other) { return tag.Equals(other.tag) && data.Equals(other.data); }
            public override bool Equals(TagLengthValue obj) { return obj as VertexChunk != null ? this.Equals(obj as VertexChunk) : false; }
            public override int GetHashCode() { return tag.GetHashCode() ^ data.GetHashCode(); }
            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public Vertex Data { get { return data; } set { if (!data.Equals(value)) { data = new Vertex(requestedApiVersion, handler, value); OnElementChanged(); } } }
            #endregion
        }

        public class QuaternionChunk : TagLengthValue
        {
            #region Attributes
            Quaternion data;
            #endregion

            #region Constructors
            public QuaternionChunk(int apiVersion, EventHandler handler, UInt32 tag) : base(apiVersion, handler, tag) { }
            public QuaternionChunk(int apiVersion, EventHandler handler, UInt32 tag, Stream s) : this(apiVersion, handler, tag) { Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s) { data = new Quaternion(requestedApiVersion, handler, s); }
            protected override void WriteTLVData(Stream s) { data.UnParse(s); }
            #endregion

            #region IEquatable<TagLengthValue> Members
            public bool Equals(QuaternionChunk other) { return tag.Equals(other.tag) && data.Equals(other.data); }
            public override bool Equals(TagLengthValue obj) { return obj as QuaternionChunk != null ? this.Equals(obj as QuaternionChunk) : false; }
            public override int GetHashCode() { return tag.GetHashCode() ^ data.GetHashCode(); }
            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public Quaternion Data { get { return data; } set { if (!data.Equals(value)) { data = new Quaternion(requestedApiVersion, handler, value); OnElementChanged(); } } }
            #endregion
        }

        public class TGIBlockChunk : TagLengthValue
        {
            #region Attributes
            TGIBlock data;
            #endregion

            #region Constructors
            public TGIBlockChunk(int apiVersion, EventHandler handler, UInt32 tag) : base(apiVersion, handler, tag) { }
            public TGIBlockChunk(int apiVersion, EventHandler handler, UInt32 tag, Stream s) : this(apiVersion, handler, tag) { Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s) { data = new TGIBlock(requestedApiVersion, handler, s); }
            protected override void WriteTLVData(Stream s) { data.UnParse(s); }
            #endregion

            #region IEquatable<TagLengthValue> Members
            public bool Equals(TGIBlockChunk other) { return tag.Equals(other.tag) && data.Equals(other.data); }
            public override bool Equals(TagLengthValue obj) { return obj as TGIBlockChunk != null ? this.Equals(obj as TGIBlockChunk) : false; }
            public override int GetHashCode() { return tag.GetHashCode() ^ data.GetHashCode(); }
            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public TGIBlock Data { get { return data; } set { if (!data.Equals(value)) { data = new TGIBlock(requestedApiVersion, handler, value); OnElementChanged(); } } }
            #endregion
        }

        public class StringChunk : TagLengthValue
        {
            #region Attributes
            private String data;
            #endregion

            #region Constructors
            public StringChunk(int apiVersion, EventHandler handler, UInt32 tag) : base(apiVersion, handler, tag) { }
            public StringChunk(int apiVersion, EventHandler handler, UInt32 tag, Stream s, UInt32 expectedLength) : this(apiVersion, handler, tag) { Parse(s, expectedLength); }
            #endregion

            #region Data I/O
            private void Parse(Stream s, UInt32 expectedLength) { data = new String(new BinaryReader(s).ReadBytes((int)expectedLength).Select(x => (char)x).ToArray()); }
            protected override void WriteTLVData(Stream s) { new BinaryWriter(s).Write(data.ToCharArray().Select(x => (byte)x).ToArray()); }
            #endregion

            #region IEquatable<TagLengthValue> Members
            public bool Equals(StringChunk other) { return tag.Equals(other.tag) && data.Equals(other.data); }
            public override bool Equals(TagLengthValue obj) { return obj as StringChunk != null ? this.Equals(obj as StringChunk) : false; }
            public override int GetHashCode() { return tag.GetHashCode() ^ data.GetHashCode(); }
            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public String Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            #endregion
        }

        #endregion

        #region AApiVersionedFields
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
        #endregion

        #region Content Fields
        [MinimumVersion(1)]
        [MaximumVersion(recommendedApiVersion)]
        [ElementPriority(1)]
        public OMGSChunk ObjectManager { get { return objectManager; } set { if (!objectManager.Equals(value)) { objectManager = new OMGSChunk(requestedApiVersion, OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }
        #endregion

        public string Value { get { return this.ValueBuilder; } }
    }

    public class DWorldResourceHandler : AResourceHandler
    {
        /// <summary>
        /// Create the content of the Dictionary.
        /// </summary>
        public DWorldResourceHandler()
        {
            this.Add(typeof(DWorldResource), new List<string>(new string[] { "0x810A102D", }));
        }
    }
}
