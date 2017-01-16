using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.Environment;

namespace DS.Structures
{
	public class DsMsgReverse : DsMsg
	{
		public readonly Connection Target;

		public DsMsgReverse (Connection target)
		{
			Target = target;
		}

		public override IEnumerable<Connection> ReferencedNodes ()
		{
			yield return Target;
		}

		public override string ToString ()
		{
			return "Reverse " + Target;
		}

		public override string ToStringShort ()
		{
			return ToString ();
		}

		public override ulong? SearchedNode ()
		{
			return null;
		}
	}
}
