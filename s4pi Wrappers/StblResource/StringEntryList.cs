/***************************************************************************
 *  Copyright (C) 2009, 2016 by the Sims 4 Tools development team          *
 *                                                                         *
 *  Contributors:                                                          *
 *  Cmar                                                                   *
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

namespace StblResource
{
    using System;
    using System.IO;
    using s4pi.Interfaces;

    public class StringEntryList : DependentList<StringEntry>
    {
        private readonly ulong numberEntries;

        public StringEntryList(EventHandler handler) : base(handler)
        {
        }

        public StringEntryList(EventHandler handler, Stream s, ulong numEntries) : base(handler)
        {
            this.numberEntries = numEntries;
            this.Parse(s);
        }

        #region Data I/O

        protected override void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            for (ulong i = 0; i < this.numberEntries; i++)
            {
                this.Add(new StringEntry(1, this.handler, s));
            }
        }

        public override void UnParse(Stream s)
        {
            foreach (StringEntry entry in this)
            {
                entry.UnParse(s);
            }
        }

        #endregion

        protected override StringEntry CreateElement(Stream s)
        {
            return new StringEntry(1, this.handler, s);
        }

        protected override void WriteElement(Stream s, StringEntry element)
        {
            element.UnParse(s);
        }
    }
}