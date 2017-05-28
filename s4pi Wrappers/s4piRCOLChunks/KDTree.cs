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
 *  along with s3pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using s4pi.Interfaces;

namespace s4pi.GenericRCOLResource
{
    public class KDTree : ARCOLBlock
    {
        static bool checking = s4pi.Settings.Settings.Checking;
        const string TAG = "KDTR";

        #region Attributes
        private UInt32 version = 0x100;// <format=hex>
        private Single scaleX;
        private Single scaleY;
        private Single scaleZ;
        private Vert bboxMin;
        private Vert bboxMax;
        private NodeList nodes;
        private VertList verts;
        private CountedUInt16List indices;
        #endregion

        #region Constructors
        public KDTree(int APIversion, EventHandler handler) : base(APIversion, handler, null) { }
        public KDTree(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
        public KDTree(int APIversion, EventHandler handler, KDTree basis)
            : base(APIversion, handler, null)
        {
            if (checking) if (basis.version != 0x100)
                    throw new InvalidDataException(String.Format("Unsupported version in basis: '0x{0:X8}'; expected: '0x00000100'; at 0x{1:X8}", basis.version));

            version = basis.version;// <format=hex>
            scaleX = basis.scaleX;
            scaleY = basis.scaleY;
            scaleZ = basis.scaleZ;
            bboxMin = new Vert(APIversion, handler, basis.bboxMin);
            bboxMax = new Vert(APIversion, handler, basis.bboxMax);
            nodes = new NodeList(handler, basis.nodes);
            verts = new VertList(handler, basis.verts);
            indices = new CountedUInt16List(handler, basis.indices);
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag { get { return TAG; } }

        [ElementPriority(3)]
        public override uint ResourceType { get { return 0x033B2B66; } }

        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);

            version = r.ReadUInt32();// <format=hex>
            if (checking) if (version != 0x100)
                    throw new InvalidDataException(String.Format("Unsupported version read: '0x{0:X8}'; expected: '0x00000100'; at 0x{1:X8}", version, s.Position));

            UInt32 nodeCount = r.ReadUInt32();
            UInt32 vertCount = r.ReadUInt32();
            UInt32 indexCount = r.ReadUInt32();
            scaleX = r.ReadSingle();
            scaleY = r.ReadSingle();
            scaleZ = r.ReadSingle();
            bboxMin = new Vert(requestedApiVersion, handler, s);
            bboxMax = new Vert(requestedApiVersion, handler, s);
            nodes = new NodeList(handler, s, nodeCount);
            verts = new VertList(handler, s, vertCount);
            indices = new CountedUInt16List(handler, s, indexCount);
        }

        public override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(version);
            if (nodes == null) nodes = new NodeList(handler);
            w.Write((UInt32)nodes.Count);
            if (verts == null) verts = new VertList(handler);
            w.Write((UInt32)verts.Count);
            if (indices == null) indices = new CountedUInt16List(handler);
            w.Write((UInt32)indices.Count);
            w.Write(scaleX);
            w.Write(scaleY);
            w.Write(scaleZ);
            if (bboxMin == null) bboxMin = new Vert(requestedApiVersion, handler);
            bboxMin.UnParse(ms);
            if (bboxMax == null) bboxMax = new Vert(requestedApiVersion, handler);
            bboxMax.UnParse(ms);
            nodes.UnParse(ms);
            verts.UnParse(ms);
            indices.UnParse(ms);

            return ms;
        }
        #endregion

        #region Sub-types
        public class Branch : Node, IEquatable<Branch>
        {
            #region Attributes
            private UInt32 type;
            private UInt32 offsetToLeftRightNodePair;
            #endregion

