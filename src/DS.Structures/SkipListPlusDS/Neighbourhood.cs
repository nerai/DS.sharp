using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.Environment;

namespace DS.Structures.SkipListPlusDS
{
	internal class Neighbourhood
	{
		public readonly uint Level;
		private readonly object _Lock = new object ();
		private readonly SkipNode _Me;
		private readonly Connection[] _Pred = new Connection[2];
		private readonly Connection[] _Succ = new Connection[2];
		private readonly HashSet<Connection> _Left = new HashSet<Connection> ();
		private readonly HashSet<Connection> _Right = new HashSet<Connection> ();

		public Neighbourhood (SkipNode me, uint level)
		{
			_Me = me;
			Level = level;
		}

		public IEnumerable<Connection> AllNodes {
			get {
				lock (_Lock) {
					return new[] { _Me }
						.Concat (_Left)
						.Concat (_Right)
						.ToArray ();
				}
			}
		}

		public ulong RangeMin // CAREFUL: can be 0, but never negative because unsigned
		{
			get {
				lock (_Lock) {
					return Math.Min (
					_Pred[0] == null ? ulong.MinValue : _Pred[0].Coordinate,
					_Pred[1] == null ? ulong.MinValue : _Pred[1].Coordinate);
				}
			}
		}

		public ulong RangeMax {
			get {
				lock (_Lock) {
					return Math.Max (
					_Succ[0] == null ? ulong.MaxValue : _Succ[0].Coordinate,
					_Succ[1] == null ? ulong.MaxValue : _Succ[1].Coordinate);
				}
			}
		}

		public List<Connection> Linearize (Connection v)
		{
			var removed = new List<Connection> ();
			var mC = _Me.Coordinate;
			var vC = v.Coordinate;
			var mH = _Me.Hash;
			var vH = v.Hash;

			if (mC == vC) {
				return removed;
			}

			Debug.Assert ((vH ^ mH).BSR () >= Level);
			var nextBit = (vH >> (63 - (int) Level)) & 1;

			if (vC > mC) {
				var max = RangeMax;
				if (v.Coordinate >= max) {
					removed.Add (v);
					return removed;
				}

				lock (_Lock) {
					_Right.Add (v);

					var cur = _Succ[nextBit];
					if (cur == null || cur.Coordinate > v.Coordinate) {
						_Succ[nextBit] = v;
						max = RangeMax;
						removed.AddRange (_Right.RemoveAndReturn (n => n.Coordinate > max));
						return removed;
					}
				}
			}
			else {
				var min = RangeMin;
				if (v.Coordinate <= min) {
					removed.Add (v);
					return removed;
				}

				lock (_Lock) {
					_Left.Add (v);

					var cur = _Pred[nextBit];
					if (cur == null || cur.Coordinate < v.Coordinate) {
						_Pred[nextBit] = v;
						min = RangeMin;
						removed.AddRange (_Left.RemoveAndReturn (n => n.Coordinate < min));
						return removed;
					}
				}
			}

			return removed;
		}

		public IEnumerable<Tuple<Connection, Connection>> Rule1a ()
		{
			Connection[] left;
			Connection[] right;
			lock (_Lock) {
				left = _Left
					.OrderBy (n => n.Coordinate)
					.Concat (new[] { _Me })
					.ToArray ();
				right = _Right
					.OrderByDescending (n => n.Coordinate)
					.Concat (new[] { _Me })
					.ToArray ();
			}

			// todo nur senden wenn sinnvoll, siehe skript

			for (int i = 0; i < left.Length - 1; i++) {
				var to = left[i];
				var about = left[i + 1];
				yield return new Tuple<Connection, Connection> (to, about);
			}
			for (int i = 0; i < right.Length - 1; i++) {
				var to = right[i];
				var about = right[i + 1];
				yield return new Tuple<Connection, Connection> (to, about);
			}
		}

		public IEnumerable<Tuple<Connection, Connection>> Rule1b ()
		{
			lock (_Lock) {
				var right0 = _Right.MinBy (n => n.Coordinate);
				if (right0 != null) {
					foreach (var to in _Left) {
						yield return new Tuple<Connection, Connection> (to, right0);
					}
				}

				var left0 = _Left.MaxBy (n => n.Coordinate);
				if (left0 != null) {
					foreach (var to in _Right) {
						yield return new Tuple<Connection, Connection> (to, left0);
					}
				}
			}
		}
	}
}
