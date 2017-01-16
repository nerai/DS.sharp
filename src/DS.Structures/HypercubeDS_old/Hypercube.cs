using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DS.Environment;

namespace DS.Structures.HypercubeDS_old
{
	public class Hypercube
	{
		private static readonly Random _R = new Random ();

		private readonly ManualResetEvent CreateRandomMessages = new ManualResetEvent (false);
		private readonly List<HypercubeNode> _Nodes;

		public Hypercube (int dim)
		{
			var n = 1 << dim;
			_Nodes = new List<HypercubeNode> (n);
			for (int i = 0; i < n; i++) {
				var node = new HypercubeNode ((uint) dim, (ulong) i);
				_Nodes.Add (node);
			}

			var env = DSEnvironment.Instance;
			for (int i = 0; i < n; i++) {
				var node = _Nodes[i];
				for (int flip = 1; flip <= dim; flip++) {
					var other = i ^ (1 << (flip - 1));
					env.CreatePacket (
						null,
						node,
						new DsMsgLinearize (new[] { _Nodes[other] }));
				}
			}

			foreach (var node in _Nodes) {
				node.Start ();
			}

			new Thread (RandomMessagingThread) {
				IsBackground = true,
				Name = "Hypercube message source",
			}.Start ();
		}

		public bool CreateRandomMessagesEnabled {
			set {
				if (value) {
					CreateRandomMessages.Set ();
				}
				else {
					CreateRandomMessages.Reset ();
				}
			}
		}

		private void RandomMessagingThread ()
		{
			while (true) {
				Thread.Sleep (10000);
				CreateRandomMessages.WaitOne ();
				CreateMessages (1);
			}
		}

		public void CreateMessages (int howMany)
		{
			for (int i = 0; i < howMany; i++) {
				var nodes = _Nodes.Where (n => n.IsStarted).ToList ();
				if (nodes.Count == 0) {
					continue;
				}

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
