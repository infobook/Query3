using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Win32;
using CommandAS.Tools;
using CommandAS.Tools.Security;
using CommandAS.QueryLib;

namespace CommandAS.QueryForm
{
  /// <summary>
  /// Summary description for Form1.
  /// </summary>
  public class frmQuery : System.Windows.Forms.Form
  {
    private Task _task;
    private QueryIconCollection _iconColl;
    private bool _lockModified;
    private ArrayList _ofArr;

    private MenuItem _miExecute;
    private MenuItem _miEditQuery;
    private MenuItem _miDeleteQuery;
    private MenuItem _miPropertyQuery;
    private Image _img;
    private ContextMenu _lcmTV;

    private bool _modified
    {
      get { return _mnuSaveSession.Enabled; }
      set
      {
        if (_lockModified)
          return;
      }
    }

    private Font _sesFont;

    private int _curSplitQPos;
    private Process _currProc;

    private System.Windows.Forms.Splitter _splitQ;
    private ucTreeQuery _tvQueries;
    private ucParameters _param;
    private ucParamDef _commParamDef;


    private System.Windows.Forms.MainMenu _mnu;
    private System.Windows.Forms.ToolBar _tb;
    private System.Windows.Forms.StatusBar _sb;
    private System.Windows.Forms.TabControl _tc;
    private System.Windows.Forms.TabPage _tcpQuery;
    private System.Windows.Forms.TabPage _tcpParam;
    private System.Windows.Forms.ToolBarButton _tbbSep1;
    private System.Windows.Forms.ToolBarButton _tbbSep2;
    private System.Windows.Forms.ToolBarButton _tbbSaveSession;
    private System.Windows.Forms.ToolBarButton _tbbLoadSession;
    private System.Windows.Forms.MenuItem menuItem9;
    private System.Windows.Forms.MenuItem _mnuExit;
    private System.Windows.Forms.MenuItem _mnuExec;
    private System.Windows.Forms.MenuItem _mnuAbout;
    private System.Windows.Forms.MenuItem _mnuSession;
    private System.Windows.Forms.MenuItem _mnuHelp;
    private System.Windows.Forms.ToolBarButton _tbbSep3;
    private System.Windows.Forms.ToolBarButton _tbbAbout;
    private System.Windows.Forms.MenuItem _mnuSaveSession;
    private System.Windows.Forms.MenuItem _mnuLoadSession;
    private System.Windows.Forms.MenuItem _mnuNewQuery;
    private System.Windows.Forms.MenuItem _mnuDeleteQuery;
    private System.Windows.Forms.ToolBarButton _tbbNewQuery;
    private System.Windows.Forms.ToolBarButton _tbbDelQuery;
    private System.Windows.Forms.MenuItem _mnuNewSession;
    private System.Windows.Forms.MenuItem _mnuSaveSessionAs;
    private System.Windows.Forms.Label _lblConnection;
    private System.Windows.Forms.TextBox _txtConnection;
    private System.Windows.Forms.MenuItem _mnuEdit;
    private System.Windows.Forms.MenuItem _mnuProperty;
    private System.Windows.Forms.MenuItem _mnuEditSQL;
    private System.Windows.Forms.MenuItem _mnuEditParam;
    private System.Windows.Forms.MenuItem _mnuEditXSLT;
    private System.Windows.Forms.ToolBarButton _tbbEditSQL;
    private System.Windows.Forms.ToolBarButton _tbbSep4;
    private System.Windows.Forms.ToolBarButton _tbbEditParam;
    private System.Windows.Forms.ToolBarButton _tbbEditXSLT;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox _txtParamBegDelim;
    private System.Windows.Forms.TextBox _txtParamEndDelim;
    private System.Windows.Forms.MenuItem _mnuSep1;
    private System.Windows.Forms.MenuItem _mnuSep2;
    private System.Windows.Forms.GroupBox _grpParamDelim;
    private System.Windows.Forms.MenuItem _mnuExport;
    private System.Windows.Forms.MenuItem _mnuImport;
    private System.Windows.Forms.MenuItem menuItem3;
    private System.Windows.Forms.MenuItem _mnuExecRTab;
    private System.Windows.Forms.MenuItem _mnuExecRExcel;
    private System.Windows.Forms.MenuItem _mnuExecRHTML;
    private System.Windows.Forms.ToolBarButton _tbbExecRTab;
    private System.Windows.Forms.ToolBarButton _tbbExecRExcel;
    private System.Windows.Forms.ToolBarButton _tbbExecRHTML;
    private IContainer components;
    private System.Windows.Forms.TabPage _tcpXSLTInc;
    private System.Windows.Forms.TabPage _tcpUser;
    private System.Windows.Forms.CheckBox _chkDefaultSession;
    private System.Windows.Forms.Button _cmdBrowseDir;
    private System.Windows.Forms.TextBox _txtTempPath;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button _cmdBrowse;
    private System.Windows.Forms.TextBox _txtTextEditor;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button _cmdFont;
    private System.Windows.Forms.TextBox _txtFont;
    private System.Windows.Forms.Label _lblFont;
    private System.Windows.Forms.Button _cmdImagePath;
    private System.Windows.Forms.TextBox _txtImagePath;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.TextBox _txtImageName;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.TextBox _txtCode;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.TextBox _txtTitle;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.TextBox _txtNote;
    private System.Windows.Forms.Button _cmdPassword;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.Panel _pnlImg;
    private System.Windows.Forms.MenuItem _mnuEditSession;
    private System.Windows.Forms.MenuItem _mnuSep0;
    private System.Windows.Forms.MenuItem _mnuHelpTopic;

    public bool pDesignerMode
    {
      get { return _mnuEditSession.Checked; }
      set
      {
        if (_task.pHash.Length > 0 && value)
        {
          dlgPassword dlg = new dlgPassword();
          if (dlg.ShowDialog() == DialogResult.OK)
          {
            if (!CasHash.HashMD5hex(dlg.pTxtPassword.Text).Equals(_task.pHash))
            {
              MessageBox.Show("Пароль неверный !!!");
              value = false;
            }
          }
          else
            value = false;
        }

        _mnuEditSession.Checked = value;

        _mnuNewSession.Visible = value;
        _mnuSaveSession.Visible = value;
        _tbbSaveSession.Visible = value;
        _mnuSaveSessionAs.Visible = value;

        if (value)
          _tvQueries.ContextMenu = _lcmTV;
        else
          _tvQueries.ContextMenu = null;

        _mnuNewQuery.Visible = value;
        _tbbNewQuery.Visible = value;
        _mnuEdit.Visible = value;
        _tbbEditSQL.Visible = value;
        _tbbEditParam.Visible = value;
        _tbbEditXSLT.Visible = value;
        _mnuDeleteQuery.Visible = value;
        _tbbDelQuery.Visible = value;
        _mnuProperty.Visible = value;

        _mnuSep2.Visible = value;

        //foreach (Control ctr in _tcpParam.Controls)
        //	ctr.Visible = value;
        //foreach (Control ctr in _tcpXSLTInc.Controls)
        //	ctr.Visible = value;
        if (!value)
        {
          _tc.TabPages.Remove(_tcpParam);
          _tc.TabPages.Remove(_tcpXSLTInc);
        }
        else if (!_tvQueries.pViewHidden)
        {
          _tc.TabPages.Add(_tcpParam);
          _tc.TabPages.Add(_tcpXSLTInc);
        }

        _tvQueries.pViewHidden = value;
        _tvQueries.Load(_task);
      }
    }


    public string pLoadDefaultSession;

    public event EventHandler LoadSession;
    public event EventHandler About;

    public TextBox pTxtConnection
    {
      get { return _txtConnection; }
    }

    public Task pTask
    {
      get { return _task; }
    }

