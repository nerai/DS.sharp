using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS.Environment;

namespace DS.Structures
{
	public class DsGroup
	{
		protected static readonly Random _R = new Random ();

		private readonly ManualResetEvent _CreateRandomMessages = new ManualResetEvent (false);

		private readonly List<DsBase> _DS = new List<DsBase> ();

		public DsGroup ()
		{
			new Thread (RandomMessagingThread) {
				IsBackground = true,
				Name = "Create random messages",
			}.Start ();
		}

		public void Register (DsBase ds)
		{
			_DS.Add (ds);
		}

		public void CreateNodes (int n, bool connected)
		{
			for (int i = 0; i < n; i++) {
				var ds = _DS[_R.Next (_DS.Count)];
				ds.CreateNodes (1, connected);
			}
		}

		public bool CreateRandomMessagesEnabled {
			set {
				if (value) {
					_CreateRandomMessages.Set ();
				}
				else {
					_CreateRandomMessages.Reset ();
				}
			}
		}

		private void RandomMessagingThread ()
		{
			while (true) {
				Thread.Sleep (10000);
				_CreateRandomMessages.WaitOne ();
				CreateMessages (1);
			}
		}

		public void CreateMessages (int howMany)
		{
			var nodes = DSEnvironment.Instance.Subjects ().ToList ();
			if (nodes.Count == 0) {
				return;
			}

			for (int i = 0; i < howMany; i++) {
				var src = nodes[_R.Next (nodes.Count)];
				var dst = nodes[_R.Next (nodes.Count)];
				var msg = new DsMsgSearch (dst.Coordinate);
				DSEnvironment.Instance.CreatePacket (
					src,
					src,
					msg);
			}
		}
	}
}
