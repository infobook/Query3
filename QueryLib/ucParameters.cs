using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using CommandAS.Tools;
using CommandAS.Tools.Controls;

namespace CommandAS.QueryLib
{
  public partial class ucParameters : UserControl
  {
    private Label _lblFilter;
    private TableLayoutPanel _tlp;
    private bool _autoHeight;

    public bool pAutoHeight
    {
      get { return _autoHeight; }
      set 
      { 
        _autoHeight = value;
        if (value)
        {
          _tlp.Visible = false;
          #region Добавлено DSY 29.11.2007
          this.Height = 0;
          #endregion
          _tlp.Height = 0;
        }
      }
    }

    /// <summary>
    /// нужно ли слово Фильтр перед параметрами
    /// </summary>
    private bool _ShowWordFilter;
    public bool pShowWordFilter
    {
      get
      {
        return _ShowWordFilter;
      }
      set 
      {
        _lblFilter.Visible = value;
        _ShowWordFilter = value;
      }
    }

    public string pFilterWord
    {
      get { return _lblFilter.Text; }
      set
      {
        if (value.Length > 0)
        {
          pShowWordFilter = true;
          _lblFilter.Text = value;
        }
        else
        {
          pShowWordFilter = false;
        }
      }
    }


    public string pIntListDelim;
    public Performer pPerf;

    public event EventHandler ChangedParameter;

    public ucParameters()
    {
      pIntListDelim = "~";
      pPerf = null;
      _autoHeight = false;

      InitializeComponent();

      #region Добавлено DSY 29.11.2007
      _lblFilter = new Label();
      _lblFilter.Text = "Фильтр";
      _lblFilter.TextAlign = ContentAlignment.BottomCenter;
      _lblFilter.Dock = DockStyle.Top;
      _lblFilter.Visible = false;
//      this.Controls.Add(_lblFilter);
      #endregion
    
      _tlp = new TableLayoutPanel();
      _tlp.Dock = DockStyle.Fill;
      _tlp.AutoScroll = true;
      //_tlp.AutoSize = true;
      _tlp.RowCount = 0;
      this.Controls.Add(_tlp);
    }

    public void ShowParam()
    {
      BeforeExecute(); // сохраним предыдущие значения параметров !!!

      ShowParam2();
    }