    public frmQuery() : this(new WorkDB(), null) { }
    public frmQuery(WorkDB aDB) : this(aDB, null) { }
    public frmQuery(WorkDB aDB, CommandAS.QueryLib.ucTreeQuery aTVQuery)
    {
      _task = new Task(aDB);
      //aDB.pQP = _task;

      _lockModified = false;
      _sesFont = new Font(this.Font.FontFamily.Name, this.Font.Size);
      _currProc = null;
      _ofArr = new ArrayList(16);
      pLoadDefaultSession = string.Empty;

      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      _tc.TabPages.Remove(_tcpParam);
      _tc.TabPages.Remove(_tcpXSLTInc);

      _txtConnection.AcceptsReturn = true;

      #region MANUAL DESIGN
      this._param = new ucParameters();
      this._splitQ = new System.Windows.Forms.Splitter();
      if (aTVQuery == null)
        this._tvQueries = new CommandAS.QueryLib.ucTreeQuery();
      else
        this._tvQueries = aTVQuery;
      // 
      // _param
      // 
      this._param.Dock = System.Windows.Forms.DockStyle.Fill;
      this._param.Location = new System.Drawing.Point(0, 144);
      this._param.Name = "_param";
      this._param.Size = new System.Drawing.Size(768, 156);
      this._param.TabIndex = 4;
      // 
      // _splitQ
      // 
      this._splitQ.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this._splitQ.Dock = System.Windows.Forms.DockStyle.Top;
      this._splitQ.Location = new System.Drawing.Point(0, 136);
      this._splitQ.Name = "_splitQ";
      this._splitQ.Size = new System.Drawing.Size(768, 8);
      this._splitQ.TabIndex = 3;
      this._splitQ.TabStop = false;
      // 
      // _tvQueries
      // 
      this._tvQueries.Dock = System.Windows.Forms.DockStyle.Top;
      this._tvQueries.ImageIndex = -1;
      this._tvQueries.Location = new System.Drawing.Point(0, 0);
      this._tvQueries.Name = "_tvQueries";
      this._tvQueries.SelectedImageIndex = -1;
      this._tvQueries.Size = new System.Drawing.Size(768, 136);
      this._tvQueries.TabIndex = 0;

      this._tcpQuery.Controls.Add(this._param);
      this._tcpQuery.Controls.Add(this._splitQ);
      this._tcpQuery.Controls.Add(this._tvQueries);
      #endregion

      _commParamDef = new ucParamDef(false);
      _commParamDef.Location = new Point(5, 5);
      _commParamDef.Size = new Size(_tcpXSLTInc.Width - 10, _tcpXSLTInc.Height - 10);
      _commParamDef.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
      _tcpXSLTInc.Controls.Add(_commParamDef);

      NewSession();
      _param.pPerf = _task;
      _commParamDef.SetData(_task.pCommonParamCollection);

      int iconSize = 0;
      try
      {
        iconSize = Convert.ToInt32(Registry.CurrentUser.OpenSubKey(AppConst.REG_APP_PATH).GetValue("IconSize"));
      }
      catch { }
      if (iconSize < 16)
        iconSize = 32;
      _iconColl = new QueryIconCollection(iconSize);

      _tb.ImageList = _iconColl.pImageList;
      _tbbExecRTab.ImageIndex = _iconColl.Index(QueryIconCollection.ExecRTab);
      _tbbExecRExcel.ImageIndex = _iconColl.Index(QueryIconCollection.ExecRExcel);
      _tbbExecRHTML.ImageIndex = _iconColl.Index(QueryIconCollection.ExecRHTML);
      _tbbNewQuery.ImageIndex = _iconColl.Index(QueryIconCollection.New);
      _tbbDelQuery.ImageIndex = _iconColl.Index(QueryIconCollection.Delete);
      _tbbSaveSession.ImageIndex = _iconColl.Index(QueryIconCollection.Save);
      _tbbLoadSession.ImageIndex = _iconColl.Index(QueryIconCollection.DocOpen);
      _tbbEditSQL.ImageIndex = _iconColl.Index(QueryIconCollection.TextSQL);
      _tbbEditParam.ImageIndex = _iconColl.Index(QueryIconCollection.TextParam);
      _tbbEditXSLT.ImageIndex = _iconColl.Index(QueryIconCollection.TextXSLT);
      _tbbAbout.ImageIndex = _iconColl.Index(QueryIconCollection.About);

      _tbbExecRTab.Tag = _mnuExecRTab;
      _tbbExecRExcel.Tag = _mnuExecRExcel;
      _tbbExecRHTML.Tag = _mnuExecRHTML;
      _tbbEditSQL.Tag = _mnuEditSQL;
      _tbbEditParam.Tag = _mnuEditParam;
      _tbbEditXSLT.Tag = _mnuEditXSLT;

      _tvQueries.HideSelection = false;
      _lcmTV = new ContextMenu();
      _miExecute = new MenuItem("Выполнить", new EventHandler(_mnuExecRTab_Click), Shortcut.F5);
      _lcmTV.MenuItems.Add(_miExecute);
      _lcmTV.MenuItems.Add(new MenuItem("-"));
      _lcmTV.MenuItems.Add(new MenuItem("Новый", new EventHandler(DoCommandNewQuery)));
      _miDeleteQuery = new MenuItem("Удалить", new EventHandler(DoCommandDeleteQuery));
      _lcmTV.MenuItems.Add(_miDeleteQuery);
      _miEditQuery = new MenuItem(this._mnuEdit.Text);
      MenuItem miEditSQL = new MenuItem(this._mnuEditSQL.Text, new EventHandler(DoCommandEditQuery));
      MenuItem miEditParam = new MenuItem(this._mnuEditParam.Text, new EventHandler(DoCommandEditQueryParam));
      MenuItem miEditXSLT = new MenuItem(this._mnuEditXSLT.Text, new EventHandler(DoCommandEditQuery));
      _miEditQuery.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { miEditSQL, miEditParam, miEditXSLT });
      _lcmTV.MenuItems.Add(_miEditQuery);
      _lcmTV.MenuItems.Add(new MenuItem("-"));
      _miPropertyQuery = new MenuItem("Свойства", new EventHandler(DoCommandPropertyQuery), Shortcut.F2);
      _lcmTV.MenuItems.Add(_miPropertyQuery);
      _lcmTV.Popup += new EventHandler(DoCommandTVQContextMenuPopup);

      _tvQueries.AfterSelect += new TreeViewEventHandler(DoCommandTVQAfterSelect);
      _tvQueries.DoubleClick += new EventHandler(DoCommandTVQDoubleClick);

      _cmdPassword.Click += new EventHandler(_cmdPassword_Click);

      _tc.SelectedIndexChanged += new EventHandler(_tc_SelectedIndexChanged);

      Load += new EventHandler(DoLoad);
      Closing += new CancelEventHandler(DoClosing);
      Activated += new EventHandler(DoActivated);

      TitleText();
      _txtFont.Text = _sesFont.ToString();
      _modified = false;
      _mnuExec.Enabled = false;
      _tbbExecRTab.Enabled = false;
      _tbbExecRExcel.Enabled = false;
      _tbbExecRHTML.Enabled = false;
      _mnuDeleteQuery.Enabled = false;
      _tbbDelQuery.Enabled = false;

      Icon = new Icon(GetType(), "Images.Icons.Query.ico");

      _img = null;
      _pnlImg.Paint += new PaintEventHandler(_pnlImg_Paint);
    }

    private void TitleText()
    {
      this.Text = _txtTitle.Text + " {" + _task.pFileName + "}";
    }

    private void DoLoad(object sender, EventArgs e)
    {
      LoadParametersFromRegister();
      DoCommandTVQAfterSelect(null, null);
    }

    private void DoClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (pDesignerMode)
      {
        DialogResult dz = MessageBox.Show(
          "Да - Выйти и сохранить все изменения" + Environment.NewLine +
          "Нет - Выйти БЕЗ сохранения" + Environment.NewLine +
          "Отмена - Вернуться в программу",
          "Завершение",
          MessageBoxButtons.YesNoCancel,
          MessageBoxIcon.Question,
          MessageBoxDefaultButton.Button1);

        if (dz == DialogResult.Cancel)
          e.Cancel = true;
        else
        {
          if (dz == DialogResult.Yes)
            DoCommandSaveSession(null, null);
          if (_currProc != null)
            _currProc.Kill();

          DeleteOpeningTemporaryFile();

          SaveParametersToRegister();
        }
      }
      else
      {
        if (MessageBox.Show("Выйти из программы?", "Завершение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
          if (_currProc != null)
            _currProc.Kill();

          DeleteOpeningTemporaryFile();

          SaveParametersToRegister();
          SaveQueriesParametersToRegister();
        }
        else
          e.Cancel = true;
      }

      _task.BeforeClosed();
    }

    private void DoActivated(object sender, EventArgs e)
    {
      //_sb.Text = "Activated - "+DateTime.Now.ToLongTimeString();
      GetContentOpeningTemporaryFile();
    }

    private void DoChangedCommon(object sender, System.EventArgs e)
    {
      _modified = true;
    }

    private void DoShowHiddenCheckChanged(object sender, System.EventArgs e)
    {
      _modified = true;
      ShowQueryTreeView();
    }

    #region DoCommand Menu & ToolBar:

    private void DoCommandNewSession(object sender, System.EventArgs e)
    {
      if (MessageBox.Show("Создать новую сессию?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
        return;

      NewSession();
    }

    private void NewSession()
    {
      _task.New();

      _GetCurrentSessionParam();

      TitleText();

      _chkDefaultSession.Checked = false;
      _sesFont = new Font(this.Font.Name, this.Font.Size, this.Font.Style);
      _sb.Text = _txtTitle.Text;
      _modified = false;

      ShowQueryTreeView();
    }


    private void DoCommandLoadSession(object sender, System.EventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.Filter = "Сессия версия 3 (*" + Performer.CURRENT_SESSION_EXT + ")|*" + Performer.CURRENT_SESSION_EXT + "|Сессия версия 2 (*.sq2)|*.sq2";
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        DoLoadSession(dlg.FileName);
      }
    }

    private void DoCommandSaveSession(object sender, System.EventArgs e)
    {
      if (_task.pFileName.Length == 0)
        DoCommandSaveSessionAs(this, new EventArgs());
      else
      {
        SaveSession(_task.pFileName);
        TitleText();
      }
    }

    private void DoCommandSaveSessionAs(object sender, System.EventArgs e)
    {
      SaveFileDialog dlg = new SaveFileDialog();
      dlg.Filter = "Сессия (*" + Performer.CURRENT_SESSION_EXT + ")|*" + Performer.CURRENT_SESSION_EXT;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        SaveSession(dlg.FileName);
        TitleText();
      }
    }

