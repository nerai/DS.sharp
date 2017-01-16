using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.Environment
{
	class Program
	{
		static void Main (string[] args)
		{
			FindGoodRandomParameters ();
		}

		private static void FindGoodRandomParameters ()
		{
			Console.WriteLine ("Searching parameters for hashing (ax+b) mod 64 with bijective n-bit subsets of the first 2^(n-1) values");

			var r = new Random ();
			var buf = new byte[sizeof (ulong)];

			Func<ulong> gen = () => {
				r.NextBytes (buf);
				var u = BitConverter.ToUInt64 (buf, 0);
				u |= 1; // odd numbers are sometimes required, so let's just always use them
				return u;
			};

			var bestA = 0ul;
			var bestB = 0ul;
			var bestN = 0;

			for (;;) {
				var a = gen ();
				var b = gen ();

				Func<ulong, ulong> hash = (u) => {
					unchecked {
						u *= a;
						u += b;
						return u;
					}
				};

				const int startbits = 1;
				for (int nbits = startbits; ; nbits++) {
					var check = new HashSet<ulong> ();
					var max = 1ul << nbits;
					for (ulong test = 1; test <= max; test++) {
						var u = hash (test);
						u >>= (64 - nbits);
						if (!check.Add (u)) {
							break;
						}
					}

					if ((ulong) check.Count != max) {
						// this one failed, previous one worked
						nbits--;
						max /= 2;

						// new best?
						if (nbits > bestN) {
							bestN = nbits;
							bestA = a;
							bestB = b;

							// print
							Console.WriteLine ($"{bestN}: A={a:X16}, B={b:X16}");
							for (ulong test = 1; test <= Math.Min (max, 32); test++) {
								var u = hash (test);
								u >>= (64 - nbits);
								Console.WriteLine (Convert.ToString ((long) u, 2).PadLeft (nbits, '0'));
							}
						}
						break;
					}
				}
			}
		}
	}
}
