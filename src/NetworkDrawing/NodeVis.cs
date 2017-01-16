using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkDrawing
{
	public class NodeVis : VisBase
	{
		public readonly ISubject Subj;
		public bool IsDragged;

		public NodeVis (ISubject subj)
		{
			Subj = subj;
		}

		public override string ToString ()
		{
			return Subj.ToString ();
		}
	}
}
