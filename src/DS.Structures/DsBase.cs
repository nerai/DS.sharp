using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS.Environment;

namespace DS.Structures
{
	public abstract class DsBase
	{
		protected static readonly Random _R = new Random ();

		public DsBase ()
		{
		}

		protected abstract Subject CreateNode (IReadOnlyCollection<Subject> globalNodes);

		public void CreateNodes (int n, bool connected)
		{
			for (int i = 0; i < n; i++) {
				AddNode (connected);
			}
		}

		private void AddNode (bool connected)
		{
			var existingNodes = DSEnvironment.Instance.Subjects ().ToList ();
			var add = CreateNode (existingNodes);

			if (existingNodes.Count == 0) {
				return;
			}

			if (connected) {
				var connect = existingNodes[_R.Next (existingNodes.Count)];

				if (_R.NextDouble () < 0.5) {
					var tmp = connect;
					connect = add;
					add = tmp;
				}

				DSEnvironment.Instance.CreatePacket (
					connect,
					add,
					new DsMsgLinearize (new[] { connect }));
			}
		}
	}
}