    private void DoCommandEditSession(object sender, System.EventArgs e)
    {
      pDesignerMode = !pDesignerMode;
    }

    private void DoCommandExit(object sender, System.EventArgs e)
    {
      this.Close();
    }


    private void _mnuExecRTab_Click(object sender, System.EventArgs e)
    {
      ExecuteShowTable();
    }

    private void _mnuExecRExcel_Click(object sender, System.EventArgs e)
    {
      ExecuteShowExcel();
    }

    private void _mnuExecRHTML_Click(object sender, System.EventArgs e)
    {
      ExecuteShowXSLT_Format1();
    }


    private void DoCommandNewQuery(object sender, System.EventArgs e)
    {
      string newNodeName = string.Empty;
      if (_tvQueries.SelectedNode != null)
        newNodeName = GetQueryNameFoldersWS(_tvQueries.SelectedNode.FullPath);
      newNodeName += "Новый запрос ";

      Query q = new Query();

      q.Name = newNodeName + (_task.pCountQuery + 1).ToString();
      q.Text = "SELECT * FROM xxx";

      _task.AddQuery(q);
      _tvQueries.Load(_task);
    }

    private string GetQueryNameFoldersWS(string aFullName)
    {
      string ret = string.Empty;
      string[] ss = aFullName.Split(_tvQueries.PathSeparator.ToCharArray());
      if (ss.Length > 1)
        for (int ii = 0; ii < ss.Length - 1; ii++)
          ret += ss[ii] + _tvQueries.PathSeparator;
      return ret;
    }

    private void DoCommandDeleteQuery(object sender, System.EventArgs e)
    {
      if (_tvQueries.SelectedNode == null)
        return;

      Query q = _tvQueries.SelectedNode.Tag as Query;
      if (q == null)
        return;

      if (MessageBox.Show("Удалить запрос [" + q.Name + "]?", "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
      {
        _task.DeleteQuery(q);
        _tvQueries.Load(_task);
      }

    }

    private void DoCommandPropertyQuery(object sender, System.EventArgs e)
    {
      if (_tvQueries.SelectedNode == null)
        return;

      Query q = _tvQueries.SelectedNode.Tag as Query;
      if (q == null)
        return;

      dlgQProperty dlg = new dlgQProperty(_task);
      dlg.pQuery = q;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        dlg.SetValueTo(q);
        ShowQueryTreeView();
      }

    }

    private void DoCommandEditQueryParam(object sender, System.EventArgs e)
    {
      //Query q = new Query();
      Query q = _tvQueries.SelectedNode.Tag as Query;
      if (q != null)
      {
        dlgParam dlg = new dlgParam(q);
        if (dlg.ShowDialog() == DialogResult.OK)
          _param.ShowParam();//q);
      }
    }

    private void DoCommandEditQuery(object sender, System.EventArgs e)
    {
      if (_tvQueries.SelectedNode == null)
        return;

      Query q = _tvQueries.SelectedNode.Tag as Query;
      if (q == null)
        return;

      eOpeningFileContent ofc = eOpeningFileContent.Undefined;
      MenuItem mi = sender as MenuItem;
      if (mi == null)
        return;
      if (mi.Text.Equals(_mnuEditSQL.Text))
        ofc = eOpeningFileContent.SQL;
      else if (mi.Text.Equals(_mnuEditXSLT.Text))
        ofc = eOpeningFileContent.XSLT;

      ExEditorRun(q, ofc);
    }


    private void ExEditorRun(Query aQuery, eOpeningFileContent aOFContent)
    {
      string fileName = CASTools.PathWithEndSeparator(_txtTempPath.Text)
        + aQuery.Name.Replace(_tvQueries.PathSeparator, "_"); //.Replace(".","_");
      switch (aOFContent)
      {
        case eOpeningFileContent.SQL:
          fileName += ".sql";
          break;
        case eOpeningFileContent.XSLT:
          fileName += ".xslt";
          break;
      }

      StreamWriter fl = new StreamWriter(fileName, false, Encoding.Default);
      switch (aOFContent)
      {
        case eOpeningFileContent.SQL:
          fl.Write(aQuery.Text);
          break;
        case eOpeningFileContent.XSLT:
          fl.Write(aQuery.XSLT);
          break;
      }
      fl.Close();

      ProcessStartInfo psi = new ProcessStartInfo();
      try
      {
        psi.FileName = _txtTextEditor.Text; //@"C:\ASW\NotePadPP\notepad++.exe"; 
        psi.Arguments = "\"" + fileName + "\"";
      }
      catch { }

      if (psi.FileName.Length > 0)
      {
        try
        {
          if (_currProc == null)
          {
            _currProc = Process.Start(psi);
            _currProc.EnableRaisingEvents = true;
            _currProc.Exited += new EventHandler(DoCurrProcExited);
          }
          else
            Process.Start(psi);

          _ofArr.Add(new OpeningFile(fileName, aOFContent, aQuery));
        }
        catch (Exception ex)
        {
          Error.ShowError(ex.Message);
        }
      }

    }

    private void DoCurrProcExited(object sender, EventArgs e)
    {
      _currProc.Exited -= new EventHandler(DoCurrProcExited);
      _currProc = null;

      //MessageBox.Show("DoCurrProcExited");
      GetContentOpeningTemporaryFile();
      DeleteOpeningTemporaryFile();
    }

    private void GetContentOpeningTemporaryFile()
    {
      foreach (OpeningFile ofl in _ofArr)
      {
        try
        {
          StreamReader ff = new StreamReader(ofl.pFileName, Encoding.Default);
          switch (ofl.pOFContent)
          {
            case eOpeningFileContent.SQL:
              ofl.pQuery.Text = ff.ReadToEnd();
              break;
            case eOpeningFileContent.XSLT:
              ofl.pQuery.XSLT = ff.ReadToEnd();
              break;
          }
          ff.Close();
          ofl.pQuery.DateLastModified = DateTime.Today;
        }
        catch { }
      }
    }

    private void DeleteOpeningTemporaryFile()
    {
      foreach (OpeningFile ofl in _ofArr)
      {
        FileInfo ff = new FileInfo(ofl.pFileName);
        ff.Delete();
      }
      _ofArr.Clear();
    }

    private void DoCommandExportQuery(object sender, System.EventArgs e)
    {
      if (_tvQueries.SelectedNode == null)
        return;

      Query q = _tvQueries.SelectedNode.Tag as Query;
      if (q == null)
        return;

      SaveFileDialog dlg = new SaveFileDialog();
      dlg.Filter = "Запрос (*" + Performer.CURRENT_QUERY_EXT + ")|*" + Performer.CURRENT_QUERY_EXT;
      dlg.FileName = Path.GetFileName(q.Name);
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        string ss = q.Name;
        q.Name = "[Imp]" + ss;
        q.SaveAs(dlg.FileName);
        q.Name = ss;
      }
    }