    public void ShowParam2()
    {

      if (pPerf == null || pPerf.pCurrentQuery == null)
      {
        Refresh();
        return;
      }

      this.SuspendLayout();
      //_tlp.SuspendLayout();
      //_tlp.Controls.Clear();
      while (_tlp.Controls.Count > 0)
        _tlp.Controls.RemoveAt(0);
      _tlp.RowCount = 0;



      #region Добавлено DSY 29.11.2007
      _tlp.Height = 0;
      _tlp.Controls.Add(_lblFilter, 0, 0);
      _tlp.SetColumnSpan(_lblFilter, 2);
      int row = 1;
      #endregion

//      int row = 0;
      foreach (Param prm in pPerf.pCurrentQuery.Params)
      {
        Control ctr = null;
        #region Create control
        switch (prm.Type)
        {
          case eQueryParamType.Boolean:
            {
              CheckBox chk = new CheckBox();
              chk.Checked = prm.ValBool;
              ctr = chk;
            }
            break;
          case eQueryParamType.Date:
            {
              DateTimePicker dtp = new DateTimePicker();
              dtp.Value = prm.ValDate;
              ctr = dtp;
            }
            break;
          case eQueryParamType.Integer:
          case eQueryParamType.String:
          case eQueryParamType.PlaceCode:
            {
              TextBox txt = new TextBox();
              txt.Text = prm.ValStr;
              ctr = txt;
            }
            break;
          /// --------------------- LIST ---------------------
          case eQueryParamType.IntSelectList:
            {
              ucParamCboInt cbo = new ucParamCboInt(pPerf);
              if (prm.CurrentValue.Length > 0)
              {
                cbo.Items.Add(new _ListBoxItem(prm.CurrentValue));
                cbo.SelectedIndex = 0;
              }
              cbo.BeforeFill += new EventHandler(cbo_BeforeFill);
              ctr = cbo;
            }
            break;
          case eQueryParamType.PCSelectList:
            {
              ucParamCboPC cbo = new ucParamCboPC(pPerf);
              if (prm.CurrentValue.Length > 0)
              {
                cbo.Items.Add(new _ListBoxPCItem(prm.CurrentValue));
                cbo.SelectedIndex = 0;
              }
              cbo.BeforeFill += new EventHandler(cbo_BeforeFill);
              ctr = cbo;
            }
            break;
          case eQueryParamType.StrSelectList:
            {
              ucParamCboStr cbo = new ucParamCboStr(pPerf);
              if (prm.CurrentValue.Length > 0)
              {
                cbo.Items.Add(new _ListBoxTextItem(prm.CurrentValue));
                cbo.SelectedIndex = 0;
              }
              cbo.BeforeFill += new EventHandler(cbo_BeforeFill);
              ctr = cbo;
            }
            break;
          /// --------------------- TREE ---------------------
          case eQueryParamType.IntSelectTree:
            {
              CASSelectFromTV sft = new CASSelectFromTV();
              ucIntParamTV tv = new ucIntParamTV(pPerf);
              tv.pSQLString = prm.SelectValue; // ReplaceCommonParameter(prm.SelectValue);
              sft.pTreeView = tv;
              /// Added M.Tor 25.06.2008:
              if (prm.CurrentValue.Length > 0)
              {
                PCTVItemData tid = new PCTVItemData();
                _ListBoxItem lbi = new _ListBoxItem(prm.CurrentValue);
                tid.pCode = lbi.code;
                tid.pText = lbi.text;
                sft.SetItem(0, 0, tid.pText);
                sft.pItemTreeNodeTag = tid;
              }
              sft.OnBeforeShowTV += new EventHandler(sft_OnBeforeShowTV);
              ctr = sft;
            }
            break;
          case eQueryParamType.StrSelectTree:
            {
              CASSelectFromTV sft = new CASSelectFromTV();
              sft.pIsMayBeWithoutRefbook = true;
              ucStringParamTV tv = new ucStringParamTV(pPerf);
              tv.pSQLString = prm.SelectValue; // ReplaceCommonParameter(prm.SelectValue);
              sft.pTreeView = tv;
              /// Added M.Tor 25.06.2008:
              if (prm.CurrentValue.Length > 0)
              {
                PCTVItemData tid = new PCTVItemData();
                _ListBoxTextItem lbi = new _ListBoxTextItem(prm.CurrentValue);
                tid.pPath = lbi.text1;
                tid.pText = lbi.text2;
                sft.SetItem(0, 0, tid.pText);
                sft.pItemTreeNodeTag = tid;
              }
              sft.OnBeforeShowTV += new EventHandler(sft_OnBeforeShowTV);
              ctr = sft;
            }
            break;
          case eQueryParamType.PCSelectTree:
            {
              CASSelectFromTV sft = new CASSelectFromTV();
              ucPCParamTV tv = new ucPCParamTV(pPerf);
              tv.pSQLString = prm.SelectValue; // ReplaceCommonParameter(prm.SelectValue);
              sft.pTreeView = tv;
              /// Added M.Tor 25.06.2008:
              if (prm.CurrentValue.Length > 0)
              {
                PCTVItemData tid = new PCTVItemData();
                _ListBoxPCItem lbi = new _ListBoxPCItem(prm.CurrentValue);
                tid.pPC = lbi.pPC;
                tid.pText = lbi.pText;
                sft.SetItem(0, 0, tid.pText);
                sft.pItemTreeNodeTag = tid;
              }
              sft.OnBeforeShowTV += new EventHandler(sft_OnBeforeShowTV);
              ctr = sft;
            }
            break;
        }
        #endregion
        if (ctr != null)
        {
          Label lbl = new Label();
          lbl.Text = prm.Title;
          lbl.TextAlign = ContentAlignment.MiddleRight;
          lbl.Tag = prm;
          //lbl.Dock = DockStyle.Fill;
          lbl.AutoSize = true;
          ctr.Tag = lbl;

          if ((ctr as DateTimePicker) == null)
            ctr.Dock = DockStyle.Fill;

          _tlp.Controls.Add(lbl, 0, row);
          _tlp.Controls.Add(ctr, 1, row++);

          if (ctr is CASSelectFromTV)
            (ctr as CASSelectFromTV).OnValueChanged += new EventHandler(ctr_TextChanged);
          else
            ctr.TextChanged += new EventHandler(ctr_TextChanged);
        }
      }
      _tlp.RowCount = row;
           

      if (_autoHeight)
      {
        #region Добавлено DSY 29.11.2007
        int startRowN = 0;
        if (pShowWordFilter && _tlp.RowCount <= 1)
          startRowN++;
        #endregion

        int rh = 0;
        for (int ii = startRowN; ii < _tlp.RowCount; ii++)
          rh += _tlp.GetRowHeights()[ii];
        _tlp.Visible = rh > 0;
        this.Height = rh;
      }

      //_tlp.ResumeLayout(true);
      this.ResumeLayout();
      this.PerformLayout();
    }

