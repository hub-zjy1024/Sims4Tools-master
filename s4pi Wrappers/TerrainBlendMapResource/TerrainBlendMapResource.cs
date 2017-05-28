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
using System.Linq;
using System.Xml;
using s4pi.Interfaces;

namespace TerrainBlendMapResource
{
    public class TerrainBlendMapResource : AResource
    {
        static bool checking = s4pi.Settings.Settings.Checking;
        const Int32 recommendedApiVersion = 1;
        const UInt32 _layerBlockWidth = 1 << 4;// 16 in normal language

        #region Attributes
        private UInt16 version;// <format=hex>
        private UInt16 signature;// <format=hex>
        private UInt32 width;
        private UInt32 height;
        private UInt32 layerCount;
        private LayerList layers;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the resource
        /// </summary>
        /// <param name="APIversion">Requested API version</param>
        /// <param name="s">Data stream to use, or null to create from scratch</param>
        public TerrainBlendMapResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); dirty = true; } stream.Position = 0; Parse(stream); }
        #endregion

        #region Data I/O
        void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);

            version = br.ReadUInt16();
            if (checking) if (version != 0x0300)
                    throw new InvalidDataException(String.Format("{0}: unsupported 'version'.  Read '0x{1:X8}', supported: '0x0300'", this.GetType().Name, version));

            signature = br.ReadUInt16();
            if (checking) if (signature != 0xA5FF)
                    throw new InvalidDataException(String.Format("{0}: unsupported 'signature'.  Read '0x{1:X8}', supported: '0xA5FF'", this.GetType().Name, signature));

            width = br.ReadUInt32();
            height = br.ReadUInt32();

            UInt32 layerBlockWidth = br.ReadUInt32();
            if (checking) if (layerBlockWidth != _layerBlockWidth)
                    throw new InvalidDataException(String.Format("{0}: unsupported 'layerBlockWidth'.  Read '{1}', supported: '{2}'", this.GetType().Name, layerBlockWidth, _layerBlockWidth));

            UInt32 layerBlockHeight = br.ReadUInt32();
            if (checking) if (layerBlockHeight != _layerBlockWidth)
                    throw new InvalidDataException(String.Format("{0}: unsupported 'layerBlockHeight'.  Read '{1}', supported: '{2}'", this.GetType().Name, layerBlockHeight, _layerBlockWidth));

            layerCount = br.ReadUInt32();
            layers = new LayerList(OnResourceChanged, s);
        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(version);
            bw.Write(signature);
            bw.Write(width);
            bw.Write(height);
            bw.Write(_layerBlockWidth);
            bw.Write(_layerBlockWidth);
            bw.Write(layerCount);
            layers.UnParse(ms);
            
            bw.Flush();
            return ms;
        }
        #endregion

        #region Sub-types
        public class LayerBlock : AHandlerElement, IEquatable<LayerBlock>
        {
            #region Attributes
            private Byte idxHi;
            private Byte idxLo;
            private Byte[] data;
            #endregion

            #region Constructors
            public LayerBlock(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public LayerBlock(int apiVersion, EventHandler handler, LayerBlock basis) : this(apiVersion, handler, basis.idxHi, basis.idxLo, basis.data) { }
            public LayerBlock(int apiVersion, EventHandler handler, Byte idxHi, Byte idxLo, IEnumerable<Byte> data) : base(apiVersion, handler) { this.idxHi = idxHi; this.idxLo = idxLo; this.data = data.ToArray(); }
            public LayerBlock(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler) { Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                idxHi = r.ReadByte();
                idxLo = r.ReadByte();

                Byte compression = r.ReadByte();
                data = (compression == 0) ? new Byte[_layerBlockWidth * _layerBlockWidth] : new Byte[1];
                for (int i = 0; i < data.Length; i++)
                    data[i] = r.ReadByte();
            }
            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);

                w.Write(idxHi);
                w.Write(idxLo);
                w.Write((Byte)(data.Length == 1 ? 0xff : 0));
                for (int i = 0; i < data.Length; i++)
                    w.Write(data[i]);
            }
            #endregion

            #region AHandlerElement Members
            private const int recommendedApiVersion = 1;
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<LayerBlock> Members
            public bool Equals(LayerBlock other) { return idxHi.Equals(other.idxHi) && idxLo.Equals(other.idxLo) && data.Equals(other.data); }
            public override bool Equals(object other) { return other as LayerBlock != null && this.Equals(other as LayerBlock); }
            public override int GetHashCode() { return idxHi ^ idxLo ^ data.GetHashCode(); }
            #endregion

            public Byte GetWeight(Int32 x, Int32 z)
            {
                return (data.Length == 1) ? data[0] :
                    data[((z & (_layerBlockWidth - 1)) << 4) + (x & (_layerBlockWidth - 1))];
            }

            #region Content Fields
            [ElementPriority(1)]
            public Byte IdxHi { get { return idxHi; } set { if (idxHi != value) { idxHi = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public Byte IdxLo { get { return idxLo; } set { if (idxLo != value) { idxLo = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public virtual BinaryReader Data
            {
                get
                {
                    MemoryStream ms = new MemoryStream();
                    UnParse(ms);
                    return new BinaryReader(ms);
                }
                set
                {
                    if (value.BaseStream.CanSeek) { value.BaseStream.Position = 0; Parse(value.BaseStream); }
                    else
                    {
                        MemoryStream ms = new MemoryStream();
                        byte[] buffer = new byte[1024 * 1024];
                        for (int read = value.BaseStream.Read(buffer, 0, buffer.Length); read > 0; read = value.BaseStream.Read(buffer, 0, buffer.Length))
                            ms.Write(buffer, 0, read);
                        Parse(ms);
                    }
                    OnElementChanged();
                }
            }
            #endregion

            public string Value { get { return ValueBuilder; } }
        }
        public class LayerBlockList : DependentList<LayerBlock> {
            UInt32 vertCount;

            #region Constructors
            public LayerBlockList(EventHandler handler) : base(handler) { }
            public LayerBlockList(EventHandler handler, Stream s, UInt32 vertCount)
                : base(handler)
            {
                this.vertCount = vertCount;
                this.elementHandler = handler;
                this.Parse(s);
                this.handler = handler;
            }
            public LayerBlockList(EventHandler handler, IEnumerable<LayerBlock> llp) : base(handler, llp) { }
            #endregion

            #region Data I/O
            protected override LayerBlock CreateElement(Stream s) { return new LayerBlock(0, elementHandler, s); }
            protected override void WriteElement(Stream s, LayerBlock element) { element.UnParse(s); }

            protected override int ReadCount(Stream s) { return (int)this.vertCount; }
            protected override void WriteCount(Stream s, int count) { }
            #endregion
}

        public class Layer : AHandlerElement, IEquatable<Layer>
        {
            #region Attributes
            Byte layerID;
            LayerBlockList layerBlocks;
            #endregion

            #region Constructors
            public Layer(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public Layer(int apiVersion, EventHandler handler, Layer basis) : this(apiVersion, handler, basis.layerID, basis.layerBlocks) { }
            public Layer(int apiVersion, EventHandler handler, Byte layerID, IEnumerable<LayerBlock> layerBlocks) : base(apiVersion, handler) { this.layerID = layerID; this.layerBlocks = new LayerBlockList(handler, layerBlocks); }
            public Layer(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler) { Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                UInt32 layerSize = r.ReadUInt32();
                long restartPos = s.Position + ((layerSize + 3) & ~3);

                UInt32 layerBlockCount = r.ReadUInt32();
                layerID = r.ReadByte();
                layerBlocks = new LayerBlockList(handler, s, layerBlockCount);

                if (checking) if (restartPos - s.Position > 3)
                        throw new InvalidDataException(String.Format("{0}: layerBlocks alignment incorrect", this.GetType().Name));

                s.Position = restartPos;
            }
            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((UInt32)layerBlocks.Count);
                w.Write(layerID);
                layerBlocks.UnParse(s);
            }
            #endregion

            #region AHandlerElement Members
            private const int recommendedApiVersion = 1;
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Layer> Members
            public bool Equals(Layer other) { return layerID.Equals(other.layerID) && layerBlocks.Equals(other.layerBlocks); }
            public override bool Equals(object other) { return other as Layer != null && this.Equals(other as Layer); }
            public override int GetHashCode() { return layerID ^ layerBlocks.GetHashCode(); }
            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public Byte LayerID { get { return layerID; } set { if (layerID != value) { layerID = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public LayerBlockList LayerBlocks { get { return layerBlocks; } set { if (!layerBlocks.Equals(value)) { layerBlocks = value == null ? null : new LayerBlockList(handler, value); OnElementChanged(); } } }
            #endregion

            public string Value { get { return ValueBuilder; } }
        }
        public class LayerList : DependentList<Layer>
        {
            #region Constructors
            public LayerList(EventHandler handler) : base(handler) { }
            public LayerList(EventHandler handler, Stream s) : base(handler, s) { }
            public LayerList(EventHandler handler, IEnumerable<Layer> llp) : base(handler, llp) { }
            #endregion

            #region Data I/O
            protected override Layer CreateElement(Stream s) { return new Layer(0, elementHandler, s); }
            protected override void WriteElement(Stream s, Layer element) { element.UnParse(s); }
            #endregion
}
        #endregion

        public Int32 BlockIndex(Int32 layer, UInt32 x, UInt32 z)
        {
            if (layer >= layers.Count)
                throw new ArgumentOutOfRangeException("layer", layer, "Layer must be less than " + layers.Count);
            if (x > width)
                throw new ArgumentOutOfRangeException("x", x, "X co-ordinate must not be greater than " + width);
            if (z > height)
                throw new ArgumentOutOfRangeException("z", z, "Z co-ordinate must not be greater than " + height);

            UInt32 blockX = (UInt32)x >> (1 << 4);
            UInt32 blockZ = (UInt32)z >> (1 << 4);
            UInt32 layerBlockMapWidth = ((width - 1) >> 4) + 1;
            UInt32 blockOffset = blockZ * layerBlockMapWidth + blockX;
            for (int bi = 0; bi < layers[layer].LayerBlocks.Count; bi++)
            {
                LayerBlock lb = layers[layer].LayerBlocks[bi];
                if (lb.IdxLo + ((UInt32)lb.IdxHi << 8) == blockOffset)
                    return bi;
            }
            return -1;
        }

        #region AApiVersionedFields
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }

        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
        #endregion

        #region Content Fields
        [MinimumVersion(1)]
        [MaximumVersion(recommendedApiVersion)]
        [ElementPriority(1)]
        public UInt16 Version { get { return version; } set { if (version != value) { version = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        public UInt16 Signature { get { return signature; } set { if (signature != value) { signature = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(3)]
        public UInt32 Width { get { return width; } set { if (width != value) { width = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(4)]
        public UInt32 Height { get { return height; } set { if (height != value) { height = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(5)]
        public UInt32 LayerCount { get { return layerCount; } set { if (layerCount != value) { layerCount = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(6)]
        public LayerList Layers { get { return layers; } set { if (!layers.Equals(value)) { layers = value == null ? null : new LayerList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }
        #endregion

        public string Value { get { return this.ValueBuilder; } }
    }

    public class TerrainBlendMapResourceeHandler : AResourceHandler
    {
        /// <summary>
        /// Create the content of the Dictionary.
        /// </summary>
        public TerrainBlendMapResourceeHandler()
        {
            this.Add(typeof(TerrainBlendMapResource), new List<string>(new string[] { "0x3D8632D0", }));
        }
    }
}