    private void DoCommandImportQuery(object sender, System.EventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.Filter = "Запрос v3 (*" + Performer.CURRENT_QUERY_EXT + ")|*" + Performer.CURRENT_QUERY_EXT
        + "|Запрос v2 (*.q2)|*.q2";
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        Query q = null;
        try
        {
          /// РАСПОЗНАЕМ версию сессии по расширению:
          string ext = Path.GetExtension(dlg.FileName).ToLower();
          if (ext.Equals(Performer.CURRENT_QUERY_EXT))
          {
            q = Query.Load(dlg.FileName);
          }
          else if (ext.Equals(".q2"))
          { // Преобразуем из версии 2 в текущую версию
            q = new Query(CommandAS.QueryLib2.Query.Load(dlg.FileName));
          }
        }
        catch { }

        if (q != null)
        {
          _task.AddQuery(q);
          _tvQueries.Load(_task);
        }
        else
          MessageBox.Show("Не известный формат файла");
      }
    }


    private void _mnuHelpTopic_Click(object sender, System.EventArgs e)
    {
      ProcessStartInfo psi = new ProcessStartInfo();
      try
      {
        psi.FileName = @"Query.chm";
        //psi.Arguments = "\""+fn+"\"";
        Process.Start(psi);
      }
      catch (Exception ex)
      {
        Error.ShowError(ex.Message);
      }
    }

    private void DoCommandAbout(object sender, System.EventArgs e)
    {

      if (About != null)
        About(this, new EventArgs());
    }

    private void OnTBButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
    {
      ToolBarButton tbb = e.Button;
      MenuItem mi = (MenuItem)tbb.Tag;
      if (mi != null)
        mi.PerformClick();
    }

    #endregion DoCommand Menu & ToolBar.

    private void DoLoadSession(string aFileName)
    {
      bool flg = pDesignerMode;

      if (_task.Load(aFileName))
      {
        _GetCurrentSessionParam();
        _DBConnection();

        _commParamDef.SetData(_task.pCommonParamCollection);

        TitleText();
        ChangeFont();
        if (LoadSession != null)
          LoadSession(this, new EventArgs());

        pDesignerMode = flg;

        if (!pDesignerMode)
          LoadQueriesParametersFromRegister();
      }
      else
        _sb.Text = _task.pError.text;

    }

    private void _DBConnection()
    {
      _sb.Text = "Установка соединения с БД ...";
      _task.pDBConnectionString = _txtConnection.Text;
      if (_task.DBConnection())
        _sb.Text = "Cоединения с БД УСПЕШНО установлено !";
      else
        _sb.Text = "ОШИБКА соединения с БД - " + _task.pError.text;
    }

    private void SaveSession(string aFileName)
    {
      _SetCurrentSessionParam();
      _task.SaveAs(aFileName);
      if (_task.pError.IsError)
        _sb.Text = "ОШИБКА - " + _task.pError.text;
      else
        _sb.Text = "Сессия сохранена.";
    }

    private void _SetCurrentSessionParam()
    {
      _task.pCode = _txtCode.Text;
      _task.pTitle = _txtTitle.Text;
      _task.pNote = _txtNote.Text;

      if (_txtConnection.Enabled)
        _DBConnection();

      _task.pParamBegDelim = _txtParamBegDelim.Text;
      _task.pParamEndDelim = _txtParamEndDelim.Text;

      _task.pImageName = _txtImageName.Text;
      _task.pImagePath = _txtImagePath.Text;

      //_task.pXSLTIncludeName = _txtXSLTIncName.Text;
      //_task.pXSLTIncludeText = _txtXSLTIncText.Text;
      _commParamDef.SaveToCollection();

    }

    private void _GetCurrentSessionParam()
    {
      _txtCode.Text = _task.pCode;
      _txtTitle.Text = _task.pTitle;
      _txtNote.Text = _task.pNote;

      _txtConnection.Text = _task.pDBConnectionString;

      _txtParamBegDelim.Text = _task.pParamBegDelim;
      _txtParamEndDelim.Text = _task.pParamEndDelim;

      _txtImageName.Text = _task.pImageName;
      _txtImagePath.Text = _task.pImagePath;


      //_txtXSLTIncName.Text = _task.pXSLTIncludeName;
      //_txtXSLTIncText.Text = _task.pXSLTIncludeText;
      //_commParamDef.SetData(_task.pCommonParamCollection);

      LoadSesImage();
    }

    private void LoadSesImage()
    {
      if ((_txtImagePath.Text.Length * _txtImageName.Text.Length) == 0)
      {
        _img = null;
      }
      else
      {
        try
        {
          _img = Image.FromFile(_txtImagePath.Text + Path.DirectorySeparatorChar + _txtImageName.Text);
        }
        catch //(Exception ex)
        {
          //MessageBox.Show(ex.Message);
        }
      }
      _pnlImg.Refresh();
    }

    private void DoCommandTVQAfterSelect(object sender, TreeViewEventArgs e)
    {
      bool flg = false;
      if (e != null && e.Node != null)
        flg = e.Node.Nodes.Count == 0;
      _mnuExec.Enabled = flg;
      _mnuDeleteQuery.Enabled = _mnuExec.Enabled;
      _mnuEdit.Enabled = _mnuExec.Enabled;
      _mnuProperty.Enabled = _mnuExec.Enabled;
      _mnuExport.Enabled = _mnuExec.Enabled;

      _tbbExecRTab.Enabled = _mnuExec.Enabled;
      _tbbExecRExcel.Enabled = _mnuExec.Enabled;
      _tbbExecRHTML.Enabled = _mnuExec.Enabled && (((Query)e.Node.Tag).XSLT.Length > 0);

      #region added by DSY 19.06.2008
      if (e != null)
      {
        if (((Query)e.Node.Tag).XSLT.ToLower().Contains("<?xml"))
          this._tbbExecRHTML.ToolTipText = "Выполнить (в HTML)";
        else
          this._tbbExecRHTML.ToolTipText = "Заполнить шаблон Excel";
      }
      #endregion

      _tbbDelQuery.Enabled = _mnuExec.Enabled;
      _tbbEditSQL.Enabled = _mnuExec.Enabled;
      _tbbEditParam.Enabled = _mnuExec.Enabled;
      _tbbEditXSLT.Enabled = _mnuExec.Enabled;

      if (e != null && _mnuExec.Enabled)
      {
        //_param.pConnStr = _txtConnection.Text;
        _task.pCurrentQuery = e.Node.Tag as Query;
      }

      _param.ShowParam();

      _task.BeforeClosed();
    }

    private void DoCommandTVQDoubleClick(object sender, EventArgs e)
    {
      if (_tvQueries.SelectedNode.Nodes.Count == 0)
        ExecuteQuery();
    }

    private void DoCommandTVQContextMenuPopup(object sender, EventArgs e)
    {
      _miExecute.Enabled = (_tvQueries.SelectedNode != null && _tvQueries.SelectedNode.Nodes.Count == 0);
      _miEditQuery.Enabled = _miExecute.Enabled;
      _miDeleteQuery.Enabled = _miExecute.Enabled;
      _miPropertyQuery.Enabled = _miExecute.Enabled;
    }

    private void DoCommandFontChange(object sender, System.EventArgs e)
    {
      FontDialog dlg = new FontDialog();
      dlg.Font = _sesFont;
      if (dlg.ShowDialog() == DialogResult.OK)
      {
        _sesFont = dlg.Font;
        ChangeFont();
      }
    }

    private void DoCommandBrowse(object sender, System.EventArgs e)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      dlg.Filter = "Программа (*.exe)|*.exe";
      if (dlg.ShowDialog() == DialogResult.OK)
        _txtTextEditor.Text = dlg.FileName;
    }

    private void DoCommandBrowseDir(object sender, System.EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      if (dlg.ShowDialog() == DialogResult.OK)
        _txtTempPath.Text = dlg.SelectedPath;
    }

    private void DoCommandBrowseImagePath(object sender, System.EventArgs e)
    {
      //FolderBrowserDialog dlg = new FolderBrowserDialog();
      //if( dlg.ShowDialog() == DialogResult.OK )
      //	_txtImagePath.Text = dlg.SelectedPath;

      OpenFileDialog dlg = new OpenFileDialog();
      dlg.Filter = "Все файлы с изображением|*.bmp;*.ico;*.gif;*.jpeg;*.jpg;*.jfif;*.png;*.tif;*.tiff|" +
        "Windows Bitmap (*.bmp)|*.bmp|" +
        "Windows Icon (*.ico)|*.ico|" +
        "Graphics Interchange Format (*.gif)|*.gif|" +
        "JPEG File Interchange Format (*.jpg)|*.jpg;*.jpeg;*.jfif|" +
        "Portable Network Graphics (*.png)|*.png|" +
        "Tag Image File Format (*.tif)|*.tif;*.tiff|" +
        "All Files (*.*)|*.*";

      if (dlg.ShowDialog() == DialogResult.OK)
      {
        _txtImagePath.Text = Path.GetDirectoryName(dlg.FileName);
        _txtImageName.Text = Path.GetFileName(dlg.FileName);
        LoadSesImage();
      }

    }

    private void _tc_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (_tc.SelectedIndex == _tc.TabPages.IndexOf(_tcpQuery))
      {
        _SetCurrentSessionParam();
        //_sb.Text = "_tc_Selected _tcpQuery - " + DateTime.Now.ToLongTimeString();
      }
    }

    private void _cmdPassword_Click(object sender, EventArgs e)
    {
      dlgPasswordSet dlg = new dlgPasswordSet(_task);
      dlg.ShowDialog();
    }

    private void _pnlImg_Paint(object sender, PaintEventArgs e)
    {
      if (_img != null)
      {
        ImageTools.ScaleImageIsotropically(e.Graphics, _img, new Rectangle(0, 0, _pnlImg.Width, _pnlImg.Height));
      }

    }

    #region Загрузка и сохранение параметров в регистр

    private void LoadParametersFromRegister()
    {
      RegistryKey regkey = Registry.CurrentUser.OpenSubKey(AppConst.REG_APP_PATH);

      try
      {
        if (regkey != null)
        {
          if ((int)regkey.GetValue("FormWindowStateMaximized") == 1)
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
          else
          {
            string[] aStr = regkey.GetValue("FormSize").ToString().Split('|');
            if (aStr.Length == 2)
              this.Size = new Size(Convert.ToInt32(aStr[0]), Convert.ToInt32(aStr[1]));
            aStr = regkey.GetValue("FormLocation").ToString().Split('|');
            if (aStr.Length == 2)
              this.Location = new Point(Convert.ToInt32(aStr[0]), Convert.ToInt32(aStr[1]));
          }
          string tt = regkey.GetValue("SplitQPosition").ToString();
          if (tt.Length > 0)
          {
            _splitQ.SplitPosition = Convert.ToInt32(tt);
            _curSplitQPos = _splitQ.SplitPosition;
          }
        }
      }
      catch { }
      try
      {
        string _sesDefault = pLoadDefaultSession;
        if (_sesDefault.Length == 0)
          _sesDefault = regkey.GetValue("DefaultSession").ToString();
        if (_sesDefault.Length > 0)
        {
          DoLoadSession(_sesDefault);
          _chkDefaultSession.Checked = true;
        }
      }
      catch { }

      try
      {
        frmQResult.pToggleStatus = Convert.ToBoolean(regkey.GetValue("VR_ToggleStatus"));
      }
      catch { }

      try
      {
        _txtTextEditor.Text = regkey.GetValue("TextEditor").ToString();
      }
      catch { }

      try
      {
        _txtTempPath.Text = regkey.GetValue("TempPath").ToString();
      }
      catch { }
      if (_txtTempPath.Text.Length == 0)
        _txtTempPath.Text = Path.GetTempPath();

      try
      {
        string[] ss = regkey.GetValue("CurrentFont").ToString().Split("|".ToCharArray());
        if (ss.Length > 0)
        {
          _sesFont = new Font(ss[0], float.Parse(ss[1]), (FontStyle)Enum.Parse(typeof(FontStyle), ss[2]));
          ChangeFont();
        }
      }
      catch { }
    }

    private void SaveParametersToRegister()
    {
      RegistryKey regkey = Registry.CurrentUser.OpenSubKey(AppConst.REG_APP_PATH, true);
      if (regkey == null)
        regkey = Registry.CurrentUser.CreateSubKey(AppConst.REG_APP_PATH);
      if (this.WindowState == System.Windows.Forms.FormWindowState.Normal)
      {
        regkey.SetValue("FormSize", this.Size.Width + "|" + this.Size.Height);
        regkey.SetValue("FormLocation", this.Location.X + "|" + this.Location.Y);
        regkey.SetValue("FormWindowStateMaximized", 0);
      }
      else if (this.WindowState == System.Windows.Forms.FormWindowState.Maximized)
        regkey.SetValue("FormWindowStateMaximized", 1);
      regkey.SetValue("SplitQPosition", _splitQ.SplitPosition.ToString());
      if (_chkDefaultSession.Checked)
        regkey.SetValue("DefaultSession", _task.pFileName);

      regkey.SetValue("VR_ToggleStatus", frmQResult.pToggleStatus ? 1 : 0);
      regkey.SetValue("TextEditor", _txtTextEditor.Text);
      regkey.SetValue("TempPath", _txtTempPath.Text);


      regkey.SetValue("CurrentFont", _sesFont.Name + "|" + _sesFont.Size + "|" + _sesFont.Style);

      regkey.Close();
    }

    private void LoadQueriesParametersFromRegister()
    {
      if (CASTools.ConvertToInt32Or0(_task.pCode) == 0)
        return;

      string regPath = AppConst.REG_APP_PATH + @"\SessionParam\" + _task.pCode + @"\";
      RegistryKey regkey = null;

      foreach (Query q in _task.pQueries)
      {
        if (q.Code == 0)
          continue;

        foreach (Param p in q.Params)
        {
          try
          {
            regkey = Registry.CurrentUser.OpenSubKey(regPath + q.Code);
            if (regkey != null)
              p.CurrentValue = regkey.GetValue(p.Name).ToString();
          }
          catch { }
        }
      }

    }

    private void SaveQueriesParametersToRegister()
    {
      if (CASTools.ConvertToInt32Or0(_task.pCode) == 0)
        return;

      string regPath = AppConst.REG_APP_PATH + @"\SessionParam\" + _task.pCode + @"\";
      RegistryKey regkey = null;
      foreach (Query q in _task.pQueries)
      {
        if (q.Code == 0)
          continue;

        regkey = Registry.CurrentUser.OpenSubKey(regPath + q.Code, true);
        if (regkey == null)
          regkey = Registry.CurrentUser.CreateSubKey(regPath + q.Code);

        foreach (Param p in q.Params)
          regkey.SetValue(p.Name, p.CurrentValue);

        regkey.Close();
      }
    }

    #endregion ... загрузка и сохранение параметров в регистр

    private void ChangeFont()
    {
      _txtFont.Text = _sesFont.ToString();
      _tvQueries.Font = new Font(_sesFont.FontFamily.Name, _sesFont.Size, _sesFont.Style);
      //_txtXSLTIncText.Font = new Font(_sesFont.FontFamily.Name, _sesFont.Size, _sesFont.Style);
    }

    private bool ExecuteQuery()
    {
      bool res = false;
      Query q = _tvQueries.SelectedNode.Tag as Query;
      if (q != null)
      {
        MouseCursor.SetCursorWait();
        try
        {
          _param.BeforeExecute();
          _sb.Text = "Выполнение запроса ...";
          _task.pTempPath = _txtTempPath.Text; // ХОТЯ это надо делать не здесь !!!
          res = _task.Execute(q);
          if (res)
            _sb.Text = "Запрос выполнен успешно!";
          else
          {
            _sb.Text = _task.pError.text;
            _task.pError.ShowIfIs();
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show("src: " + ex.Source + "\nmsg: " + ex.Message);
        }
        finally
        {
          MouseCursor.SetCursorDefault();
        }
      }
      else
        _sb.Text = "Не выбран запрос!";

      return res;
    }

    private void ExecuteShowTable()
    {
      if (ExecuteQuery())
      {
        frmQResult frm = new frmQResult(_task);
        frm.pIconColl = _iconColl;
        frm.Show();
      }
    }

    private void ExecuteShowExcel()
    {
      if (ExecuteQuery())
        _task.ToExcel();
    }

    private void ExecuteShowXSLT_Format1()
    {
      this.Cursor = Cursors.WaitCursor;
      if (ExecuteQuery())
      {
        string fn = _task.GetFilefromXML_XSL();

        #region added by DSY 19.06.2008
        //  это значит, что заполняется шаблон
        if (fn.Length == 0)
        {
          this.Cursor = Cursors.Default;
          return;
        }
        #endregion

        if (_task.pError.IsOk)
        {
          ProcessStartInfo psi = new ProcessStartInfo();
          try
          {
            psi.FileName = @"iexplore.exe";
            psi.Arguments = "\"" + fn + "\"";
          }
          catch { }

          if (psi.FileName.Length > 0)
          {
            try
            {
              Process.Start(psi);
            }
            catch (Exception ex)
            {
              Error.ShowError(ex.Message);
            }
          }
        }
        else
        {
          _task.pError.Show();
        }
      }
      this.Cursor = Cursors.Default;
    }

    private void ShowQueryTreeView()
    {
      _tvQueries.Load(_task);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this._mnu = new System.Windows.Forms.MainMenu(this.components);
      this._mnuSession = new System.Windows.Forms.MenuItem();
      this._mnuNewSession = new System.Windows.Forms.MenuItem();
      this._mnuLoadSession = new System.Windows.Forms.MenuItem();
      this._mnuSaveSession = new System.Windows.Forms.MenuItem();
      this._mnuSaveSessionAs = new System.Windows.Forms.MenuItem();
      this._mnuSep0 = new System.Windows.Forms.MenuItem();
      this._mnuEditSession = new System.Windows.Forms.MenuItem();
      this._mnuSep1 = new System.Windows.Forms.MenuItem();
      this._mnuExit = new System.Windows.Forms.MenuItem();
      this._mnuSep2 = new System.Windows.Forms.MenuItem();
      this._mnuExec = new System.Windows.Forms.MenuItem();
      this._mnuExecRTab = new System.Windows.Forms.MenuItem();
      this._mnuExecRExcel = new System.Windows.Forms.MenuItem();
      this._mnuExecRHTML = new System.Windows.Forms.MenuItem();
      this.menuItem9 = new System.Windows.Forms.MenuItem();
      this._mnuNewQuery = new System.Windows.Forms.MenuItem();
      this._mnuDeleteQuery = new System.Windows.Forms.MenuItem();
      this._mnuEdit = new System.Windows.Forms.MenuItem();
      this._mnuEditSQL = new System.Windows.Forms.MenuItem();
      this._mnuEditParam = new System.Windows.Forms.MenuItem();
      this._mnuEditXSLT = new System.Windows.Forms.MenuItem();
      this._mnuProperty = new System.Windows.Forms.MenuItem();
      this.menuItem3 = new System.Windows.Forms.MenuItem();
      this._mnuExport = new System.Windows.Forms.MenuItem();
      this._mnuImport = new System.Windows.Forms.MenuItem();
      this._mnuHelp = new System.Windows.Forms.MenuItem();
      this._mnuAbout = new System.Windows.Forms.MenuItem();
      this._mnuHelpTopic = new System.Windows.Forms.MenuItem();
      this._tb = new System.Windows.Forms.ToolBar();
      this._tbbSaveSession = new System.Windows.Forms.ToolBarButton();
      this._tbbLoadSession = new System.Windows.Forms.ToolBarButton();
      this._tbbSep1 = new System.Windows.Forms.ToolBarButton();
      this._tbbExecRTab = new System.Windows.Forms.ToolBarButton();
      this._tbbExecRExcel = new System.Windows.Forms.ToolBarButton();
      this._tbbExecRHTML = new System.Windows.Forms.ToolBarButton();
      this._tbbSep2 = new System.Windows.Forms.ToolBarButton();
      this._tbbNewQuery = new System.Windows.Forms.ToolBarButton();
      this._tbbDelQuery = new System.Windows.Forms.ToolBarButton();
      this._tbbSep3 = new System.Windows.Forms.ToolBarButton();
      this._tbbEditSQL = new System.Windows.Forms.ToolBarButton();
      this._tbbEditParam = new System.Windows.Forms.ToolBarButton();
      this._tbbEditXSLT = new System.Windows.Forms.ToolBarButton();
      this._tbbSep4 = new System.Windows.Forms.ToolBarButton();
      this._tbbAbout = new System.Windows.Forms.ToolBarButton();
      this._sb = new System.Windows.Forms.StatusBar();
      this._tc = new System.Windows.Forms.TabControl();
      this._tcpQuery = new System.Windows.Forms.TabPage();
      this._tcpParam = new System.Windows.Forms.TabPage();
      this._pnlImg = new System.Windows.Forms.Panel();
      this.label12 = new System.Windows.Forms.Label();
      this._cmdPassword = new System.Windows.Forms.Button();
      this._txtNote = new System.Windows.Forms.TextBox();
      this.label11 = new System.Windows.Forms.Label();
      this._txtCode = new System.Windows.Forms.TextBox();
      this.label8 = new System.Windows.Forms.Label();
      this._txtTitle = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this._txtImageName = new System.Windows.Forms.TextBox();
      this.label10 = new System.Windows.Forms.Label();
      this._cmdImagePath = new System.Windows.Forms.Button();
      this._txtImagePath = new System.Windows.Forms.TextBox();
      this.label9 = new System.Windows.Forms.Label();
      this._grpParamDelim = new System.Windows.Forms.GroupBox();
      this._txtParamEndDelim = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this._txtParamBegDelim = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this._txtConnection = new System.Windows.Forms.TextBox();
      this._lblConnection = new System.Windows.Forms.Label();
      this._tcpXSLTInc = new System.Windows.Forms.TabPage();
      this._tcpUser = new System.Windows.Forms.TabPage();
      this._chkDefaultSession = new System.Windows.Forms.CheckBox();
      this._cmdBrowseDir = new System.Windows.Forms.Button();
      this._txtTempPath = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this._cmdBrowse = new System.Windows.Forms.Button();
      this._txtTextEditor = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this._cmdFont = new System.Windows.Forms.Button();
      this._txtFont = new System.Windows.Forms.TextBox();
      this._lblFont = new System.Windows.Forms.Label();
      this._tc.SuspendLayout();
      this._tcpParam.SuspendLayout();
      this._grpParamDelim.SuspendLayout();
      this._tcpUser.SuspendLayout();
      this.SuspendLayout();
      // 
      // _mnu
      // 
      this._mnu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._mnuSession,
            this._mnuSep2,
            this._mnuHelp});
      // 
      // _mnuSession
      // 
      this._mnuSession.Index = 0;
      this._mnuSession.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._mnuNewSession,
            this._mnuLoadSession,
            this._mnuSaveSession,
            this._mnuSaveSessionAs,
            this._mnuSep0,
            this._mnuEditSession,
            this._mnuSep1,
            this._mnuExit});
      this._mnuSession.Text = "Сессия";
      // 
      // _mnuNewSession
      // 
      this._mnuNewSession.Index = 0;
      this._mnuNewSession.Text = "Новая";
      this._mnuNewSession.Click += new System.EventHandler(this.DoCommandNewSession);
      // 
      // _mnuLoadSession
      // 
      this._mnuLoadSession.Index = 1;
      this._mnuLoadSession.Text = "Загрузить";
      this._mnuLoadSession.Click += new System.EventHandler(this.DoCommandLoadSession);
      // 
      // _mnuSaveSession
      // 
      this._mnuSaveSession.Index = 2;
      this._mnuSaveSession.Text = "Сохранить";
      this._mnuSaveSession.Click += new System.EventHandler(this.DoCommandSaveSession);
      // 
      // _mnuSaveSessionAs
      // 
      this._mnuSaveSessionAs.Index = 3;
      this._mnuSaveSessionAs.Text = "Сохранить как ...";
      this._mnuSaveSessionAs.Click += new System.EventHandler(this.DoCommandSaveSessionAs);
      // 
      // _mnuSep0
      // 
      this._mnuSep0.Index = 4;
      this._mnuSep0.Text = "-";
      // 
      // _mnuEditSession
      // 
      this._mnuEditSession.Index = 5;
      this._mnuEditSession.Text = "Конструирование";
      this._mnuEditSession.Click += new System.EventHandler(this.DoCommandEditSession);
      // 
      // _mnuSep1
      // 
      this._mnuSep1.Index = 6;
      this._mnuSep1.Text = "-";
      // 
      // _mnuExit
      // 
      this._mnuExit.Index = 7;
      this._mnuExit.Text = "Выход";
      this._mnuExit.Click += new System.EventHandler(this.DoCommandExit);
      // 
      // _mnuSep2
      // 
      this._mnuSep2.Index = 1;
      this._mnuSep2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._mnuExec,
            this.menuItem9,
            this._mnuNewQuery,
            this._mnuDeleteQuery,
            this._mnuEdit,
            this._mnuProperty,
            this.menuItem3,
            this._mnuExport,
            this._mnuImport});
      this._mnuSep2.Text = "Запрос";
      // 
      // _mnuExec
      // 
      this._mnuExec.Index = 0;
      this._mnuExec.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._mnuExecRTab,
            this._mnuExecRExcel,
            this._mnuExecRHTML});
      this._mnuExec.Shortcut = System.Windows.Forms.Shortcut.F5;
      this._mnuExec.Text = "Выполнить";
      // 
      // _mnuExecRTab
      // 
      this._mnuExecRTab.Index = 0;
      this._mnuExecRTab.Text = "результат в таблицу";
      this._mnuExecRTab.Click += new System.EventHandler(this._mnuExecRTab_Click);
      // 
      // _mnuExecRExcel
      // 
      this._mnuExecRExcel.Index = 1;
      this._mnuExecRExcel.Text = "результат в Excel";
      this._mnuExecRExcel.Click += new System.EventHandler(this._mnuExecRExcel_Click);
      // 
      // _mnuExecRHTML
      // 
      this._mnuExecRHTML.Index = 2;
      this._mnuExecRHTML.Text = "результат в HTML";
      this._mnuExecRHTML.Click += new System.EventHandler(this._mnuExecRHTML_Click);
      // 
      // menuItem9
      // 
      this.menuItem9.Index = 1;
      this.menuItem9.Text = "-";
      // 
      // _mnuNewQuery
      // 
      this._mnuNewQuery.Index = 2;
      this._mnuNewQuery.Text = "Новый";
      this._mnuNewQuery.Click += new System.EventHandler(this.DoCommandNewQuery);
      // 
      // _mnuDeleteQuery
      // 
      this._mnuDeleteQuery.Index = 3;
      this._mnuDeleteQuery.Text = "Удалить";
      this._mnuDeleteQuery.Click += new System.EventHandler(this.DoCommandDeleteQuery);
      // 
      // _mnuEdit
      // 
      this._mnuEdit.Index = 4;
      this._mnuEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._mnuEditSQL,
            this._mnuEditParam,
            this._mnuEditXSLT});
      this._mnuEdit.Text = "Редактировать";
      // 
      // _mnuEditSQL
      // 
      this._mnuEditSQL.Index = 0;
      this._mnuEditSQL.Text = "Текст запроса";
      this._mnuEditSQL.Click += new System.EventHandler(this.DoCommandEditQuery);
      // 
      // _mnuEditParam
      // 
      this._mnuEditParam.Index = 1;
      this._mnuEditParam.Text = "Параметры";
      this._mnuEditParam.Click += new System.EventHandler(this.DoCommandEditQueryParam);
      // 
      // _mnuEditXSLT
      // 
      this._mnuEditXSLT.Index = 2;
      this._mnuEditXSLT.Text = "XSLT преобразование";
      this._mnuEditXSLT.Click += new System.EventHandler(this.DoCommandEditQuery);
      // 
      // _mnuProperty
      // 
      this._mnuProperty.Index = 5;
      this._mnuProperty.Text = "Свойства";
      this._mnuProperty.Click += new System.EventHandler(this.DoCommandPropertyQuery);
      // 
      // menuItem3
      // 
      this.menuItem3.Index = 6;
      this.menuItem3.Text = "-";
      // 
      // _mnuExport
      // 
      this._mnuExport.Index = 7;
      this._mnuExport.Text = "Экспорт";
      this._mnuExport.Click += new System.EventHandler(this.DoCommandExportQuery);
      // 
      // _mnuImport
      // 
      this._mnuImport.Index = 8;
      this._mnuImport.Text = "Импорт";
      this._mnuImport.Click += new System.EventHandler(this.DoCommandImportQuery);
      // 
      // _mnuHelp
      // 
      this._mnuHelp.Index = 2;
      this._mnuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this._mnuAbout,
            this._mnuHelpTopic});
      this._mnuHelp.Text = "?";
      // 
      // _mnuAbout
      // 
      this._mnuAbout.Index = 0;
      this._mnuAbout.Text = "О программе";
      this._mnuAbout.Click += new System.EventHandler(this.DoCommandAbout);
      // 
      // _mnuHelpTopic
      // 
      this._mnuHelpTopic.Index = 1;
      this._mnuHelpTopic.Text = "Помощь";
      this._mnuHelpTopic.Click += new System.EventHandler(this._mnuHelpTopic_Click);
      // 
      // _tb
      // 
      this._tb.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this._tbbSaveSession,
            this._tbbLoadSession,
            this._tbbSep1,
            this._tbbExecRTab,
            this._tbbExecRExcel,
            this._tbbExecRHTML,
            this._tbbSep2,
            this._tbbNewQuery,
            this._tbbDelQuery,
            this._tbbSep3,
            this._tbbEditSQL,
            this._tbbEditParam,
            this._tbbEditXSLT,
            this._tbbSep4,
            this._tbbAbout});
      this._tb.DropDownArrows = true;
      this._tb.Location = new System.Drawing.Point(0, 0);
      this._tb.Name = "_tb";
      this._tb.ShowToolTips = true;
      this._tb.Size = new System.Drawing.Size(752, 28);
      this._tb.TabIndex = 0;
      this._tb.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.OnTBButtonClick);
      // 
      // _tbbSaveSession
      // 
      this._tbbSaveSession.Name = "_tbbSaveSession";
      this._tbbSaveSession.Tag = this._mnuSaveSession;
      this._tbbSaveSession.ToolTipText = "Сохранить сессию";
      // 
      // _tbbLoadSession
      // 
      this._tbbLoadSession.Name = "_tbbLoadSession";
      this._tbbLoadSession.Tag = this._mnuLoadSession;
      this._tbbLoadSession.ToolTipText = "Загрузить сессию";
      // 
      // _tbbSep1
      // 
      this._tbbSep1.Name = "_tbbSep1";
      this._tbbSep1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
      // 
      // _tbbExecRTab
      // 
      this._tbbExecRTab.Name = "_tbbExecRTab";
      this._tbbExecRTab.Tag = "";
      this._tbbExecRTab.ToolTipText = "Выполнить (в таб.)";
      // 
      // _tbbExecRExcel
      // 
      this._tbbExecRExcel.Name = "_tbbExecRExcel";
      this._tbbExecRExcel.ToolTipText = "Выполнить (в Excel)";
      // 
      // _tbbExecRHTML
      // 
      this._tbbExecRHTML.Name = "_tbbExecRHTML";
      this._tbbExecRHTML.ToolTipText = "Выполнить (в HTML)";
      // 
      // _tbbSep2
      // 
      this._tbbSep2.Name = "_tbbSep2";
      this._tbbSep2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
      // 
      // _tbbNewQuery
      // 
      this._tbbNewQuery.Name = "_tbbNewQuery";
      this._tbbNewQuery.Tag = this._mnuNewQuery;
      this._tbbNewQuery.ToolTipText = "Новый запрос";
      // 
      // _tbbDelQuery
      // 
      this._tbbDelQuery.Name = "_tbbDelQuery";
      this._tbbDelQuery.Tag = this._mnuDeleteQuery;
      this._tbbDelQuery.ToolTipText = "Удалить запрос";
      // 
      // _tbbSep3
      // 
      this._tbbSep3.Name = "_tbbSep3";
      this._tbbSep3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
      // 
      // _tbbEditSQL
      // 
      this._tbbEditSQL.Name = "_tbbEditSQL";
      // 
      // _tbbEditParam
      // 
      this._tbbEditParam.Name = "_tbbEditParam";
      // 
      // _tbbEditXSLT
      // 
      this._tbbEditXSLT.Name = "_tbbEditXSLT";
      // 
      // _tbbSep4
      // 
      this._tbbSep4.Name = "_tbbSep4";
      this._tbbSep4.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
      // 
      // _tbbAbout
      // 
      this._tbbAbout.Name = "_tbbAbout";
      this._tbbAbout.Tag = this._mnuAbout;
      this._tbbAbout.ToolTipText = "О программе";
      // 
      // _sb
      // 
      this._sb.Location = new System.Drawing.Point(0, 472);
      this._sb.Name = "_sb";
      this._sb.Size = new System.Drawing.Size(752, 22);
      this._sb.TabIndex = 1;
      // 
      // _tc
      // 
      this._tc.Controls.Add(this._tcpQuery);
      this._tc.Controls.Add(this._tcpParam);
      this._tc.Controls.Add(this._tcpXSLTInc);
      this._tc.Controls.Add(this._tcpUser);
      this._tc.Dock = System.Windows.Forms.DockStyle.Fill;
      this._tc.Location = new System.Drawing.Point(0, 28);
      this._tc.Name = "_tc";
      this._tc.SelectedIndex = 0;
      this._tc.Size = new System.Drawing.Size(752, 444);
      this._tc.TabIndex = 2;
      // 
      // _tcpQuery
      // 
      this._tcpQuery.Location = new System.Drawing.Point(4, 22);
      this._tcpQuery.Name = "_tcpQuery";
      this._tcpQuery.Size = new System.Drawing.Size(744, 418);
      this._tcpQuery.TabIndex = 0;
      this._tcpQuery.Text = "Запросы";
      // 
      // _tcpParam
      // 
      this._tcpParam.Controls.Add(this._pnlImg);
      this._tcpParam.Controls.Add(this.label12);
      this._tcpParam.Controls.Add(this._cmdPassword);
      this._tcpParam.Controls.Add(this._txtNote);
      this._tcpParam.Controls.Add(this.label11);
      this._tcpParam.Controls.Add(this._txtCode);
      this._tcpParam.Controls.Add(this.label8);
      this._tcpParam.Controls.Add(this._txtTitle);
      this._tcpParam.Controls.Add(this.label1);
      this._tcpParam.Controls.Add(this._txtImageName);
      this._tcpParam.Controls.Add(this.label10);
      this._tcpParam.Controls.Add(this._cmdImagePath);
      this._tcpParam.Controls.Add(this._txtImagePath);
      this._tcpParam.Controls.Add(this.label9);
      this._tcpParam.Controls.Add(this._grpParamDelim);
      this._tcpParam.Controls.Add(this._txtConnection);
      this._tcpParam.Controls.Add(this._lblConnection);
      this._tcpParam.Location = new System.Drawing.Point(4, 22);
      this._tcpParam.Name = "_tcpParam";
      this._tcpParam.Size = new System.Drawing.Size(619, 356);
      this._tcpParam.TabIndex = 1;
      this._tcpParam.Text = "Настройки сессии";
      // 
      // _pnlImg
      // 
      this._pnlImg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)));
      this._pnlImg.Location = new System.Drawing.Point(7, 208);
      this._pnlImg.Name = "_pnlImg";
      this._pnlImg.Size = new System.Drawing.Size(173, 114);
      this._pnlImg.TabIndex = 43;
      // 
      // label12
      // 
      this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.label12.Location = new System.Drawing.Point(413, 328);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(14, 20);
      this.label12.TabIndex = 42;
      this.label12.Text = "\\";
      // 
      // _cmdPassword
      // 
      this._cmdPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._cmdPassword.Location = new System.Drawing.Point(313, 7);
      this._cmdPassword.Name = "_cmdPassword";
      this._cmdPassword.Size = new System.Drawing.Size(300, 20);
      this._cmdPassword.TabIndex = 9;
      this._cmdPassword.Text = "Установить/изменить пароль";
      // 
      // _txtNote
      // 
      this._txtNote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._txtNote.Location = new System.Drawing.Point(187, 55);
      this._txtNote.Multiline = true;
      this._txtNote.Name = "_txtNote";
      this._txtNote.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this._txtNote.Size = new System.Drawing.Size(426, 118);
      this._txtNote.TabIndex = 2;
      // 
      // label11
      // 
      this.label11.Location = new System.Drawing.Point(80, 55);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(93, 20);
      this.label11.TabIndex = 38;
      this.label11.Text = "Комментарии";
      // 
      // _txtCode
      // 
      this._txtCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this._txtCode.ForeColor = System.Drawing.SystemColors.HotTrack;
      this._txtCode.Location = new System.Drawing.Point(7, 28);
      this._txtCode.Name = "_txtCode";
      this._txtCode.Size = new System.Drawing.Size(53, 19);
      this._txtCode.TabIndex = 0;
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(13, 7);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(47, 21);
      this.label8.TabIndex = 36;
      this.label8.Text = "Код";
      // 
      // _txtTitle
      // 
      this._txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._txtTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this._txtTitle.ForeColor = System.Drawing.SystemColors.HotTrack;
      this._txtTitle.Location = new System.Drawing.Point(67, 28);
      this._txtTitle.Name = "_txtTitle";
      this._txtTitle.Size = new System.Drawing.Size(546, 19);
      this._txtTitle.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(67, 7);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(125, 23);
      this.label1.TabIndex = 34;
      this.label1.Text = "Название";
      // 
      // _txtImageName
      // 
      this._txtImageName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._txtImageName.Location = new System.Drawing.Point(427, 328);
      this._txtImageName.Name = "_txtImageName";
      this._txtImageName.Size = new System.Drawing.Size(160, 20);
      this._txtImageName.TabIndex = 7;
      // 
      // label10
      // 
      this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.label10.Location = new System.Drawing.Point(427, 308);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(93, 20);
      this.label10.TabIndex = 32;
      this.label10.Text = "Изображение";
      // 
      // _cmdImagePath
      // 
      this._cmdImagePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cmdImagePath.Location = new System.Drawing.Point(587, 328);
      this._cmdImagePath.Name = "_cmdImagePath";
      this._cmdImagePath.Size = new System.Drawing.Size(24, 23);
      this._cmdImagePath.TabIndex = 8;
      this._cmdImagePath.Text = "...";
      this._cmdImagePath.Click += new System.EventHandler(this.DoCommandBrowseImagePath);
      // 
      // _txtImagePath
      // 
      this._txtImagePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._txtImagePath.Location = new System.Drawing.Point(7, 328);
      this._txtImagePath.Name = "_txtImagePath";
      this._txtImagePath.Size = new System.Drawing.Size(400, 20);
      this._txtImagePath.TabIndex = 6;
      this._txtImagePath.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // label9
      // 
      this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.label9.Location = new System.Drawing.Point(287, 308);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(120, 22);
      this.label9.TabIndex = 29;
      this.label9.Text = "Месторасположение";
      // 
      // _grpParamDelim
      // 
      this._grpParamDelim.Controls.Add(this._txtParamEndDelim);
      this._grpParamDelim.Controls.Add(this.label5);
      this._grpParamDelim.Controls.Add(this._txtParamBegDelim);
      this._grpParamDelim.Controls.Add(this.label4);
      this._grpParamDelim.Location = new System.Drawing.Point(13, 90);
      this._grpParamDelim.Name = "_grpParamDelim";
      this._grpParamDelim.Size = new System.Drawing.Size(167, 69);
      this._grpParamDelim.TabIndex = 4;
      this._grpParamDelim.TabStop = false;
      this._grpParamDelim.Text = "Ограничитель параметра";
      // 
      // _txtParamEndDelim
      // 
      this._txtParamEndDelim.Location = new System.Drawing.Point(100, 42);
      this._txtParamEndDelim.Name = "_txtParamEndDelim";
      this._txtParamEndDelim.Size = new System.Drawing.Size(60, 20);
      this._txtParamEndDelim.TabIndex = 3;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(7, 42);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(86, 20);
      this.label5.TabIndex = 2;
      this.label5.Text = "окончание";
      // 
      // _txtParamBegDelim
      // 
      this._txtParamBegDelim.Location = new System.Drawing.Point(100, 21);
      this._txtParamBegDelim.Name = "_txtParamBegDelim";
      this._txtParamBegDelim.Size = new System.Drawing.Size(60, 20);
      this._txtParamBegDelim.TabIndex = 1;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(7, 21);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(86, 20);
      this.label4.TabIndex = 0;
      this.label4.Text = "начало";
      // 
      // _txtConnection
      // 
      this._txtConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._txtConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this._txtConnection.ForeColor = System.Drawing.SystemColors.MenuText;
      this._txtConnection.Location = new System.Drawing.Point(187, 180);
      this._txtConnection.Multiline = true;
      this._txtConnection.Name = "_txtConnection";
      this._txtConnection.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this._txtConnection.Size = new System.Drawing.Size(426, 114);
      this._txtConnection.TabIndex = 5;
      this._txtConnection.TextChanged += new System.EventHandler(this.DoChangedCommon);
      // 
      // _lblConnection
      // 
      this._lblConnection.Location = new System.Drawing.Point(80, 180);
      this._lblConnection.Name = "_lblConnection";
      this._lblConnection.Size = new System.Drawing.Size(105, 24);
      this._lblConnection.TabIndex = 0;
      this._lblConnection.Text = "Соединение с БД";
      // 
      // _tcpXSLTInc
      // 
      this._tcpXSLTInc.Location = new System.Drawing.Point(4, 22);
      this._tcpXSLTInc.Name = "_tcpXSLTInc";
      this._tcpXSLTInc.Size = new System.Drawing.Size(619, 356);
      this._tcpXSLTInc.TabIndex = 2;
      this._tcpXSLTInc.Text = "Параметры сессии";
      // 
      // _tcpUser
      // 
      this._tcpUser.Controls.Add(this._chkDefaultSession);
      this._tcpUser.Controls.Add(this._cmdBrowseDir);
      this._tcpUser.Controls.Add(this._txtTempPath);
      this._tcpUser.Controls.Add(this.label3);
      this._tcpUser.Controls.Add(this._cmdBrowse);
      this._tcpUser.Controls.Add(this._txtTextEditor);
      this._tcpUser.Controls.Add(this.label2);
      this._tcpUser.Controls.Add(this._cmdFont);
      this._tcpUser.Controls.Add(this._txtFont);
      this._tcpUser.Controls.Add(this._lblFont);
      this._tcpUser.Location = new System.Drawing.Point(4, 22);
      this._tcpUser.Name = "_tcpUser";
      this._tcpUser.Size = new System.Drawing.Size(619, 356);
      this._tcpUser.TabIndex = 3;
      this._tcpUser.Text = "Настройки пользователя";
      // 
      // _chkDefaultSession
      // 
      this._chkDefaultSession.Location = new System.Drawing.Point(7, 97);
      this._chkDefaultSession.Name = "_chkDefaultSession";
      this._chkDefaultSession.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this._chkDefaultSession.Size = new System.Drawing.Size(246, 42);
      this._chkDefaultSession.TabIndex = 5;
      this._chkDefaultSession.Text = "Запускать текущую сессию по умолчанию";
      this._chkDefaultSession.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // _cmdBrowseDir
      // 
      this._cmdBrowseDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._cmdBrowseDir.Location = new System.Drawing.Point(588, 42);
      this._cmdBrowseDir.Name = "_cmdBrowseDir";
      this._cmdBrowseDir.Size = new System.Drawing.Size(24, 22);
      this._cmdBrowseDir.TabIndex = 3;
      this._cmdBrowseDir.Text = "...";
      this._cmdBrowseDir.Click += new System.EventHandler(this.DoCommandBrowseDir);
      // 
      // _txtTempPath
      // 
      this._txtTempPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._txtTempPath.Location = new System.Drawing.Point(242, 42);
      this._txtTempPath.Name = "_txtTempPath";
      this._txtTempPath.Size = new System.Drawing.Size(340, 20);
      this._txtTempPath.TabIndex = 2;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(8, 42);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(227, 22);
      this.label3.TabIndex = 26;
      this.label3.Text = "Папка для временных файлов";
      // 
      // _cmdBrowse
      // 
      this._cmdBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._cmdBrowse.Location = new System.Drawing.Point(588, 14);
      this._cmdBrowse.Name = "_cmdBrowse";
      this._cmdBrowse.Size = new System.Drawing.Size(24, 22);
      this._cmdBrowse.TabIndex = 1;
      this._cmdBrowse.Text = "...";
      this._cmdBrowse.Click += new System.EventHandler(this.DoCommandBrowse);
      // 
      // _txtTextEditor
      // 
      this._txtTextEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._txtTextEditor.Location = new System.Drawing.Point(242, 14);
      this._txtTextEditor.Name = "_txtTextEditor";
      this._txtTextEditor.Size = new System.Drawing.Size(340, 20);
      this._txtTextEditor.TabIndex = 0;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(10, 14);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(225, 22);
      this.label2.TabIndex = 23;
      this.label2.Text = "Текстовой редактор";
      // 
      // _cmdFont
      // 
      this._cmdFont.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._cmdFont.Location = new System.Drawing.Point(588, 69);
      this._cmdFont.Name = "_cmdFont";
      this._cmdFont.Size = new System.Drawing.Size(24, 23);
      this._cmdFont.TabIndex = 4;
      this._cmdFont.Text = "...";
      this._cmdFont.Click += new System.EventHandler(this.DoCommandFontChange);
      // 
      // _txtFont
      // 
      this._txtFont.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._txtFont.Location = new System.Drawing.Point(242, 69);
      this._txtFont.Name = "_txtFont";
      this._txtFont.ReadOnly = true;
      this._txtFont.Size = new System.Drawing.Size(340, 20);
      this._txtFont.TabIndex = 21;
      // 
      // _lblFont
      // 
      this._lblFont.Location = new System.Drawing.Point(10, 69);
      this._lblFont.Name = "_lblFont";
      this._lblFont.Size = new System.Drawing.Size(225, 23);
      this._lblFont.TabIndex = 20;
      this._lblFont.Text = "Шрифт";
      // 
      // frmQuery
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(752, 494);
      this.Controls.Add(this._tc);
      this.Controls.Add(this._sb);
      this.Controls.Add(this._tb);
      this.Menu = this._mnu;
      this.Name = "frmQuery";
      this._tc.ResumeLayout(false);
      this._tcpParam.ResumeLayout(false);
      this._tcpParam.PerformLayout();
      this._grpParamDelim.ResumeLayout(false);
      this._grpParamDelim.PerformLayout();
      this._tcpUser.ResumeLayout(false);
      this._tcpUser.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    } 

    #endregion

  }

  public class AppConst
  {
    public const string REG_APP_PATH = @"SOFTWARE\SoftCommandAs\Query";

    private AppConst() { }
  }
}
