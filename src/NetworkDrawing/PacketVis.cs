using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkDrawing
{
	public class PacketVis : VisBase
	{
		public readonly IPacket Pack;
		public Point Origin; // todo: readonly?
		public bool IsTracked;

		public PacketVis (IPacket pack)
		{
			Pack = pack;
		}

		public override string ToString ()
		{
			return Pack.ToString ();
		}
	}
}
