/***************************************************************************
 *  Copyright (C) 2014, 2016 by the Sims 4 Tools development team          *
 *                                                                         *
 *  Contributors:                                                          *
 *  Peter Jones                                                            *
 *  Keyi Zhang                                                             *
 *  CmarNYC                                                                *
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
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;
using s4pi.Interfaces;
using s4pi.Settings;

namespace TerrainMeshResource
{
    public class TerrainMeshResource : AResource
    {
        static bool checking = s4pi.Settings.Settings.Checking;
        const Int32 recommendedApiVersion = 1;

        #region Attributes
        UInt32 version;

        UInt32 layerIndexCount;

        UInt16[] min = new UInt16[3];
        UInt16[] max = new UInt16[3];

        TerrainVertexList verts;

        UInt16[] indices;

        PassList passes;

        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the resource
        /// </summary>
        /// <param name="APIversion">Requested API version</param>
        /// <param name="s">Data stream to use, or null to create from scratch</param>
        public TerrainMeshResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); dirty = true; } stream.Position = 0; Parse(stream); }
        #endregion

        #region AApiVersionedFields
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
        #endregion

        #region Data I/O
        void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);

            version = br.ReadUInt32();
            if (checking) if (version != 0x200)
                    throw new InvalidDataException(String.Format("{0}: unsupported 'version'.  Read '0x{1:X8}', supported: '0x00000008'", this.GetType().Name, version));

            UInt32 vertCount = br.ReadUInt32();
            UInt32 indexCount = br.ReadUInt32();
            layerIndexCount = br.ReadUInt32();
            UInt32 passCount = br.ReadUInt32();

            for (int i = 0; i < 3; i++) { this.min[i] = br.ReadUInt16(); }
            for (int i = 0; i < 3; i++) { this.max[i] = br.ReadUInt16(); }

            this.verts = new TerrainVertexList(this.OnResourceChanged, s, vertCount);

            this.indices = new UInt16[indexCount];
            for (int i = 0; i < indexCount; i++) { this.indices[i] = br.ReadUInt16(); }

            passes = new PassList(this.OnResourceChanged, s, passCount);
        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(version);

            bw.Write((UInt32)verts.LongCount());
            bw.Write((UInt32)indices.LongLength);
            bw.Write(layerIndexCount);
            bw.Write((UInt32)passes.LongCount());

            for (int i = 0; i < 3; i++) { bw.Write(this.min[i]); }
            for (int i = 0; i < 3; i++) { bw.Write(this.max[i]); }

            this.verts.UnParse(ms);

            for (int i = 0; i < this.indices.Length; i++) { bw.Write(this.indices[i]); }

            this.passes.UnParse(ms);
            
            bw.Flush();
            return ms;
        }
        #endregion

        public string Value { get { return this.ValueBuilder; } }

        #region Content Fields
        [MinimumVersion(1)]
        [MaximumVersion(recommendedApiVersion)]

        [ElementPriority(0)]
        public UInt32 Version { get { return version; } set { if (version != value) { version = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(1)]
        public UInt32 LayerIndexCount { get { return layerIndexCount; } set { if (layerIndexCount != value) { layerIndexCount = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        public UInt16[] Min { get { return min; } set { if (min != value) { min = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(3)]
        public UInt16[] Max { get { return max; } set { if (max != value) { max = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(4)]
        public TerrainVertexList Vertices { get { return verts; } set { if (verts != value) { verts = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(5)]
        public UInt16[] Indices { get { return indices; } set { if (indices != value) { indices = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(6)]
        public PassList Passes { get { return passes; } set { if (passes != value) { passes = value; OnResourceChanged(this, EventArgs.Empty); } } }

        #endregion

        public class TerrainVertexList : DependentList<TerrainVertex>
        {
            #region Constructors
            public TerrainVertexList(EventHandler handler) : base(handler) { }
            public TerrainVertexList(EventHandler handler, Stream s) : base(handler, s) { }
            public TerrainVertexList(EventHandler handler, Stream s, UInt32 count) : base(handler) { this.Parse(s, count); }
            public TerrainVertexList(EventHandler handler, IEnumerable<TerrainVertex> le) : base(handler, le) { }
            #endregion
            #region Data I/O

            protected void Parse(Stream s, UInt32 count)
            {
                var r = new BinaryReader(s);
                for (var i = 0; i < count; i++)
                {
                    this.Add(new TerrainVertex(1, handler, s));
                }
            }

            public override void UnParse(Stream s)
            {
                var w = new BinaryWriter(s);
                foreach (TerrainVertex vert in this)
                {
                    vert.UnParse(s);
                }
            }
            #endregion
            protected override TerrainVertex CreateElement(Stream s) { return new TerrainVertex(1, elementHandler, s); }
            protected override void WriteElement(Stream s, TerrainVertex element) { element.UnParse(s); }
        }

        public class TerrainVertex : AHandlerElement, IEquatable<TerrainVertex>
        {
            private const int recommendedApiVersion = 1;

            private Int16 x;
            private Int16 y;
            private Int16 z;
            private Int16 morphY;

            public TerrainVertex(int apiVersion, EventHandler handler)
                : base(apiVersion, handler)
            {
            }

            public TerrainVertex(int apiVersion, EventHandler handler, Stream s)
                : base(apiVersion, handler)
            {
                this.Parse(s);
            }

            public void Parse(Stream s)
            {
                var r = new BinaryReader(s);
                this.x = r.ReadInt16();
                this.y = r.ReadInt16();
                this.z = r.ReadInt16();
                this.morphY = r.ReadInt16();
            }

            public void UnParse(Stream s)
            {
                var w = new BinaryWriter(s);
                w.Write(this.x);
                w.Write(this.y);
                w.Write(this.z);
                w.Write(this.morphY);
            }

            #region AHandlerElement Members

            public override int RecommendedApiVersion
            {
                get { return recommendedApiVersion; }
            }

            public override List<string> ContentFields
            {
                get { return GetContentFields(this.requestedApiVersion, this.GetType()); }
            }

            #endregion

            #region Content Fields

            [ElementPriority(0)]
            public Int16 X
            {   get { return this.x; }
                set { if (!value.Equals(this.x)) { this.x = value; this.OnElementChanged(); } }
            }

            [ElementPriority(1)]
            public Int16 Y
            {
                get { return this.y; }
                set { if (!value.Equals(this.y)) { this.y = value; this.OnElementChanged(); } }
            }

            [ElementPriority(2)]
            public Int16 Z
            {
                get { return this.z; }
                set { if (!value.Equals(this.z)) { this.z = value; this.OnElementChanged(); } }
            }

            [ElementPriority(3)]
            public Int16 MorphY
            {
                get { return this.morphY; }
                set { if (!value.Equals(this.morphY)) { this.morphY = value; this.OnElementChanged(); } }
            }

            public string Value
            {
                get { return this.ValueBuilder; }
            }

            #endregion

            #region IEquatable

            public bool Equals(TerrainVertex other)
            {
                return this.x == other.x && this.y == other.y && this.z == other.z && this.morphY == other.morphY;
            }

            #endregion
        }

        public class PassList : DependentList<Pass>
        {
            #region Constructors
            public PassList(EventHandler handler) : base(handler) { }
            public PassList(EventHandler handler, Stream s) : base(handler, s) { }
            public PassList(EventHandler handler, Stream s, UInt32 count) : base(handler) { this.Parse(s, count); }
            public PassList(EventHandler handler, IEnumerable<Pass> le) : base(handler, le) { }
            #endregion
            #region Data I/O

            protected void Parse(Stream s, UInt32 count)
            {
                var r = new BinaryReader(s);
                for (var i = 0; i < count; i++)
                {
                    this.Add(new Pass(1, handler, s));
                }
            }

            public override void UnParse(Stream s)
            {
                var w = new BinaryWriter(s);
                foreach (Pass pass in this)
                {
                    pass.UnParse(s);
                }
            }
            #endregion

            protected override Pass CreateElement(Stream s) { return new Pass(1, elementHandler, s); }
            protected override void WriteElement(Stream s, Pass element) { element.UnParse(s); }
        }

        public class Pass : AHandlerElement, IEquatable<Pass>
        {
            private const int recommendedApiVersion = 1;

            private UInt16[] bounds = new UInt16[6];
            private byte isOpaque;
            private byte isLighting;
            byte[] layers;
            UInt32[] triangles;

            public Pass(int apiVersion, EventHandler handler)
                : base(apiVersion, handler)
            {
            }

            public Pass(int apiVersion, EventHandler handler, Stream s)
                : base(apiVersion, handler)
            {
                this.Parse(s);
            }

            public void Parse(Stream s)
            {
                var r = new BinaryReader(s);
                for (int i = 0; i < 6; i++) { this.bounds[i] = r.ReadUInt16(); }
                this.isOpaque = r.ReadByte();
                this.isLighting = r.ReadByte();
                byte layerCount = r.ReadByte();
                layers = new byte[layerCount];
                for (int i = 0; i < layerCount; i++) { layers[i] = r.ReadByte(); }
                UInt32 triCount = r.ReadUInt32();
                triangles = new UInt32[triCount];
                for (uint i = 0; i < triCount; i++) { triangles[i] = r.ReadUInt32(); }
            }

            public void UnParse(Stream s)
            {
                var w = new BinaryWriter(s);
                for (int i = 0; i < 6; i++) { w.Write(this.bounds[i]); }
                w.Write(this.isOpaque);
                w.Write(this.isLighting);
                w.Write((byte)this.layers.Length);
                for (int i = 0; i < layers.Length; i++) { w.Write(layers[i]); }
                w.Write((UInt32)this.triangles.LongLength);
                for (uint i = 0; i < this.triangles.LongLength; i++) { w.Write(triangles[i]); }
            }

            #region AHandlerElement Members

            public override int RecommendedApiVersion
            {
                get { return recommendedApiVersion; }
            }

            public override List<string> ContentFields
            {
                get { return GetContentFields(this.requestedApiVersion, this.GetType()); }
            }

            #endregion

            #region Content Fields

            [ElementPriority(0)]
            public UInt16[] Bounds
            {
                get { return this.bounds; }
                set { if (!value.Equals(this.bounds)) { this.bounds = value; this.OnElementChanged(); } }
            }

            [ElementPriority(1)]
            public byte IsOpaque
            {
                get { return this.isOpaque; }
                set { if (!value.Equals(this.isOpaque)) { this.isOpaque = value; this.OnElementChanged(); } }
            }

            [ElementPriority(2)]
            public byte IsLighting
            {
                get { return this.isLighting; }
                set { if (!value.Equals(this.isLighting)) { this.isLighting = value; this.OnElementChanged(); } }
            }

            [ElementPriority(3)]
            public byte[] Layers
            {
                get { return this.layers; }
                set { if (!value.Equals(this.layers)) { this.layers = value; this.OnElementChanged(); } }
            }

            [ElementPriority(4)]
            public UInt32[] Triangles
            {
                get { return this.triangles; }
                set { if (!value.Equals(this.triangles)) { this.triangles = value; this.OnElementChanged(); } }
            }

            public string Value
            {
                get { return this.ValueBuilder; }
            }

            #endregion

            #region IEquatable

            public bool Equals(Pass other)
            {
                return this.bounds.SequenceEqual(other.bounds) && this.isOpaque == other.isOpaque && this.isLighting == other.isLighting &&
                    this.layers.SequenceEqual(other.layers) && this.triangles.SequenceEqual(other.triangles);
            }

            #endregion
        }
    }

    public class TerrainMeshResourceHandler : AResourceHandler
    {
        /// <summary>
        /// Create the content of the Dictionary.
        /// </summary>
        public TerrainMeshResourceHandler()
        {
            this.Add(typeof(TerrainMeshResource), new List<string>(new string[] { "0xAE39399F", }));
        }
    }
}
