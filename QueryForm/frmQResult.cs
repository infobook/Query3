using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
//using System.Diagnostics;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using CommandAS.Tools;
using CommandAS.Tools.Controls;

namespace CommandAS.QueryForm
{
	/// <summary>
	/// Summary description for frmQResult.
	/// </summary>
	public class frmQResult : System.Windows.Forms.Form
	{
		public static bool										pToggleStatus = false;

		private Task													_tsk;
		private DataTable											_dt;
		private QueryIconCollection						_iconColl;

		private System.Windows.Forms.MainMenu _mnu;
		private System.Windows.Forms.ToolBar _tb;
		private System.Windows.Forms.StatusBar _sb;
		private ucMoveNavigator _mn;
		private System.Windows.Forms.ToolBarButton _tbbSaveAs;
		private System.Windows.Forms.Panel _pnl;
		private System.Windows.Forms.Splitter _split;
		private System.Windows.Forms.TextBox _txt;
		private System.Windows.Forms.ToolBarButton _tbbSep1;
		private System.Windows.Forms.ToolBarButton _tbb2Excel;
		private System.Windows.Forms.DataGrid _dgr;
		private System.Windows.Forms.ToolBarButton _tbbSep2;
		private System.Windows.Forms.ToolBarButton _tbbViewStatus;
		private System.Windows.Forms.StatusBarPanel _sbpMain;
		private System.Windows.Forms.StatusBarPanel _sbpTime;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox _cboTabOfSet;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public QueryIconCollection								pIconColl
		{
			set
			{
				_iconColl = value;
				if (value != null)
				{
					_tb.ImageList = value.pImageList;
					_tbbSaveAs.ImageIndex = value.Index(QueryIconCollection.Save);
					_tbb2Excel.ImageIndex = value.Index(QueryIconCollection.Excel);
					StatusToggle();
					int prevTop = _pnl.Top;
					_pnl.Top = _tb.Height + 3;
					_pnl.Height -= (_pnl.Top-prevTop);
				}
			}
		}

		
		
		public frmQResult(Task aTsk)
		{
			_tsk = aTsk;

			pIconColl = null;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			#region MANUAL design
			// _mn
			_mn = new CommandAS.Tools.Controls.ucMoveNavigator();
			_mn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			_mn.Location = new System.Drawing.Point(2, 438);
			_mn.Name = "_mn";
			_mn.pPosition = 1;
			_mn.Size = new System.Drawing.Size(212, 37);
			_mn.TabIndex = 3;
			_mn.MoveNavigator += new EvH_MoveNavigator(DoMoveNavigator);
			#endregion

			Icon = new Icon(GetType(),"Images.Icons.Query.ico");
			Closed += new EventHandler(DoClosed);

			_dgr.ReadOnly = true;
			_dgr.CurrentCellChanged += new EventHandler(DoCurrentCellChanged);
			_dgr.ContextMenu = new ContextMenu();
			_dgr.ContextMenu.MenuItems.Add(new MenuItem("Копировать", new EventHandler(DoCommandCopy),Shortcut.CtrlC));
			_dgr.ContextMenu.MenuItems.Add(new MenuItem("-"));
			_dgr.ContextMenu.MenuItems.Add(new MenuItem("Выделить все", new EventHandler(DoCommandSelectAll),Shortcut.CtrlA));

			foreach (DataTable dTab in _tsk.pResultSet.Tables)
				_cboTabOfSet.Items.Add(dTab.TableName);

			if (_cboTabOfSet.Items.Count > 0)
			{
				_cboTabOfSet.SelectedIndex = 0;
				_tbbViewStatus.Pushed = pToggleStatus;
			}
			else
				_tbbViewStatus.Pushed = false;

			this.Text = _tsk.pCurrentQuery.Name;
			_txt.Text = _tsk.pResultMessage.Replace("\n", Environment.NewLine);
			_txt.Select(_txt.Text.Length,0);
			_sbpTime.Text = new DateTime(_tsk.pExecTime.Ticks).ToString("mm:ss (fff)");

		}


		private void _cboTabOfSet_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			_dt = _tsk.pResultSet.Tables[_cboTabOfSet.Text];
			_dgr.DataSource =_dt;

			AutoResizeDataGridTableStyle dgs = new AutoResizeDataGridTableStyle();
			_dgr.TableStyles.Clear();
			dgs.MappingName = _dt.TableName;
			_dgr.TableStyles.Add(dgs);
			dgs.OnDataGridResize(dgs, new EventArgs());

			_sbpMain.Text = "в таб. "+_dt.Rows.Count+" строк";
		}

		private void DoClosed(object sender, EventArgs e)
		{
			_tsk.BeforeClosed();
		}

