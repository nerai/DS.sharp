using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NetworkDrawing
{
	public partial class NetworkDrawer : UserControl
	{
		private readonly DrawerController _C;

		public event Action<int> NodeCountChanged = delegate { };

		public event Action<int> PacketCountChanged = delegate { };

		private NodeVis Dragging = null;
		private NodeVis ControlNode = null;

		private readonly ObservableCollection<NodeVis> _ObservableNodes = new ObservableCollection<NodeVis> ();
		private readonly ObservableCollection<PacketVis> _ObservablePackets = new ObservableCollection<PacketVis> ();

		private readonly Dictionary<PacketVis, DateTime> _RecentlyArrivedPackets = new Dictionary<PacketVis, DateTime> ();
		private readonly Dictionary<PacketVis, DateTime> _RecentlyCreatedPackets = new Dictionary<PacketVis, DateTime> ();

		public NetworkDrawer ()
		{
			InitializeComponent ();

			_C = new DrawerController ();
			_C.InvalidateVisual += () => {
				Dispatcher.Invoke (lstPackets.Items.Refresh);
				Dispatcher.Invoke (InvalidateVisual, DispatcherPriority.Background);
			};
			_C.NodeCountChanged += NodeCountChanged;
			_C.PacketCountChanged += PacketCountChanged;
			_C.SubjectAdded += C_SubjectAdded;
			_C.SubjectRemoved += C_SubjectRemoved;
			_C.PacketAdded += C_PacketAdded;
			_C.PacketForwarded += C_PacketForwarded;
			_C.PacketArrived += C_PacketArrived;
			_C.PacketRemoved += C_PacketRemoved;

			lstPackets.ItemsSource = _ObservablePackets;
		}

		public bool DisplayPacketList {
			get {
				bool v = false;
				Dispatcher.Invoke ((Action) (() => {
					v = grpArrivingPackets.Visibility == Visibility.Hidden;
				}));
				return v;
			}
			set {
				Dispatcher.Invoke ((Action) (() => {
					grpArrivingPackets.Visibility = Visibility.Hidden;
				}));
			}
		}

		private void C_SubjectRemoved (NodeVis nv)
		{
			Dispatcher.BeginInvoke ((Action) (() => {
				_ObservableNodes.Remove (nv);
			}));
		}

		private void C_PacketRemoved (PacketVis pv)
		{
			Dispatcher.BeginInvoke ((Action) (() => {
				_ObservablePackets.Remove (pv);
			}));
		}

		private void C_PacketArrived (PacketVis pv)
		{
			// todo _RecentlyArrivedPackets.Add (pv);

			Dispatcher.BeginInvoke ((Action) (() => {
				_ObservablePackets.Remove (pv);
			}));
		}

		private void C_PacketForwarded (PacketVis pv)
		{
			Dispatcher.BeginInvoke ((Action) (() => {
				_ObservablePackets.Remove (pv);
				_ObservablePackets.Add (pv);
			}));
		}

		private void C_PacketAdded (PacketVis pv)
		{
			_RecentlyCreatedPackets.Add (pv, DateTime.UtcNow);

			Dispatcher.BeginInvoke ((Action) (() => {
				_ObservablePackets.Add (pv);
			}));
		}

		private void C_SubjectAdded (NodeVis nv)
		{
		}

		public int SubjectCount {
			get {
				return _C.SubjectCount;
			}
		}

		public int PacketCount {
			get {
				return _C.PacketCount;
			}
		}

		public bool FreezeLayout {
			get {
				return _C.FreezeLayout;
			}
			set {
				_C.FreezeLayout = value;
			}
		}

		public bool CircleLayout {
			get {
				return _C.CircleLayout;
			}
			set {
				_C.CircleLayout = value;
			}
		}

		public Point ToScreen (Point p)
		{
			var x = (0.05 + p.X * 0.9) * RenderSize.Width;
			var y = (0.05 + p.Y * 0.9) * RenderSize.Height;
			return new Point (x, y);
		}

		public Point FromScreen (Point p)
		{
			var x = (p.X / RenderSize.Width - 0.05) / 0.9;
			var y = (p.Y / RenderSize.Height - 0.05) / 0.9;
			return new Point (x, y);
		}

		private void DrawArrowhead (
			DrawingContext dc,
			Point p0, // start point (base line of arrow)
			Point p1, // target point (pointy end of arrow)
			bool favorTargetPoint, // draw closer to end instead of beginning of line
			double preferredLength,
			double preferredOffset = 0, // stay away from either end by this much, of enough room to spare
			double baseWidthFactor = 0.3)
		{
			/*
			 * First, calculate the actual start and end points
			 */
			var v = p1 - p0;
			var vn = v.Clone ();
			vn.Normalize ();

			var len = v.Length;
			if (len >= preferredLength + 2 * preferredOffset) {
				// too long, cut off
				v = vn * (preferredLength + 2 * preferredOffset);
				if (favorTargetPoint) {
					p0 = p1 - v;
				}
				else {
					p1 = p0 + v;
				}
			}
			if (len > preferredLength) {
				// fulfill preferred offset as far as possible
				var spare = v.Length - preferredLength;
				p0 += vn * spare / 2;
				p1 -= vn * spare / 2;
				v = p1 - p0;
			}

			/*
			 * Now calculate the arrow shape and draw it
			 */
			v *= baseWidthFactor * 0.5;
			var p2 = new Point (p0.X + v.Y, p0.Y - v.X);
			var p3 = new Point (p0.X - v.Y, p0.Y + v.X);

			var brush = Brushes.Black;
			//var pen = new Pen (Brushes.Black, 1.0);

			var geom = new StreamGeometry ();
			using (var gc = geom.Open ()) {
				gc.BeginFigure (p1, true, true);
				gc.LineTo (p2, true, true);
				gc.LineTo (p3, true, true);
			}
			geom.Freeze ();
			dc.DrawGeometry (brush, null, geom);
		}

		protected override void OnRender (DrawingContext dc)
		{
			base.OnRender (dc);

			dc.DrawRectangle (Brushes.White, null, new Rect (RenderSize));

			var nodes = _C.Nodes
				.OrderBy (node => node.Highlight ? 1 : 0)
				.ThenByDescending (node => node.Center.Y)
				.ToDictionary (node => node.Subj, node => node);
			const int nodeRectWi = 64;
			const int nodeRectHi = 50;
			var nodePos_ = new Dictionary<NodeVis, Point> ();
			var nodeRect = new Dictionary<NodeVis, Rect> ();

			foreach (var node in nodes.Values) {
				var p0 = ToScreen (node.Center);
				var rect = new Rect (
					p0.X - nodeRectWi / 2,
					p0.Y - nodeRectHi / 2,
					nodeRectWi,
					nodeRectHi);
				nodePos_[node] = p0;
				nodeRect[node] = rect;
			}

			// Connections
			foreach (var node in nodes.Values) {
				var p0 = nodePos_[node];
				var knows = node.Subj.ReportKnownConnections ();

				foreach (var pair in knows) {
					var to = pair.Key;
					var reason = pair.Value;
					if (!nodes.ContainsKey (to)) {
						continue;
					}
					var p1 = ToScreen (nodes[to].Center);

					// Indent line away from center for a more 'round' shape
					var pm = new Point (
						(p0.X + p1.X) / 2,
						(p0.Y + p1.Y) / 2);
					var center = ToScreen (new Point (0.5, 0.5));
					var lineCenterToPM = pm - center;
					pm = Point.Add (pm, lineCenterToPM * 0.12);

					// Draw lines
					if (node.Highlight) {
						dc.DrawLine (DrawerBuffer.PenNodeConnectionHighlighted, p0, pm);
						dc.DrawLine (DrawerBuffer.PenNodeConnectionHighlighted, pm, p1);
					}
					else {
						dc.DrawLine (DrawerBuffer.PenNodeConnection, p0, pm);
						dc.DrawLine (DrawerBuffer.PenNodeConnection, pm, p1);
					}

					// Draw arrowheads
					DrawArrowhead (dc, p0, pm, false, 30, 80, 0.4);
					DrawArrowhead (dc, pm, p1, true, 30, 45, 0.4);

					// Draw connection info
					var ft = new FormattedText (
						reason,
						CultureInfo.InvariantCulture,
						FlowDirection.LeftToRight,
						DrawerBuffer.TypeVerdana,
						node.Highlight ? 16 : 10.0,
						Brushes.Black);
					dc.DrawText (ft, p0 + (pm - p0) * 0.5);
				}
			}

			// Highlight, Body, Caption
			var hlNode = nodes.Values.SingleOrDefault (n => n.Highlight);

			foreach (var node in nodes.Values) {
				var rect = nodeRect[node];

				// Draw second node border according to its relation to the highlighted node
				var penh = DrawerBuffer.GetPenForNodeBorder (node, hlNode);
				if (penh != null) {
					dc.DrawRectangle (null, penh, rect);
				}

				// Draw main node border
				//dc.DrawRectangle (null, DrawerBuffer.PenNodeFrame1, rect);
				dc.DrawRectangle (Brushes.Wheat, DrawerBuffer.PenNodeFrame2, rect);

				var s = node.Subj.ToString ();
				s = s.Replace (" ", "\n").Replace ("[", "").Replace ("]", "");
				var ft = new FormattedText (
					s,
					CultureInfo.InvariantCulture,
					FlowDirection.LeftToRight,
					DrawerBuffer.TypeVerdana,
					18.0,
					Brushes.Black);
				dc.DrawText (ft, nodeRect[node].TopLeft + new Vector (+4, +4));
			}

			// Packets
			var packets = _C.Packets
				.OrderBy (p => p.Highlight ? 1 : 0)
				.ThenByDescending (p => p.Center.Y)
				.ToArray ();

			foreach (var packet in packets) {
				// Determine packet position
				var srcNode = packet.Pack.From == null
					? null
					: nodes.TryGet (packet.Pack.From);
				var dstNode = nodes.TryGet (packet.Pack.To);
				if (dstNode == null) {
					continue;
				}

				var influences = new List<Point> ();
				// position According To Original Route
				influences.Add (packet.Origin
					+ packet.Pack.Progress * (dstNode.Center - packet.Origin));
				if (srcNode != null) {
					// position According To Current Route
					influences.Add (srcNode.Center
						+ packet.Pack.Progress * (dstNode.Center - srcNode.Center));
				}
				packet.Center = influences.Average ();

				var queuerect = new Rect (
					ToScreen (packet.Center).X - 4,
					ToScreen (packet.Center).Y - 4,
					8,
					8);

				// Highlight
				if (packet.Highlight) {
					var largerect = new Rect (
						queuerect.Left - 45,
						queuerect.Top - 2,
						queuerect.Width + 90,
						queuerect.Height + 40);
					var penh = DrawerBuffer.PenPacketBodyHighlight;
					dc.DrawRectangle (Brushes.White, penh, largerect);

					var text = packet.Pack.Description ();
					var ft = new FormattedText (
						text,
						CultureInfo.InvariantCulture,
						FlowDirection.LeftToRight,
						DrawerBuffer.TypeVerdana,
						10.0,
						Brushes.Black);
					dc.DrawText (ft, largerect.TopLeft + new Vector (4, 10));
				}

				// Tracked
				if (packet.IsTracked) {
					var largerect = new Rect (
						queuerect.Left - 10,
						queuerect.Top - 10,
						queuerect.Width + 20,
						queuerect.Height + 20);
					dc.DrawRectangle (Brushes.Yellow, null, largerect);
				}

				// Draw packet body
				var brushPacketBody = srcNode == null
					? Brushes.Magenta
					: srcNode == dstNode
					? Brushes.Gray
					: Brushes.Blue;
				dc.DrawRectangle (
					brushPacketBody,
					DrawerBuffer.PenPacketBody,
					queuerect);

				// Connections
				foreach (var refCon in packet.Pack.ReferencedNodes ()) {
					var refNV = nodes.TryGet (refCon);
					if (refNV == null) {
						continue;
					}
					dc.DrawLine (
						DrawerBuffer.PenPacketRefLink,
						queuerect.Center (),
						nodeRect[refNV].Center ());
				}
				/* TODO: enable this in a general way, not just for 'referenced' and 'searched' nodes
				var searchId = packet.Pack.Payload.SearchedNode ();
				if (searchId.HasValue) {
					var searchNode = nodes.Values.FirstOrDefault (node => searchId.Equals (node.Subj.Coordinate));
					if (searchNode == null) {
						continue;
					}
					dc.DrawLine (
						DrawerBuffer.PenPacketSearchLink,
						queuerect.Center (),
						nodeRect[searchNode].Center ());
				}
				*/
				if (srcNode != null) {
					dc.DrawLine (
						DrawerBuffer.PenPacketFromLink,
						queuerect.Center (),
						nodeRect[srcNode].Center ());
				}
				if (dstNode != null) {
					dc.DrawLine (
						DrawerBuffer.PenPacketToLink,
						queuerect.Center (),
						nodeRect[dstNode].Center ());
				}

				/*
				// Recently created packet animations
				DateTime tCreated;
				if (_RecentlyCreatedPackets.TryGetValue (packet, out tCreated)) {
					var relT = DateTime.UtcNow.Subtract (tCreated).TotalSeconds;
					if (relT > 1) {
						_RecentlyCreatedPackets.Remove (packet);
					}
					else {
						// TODO cache
						var size = (1 - relT) * 5 + 1;
						var penRecentlyCreatedPacket = new Pen (Brushes.Green, size);
						penRecentlyCreatedPacket.Freeze ();
						dc.DrawRectangle (
							brushPacketBody,
							penRecentlyCreatedPacket,
							queuerect);
					}
				}
				*/
			}
		}

		private void Control_MouseDown (object sender, MouseButtonEventArgs e)
		{
			Control_MouseMove (sender, e);
		}

		private void Control_MouseMove (object sender, MouseEventArgs e)
		{
			var mouseabs = e.GetPosition (this);
			var mouserel = FromScreen (mouseabs);

			Func<VisBase, double> distanceToCursor = source => {
				Point p = source.Center;
				return ToScreen (p).Distance (mouseabs);
			};

			// Find closest node
			var nodes = _C.Nodes;
			var minNode = (NodeVis) nodes.MinBy (distanceToCursor);
			if (minNode != null && distanceToCursor (minNode) > 80) {
				minNode = null;
			}

			// Find closest packet
			var packets = _C.Packets;
			var minPacket = (PacketVis) packets.MinBy (distanceToCursor);
			if (minPacket != null && distanceToCursor (minPacket) > 40) {
				minPacket = null;
			}

			// Update highlight (prefer nodes over packets)
			if (minPacket != null && minNode != null) {
				if (0.5 * distanceToCursor (minNode) < distanceToCursor (minPacket)) {
					minPacket = null;
				}
				else {
					minNode = null;
				}
			}
			if (minNode != null) {
				minNode.Highlight = true;
			}
			else if (minPacket != null) {
				minPacket.Highlight = true;
			}
			foreach (var node in nodes.Where (node => node != minNode)) {
				node.Highlight = false;
			}
			foreach (var pack in packets.Where (pack => pack != minPacket)) {
				pack.Highlight = false;
			}

			var ctrlKey = Keyboard.Modifiers.HasFlag (ModifierKeys.Control);

			// Drag & drop nodes
			if (e.LeftButton == MouseButtonState.Pressed && !ctrlKey) {
				if (Dragging != null) {
					Dragging.Center = mouserel;
				}
				else if (minNode != null) {
					Dragging = minNode;
					Dragging.IsDragged = true;
					// todo: relative cursorposition merken/anwenden
				}
			}
			else {
				if (Dragging != null) {
					Dragging.IsDragged = false;
					Dragging = null;
				}
			}

			if (true
				&& minNode != null
				&& e.LeftButton == MouseButtonState.Pressed
				&& ctrlKey
				) {
				if (ControlNode == null) {
					ControlNode = minNode;
				}
			}

			// Redraw
			InvalidateVisual ();
		}

		private void lstPackets_SelectionChanged (object sender, SelectionChangedEventArgs e)
		{
			var added = e.AddedItems.Cast<PacketVis> ();
			var removed = e.RemovedItems.Cast<PacketVis> ();

			foreach (var pv in added) {
				pv.IsTracked = true;
			}
			foreach (var pv in removed) {
				pv.IsTracked = false;
			}

			var displayPv = lstPackets.SelectedItem as PacketVis;
			if (displayPv != null) {
				txtPacketDetails.Text = displayPv.Pack.Description ();
			}
		}
	}
}
