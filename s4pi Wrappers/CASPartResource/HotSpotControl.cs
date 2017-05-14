/***************************************************************************
 *  Copyright (C) 2014 by the s4pe team                                    *
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
using s4pi.Interfaces;
using s4pi.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;
using System.Collections;

using CASPartResource.Lists;

namespace CASPartResource
{
    public class HotSpotControl : AResource
    {
        const int recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
        static bool checking = s4pi.Settings.Settings.Checking;

        private uint version;
        private AgeGenderFlags ageGender;
        private uint unknown1;
        private byte zoomLevel;
        private byte sliderID;
        private ushort unknown2;
        private ulong unknown3;
        private ulong textureReference;
        private uint unknown4;
        private SliderList sliderDescriptions;

        public HotSpotControl(int APIversion, Stream s) : base(APIversion, s) { if (stream == null || stream.Length == 0) { stream = UnParse(); OnResourceChanged(this, EventArgs.Empty); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            s.Position = 0;
            this.version = r.ReadUInt32();
            this.ageGender = (AgeGenderFlags)r.ReadUInt32();
            this.unknown1 = r.ReadUInt32();
            this.zoomLevel = r.ReadByte();
            this.sliderID = r.ReadByte();
            this.unknown2 = r.ReadUInt16();
            this.unknown3 = r.ReadUInt64();
            this.textureReference = r.ReadUInt64();
            this.unknown4 = r.ReadUInt32();
            byte sliderCount = r.ReadByte();
            this.sliderDescriptions = new SliderList(OnResourceChanged);
            for (int i = 0; i < sliderCount; i++)
            {
                this.sliderDescriptions.Add(new SliderDesc(recommendedApiVersion, OnResourceChanged, s));
            }
        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);
            w.Write(this.version);
            w.Write((uint)this.ageGender);
            w.Write(this.unknown1);
            w.Write(this.zoomLevel);
            w.Write(this.sliderID);
            w.Write(this.unknown2);
            w.Write(this.unknown3);
            w.Write(this.textureReference);
            w.Write(this.unknown4);
            w.Write((byte)this.sliderDescriptions.Count);
            if (sliderDescriptions == null) sliderDescriptions = new SliderList(OnResourceChanged);
            for (int i = 0; i < this.sliderDescriptions.Count; i++)
            {
                this.sliderDescriptions[i].UnParse(ms);
            }
            return ms;
        }
        #endregion

        #region Content Fields
        public string Value { get { return ValueBuilder; } }

        [ElementPriority(0)]
        public uint Version { get { return this.version; } set { if (!this.version.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.version = value; } } }
        [ElementPriority(1)]
        public AgeGenderFlags AgeGender { get { return this.ageGender; } set { if (!this.ageGender.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.ageGender = value; } } }
        [ElementPriority(2)]
        public uint Unknown1 { get { return this.unknown1; } set { if (!this.unknown1.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown1 = value; } } }
        [ElementPriority(3)]
        public byte DetailLevel { get { return this.zoomLevel; } set { if (!this.zoomLevel.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.zoomLevel = value; } } }
        [ElementPriority(4)]
        public byte SliderID { get { return this.sliderID; } set { if (!this.sliderID.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.sliderID = value; } } }
        [ElementPriority(5)]
        public ushort Unknown2 { get { return this.unknown2; } set { if (!this.unknown2.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown2 = value; } } }
        [ElementPriority(6)]
        public ulong Unknown3 { get { return this.unknown3; } set { if (!this.unknown3.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown3 = value; } } }
        [ElementPriority(7)]
        public ulong TextureReference { get { return this.textureReference; } set { if (!this.textureReference.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.textureReference = value; } } }
        [ElementPriority(8)]
        public uint Unknown4 { get { return this.unknown4; } set { if (!this.unknown4.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown4 = value; } } }
        [ElementPriority(9)]
        public SliderList SliderDescription { get { return this.sliderDescriptions; } set { if (!this.sliderDescriptions.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.sliderDescriptions = value; } } }
        
        #endregion

        #region Sub-Types
        public class SliderDesc : AHandlerElement, IEquatable<SliderDesc>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            private ushort unknown6;
            private float[] unknown7;       //2 floats
            private ulong[] simModifierReference;      //4 Instance IDs
            #endregion

            public SliderDesc(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public SliderDesc(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler) { Parse(s); }
            public SliderDesc(int apiVersion, EventHandler handler, SliderDesc basis)
                : this(apiVersion, handler, basis.unknown6, basis.unknown7, basis.simModifierReference) { }
            public SliderDesc(int apiVersion, EventHandler handler, ushort unknown6, float[] unknown7, ulong[] simModifierReference)
                : base(apiVersion, handler)
            {
                this.unknown6 = unknown6;
                this.unknown7 = new float[unknown7.Length];
                for (int i = 0; i < this.unknown7.Length; i++) this.unknown7[i] = unknown7[i];
                this.simModifierReference = new ulong[simModifierReference.Length];
                for (int i = 0; i < this.simModifierReference.Length; i++) this.simModifierReference[i] = simModifierReference[i];
            }

            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                this.unknown6 = r.ReadUInt16();
                this.unknown7 = new float[2];
                for (int i = 0; i < 2; i++) { this.unknown7[i] = r.ReadSingle(); }
                this.simModifierReference = new ulong[4];
                for (int i = 0; i < 4; i++) { this.simModifierReference[i] = r.ReadUInt64(); }
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(this.unknown6);
                for (int i = 0; i < 2; i++) { w.Write(this.unknown7[i]); }
                for (int i = 0; i < 4; i++) { w.Write(this.simModifierReference[i]); }
            }

            #region AHandlerElement
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Vector>
            public bool Equals(SliderDesc other)
            {
                return this.unknown6 == other.unknown6 && this.unknown7.SequenceEqual(other.unknown7) && this.simModifierReference.SequenceEqual(other.simModifierReference);
            }

            public override bool Equals(object obj) { return obj is SliderDesc && Equals(obj as SliderDesc); }

            public override int GetHashCode() { return this.unknown6.GetHashCode() + this.unknown7.GetHashCode() + this.simModifierReference.GetHashCode(); }
            #endregion

            [ElementPriority(10)]
            public ushort Unknown6 { get { return this.unknown6; } set { if (!this.unknown6.Equals(value)) { this.unknown6 = value; OnElementChanged(); } } }
            [ElementPriority(11)]
            public float[] Unknown7 { get { return this.unknown7; } set { if (!this.unknown7.Equals(value)) { this.unknown7 = value; OnElementChanged(); } } }
            [ElementPriority(12)]
            public ulong[] SimModifierReference { get { return this.simModifierReference; } set { if (!this.simModifierReference.Equals(value)) { this.simModifierReference = value; OnElementChanged(); } } }

            public string Value { get { return ValueBuilder; } }
        }
        public class SliderList : DependentList<SliderDesc>
        {
            #region Constructors
            public SliderList(EventHandler handler) : base(handler) { }
            public SliderList(EventHandler handler, Stream s) : base(handler, s) { }
            public SliderList(EventHandler handler, IEnumerable<SliderDesc> le) : base(handler, le) { }
            #endregion

         //   protected override int ReadCount(Stream s) { return base.ReadCount(s) / 3; }
            protected override SliderDesc CreateElement(Stream s) { return new SliderDesc(0, elementHandler, s); }
            //  protected override void WriteCount(Stream s, int count) { base.WriteCount(s, (int)(count * 3)); }
            protected override void WriteElement(Stream s, SliderDesc element) { element.UnParse(s); }
        }
        #endregion
    }

    public class HotSpotControlResourceHandler : AResourceHandler
    {
        public HotSpotControlResourceHandler()
        {
            this.Add(typeof(HotSpotControl), new List<string>(new string[] { "0x8B18FF6E", }));
        }
    }
}

