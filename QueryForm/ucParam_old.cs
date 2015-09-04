using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using CommandAS.Tools;
using CommandAS.Tools.Controls;
using CommandAS.QueryLib;

namespace CommandAS.QueryForm
{
  /// <summary>
  /// Summary description for ucParam.
  /// </summary>
  public class ucParam : System.Windows.Forms.UserControl
  {
    private const int IND = 5;
    //private Query											_cq;
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public string pIntListDelim;
    public Task pTask;

    public ucParam()
    {
      //pConnStr = string.Empty;
      pIntListDelim = "~";
      pTask = null;
      //_cq = null;

      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();
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

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      // 
      // ucParam
      // 
      this.Name = "ucParam";
      this.Size = new System.Drawing.Size(776, 216);

    }
    #endregion

    public void ShowParam() //Query aQr)
    {
      BeforeExecute(); // сохраним предыдущие значения параметров !!!

      this.Controls.Clear();

      //_cq = aQr;
      //pTask.pCurrentQuery

      if (pTask.pCurrentQuery == null)
      {
        Refresh();
        return;
      }

      Label lbl;
      foreach (Param prm in pTask.pCurrentQuery.Params)
      {
        lbl = new Label();
        lbl.Text = prm.Title;
        lbl.TextAlign = ContentAlignment.MiddleRight;
        lbl.Tag = prm;
        this.Controls.Add(lbl);

        Control ctr = null;
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
              ucParamCboInt cbo = new ucParamCboInt(pTask);
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
              ucParamCboPC cbo = new ucParamCboPC(pTask);
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
              ucParamCboStr cbo = new ucParamCboStr(pTask);
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
              sft.OnBeforeShowTV += new EventHandler(sft_OnBeforeShowTV);
              ucIntParamTV tv = new ucIntParamTV(pTask);
              tv.pSQLString = prm.SelectValue; // ReplaceCommonParameter(prm.SelectValue);
              sft.pTreeView = tv;
              ctr = sft;
            }
            break;
          case eQueryParamType.StrSelectTree:
            {
              CASSelectFromTV sft = new CASSelectFromTV();
              sft.OnBeforeShowTV += new EventHandler(sft_OnBeforeShowTV);
              ucStringParamTV tv = new ucStringParamTV(pTask);
              tv.pSQLString = prm.SelectValue; // ReplaceCommonParameter(prm.SelectValue);
              sft.pTreeView = tv;
              ctr = sft;
            }
            break;
          case eQueryParamType.PCSelectTree:
            {
              CASSelectFromTV sft = new CASSelectFromTV();
              sft.OnBeforeShowTV += new EventHandler(sft_OnBeforeShowTV);
              ucPCParamTV tv = new ucPCParamTV(pTask);
              tv.pSQLString = prm.SelectValue; // ReplaceCommonParameter(prm.SelectValue);
              sft.pTreeView = tv;
              ctr = sft;
            }
            break;
        }
        if (ctr != null)
        {
          ctr.Tag = lbl;
          this.Controls.Add(ctr);
          ctr.TextChanged += new EventHandler(ctr_TextChanged);
        }
      }

      //Refresh();
      OnResize(new EventArgs());
    }

    public void BeforeExecute()
    {
      Param prm = null;

      foreach (Control ctr in Controls)
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
          {
            if (prm.SelectValue.Length == 0)
              prm.CurrentValue = ((DateTimePicker)ctr).Value.ToShortDateString();
            else
              prm.CurrentValue = ((DateTimePicker)ctr).Value.ToString(prm.SelectValue);
          }
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

    protected override void OnResize(EventArgs e)
    {
      base.OnResize(e);

      int X1 = IND;
      int X2 = Width / 2 + IND;
      int Y = IND;
      int W1 = X2 - 2 * IND - X1;
      int W2 = W1; // вдруг захочится не пополам !!!
      int H = 22;

      foreach (Control ctr in Controls)
      {
        if ((ctr as Label) != null)
        {
          ctr.Location = new System.Drawing.Point(X1, Y);
          ctr.Size = new System.Drawing.Size(W1, H);
        }
        else
        {
          ctr.Location = new System.Drawing.Point(X2, Y);
          ctr.Size = new System.Drawing.Size(W2, H);
          Y += ctr.Height + IND;
        }
      }
    }

    //		private string ReplaceCommonParameter(string aSQL)
    //		{
    //			if (pTask.pCommonParamCollection != null)
    //			{
    //				foreach (Param prm in pTask.pCommonParamCollection)
    //					aSQL = aSQL.Replace(pTask.pParamBegDelim+prm.Name+pTask.pParamEndDelim, prm.CurrentValue);
    //			}
    //			return aSQL;
    //		}

    private void ctr_TextChanged(object sender, EventArgs e)
    {
      Param prm = (Param)((Label)((Control)sender).Tag).Tag;

      foreach (Control ctr in Controls)
      {
        Label lbl = ctr as Label;
        if (lbl != null)
          continue;

        if (ctr.Equals(sender))
          continue;

        Param cp = (Param)((Label)ctr.Tag).Tag;
        if (cp.SelectValue.Length > 0 && cp.SelectValue.IndexOf(pTask.pParamBegDelim + prm.Name + pTask.pParamEndDelim) >= 0)
        {
          cp.CurrentValue = string.Empty;
          ctr.Text = string.Empty;
        }
      }
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
