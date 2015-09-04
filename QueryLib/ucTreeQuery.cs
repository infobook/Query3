using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using CommandAS.Tools;

namespace CommandAS.QueryLib
{
	/// <summary>
	/// 
	/// </summary>
	public class ucTreeQuery : TreeView
	{
		private System.ComponentModel.IContainer components = null;
	
		public class CompareQueryName : IComparer
		{
			int IComparer.Compare(Object a1, Object a2)  
			{
				int ret = 0;

				Query q1 = a1 as Query;
				Query q2 = a2 as Query;
				if (q1 != null && q2 != null)
				{
					ret = q1.Name.CompareTo(q2.Name);
				}
				else
					throw new ArgumentException("Objects is not a Query");    

				return ret;
			}
		}


		//private Session							_ses;
		private Query								_prevSelectedQuery;
		private ToolTip							_tt;
		private Session							_currSes;

		protected virtual Session		mCurrSession
		{
			get { return _currSes;  }
			set { _currSes = value; }
		}

		public bool									pViewHidden;

		public ucTreeQuery()
		{
			//_ses = null;
			_prevSelectedQuery = null;
			pViewHidden = false;

			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			_tt = new ToolTip();
			// Set up the delays for the ToolTip.
			_tt.AutoPopDelay = 100;
			//_tt.InitialDelay = 1000;
			//_tt.ReshowDelay = 500;
			_tt.AutomaticDelay = 0;
			// Force the ToolTip text to be displayed whether or not the form is active.
			_tt.ShowAlways = true;

			this.MouseMove += new MouseEventHandler(ucTreeQuery_MouseMove);
		}

		public void Load (Performer aPrf)
		{
			if (SelectedNode != null)
				_prevSelectedQuery = SelectedNode.Tag as Query;

			Nodes.Clear();

			mCurrSession = aPrf.pSes;

			if (aPrf == null || aPrf.pQueries.Count == 0)
				return;

			Stack ownStack = new Stack(16);
			string [] ss = null;
			string [] prevSS = new string[0];
			TreeNode node = null;
			//ArrayList qns = new ArrayList(_ses.Queries.Count);
			//foreach (Query q in _ses.Queries)
			//	qns.Add(q.Name);
			//	qns.Sort();
			CompareQueryName cqn = new CompareQueryName();
			aPrf.pQueries.Sort(cqn);
			foreach (Query q in aPrf.pQueries)
			{
				if (q.Hidden && (!pViewHidden))
					continue;

				/// если установлен запрет на отображение, через виртуальную
				/// функцию _IsProhibition
				if (_IsProhibition(q))
					continue;

				ss = q.Name.Split(PathSeparator.ToCharArray());
				int ii = 0;
				for (; ii < prevSS.Length && ii < ss.Length && prevSS[ii].Equals(ss[ii]); ii++)
				{
					continue;
				}

				for(int jj = prevSS.Length-ii; jj>0 && ownStack.Count>0; jj--)
					ownStack.Pop();

				for (; ii < ss.Length; ii++)
				{
					node = new TreeNode(ss[ii]);
					node.Tag = q;
					if (ownStack.Count > 0)
					{
						((TreeNode)ownStack.Peek()).Nodes.Add(node);
						if (q.Hidden)
							node.ForeColor = SystemColors.GrayText;
					}
					else
					{
						Nodes.Add(node);
					}

					ownStack.Push(node);
				}
				prevSS = ss;
			}

			if (_prevSelectedQuery != null)
				SelectedNode = FindNodeByQuery(Nodes, _prevSelectedQuery);

			if (pViewHidden)
				ViewCodeText(Nodes);
		}

		/// <summary>
		/// «апрет на отображение (исполнение) запроса.
		/// ћожет быть переопределено в производном классе.
		/// </summary>
		/// <param name="aQ">запрос</param>
		/// <returns>true - отображение запрещено; false - отображение разрешено</returns>
		protected virtual bool _IsProhibition(Query aQ)
		{
			return false;
		}

		private void ViewCodeText(TreeNodeCollection aTNC)
		{
			foreach (TreeNode tn in aTNC)
			{
				if (tn.Nodes.Count > 0)
					ViewCodeText(tn.Nodes);
				else
					tn.Text = "["+((Query)tn.Tag).Code.ToString()+"] "+tn.Text; 
			}
		}

		public TreeNode FindNodeByQuery(TreeNodeCollection aNColl,  Query aQ)
		{
			TreeNode ret = null;
			foreach (TreeNode tn in aNColl)
			{
				ret = FindNodeByQuery(tn.Nodes,  aQ);
				if (ret != null)
					return ret;
				
				if (aQ.Equals(tn.Tag))
					return  tn;
			}
			return ret;
		}


		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{

		}
		#endregion

		private void ucTreeQuery_MouseMove(object sender, MouseEventArgs e)
		{
			TreeNode tn = this.GetNodeAt(e.X, e.Y);
			if (tn != null && tn.Nodes.Count == 0)
			{
				Query q = tn.Tag as Query;
				if (q != null)
				{
					if (!_tt.GetToolTip(this).Equals(q.Note))
					{
						_tt.SetToolTip(this, q.Note);
						_tt.Active = true;
					}
				}
			}
			else
				_tt.RemoveAll();
		}
	}


	/// <summary>
	///  ласс отображени€ иерархии запросов с учетом специфики
	/// определени€ прав доступа к запросам Ѕƒ InfoBook.
	/// </summary>
	public class ucIBTreeQuery : ucTreeQuery
	{

		private WorkDB						_wdb;
		private int								_userCode;
		private int								_dbPlace;

		protected override Session		mCurrSession
		{
			set 
			{
				base.mCurrSession = value;
				
				if (value != null)
				{
          _wdb.LoadRights(value.Code);
				}
			}
		}

		public ucIBTreeQuery(WorkDB aWDB, int aUserCode, int aDBPlace) : base ()
		{
			_wdb = aWDB;
			_userCode = aUserCode;
			_dbPlace = aDBPlace;
		}

		protected override bool _IsProhibition(Query aQ)
		{
      return _wdb.IsProhibition(aQ.Code);
		}
	}
}