		private void DoTBButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch (e.Button.Tag.ToString())
			{
				case "cmdSaveAs":
					SaveAs();
					break;
				case "cmd2Excel":
					ToExcel();
					break;
				case "cmdViewStatus":
					StatusToggle();
					break;
			}
		}

		private void DoMoveNavigator(object sender, EvA_MoveNavigator e)
		{
			_dgr.CurrentRowIndex = _mn.pPosition - 1;

			switch (e.pDirection)
			{
				case eDirection.First:
					_dgr.CurrentRowIndex = 0;
					break;
				case eDirection.Prev:
					if (_dgr.CurrentRowIndex > 0)
						_dgr.CurrentRowIndex--;
					break;
				case eDirection.Next:
					if (_dgr.CurrentRowIndex < _dt.Rows.Count-1)
						_dgr.CurrentRowIndex ++;
					break;
				case eDirection.Last:
					_dgr.CurrentRowIndex = _dt.Rows.Count-1;
					break;
				case eDirection.Position:
					break;
			}
		}

		private void DoCurrentCellChanged(object sender, EventArgs e)
		{
			_mn.pPosition = _dgr.CurrentRowIndex + 1;
		}


		private void DoCommandCopy(object sender, EventArgs e)
		{
			MouseCursor.SetCursorWait();
			try
			{
				Clipboard.SetDataObject(TableToString(_dt));
				//Clipboard.SetDataObject(_dgr);
			}
			finally
			{
				MouseCursor.SetCursorDefault();
			}
		}
		
		private void DoCommandSelectAll(object sender, EventArgs e)
		{
			for(int ii = 0; ii < _dt.Rows.Count; ii++)
				_dgr.Select(ii);
		}

		private string TableToString(DataTable dt)
		{
			string strData = this.Text + "\r\n";
			_sbpMain.Text = "Подождите пожалуйста, выполняется копирование данных в буффер обмена ...";
			string sep = string.Empty;
			if (dt.Rows.Count > 0)
			{
				foreach (DataColumn c in dt.Columns)
				{
					if(c.DataType != typeof(System.Guid) &&
						c.DataType != typeof(System.Byte[]))
					{
						strData += sep + c.ColumnName;
						sep = "\t";
					}
				}
				strData += "\r\n";
				int ii = 0;
				foreach(DataRow r in dt.Rows)
				{
					if (_dgr.IsSelected(ii++))
					{
						sep = string.Empty;
						foreach(DataColumn c in dt.Columns)
						{
							if(c.DataType != typeof(System.Guid) &&	c.DataType != typeof(System.Byte[]))
							{
								if(!Convert.IsDBNull(r[c.ColumnName]))
									strData += sep + r[c.ColumnName].ToString().Replace("\r\n"," ");
								else
									strData += sep + string.Empty;
								sep = "\t";
							}
						}
						strData += "\r\n";
						//Application.DoEvents();
						//MouseCursor.SetCursorWait();
					}
				}
			}
			else
				strData += "\r\n---> Table was empty!";

			_sbpMain.Text = "Копирование данных в буффер обмена завершен.";
			return strData;
		}

		private void SaveAs ()
		{
			//CASTools.MessageSorryDoItLater();
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Таблица XML|*.xml";
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				_tsk.SaveTable2XMLFile(dlg.FileName);
			}
		}
		
		private void ToExcel()
		{
			_tsk.ToExcel();
			#region OLD ...
			/*
			MouseCursor.SetCursorWait();
			string ts = null;
			try
			{
				ts = TableToString(_dt);
				if (_qntSR == 0)
				{
					DoCommandSelectAll(null, null);
					ts = TableToString(_dt);
				}
			}
			finally
			{
				MouseCursor.SetCursorDefault();
			}
			
			if (ts != null)
			{
				qExcel ex = new qExcel();
				ex.ExcelVisible = true;
				ex.StartExcel();
				ex.SetFromClipboard(Text, ts);
			}
			*/
			#endregion
		}


		private void StatusToggle()
		{
			if (_tbbViewStatus.Pushed)
				_tbbViewStatus.ImageIndex = _iconColl.Index(QueryIconCollection.ViewStatus);
			else
				_tbbViewStatus.ImageIndex = _iconColl.Index(QueryIconCollection.HideStatus);

			_txt.Visible		= !_tbbViewStatus.Pushed;
			_split.Visible	= _txt.Visible;

			pToggleStatus	= _tbbViewStatus.Pushed;
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this._mnu = new System.Windows.Forms.MainMenu();
			this._tb = new System.Windows.Forms.ToolBar();
			this._tbbSaveAs = new System.Windows.Forms.ToolBarButton();
			this._tbbSep1 = new System.Windows.Forms.ToolBarButton();
			this._tbb2Excel = new System.Windows.Forms.ToolBarButton();
			this._tbbSep2 = new System.Windows.Forms.ToolBarButton();
			this._tbbViewStatus = new System.Windows.Forms.ToolBarButton();
			this._sb = new System.Windows.Forms.StatusBar();
			this._sbpMain = new System.Windows.Forms.StatusBarPanel();
			this._sbpTime = new System.Windows.Forms.StatusBarPanel();
			this._pnl = new System.Windows.Forms.Panel();
			this._dgr = new System.Windows.Forms.DataGrid();
			this._split = new System.Windows.Forms.Splitter();
			this._txt = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this._cboTabOfSet = new System.Windows.Forms.ComboBox();
			((System.ComponentModel.ISupportInitialize)(this._sbpMain)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._sbpTime)).BeginInit();
			this._pnl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._dgr)).BeginInit();
			this.SuspendLayout();
			// 
			// _tb
			// 
			this._tb.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																																					 this._tbbSaveAs,
																																					 this._tbbSep1,
																																					 this._tbb2Excel,
																																					 this._tbbSep2,
																																					 this._tbbViewStatus});
			this._tb.Dock = System.Windows.Forms.DockStyle.None;
			this._tb.DropDownArrows = true;
			this._tb.Location = new System.Drawing.Point(0, 0);
			this._tb.Name = "_tb";
			this._tb.ShowToolTips = true;
			this._tb.Size = new System.Drawing.Size(328, 28);
			this._tb.TabIndex = 0;
			this._tb.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.DoTBButtonClick);
			// 
			// _tbbSaveAs
			// 
			this._tbbSaveAs.Tag = "cmdSaveAs";
			this._tbbSaveAs.ToolTipText = "Сорханить как ...";
			// 
			// _tbbSep1
			// 
			this._tbbSep1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// _tbb2Excel
			// 
			this._tbb2Excel.Tag = "cmd2Excel";
			this._tbb2Excel.ToolTipText = "Перенести в Excel";
			// 
			// _tbbSep2
			// 
			this._tbbSep2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// _tbbViewStatus
			// 
			this._tbbViewStatus.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
			this._tbbViewStatus.Tag = "cmdViewStatus";
			this._tbbViewStatus.ToolTipText = "Отображать/скрыть окно статуса выполнения";
			// 
			// _sb
			// 
			this._sb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this._sb.Dock = System.Windows.Forms.DockStyle.None;
			this._sb.Location = new System.Drawing.Point(223, 443);
			this._sb.Name = "_sb";
			this._sb.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																																					 this._sbpMain,
																																					 this._sbpTime});
			this._sb.ShowPanels = true;
			this._sb.Size = new System.Drawing.Size(552, 28);
			this._sb.TabIndex = 1;
			// 
			// _sbpMain
			// 
			this._sbpMain.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this._sbpMain.Width = 436;
			// 
			// _sbpTime
			// 
			this._sbpTime.MinWidth = 100;
			this._sbpTime.Text = "00:00.000";
			// 
			// _pnl
			// 
			this._pnl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this._pnl.Controls.Add(this._dgr);
			this._pnl.Controls.Add(this._split);
			this._pnl.Controls.Add(this._txt);
			this._pnl.Location = new System.Drawing.Point(0, 37);
			this._pnl.Name = "_pnl";
			this._pnl.Size = new System.Drawing.Size(778, 397);
			this._pnl.TabIndex = 4;
			// 
			// _dgr
			// 
			this._dgr.CaptionText = "Результат";
			this._dgr.DataMember = "";
			this._dgr.Dock = System.Windows.Forms.DockStyle.Fill;
			this._dgr.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this._dgr.Location = new System.Drawing.Point(0, 0);
			this._dgr.Name = "_dgr";
			this._dgr.Size = new System.Drawing.Size(778, 277);
			this._dgr.TabIndex = 6;
			// 
			// _split
			// 
			this._split.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this._split.Dock = System.Windows.Forms.DockStyle.Bottom;
			this._split.Location = new System.Drawing.Point(0, 277);
			this._split.Name = "_split";
			this._split.Size = new System.Drawing.Size(778, 8);
			this._split.TabIndex = 4;
			this._split.TabStop = false;
			// 
			// _txt
			// 
			this._txt.Dock = System.Windows.Forms.DockStyle.Bottom;
			this._txt.Location = new System.Drawing.Point(0, 285);
			this._txt.Multiline = true;
			this._txt.Name = "_txt";
			this._txt.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this._txt.Size = new System.Drawing.Size(778, 112);
			this._txt.TabIndex = 5;
			this._txt.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(336, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(152, 23);
			this.label1.TabIndex = 5;
			this.label1.Text = "Таблица из набора";
			// 
			// _cboTabOfSet
			// 
			this._cboTabOfSet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._cboTabOfSet.Location = new System.Drawing.Point(488, 8);
			this._cboTabOfSet.Name = "_cboTabOfSet";
			this._cboTabOfSet.Size = new System.Drawing.Size(280, 24);
			this._cboTabOfSet.TabIndex = 6;
			this._cboTabOfSet.SelectedIndexChanged += new System.EventHandler(this._cboTabOfSet_SelectedIndexChanged);
			// 
			// frmQResult
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(777, 473);
			this.Controls.Add(this._cboTabOfSet);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._pnl);
			this.Controls.Add(this._sb);
			this.Controls.Add(this._tb);
			this.Menu = this._mnu;
			this.Name = "frmQResult";
			((System.ComponentModel.ISupportInitialize)(this._sbpMain)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._sbpTime)).EndInit();
			this._pnl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._dgr)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

	}
}
