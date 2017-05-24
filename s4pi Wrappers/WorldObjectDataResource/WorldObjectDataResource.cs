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

namespace WorldObjectDataResource
{
    public class WorldObjectDataResource : AResource
    {
        static bool checking = s4pi.Settings.Settings.Checking;
        const Int32 recommendedApiVersion = 1;

        #region Attributes
        private UInt32 version;
        private LotList lots;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the resource
        /// </summary>
        /// <param name="APIversion">Requested API version</param>
        /// <param name="s">Data stream to use, or null to create from scratch</param>
        public WorldObjectDataResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); dirty = true; } stream.Position = 0; Parse(stream); }
        #endregion

        #region Data I/O
        void Parse(Stream s)
        {
            BinaryReader br = new BinaryReader(s);
            version = br.ReadUInt32();
            if (checking) if (version != 7)
                    throw new InvalidDataException(String.Format("{0}: unsupported 'version'.  Read '0x{1:X8}', supported: '0x00000007'", this.GetType().Name, version));
            lots = new LotList(OnResourceChanged, s);
        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(version);

            if (lots == null) lots = new LotList(OnResourceChanged);
            lots.UnParse(ms);

            bw.Flush();
            return ms;
        }
        #endregion

        #region AApiVersionedFields
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
        #endregion

        #region Sub-types
        public class Obj : AHandlerElement, IEquatable<Obj>
        {
            #region Attributes
            Locator locator;
            UInt64 objectId;
            SByte minSpecLOD;
            UInt64 parentId;
            UInt32 slotHash;
            #endregion

            #region Constructors
            public Obj(int apiVersion, EventHandler handler) : this(apiVersion, handler,
                new Locator(apiVersion, handler), 0, 0, 0, 0) { }
            public Obj(int apiVersion, EventHandler handler, Obj basis)
                : this(apiVersion, handler,
                basis.locator, basis.objectId, basis.minSpecLOD, basis.parentId, basis.slotHash) { }
            public Obj(int apiVersion, EventHandler handler,
                Locator locator, UInt64 objectId, SByte minSpecLOD, UInt64 parentId, UInt32 slotHash)
                : base(apiVersion, handler)
            {
                this.locator = new Locator(apiVersion, handler, locator);
                this.objectId = objectId;
                this.minSpecLOD = minSpecLOD;
                this.parentId = parentId;
                this.slotHash = slotHash;
            }
            public Obj(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                locator = new Locator(requestedApiVersion, handler, s);
                objectId = r.ReadUInt64();
                minSpecLOD = r.ReadSByte();
                parentId = r.ReadUInt64();
                slotHash = r.ReadUInt32();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);

                locator.UnParse(s);
                w.Write(objectId);
                w.Write(minSpecLOD);
                w.Write(parentId);
                w.Write(slotHash);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Obj> Members

            public bool Equals(Obj other)
            {
                return locator.Equals(other.locator)
                    && objectId.Equals(other.objectId)
                    && minSpecLOD.Equals(other.minSpecLOD)
                    && parentId.Equals(other.parentId)
                    && slotHash.Equals(other.slotHash)
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as Obj != null ? this.Equals(obj as Obj) : false;
            }

            public override int GetHashCode()
            {
                return locator.GetHashCode()
                    ^ objectId.GetHashCode()
                    ^ minSpecLOD.GetHashCode()
                    ^ parentId.GetHashCode()
                    ^ slotHash.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public Locator Locator { get { return locator; } set { if (!locator.Equals(value)) { locator = value == null ? null : new Locator(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(2)]
            public UInt64 ObjectId { get { return objectId; } set { if (objectId != value) { objectId = value; OnElementChanged(); } } }
            [ElementPriority(3)]
            public SByte MinSpecLOD { get { return minSpecLOD; } set { if (minSpecLOD != value) { minSpecLOD = value; OnElementChanged(); } } }
            [ElementPriority(4)]
            public UInt64 ParentId { get { return parentId; } set { if (parentId != value) { parentId = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public UInt32 SlotHash { get { return slotHash; } set { if (slotHash != value) { slotHash = value; OnElementChanged(); } } }
            #endregion

            public string Value { get { return ValueBuilder; } }
        }
        public class ObjList : DependentList<Obj>
        {
            #region Constructors
            public ObjList(EventHandler handler) : base(handler) { }
            public ObjList(EventHandler handler, Stream s) : base(handler, s) { }
            public ObjList(EventHandler handler, IEnumerable<Obj> llp) : base(handler, llp) { }
            #endregion

            #region Data I/O
            protected override Obj CreateElement(Stream s) { return new Obj(0, elementHandler, s); }
            protected override void WriteElement(Stream s, Obj element) { element.UnParse(s); }
            #endregion
        }

        public class Locator : AHandlerElement, IEquatable<Locator>
        {
            #region Attributes
            TGIBlock footprintKey;
            Vertex position;
            Quaternion rotation;
            Single scale;
            UInt64 objDefGuid;
            #endregion

            #region Constructors
            public Locator(int apiVersion, EventHandler handler) : this(apiVersion, handler,
                new TGIBlock(apiVersion, handler), new Vertex(apiVersion, handler), new Quaternion(apiVersion, handler), 0f, 0) { }
            public Locator(int apiVersion, EventHandler handler, Locator basis)
                : this(apiVersion, handler,
                basis.footprintKey, basis.position, basis.rotation, basis.scale, basis.objDefGuid) { }
            public Locator(int apiVersion, EventHandler handler,
                TGIBlock footprintKey, Vertex position, Quaternion rotation, Single scale, UInt64 objDefGuid)
                : base(apiVersion, handler)
            {
                this.footprintKey = new TGIBlock(apiVersion, handler, footprintKey);
                this.position = new Vertex(apiVersion, handler, position);
                this.rotation = new Quaternion(apiVersion, handler, rotation);
                this.scale = scale;
                this.objDefGuid = objDefGuid;
            }
            public Locator(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                footprintKey = new TGIBlock(requestedApiVersion, handler, s);
                position = new Vertex(requestedApiVersion, handler, s);
                rotation = new Quaternion(requestedApiVersion, handler, s);
                scale = r.ReadSingle();
                objDefGuid = r.ReadUInt64();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);

                footprintKey.UnParse(s);
                position.UnParse(s);
                rotation.UnParse(s);
                w.Write(scale);
                w.Write(objDefGuid);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Locator> Members

            public bool Equals(Locator other)
            {
                return footprintKey.Equals(other.footprintKey)
                    && position.Equals(other.position)
                    && rotation.Equals(other.rotation)
                    && scale.Equals(other.scale)
                    && objDefGuid.Equals(other.objDefGuid)
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as Locator != null ? this.Equals(obj as Locator) : false;
            }

            public override int GetHashCode()
            {
                return footprintKey.GetHashCode()
                    ^ position.GetHashCode()
                    ^ rotation.GetHashCode()
                    ^ scale.GetHashCode()
                    ^ objDefGuid.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public TGIBlock FootprintKey { get { return footprintKey; } set { if (!footprintKey.Equals(value)) { footprintKey = value == null ? null : new TGIBlock(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(2)]
            public Vertex Position { get { return position; } set { if (!position.Equals(value)) { position = value == null ? null : new Vertex(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(3)]
            public Quaternion Rotation { get { return rotation; } set { if (!rotation.Equals(value)) { rotation = value == null ? null : new Quaternion(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(4)]
            public Single Scale { get { return scale; } set { if (scale != value) { scale = value; OnElementChanged(); } } }
            [ElementPriority(5)]
            public UInt64 ObjDefGuid { get { return objDefGuid; } set { if (objDefGuid != value) { objDefGuid = value; OnElementChanged(); } } }
            #endregion

            public string Value { get { return ValueBuilder; } }
        }
        public class LocatorList : DependentList<Locator>
        {
            #region Constructors
            public LocatorList(EventHandler handler) : base(handler) { }
            public LocatorList(EventHandler handler, Stream s) : base(handler, s) { }
            public LocatorList(EventHandler handler, IEnumerable<Locator> llp) : base(handler, llp) { }
            #endregion

            #region Data I/O
            protected override Locator CreateElement(Stream s) { return new Locator(0, elementHandler, s); }
            protected override void WriteElement(Stream s, Locator element) { element.UnParse(s); }
            #endregion
        }

        public class Lot : AHandlerElement, IEquatable<Lot>
        {
            #region Attributes
            private UInt32 lotId;
            private TGIBlock footprintKey;
            private ObjList objects;
            private ObjList objectsNoScript;
            private LocatorList locators;
            #endregion

            #region Constructors
            public Lot(int apiVersion, EventHandler handler) : this(apiVersion, handler,
                0, new TGIBlock(apiVersion, handler), new ObjList(handler), new ObjList(handler), new LocatorList(handler)) { }
            public Lot(int apiVersion, EventHandler handler, Lot basis) : this(apiVersion, handler,
                basis.lotId, basis.footprintKey, basis.objects, basis.objectsNoScript, basis.locators) { }
            public Lot(int apiVersion, EventHandler handler,
                UInt32 lotId, TGIBlock footprintKey, ObjList objects, ObjList objectsNoScript, LocatorList locators) : base(apiVersion, handler)
            {
                this.lotId = lotId;
                this.footprintKey = new TGIBlock(apiVersion, handler, footprintKey);
                this.objects = new ObjList(handler, objects);
                this.objectsNoScript = new ObjList(handler, objectsNoScript);
                this.locators = new LocatorList(handler, locators);
            }
            public Lot(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                lotId = r.ReadUInt32();
                footprintKey = new TGIBlock(requestedApiVersion, handler, s);
                objects = new ObjList(handler, s);
                objectsNoScript = new ObjList(handler, s);
                locators = new LocatorList(handler, s);
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);

                w.Write(lotId);
                footprintKey.UnParse(s);
                objects.UnParse(s);
                objectsNoScript.UnParse(s);
                locators.UnParse(s);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<Lot> Members

            public bool Equals(Lot other)
            {
                return lotId.Equals(other.lotId)
                    && footprintKey.Equals(other.footprintKey)
                    && objects.Equals(other.objects)
                    && objectsNoScript.Equals(other.objectsNoScript)
                    && locators.Equals(other.locators)
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as Lot != null ? this.Equals(obj as Lot) : false;
            }

            public override int GetHashCode()
            {
                return lotId.GetHashCode()
                    ^ footprintKey.GetHashCode()
                    ^ objects.GetHashCode()
                    ^ objectsNoScript.GetHashCode()
                    ^ locators.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [MinimumVersion(1)]
            [MaximumVersion(recommendedApiVersion)]
            [ElementPriority(1)]
            public UInt32 LotId { get { return lotId; } set { if (lotId != value) { lotId = value; OnElementChanged(); } } }
            [ElementPriority(2)]
            public TGIBlock FootprintKey { get { return footprintKey; } set { if (!footprintKey.Equals(value)) { footprintKey = value == null ? null : new TGIBlock(requestedApiVersion, handler, value); OnElementChanged(); } } }
            [ElementPriority(3)]
            public ObjList Objects { get { return objects; } set { if (!objects.Equals(value)) { objects = value == null ? null : new ObjList(handler, value); OnElementChanged(); } } }
            [ElementPriority(4)]
            public ObjList ObjectsNoScript { get { return objectsNoScript; } set { if (!objectsNoScript.Equals(value)) { objectsNoScript = value == null ? null : new ObjList(handler, value); OnElementChanged(); } } }
            [ElementPriority(5)]
            public LocatorList Locators { get { return locators; } set { if (!locators.Equals(value)) { locators = value == null ? null : new LocatorList(handler, value); OnElementChanged(); } } }
            #endregion

            public string Value { get { return ValueBuilder; } }
        }
        public class LotList : DependentList<Lot>
        {
            #region Constructors
            public LotList(EventHandler handler) : base(handler) { }
            public LotList(EventHandler handler, Stream s) : base(handler, s) { }
            public LotList(EventHandler handler, IEnumerable<Lot> llp) : base(handler, llp) { }
            #endregion

            #region Data I/O
            protected override Lot CreateElement(Stream s) { return new Lot(0, elementHandler, s); }
            protected override void WriteElement(Stream s, Lot element) { element.UnParse(s); }
            #endregion
        }
        #endregion

        #region Content Fields
        [MinimumVersion(1)]
        [MaximumVersion(recommendedApiVersion)]
        [ElementPriority(1)]
        public UInt32 Version { get { return version; } set { if (version != value) { version = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(2)]
        public LotList Lots { get { return lots; } set { if (!lots.Equals(value)) { lots = value == null ? null : new LotList(OnResourceChanged, value); OnResourceChanged(this, EventArgs.Empty); } } }
        #endregion

        public string Value { get { return this.ValueBuilder; } }
    }

    public class WorldObjectDataResourceHandler : AResourceHandler
    {
        /// <summary>
        /// Create the content of the Dictionary.
        /// </summary>
        public WorldObjectDataResourceHandler()
        {
            this.Add(typeof(WorldObjectDataResource), new List<string>(new string[] { "0xFCB1A1E4", }));
        }
    }
}
