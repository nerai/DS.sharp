using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NetworkDrawing
{
	public class DrawerController
	{
		private readonly Random _R = new Random ();
		private readonly Dictionary<ISubject, NodeVis> _Nodes = new Dictionary<ISubject, NodeVis> ();
		private readonly Dictionary<IPacket, PacketVis> _Packets = new Dictionary<IPacket, PacketVis> ();

		public bool FreezeLayout = false;
		public bool CircleLayout = false;

		public event Action<int> NodeCountChanged = delegate { };

		public event Action<int> PacketCountChanged = delegate { };

		public event Action InvalidateVisual = delegate { };

		public event Action<NodeVis> SubjectAdded = delegate { };

		public event Action<NodeVis> SubjectRemoved = delegate { };

		public event Action<PacketVis> PacketAdded = delegate { };

		public event Action<PacketVis> PacketForwarded = delegate { };

		public event Action<PacketVis> PacketArrived = delegate { };

		public event Action<PacketVis> PacketRemoved = delegate { };

		public DrawerController ()
		{
			new Thread (BackgroundImproveSpacing) {
				IsBackground = true,
				Name = "DSDrawer background update",
			}.Start ();
		}

		public NodeVis[] Nodes {
			get {
				lock (_Nodes) {
					return _Nodes.Values.ToArray ();
				}
			}
		}

		public PacketVis[] Packets {
			get {
				lock (_Packets) {
					return _Packets.Values.ToArray ();
				}
			}
		}

		public int SubjectCount {
			get {
				lock (_Nodes) {
					return _Nodes.Count;
				}
			}
		}

		public int PacketCount {
			get {
				lock (_Packets) {
					return _Packets.Count;
				}
			}
		}

		public void OnSubjectAdded (ISubject subj)
		{
			var p = new Point (_R.NextDouble (), _R.NextDouble ());
			var sv = new NodeVis () {
				Center = p,
				Subj = subj,
			};
			int n;
			lock (_Nodes) {
				_Nodes.Add (subj, sv);
				n = _Nodes.Count;
			}
			NodeCountChanged (n);
			SubjectAdded (sv);
		}

		public void OnPacketAdded (IPacket pack)
		{
			Point p;
			lock (_Nodes) {
				if (pack.From == null) {
					var to = _Nodes.TryGet (pack.To);
					if (to == null) {
						return;
					}
					p = to.Center;
					var dx = 0.1 * (2 * _R.NextDouble () - 1);
					var dy = 0.1 * (2 * _R.NextDouble () - 1);
					p.Offset (dx, dy);
				}
				else {
					var from = _Nodes.TryGet ((ISubject) pack.From);
					if (from == null) {
						return;
					}
					p = from.Center;
				}
			}
			var pv = new PacketVis () {
				Origin = p,
				Center = p,
				Pack = pack,
			};
			int n;
			lock (_Packets) {
				_Packets.Add (pack, pv);
				n = _Packets.Count;
			}
			PacketCountChanged (n);
			PacketAdded (pv);
		}

		public void OnPacketForwarded (IPacket p)
		{
			PacketVis pv;
			lock (_Packets) {
				if (!_Packets.TryGetValue (p, out pv)) {
					return;
				}
			}
			pv.Origin = pv.Center;
			PacketForwarded (pv);
		}

		public void OnPacketArrived (IPacket p)
		{
			Debug.Assert (p.State == PacketState.Delivered);

			// todo: vielleicht nicht gleich löschen
			PacketVis pv;
			int n;
			lock (_Packets) {
				if (!_Packets.TryGetValue (p, out pv)) {
					return;
				}
				_Packets.Remove (p);
				n = _Packets.Count;
			}
			PacketCountChanged (n);
			PacketArrived (pv);
		}

		public void OnSubjectRemoved (ISubject n)
		{
			NodeVis nv;
			int count;
			lock (_Nodes) {
				if (!_Nodes.TryGetValue (n, out nv)) {
					return;
				}
				_Nodes.Remove (n);
				count = _Nodes.Count;
			}
			NodeCountChanged (count);
			SubjectRemoved (nv);
		}

		public void OnPacketRemoved (IPacket p)
		{
			PacketVis pv;
			int n;
			lock (_Packets) {
				if (!_Packets.TryGetValue (p, out pv)) {
					return;
				}
				_Packets.Remove (p);
				n = _Packets.Count;
			}
			PacketCountChanged (n);
			PacketArrived (pv);
		}

		public void RemovePacket (PacketVis pv)
		{
			E.RemovePacket (pv.Pack);
		}

		public void RemoveNode (NodeVis n)
		{
			E.RemoveNode (n.Subj);
		}

		private void BackgroundImproveSpacing ()
		{
			while (true) {
				Thread.Sleep (20);
				try {
					/*
					bool isDragging;
					lock (_Nodes) {
						isDragging = _Nodes.Values.Any (node => node.DragOrigin != null);
					}
					if (!isDragging) {
					*/
					ImproveSpacing ();
					//}
					InvalidateVisual ();
				}
				catch (TaskCanceledException ex) {
					Console.Error.WriteLine (ex);
				}
			}
		}

		private void ImproveSpacing ()
		{
			if (FreezeLayout) {
				return;
			}

			NodeVis[] nodes;
			lock (_Nodes) {
				nodes = _Nodes.Values.ToArray ();
			}
			if (nodes.Length == 0) {
				return;
			}

			var targets = new Dictionary<NodeVis, Point> ();
			foreach (var node in nodes) {
				if (node.IsDragged) {
					continue;
				}

				var knows = node.Subj.ReportKnownNodes ();
				var target = new Vector (node.Center.X, node.Center.Y);
				//target.X += 0.01 * (_R.NextDouble () - 0.5);
				//target.Y += 0.01 * (_R.NextDouble () - 0.5);

				foreach (var dst in nodes) {
					if (node == dst) {
						continue;
					}

					var dist = node.Center.Distance (dst.Center);
					var repelforce = 0.1 / Math.Pow (dist + 0.1, 2);
					if (knows.Contains (dst.Subj)) {
						repelforce *= 0.1;
					}

					var v = dst.Center - node.Center;
					v *= -repelforce;
					target += v;
				}

				var x = 0.9 * node.Center.X + 0.1 * target.X;
				var y = 0.9 * node.Center.Y + 0.1 * target.Y;
				targets[node] = new Point (x, y);
			}

			if (CircleLayout) {
				const double circlePreference = 0.3;
				var r = targets.Values.Average (p => Math.Sqrt (p.X * p.X + p.Y * p.Y));
				r *= circlePreference;
				var ordered = targets
					.OrderBy (pair => pair.Key.Subj.Coordinate)
					.ToArray ();
				for (int i = 0; i < ordered.Length; i++) {
					var pair = ordered[i];
					var phi = 2 * Math.PI * i / ordered.Length;
					var x = r * Math.Cos (phi) + (1 - circlePreference) * pair.Value.X;
					var y = r * Math.Sin (phi) + (1 - circlePreference) * pair.Value.Y;
					targets[pair.Key] = new Point (x, y);
				}
			}

			var pts = targets.Values;
			var min = pts.Min ();
			var max = pts.Max ();

			foreach (var pair in targets) {
				// normalize
				var src = pair.Value;
				var x = (src.X - min.X + 0.01) / (max.X - min.Y + 0.02);
				var y = (src.Y - min.Y + 0.01) / (max.Y - min.Y + 0.02);

				// 50% movement to target
				var c = pair.Key.Center;
				x = 0.5 * c.X + 0.5 * x;
				y = 0.5 * c.Y + 0.5 * y;

				pair.Key.Center = new Point (x, y);
			}
		}
	}
}