            #region Constructors
            public Branch(int apiVersion, EventHandler handler) : this(apiVersion, handler, 0xFFFFFFFF, 0) { }
            public Branch(int apiVersion, EventHandler handler, Branch basis) : this(apiVersion, handler, basis.type, basis.offsetToLeftRightNodePair) { }
            public Branch(int apiVersion, EventHandler handler, UInt32 type, UInt32 offsetToLeftRightNodePair) : base(apiVersion, handler)
            {
                if (type == 0)
                    throw new ArgumentOutOfRangeException("Branch type must not be zero");
                this.type = type;
                this.offsetToLeftRightNodePair = offsetToLeftRightNodePair;
            }
            public Branch(int apiVersion, EventHandler handler, Stream s, UInt32 type) : base(apiVersion, handler)
            {
                if (type == 0)
                    throw new InvalidDataException(String.Format("Branch type must not be zero; at 0x{0:X8}", s.Position));
                this.type = type;
                Parse(s);
            }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                offsetToLeftRightNodePair = r.ReadUInt32();
            }
            public override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(type);
                w.Write(offsetToLeftRightNodePair);
            }
            #endregion

            #region AHandlerElement Members
            private const int recommendedApiVersion = 1;
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Vert> Members
            public bool Equals(Branch other) { return type.Equals(other.type) && offsetToLeftRightNodePair.Equals(other.offsetToLeftRightNodePair); }
            public override bool Equals(Node other) { return other as Branch != null && this.Equals(other as Branch); }
            public override bool Equals(object other) { return other as Branch != null && this.Equals(other as Branch); }
            public override int GetHashCode() { return (int)(type ^ offsetToLeftRightNodePair); }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public UInt32 NodeType
            {
                get { return type; }
                set {
                    if (value == 0)
                        throw new InvalidDataException("Branch type must not be zero");
                    if (type != value)
                    {
                        type = value; OnElementChanged();
                    }
                }
            }
            [ElementPriority(2)]
            public UInt32 OffsetToLeftRightNodePair { get { return offsetToLeftRightNodePair; } set { if (offsetToLeftRightNodePair != value) { offsetToLeftRightNodePair = value; OnElementChanged(); } } }
            #endregion

            public string Value { get { return ValueBuilder; } }
        }

        public class Leaf : Node, IEquatable<Leaf>
        {
            #region Attributes
            private readonly UInt32 type = 0;
            private UInt32 triangleStart;
            private UInt32 triangleSCount;
            #endregion

            #region Constructors
            public Leaf(int apiVersion, EventHandler handler) : this(apiVersion, handler, 0, 0, 0) { }
            public Leaf(int apiVersion, EventHandler handler, Leaf basis) : this(apiVersion, handler, basis.type, basis.triangleStart, basis.triangleSCount) { }
            public Leaf(int apiVersion, EventHandler handler, UInt32 type, UInt32 triangleStart, UInt32 triangleSCount) : base(apiVersion, handler)
            {
                if (type != 0)
                    throw new ArgumentOutOfRangeException(String.Format("Leaf type must be zero; received '{0}'", type));
                this.triangleStart = triangleStart;
                this.triangleSCount = triangleSCount;
            }
            public Leaf(int apiVersion, EventHandler handler, Stream s, UInt32 type) : base(apiVersion, handler)
            {
                if (type != 0)
                    throw new InvalidDataException(String.Format("Leaf type must be zero; received '{0}'; at 0x{1:X8}", type, s.Position));
                Parse(s);
            }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                triangleStart = r.ReadUInt32();
                triangleSCount = r.ReadUInt32();
            }
            public override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((UInt32)0);
                w.Write(triangleStart);
                w.Write(triangleSCount);
            }
            #endregion

            #region AHandlerElement Members
            private const int recommendedApiVersion = 1;
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Vert> Members
            public bool Equals(Leaf other) { return type.Equals(other.type) && triangleStart.Equals(other.triangleStart) && triangleSCount.Equals(other.triangleSCount); }
            public override bool Equals(Node other) { return other as Leaf != null && this.Equals(other as Leaf); }
            public override bool Equals(object other) { return other as Leaf != null && this.Equals(other as Leaf); }
            public override int GetHashCode() { return (int)(type ^ triangleStart ^ triangleSCount); }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public UInt32 NodeType { get { return type; } }
            [ElementPriority(2)]
            public UInt32 TriangleStart { get { return triangleStart; } set { if (triangleStart != value) { triangleStart = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public UInt32 TriangleSCount { get { return triangleSCount; } set { if (triangleSCount != value) { triangleSCount = value; OnElementChanged(); } } }
            #endregion

            public string Value { get { return ValueBuilder; } }
        }

        public abstract class Node : AHandlerElement, IEquatable<Node>
        {
            public Node(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public override AHandlerElement Clone(EventHandler handler) { return NodeFactory(requestedApiVersion, handler, this); }
            public Node NodeFactory(int apiVersion, EventHandler handler, Node basis)
            {
                return basis as Leaf != null ? (Node)new Leaf(apiVersion, handler, basis as Leaf) : (Node)new Branch(apiVersion, handler, basis as Branch);
            }
            public static Node NodeFactory(int apiVersion, EventHandler handler, Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                UInt32 type = r.ReadUInt32();
                return type == 0 ? (Node)new Leaf(apiVersion, handler, s, type) : (Node)new Branch(apiVersion, handler, s, type);
            }
            public abstract void UnParse(Stream s);
            public abstract bool Equals(Node other);
        }

        public class NodeList : DependentList<Node>
        {
            UInt32 nodeCount;

            #region Constructors
            public NodeList(EventHandler handler) : base(handler) { }
            public NodeList(EventHandler handler, Stream s, UInt32 nodeCount) : base(handler)
            {
                this.nodeCount = nodeCount;
                this.elementHandler = handler;
                this.Parse(s);
                this.handler = handler;
            }
            public NodeList(EventHandler handler, IEnumerable<Node> llp) : base(handler, llp) { }
            #endregion

            #region Data I/O
            protected override Node CreateElement(Stream s) { return Node.NodeFactory(0, elementHandler, s); }
            protected override void WriteElement(Stream s, Node element) { element.UnParse(s); }

            protected override int ReadCount(Stream s) { return (int)this.nodeCount; }
            protected override void WriteCount(Stream s, int count) { }
            #endregion
        }

        public class Vert : AHandlerElement, IEquatable<Vert>
        {
            #region Attributes
            private UInt16 x;
            private UInt16 y;
            private UInt16 z;
            #endregion

            #region Constructors
            public Vert(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public Vert(int apiVersion, EventHandler handler, Vert basis) : this(apiVersion, handler, basis.x, basis.y, basis.z) { }
            public Vert(int apiVersion, EventHandler handler, UInt16 x, UInt16 y, UInt16 z) : base(apiVersion, handler) { this.x = x; this.y = y; this.z = z; }
            public Vert(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler) { Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                x = r.ReadUInt16();
                y = r.ReadUInt16();
                z = r.ReadUInt16();
            }
            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(x);
                w.Write(y);
                w.Write(z);
            }
            #endregion

            #region AHandlerElement Members
            private const int recommendedApiVersion = 1;
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Vert> Members
            public bool Equals(Vert other) { return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z); }
            public override bool Equals(object other) { return other as Vert != null && this.Equals(other as Vert); }
            public override int GetHashCode() { return x ^ y ^ z; }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public UInt16 X { get { return x; } set { if (x != value) { x = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public UInt16 Y { get { return y; } set { if (y != value) { y = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public UInt16 Z { get { return z; } set { if (z != value) { z = value; OnElementChanged(); } } }
            #endregion

            public string Value { get { return ValueBuilder; } }
        }

        public class VertList : DependentList<Vert>
        {
            UInt32 vertCount;

            #region Constructors
            public VertList(EventHandler handler) : base(handler) { }
            public VertList(EventHandler handler, Stream s, UInt32 vertCount)
                : base(handler)
            {
                this.vertCount = vertCount;
                this.elementHandler = handler;
                this.Parse(s);
                this.handler = handler;
            }
            public VertList(EventHandler handler, IEnumerable<Vert> llp) : base(handler, llp) { }
            #endregion

            #region Data I/O
            protected override Vert CreateElement(Stream s) { return new Vert(0, elementHandler, s); }
            protected override void WriteElement(Stream s, Vert element) { element.UnParse(s); }

            protected override int ReadCount(Stream s) { return (int)this.vertCount; }
            protected override void WriteCount(Stream s, int count) { }
            #endregion
        }

        public class CountedUInt16List : SimpleList<UInt16>
        {
            UInt32 indexCount;

            #region Constructors
            public CountedUInt16List(EventHandler handler) : base(handler, ReadUInt16, WriteUInt16) { }
            public CountedUInt16List(EventHandler handler, IEnumerable<UInt16> basis) : base(handler, basis, ReadUInt16, WriteUInt16) { }
            public CountedUInt16List(EventHandler handler, Stream s, UInt32 indexCount) : base(handler, s, ReadUInt16, WriteUInt16) { }
            #endregion

            #region Data I/O
            static UInt16 ReadUInt16(Stream s) { return new BinaryReader(s).ReadUInt16(); }
            static void WriteUInt16(Stream s, UInt16 value) { new BinaryWriter(s).Write(value); }

            protected override Int32 ReadCount(Stream s) { return (int)indexCount; }
            protected override void WriteCount(Stream s, Int32 value) { }
            #endregion
        }
        #endregion

        #region Content Fields
        [ElementPriority(11)]
        public UInt32 Version { get { return version; } set { if (version != value) { version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public Single ScaleX { get { return scaleX; } set { if (scaleX != value) { scaleX = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public Single ScaleY { get { return scaleY; } set { if (scaleY != value) { scaleY = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(14)]
        public Single ScaleZ { get { return scaleZ; } set { if (scaleZ != value) { scaleZ = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(15)]
        public Vert BBoxMin { get { return bboxMin; } set { if (!bboxMin.Equals(value)) { bboxMin = value == null ? null : new Vert(requestedApiVersion, handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(16)]
        public Vert BBoxMax { get { return bboxMax; } set { if (!bboxMax.Equals(value)) { bboxMax = value == null ? null : new Vert(requestedApiVersion, handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(17)]
        public NodeList Nodes { get { return nodes; } set { if (!nodes.Equals(value)) { nodes = value == null ? null : new NodeList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(18)]
        public VertList Verts { get { return verts; } set { if (!verts.Equals(value)) { verts = value == null ? null : new VertList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(19)]
        public CountedUInt16List Indices { get { return indices; } set { if (!indices.Equals(value)) { indices = value == null ? null : new CountedUInt16List(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        #endregion

        public string Value { get { return ValueBuilder; } }
    }
}
