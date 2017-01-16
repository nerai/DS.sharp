using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DS.Environment;
using DS_Vis.Drawer;

namespace DS.Vis.Drawer
{
	public partial class ContextMenu_Packet : UserControl
	{
		public DrawerController C = null;

		public PacketVis P = null;

		public ContextMenu_Packet ()
		{
			InitializeComponent ();
		}

		private void cmdRemove_Click (object sender, RoutedEventArgs e)
		{
			C.RemovePacket (P);
			Visibility = Visibility.Hidden;
		}

		private void cmdTrack_Click (object sender, RoutedEventArgs e)
		{
			P.IsTracked = true;
		}

		private void cmdArrive_Click (object sender, RoutedEventArgs e)
		{
			DSEnvironment.Instance.ForcePacketArrival (P.Pack);
		}
	}
}
