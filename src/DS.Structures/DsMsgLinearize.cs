using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.Environment;

namespace DS.Structures
{
	public class DsMsgLinearize : DsMsg
	{
		public readonly Connection[] Cons;

		public DsMsgLinearize (IEnumerable<Connection> connections)
		{
			if (false
				|| connections == null
				|| !connections.Any ()
				|| connections.Any (con => con == null)
				) {
				throw new ArgumentNullException ();
			}

			Cons = connections.ToArray ();
		}

		public override IEnumerable<Connection> ReferencedNodes ()
		{
			return Cons;
		}

		public override ulong? SearchedNode ()
		{
			return null;
		}

		public override string ToString ()
		{
			return "Lin " + string.Join (", ", (object[]) Cons);
		}

		public override string ToStringShort ()
		{
			return "Lin " + string.Join (", ", Cons.Select (con => con.ToStringShort ()));
		}
	}
}
