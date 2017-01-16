using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.Environment;
using DS.Structures.HypercubeDS_old;

namespace DS.Structures
{
	class Program
	{
		static void Main (string[] args)
		{
			DSEnvironment.Instance.EnableAutonomousMsgs (true);
			DSEnvironment.Instance.EnableAutonomousTicks (true);

			/*
			var hc = new Hypercube (4);
			hc.CreateRandomMessagesEnabled = true;
			*/

			Console.ReadKey (true);
		}
	}
}
