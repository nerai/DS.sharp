using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.Environment
{
	public abstract class DsMsg
	{
		/* todo?
		private static int _Count = 0;
		public readonly int HyperMessageId = Interlocked.Increment (ref _Count);
		*/

		public abstract IEnumerable<Connection> ReferencedNodes ();

		public abstract ulong? SearchedNode ();

		public abstract string ToStringShort ();
	}
}
