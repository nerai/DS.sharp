using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DS.Environment;

namespace DS_Vis.Drawer
{
	public class PacketVis : VisBase
	{
		public Packet Pack;
		public Point Origin;
		public bool IsTracked;

		public override string ToString ()
		{
			return Pack.ToStringShort ();
		}

		internal void ForceArrival ()
		{
			Pack.Arrive ();
		}
	}
}
