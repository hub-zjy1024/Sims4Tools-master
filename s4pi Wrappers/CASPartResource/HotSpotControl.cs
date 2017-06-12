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
        static bool checking = s4pi.Settings.Settings.Checking;

        private uint version;
        private AgeGenderFlags ageGender;
        private uint unknown1;
        private HotSpotLevel zoomLevel;
        private byte sliderID;
        private SliderCursor cursor;
        private SimRegion region;
        private uint unknown3;
        private BodyFrameGender frameGender;
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
            if (version >= 0x0E) this.unknown1 = r.ReadUInt32();
            this.zoomLevel = (HotSpotLevel)r.ReadByte();
            this.sliderID = r.ReadByte();
            this.cursor = (SliderCursor)r.ReadByte();
            this.region = (SimRegion)r.ReadUInt32();
            if (version >= 0x0E)
            {
                this.unknown3 = r.ReadUInt32();
                this.frameGender = (BodyFrameGender)r.ReadByte();
            }
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
            if (version >= 0x0E) w.Write(this.unknown1);
            w.Write((byte)this.zoomLevel);
            w.Write(this.sliderID);
            w.Write((byte)this.cursor);
            w.Write((uint)this.region);
            if (version >= 0x0E)
            {
                w.Write(this.unknown3);
                w.Write((byte)this.frameGender);
            }
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
        [ElementPriority(0)]
        public uint Version { get { return this.version; } set { if (!this.version.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.version = value; } } }
        [ElementPriority(1)]
        public AgeGenderFlags AgeGender { get { return this.ageGender; } set { if (!this.ageGender.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.ageGender = value; } } }
        [ElementPriority(2)]
        public uint Unknown1 { get { return this.unknown1; } set { if (!this.unknown1.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown1 = value; } } }
        [ElementPriority(3)]
        public HotSpotLevel DetailLevel { get { return this.zoomLevel; } set { if (!this.zoomLevel.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.zoomLevel = value; } } }
        [ElementPriority(4)]
        public byte SliderID { get { return this.sliderID; } set { if (!this.sliderID.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.sliderID = value; } } }
        [ElementPriority(5)]
        public SliderCursor CursorSymbol { get { return this.cursor; } set { if (!this.cursor.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.cursor = value; } } }
        [ElementPriority(6)]
        public SimRegion Region { get { return this.region; } set { if (!this.region.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.region = value; } } }
        [ElementPriority(7)]
        public uint Unknown3 { get { return this.unknown3; } set { if (!this.unknown3.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown3 = value; } } }
        [ElementPriority(7)]
        public BodyFrameGender FrameGender { get { return this.frameGender; } set { if (!this.frameGender.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.frameGender = value; } } }
        [ElementPriority(8)]
        public ulong TextureReference { get { return this.textureReference; } set { if (!this.textureReference.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.textureReference = value; } } }
        [ElementPriority(9)]
        public uint Unknown4 { get { return this.unknown4; } set { if (!this.unknown4.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown4 = value; } } }
        [ElementPriority(10)]
        public SliderList SliderDescription { get { return this.sliderDescriptions; } set { if (!this.sliderDescriptions.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.sliderDescriptions = value; } } }
        
        public string Value { get { return ValueBuilder; } }

        public override List<string> ContentFields
        {
            get
            {
                var res = base.ContentFields;
                if (this.version < 0x0E)
                {
                    res.Remove("Unknown1");
                    res.Remove("Unknown3");
                    res.Remove("FrameGender");
                }
                return res;
            }
        }

        #endregion

        #region Sub-Types
        public class SliderDesc : AHandlerElement, IEquatable<SliderDesc>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            private ViewAngle angle;
            private bool flip;
            private float[] unknown7;       //2 floats
            private ulong[] simModifierReference;      //4 Instance IDs
            #endregion

            public SliderDesc(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public SliderDesc(int apiVersion, EventHandler handler, Stream s) : base(apiVersion, handler) { Parse(s); }
            public SliderDesc(int apiVersion, EventHandler handler, SliderDesc basis)
                : this(apiVersion, handler, basis.angle, basis.flip, basis.unknown7, basis.simModifierReference) { }
            public SliderDesc(int apiVersion, EventHandler handler, ViewAngle angle, bool flip, float[] unknown7, ulong[] simModifierReference)
                : base(apiVersion, handler)
            {
                this.angle = angle;
                this.flip = flip;
                this.unknown7 = new float[unknown7.Length];
                for (int i = 0; i < this.unknown7.Length; i++) this.unknown7[i] = unknown7[i];
                this.simModifierReference = new ulong[simModifierReference.Length];
                for (int i = 0; i < this.simModifierReference.Length; i++) this.simModifierReference[i] = simModifierReference[i];
            }

            private void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                this.angle = (ViewAngle)r.ReadByte();
                this.flip = r.ReadBoolean();
                this.unknown7 = new float[2];
                for (int i = 0; i < 2; i++) { this.unknown7[i] = r.ReadSingle(); }
                this.simModifierReference = new ulong[4];
                for (int i = 0; i < 4; i++) { this.simModifierReference[i] = r.ReadUInt64(); }
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((byte)this.angle);
                w.Write(this.flip);
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
                return this.angle == other.angle && this.flip == other.flip && this.unknown7.SequenceEqual(other.unknown7) && this.simModifierReference.SequenceEqual(other.simModifierReference);
            }

            public override bool Equals(object obj) { return obj is SliderDesc && Equals(obj as SliderDesc); }

            public override int GetHashCode() { return this.angle.GetHashCode() + this.flip.GetHashCode() + this.unknown7.GetHashCode() + this.simModifierReference.GetHashCode(); }
            #endregion

            [ElementPriority(10)]
            public ViewAngle ViewingAngle { get { return this.angle; } set { if (!this.angle.Equals(value)) { this.angle = value; OnElementChanged(); } } }
            [ElementPriority(11)]
            public bool FlipDirections { get { return this.flip; } set { if (!this.flip.Equals(value)) { this.flip = value; OnElementChanged(); } } }
            [ElementPriority(12)]
            public float[] Unknown7 { get { return this.unknown7; } set { if (!this.unknown7.Equals(value)) { this.unknown7 = value; OnElementChanged(); } } }
            [ElementPriority(13)]
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

        public enum HotSpotLevel : byte
        {
            Macro = 0,
            Micro = 1,
            Special = 2
        }

        [Flags]
        public enum ViewAngle : byte
        {
            None = 0,
            FrontView = 1,
            FrontUnknown1 = 1 << 1,
            FrontUnknown2 = 1 << 2,
            SideUnknown = 1 << 3,
            SideView = 1 << 4,
            BackView = 1 << 5,
            BackUnknown = 1 << 6
        }

        [Flags]
        public enum BodyFrameGender : byte
        {
            None = 0,
            Male = 1,
            Female = 2
        }

        public enum SliderCursor : byte
        {
            None = 0,
            HorizontalAndVerticalArrows = 1,
            HorizontalArrows = 2,
            VerticalArrows = 3,
            Diagonal = 4,
            Rotation = 5
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

