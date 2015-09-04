using System;
//using System.Collections;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using CommandAS.Tools;
using CommandAS.Tools.Controls;

namespace CommandAS.QueryLib
{
  public abstract class ucParamCbo : ComboBox
  {
    public string pIntListDelim;

    protected Performer mPerf;

    public event EventHandler BeforeFill;

    public ucParamCbo(Performer aPerf)
    {
      mPerf = aPerf;
      pIntListDelim = "~";
      this.DropDown += new EventHandler(_DoDropDown);
    }

    protected abstract void mFillListFromDB(OleDbCommand aCmd);
    protected abstract void mFillListFromText(string aText);
    protected abstract void mSetCurrent(Param aPrm);
    public abstract void SetParam(Param aPrm);
    private void _DoDropDown(object sender, EventArgs e)
    {
      Param prm = null;
      Label lbl = this.Tag as Label;
      if (lbl != null)
        prm = lbl.Tag as Param;
      Load(prm);
    }

    public void Load(Param aPrm)
    {
      this.Items.Clear();
      try
      {
        string[] ssSQL = aPrm.SelectValue.Split(";".ToCharArray());
        int wasCount = 0;
        for (int ii = 0; ii < ssSQL.Length; ii++)
        {
          if (ssSQL[ii].Trim().Length == 0)
            continue;

          OleDbCommand cmd = mPerf.pWDB.NewOleDbCommand(ssSQL[ii].Trim());

          if (BeforeFill != null)
            BeforeFill(this, new EventArgs());
          /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
          /// BeforeExecute();                                         !!!!!!!!!!!!!!!!!!!!!!!!!!!!
          /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

          mPerf.QueryParam(cmd);
          wasCount = this.Items.Count;
          mFillListFromDB(cmd);
          if (this.Items.Count > wasCount)
          {
            mSetCurrent(aPrm);
          }
          else
          #region  если список пуст, то попытаемся разобрать его по строчно !!!
          {
            try
            {
              mFillListFromText(ssSQL[ii]);
              wasCount = this.Items.Count;
              if (this.Items.Count > wasCount)
              {
                mSetCurrent(aPrm);
              }
            }
            catch
            {
              this.Items.Clear();
            }
          }
          #endregion
        }
      }
#if DEBUG
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
#else
      catch { }
#endif
    }
  }

  public class ucParamCboInt : ucParamCbo
  {
    public ucParamCboInt(Performer aPerf)
      : base(aPerf)
    {
    }

    protected override void mFillListFromDB(OleDbCommand aCmd)
    {
      CommonListComboBox.FillListCodeTextFromDB(this, aCmd, false, false);
    }

    protected override void mFillListFromText(string aText)
    {
      string[] ss = aText.Split("\r\n".ToCharArray());
      for (int jj = 0; jj < ss.Length; jj++)
      {
        if (ss[jj].Trim().Length == 0)
          continue;
        string[] ss2 = ss[jj].Trim().Split(pIntListDelim.ToCharArray());
        this.Items.Add(new _ListBoxItem(CASTools.ConvertToInt32Or0(ss2[0]), ss2[1]));
      }
    }

    protected override void mSetCurrent(Param aPrm)
    {
      int code = aPrm.ValInt;
      if (code > 0)
        CommonListComboBox.SelectCode(this, code);
    }

    public override void SetParam(Param aPrm)
    {
      _ListBoxItem si = this.SelectedItem as _ListBoxItem;
      if (si != null)
        aPrm.CurrentValue = si.pCodeText;
      else
        aPrm.CurrentValue = "0";
    }
  }

  public class ucParamCboPC : ucParamCbo
  {
    public ucParamCboPC(Performer aPerf)
      : base(aPerf)
    {
    }

    protected override void mFillListFromDB(OleDbCommand aCmd)
    {
      CommonListComboBox.FillListPCTextFromDB(this, aCmd, false);
    }

    protected override void mFillListFromText(string aText)
    {
      string[] ss = aText.Split("\r\n".ToCharArray());
      for (int jj = 0; jj < ss.Length; jj++)
      {
        if (ss[jj].Trim().Length == 0)
          continue;
        string[] ss2 = ss[jj].Trim().Split(pIntListDelim.ToCharArray());
        this.Items.Add(new _ListBoxPCItem(PlaceCode.PDC2PlaceCode(ss2[0]), ss2[1]));
      }
    }

    protected override void mSetCurrent(Param aPrm)
    {
      PlaceCode pc = aPrm.ValPC;
      if (pc.IsDefined)
        CommonListComboBox.SelectPC(this, pc);
    }

    public override void SetParam(Param aPrm)
    {
      _ListBoxPCItem si = this.SelectedItem as _ListBoxPCItem;
      if (si != null)
        aPrm.CurrentValue = si.pPCText;
      else
        aPrm.CurrentValue = PlaceCode.PlaceCode2PDC(PlaceCode.Empty);
    }
  }

  public class ucParamCboStr : ucParamCbo
  {
    public ucParamCboStr(Performer aPerf)
      : base(aPerf)
    {
    }

    protected override void mFillListFromDB(OleDbCommand aCmd)
    {
      OleDbDataReader dReader = null;
      try
      {
        dReader = aCmd.ExecuteReader();
        while (dReader.Read())
          this.Items.Add(new _ListBoxTextItem(dReader[0].ToString(), dReader[1].ToString()));
      }
      catch { }
      finally
      {
        if (dReader != null)
          dReader.Close();
      }
    }

    protected override void mFillListFromText(string aText)
    {
      string[] ss = aText.Split("\r\n".ToCharArray());
      for (int ii = 0; ii < ss.Length; ii++)
      {
        if (ss[ii].Trim().Length == 0)
          continue;
        string[] ss2 = ss[ii].Trim().Split(pIntListDelim.ToCharArray());
        this.Items.Add(new _ListBoxTextItem(ss2[0], ss2[1]));
      }
    }

    protected override void mSetCurrent(Param aPrm)
    {
      if (aPrm.CurrentValue.Length > 0)
        this.SelectedItem = aPrm.CurrentValue;
    }

    public override void SetParam(Param aPrm)
    {
      _ListBoxTextItem si = this.SelectedItem as _ListBoxTextItem;
      if (si != null)
        aPrm.CurrentValue = si.pTextText;
      else
        aPrm.CurrentValue = string.Empty;
    }
  }

}
