using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NetworkDrawing
{
	public static class WpfExtensions
	{
		public static double Distance (this Point p0, Point p1, bool manhattan = false)
		{
			var dx = p0.X - p1.X;
			var dy = p0.Y - p1.Y;
			var s = manhattan ? Math.Abs (dx) + Math.Abs (dy) : dx * dx + dy * dy;
			return Math.Sqrt (s);
		}

		public static double Distance (this Vector p0, Vector p1, bool manhattan = false)
		{
			var dx = p0.X - p1.X;
			var dy = p0.Y - p1.Y;
			var s = manhattan ? Math.Abs (dx) + Math.Abs (dy) : dx * dx + dy * dy;
			return Math.Sqrt (s);
		}

		public static Point Average (this IEnumerable<Point> ps)
		{
			var x = ps.Average (p => p.X);
			var y = ps.Average (p => p.Y);
			return new Point (x, y);
		}

		public static Vector Clone (this Vector v)
		{
			return new Vector (v.X, v.Y);
		}

		public static Point Clone (this Point v)
		{
			return new Point (v.X, v.Y);
		}

		public static Vector Min (this IEnumerable<Point> ps)
		{
			return new Vector (ps.Min (p => p.X), ps.Min (p => p.Y));
		}

		public static Vector Max (this IEnumerable<Point> ps)
		{
			return new Vector (ps.Max (p => p.X), ps.Max (p => p.Y));
		}

		public static Point Center (this Rect rect)
		{
			return new Point (
				rect.X + rect.Width / 2,
				rect.Y + rect.Height / 2);
		}
	}
}
