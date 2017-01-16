using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.Environment;

namespace DS.Structures
{
	public abstract class NodeBase : Subject
	{
		protected void SendLinearizations (List<Tuple<Connection, Connection>> work)
		{
			if (work == null) {
				throw new ArgumentNullException ();
			}
			if (!work.Any ()) {
				return;
			}
			if (work.Any (tup => tup.Item2 == null)) {
				throw new ArgumentException ();
			}

			var workPerTarget = work
				.GroupBy (tup => tup.Item1)
				.ToDictionary (
					group => group.Key,
					group => group.Select (tup => tup.Item2).Distinct ().ToArray ());
			foreach (var pair in workPerTarget) {
				DSEnvironment.Instance.CreatePacket (
					this,
					pair.Key,
					new DsMsgLinearize (pair.Value));
			}
		}
	}
}
