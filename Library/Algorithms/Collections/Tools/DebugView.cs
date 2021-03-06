﻿#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace Softperson.Collections
{
	public struct Collection<T> : IEnumerable<T>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly IEnumerable<T> collection;

		public Collection(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new NotSupportedException("Collection");
			this.collection = collection;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] Items
		{
			get
			{
				if (collection == null)
					return null;
				return collection.ToArray();
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return collection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}


	public class CollectionDebugView<T>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly IEnumerable collection;

		public CollectionDebugView(IEnumerable collection)
		{
			if (collection == null)
				throw new NotSupportedException("Collection Debug View");
			this.collection = collection;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public object[] Items
		{
			get
			{
				if (collection == null)
					return null;
				return collection.Cast<object>().ToArray();
			}
		}
	}

	public class SortedCollectionDebugView<T>
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly IEnumerable collection;

		public SortedCollectionDebugView(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new NotSupportedException("Collection Debug View");
			this.collection = collection;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public object[] Items
		{
			get
			{
				var items = collection.Cast<object>().ToArray();
				Array.Sort(items);
				return items;
			}
		}
	}

	public class DictionaryDebugView<K, V>
	{
		private readonly ICollection<KeyValuePair<K, V>> dict;

		public DictionaryDebugView(ICollection<KeyValuePair<K, V>> dictionary)
		{
			if (dictionary == null)
				throw new NotSupportedException("Dictionary Debug View");
			dict = dictionary;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public virtual KeyValuePair<object, object>[] Items
		{
			get
			{
				var list = new KeyValuePair<object, object>[dict.Count];
				var i = 0;
				foreach (var pair in dict)
				{
					if (i >= list.Length) break;
					list[i++] = new KeyValuePair<object, object>(pair.Key, pair.Value);
				}
				return list;
			}
		}
	}

	public class SortedDictionaryDebugView<K, V>
	{
		private readonly ICollection<KeyValuePair<K, V>> dict;

		public SortedDictionaryDebugView(ICollection<KeyValuePair<K, V>> dictionary)
		{
			if (dictionary == null)
				throw new NotSupportedException("Dictionary Debug View");
			dict = dictionary;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public virtual KeyValue<K, V>[] Items
		{
			get
			{
				var list = new KeyValue<K, V>[dict.Count];
				var i = 0;
				foreach (var pair in dict)
				{
					if (i >= list.Length)
						break;
					list[i++] = new KeyValue<K, V>(pair.Key, pair.Value);
				}
				var items = list;
				var comparer = Comparer<K>.Default;
				Array.Sort(items,
					(item1, item2) => comparer.Compare((K) item1.Key, (K) item2.Key));
				return items;
			}
		}
	}

	[DebuggerDisplay("{value}", Name = "[{key,nq}]", Type = "")]
	public class KeyValue<K, V>
	{
		// Fields
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly K key;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] private readonly V value;

		// Methods
		public KeyValue(K key, V value)
		{
			this.value = value;
			this.key = key;
		}

		// Properties
		public object Key
		{
			get { return key; }
		}

		public object Value
		{
			get { return value; }
		}
	}
}