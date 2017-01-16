using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.Environment
{
	public enum PacketState
	{
		InTransit,
		Delivered,
		NoRouteFound,
	}

	public class Packet
	{
		public readonly DateTime TCreation = DateTime.UtcNow;

		private readonly List<Connection> _Route = new List<Connection> ();

		public Connection From {
			get;
			private set;
		}

		public Connection To {
			get;
			private set;
		}

		public readonly DsMsg Payload;

		public double Progress {
			get; internal set;
		}

		public PacketState State {
			get; internal set;
		}

		internal Packet (Subject from, Connection to, DsMsg payload)
		{
			if (from == null) {
				throw new ArgumentNullException ();
			}
			if (to == null) {
				throw new ArgumentNullException ();
			}
			if (payload == null) {
				throw new ArgumentNullException ();
			}

			From = from;
			To = to;
			Payload = payload;
			_Route.Add (from);
			_Route.Add (to);
			State = PacketState.InTransit;
		}

		public void Arrive ()
		{
			if (State != PacketState.InTransit) {
				throw new InvalidOperationException ();
			}

			State = PacketState.Delivered;
		}

		public void Forward (Connection to)
		{
			if (to == null) {
				throw new ArgumentNullException ();
			}
			if (State != PacketState.Delivered) {
				throw new InvalidOperationException ();
			}

			From = To;
			To = to;
			Progress = 0.0;
			State = PacketState.InTransit;
			_Route.Add (to);
		}

		public override string ToString ()
		{
			return $" From {From} To {To}: {Payload}"
				.Replace ("\n", " ");
		}

		public string ToStringMultiline ()
		{
			return ""
				+ "From: " + From.ToString ().Replace ("\n", " ") + "\n"
				+ "To: " + To.ToString ().Replace ("\n", " ") + "\n"
				+ Payload.ToStringShort ();
		}

		public string ToStringShort ()
		{
			return ""
				+ From.ToStringShort ()
				+ "->" + To.ToStringShort ()
				+ ": " + Payload.ToStringShort ()
				+ $" ({Progress:0.00})";
		}

		public string ToStringDetailed ()
		{
			var sb = new StringBuilder ();

			sb.Append (State.ToString ());
			sb.AppendLine ();

			sb.Append ($"From {From} to {To}");
			sb.AppendLine ();

			sb.Append ("Payload: " + Payload);
			sb.AppendLine ();

			sb.Append ("Route: ");
			sb.Append (string.Join (", ", _Route.Select (c => c.Coordinate)));

			return sb.ToString ();
		}
	}
}
