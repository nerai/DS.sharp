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
using DS.Structures;
using DS.Structures.LinkedListDS;
using DS.Structures.SkipListPlusDS;

namespace DS.Vis
{
	public partial class MainWindow : Window
	{
		private readonly DSEnvironment Env = DSEnvironment.Instance;

		private readonly DsGroup _DSG = new DsGroup ();
		private readonly LinkedList _Linkedlist;
		private readonly SkipList _Skiplist;

		public MainWindow ()
		{
			InitializeComponent ();

			Env.RoundChanged += RoundChanged;

			drawer.NodeCountChanged += Drawer_NodeCountChanged;
			drawer.PacketCountChanged += Drawer_PacketCountChanged;

			_Skiplist = new SkipList ();
			_Linkedlist = new LinkedList ();
			_DSG.Register (_Skiplist);
			_DSG.Register (_Linkedlist);
		}

		private void Drawer_NodeCountChanged (int n)
		{
			Dispatcher.BeginInvoke ((Action) (() => {
				lblTotalNodes.Content = n;
			}));
		}

		private void Drawer_PacketCountChanged (int n)
		{
			Dispatcher.BeginInvoke ((Action) (() => {
				lblTotalPackets.Content = n;
			}));
		}

		private void RoundChanged ()
		{
			Dispatcher.BeginInvoke ((Action) (() => {
				lblCurrentRound.Content = Env.Round;
			}));
		}

		private void cmdStepMsg_Click (object sender, RoutedEventArgs e)
		{
			Env.NextMsg ();
		}

		private void cmdStepMsg100_Click (object sender, RoutedEventArgs e)
		{
			for (int i = 0; i < 100; i++) {
				Env.NextMsg ();
			}
		}

		private void cmdStepTick_Click (object sender, RoutedEventArgs e)
		{
			Env.NextTimeout ();
		}

		private void chkAdvanceMsgs_Checked (object sender, RoutedEventArgs e)
		{
			var isChecked = chkAdvanceMsgs.IsChecked == true;
			Env.EnableAutonomousMsgs (isChecked);
		}

		private void chkAdvanceTicks_Checked (object sender, RoutedEventArgs e)
		{
			var isChecked = chkAdvanceTicks.IsChecked == true;
			Env.EnableAutonomousTicks (isChecked);
		}

		private void chkCreateRandomMessages_Checked (object sender, RoutedEventArgs e)
		{
			var isChecked = chkCreateRandomMessages.IsChecked == true;
			_DSG.CreateRandomMessagesEnabled = isChecked;
		}

		private void chkFreeze_Checked (object sender, RoutedEventArgs e)
		{
			var isChecked = chkFreeze.IsChecked == true;
			drawer.FreezeLayout = isChecked;
		}

		private void chkLayoutCircle_Checked (object sender, RoutedEventArgs e)
		{
			var isChecked = chkLayoutCircle.IsChecked == true;
			drawer.CircleLayout = isChecked;
		}

		private void cmdCreateMessages1_Click (object sender, RoutedEventArgs e)
		{
			_DSG.CreateMessages (1);
		}

		private void cmdCreateMessages10_Click (object sender, RoutedEventArgs e)
		{
			_DSG.CreateMessages (10);
		}

		private void cmdCreateMessages100_Click (object sender, RoutedEventArgs e)
		{
			_DSG.CreateMessages (100);
		}

		private void cmdCreateNode_SkipPlusC_Click (object sender, RoutedEventArgs e)
		{
			_Skiplist.CreateNodes (1, true);
		}

		private void cmdCreateNode_LinkedListC_Click (object sender, RoutedEventArgs e)
		{
			_Linkedlist.CreateNodes (1, true);
		}

		private void cmdCreateNode_SkipPlusNC_Click (object sender, RoutedEventArgs e)
		{
			_Skiplist.CreateNodes (1, false);
		}

		private void cmdCreateNode_LinkedListNC_Click (object sender, RoutedEventArgs e)
		{
			_Linkedlist.CreateNodes (1, false);
		}
	}
}
