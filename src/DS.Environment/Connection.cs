using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.Environment
{
	public abstract class Connection
	{
		/*
		 * These random numbers were chosen such that, starting at n=4 up to including
		 * n=20, the first n bits of all hashes of values from 1 to 1<<(n-1) are distinct.
		 * For instance, if you generate all hashes of the values 1 to 64 (n=6), and then
		 * take only the first 6 bits of each hash, you will get 64 unique values.
		 * In math speak, the hash function is bijective for n-bit subsets.
		 *
		 * TODO: I forgot how I generated these, they might be incorrect. Should not matter
		 * though.
		 */
		private const ulong A = 0x8D1C05F46BA0BB2FUL; // random odd number
		private const ulong B = 0x2571CB8851F40969UL; // random number

		public abstract ulong Coordinate { get; }

		public static ulong GetHash (ulong coordinate)
		{
			unchecked {
				var hash = coordinate;
				hash *= A;
				hash += B;
				return hash;
			}
		}

		/// <summary>
		/// Hash of this node's ID number. Bijective and unique.
		/// </summary>
		public ulong Hash {
			get {
				return GetHash (Coordinate);
			}
		}

		public override string ToString ()
		{
			const int nBitsShown = 5;

			var h = "";
			var hash = Hash >> (64 - nBitsShown);
			for (int i = nBitsShown - 1; i >= 0; i--) {
				var b = (hash >> i) & 1ul;
				h += b;
			}

			return "#" + Coordinate
				+ " ["
				+ h + "]";
		}

		public string ToStringShort ()
		{
			return Coordinate.ToString ();
		}
	}
}
