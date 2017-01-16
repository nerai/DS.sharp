using System.Collections.Generic;

namespace NetworkDrawing
{
	public interface ISubject
	{
		long Coordinate { get; }

		IEnumerable<ISubject> ReportKnownNodes ();

		Dictionary<ISubject, string> ReportKnownConnections ();
	}
}
