using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkDrawing
{
	public class VisBase
	{
		public Point Center;

		/// <summary>
		/// Cursor hovering over this item
		/// </summary>
		public bool Highlight { get; internal set; }

		/// <summary>
		/// This item was clicked
		/// </summary>
		public bool Selected { get; internal set; }
	}
}
