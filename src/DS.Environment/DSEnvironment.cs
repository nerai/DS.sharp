using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS.Environment
{
	public class DSEnvironment
	{
		private static readonly Random _R = new Random ();

		public static readonly DSEnvironment Instance = new DSEnvironment ();

		private readonly List<Subject> _Nodes = new List<Subject> ();

		private readonly List<Subject> _TimeoutQueue = new List<Subject> ();

		private readonly HashSet<Subject> _MustStillWorkOnMessageThisRound = new HashSet<Subject> ();

		private readonly List<Packet> _Packets = new List<Packet> ();

		public event Action<Subject> SubjectAdded = delegate { };

		public event Action<Subject> SubjectRemoved = delegate { };

		public event Action<Packet> PacketAdded = delegate { };

		public event Action<Packet> PacketForwarded = delegate { };

		public event Action<Packet> PacketArrived = delegate { };

		public event Action<Packet> PacketRemoved = delegate { };

		public event Action RoundChanged = delegate { };

		private bool _MsgReadyForNextRound = false;
		private bool _TimeoutReadyForNextRound = false;
		public uint Round { get; private set; }

		private readonly ManualResetEvent EnableAutonomousWorkMsgs = new ManualResetEvent (false);
		private readonly ManualResetEvent EnableAutonomousWorkTicks = new ManualResetEvent (false);

		private DSEnvironment ()
		{
			new Thread (AutonomousWorkMsgsThread) {
				IsBackground = true,
				Name = "AutonomousWorkMsgsThread",
			}.Start ();
			new Thread (AutonomousWorkTicksThread) {
				IsBackground = true,
				Name = "AutonomousWorkTicksThread",
			}.Start ();
		}

		public void EnableAutonomousMsgs (bool enable)
		{
			if (enable) {
				EnableAutonomousWorkMsgs.Set ();
			}
			else {
				EnableAutonomousWorkMsgs.Reset ();
			}
		}

		public void EnableAutonomousTicks (bool enable)
		{
			if (enable) {
				EnableAutonomousWorkTicks.Set ();
			}
			else {
				EnableAutonomousWorkTicks.Reset ();
			}
		}

		public IEnumerable<Subject> Subjects ()
		{
			lock (_Nodes) {
				return _Nodes.ToList ();
			}
		}

		/// <summary>
		/// List of packets, ordered by progress descending
		/// </summary>
		public IEnumerable<Packet> Packets ()
		{
			lock (_Packets) {
				return _Packets.ToList ();
			}
		}

		private void ReadyForNextRound (bool sourceIsMsg)
		{
			if (sourceIsMsg) {
				_MsgReadyForNextRound = true;
			}
			else {
				_TimeoutReadyForNextRound = true;
			}
			if (_MsgReadyForNextRound && _TimeoutReadyForNextRound) {
				Round++;
				_MsgReadyForNextRound = false;
				_TimeoutReadyForNextRound = false;
				RoundChanged ();
			}
		}

		public const bool FifoPackets = true;

		public void ForcePacketArrival (Packet p)
		{
			p.Progress = 1.0;
		}

		public void NextMsg ()
		{
			Packet[] packets;
			lock (_Packets) {
				packets = _Packets
					.Where (p => ((Subject) p.To).IsStarted)
					.ToArray ();
			}
			if (packets.Length == 0) {
				return;
			}

			if (!FifoPackets) {
				packets = packets.OrderBy (p => _R.NextDouble ()).ToArray ();
			}

			var advance = 1.0 - packets.First ().Progress;
			foreach (var p in packets) {
				p.Progress += advance;
				advance *= 0.9 + 0.1 * _R.NextDouble ();
			}
			var pack = packets[0];

			/*
			var maxProgress = packets.Max (p => p.Progress);
			var spare = 1.0 - maxProgress;

			var packetsGroups = packets.GroupBy (p => p.To).ToList ();
			foreach (var group in packetsGroups) {
				var add = 0.9 * spare;
				foreach (var p in group.OrderBy (p => p.TCreation)) {
					add *= (4 + _R.NextDouble ()) / 5;
					p.Progress += add;
				}
			}

			var pack = packets.MaxBy (p => p.Progress);
			*/

			WorkOnPacket (pack);
		}

		private void SubjectDidWorkOnMessages (Subject to)
		{
			_MustStillWorkOnMessageThisRound.Remove (to);
			if (_MustStillWorkOnMessageThisRound.Count == 0) {
				lock (_Packets) {
					var nodes = _Packets
						.Select (p => (Subject) p.To)
						.Where (node => node.IsStarted);
					foreach (var node in nodes) {
						_MustStillWorkOnMessageThisRound.Add (node);
					}
				}
				ReadyForNextRound (true);
			}
		}

		public void AdvanceAllPackets (double by = 0.02)
		{
			Packet[] packets;
			lock (_Packets) {
				packets = _Packets
					.Where (p => ((Subject) p.To).IsStarted)
					.ToArray ();
			}
			if (packets.Length == 0) {
				return;
			}

			if (FifoPackets) {
				foreach (var p in packets) {
					p.Progress += by;
				}
			}
			else {
				throw new NotImplementedException ();
			}

			foreach (var p in packets) {
				if (p.Progress >= 1.0) {
					WorkOnPacket (p);
				}
			}
		}

		private void WorkOnPacket (Packet p)
		{
			p.Progress = 1.0;

			lock (_Packets) {
				_Packets.Remove (p);
			}

			if (((Subject) p.To).ReceiveNextMessage (p)) {
				PacketRemoved (p);
			}
			else {
				PacketForwarded (p);
				lock (_Packets) {
					// Must re-insert because of sorting
					_Packets.Add (p);
				}
			}
			SubjectDidWorkOnMessages ((Subject) p.To);
		}

		public void CreatePacket (Subject from, Connection to, DsMsg payload)
		{
			var p = new Packet (from, to, payload);
			lock (_Packets) {
				_Packets.Add (p);
			}
			PacketAdded (p);
		}

		public void NextTimeout ()
		{
			Subject t;
			lock (_TimeoutQueue) {
				if (_TimeoutQueue.Count == 0) {
					lock (_Nodes) {
						_TimeoutQueue.AddRange (_Nodes.Where (n => n.IsStarted));
					}
					_TimeoutQueue.Sort ((s1, s2) => _R.NextDouble () < 0.5 ? +1 : -1);
					if (_TimeoutQueue.Count == 0) {
						return;
					}
				}
				t = _TimeoutQueue[0];
				_TimeoutQueue.RemoveAt (0);
				if (_TimeoutQueue.Count == 0) {
					ReadyForNextRound (false);
				}
			}
			t.Timeout ();
		}

		private void AutonomousWorkMsgsThread ()
		{
			while (true) {
				EnableAutonomousWorkMsgs.WaitOne ();

				AdvanceAllPackets ();

				Thread.Sleep (100);
			}
		}

		private void AutonomousWorkTicksThread ()
		{
			while (true) {
				EnableAutonomousWorkTicks.WaitOne ();

				long nodeCount;
				long packetCount;
				lock (_Nodes) {
					nodeCount = _Nodes.Count;
				}
				lock (_Packets) {
					packetCount = _Packets.Count;
				}
				var overcrowding = 1.0 * packetCount / nodeCount;
				var chance = 1.0 / (0.1 + overcrowding * overcrowding);

				if (_R.NextDouble () < chance) {
					NextTimeout ();
				}

				Thread.Sleep (100);
			}
		}

		public void Add (Subject s)
		{
			lock (_Nodes) {
				_Nodes.Add (s);
			}
			SubjectAdded (s);
			s.Start ();
		}

		public bool RemovePacket (Packet p)
		{
			bool b;
			lock (_Packets) {
				b = _Packets.Remove (p);
			}
			PacketRemoved (p);
			return b;
		}

		public bool RemoveNode (Subject n)
		{
			bool b;
			lock (_Nodes) {
				b = _Nodes.Remove (n);
			}
			SubjectRemoved (n);
			return b;
		}
	}
}