    public void BeforeExecute()
    {
      Param prm = null;

      foreach (Control ctr in _tlp.Controls)
      {
        if ((ctr as CheckBox) != null)
        {
          prm = (ctr.Tag as Label).Tag as Param;
          prm.ValBool = ((CheckBox)ctr).Checked;
        }
        else if ((ctr as DateTimePicker) != null)
        {
          prm = (ctr.Tag as Label).Tag as Param;
          if (prm.Type == eQueryParamType.Date)
            prm.ValDate = ((DateTimePicker)ctr).Value;
          else
            prm.CurrentValue = ((DateTimePicker)ctr).Value.ToString();
        }
        else if ((ctr as TextBox) != null)
        {
          prm = (ctr.Tag as Label).Tag as Param;
          prm.CurrentValue = ((TextBox)ctr).Text;
        }
        else if ((ctr as ucParamCbo) != null)
        {
          prm = (ctr.Tag as Label).Tag as Param;
          ucParamCbo cbo = ctr as ucParamCbo;
          cbo.SetParam(prm);
        }
        else if ((ctr as CASSelectFromTV) != null)
        {
          prm = (ctr.Tag as Label).Tag as Param;
          CASSelectFromTV sft = ctr as CASSelectFromTV;
          ucParamTV ptv = sft.pTreeView as ucParamTV;
          if (ptv != null)
            ptv.SetParam(sft.pItemTreeNodeTag as PCTVItemData, prm);
        }
      }
    }

    private void ctr_TextChanged(object sender, EventArgs e)
    {
      Param prm = (Param)((Label)((Control)sender).Tag).Tag;

      foreach (Control ctr in _tlp.Controls)
      {
        Label lbl = ctr as Label;
        if (lbl != null)
          continue;

        if (ctr.Equals(sender))
          continue;

        Param cp = (Param)((Label)ctr.Tag).Tag;
        if (cp.SelectValue.Length > 0 && cp.SelectValue.IndexOf(pPerf.pSes.ParamBegDelim + prm.Name + pPerf.pSes.ParamEndDelim) >= 0)
        {
          cp.CurrentValue = string.Empty;
          if (ctr is CASSelectFromTV)
            ((CASSelectFromTV)ctr).pTextBox.Text = string.Empty;
          else
            ctr.Text = string.Empty;
        }
      }

      if (ChangedParameter != null)
        ChangedParameter(this, new EventArgs());
    }

    private void cbo_BeforeFill(object sender, EventArgs e)
    {
      BeforeExecute();
    }

    private void sft_OnBeforeShowTV(object sender, EventArgs e)
    {
      BeforeExecute();
    }
  }
}
