using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.Environment;

namespace DS.Structures.SkipListPlusDS
{
	public class SkipNode : NodeBase
	{
		public override ulong Coordinate { get; }

		private readonly Dictionary<uint, Neighbourhood> _Ns = new Dictionary<uint, Neighbourhood> ();

		internal SkipNode (ulong coord)
		{
			Coordinate = coord;
		}

		protected override void Receive (Packet packet)
		{
			var payload = packet.Payload;

			if (payload is DsMsgLinearize) {
				var msg = (DsMsgLinearize) payload;
				var work = new List<Tuple<Connection, Connection>> ();
				foreach (var con in msg.Cons) {
					work.AddRange (Linearize (con));
				}
				SendLinearizations (work);
			}
			else if (payload is DsMsgSearch) {
				Search (packet);
			}
			else {
				throw new InvalidOperationException ();
			}
		}

		private void Search (Packet packet)
		{
			var sid = ((DsMsgSearch) packet.Payload).SearchId;

			if (Coordinate == sid) {
				Console.WriteLine ("FOUND " + sid);
				return;
			}

			Neighbourhood[] Ns;
			lock (_Ns) {
				Ns = _Ns.Values.ToArray ();
			}
			var closest = Ns
				.SelectMany (N => N.AllNodes)
				.MinBy (n => Math.Abs ((long) n.Coordinate - (long) sid));

			if (closest == null || closest == this) {
				Console.WriteLine ("FAILED TO FIND " + sid);
				return;
			}

			packet.Forward (closest);
		}

		protected override void Timeout ()
		{
			Neighbourhood[] Ns;
			lock (_Ns) {
				Ns = _Ns.Values.ToArray ();
			}

			var work = new List<Tuple<Connection, Connection>> ();
			foreach (var N in Ns) {
				work.AddRange (N.Rule1a ());
				work.AddRange (N.Rule1b ());
			}
			SendLinearizations (work);
		}

		private Neighbourhood GetN (uint level)
		{
			Neighbourhood list;
			lock (_Ns) {
				if (!_Ns.TryGetValue (level, out list)) {
					list = new Neighbourhood (this, level);
					_Ns.Add (level, list);
				}
			}
			return list;
		}

		private IEnumerable<Tuple<Connection, Connection>> Linearize (Connection v)
		{
			// Console.WriteLine (this + " linearizes " + v);

			if (v == null) {
				yield break;
			}
			if (this == v) {
				yield break;
			}

			var mH = Hash;
			var vH = v.Hash;
			var removed = new List<Connection> ();

			var sharedBitCount = (mH ^ vH).BSR ();
			for (uint i = 0; i <= sharedBitCount; i++) {
				var N = GetN (i);
				lock (N) {
					var expelled = N.Linearize (v);
					removed.AddRange (expelled);
				}
			}

			var known = ReportKnownNodes ().ToArray ();
			removed = removed
				.Distinct ()
				.Where (n => !known.Contains (n))
				.ToList ();

			foreach (var node in removed) {
				var to = FindClosestNodeByHash (node.Coordinate);
				Debug.Assert (to != this);
				Debug.Assert (to != null);
				yield return new Tuple<Connection, Connection> (to, node);
			}
		}

		private Connection FindClosestNodeByHash (ulong searchCoordinate)
		{
			var searchHash = Connection.GetHash (searchCoordinate);
			var commonBits = (Hash ^ searchHash).BSR ();
			var N = GetN (commonBits);
			Connection[] options;
			lock (N) {
				options = N.AllNodes
				.Where (n => n.Coordinate > Coordinate == searchCoordinate > Coordinate)
				.ToArray ();
			}
			var best = options.MaxBy (o => (o.Hash ^ searchHash).BSR (false));
			return best;
		}

		public override List<Connection> ReportKnownNodes ()
		{
			var res = new List<Connection> ();
			lock (_Ns) {
				foreach (var N in _Ns.Values) {
					res.AddRange (N.AllNodes);
				}
			}
			return res.Distinct ().ToList ();
		}

		public override Dictionary<Connection, string> ReportKnownConnections ()
		{
			var res = new Dictionary<Connection, string> ();
			lock (_Ns) {
				foreach (var N in _Ns.Values.OrderBy (N => N.Level)) {
					foreach (var node in N.AllNodes) {
						string s;
						if (res.TryGetValue (node, out s)) {
							s += "," + N.Level;
						}
						else {
							s = "" + N.Level;
						}
						res[node] = s;
					}
				}
			}
			return res;
		}
	}
}
