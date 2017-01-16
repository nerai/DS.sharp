using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NetworkDrawing
{
	internal static class DrawerBuffer
	{
		public static readonly Pen PenNodeConnection;
		public static readonly Pen PenNodeConnectionHighlighted;

		public static readonly Pen PenNodeFrame1;
		public static readonly Pen PenNodeFrame2;

		public static readonly Typeface TypeVerdana;

		private static readonly Pen PenNodeBorderHighlighted;
		private static readonly Pen PenNodeBorderKnownKnows;
		private static readonly Pen PenNodeBorderKnownUnknows;
		private static readonly Pen PenNodeBorderUnknownKnows;

		public static readonly Pen PenPacketBody;
		public static readonly Pen PenPacketBodyHighlight;

		public static readonly Pen PenPacketFromLink;
		public static readonly Pen PenPacketToLink;
		public static readonly Pen PenPacketSearchLink;
		public static readonly Pen PenPacketRefLink;

		static DrawerBuffer ()
		{
			PenNodeConnection = new Pen (Brushes.Blue, 1.5);
			PenNodeConnectionHighlighted = new Pen (Brushes.Orange, 2.0);
			PenNodeConnection.Freeze ();
			PenNodeConnectionHighlighted.Freeze ();

			PenNodeFrame1 = new Pen (Brushes.White, 4.0);
			PenNodeFrame2 = new Pen (Brushes.Black, 2.0);
			PenNodeFrame1.Freeze ();
			PenNodeFrame2.Freeze ();

			TypeVerdana = new Typeface ("Verdana");

			PenNodeBorderHighlighted = new Pen (Brushes.Orange, 15.0);
			PenNodeBorderKnownKnows = new Pen (Brushes.Green, 12.0);
			PenNodeBorderKnownUnknows = new Pen (Brushes.LightGreen, 12.0);
			PenNodeBorderUnknownKnows = new Pen (Brushes.LightGray, 12.0);
			PenNodeBorderHighlighted.Freeze ();
			PenNodeBorderKnownKnows.Freeze ();
			PenNodeBorderKnownUnknows.Freeze ();
			PenNodeBorderUnknownKnows.Freeze ();

			PenPacketBody = new Pen (Brushes.White, 1.0);
			PenPacketBody.Freeze ();
			PenPacketBodyHighlight = new Pen (Brushes.Gray, 2.0);
			PenPacketBodyHighlight.Freeze ();

			var brushPacketFromLink = new SolidColorBrush (Colors.Gray) { Opacity = 0.3 };
			var brushPacketToLink = new SolidColorBrush (Colors.Green) { Opacity = 0.6 };
			var brushPacketSearchLink = new SolidColorBrush (Colors.Red) { Opacity = 0.10 };
			var brushPacketRefLink = new SolidColorBrush (Colors.Blue) { Opacity = 0.05 };
			brushPacketFromLink.Freeze ();
			brushPacketToLink.Freeze ();
			brushPacketSearchLink.Freeze ();
			brushPacketRefLink.Freeze ();

			PenPacketFromLink = new Pen (brushPacketFromLink, 1.0);
			PenPacketToLink = new Pen (brushPacketToLink, 1.5);
			PenPacketSearchLink = new Pen (brushPacketSearchLink, 6.0);
			PenPacketRefLink = new Pen (brushPacketRefLink, 5.0);
			PenPacketFromLink.Freeze ();
			PenPacketToLink.Freeze ();
			PenPacketSearchLink.Freeze ();
			PenPacketRefLink.Freeze ();
		}

		public static Pen GetPenForNodeBorder (NodeVis node, NodeVis highlightedNode)
		{
			if (node.Highlight) {
				return PenNodeBorderHighlighted;
			}

			if (highlightedNode != null) {
				// Is there a connection from the highlighted node to this node?
				var highlightKnownsThis = highlightedNode
					.Subj
					.ReportKnownNodes ()
					.Contains (node.Subj);
				// Is there a connection from this node to the highlighted node?
				var thisKnownsHighlight = node
					.Subj
					.ReportKnownNodes ()
					.Contains (highlightedNode.Subj);

				if (highlightKnownsThis && thisKnownsHighlight) {
					return PenNodeBorderKnownKnows;
				}
				if (highlightKnownsThis && !thisKnownsHighlight) {
					return PenNodeBorderKnownUnknows;
				}
				if (!highlightKnownsThis && thisKnownsHighlight) {
					return PenNodeBorderUnknownKnows;
				}
			}

			return null;
		}
	}
}
