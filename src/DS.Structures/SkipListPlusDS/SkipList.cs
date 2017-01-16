using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS.Environment;

namespace DS.Structures.SkipListPlusDS
{
	public class SkipList : DsBase
	{
		public SkipList ()
		{
		}

		protected override Subject CreateNode (IReadOnlyCollection<Subject> globalNodes)
		{
			var add = new SkipNode ((ulong) globalNodes.Count + 1);
			return add;
		}
	}
}
