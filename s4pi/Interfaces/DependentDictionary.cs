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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace s4pi.Interfaces
{
    /// <summary>
    ///     Abstract extension to <see cref="AHandlerDictionary{TKey, TValue}" /> adding support for <see cref="System.IO.Stream" /> IO
    ///     and partially implementing <see cref="IGenericAdd" />.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <seealso cref="AHandlerDictionary{TKey, TValue}" />
    /// <seealso cref="IGenericAdd" />
    [Serializable]
    [ComVisible(false)]
    [DebuggerDisplay("Count = {Count}")]
    public abstract class DependentDictionary<TKey, TValue> : AHandlerDictionary<TKey, TValue>,
        IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback
        where TValue : AHandlerElement, IEquatable<TValue>
    {
        /// <summary>
        ///     Holds the <see cref="EventHandler" /> delegate to invoke if an element in the <see cref="DependentDictionary{TKey, TValue}" />
        ///     changes.
        /// </summary>
        /// <remarks>Work around for dictionary event handler triggering during stream constructor and other places.</remarks>
        protected EventHandler elementHandler;

		#region Constructors

		/// <summary>
        ///     Initializes a new instance of the <see cref="DependentDictionary{TKey, TValue}" /> class
		///     that is empty.
		/// </summary>
		/// <param name="handler">The <see cref="EventHandler" /> to call on changes to the list or its elements.</param>
		protected DependentDictionary(EventHandler handler) : base(handler) { }

		/// <summary>
        ///     Initializes a new instance of the <see cref="DependentDictionary{TKey, TValue}" /> class
        ///     filled with the content of <paramref name="dictionary" />.
		/// </summary>
		/// <param name="handler">The <see cref="EventHandler" /> to call on changes to the list or its elements.</param>
        /// <param name="dictionary">The initial content of the dictionary.</param>
		/// <remarks>
        ///     Calls <c>this.Add(...)</c> to ensure a fresh instance is created, rather than passing <paramref name="dictionary" />
		///     to the base constructor.
		/// </remarks>
        protected DependentDictionary(EventHandler handler, IDictionary<TKey, TValue> dictionary)
			: base(null)
		{
			this.elementHandler = handler;
            foreach (KeyValuePair<TKey, TValue> kv in dictionary)
            {
                this.Add(kv.Key, (TValue)kv.Value.Clone(null));
            }
			this.handler = handler;
		}

		// Add stream-based constructors and support
		/// <summary>
		///     Initializes a new instance of the <see cref="DependentList{T}" /> class
		///     filled from <see cref="System.IO.Stream" /> <paramref name="s" />.
		/// </summary>
		/// <param name="handler">The <see cref="EventHandler" /> to call on changes to the list or its elements.</param>
		/// <param name="s">The <see cref="System.IO.Stream" /> to read for the initial content of the list.</param>
		/// <exception cref="System.InvalidOperationException">Thrown when list size exceeded.</exception>
        protected DependentDictionary(EventHandler handler, Stream s)
			: base(null)
		{
			this.elementHandler = handler;
			this.Parse(s);
			this.handler = handler;
		}

		#endregion

        #region Data I/O

        /// <summary>
        ///     Read list entries from a stream
        /// </summary>
        /// <param name="s">Stream containing list entries</param>
        /// <remarks>This method bypasses <see cref="DependentDictionary{TKey, TValue}.Add(TKey, TValue)"/> because
        /// <see cref="CreateKey(Stream, out bool)"/> and <see cref="CreateValue(Stream, out bool)"/>
        /// must take care of the same issues.</remarks>
        protected virtual void Parse(Stream s)
        {
            this.Clear();
            var incKey = true;
            var incValue = true;
            int count = this.ReadCount(s);
            for (var i = count; i > 0; i = i - ((incKey && incValue) ? 1 : 0))
            {
                base.Add(this.CreateKey(s, out incKey), this.CreateValue(s, out incValue));
            }
        }

        /// <summary>
        ///     Return the number of elements to be created.
        /// </summary>
        /// <param name="s"><see cref="System.IO.Stream" /> being processed.</param>
        /// <returns>The number of elements to be created.</returns>
        protected virtual int ReadCount(Stream s)
        {
            return (new BinaryReader(s)).ReadInt32();
        }

        /// <summary>
        ///     Create a new key from the <see cref="System.IO.Stream" />.
        /// </summary>
        /// <param name="s">Stream containing element data.</param>
        /// <returns>A new dictionary key.</returns>
        protected abstract TKey CreateKey(Stream s);

        /// <summary>
        ///     Create a new key from the <see cref="System.IO.Stream" /> and indicate whether the key-value pair counts towards the number of
        ///     elements to be created.
        /// </summary>
        /// <param name="s"><see cref="System.IO.Stream" /> containing element data.</param>
        /// <param name="inc"><c>true</c> if this key-value pair will count towards the number of dictionary elements to be created.</param>
        /// <returns>A new dictionary key.</returns>
        protected virtual TKey CreateKey(Stream s, out bool inc)
        {
            inc = true;
            return this.CreateKey(s);
        }

        /// <summary>
        ///     Create a new value from the <see cref="System.IO.Stream" />.
        /// </summary>
        /// <param name="s">Stream containing element data.</param>
        /// <returns>A new dictionary value.</returns>
        protected abstract TValue CreateValue(Stream s);

        /// <summary>
        ///     Create a new value from the <see cref="System.IO.Stream" /> and indicates whether the key-value pair counts towards the number of
        ///     elements to be created.
        /// </summary>
        /// <param name="s"><see cref="System.IO.Stream" /> containing element data.</param>
        /// <param name="inc"><c>true</c> if this key-value pair will count towards the number of dictionary elements to be created.</param>
        /// <returns>A new dictionary value.</returns>
        protected virtual TValue CreateValue(Stream s, out bool inc)
        {
            inc = true;
            return this.CreateValue(s);
        }

        /// <summary>
        ///     Write list entries to a stream
        /// </summary>
        /// <param name="s">Stream to receive list entries</param>
        public virtual void UnParse(Stream s)
        {
            this.WriteCount(s, this.Count);
            foreach (var kv in this)
            {
                this.WriteKey(s, kv.Key);
                this.WriteValue(s, kv.Value);
            }
        }

        /// <summary>
        ///     Write the count of dictionary elements to the stream.
        /// </summary>
        /// <param name="s"><see cref="System.IO.Stream" /> to write <paramref name="count" /> to.</param>
        /// <param name="count">Value to write to <see cref="System.IO.Stream" /> <paramref name="s" />.</param>
        protected virtual void WriteCount(Stream s, int count)
        {
            (new BinaryWriter(s)).Write(count);
        }

        /// <summary>
        ///     Write a key to the stream.
        /// </summary>
        /// <param name="s"><see cref="System.IO.Stream" /> to write <paramref name="key" /> to.</param>
        /// <param name="key">The key to write to <see cref="System.IO.Stream" /> <paramref name="s" />.</param>
        protected abstract void WriteKey(Stream s, TKey key);

        /// <summary>
        ///     Write a value to the stream.
        /// </summary>
        /// <param name="s"><see cref="System.IO.Stream" /> to write <paramref name="value" /> to.</param>
        /// <param name="value">The value to write to <see cref="System.IO.Stream" /> <paramref name="s" />.</param>
        protected abstract void WriteValue(Stream s, TValue value);

        #endregion

        /// <summary>
        /// Gets or sets the value associated with the specified <paramref name="key"/>, setting the element change handler..
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>
        /// The value associated with the specified <paramref name="key"/>.
        /// If the specified <paramref name="key"/> is not found, a get operation throws a <see cref="System.Collections.Generic.KeyNotFoundException"/>,
        /// and a set operation creates a new element with the specified <paramref name="key"/>.
        /// Any change to the list invokes the supplied dictionary changed event handler.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> does not exist in the collection.</exception>
        public override TValue this[TKey key] { get { return base[key]; } set { if (!base[key].Equals(value)) { base[key] = value; value.SetHandler(this.elementHandler); } } }

        /// <summary>
        ///     Add a default element to a <see cref="DependentDictionary{TKey,TValue}" /> of the specified type, <paramref name="valueType" />.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueType">A concrete type assignable to the list generic type, <typeparamref name="TValue" />.</param>
        /// <exception cref="ArgumentException"><paramref name="valueType" /> is abstract.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="valueType" /> is not an implementation of the dictionary value type, <typeparamref name="TValue" />.
        /// </exception>
        /// <seealso cref="Activator.CreateInstance(Type, object[])" />
        public virtual void Add(TKey key, Type valueType)
        {
            if (valueType.IsAbstract)
            {
                throw new ArgumentException("Value type must be concrete.", "valueType");
            }

            if (!typeof(TValue).IsAssignableFrom(valueType))
            {
                throw new ArgumentException("Cannot assign from valueType to dictionary generic value type.", "valueType");
            }

            var newElement = Activator.CreateInstance(valueType, 0, this.elementHandler) as TValue;
            base.Add(key, newElement);
        }

        /// <summary>
        ///     Add a default element to a <see cref="DependentDictionary{TKey, TValue}" />.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <exception cref="NotImplementedException"><c>{TValue}</c> is abstract.</exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="System.ArgumentException">An element with the same key already exists in the <see cref="DependentDictionary{TKey,TValue}"/>.</exception>
        public virtual void Add(TKey key)
        {
            if (typeof(TValue).IsAbstract)
            {
                throw new NotImplementedException("Value type must be concrete.");
            }

            this.Add(key, typeof(TValue));
        }

        /// <summary>
        ///     Adds an entry to a <see cref="DependentDictionary{TKey, TValue}" />, setting the element change handler.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">An instance of type <c>{TValue}</c> to add to the list.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="System.ArgumentException">An element with the same key already exists in the <see cref="DependentDictionary{TKey,TValue}"/>.</exception>
        public override void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            value.SetHandler(this.elementHandler);
        }

        /// <summary>
        ///     Adds a collection of <see cref="KeyValuePair{TKey, TValue}" /> items, setting the element change handler for each.
        ///     The dictionary change handler will only be called once for the collection.
        /// </summary>
        /// <param name="collection">A collection of <see cref="KeyValuePair{TKey, TValue}" /> items to add.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="collection"/> is null or the key of one of the items in the collection is null.</exception>
        /// <exception cref="System.ArgumentException">An element with the same key already exists in the <see cref="AHandlerDictionary{TKey,TValue}"/>.</exception>
        public virtual void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            EventHandler h = handler;
            handler = null;
            try
            {
                foreach (var kv in collection)
                {
                    this.Add(kv.Key, kv.Value);
                }
            }
            finally
            {
                handler = h;
            }
        }
    
    }

    /// <summary>
    /// A flexible generic list that implements <see cref="DependentDictionary{TKey, TValue}"/> for
    /// a simple value data type (such as <see cref="UInt32"/>).
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">A simple data type (such as <see cref="UInt32"/>).</typeparam>
    /// <seealso cref="HandlerElement{T}"/>
    public abstract class SimpleDictionary<TKey, TValue> : DependentDictionary<TKey, HandlerElement<TValue>>, IDictionary<TKey, HandlerElement<TValue>>
        where TValue : struct, IComparable, IConvertible, IEquatable<TValue>, IComparable<TValue>
    {
        #region Attributes
        CreateValueMethod createValue;
        WriteValueMethod writeValue;
        ReadCountMethod readCount;
        WriteCountMethod writeCount;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionary{TKey, TValue}"/> class
        /// that is empty.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the dictionary or its elements.</param>
        /// <param name="createValue">Optional; the method to create a new element in the dictionary from a stream.  If null, return default{TValue}.</param>
        /// <param name="writeValue">Optional; the method to create a new element in the dictionary from a stream.  No operation if null.</param>
        /// <param name="readCount">Optional; default is to read a <see cref="Int32"/> from the <see cref="Stream"/>.</param>
        /// <param name="writeCount">Optional; default is to write a <see cref="Int32"/> to the <see cref="Stream"/>.</param>
        public SimpleDictionary(EventHandler handler, CreateValueMethod createValue = null, WriteValueMethod writeValue = null, ReadCountMethod readCount = null, WriteCountMethod writeCount = null) : base(handler) { this.createValue = createValue; this.writeValue = writeValue; this.readCount = readCount; this.writeCount = writeCount; }
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionary{TKey, TValue}"/> class
        /// from <paramref name="s"/>.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the dictionary or its elements.</param>
        /// <param name="s">The <see cref="Stream"/> to read for the initial content of the dictionary.</param>
        /// <param name="createValue">Required; the method to create a new element in the dictionary from a stream.</param>
        /// <param name="writeValue">Required; the method to create a new element in the dictionary from a stream.</param>
        /// <param name="readCount">Optional; default is to read a <see cref="Int32"/> from the <see cref="Stream"/>.</param>
        /// <param name="writeCount">Optional; default is to write a <see cref="Int32"/> to the <see cref="Stream"/>.</param>
        public SimpleDictionary(EventHandler handler, Stream s, CreateValueMethod createValue, WriteValueMethod writeValue, ReadCountMethod readCount = null, WriteCountMethod writeCount = null) : this(null, createValue, writeValue, readCount, writeCount) { elementHandler = handler; Parse(s); this.handler = handler; }
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionary{TKey, TValue}"/> class
        /// from <paramref name="dictionary"/>, wrapping each value in a <see cref="HandlerElement{TValue}"/> instance.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the dictionary or its elements.</param>
        /// <param name="dictionary">The source to use as the initial content of the dictionary.</param>
        /// <param name="createElement">Optional; the method to create a new element in the dictionary from a stream.  If null, return default{TValue}.</param>
        /// <param name="writeElement">Optional; the method to create a new element in the dictionary from a stream.  No operation if null.</param>
        /// <param name="readCount">Optional; default is to read a <see cref="Int32"/> from the <see cref="Stream"/>.</param>
        /// <param name="writeCount">Optional; default is to write a <see cref="Int32"/> to the <see cref="Stream"/>.</param>
        public SimpleDictionary(EventHandler handler, IDictionary<TKey, TValue> dictionary, CreateValueMethod createElement = null, WriteValueMethod writeElement = null, ReadCountMethod readCount = null, WriteCountMethod writeCount = null) : this(null, createElement, writeElement, readCount, writeCount) { elementHandler = handler; this.AddRange(dictionary); this.handler = handler; }
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDictionary{TKey, TValue}"/> class from the existing <paramref name="dictionary"/>.
        /// </summary>
        /// <param name="handler">The <see cref="EventHandler"/> to call on changes to the dictionary or its elements.</param>
        /// <param name="dictionary">The source to use as the initial content of the dictionary.</param>
        /// <param name="createElement">Optional; the method to create a new element in the dictionary from a stream.  If null, return default{TValue}.</param>
        /// <param name="writeElement">Optional; the method to create a new element in the dictionary from a stream.  No operation if null.</param>
        /// <param name="readCount">Optional; default is to read a <see cref="Int32"/> from the <see cref="Stream"/>.</param>
        /// <param name="writeCount">Optional; default is to write a <see cref="Int32"/> to the <see cref="Stream"/>.</param>
        public SimpleDictionary(EventHandler handler, IDictionary<TKey, HandlerElement<TValue>> dictionary, CreateValueMethod createElement = null, WriteValueMethod writeElement = null, ReadCountMethod readCount = null, WriteCountMethod writeCount = null) : this(null, createElement, writeElement, readCount, writeCount) { elementHandler = handler; base.AddRange(dictionary); this.handler = handler; }
        #endregion

        #region Data I/O
        /// <summary>
        /// Return the number of elements to be created.
        /// </summary>
        /// <param name="s"><see cref="System.IO.Stream"/> being processed.</param>
        /// <returns>The number of elements to be created.</returns>
        protected override int ReadCount(Stream s) { return readCount == null ? base.ReadCount(s) : readCount(s); }
        /// <summary>
        /// Write the count of list elements to the stream.
        /// </summary>
        /// <param name="s"><see cref="System.IO.Stream"/> to write <paramref name="count"/> to.</param>
        /// <param name="count">Value to write to <see cref="System.IO.Stream"/> <paramref name="s"/>.</param>
        protected override void WriteCount(Stream s, int count) { if (writeCount == null) base.WriteCount(s, count); else writeCount(s, count); }

        /// <summary>
        /// Creates an new list element of type <typeparamref name="TValue"/> by reading <paramref name="s"/>.
        /// </summary>
        /// <param name="s"><see cref="Stream"/> containing data.</param>
        /// <returns>New list element.</returns>
        protected override HandlerElement<TValue> CreateValue(Stream s) { return new HandlerElement<TValue>(0, elementHandler, createValue == null ? default(TValue) : createValue(s)); }
        /// <summary>
        /// Writes the value of a list element to <paramref name="s"/>.
        /// </summary>
        /// <param name="s"><see cref="Stream"/> containing data.</param>
        /// <param name="element">List element for which to write the value to the <see cref="Stream"/>.</param>
        protected override void WriteValue(Stream s, HandlerElement<TValue> element) { if (writeValue != null) writeValue(s, element.Val); }
        #endregion

        #region Sub-classes
        /// <summary>
        /// Create a new element of type <typeparamref name="TValue"/> from a <see cref="Stream"/>
        /// to be used as the value of a new dictionary item.
        /// </summary>
        /// <param name="s">The <see cref="Stream"/> from which to read the element data.</param>
        /// <returns>A new element of type <typeparamref name="TValue"/>.</returns>
        public delegate TValue CreateValueMethod(Stream s);
        /// <summary>
        /// Write the value (of type <typeparamref name="TValue"/>) of a dictionary item to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="s">The <see cref="Stream"/> to which to write the value.</param>
        /// <param name="value">The value of type <typeparamref name="TValue"/> to write.</param>
        public delegate void WriteValueMethod(Stream s, TValue value);
        /// <summary>
        /// Return the number of dictionary elements to read.
        /// </summary>
        /// <param name="s">A <see cref="Stream"/> that may contain the number of elements.</param>
        /// <returns>The number of dictionary elements to read.</returns>
        public delegate int ReadCountMethod(Stream s);
        /// <summary>
        /// Store the number of elements in the dictionary.
        /// </summary>
        /// <param name="s">A <see cref="Stream"/> to which list elements will be written after the count.</param>
        /// <param name="count">The number of dictionary elements.</param>
        public delegate void WriteCountMethod(Stream s, int count);
        #endregion

        /// <summary>
        /// Gets or sets the value associated with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>
        /// The value associated with the specified <paramref name="key"/>.
        /// If the specified <paramref name="key"/> is not found, a get operation throws a <see cref="System.Collections.Generic.KeyNotFoundException"/>,
        /// and a set operation creates a new element with the specified <paramref name="key"/>.
        /// Any change to the list invokes the supplied dictionary changed event handler.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key"/> does not exist in the collection.</exception>
        public new TValue this[TKey key] { get { return base[key].Val; } set { base[key] = new HandlerElement<TValue>(0, elementHandler, value); } }

        /// <summary>
        ///     Add a default element to a <see cref="SimpleDictionary{TKey, TValue}" />.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <exception cref="NotImplementedException"><typeparamref name="TKey" /> is abstract.</exception>
        /// <exception cref="InvalidOperationException">Thrown when list size exceeded.</exception>
        /// <exception cref="NotSupportedException">The <see cref="DependentList{T}" /> is read-only.</exception>
        public override void Add(TKey key)
        {
            this.Add(key, default(TValue));
        }
        /// <summary>
        /// Adds an entry to a <see cref="SimpleList{T}"/>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="item">The object to add.</param>
        /// <returns>True on success</returns>
        /// <exception cref="InvalidOperationException">Thrown when list size exceeded.</exception>
        /// <exception cref="NotSupportedException">The <see cref="SimpleList{T}"/> is read-only.</exception>
        public virtual void Add(TKey key, TValue item) { base.Add(key, new HandlerElement<TValue>(0, elementHandler, item)); }

        /// <summary>
        ///     Adds a collection of <see cref="KeyValuePair{TKey, TValue}" /> items, setting the element change handler for each.
        ///     The dictionary change handler will only be called once for the collection.
        /// </summary>
        /// <param name="collection">A collection of <see cref="KeyValuePair{TKey, TValue}" /> items to add.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="collection"/> is null or one of the items in the collection is null.</exception>
        /// <exception cref="System.ArgumentException">An element with the same key already exists in the <see cref="AHandlerDictionary{TKey,TValue}"/>.</exception>
        public virtual void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            EventHandler h = handler;
            handler = null;
            try
            {
                foreach (var kv in collection)
                {
                    this.Add(kv.Key, kv.Value);
                }
            }
            finally
            {
                handler = h;
            }
        }

    }
}
