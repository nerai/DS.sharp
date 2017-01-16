using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.Environment;

namespace DS.Structures
{
	public class DsMsgSearch : DsMsg
	{
		public readonly ulong SearchId;

		public DsMsgSearch (ulong searchId)
		{
			SearchId = searchId;
		}

		public override IEnumerable<Connection> ReferencedNodes ()
		{
			yield break;
		}

		public override ulong? SearchedNode ()
		{
			return SearchId;
		}

		public override string ToString ()
		{
			return "Search " + SearchId;
		}

		public override string ToStringShort ()
		{
			return ToString ();
		}
	}
}
