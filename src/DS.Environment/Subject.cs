using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS.Environment
{
	public abstract class Subject : Connection
	{
		public bool IsStarted { get; private set; }

		public bool IsRemoved { get; set; }

		public Subject ()
		{
			DSEnvironment.Instance.Add (this);
		}

		private Subject (bool dummy) { }

		public void Start ()
		{
			IsStarted = true;
		}

		private static readonly Random _R = new Random ();

		/// <summary>
		/// Returns true if packet is finished.
		/// Returns false if packet was forwarded.
		/// </summary>
		internal bool ReceiveNextMessage (Packet p)
		{
			p.Arrive ();

			if (IsRemoved) {
				p.State = PacketState.NoRouteFound;
				return true;
			}

			Receive (p);
			return p.State == PacketState.Delivered;
		}

		protected abstract void Receive (Packet packet);

		internal protected virtual void Timeout ()
		{
			Console.WriteLine (this + " timeout");
		}

		public abstract List<Connection> ReportKnownNodes ();

		public abstract Dictionary<Connection, string> ReportKnownConnections ();
	}
}
