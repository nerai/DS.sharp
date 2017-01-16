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
using DS.Structures.LinkedListDS;
using DS_Vis.Drawer;

namespace DS.Vis.Drawer
{
	public partial class ContextMenu_Node : UserControl
	{
		public DrawerController C = null;

		public NodeVis N = null;

		public ContextMenu_Node ()
		{
			InitializeComponent ();
		}

		private void cmdRemove_Click (object sender, RoutedEventArgs e)
		{
			C.RemoveNode (N);
			Visibility = Visibility.Hidden;
		}

		private void cmdTrack_Click (object sender, RoutedEventArgs e)
		{
			/* xxx
			N.IsTracked = true;
			*/
		}

		private void cmdSleep_Click (object sender, RoutedEventArgs e)
		{
			var lln = N.Subj as LLNode;
			lln?.Leave ();
		}
	}
}
