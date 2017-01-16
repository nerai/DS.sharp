using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.Environment;

namespace DS.Structures.HypercubeDS_old
{
	public class HypercubeNode : NodeBase
	{
		public readonly uint Dimension;

		private readonly Dictionary<uint, Connection> _Neighbours = new Dictionary<uint, Connection> ();

		public override ulong Coordinate {
			get;
		}

		internal HypercubeNode (uint dim, ulong coord)
		{
			Dimension = dim;
			Coordinate = coord;
		}

		protected override void Receive (Packet packet)
		{
			var payload = packet.Payload;

			if (payload is DsMsgLinearize) {
				var msg = (DsMsgLinearize) payload;
				foreach (var con in msg.Cons) {
					var bit = (con.Coordinate ^ Coordinate).BSF ();
					lock (_Neighbours) {
						_Neighbours[bit] = con;
					}
				}
			}
			else if (payload is DsMsgSearch) {
				var msg = (DsMsgSearch) payload;
				var searchId = msg.SearchId;
				if (searchId == Coordinate) {
					Console.WriteLine (msg + " arrived at target " + this);
					return;
				}

				Connection route = null;
				lock (_Neighbours) {
					foreach (var bit in (searchId ^ Coordinate).BsfStream ()) {
						if (_Neighbours.TryGetValue (bit, out route)) {
							break;
						}
					}
				}
				if (route == null) {
					throw new InvalidOperationException ("missing route to target");
				}
				Console.WriteLine (""
					+ "Transfer " + msg
					+ " to " + Extensions.FormatBits (Dimension, searchId)
					+ " via " + route
					+ " at " + this);
				packet.Forward (route);
			}
			else {
				throw new InvalidOperationException ();
			}
		}

		protected override void Timeout ()
		{
		}

		public override string ToString ()
		{
			return "#" + Extensions.FormatBits (Dimension, Coordinate);
		}

		public override List<Connection> ReportKnownNodes ()
		{
			lock (_Neighbours) {
				return _Neighbours.Values.ToList ();
			}
		}

		public override Dictionary<Connection, string> ReportKnownConnections ()
		{
			throw new NotImplementedException ();
		}
	}
}
