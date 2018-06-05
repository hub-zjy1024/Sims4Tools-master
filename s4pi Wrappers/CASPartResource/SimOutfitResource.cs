﻿/***************************************************************************
 *  Copyright (C) 2014 by Keyi Zhang                                       *
 *  kz005@bucknell.edu                                                     *
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

// This resource is based on my and Snaitf's analysis

using System;
using System.Collections.Generic;
using System.IO;
using s4pi.Interfaces;

namespace CASPartResource
{
    public class SimOutfitResource : AResource
    {
        private uint version;
        private float unknown1;
        private float unknown2;
        private float unknown3;
        private float unknown4;
        private float unknown5;
        private float unknown6;
        private float unknown7;
        private float unknown8;
        private AgeGenderFlags age;
        private AgeGenderFlags gender;
        private ulong skinToneReference;
        private ByteIndexList unknown9;

        private SimpleList<byte> unknownByteList;

        private CountedTGIBlockList tgiList;
        private SliderReferenceList sliderReferences1;
        private SliderReferenceList sliderReferences2;
        private DataBlobHandler unknown10;
        private UnknownBlockList unknown11;

        private SliderReferenceList sliderReferences3;
        private SliderReferenceList sliderReferences4;
        private DataBlobHandler unknown12;
        private SliderReferenceList sliderReferences5;
        private DataBlobHandler unknown13;
        private ulong caspReference;
        private SimpleList<ulong> dataReferenceList;


        public SimOutfitResource(int APIversion, Stream s) : base(APIversion, s) { if (s == null) { OnResourceChanged(this, EventArgs.Empty); } else { Parse(s); } }

        public void Parse(Stream s)
        {
            s.Position = 0;
            BinaryReader r = new BinaryReader(s);

            this.version = r.ReadUInt32();
            uint tgiOffset = r.ReadUInt32() + 8;

            // get TGI list
            long tempPosition = s.Position;
            s.Position = tgiOffset;
            TGIBlock[] _tgilist = new TGIBlock[r.ReadByte()];
            for (int i = 0; i < _tgilist.Length; i++) _tgilist[i] = new TGIBlock(1, OnResourceChanged, "IGT", s);
            this.tgiList = new CountedTGIBlockList(OnResourceChanged, _tgilist);
            s.Position = tempPosition;

            this.unknown1 = r.ReadSingle();
            this.unknown2 = r.ReadSingle();
            this.unknown3 = r.ReadSingle();
            this.unknown4 = r.ReadSingle();
            this.unknown5 = r.ReadSingle();
            this.unknown6 = r.ReadSingle();
            this.unknown7 = r.ReadSingle();
            this.unknown8 = r.ReadSingle();

            this.age = (AgeGenderFlags)r.ReadUInt32();
            this.gender = (AgeGenderFlags)r.ReadUInt32();
            this.skinToneReference = r.ReadUInt64();

            byte[] unknown18 = new byte[r.ReadByte()];
            for (int i = 0; i < unknown18.Length; i++) unknown18[i] = r.ReadByte();
            this.unknown9 = new ByteIndexList(OnResourceChanged, unknown18, this.tgiList);

            sliderReferences1 = new SliderReferenceList(OnResourceChanged, s, tgiList);
            sliderReferences2 = new SliderReferenceList(OnResourceChanged, s, tgiList);

            this.unknown10 = new DataBlobHandler(RecommendedApiVersion, OnResourceChanged, r.ReadBytes(24));
            this.unknown11 = new UnknownBlockList(OnResourceChanged, s, this.tgiList);

            this.unknownByteList = new SimpleList<byte>(OnResourceChanged);
            int count1 = r.ReadByte();
            for (int i = 0; i < count1; i++)
            {
                this.unknownByteList.Add(r.ReadByte());
            }

            sliderReferences3 = new SliderReferenceList(OnResourceChanged, s, tgiList);
            sliderReferences4 = new SliderReferenceList(OnResourceChanged, s, tgiList);

            this.unknown12 = new DataBlobHandler(RecommendedApiVersion, OnResourceChanged, r.ReadBytes(16));
            this.sliderReferences5 = new SliderReferenceList(OnResourceChanged, s, tgiList);
            this.unknown13 = new DataBlobHandler(recommendedApiVersion, OnResourceChanged, r.ReadBytes(9));
            this.caspReference = r.ReadUInt64();
            this.dataReferenceList = new SimpleList<ulong>(OnResourceChanged);
            int count2 = r.ReadByte();
            for (int i = 0; i < count2; i++)
                this.dataReferenceList.Add(r.ReadUInt64());

        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);
            w.Write(this.version);
            long tgiOffsetPosition = ms.Position;
            w.Write(0);
            w.Write(unknown1);
            w.Write(unknown2);
            w.Write(unknown3);
            w.Write(unknown4);
            w.Write(unknown5);
            w.Write(unknown6);
            w.Write(unknown7);
            w.Write(unknown8);
            w.Write((uint)age);
            w.Write((uint)gender);
            w.Write(skinToneReference);
            w.Write((byte)this.unknown9.Count);
            foreach (var value in this.unknown9) w.Write(value);
            sliderReferences1.UnParse(ms);
            sliderReferences2.UnParse(ms);
            unknown10.UnParse(ms);
            this.unknown11.UnParse(ms);

            w.Write((byte)this.unknownByteList.Count);
            foreach (var i in this.unknownByteList) w.Write(i);

            sliderReferences3.UnParse(ms);
            sliderReferences4.UnParse(ms);

            unknown12.UnParse(ms);
            sliderReferences5.UnParse(ms);

            this.unknown13.UnParse(ms);
            w.Write(this.caspReference);
            w.Write((byte)this.dataReferenceList.Count);
            foreach (var i in this.dataReferenceList) w.Write(i);

            long tmpPostion = ms.Position;
            ms.Position = tgiOffsetPosition;
            w.Write((uint)tmpPostion - 8);
            ms.Position = tmpPostion;
            w.Write((byte)tgiList.Count);
            foreach (var tgi in this.tgiList)
            {
                w.Write(tgi.Instance);
                w.Write(tgi.ResourceGroup);
                w.Write(tgi.ResourceType);
            }

            ms.Position = 0;
            return ms;
        }


        #region Sub-Type
        public class SliderReference : AHandlerElement, IEquatable<SliderReference>
        {
            public CountedTGIBlockList ParentTGIList { get; private set; }
            private byte index;
            private float sliderValue;

            public SliderReference(int apiVersion, EventHandler handler, CountedTGIBlockList tgiList) : base(apiVersion, handler) { this.ParentTGIList = tgiList; }
            public SliderReference(int apiVersion, EventHandler handler, Stream s, CountedTGIBlockList tgiList) : base(apiVersion, handler) { this.ParentTGIList = tgiList; Parse(s); }

            public void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                this.index = r.ReadByte();
                this.sliderValue = r.ReadSingle();
            }

            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(this.index);
                w.Write(this.sliderValue);
            }


            const int recommendedApiVersion = 1;
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { var res = GetContentFields(requestedApiVersion, this.GetType()); res.Remove("ParentTGIList"); return res; } }
            public string Value { get { return ValueBuilder; } }

            public bool Equals(SliderReference other)
            {
                return this.index == other.index && this.sliderValue == other.sliderValue;
            }
            [ElementPriority(0), TGIBlockListContentField("ParentTGIList")]
            public byte Index { get { return this.index; } set { if (!this.index.Equals(value)) { OnElementChanged(); this.index = value; } } }
            [ElementPriority(1)]
            public float SliderValue { get { return this.sliderValue; } set { if (!this.sliderValue.Equals(value)) { OnElementChanged(); this.sliderValue = value; } } }
        }


        public class SliderReferenceList : DependentList<SliderReference>
        {
            private CountedTGIBlockList tgiList;
            public SliderReferenceList(EventHandler handler, CountedTGIBlockList tgiList) : base(handler) { this.tgiList = tgiList; }
            public SliderReferenceList(EventHandler handler, Stream s, CountedTGIBlockList tgiList) : this(handler, tgiList) { Parse(s, tgiList); }

            #region Data I/O
            protected void Parse(Stream s, CountedTGIBlockList tgiList)
            {
                int count = s.ReadByte();
                for (int i = 0; i < count; i++) this.Add(new SliderReference(recommendedApiVersion, handler, s, this.tgiList));
            }

            public override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((byte)this.Count);
                foreach (var entry in this) entry.UnParse(s);
            }

            protected override SliderReference CreateElement(Stream s) { throw new NotImplementedException(); }
            protected override void WriteElement(Stream s, SliderReference element) { throw new NotImplementedException(); }
            #endregion
        }

        public class UnknownReference : AHandlerElement, IEquatable<UnknownReference>
        {
            private CountedTGIBlockList tgiList;
            private DataBlobHandler unknownBlock;
            private UnknownReference2List unknownReference2List;

            public UnknownReference(int apiVersion, EventHandler handler, CountedTGIBlockList tgiList) : base(apiVersion, handler) { this.tgiList = tgiList; }
            public UnknownReference(int apiVersion, EventHandler handler, Stream s, CountedTGIBlockList tgiList) : base(apiVersion, handler) { this.tgiList = tgiList; Parse(s); }

            public void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                this.unknownBlock = new DataBlobHandler(recommendedApiVersion, handler, r.ReadBytes(17));
                this.unknownReference2List = new UnknownReference2List(handler, s, tgiList);
            }

            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                this.unknownBlock.UnParse(s);
                this.unknownReference2List.UnParse(s);
            }


            public bool Equals(UnknownReference other)
            {
                return this.unknownBlock.Equals(other.unknownBlock) && this.unknownReference2List.Equals(other.unknownReference2List);
            }
            const int recommendedApiVersion = 1;
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            public DataBlobHandler UnknownBlock { get { return this.unknownBlock; } set { if (!this.unknownBlock.Equals(value)) { OnElementChanged(); this.unknownBlock = value; } } }
            public UnknownReference2List UnknownReference2List { get { return this.unknownReference2List; } set { if (!this.unknownReference2List.Equals(value)) { OnElementChanged(); this.unknownReference2List = value; } } }
            public string Value { get { return ValueBuilder; } }            
        }

        public class UnknownReference2 : AHandlerElement, IEquatable<UnknownReference2>
        {
            public CountedTGIBlockList ParentTGIList { get; private set; }
            public UnknownReference2(int apiVersion, EventHandler handler, CountedTGIBlockList tgiList) : base(apiVersion, handler) { this.ParentTGIList = tgiList; }
            public UnknownReference2(int apiVersion, EventHandler handler, Stream s, CountedTGIBlockList tgiList) : base(apiVersion, handler) { this.ParentTGIList = tgiList; Parse(s); }
            private byte index;
            private uint referenceValue;

            public void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);
                this.index = r.ReadByte();
                this.referenceValue = r.ReadUInt32();
            }

            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(this.index);
                w.Write(this.referenceValue);
            }

            const int recommendedApiVersion = 1;
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { var res = GetContentFields(requestedApiVersion, this.GetType()); res.Remove("ParentTGIList"); return res; } }
            [ElementPriority(0), TGIBlockListContentField("ParentTGIList")]
            public byte TGIReference { get { return this.index; } set { if (!this.index.Equals(value)) { this.index = value; } } }
            [ElementPriority(1)]
            public uint ReferenceValue { get { return this.referenceValue; } set { if (!this.referenceValue.Equals(value)) { this.referenceValue = value; } } }
            public string Value { get { return ValueBuilder; } }

            #region IEquatable
            public bool Equals(UnknownReference2 other)
            {
                return this.index == other.index && this.referenceValue == other.referenceValue;
            }
            #endregion
        }

        public class UnknownBlock : AHandlerElement, IEquatable<UnknownBlock>
        {

            private byte blockIndex;
            private uint unknown1;
            private UnknownReferenceList unknownReferenceList;
            private CountedTGIBlockList tgiList;

            public UnknownBlock(int apiVersion, EventHandler handler, CountedTGIBlockList tgiList) : base(apiVersion, handler) { this.tgiList = tgiList; }
            public UnknownBlock(int apiVersion, EventHandler handler, Stream s, CountedTGIBlockList tgiList) : base(apiVersion, handler) { this.tgiList = tgiList; Parse(s, tgiList); }
            
            protected void Parse(Stream s, CountedTGIBlockList tgiList)
            {
                BinaryReader r = new BinaryReader(s);
                this.blockIndex = r.ReadByte();
                this.unknown1 = r.ReadUInt32();
                this.unknownReferenceList = new UnknownReferenceList(handler, s, tgiList);
            }

            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(this.blockIndex);
                w.Write(this.unknown1);
                this.unknownReferenceList.UnParse(s);
            }


            #region Content Fields
            [ElementPriority(0)]
            public byte BlockIndex { get { return this.blockIndex; } set { if (!this.blockIndex.Equals(value)) { this.blockIndex = value; } } }
            [ElementPriority(1)]
            public uint Unknown1 { get { return this.unknown1; } set { if (!this.unknown1.Equals(value)) { this.unknown1 = value; } } }
            [ElementPriority(2)]
            public UnknownReferenceList UnknownReferenceList { get { return this.unknownReferenceList; } set { if (!this.unknownReferenceList.Equals(value)) { this.unknownReferenceList = value; } } }
            const int recommendedApiVersion = 1;
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            public string Value { get { return ValueBuilder; } }
            #endregion

            #region IEquatable
            public bool Equals(UnknownBlock other)
            {
                return this.blockIndex == other.blockIndex && this.unknown1 == other.unknown1 && this.unknownReferenceList.Equals(other.unknownReferenceList);
            }
            #endregion

        }

        public class UnknownBlockList: DependentList<UnknownBlock>
        {
            private CountedTGIBlockList tgiList;
            public UnknownBlockList(EventHandler handler, CountedTGIBlockList tgiList) : base(handler) { this.tgiList = tgiList; }
            public UnknownBlockList(EventHandler handler, Stream s, CountedTGIBlockList tgiList) : this(handler, tgiList) { Parse(s, tgiList); }

            #region Data I/O
            protected void Parse(Stream s, CountedTGIBlockList tgiList)
            {
                BinaryReader r = new BinaryReader(s);
                int count = r.ReadInt32();
                for (int i = 0; i < count; i++) this.Add(new UnknownBlock(recommendedApiVersion, handler, s, this.tgiList));
            }

            public override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(this.Count);
                foreach (var entry in this) entry.UnParse(s);
            }

            protected override UnknownBlock CreateElement(Stream s) { throw new NotImplementedException(); }
            protected override void WriteElement(Stream s, UnknownBlock element) { throw new NotImplementedException(); }
            #endregion
        }

        public class UnknownReference2List : DependentList<UnknownReference2>
        {
            private CountedTGIBlockList tgiList;
            public UnknownReference2List(EventHandler handler, CountedTGIBlockList tgiList) : base(handler) { this.tgiList = tgiList; }
            public UnknownReference2List(EventHandler handler, Stream s, CountedTGIBlockList tgiList) : this(handler, tgiList) { Parse(s, tgiList); }

            #region Data I/O
            protected void Parse(Stream s, CountedTGIBlockList tgiList)
            {
                BinaryReader r = new BinaryReader(s);
                int count = r.ReadInt32();
                for (int i = 0; i < count; i++) this.Add(new UnknownReference2(recommendedApiVersion, handler, s, this.tgiList));
            }

            public override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(this.Count);
                foreach (var entry in this) entry.UnParse(s);
            }

            protected override UnknownReference2 CreateElement(Stream s) { throw new NotImplementedException(); }
            protected override void WriteElement(Stream s, UnknownReference2 element) { throw new NotImplementedException(); }
            #endregion
        }

        public class UnknownReferenceList : DependentList<UnknownReference>
        {
            private CountedTGIBlockList tgiList;
            public UnknownReferenceList(EventHandler handler, CountedTGIBlockList tgiList) : base(handler) { this.tgiList = tgiList; }
            public UnknownReferenceList(EventHandler handler, Stream s, CountedTGIBlockList tgiList) : this(handler, tgiList) { Parse(s, tgiList); }

            #region Data I/O
            protected void Parse(Stream s, CountedTGIBlockList tgiList)
            {
                BinaryReader r = new BinaryReader(s);
                int count = r.ReadInt32();
                for (int i = 0; i < count; i++) this.Add(new UnknownReference(recommendedApiVersion, handler, s, this.tgiList));
            }

            public override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write(this.Count);
                foreach (var entry in this) entry.UnParse(s);
            }

            protected override UnknownReference CreateElement(Stream s) { throw new NotImplementedException(); }
            protected override void WriteElement(Stream s, UnknownReference element) { throw new NotImplementedException(); }
            #endregion

        }
        #endregion

        public string Value { get { return ValueBuilder; } }

        const int recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }


        #region Content Fields
        [ElementPriority(0)]
        public uint Version { get { return this.version; } set { if (!this.version.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.version = value; } } }
        [ElementPriority(1)]
        public float Unknown1 { get { return this.unknown1; } set { if (!this.unknown1.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown1 = value; } } }
        [ElementPriority(2)]
        public float Unknown2 { get { return this.unknown2; } set { if (!this.unknown2.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown2 = value; } } }
        [ElementPriority(3)]
        public float Unknown3 { get { return this.unknown3; } set { if (!this.unknown3.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown3 = value; } } }
        [ElementPriority(4)]
        public float Unknown4 { get { return this.unknown4; } set { if (!this.unknown4.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown4 = value; } } }
        [ElementPriority(5)]
        public float Unknown5 { get { return this.unknown5; } set { if (!this.unknown5.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown5 = value; } } }
        [ElementPriority(6)]
        public float Unknown6 { get { return this.unknown6; } set { if (!this.unknown6.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown6 = value; } } }
        [ElementPriority(7)]
        public float Unknown7 { get { return this.unknown7; } set { if (!this.unknown7.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown7 = value; } } }
        [ElementPriority(8)]
        public float Unknown8 { get { return this.unknown8; } set { if (!this.unknown8.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown8 = value; } } }
        [ElementPriority(9)]
        public AgeGenderFlags Age { get { return this.age; } set { if (!this.age.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.age = value; } } }
        [ElementPriority(10)]
        public AgeGenderFlags Gender { get { return this.gender; } set { if (!this.gender.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.gender = value; } } }
        [ElementPriority(11)]
        public ulong SkinToneReference { get { return this.skinToneReference; } set { if (!this.skinToneReference.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.skinToneReference = value; } } }
        [ElementPriority(12)]
        public ByteIndexList Unknown9 { get { return this.unknown9; } set { if (!this.unknown9.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown9 = value; } } }
        [ElementPriority(13)]
        public SimpleList<byte> UnknownByteList { get { return this.unknownByteList; } set { if (!this.unknownByteList.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknownByteList = value; } } }
        [ElementPriority(14)]
        public SliderReferenceList SliderReferences1 { get { return this.sliderReferences1; } set { if (!this.sliderReferences1.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.sliderReferences1 = value; } } }
        [ElementPriority(15)]
        public SliderReferenceList SliderReferences2 { get { return this.sliderReferences2; } set { if (!this.sliderReferences2.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.sliderReferences2 = value; } } }
        [ElementPriority(16)]
        public DataBlobHandler Unknown10 { get { return this.unknown10; } set { if (!this.unknown10.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown10 = value; } } }
        [ElementPriority(17)]
        public UnknownBlockList Unknown11 { get { return this.unknown11; } set { if (!this.unknown11.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown11 = value; } } }
        [ElementPriority(18)]
        public SliderReferenceList SliderReferences3 { get { return this.sliderReferences3; } set { if (!this.sliderReferences3.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.sliderReferences3 = value; } } }
        [ElementPriority(19)]
        public SliderReferenceList SliderReferences4 { get { return this.sliderReferences4; } set { if (!this.sliderReferences4.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.sliderReferences4 = value; } } }
        [ElementPriority(20)]
        public DataBlobHandler Unknown12 { get { return this.unknown12; } set { if (!this.unknown12.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown12 = value; } } }
        [ElementPriority(21)]
        public SliderReferenceList SliderReferences5 { get { return this.sliderReferences5; } set { if (!this.sliderReferences5.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.sliderReferences5 = value; } } }
        [ElementPriority(22)]
        public DataBlobHandler Unknown13 { get { return this.unknown13; } set { if (!this.unknown13.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.unknown13 = value; } } }
        [ElementPriority(23)]
        public ulong CASPReference { get { return this.caspReference; } set { if (!this.caspReference.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.caspReference = value; } } }
        [ElementPriority(24)]
        public SimpleList<ulong> DataReferenceList { get { return this.dataReferenceList; } set { if (!this.dataReferenceList.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.dataReferenceList = value; } } }
        [ElementPriority(26)]
        public CountedTGIBlockList TGIList { get { return this.tgiList; } set { if (!this.tgiList.Equals(value)) { OnResourceChanged(this, EventArgs.Empty); this.tgiList = value; } } }
        #endregion
    }

    public class SimOutfitHandler : AResourceHandler
    {
        public SimOutfitHandler()
        {
            if (s4pi.Settings.Settings.IsTS4)
                this.Add(typeof(SimOutfitResource), new List<string>(new string[] { "0x025ED6F4", }));
        }
    }
}

