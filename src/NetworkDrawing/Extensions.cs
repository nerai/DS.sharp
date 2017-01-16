using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkDrawing
{
	public static class Extensions
	{
		public static uint BSF (this ulong u)
		{
			if (u == 0) {
				throw new ArgumentException ("u");
			}

			uint n = 1; // TODO: should be 0, no?
			while ((u & 1ul) == 0) {
				u >>= 1;
				n++;
			}
			return n;
		}

		private static readonly uint[] BSR_Lookup = {
			uint.MaxValue, // is 0
			3, // 1 set
			2, 2, // 2 set
			1, 1, 1, 1, // 4 set
			0, 0, 0, 0, 0, 0, 0, 0 // 8 set
			};

		/// <summary>
		/// Return zero based index of first bit from the left, i.e. most significant bit
		/// </summary>
		public static uint BSR (this ulong u, bool throwIf0 = true)
		{
			if (u == 0) {
				if (throwIf0) {
					throw new ArgumentOutOfRangeException ("v");
				}
				else {
					return uint.MaxValue;
				}
			}

			uint b = 0;

			if ((u & 0xFFFFFFFF00000000ul) != 0) {
				u >>= 32;
			}
			else {
				b += 32;
			}
			if ((u & 0xFFFF0000ul) != 0) {
				u >>= 16;
			}
			else {
				b += 16;
			}
			if ((u & 0xFF00ul) != 0) {
				u >>= 8;
			}
			else {
				b += 8;
			}
			if ((u & 0xF0ul) != 0) {
				u >>= 4;
			}
			else {
				b += 4;
			}

			Debug.Assert (u != 0);
			b += BSR_Lookup[u];
			return b;
		}

		public static IEnumerable<uint> BsfStream (this ulong u)
		{
			uint n = 1; // TODO: should be 0, no?
			while (u != 0) {
				if ((u & 1ul) == 1) {
					yield return n;
				}
				u >>= 1;
				n++;
			}
		}

		public static string FormatBits (uint dim, ulong u)
		{
			var sb = new StringBuilder ();
			for (uint i = dim; i > 0; i--) {
				var bit = u & (1ul << (int) i - 1);
				sb.Append (bit != 0 ? "1" : "0");
			}
			return sb.ToString ();
		}

		public static V TryGet<T, V> (this Dictionary<T, V> dict, T key)
		{
			if (key == null) {
				return default (V);
			}

			V v;
			if (!dict.TryGetValue (key, out v)) {
				v = default (V);
			}
			return v;
		}

		public static int RemoveAll<T, V> (this Dictionary<T, V> dict, Func<KeyValuePair<T, V>, bool> f)
		{
			int n = 0;
			foreach (var pair in dict.ToList ()) {
				if (f (pair)) {
					dict.Remove (pair.Key);
					n++;
				}
			}
			return n;
		}

		public static T MinBy<T> (this IEnumerable<T> t, Func<T, double> f)
		{
			var v = double.MaxValue;
			var e = default (T);
			foreach (var item in t) {
				var c = f (item);
				if (c <= v) {
					v = c;
					e = item;
				}
			}
			return e;
		}

		public static T MinBy<T> (this IEnumerable<T> t, Func<T, long> f)
		{
			var v = long.MaxValue;
			var e = default (T);
			foreach (var item in t) {
				var c = f (item);
				if (c <= v) {
					v = c;
					e = item;
				}
			}
			return e;
		}

		public static T MaxBy<T> (this IEnumerable<T> t, Func<T, double> f)
		{
			var v = double.MinValue;
			var e = default (T);
			foreach (var item in t) {
				var c = f (item);
				if (c >= v) {
					v = c;
					e = item;
				}
			}
			return e;
		}

		public static T MaxBy<T> (this IEnumerable<T> t, Func<T, long> f)
		{
			var v = long.MinValue;
			var e = default (T);
			foreach (var item in t) {
				var c = f (item);
				if (c >= v) {
					v = c;
					e = item;
				}
			}
			return e;
		}

		public static List<T> RemoveAndReturn<T> (this List<T> list, Predicate<T> match)
		{
			var removed = list.Where (x => match (x)).ToList ();
			list.RemoveAll (match);
			return removed;
		}

		public static List<T> RemoveAndReturn<T> (this ICollection<T> t, Predicate<T> match)
		{
			var removed = new List<T> ();
			foreach (var item in t.ToList ()) {
				if (match (item)) {
					removed.Add (item);
					t.Remove (item);
				}
			}
			return removed;
		}
	}
}
