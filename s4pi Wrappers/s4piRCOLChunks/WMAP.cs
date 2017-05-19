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
    public class WMAP : ARCOLBlock
    {
        static bool checking = s4pi.Settings.Settings.Checking;
        const string TAG = "WMAP";

        #region Attributes
        UInt32 tag = (uint)FOURCC(TAG);
        UInt32 version = 2;
        Single fov = 0f;
        Single aspectRatio = 0f;
        Vertex cameraPosition;
        Vertex cameraDirection;
        LotInfoList lotInfo;
        #endregion

        #region Constructors
        public WMAP(int APIversion, EventHandler handler) : base(APIversion, handler, null) { }
        public WMAP(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler, s) { }
        public WMAP(int APIversion, EventHandler handler, WMAP basis)
            : base(APIversion, handler, null)
        {
            if (checking) if (basis.version != version)
                    throw new InvalidDataException(String.Format("Invalid Version in basis: '{0}'; expected: '{1}'", basis.version, version));
            this.fov = basis.fov;
            this.aspectRatio = basis.aspectRatio;
            this.cameraPosition = new Vertex(requestedApiVersion, handler, basis.cameraPosition);
            this.cameraDirection = new Vertex(requestedApiVersion, handler, basis.cameraDirection);
            this.lotInfo = new LotInfoList(handler, basis.lotInfo);
        }
        #endregion

        #region ARCOLBlock
        [ElementPriority(2)]
        public override string Tag { get { return TAG; } }

        [ElementPriority(3)]
        public override uint ResourceType { get { return 0x1CC04273; } }

        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            tag = r.ReadUInt32();
            if (checking) if (tag != (uint)FOURCC(TAG))
                    throw new InvalidDataException(String.Format("Invalid Tag read: '{0}'; expected: '{1}'; at 0x{2:X8}", FOURCC(tag), TAG, s.Position));
            UInt32 fileVersion = r.ReadUInt32();
            if (checking) if (fileVersion != version)
                    throw new InvalidDataException(String.Format("Invalid Version read: '{0}'; expected: '{1}'; at 0x{2:X8}", fileVersion, version, s.Position));
            fov = r.ReadSingle();
            aspectRatio = r.ReadSingle();
            cameraPosition = new Vertex(requestedApiVersion, handler, s);
            cameraDirection = new Vertex(requestedApiVersion, handler, s);
            lotInfo = new LotInfoList(handler, s);
        }

        public override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter w = new BinaryWriter(ms);

            w.Write(tag);
            w.Write(version);
            w.Write(fov);
            w.Write(aspectRatio);
            if (cameraPosition == null) cameraPosition = new Vertex(requestedApiVersion, handler);
            cameraPosition.UnParse(ms);
            if (cameraDirection == null) cameraDirection = new Vertex(requestedApiVersion, handler);
            cameraDirection.UnParse(ms);
            if (lotInfo == null) lotInfo = new LotInfoList(handler);
            lotInfo.UnParse(ms);

            return ms;
        }
        #endregion

        #region Sub-types
        public class LotInfoElement : AHandlerElement, IEquatable<LotInfoElement>
        {
            const int recommendedApiVersion = 1;

            #region Attributes
            Vertex position;
            Single rotY;
            Single sizeX;
            Single sizeY;
            UInt32 worldId; // <format=hex> Hash of the world name
            UInt32 lotId;
            #endregion

            #region Constructors
            public LotInfoElement(int APIversion, EventHandler handler)
                : this(APIversion, handler
                , new Vertex(APIversion, handler, 0f, 0f, 0f)
                , 0f
                , 0f, 0f
                , 0
                , 0
                ) { }
            public LotInfoElement(int APIversion, EventHandler handler, LotInfoElement basis)
                : this(APIversion, handler
                , basis.position
                , basis.rotY
                , basis.sizeX , basis.sizeY
                , basis.worldId
                , basis.lotId
                ) { }
            public LotInfoElement(int APIversion, EventHandler handler
                , Vertex position
                , Single rotY
                , Single sizeX, Single sizeY
                , UInt32 worldId
                , UInt32 lotId
                )
                : base(APIversion, handler)
            {
                this.position = new Vertex(requestedApiVersion, handler, position);
                this.rotY = rotY;
                this.sizeX = sizeX;
                this.sizeY = sizeY;
                this.worldId = worldId;
                this.lotId = lotId;
            }
            public LotInfoElement(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            #endregion

            #region Data I/O
            void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                position = new Vertex(requestedApiVersion, handler, s);
                rotY = r.ReadSingle();
                sizeX = r.ReadSingle();
                sizeY = r.ReadSingle();
                worldId = r.ReadUInt32(); // <format=hex> Hash of the world name
                lotId = r.ReadUInt32();
            }

            internal void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                position.UnParse(s);
                w.Write(rotY);
                w.Write(sizeX);
                w.Write(sizeY);
                w.Write(worldId);
                w.Write(lotId);
            }
            #endregion

            #region AHandlerElement Members
            public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
            public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }
            #endregion

            #region IEquatable<LightSource> Members

            public bool Equals(LotInfoElement other)
            {
                return position.Equals(other.position)
                    && rotY.Equals(other.rotY)
                    && sizeX.Equals(other.sizeX)
                    && sizeY.Equals(other.sizeY)
                    && worldId.Equals(other.worldId)
                    && lotId.Equals(other.lotId)
                    ;
            }

            public override bool Equals(object obj)
            {
                return obj as LotInfoElement != null ? this.Equals(obj as LotInfoElement) : false;
            }

            public override int GetHashCode()
            {
                return position.GetHashCode()
                    ^ rotY.GetHashCode()
                    ^ sizeX.GetHashCode()
                    ^ sizeY.GetHashCode()
                    ^ worldId.GetHashCode()
                    ^ lotId.GetHashCode()
                    ;
            }

            #endregion

            #region Content Fields
            [ElementPriority(1)]
            public Vertex Position { get { return position; } set { if (!position.Equals(value)) { position = new Vertex(requestedApiVersion, handler, value); OnElementChanged(); } } }
            public Single RotY { get { return rotY; } set { if (rotY != value) { rotY = value; OnElementChanged(); } } }
            public Single SizeX { get { return sizeX; } set { if (sizeX != value) { sizeX = value; OnElementChanged(); } } }
            public Single SizeY { get { return sizeY; } set { if (sizeY != value) { sizeY = value; OnElementChanged(); } } }
            public UInt32 WorldId { get { return worldId; } set { if (worldId != value) { worldId = value; OnElementChanged(); } } }
            public UInt32 LotId { get { return lotId; } set { if (lotId != value) { lotId = value; OnElementChanged(); } } }
            #endregion

            public string Value { get { return ValueBuilder; } }
        }

        public class LotInfoList : DependentList<LotInfoElement>
        {
            int count;

            #region Constructors
            public LotInfoList(EventHandler handler) : base(handler) { }
            public LotInfoList(EventHandler handler, Stream s) : base(handler, s) { }
            public LotInfoList(EventHandler handler, IEnumerable<LotInfoElement> llp) : base(handler, llp) { }
            #endregion

            #region Data I/O
            protected override LotInfoElement CreateElement(Stream s) { return new LotInfoElement(0, elementHandler, s); }
            protected override void WriteElement(Stream s, LotInfoElement element) { element.UnParse(s); }
            #endregion
        }
        #endregion

        #region Content Fields
        [ElementPriority(11)]
        public UInt32 Version { get { return version; } set { if (version != value) { version = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(12)]
        public Single FOV { get { return fov; } set { if (fov != value) { fov = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(13)]
        public Single AspectRatio { get { return aspectRatio; } set { if (aspectRatio != value) { aspectRatio = value; OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(14)]
        public Vertex CameraPosition { get { return cameraPosition; } set { if (cameraPosition != value) { cameraPosition = value == null ? null : new Vertex(requestedApiVersion, handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(15)]
        public Vertex CameraDirection { get { return cameraDirection; } set { if (cameraDirection != value) { cameraDirection = value == null ? null : new Vertex(requestedApiVersion, handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        [ElementPriority(16)]
        public LotInfoList LotInfo { get { return lotInfo; } set { if (lotInfo != value) { lotInfo = value == null ? null : new LotInfoList(handler, value); OnRCOLChanged(this, EventArgs.Empty); } } }
        #endregion

        public string Value { get { return ValueBuilder; } }
    }
}
