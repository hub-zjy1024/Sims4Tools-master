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
using s4pi.Interfaces;

namespace WorldDescriptionResource
{
    public class WorldDescriptionResource : AResource
    {
        static bool checking = s4pi.Settings.Settings.Checking;
        const Int32 recommendedApiVersion = 1;

        #region Attributes
        UInt32 version;

        UInt32 worldNameKey;//                 <format=hex>
        UInt32 worldDescriptionKey;//          <format=hex>
        UInt32 simoleonPrice;
        UInt64 regionDescriptionInstanceId; // Region this world belongs to  <format=hex>

        String worldName;

        UInt64 ambienceFileInstanceId;//       <format=hex>
        UInt32 publicSpaceAuralMaterial;//     <format=hex>

        Byte enableTimeOverride;
        Byte hour;
        Byte minute;

        UInt64 hsvTweakerFileInstanceId;//     <format=hex>

        UInt64 descriptorIconFileNameHash;//           <format=hex>
        UInt64 descriptorSelectedIconFileNameHash;//   <format=hex>
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the resource
        /// </summary>
        /// <param name="APIversion">Requested API version</param>
        /// <param name="s">Data stream to use, or null to create from scratch</param>
        public WorldDescriptionResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); dirty = true; } stream.Position = 0; Parse(stream); }
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
            if (checking) if (version != 8)
                    throw new InvalidDataException(String.Format("{0}: unsupported 'version'.  Read '0x{1:X8}', supported: '0x00000008'", this.GetType().Name, version));

            worldNameKey = br.ReadUInt32();//                 <format=hex>
            worldDescriptionKey = br.ReadUInt32();//          <format=hex>
            simoleonPrice = br.ReadUInt32();
            regionDescriptionInstanceId = br.ReadUInt64(); // Region this world belongs to  <format=hex>

            worldName = new String(br.ReadChars(br.ReadInt32()));

            ambienceFileInstanceId = br.ReadUInt64();//       <format=hex>
            publicSpaceAuralMaterial = br.ReadUInt32();//     <format=hex>

            enableTimeOverride = br.ReadByte();
            hour = br.ReadByte();
            minute = br.ReadByte();

            hsvTweakerFileInstanceId = br.ReadUInt64();//     <format=hex>

            descriptorIconFileNameHash = br.ReadUInt64();//           <format=hex>
            descriptorSelectedIconFileNameHash = br.ReadUInt64();//   <format=hex>
        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(version);

            bw.Write(worldNameKey);//                 <format=hex>
            bw.Write(worldDescriptionKey);//          <format=hex>
            bw.Write(simoleonPrice);
            bw.Write(regionDescriptionInstanceId); // Region this world belongs to  <format=hex>

            bw.Write(worldName.Length);
            bw.Write(worldName.ToCharArray());

            bw.Write(ambienceFileInstanceId);//       <format=hex>
            bw.Write(publicSpaceAuralMaterial);//     <format=hex>

            bw.Write(enableTimeOverride);
            bw.Write(hour);
            bw.Write(minute);

            bw.Write(hsvTweakerFileInstanceId);//     <format=hex>

            bw.Write(descriptorIconFileNameHash);//           <format=hex>
            bw.Write(descriptorSelectedIconFileNameHash);//   <format=hex>
            
            bw.Flush();
            return ms;
        }
        #endregion

        public string Value { get { return this.ValueBuilder; } }

        #region Content Fields
        [MinimumVersion(1)]
        [MaximumVersion(recommendedApiVersion)]
        [ElementPriority(1)]
        public UInt32 Version { get { return version; } set { if (version != value) { version = value; OnResourceChanged(this, EventArgs.Empty); } } }

        [ElementPriority(2)]
        public UInt32 WorldNameKey { get { return worldNameKey; } set { if (worldNameKey != value) { worldNameKey = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(3)]
        public UInt32 WorldDescriptionKey { get { return worldDescriptionKey; } set { if (worldDescriptionKey != value) { worldDescriptionKey = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(4)]
        public UInt32 SimoleonPrice { get { return simoleonPrice; } set { if (simoleonPrice != value) { simoleonPrice = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(5)]
        public UInt64 RegionDescriptionInstanceId { get { return regionDescriptionInstanceId; } set { if (regionDescriptionInstanceId != value) { regionDescriptionInstanceId = value; OnResourceChanged(this, EventArgs.Empty); } } }

        [ElementPriority(6)]
        public String WorldName { get { return worldName; } set { if (worldName != value) { worldName = value; OnResourceChanged(this, EventArgs.Empty); } } }

        [ElementPriority(7)]
        public UInt64 AmbienceFileInstanceId { get { return ambienceFileInstanceId; } set { if (ambienceFileInstanceId != value) { ambienceFileInstanceId = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(8)]
        public UInt32 PublicSpaceAuralMaterial { get { return publicSpaceAuralMaterial; } set { if (publicSpaceAuralMaterial != value) { publicSpaceAuralMaterial = value; OnResourceChanged(this, EventArgs.Empty); } } }

        [ElementPriority(9)]
        public Byte EnableTimeOverride { get { return enableTimeOverride; } set { if (enableTimeOverride != value) { enableTimeOverride = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(10)]
        public Byte Hour { get { return hour; } set { if (hour != value) { hour = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(11)]
        public Byte Minute { get { return minute; } set { if (minute != value) { minute = value; OnResourceChanged(this, EventArgs.Empty); } } }

        [ElementPriority(12)]
        public UInt64 HSVTweakerFileInstanceId { get { return hsvTweakerFileInstanceId; } set { if (hsvTweakerFileInstanceId != value) { hsvTweakerFileInstanceId = value; OnResourceChanged(this, EventArgs.Empty); } } }

        [ElementPriority(13)]
        public UInt64 DescriptorIconFileNameHash { get { return descriptorIconFileNameHash; } set { if (descriptorIconFileNameHash != value) { descriptorIconFileNameHash = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(14)]
        public UInt64 DescriptorSelectedIconFileNameHash { get { return descriptorSelectedIconFileNameHash; } set { if (descriptorSelectedIconFileNameHash != value) { descriptorSelectedIconFileNameHash = value; OnResourceChanged(this, EventArgs.Empty); } } }
        #endregion
    }

    public class WorldDescriptionResourceHandler : AResourceHandler
    {
        /// <summary>
        /// Create the content of the Dictionary.
        /// </summary>
        public WorldDescriptionResourceHandler()
        {
            this.Add(typeof(WorldDescriptionResource), new List<string>(new string[] { "0xA680EA4B", }));
        }
    }
}
