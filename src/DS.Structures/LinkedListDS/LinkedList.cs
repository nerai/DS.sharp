using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS.Environment;

namespace DS.Structures.LinkedListDS
{
	public class LinkedList : DsBase
	{
		public LinkedList ()
		{
		}

		protected override Subject CreateNode (IReadOnlyCollection<Subject> globalNodes)
		{
			var add = new LLNode ((ulong) globalNodes.Count + 1);
			return add;
		}
	}
}
