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

namespace ThumbnailCacheResource
{
    public class ThumbnailCacheResource : AResource
    {
        static bool checking = s4pi.Settings.Settings.Checking;
        const Int32 recommendedApiVersion = 1;

        #region Attributes
        private UInt32 version;
        private UInt64 nextInstanceValue;
        private ThumbnailList thumbnails;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the resource
        /// </summary>
        /// <param name="APIversion">Requested API version</param>
        /// <param name="s">Data stream to use, or null to create from scratch</param>
        public ThumbnailCacheResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); dirty = true; } stream.Position = 0; Parse(stream); }
        #endregion

        #region Data I/O
        void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            version = br.ReadUInt32();
            nextInstanceValue = br.ReadUInt64();
            //if (checking) if (version != 6)
            //        throw new InvalidDataException(String.Format("{0}: unsupported 'version'.  Read '0x{1:X8}', supported: '0x00000006'", this.GetType().Name, version));
            thumbnails = new ThumbnailList(OnResourceChanged, s);
        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(version);
            bw.Write(nextInstanceValue);

            if (thumbnails == null) thumbnails = new ThumbnailList(OnResourceChanged);
            thumbnails.UnParse(ms);

            bw.Flush();
            return ms;
        }
        #endregion

        #region AApiVersionedFields
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
        #endregion

        #region Sub-types
        public enum ThumbnailType
        {
            OBJECT,
            SIM,
            SIM_BUST,
            SIM_CAS_PRESET,
            SIM_CAS_PART,
            SIM_COMPLETE_HEAD,
            SIM_FEATURED_OUTFIT,
            SIM_FULLBODY,
            SIM_PORTRAIT_CAS,
            SIM_HOUSEHOLD,
            FLOOR,
            WALL,
            MODEL,
            AVATAR,
            LOT_BLUEPRINT,
            FENCE,
            STAIR,
            RAILING,
            FLOORTRIM_FRIEZE,
            ROOFTRIM,
            LOT_PREVIEW,
            MAGALOG,
            MAGALOG_MASK,
            MEMORY,
            GALLERY,
            PHOTOBOOTH_FAMILY,
            SIM_TRAVEL,
            MAGALOG_EXCHANGE,
            ROOF_PATTERN,
            CEILING_RAIL,
            LOT_PAINT,
            WORLDMAP_LOT,
            SIM_GALLERY,
            BUNDLE_PREVIEW,
            SIM_MANNEQUIN_OUTFIT,
            SIM_PORTRAIT,
        }
        private static ThumbnailType[] thumbnailTypeSim = {
            ThumbnailType.SIM,
            ThumbnailType.SIM_BUST,
            ThumbnailType.SIM_CAS_PRESET,
            ThumbnailType.SIM_CAS_PART,
            ThumbnailType.SIM_COMPLETE_HEAD,
            ThumbnailType.SIM_FEATURED_OUTFIT,
            ThumbnailType.SIM_FULLBODY,
            ThumbnailType.SIM_MANNEQUIN_OUTFIT,
            ThumbnailType.SIM_PORTRAIT_CAS,
            ThumbnailType.SIM_HOUSEHOLD,
            ThumbnailType.PHOTOBOOTH_FAMILY,
            ThumbnailType.SIM_TRAVEL,
            ThumbnailType.SIM_GALLERY,
            ThumbnailType.SIM_PORTRAIT
                                                         };

        public enum ThumbnailSize
        {
            SMALL,
            MEDIUM,
            LARGE,
            EXTRALARGE,
            ENORMOUS,
            MAX
        }

        public class ThumbnailList : DependentList<Thumbnail>
        {
            #region Constructors
            public ThumbnailList(EventHandler handler) : base(handler) { }
            public ThumbnailList(EventHandler handler, Stream s) : base(handler, s) { }
            public ThumbnailList(EventHandler handler, IEnumerable<Thumbnail> llp) : base(handler, llp) { }
            #endregion

            #region Data I/O
            protected override Thumbnail CreateElement(Stream s) { return new Thumbnail(0, elementHandler, s); }
            protected override void WriteElement(Stream s, Thumbnail element) { element.UnParse(s); }
            #endregion
        }

        public class Thumbnail : AHandlerElement, IEquatable<Thumbnail>
        {
            #region Attributes
            ThumbnailType type;
            ThumbnailSize size;
            UInt32 versionType;
            UInt64 resourceID;
            UInt32 index;
            ThumbnailDataList data;
            TGIBlock resourceKey;
            Boolean isAlias;
            #endregion

            #region Constructors
            public Thumbnail(int apiVersion, EventHandler handler)
                : this(apiVersion, handler,
                ThumbnailType.OBJECT, ThumbnailSize.SMALL, 0, 0, 0, new ThumbnailDataList(null, ThumbnailType.OBJECT), new TGIBlock(apiVersion, null), false) { }
            public Thumbnail(int apiVersion, EventHandler handler, Thumbnail basis)
                : this(apiVersion, handler,
                basis.type, basis.size, basis.versionType, basis.resourceID, basis.index, basis.data, basis.resourceKey, basis.isAlias) { }
            public Thumbnail(int apiVersion, EventHandler handler,
                ThumbnailType type, ThumbnailSize size, UInt32 versionType, UInt64 resourceID, UInt32 index, IEnumerable<ThumbnailData> data, TGIBlock resourceKey, Boolean isAlias)
                : base(apiVersion, handler)
            {
                this.type = type;
                this.size = size;
                this.versionType = versionType;
                this.resourceID = resourceID;
                this.index = index;
                this.data = new ThumbnailDataList(handler, data, type);
                this.resourceKey = new TGIBlock(apiVersion, handler, resourceKey);
                this.isAlias = isAlias;
            }

            public Thumbnail(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            #endregion

            #region Data I/O
            public void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                this.type = (ThumbnailType)r.ReadUInt32();
                this.size = (ThumbnailSize)r.ReadUInt32();
                this.versionType = r.ReadUInt32();
                this.resourceID = r.ReadUInt64();
                this.index = r.ReadUInt32();
                this.data = new ThumbnailDataList(handler, s, type);
                this.resourceKey = new TGIBlock(this.requestedApiVersion, handler, s);
                this.isAlias = r.ReadByte() != 0;
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);

                w.Write((UInt32)type);
                w.Write((UInt32)size);
                w.Write(versionType);
                w.Write(resourceID);
                w.Write(index);
                data.UnParse(s);
                resourceKey.UnParse(s);
                w.Write((Byte)(isAlias ? 1 : 0));
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<TableEntry> Members

            public bool Equals(Thumbnail other)
            {
                return type.Equals(other.type)
                    && size.Equals(other.size)
                    && versionType.Equals(other.versionType)
                    && resourceID.Equals(other.resourceID)
                    && index.Equals(other.index)
                    && data.Equals(other.data)
                    && resourceKey.Equals(other.resourceKey)
                    && isAlias.Equals(other.isAlias)
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as Thumbnail != null ? this.Equals(obj as Thumbnail) : false;
            }

            public override int GetHashCode()
            {
                return type.GetHashCode()
                    ^ size.GetHashCode()
                    ^ versionType.GetHashCode()
                    ^ resourceID.GetHashCode()
                    ^ index.GetHashCode()
                    ^ data.GetHashCode()
                    ^ resourceKey.GetHashCode()
                    ^ isAlias.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public ThumbnailType ThumbType { get { return type; } set { if (type != value) { type = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public ThumbnailSize ThumbSize { get { return size; } set { if (size != value) { size = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public UInt32 VersionType { get { return versionType; } set { if (versionType != value) { versionType = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public UInt64 ResourceID { get { return resourceID; } set { if (resourceID != value) { resourceID = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public UInt32 Index { get { return index; } set { if (index != value) { index = value; OnElementChanged(); } } }
            [ElementPriority(6)]
            public ThumbnailDataList ThumbData { get { return data; } set { if (!data.Equals(value)) { data = value == null ? null : new ThumbnailDataList(handler, value, type); OnElementChanged(); } } }
            [ElementPriority(7)]
            public TGIBlock ResourceKey { get { return resourceKey; } set { if (!resourceKey.Equals(value)) { resourceKey = value == null ? null : new TGIBlock(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(8)]
            public Boolean ObjectId { get { return isAlias; } set { if (isAlias != value) { isAlias = value; OnElementChanged(); } } }
            #endregion

            public string Value { get { return ValueBuilder; } }
        }

        public class ThumbnailDataList : DependentList<ThumbnailData>
        {
            ThumbnailType type;

            #region Constructors
            public ThumbnailDataList(EventHandler handler, ThumbnailType type) : base(handler, 1) { this.type = type; }
            public ThumbnailDataList(EventHandler handler, Stream s, ThumbnailType type) : this(null, type) { this.elementHandler = handler; this.Parse(s); this.handler = handler; }
            public ThumbnailDataList(EventHandler handler, IEnumerable<ThumbnailData> llp, ThumbnailType type) : this(null, type) { this.elementHandler = handler; foreach (var t in llp) this.Add((ThumbnailData)t.Clone(null)); this.handler = handler; }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) { return (new BinaryReader(s)).ReadByte() == 0 ? 0 : 1; }
            protected override void WriteCount(Stream s, int count)
            {
                if (count != 0 && count != 1)
                    throw new InvalidOperationException(String.Format("TableEntryDataList should only ever have zero or one entries.  Found {0}.", count));
                (new BinaryWriter(s)).Write((Byte)count);
            }
            protected override ThumbnailData CreateElement(Stream s)
            {
                return ThumbnailData.Factory(0, elementHandler, s, type);
            }
            protected override void WriteElement(Stream s, ThumbnailData element) { element.UnParse(s); }
            #endregion
        }

        public class ThumbnailData : AHandlerElement, IEquatable<ThumbnailData>
        {
            #region Attributes
            protected UInt32 serializationID;
            #endregion

            #region Constructors
            public ThumbnailData(int apiVersion, EventHandler handler, UInt32 serializationID) : base(apiVersion, handler) { this.serializationID = serializationID; }
            public ThumbnailData(int apiVersion, EventHandler handler, ThumbnailData basis) : this(apiVersion, handler, basis.serializationID) { }

            public static ThumbnailData Factory(int APIversion, EventHandler handler, ThumbnailType type)
            {
                if (thumbnailTypeSim.Contains(type))
                {
                    return new ThumbnailData(APIversion, handler, 0);
                }
                else if (type.Equals(ThumbnailType.MODEL) || type.Equals(ThumbnailType.OBJECT))
                {
                    return new ThumbnailDataModelObject(APIversion, handler);
                }
                throw new InvalidOperationException(String.Format("No TableEntryData for ThumbnailType {0}.", type));
            }
            #endregion

            #region Data I/O
            public static ThumbnailData Factory(int APIversion, EventHandler handler, Stream s, ThumbnailType type)
            {
                UInt32 serializationID = new BinaryReader(s).ReadUInt32();
                if (thumbnailTypeSim.Contains(type))
                {
                    if (serializationID.Equals(0x11111111))
                    {
                        return new ThumbnailDataSimCasPart(APIversion, handler, s);
                    }
                    else if (serializationID.Equals(0x00000001))
                    {
                        return new ThumbnailDataSim(APIversion, handler, s);
                    }
                    else if (serializationID.Equals(0x00000002))
                    {
                        return new ThumbnailSimHousehold(APIversion, handler, s);
                    }
                    return new ThumbnailData(APIversion, handler, serializationID);
                }
                else if (type.Equals(ThumbnailType.MODEL) || type.Equals(ThumbnailType.OBJECT))
                {
                    return new ThumbnailDataModelObject(APIversion, handler, s, serializationID);
                }
                throw new InvalidOperationException(String.Format("No TableEntryData for ThumbnailType {0}.", type));
            }
            public virtual void UnParse(Stream s)
            {
                new BinaryWriter(s).Write(serializationID);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<ModelData> Members

            public virtual bool Equals(ThumbnailData other)
            {
                return this.serializationID.Equals(other.serializationID)
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as ThumbnailData != null ? this.Equals(obj as ThumbnailData) : false;
            }

            public override int GetHashCode()
            {
                return serializationID.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public UInt32 ThumbnailDataType { get { return serializationID; } set { if (serializationID != value) { serializationID = value; OnElementChanged(); } } }
            #endregion

            public string Value { get { return this.ValueBuilder; } }
        }

        public class ThumbnailDataSimCasPart : ThumbnailData, IEquatable<ThumbnailDataSimCasPart>
        {
            #region Attributes
            private Byte gender;
            #endregion

            #region Constructors
            public ThumbnailDataSimCasPart(int apiVersion, EventHandler handler) : base(apiVersion, handler, 0x11111111) { }
            public ThumbnailDataSimCasPart(int apiVersion, EventHandler handler, ThumbnailDataSimCasPart basis) : this(apiVersion, handler, basis.gender) { }
            public ThumbnailDataSimCasPart(int apiVersion, EventHandler handler, Byte gender) : this(apiVersion, handler) { this.gender = gender; }
            internal ThumbnailDataSimCasPart(int apiVersion, EventHandler handler, Stream s) : this(apiVersion, handler) { this.Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s) { gender = new BinaryReader(s).ReadByte(); }
            public override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(gender); }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields
            {
                get
                {
                    List<String> contentFields = GetContentFields(requestedApiVersion, this.GetType());
                    contentFields.Remove("ThumbnailDataType");
                    return contentFields;
                }
            }
            #endregion

            #region IEquatable<ThumbnailDataSimCasPart> Members

            public bool Equals(ThumbnailDataSimCasPart other)
            {
                return base.Equals((ThumbnailData)other)
                    && this.gender.Equals(other.gender)
                    ;
            }

            public override bool Equals(ThumbnailData obj)
            {
                return obj as ThumbnailDataSimCasPart != null ? this.Equals(obj as ThumbnailDataSimCasPart) : false;
            }

            public override bool Equals(object obj)
            {
                return obj as ThumbnailDataSimCasPart != null ? this.Equals(obj as ThumbnailDataSimCasPart) : false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode()
                    ^ gender.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public Byte Gender { get { return gender; } set { if (gender != value) { gender = value; OnElementChanged(); } } }
            #endregion
        }

        public class ThumbnailDataSim : ThumbnailData, IEquatable<ThumbnailDataSim>
        {
            #region Attributes
            private UInt64 simID;
            private UInt32 pose;
            #endregion

            #region Constructors
            public ThumbnailDataSim(int apiVersion, EventHandler handler) : base(apiVersion, handler, 0x00000001) { }
            public ThumbnailDataSim(int apiVersion, EventHandler handler, ThumbnailDataSim basis) : this(apiVersion, handler, basis.simID, basis.pose) { }
            public ThumbnailDataSim(int apiVersion, EventHandler handler, UInt64 simID, UInt32 pose) : this(apiVersion, handler) { this.simID = simID; this.pose = pose; }
            internal ThumbnailDataSim(int apiVersion, EventHandler handler, Stream s) : this(apiVersion, handler) { this.Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s) { BinaryReader r = new BinaryReader(s); simID = r.ReadUInt64(); pose = r.ReadUInt32(); }
            public override void UnParse(Stream s) { base.UnParse(s); BinaryWriter w = new BinaryWriter(s); w.Write(simID); w.Write(pose); }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields
            {
                get
                {
                    List<String> contentFields = GetContentFields(requestedApiVersion, this.GetType());
                    contentFields.Remove("ThumbnailDataType");
                    return contentFields;
                }
            }
            #endregion

            #region IEquatable<ThumbnailSimHousehold> Members

            public bool Equals(ThumbnailDataSim other)
            {
                return base.Equals((ThumbnailData)other)
                    && this.simID.Equals(other.simID)
                    && this.pose.Equals(other.pose)
                    ;
            }

            public override bool Equals(ThumbnailData obj)
            {
                return obj as ThumbnailDataSim != null ? this.Equals(obj as ThumbnailDataSim) : false;
            }

            public override bool Equals(object obj)
            {
                return obj as ThumbnailDataSim != null ? this.Equals(obj as ThumbnailDataSim) : false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode()
                    ^ simID.GetHashCode()
                    ^ pose.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public UInt64 FamilyID { get { return simID; } set { if (simID != value) { simID = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public UInt32 Pose { get { return pose; } set { if (pose != value) { pose = value; OnElementChanged(); } } }
            #endregion
        }

        public class ThumbnailSimHousehold : ThumbnailData, IEquatable<ThumbnailSimHousehold>
        {
            #region Attributes
            private UInt64 familyID;
            #endregion

            #region Constructors
            public ThumbnailSimHousehold(int apiVersion, EventHandler handler) : base(apiVersion, handler, 0x00000002) { }
            public ThumbnailSimHousehold(int apiVersion, EventHandler handler, ThumbnailSimHousehold basis) : this(apiVersion, handler, basis.familyID) { }
            public ThumbnailSimHousehold(int apiVersion, EventHandler handler, UInt64 familyID) : this(apiVersion, handler) { this.familyID = familyID; }
            internal ThumbnailSimHousehold(int apiVersion, EventHandler handler, Stream s) : this(apiVersion, handler) { this.Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s) { familyID = new BinaryReader(s).ReadUInt64(); }
            public override void UnParse(Stream s) { base.UnParse(s); new BinaryWriter(s).Write(familyID); }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields
            {
                get
                {
                    List<String> contentFields = GetContentFields(requestedApiVersion, this.GetType());
                    contentFields.Remove("ThumbnailDataType");
                    return contentFields;
                }
            }
            #endregion

            #region IEquatable<ThumbnailSimHousehold> Members

            public bool Equals(ThumbnailSimHousehold other)
            {
                return base.Equals((ThumbnailData)other)
                    && this.familyID.Equals(other.familyID)
                    ;
            }

            public override bool Equals(ThumbnailData obj)
            {
                return obj as ThumbnailSimHousehold != null ? this.Equals(obj as ThumbnailSimHousehold) : false;
            }

            public override bool Equals(object obj)
            {
                return obj as ThumbnailSimHousehold != null ? this.Equals(obj as ThumbnailSimHousehold) : false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode()
                    ^ familyID.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public UInt64 FamilyID { get { return familyID; } set { if (familyID != value) { familyID = value; OnElementChanged(); } } }
            #endregion
        }

        public class ModelData : AHandlerElement, IEquatable<ModelData>
        {
            #region Attributes
            private UInt64 modelKey;
            private Quaternion position;
            #endregion

            #region Constructors
            public ModelData(int apiVersion, EventHandler handler) : this(apiVersion, handler, 0, new Quaternion(apiVersion, handler)) { }
            public ModelData(int apiVersion, EventHandler handler, ModelData basis) : this(apiVersion, handler, basis.modelKey, basis.position) { }
            public ModelData(int apiVersion, EventHandler handler, UInt64 modelKey, Quaternion position)
                : base(apiVersion, handler)
            {
                this.modelKey = modelKey;
                this.position = position;
            }
            public ModelData(int apiVersion, EventHandler handler, Stream s) : this(apiVersion, handler) { this.Parse(s); }
            #endregion

            #region Data I/O
            private void Parse(Stream s)
            {
                modelKey = new BinaryReader(s).ReadUInt64();
                position = new Quaternion(requestedApiVersion, handler, s);
            }
            internal void UnParse(Stream s)
            {
                new BinaryWriter(s).Write(modelKey);
                position.UnParse(s);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<ModelData> Members

            public bool Equals(ModelData other)
            {
                return this.modelKey.Equals(other.modelKey)
                    && this.position.Equals(other.position)
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as ModelData != null ? this.Equals(obj as ModelData) : false;
            }

            public override int GetHashCode()
            {
                return modelKey.GetHashCode()
                    ^ position.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public UInt64 ModelKey { get { return modelKey; } set { if (modelKey != value) { modelKey = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public Quaternion Position { get { return position; } set { if (!position.Equals(value)) { position = value == null ? null : new Quaternion(requestedApiVersion, handler, value); OnElementChanged(); } } }
            #endregion

            public string Value { get { return ValueBuilder; } }
        }

        public class ModelDataList : DependentList<ModelData>
        {
            private UInt16 count;

            #region Constructors
            public ModelDataList(EventHandler handler) : base(handler, UInt16.MaxValue) { }
            public ModelDataList(EventHandler handler, Stream s, UInt16 count) : this(handler) { this.count = count; this.elementHandler = handler; this.Parse(s); this.handler = handler; }
            public ModelDataList(EventHandler handler, IEnumerable<ModelData> llp) : base(handler, llp, UInt16.MaxValue) { }
            #endregion

            #region Data I/O
            protected override int ReadCount(Stream s) { return count; }
            protected override void WriteCount(Stream s, int count) { }
            protected override ModelData CreateElement(Stream s) { return new ModelData(0, elementHandler, s); }
            protected override void WriteElement(Stream s, ModelData element) { element.UnParse(s); }
            #endregion
        }

        [Flags]
        public enum ModelInfoValueFlag : ulong
        {
            unknown17 = 0x00010000,
            unknown18 = 0x00020000,
            hasModelIndex19 = 0x00040000,
            hasModelIndex20 = 0x00080000,
            hasModelIndex21 = 0x00100000,
            hasModelIndex22 = 0x00200000,
            unknown23 = 0x004000000,
            unknown24 = 0x008000000,
            hasPaintingGroup25 = 0x01000000,
            hasMaterialState26 = 0x02000000,
            unknown27 = 0x04000000,
            unknown28 = 0x08000000,
            unknown29 = 0x10000000,
            unknown30 = 0x20000000,
            hasGeoState31 = 0x40000000,
            hasPaintingKey32 = 0x80000000,
        }

        public abstract class ModelInfoValue : AHandlerElement, IEquatable<ModelInfoValue>
        {
            public ModelInfoValue(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }

            #region IEquatable<ModelData> Members

            public virtual bool Equals(ModelInfoValue other)
            {
                return true
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as ModelInfoValue != null ? this.Equals(obj as ModelInfoValue) : false;
            }

            public override int GetHashCode()
            {
                return 0.GetHashCode()
                    ;
            }

            #endregion

            public abstract void UnParse(Stream s);

            public string Value { get { return ValueBuilder; } }
        }

        public class ModelInfoValueByte : ModelInfoValue, IEquatable<ModelInfoValueByte>
        {
            #region Attributes
            private Byte data;
            #endregion

            #region Constructors
            public ModelInfoValueByte(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public ModelInfoValueByte(int apiVersion, EventHandler handler, ModelInfoValueByte basis) : this(apiVersion, handler, basis.data) { }
            public ModelInfoValueByte(int apiVersion, EventHandler handler, Byte data) : this(apiVersion, handler) { this.data = data; }
            public ModelInfoValueByte(int apiVersion, EventHandler handler, Stream s) : this(apiVersion, handler) { this.Parse(s); }
            static public implicit operator ModelInfoValueByte(ModelInfoValueUInt32 value) { return new ModelInfoValueByte(0, null, (Byte)(value.Data & 0xFF)); }
            static public implicit operator ModelInfoValueByte(ModelInfoValueUInt64 value) { return new ModelInfoValueByte(0, null, (Byte)(value.Data & 0xFF)); ; }
            #endregion

            #region Data I/O
            private void Parse(Stream s) { BinaryReader r = new BinaryReader(s); data = r.ReadByte(); }
            public override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(data); }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<ModelInfoValueByte> Members

            public bool Equals(ModelInfoValueByte other)
            {
                return base.Equals((ModelInfoValue)other)
                    && this.data.Equals(other.data)
                    ;
            }

            public override bool Equals(ModelInfoValue obj)
            {
                return obj as ModelInfoValueByte != null ? this.Equals(obj as ModelInfoValueByte) : false;
            }

            public override bool Equals(object obj)
            {
                return obj as ModelInfoValueByte != null ? this.Equals(obj as ModelInfoValueByte) : false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode()
                    ^ data.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public Byte Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            #endregion
        }

        public class ModelInfoValueUInt32 : ModelInfoValue, IEquatable<ModelInfoValueUInt32>
        {
            #region Attributes
            private UInt32 data;
            #endregion

            #region Constructors
            public ModelInfoValueUInt32(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public ModelInfoValueUInt32(int apiVersion, EventHandler handler, ModelInfoValueUInt32 basis) : this(apiVersion, handler, basis.data) { }
            public ModelInfoValueUInt32(int apiVersion, EventHandler handler, UInt32 data) : this(apiVersion, handler) { this.data = data; }
            public ModelInfoValueUInt32(int apiVersion, EventHandler handler, Stream s) : this(apiVersion, handler) { this.Parse(s); }
            static public implicit operator ModelInfoValueUInt32(ModelInfoValueByte value) { return new ModelInfoValueUInt32(0, null, (UInt32)(value.Data & 0xFFFFFFFF)); }
            static public implicit operator ModelInfoValueUInt32(ModelInfoValueUInt64 value) { return new ModelInfoValueUInt32(0, null, (UInt32)(value.Data & 0xFFFFFFFF)); }
            #endregion

            #region Data I/O
            private void Parse(Stream s) { BinaryReader r = new BinaryReader(s); data = r.ReadUInt32(); }
            public override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(data); }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<ModelInfoValueUInt32> Members

            public bool Equals(ModelInfoValueUInt32 other)
            {
                return base.Equals((ModelInfoValue)other)
                    && this.data.Equals(other.data)
                    ;
            }

            public override bool Equals(ModelInfoValue obj)
            {
                return obj as ModelInfoValueUInt32 != null ? this.Equals(obj as ModelInfoValueUInt32) : false;
            }

            public override bool Equals(object obj)
            {
                return obj as ModelInfoValueUInt32 != null ? this.Equals(obj as ModelInfoValueUInt32) : false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode()
                    ^ data.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public UInt32 Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            #endregion
        }

        public class ModelInfoValueUInt64 : ModelInfoValue, IEquatable<ModelInfoValueUInt64>
        {
            #region Attributes
            private UInt64 data;
            #endregion

            #region Constructors
            public ModelInfoValueUInt64(int apiVersion, EventHandler handler) : base(apiVersion, handler) { }
            public ModelInfoValueUInt64(int apiVersion, EventHandler handler, ModelInfoValueUInt64 basis) : this(apiVersion, handler, basis.data) { }
            public ModelInfoValueUInt64(int apiVersion, EventHandler handler, UInt64 data) : this(apiVersion, handler) { this.data = data; }
            public ModelInfoValueUInt64(int apiVersion, EventHandler handler, Stream s) : this(apiVersion, handler) { this.Parse(s); }
            static public implicit operator ModelInfoValueUInt64(ModelInfoValueByte value) { return new ModelInfoValueUInt64(0, null, (UInt64)(value.Data)); }
            static public implicit operator ModelInfoValueUInt64(ModelInfoValueUInt32 value) { return new ModelInfoValueUInt64(0, null, (UInt64)(value.Data)); }
            #endregion

            #region Data I/O
            private void Parse(Stream s) { BinaryReader r = new BinaryReader(s); data = r.ReadUInt64(); }
            public override void UnParse(Stream s) { BinaryWriter w = new BinaryWriter(s); w.Write(data); }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<ModelInfoValueUInt32> Members

            public bool Equals(ModelInfoValueUInt64 other)
            {
                return base.Equals((ModelInfoValue)other)
                    && this.data.Equals(other.data)
                    ;
            }

            public override bool Equals(ModelInfoValue obj)
            {
                return obj as ModelInfoValueUInt64 != null ? this.Equals(obj as ModelInfoValueUInt64) : false;
            }

            public override bool Equals(object obj)
            {
                return obj as ModelInfoValueUInt64 != null ? this.Equals(obj as ModelInfoValueUInt64) : false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode()
                    ^ data.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public UInt64 Data { get { return data; } set { if (data != value) { data = value; OnElementChanged(); } } }
            #endregion
        }

        public class ModelInfoDict : DependentDictionary<ModelInfoValueFlag, ModelInfoValue>
        {
            #region Constructors
            public ModelInfoDict(EventHandler handler) : base(handler) { }
            public ModelInfoDict(EventHandler handler, Stream s) : this(null) { throw new NotImplementedException(); }
            public ModelInfoDict(EventHandler handler, IDictionary<ModelInfoValueFlag, ModelInfoValue> dictionary) : this(null) { elementHandler = handler; this.AddRange(dictionary); this.handler = handler; }
            #endregion

            #region Data I/O
            protected override void Parse(Stream s) { throw new NotImplementedException(); }
            public override void UnParse(Stream s) { throw new NotImplementedException(); }
            protected override Int32 ReadCount(Stream s) { throw new NotImplementedException(); }
            protected override void WriteCount(Stream s, Int32 count) { throw new NotImplementedException(); }
            protected override ModelInfoValueFlag CreateKey(Stream s) { throw new NotImplementedException(); }
            protected override ModelInfoValue CreateValue(Stream s) { throw new NotImplementedException(); }
            protected override void WriteKey(Stream s, ModelInfoValueFlag key) { throw new NotImplementedException(); }
            protected override void WriteValue(Stream s, ModelInfoValue value) { throw new NotImplementedException(); }
            #endregion

            public ModelInfoValueFlag Flags
            {
                get
                {
                    UInt32 res = 0x00000000;
                    foreach (var key in this.Keys)
                        res &= (UInt32)key;
                    return (ModelInfoValueFlag)res;
                }
            }

            public override ModelInfoValue this[ModelInfoValueFlag key]
            {
                get
                {
                    return base[key];
                }
                set
                {
                    switch ((UInt32)key)
                    {
                        case 0x01000000: //ModelInfoValueFlags.hasPaintingGroup25
                            base[key] = new ModelInfoValueUInt32(recommendedApiVersion, handler, (ModelInfoValueUInt32)value);
                            break;
                        case 0x02000000: //ModelInfoValueFlags.hasMaterialState26
                            base[key] = new ModelInfoValueUInt32(recommendedApiVersion, handler, (ModelInfoValueUInt32)value);
                            break;
                        case 0x40000000: //ModelInfoValueFlags.hasGeoState31
                            base[key] = new ModelInfoValueUInt32(recommendedApiVersion, handler, (ModelInfoValueUInt32)value);
                            break;
                        case 0x80000000: //ModelInfoValueFlags.hasPaintingKey32
                            base[key] = new ModelInfoValueUInt64(recommendedApiVersion, handler, (ModelInfoValueUInt64)value);
                            break;
                        default:
                            if (((UInt32)key & 0x003C0000) != 0) //ModelInfoValueFlags.hasModelIndex19-22 - but only one can have a value, setting enforces this
                            {
                                // Remove any existing hasModelIndex (other than that under 'key')
                                for (var i = (UInt32)ModelInfoValueFlag.hasModelIndex19; !base.ContainsKey(key) && ((UInt32)Flags & 0x003C0000) != 0; i <<= 1)
                                    this.Remove((ModelInfoValueFlag)i);
                                base[key] = new ModelInfoValueByte(recommendedApiVersion, handler, (ModelInfoValueByte)value);
                                break;
                            }
                            else
                                throw new ArgumentException(String.Format("Unsupported ModelInfoValueFlag {0}", key));
                    }
                }
            }
        }

        public class ThumbnailDataModelObject : ThumbnailData, IEquatable<ThumbnailDataModelObject>
        {
            #region Attributes
            private ModelInfoValueFlag flags;
            private ModelDataList data;
            private ModelInfoDict info;
            #endregion

            #region Constructors
            public ThumbnailDataModelObject(int apiVersion, EventHandler handler)
                : this(apiVersion, handler,
                0, new ModelDataList(null), new ModelInfoDict(null)) { }
            public ThumbnailDataModelObject(int apiVersion, EventHandler handler, ThumbnailDataModelObject basis)
                : this(apiVersion, handler,
                basis.flags, basis.data, basis.info) { }
            public ThumbnailDataModelObject(int apiVersion, EventHandler handler,
                ModelInfoValueFlag flags, IEnumerable<ModelData> data, IDictionary<ModelInfoValueFlag, ModelInfoValue> info)
                : base(apiVersion, handler, 0)
            {
                this.flags = flags;
                this.data = new ModelDataList(handler, data);
                this.info = new ModelInfoDict(handler, info);

                // It is not that simple, however.  flags and info need to be consistent.

                // First, sort out hasModelIndex...  This is even more complicated because multiple bits indicate "yes, that value should be there".
                // Just having the value does not tell us which bit, though.  So I picked hasModelIndex19.
                ModelInfoValueFlag flagsHasModelIndex = flags & (ModelInfoValueFlag)~(UInt32)0x003C0000; // what did we get told in 'flags'?
                ModelInfoValueFlag infoHasModelIndex = this.info.Flags & (ModelInfoValueFlag)~(UInt32)0x003C0000; // what did we get told in 'info'?
                if (flagsHasModelIndex == 0 && infoHasModelIndex != 0) // we have an inconsistent state
                    this.flags |= ModelInfoValueFlag.hasModelIndex19; // arbitrary flag just to make it consistent, rather than throw away 'info' value
                else if (flagsHasModelIndex != 0 && infoHasModelIndex == 0) // we have an inconsistent state
                    this.flags &= (ModelInfoValueFlag)~(UInt32)0x003C0000; // clear all the flags as we have no 'info' value
                
                // Moving on... We can just clear the remaining flags for which we may have 'info' values and set them again if we have
                this.flags &= (ModelInfoValueFlag)~(UInt32)0xC3000000;
                this.flags |= this.info.Flags & (ModelInfoValueFlag)~(UInt32)0x003C0000;

                // Now expose this to the parent
                base.serializationID = ((UInt32)this.flags | (UInt32)this.data.Count);
            }
            public ThumbnailDataModelObject(int APIversion, EventHandler handler, Stream s, UInt32 serializationID) : base(APIversion, handler, serializationID) { Parse(s); }
            #endregion

            #region Data I/O
            public void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                this.flags = (ModelInfoValueFlag)(serializationID & 0xFFFF0000);
                this.data = new ModelDataList(handler, s, (UInt16)(serializationID & 0xFFFF));

                this.info = new ModelInfoDict(handler);
                if ((flags & ModelInfoValueFlag.hasPaintingKey32) != 0) info[ModelInfoValueFlag.hasPaintingKey32] = new ModelInfoValueUInt64(requestedApiVersion, handler, r.ReadUInt64());
                if ((flags & ModelInfoValueFlag.hasPaintingGroup25) != 0) info[ModelInfoValueFlag.hasPaintingGroup25] = new ModelInfoValueUInt32(requestedApiVersion, handler, r.ReadUInt32());
                if ((flags & ModelInfoValueFlag.hasGeoState31) != 0) info[ModelInfoValueFlag.hasGeoState31] = new ModelInfoValueUInt32(requestedApiVersion, handler, r.ReadUInt32());
                if ((flags & ModelInfoValueFlag.hasMaterialState26) != 0) info[ModelInfoValueFlag.hasMaterialState26] = new ModelInfoValueUInt32(requestedApiVersion, handler, r.ReadUInt32());

                // hasModelIndex is not so simple, of course.  We will take the lowest order bit...
                for (UInt32 i = (UInt32)ModelInfoValueFlag.hasModelIndex19, j = 0; ((UInt32)flags & 0x003C0000) != 0 && j < 4; i <<= 1, j++)
                    if (((UInt32)flags & i) != 0)
                    {
                        info[(ModelInfoValueFlag)i] = new ModelInfoValueByte(requestedApiVersion, handler, r.ReadByte());
                        break;
                    }
            }

            public override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);

                // Make sure things are sane again... same as constructor
                ModelInfoValueFlag flagsHasModelIndex = this.flags & (ModelInfoValueFlag)~(UInt32)0x003C0000;
                ModelInfoValueFlag infoHasModelIndex = this.info.Flags & (ModelInfoValueFlag)~(UInt32)0x003C0000;
                if (flagsHasModelIndex == 0 && infoHasModelIndex != 0) // we have an inconsistent state
                    this.flags |= ModelInfoValueFlag.hasModelIndex19;
                else if (flagsHasModelIndex != 0 && infoHasModelIndex == 0) // we have an inconsistent state
                    this.flags &= (ModelInfoValueFlag)~(UInt32)0x003C0000;

                this.flags &= (ModelInfoValueFlag)~0xC3000000;
                this.flags |= (this.info.Flags & (ModelInfoValueFlag)~(UInt32)0x003C0000);

                base.serializationID = ((UInt32)this.flags | (UInt32)this.data.Count);

                base.UnParse(s);
                this.data.UnParse(s);
                if ((flags & ModelInfoValueFlag.hasPaintingKey32) != 0) info[ModelInfoValueFlag.hasPaintingKey32].UnParse(s);
                if ((flags & ModelInfoValueFlag.hasPaintingGroup25) != 0) info[ModelInfoValueFlag.hasPaintingGroup25].UnParse(s);
                if ((flags & ModelInfoValueFlag.hasGeoState31) != 0) info[ModelInfoValueFlag.hasGeoState31].UnParse(s);
                if ((flags & ModelInfoValueFlag.hasMaterialState26) != 0) info[ModelInfoValueFlag.hasMaterialState26].UnParse(s);

                for (UInt32 i = (UInt32)ModelInfoValueFlag.hasModelIndex19, j = 0; ((UInt32)flags & 0x003C0000) != 0 && j < 4; i <<= 1, j++)
                    if (((UInt32)flags & i) != 0)
                    {
                        info[(ModelInfoValueFlag)i].UnParse(s);
                        break;
                    }
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields
            {
                get
                {
                    List<String> contentFields = GetContentFields(requestedApiVersion, this.GetType());
                    contentFields.Remove("ThumbnailDataType");
                    return contentFields;
                }
            }
            #endregion

            #region IEquatable<ThumbnailDataModelObject> Members

            public bool Equals(ThumbnailDataModelObject other)
            {
                return base.Equals((ThumbnailData)other)
                    && data.Equals(other.data)
                    && info.Equals(other.info)
                    ;
            }

            public override bool Equals(ThumbnailData obj)
            {
                return obj as ThumbnailDataModelObject != null ? this.Equals(obj as ThumbnailDataModelObject) : false;
            }

            public override bool Equals(object obj)
            {
                return obj as ThumbnailDataModelObject != null ? this.Equals(obj as ThumbnailDataModelObject) : false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode()
                    ^ data.GetHashCode()
                    ^ info.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public ModelDataList Data { get { return data; } set { if (!data.Equals(value)) { data = value == null ? null : new ModelDataList(handler, value); OnElementChanged(); } } }
            [ElementPriority(1)]
            public ModelInfoDict Info { get { return info; } set { if (!info.Equals(value)) { info = value == null ? null : new ModelInfoDict(handler, value); OnElementChanged(); } } }
            #endregion
        }




        #endregion

        #region Content Fields
        [MinimumVersion(1)]
        [MaximumVersion(recommendedApiVersion)]
        [ElementPriority(1)]
        public UInt32 Version { get { return version; } set { if (version != value) { version = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        public UInt64 NextInstanceValue { get { return nextInstanceValue; } set { if (nextInstanceValue != value) { nextInstanceValue = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(3)]
        public ThumbnailList Thumnails { get { return thumbnails; } set { if (!thumbnails.Equals(value)) { thumbnails = value == null ? null : new ThumbnailList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }
        #endregion

        public string Value { get { return this.ValueBuilder; } }
    }

    public class ThumbnailCacheResourceHandler : AResourceHandler
    {
        /// <summary>
        /// Create the content of the Dictionary.
        /// </summary>
        public ThumbnailCacheResourceHandler()
        {
            this.Add(typeof(ThumbnailCacheResource), new List<string>(new string[] { "0xB93A9915", }));
        }
    }
}
