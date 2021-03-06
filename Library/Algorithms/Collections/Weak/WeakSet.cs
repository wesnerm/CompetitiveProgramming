﻿#region Using

/////////////////////////////////////////////////////////////////////////////
// This source code may not be reviewed, copied, or redistributed without
// the expressed permission of Wesner Moise.
// 
// Copyright (C) 2002-2010, Wesner Moise.
//////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Softperson.Mathematics;

#endregion

namespace Softperson.Collections
{
	/// <summary>
	///     Summary description for WeakSet.
	/// </summary>
	[DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof (CollectionDebugView<>))]
	public sealed class WeakSet<T> : ICollection<T>
		where T : class
	{
		#region Validation

		public void CheckConsistency()
		{
			Utility.Assert(loadFactor > 0 && loadFactor <= 1);
			Utility.Assert(Capacity > 0);
			Utility.Assert(Count <= Capacity);
			Utility.Assert(Capacity <= buckets.Length);
			Utility.Assert(bucketHandle.Target == buckets);

			var actualCount = 0;
			foreach (var b in buckets)
				if (b.key.IsAllocated && b.key.Target != buckets)
					actualCount++;

			Utility.Assert(actualCount == Count);
		}

		#endregion

		#region Variables

		private const int SampleSize = 20;
		private const float loadFactor = .72f;

		private GCHandle bucketHandle;
		private bucket[] buckets;
		private int version;

		private struct bucket
		{
			public int code; // Store hash code; sign bit means there was a collision.
			public GCHandle key;
		}

		#endregion

		#region Constructors

		public WeakSet() : this(0)
		{
		}

		public WeakSet(int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException("capacity");

			double rawsize = capacity/loadFactor;
			if (rawsize > int.MaxValue)
				throw new ArgumentException();

			var hashsize = NumberTheory.GetPrime((int) rawsize);
			buckets = new bucket[hashsize];
			bucketHandle = GCHandle.Alloc(buckets, GCHandleType.Normal);

			Capacity = (int) (loadFactor*hashsize);
			if (Capacity >= hashsize)
				Capacity = hashsize - 1;
		}

		public WeakSet(ICollection<T> c)
			: this(c != null ? c.Count : 0)
		{
			if (c != null)
				foreach (var o in c)
					Add(o);
		}

		public WeakSet<T> Clone()
		{
			var set = new WeakSet<T>(Count);
			set.Count = 0;
			set.AddRange(this);
			return set;
		}

		~WeakSet()
		{
			for (var i = 0; i < buckets.Length; i++)
				SetKey(ref buckets[i], null);
			bucketHandle.Free();
		}

		#endregion

		#region Operations

		public void Clear()
		{
			if (Count == 0)
				return;

			for (var i = 0; i < buckets.Length; i++)
			{
				buckets[i].code = 0;
				SetKey(ref buckets[i], null);
			}

			Count = 0;
		}

		public bool Contains(T key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			uint seed;
			uint incr;
			// Take a snapshot of buckets, in case another thread resizes table
			var lbuckets = buckets;
			var hashcode = InitHash(key, lbuckets.Length, out seed, out incr);
			var ntry = 0;

			int bucketNumber;
			do
			{
				bucketNumber = (int) (seed%(uint) lbuckets.Length);
				var bucketKey = GetKey(ref lbuckets[bucketNumber]);
				if (bucketKey == null)
					return false;
				if ((lbuckets[bucketNumber].code & 0x7FFFFFFF) == hashcode && key.Equals(bucketKey))
					return true;
				seed += incr;
			} while (lbuckets[bucketNumber].code < 0 && ++ntry < lbuckets.Length);
			return false;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			var lbuckets = buckets;
			for (var i = lbuckets.Length; --i >= 0;)
			{
				var keyv = GetKey(ref lbuckets[i]);
				if ((keyv != null) && (keyv != buckets))
					array[arrayIndex++] = (T) keyv;
			}
		}

		void ICollection<T>.Add(T key)
		{
			Add(key);
		}

		public bool Remove(T key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			uint seed;
			uint incr;
			var hashcode = InitHash(key, buckets.Length, out seed, out incr);
			var ntry = 0;

			bucket b;
			int bn; // bucketNumber
			do
			{
				bn = (int) (seed%(uint) buckets.Length);
				b = buckets[bn];
				var bucketKey = GetKey(ref buckets[bn]);
				if ((b.code & 0x7FFFFFFF) == hashcode && key.Equals(bucketKey))
				{
					buckets[bn].code &= unchecked((int) 0x80000000);
					if (buckets[bn].code != 0)
						SetKey(ref buckets[bn], buckets);
					else
						SetKey(ref buckets[bn], null);
					Count--;
					return true;
				}
				seed += incr;
			} while (buckets[bn].code < 0 && ++ntry < buckets.Length);
			return false;
		}

		private static uint InitHash(object key, int hashsize, out uint seed, out uint incr)
		{
			var hashcode = unchecked((uint) key.GetHashCode() & 0x7FFFFFFF);
			seed = hashcode;
			incr = 1 + ((seed >> 5) + 1)%((uint) hashsize - 1);
			return hashcode;
		}

		private void Expand()
		{
			// Allocate new Array 
			var oldhashsize = buckets.Length;

			var rawsize = 1 + oldhashsize*2;
			if (rawsize < 0)
				throw new ArgumentException();

			var hashsize = NumberTheory.GetPrime(rawsize);
			var newBuckets = new bucket[hashsize];

			// rehash table into new buckets
			for (var nb = 0; nb < oldhashsize; nb++)
			{
				var bucketKey = GetKey(ref buckets[nb]);
				if (bucketKey != null && bucketKey != buckets)
					PutEntry(newBuckets, buckets[nb].key, buckets[nb].code & 0x7FFFFFFF);
			}

			buckets = newBuckets;
			bucketHandle.Target = buckets;
			Capacity = (int) (loadFactor*hashsize);
			if (Capacity >= hashsize)
				Capacity = hashsize - 1;
		}

		private void Sample()
		{
			var n = Math.Min(SampleSize, Capacity);
			var originalCount = 0;
			while (n-- > 0)
			{
				GetKey(ref buckets[n]);
				if (Count < originalCount)
					return;
			}
		}

		private void RecalculateCounts()
		{
			var actualCount = 0;
			for (var i = 0; i < buckets.Length; i++)
			{
				var o = GetKey(ref buckets[i]);
				if (o != null && o != buckets)
					actualCount++;
			}

			Utility.Assert(actualCount == Count);
			Count = actualCount;
		}

		public void AddRange(ICollection<T> range)
		{
			foreach (var o in range)
				if (o != null)
					Add(o);
		}

		public void RemoveRange(ICollection<T> range)
		{
			foreach (var o in range)
				if (o != null)
					Remove(o);
		}

		public T Add(T key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			if (Count >= Capacity && !Contains(key))
			{
				Sample();
				// Expand only if we are creating a new item
				if (Count >= Capacity)
					Expand();
			}

			uint seed;
			uint incr;
			var hashcode = InitHash(key, buckets.Length, out seed, out incr);
			var ntry = 0;
			var emptySlotNumber = -1;

			do
			{
				var bucketNumber = (int) (seed%(uint) buckets.Length);
				var bucketKey = GetKey(ref buckets[bucketNumber]);
				if (emptySlotNumber == -1 && (bucketKey == buckets) && (buckets[bucketNumber].code < 0))
					emptySlotNumber = bucketNumber;

				if ((bucketKey == null) ||
					(bucketKey == buckets && ((buckets[bucketNumber].code & 0x80000000) == 0)))
				{
					if (emptySlotNumber != -1) // Reuse slot
						bucketNumber = emptySlotNumber;

					SetKey(ref buckets[bucketNumber], key);
					buckets[bucketNumber].code |= (int) hashcode;
					Count++;
					version++;
					return key;
				}
				if (((buckets[bucketNumber].code & 0x7FFFFFFF) == hashcode) &&
					key.Equals(bucketKey))
					return (T) bucketKey;
				if (emptySlotNumber == -1)
					buckets[bucketNumber].code |= unchecked((int) 0x80000000);
				seed += incr;
			} while (++ntry < buckets.Length);

			if (emptySlotNumber != -1)
			{
				SetKey(ref buckets[emptySlotNumber], key);
				buckets[emptySlotNumber].code |= (int) hashcode;
				Count++;
				version++;
				return key;
			}

			throw new InvalidOperationException();
		}

		private static void PutEntry(bucket[] newBuckets, GCHandle key, int hashcode)
		{
			Utility.Assert(hashcode >= 0, "hashcode >= 0");

			var seed = (uint) hashcode;
			var incr = 1 + ((seed >> 5) + 1)%((uint) newBuckets.Length - 1);

			do
			{
				var bucketNumber = (int) (seed%(uint) newBuckets.Length);
				if (!newBuckets[bucketNumber].key.IsAllocated)
				{
					newBuckets[bucketNumber].key = key;
					newBuckets[bucketNumber].code |= hashcode;
					return;
				}
				newBuckets[bucketNumber].code |= unchecked((int) 0x80000000);
				seed += incr;
			} while (true);
		}

		#endregion

		#region Properties

		public int Capacity { get; private set; }

		public int ActualCount
		{
			get
			{
				int oldCount;
				do
				{
					oldCount = GC.CollectionCount(0);
					RecalculateCounts();
				} while (GC.CollectionCount(0) != oldCount);
				return Count;
			}
		}

		public T this[T key]
		{
			get
			{
				if (key == null)
					throw new ArgumentNullException("key");

				uint seed;
				uint incr;
				// Take a snapshot of buckets, in case another thread does a resize
				var lbuckets = buckets;
				var hashcode = InitHash(key, lbuckets.Length, out seed, out incr);
				var ntry = 0;

				bucket b;
				do
				{
					var bucketNumber = (int) (seed%(uint) lbuckets.Length);
					b = lbuckets[bucketNumber];
					var bucketKey = GetKey(ref lbuckets[bucketNumber]);
					if (bucketKey == null)
						return null;
					if (((b.code & 0x7FFFFFFF) == hashcode) &&
						key.Equals(bucketKey))
						return (T) bucketKey;
					seed += incr;
				} while (b.code < 0 && ++ntry < lbuckets.Length);
				return null;
			}
		}

		public int Count { get; private set; }

		public bool IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region Enumeration

		public IEnumerator<T> GetEnumerator()
		{
			var v = version;
			for (var bucket = buckets.Length - 1; bucket >= 0; bucket--)
			{
				if (v != version) throw new InvalidOperationException();
				var keyv = GetKey(ref buckets[bucket]);
				if ((keyv != null) && (keyv != buckets))
					yield return (T) keyv;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Bucket Operations

		private object GetKey(ref bucket b)
		{
			if (!b.key.IsAllocated)
				return null;

			var obj = b.key.Target;
			if (obj != null)
				return obj;

			b.code &= unchecked((int) 0x80000000);
			Count--;
			Utility.Assert(Count >= 0);

			if (b.code != 0)
			{
				b.key.Target = buckets;
				return buckets;
			}

			b.key.Free();
			b.key = new GCHandle();
			return null;
		}

		private static void SetKey(ref bucket b, object value)
		{
			if (value == null)
			{
				if (b.key.IsAllocated)
					b.key.Free();
				return;
			}

			if (b.key.IsAllocated)
			{
				b.key.Target = value;
				return;
			}

			b.key = GCHandle.Alloc(value, GCHandleType.Weak);
		}

		#endregion
	}
}