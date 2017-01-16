using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.Environment;

namespace DS.Structures.LinkedListDS
{
	public class LLNode : NodeBase
	{
		public override ulong Coordinate { get; }

		private readonly object _Lock = new object ();
		private Connection _Left = null;
		private Connection _Right = null;

		public bool IsLeaving { get; private set; }
		public bool IsSleeping { get; private set; }

		public void Leave ()
		{
			IsLeaving = true;
		}

		internal LLNode (ulong coord)
		{
			Coordinate = coord;
		}

		protected override void Receive (Packet packet)
		{
			IsSleeping = false;
			var payload = packet.Payload;

			if (payload is DsMsgLinearize) {
				var msg = (DsMsgLinearize) payload;
				var work = new List<Tuple<Connection, Connection>> ();
				foreach (var con in msg.Cons) {
					work.AddRange (Linearize_ (con));
				}
				SendLinearizations (work);
			}
			else if (payload is DsMsgSearch) {
				var msg = (DsMsgSearch) payload;
				Search (packet);
			}
			else if (payload is DsMsgReverse) {
				var msg = (DsMsgReverse) payload;
				Reverse (msg.Target);
			}
			else {
				throw new InvalidOperationException ();
			}
		}

		private void Reverse (Connection target)
		{
			var msg = new DsMsgLinearize (new[] { this });
			if (_Left == target) {
				if (!IsLeaving) { // asymmetric: Enforce hierarchy of leaving nodes
					DSEnvironment.Instance.CreatePacket (this, _Left, msg);
					_Left = null;
				}
			}
			if (_Right == target) {
				DSEnvironment.Instance.CreatePacket (this, _Right, msg);
				_Right = null;
			}
		}

		private void Search (Packet packet)
		{
			var sid = ((DsMsgSearch) packet.Payload).SearchId;

			if (Coordinate == sid) {
				Console.WriteLine ("FOUND " + sid);
				return;
			}

			lock (_Lock) {
				if (false
				|| (_Left == null || _Left.Coordinate < sid) && (sid < Coordinate)
				|| (_Right == null || _Right.Coordinate > sid) && (sid > Coordinate)
				) {
					Console.WriteLine ("FAILED TO FIND " + sid);
					return;
				}

				if (sid < Coordinate && _Left != null) {
					packet.Forward (_Left);
				}
				if (sid > Coordinate && _Right != null) {
					packet.Forward (_Right);
				}
			}
		}

		protected override void Timeout ()
		{
			if (IsSleeping) {
				return;
			}

			var work = new List<Tuple<Connection, Connection>> ();

			lock (_Lock) {
				if (IsLeaving) {
					var msg = new DsMsgReverse (this);
					if (_Left != null) {
						DSEnvironment.Instance.CreatePacket (this, _Left, msg);
					}
					if (_Right != null) {
						DSEnvironment.Instance.CreatePacket (this, _Right, msg);
					}
					work.Add (new Tuple<Connection, Connection> (_Left, _Right));
					work.Add (new Tuple<Connection, Connection> (_Right, _Left));
					IsSleeping = true;
				}
				else {
					if (_Left != null) {
						if (_Left.Coordinate < Coordinate) {
							// _Left.Linearize (this);
							work.Add (new Tuple<Connection, Connection> (_Left, this));
						}
						else {
							work.AddRange (Linearize_ (_Left));
							_Left = null;
						}
					}

					if (_Right != null) {
						if (_Right.Coordinate > Coordinate) {
							// _Right.Linearize (this);
							work.Add (new Tuple<Connection, Connection> (_Right, this));
						}
						else {
							work.AddRange (Linearize_ (_Right));
							_Right = null;
						}
					}
				}
			}

			SendLinearizations (work);
		}

		private IEnumerable<Tuple<Connection, Connection>> Linearize_ (Connection v)
		{
			Console.WriteLine (this + " linearizes " + v);

			if (v == null) {
				yield break;
			}

			lock (_Lock) {
				var mC = (long) Coordinate;
				var vC = (long) v.Coordinate;
				var lC = _Left == null ? long.MinValue : (long) _Left.Coordinate;
				var rC = _Right == null ? long.MaxValue : (long) _Right.Coordinate;

				if (vC < lC) {
					if (_Left != null) {
						// _Left.Linearize (v);
						yield return new Tuple<Connection, Connection> (_Left, v);
					}
				}
				if (lC < vC && vC < mC) {
					// v.Linearize (_Left);
					if (_Left != null) {
						yield return new Tuple<Connection, Connection> (v, _Left);
					}
					_Left = v;
				}
				if (mC < vC && vC < rC) {
					// v.Linearize (_Right);
					if (_Right != null) {
						yield return new Tuple<Connection, Connection> (v, _Right);
					}
					_Right = v;
				}
				if (rC < vC) {
					if (_Right != null) {
						// _Right.Linearize (v);
						yield return new Tuple<Connection, Connection> (_Right, v);
					}
				}
			}
		}

		public override List<Connection> ReportKnownNodes ()
		{
			var res = new List<Connection> ();
			lock (_Lock) {
				if (_Left != null) {
					res.Add (_Left);
				}
				if (_Right != null) {
					res.Add (_Right);
				}
			}
			return res;
		}

		public override Dictionary<Connection, string> ReportKnownConnections ()
		{
			var dict = new Dictionary<Connection, string> ();
			lock (_Lock) {
				if (_Left != null) {
					dict.Add (_Left, "L");
				}
				if (_Right != null) {
					dict.Add (_Right, "R");
				}
			}
			return dict;
		}
	}
}
