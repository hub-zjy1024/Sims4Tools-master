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
 *  s3pi is distributed in the hope that it will be useful,                *
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *  GNU General Public License for more details.                           *
 *                                                                         *
 *  You should have received a copy of the GNU General Public License      *
 *  along with s4pi.  If not, see <http://www.gnu.org/licenses/>.          *
 ***************************************************************************/
using s4pi.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace s4pi.Miscellaneous
{
    /// <summary>
    /// A resource wrapper that understands AUEV resources
    /// </summary>
    public class AUEVResource : AResource
    {
        const int recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        public override List<string> ContentFields { get { return GetContentFields(requestedApiVersion, this.GetType()); } }

        static bool checking = s4pi.Settings.Settings.Checking;

        #region Attributes
        uint version;
        int groupCount = 0;
        string[] content;
        #endregion

        public AUEVResource(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, EventArgs.Empty); } stream.Position = 0; Parse(stream); }

        #region Data I/O
        void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);

            uint magic = r.ReadUInt32();
            if (checking) if (magic != FOURCC("AUEV"))
                    throw new InvalidDataException(String.Format("Expected magic tag 0x{0:X8}; read 0x{1:X8}; position 0x{2:X8}",
                        FOURCC("AUEV"), magic, s.Position));

            version = r.ReadUInt32();

            groupCount = r.ReadInt32();
            content = new string[groupCount * 3];
            for(int i =  0; i < groupCount * 3; i++)
            {
                content[i] = System.Text.ASCIIEncoding.ASCII.GetString(r.ReadBytes(r.ReadInt32() - 1));
                r.ReadByte();
            }


        }

        protected override Stream UnParse()
        {
            MemoryStream ms = new MemoryStream();

            BinaryWriter w = new BinaryWriter(ms);

            w.Write((uint)FOURCC("AUEV"));

            w.Write(version);

            w.Write(groupCount);

            foreach(string str in content)
            {
                w.Write(str.Length);
                w.Write(System.Text.ASCIIEncoding.ASCII.GetBytes(str));
                w.Write((byte)0);
            }

            return ms;
        }
        #endregion



        #region Content Fields
        [ElementPriority(0)]
        public uint Version { get { return version; } set { if (version != value) { version = value; OnResourceChanged(this, EventArgs.Empty); } } }
        [ElementPriority(0)]
        public int GroupCount { get { return groupCount; } }
        [ElementPriority(0)]
        public string[] Content { get { return content; } set { if (value != content) { content = value; groupCount = content.Length / 3; OnResourceChanged(this, EventArgs.Empty); } } }
        public String Value
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Version: 0x{0:X8}\n", version);
                sb.AppendFormat("Group Count:0x{0:X8}\n", groupCount);
                sb.AppendLine("-".PadLeft(20, '-'));
                for (int i = 0; i < groupCount * 3; i ++)
                {
                    if (i % 3 == 0 && i != 0) sb.AppendLine();
                    sb.AppendFormat("[0x{0:X2}]: {1}\n", i, content[i]);
                }
                return sb.ToString();
            }
        }
        #endregion
    }

    /// <summary>
    /// ResourceHandler for AUEVResource wrapper
    /// </summary>
    public class AUEVResourceHandler : AResourceHandler
    {
        public AUEVResourceHandler()
        {
            this.Add(typeof(AUEVResource), new List<string>(new string[] { "0xBDD82221", }));
        }
    }
}
