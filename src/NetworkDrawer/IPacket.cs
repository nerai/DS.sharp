using System.Collections.Generic;
using System.Windows;

namespace NetworkDrawing
{
	public interface IPacket
	{
		ISubject From { get; }
		double Progress { get; }
		PacketState State { get; }
		ISubject To { get; }

		IEnumerable<ISubject> ReferencedNodes ();

		string Description ();
	}
}
