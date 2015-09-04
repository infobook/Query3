using System;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using CommandAS.Tools;
using CommandAS.Tools.Controls;


namespace CommandAS.QueryLib
{

  /// <summary>
  /// Данные хранящиеся в таге (TreeNode.Tag)
  /// каждого элемента дерева (TreeView).
  /// </summary>
  public class PCTVItemData : CASTreeItemData
  {
    public string pText;
    public string pPath;
    public ArrayList pLPA;

    public PCTVItemData()
    {
      pText = string.Empty;
      pPath = string.Empty;
      pLPA = new ArrayList();
    }
  }

  public abstract class ucParamTV : CASTreeViewStep
  {
    protected const string PARAM_NAME_ICON_INDEX = "_ITEMIMAGE";
    protected const string PARAM_NAME_ITEM_COLOR = "_ITEMCOLOR";

    protected Performer mPerf;
    protected ArrayList mLocParams;
    protected string[] mSQLExps;
    protected bool mIsShowEmptyItem;

    public bool pIsShowEmptyItem
    {
      get { return mIsShowEmptyItem; }
      set { mIsShowEmptyItem = value; }
    }

    public string pSQLString
    {
      set
      {
        pErr.Clear();
        mLocParams.Clear();
        pIsImageDefined = false;

        string[] ss = value.Split(";".ToCharArray());

        try
        {
          if (ss.Length < 2)
            throw new Exception();

          #region parse PARAM RANGE
          if (ss[0].Trim().Length > 0)
          {
            string[] ssp = ss[0].Split(Environment.NewLine.ToCharArray());
            for (int si = 0; si < ssp.Length; si++)
            {
              if (ssp[si].Trim().Length == 0)
                continue;

              //string[] sspp = ssp[si].Split(" ".ToCharArray());
              StringBuilder sb = new StringBuilder(ssp[si].Trim());
              string prmName = string.Empty;
              string prmType = string.Empty;
              string prmVal = string.Empty;
              bool isPrevSpace = false;
              int beg = 0;
              for (int ii = 0; ii < sb.Length; ii++)
              {
                if (sb[ii].Equals(' '))
                {
                  if (isPrevSpace)
                    continue;
                  isPrevSpace = true;
                  if (prmName.Length == 0)
                  {
                    prmName = sb.ToString(beg, ii - beg);
                    beg = ii + 1;
                  }
                  else // if (prmType.Length == 0)
                  {
                    prmType = sb.ToString(beg, ii - beg);
                    prmVal = sb.ToString(ii + 1, sb.Length - ii - 1);
                    break;
                  }
                }
                else
                  isPrevSpace = false;
              }
              if (prmName.Length > 0)
              {
                Param prm = new Param();
                prm.Name = prmName;
                if (prmType.Equals("i"))
                  prm.Type = eQueryParamType.Integer;
                else if (prmType.Equals("d"))
                  prm.Type = eQueryParamType.Date;
                else
                  prm.Type = eQueryParamType.String;
                prm.CurrentValue = prmVal;
                prm.DefaultValue = prmVal;
                mLocParams.Add(prm);

                if (!pIsImageDefined)
                  pIsImageDefined = prm.Name.ToUpper().Equals(PARAM_NAME_ICON_INDEX);
              }
            }
          }
          #endregion end parse PARAM RANGE

          #region parse SQL RANGE

          mSQLExps = new string[ss.Length - 1];
          for (int pi = 1; pi < ss.Length; pi++)
            mSQLExps[pi - 1] = ss[pi].Trim();

          #endregion end parse SQL RANGE
        }
        catch
        {
          pErr.text = "НЕПРАВИЛЬНЫЙ ФОРМАТ SQL запроса  для иерархического параметра!!!";
        }
      }
    }

    public bool pIsImageDefined;

    public Query pCurrentQuery;

    public ucParamTV(Performer aPerf)
    {
      mPerf = aPerf;
      mLocParams = new ArrayList(2);
      mSQLExps = new string[] { };
      mIsShowEmptyItem = false;
      pIsImageDefined = false;
      pCurrentQuery = null;
    }

    protected abstract TreeNode AddNodeItem(TreeNode aTn, OleDbDataReader aReader, ref int aSI);
    public abstract void SetParam(PCTVItemData aTID, Param aPrm);

    public Param GetParamValueSelectedNode(string aName)
    {
      return GetParamValue(SelectedNode, aName);
    }
    public Param GetParamValue(TreeNode aTn, string aName)
    {
      if (aTn == null)
        return null;

      PCTVItemData tid = aTn.Tag as PCTVItemData;
      if (tid == null)
        return null;

      int ii = 0;
      foreach (Param prm in mLocParams)
      {
        if (prm.Name.Equals(aName) && ii < tid.pLPA.Count)
        {
          Param retPrm = new Param();
          retPrm.Type = prm.Type;
          if (prm.Type == eQueryParamType.Integer)
            retPrm.ValInt = (int)tid.pLPA[ii];
          else if (prm.Type == eQueryParamType.Date)
            retPrm.ValDate = (DateTime)tid.pLPA[ii];
          else
            retPrm.ValStr = (string)tid.pLPA[ii];
          return retPrm;
        }
        ii++;
      }
      return null;
    }

    public PCTVItemData GetSelectedNodeTag()
    {
      PCTVItemData tid = null;
      if (SelectedNode != null)
        tid = SelectedNode.Tag as PCTVItemData;
      return tid;
    }

    protected override void BeforeExpandNode(TreeNode aTn)
    {
      if (mPerf == null)
        return;

      if (aTn != null && aTn.Nodes.Count == 0)
        return;

      if (pCurrentQuery != null)
        mPerf.pCurrentQuery = pCurrentQuery;

      this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
      BeginUpdate();

      int lvl = aTn == null ? 0 : CASTreeViewBase.Level(aTn);
      try
      {
        string sql = mSQLExps.Length > 0
          ? (lvl < mSQLExps.Length ? mSQLExps[lvl] : mSQLExps[mSQLExps.Length - 1]) 
          : string.Empty;
        OleDbCommand cmd = mPerf.pWDB.NewOleDbCommand(sql);

        if (aTn != null)
        {
          aTn.Nodes.Clear();
          PCTVItemData tid = (PCTVItemData)aTn.Tag;
          try
          {
            int ii = 0;
            foreach (Param prm in mLocParams)
            {
              if (prm.Type == eQueryParamType.Integer)
                prm.ValInt = (int)tid.pLPA[ii++];
              else if (prm.Type == eQueryParamType.Date)
                prm.ValDate = (DateTime)tid.pLPA[ii++];
              else
                prm.ValStr = (string)tid.pLPA[ii++];
            }
          }
          catch { }
        }
        else
        {
          foreach (Param prm in mLocParams)
            prm.CurrentValue = prm.DefaultValue;
        }


        mPerf.QueryParam(cmd, mLocParams);

        OleDbDataReader dtReader = null;
        try
        {
          dtReader = cmd.ExecuteReader();
          TreeNode tn = null;
          while (dtReader.Read())
          {
            int si = 0;
            tn = AddNodeItem(aTn, dtReader, ref si);
            PCTVItemData tid = (PCTVItemData)tn.Tag;
            foreach (Param prm in mLocParams)
            {
              try
              {
                if (prm.Type == eQueryParamType.Integer)
                {
                  if (prm.Name.ToUpper().Equals(PARAM_NAME_ICON_INDEX))
                  {
                    tn.ImageIndex = dtReader.GetInt32(si);
                    tn.SelectedImageIndex = tn.ImageIndex;
                  }
                  if (prm.Name.ToUpper().Equals(PARAM_NAME_ITEM_COLOR) && dtReader.GetInt32(si) > 0)
                    tn.ForeColor = System.Drawing.Color.FromArgb(dtReader.GetInt32(si));

                  tid.pLPA.Add(dtReader.GetInt32(si++));
                }
                else if (prm.Type == eQueryParamType.Date)
                {
                  tid.pLPA.Add(dtReader.GetDateTime(si++));
                }
                else
                {
                  tid.pLPA.Add(dtReader.GetString(si++));
                }
              }

#if DEBUG
              catch (Exception ex) 
              { 
                tn.Text += " (err: "+ex.Message+")"; 
              }
#else
              catch { }
#endif
            }
            if (tn == null && mIsShowEmptyItem && aTn != null)
              AddEmptyNodeItem(aTn);
          }
        }
        catch (Exception e)
        {
          pErr.ex = e;
        }
        finally
        {
          if (dtReader != null)
            dtReader.Close();
        }
      }
      finally
      {
        EndUpdate();
        this.Cursor = System.Windows.Forms.Cursors.Default;
      }
    }

    protected virtual void AddEmptyNodeItem(TreeNode aTn)
    {
      aTn.Nodes.Add(pEmptyItemText);
    }
  }

  public class ucIntParamTV : ucParamTV
  {
    public ucIntParamTV(Performer aPerf)
      : base(aPerf)
    {
    }

    public override void SetParam(PCTVItemData aTID, Param aPrm)
    {
      //if (aTID != null)
      //  aPrm.CurrentValue = aTID.pPC.code.ToString();
      //else
      //  aPrm.CurrentValue = "0";
      /// Modified by M.Tor 25.06.2008:
      if (aTID != null)
        aPrm.CurrentValue = aTID.pPC.code.ToString() + PlaceCode.DELIM.ToString() + aTID.pText;
      else
        aPrm.CurrentValue = "0:";
    }

    protected override TreeNode AddNodeItem(TreeNode aTn, OleDbDataReader aReader, ref int aSI)
    {
      /// В pSQLString должен быть определен примерно такой select:
      /// ...
      /// SELECT 
      ///		refCode, refBName, 1, ...
      /// FROM 
      ///		rbRef
      /// ...

      TreeNode ntn = new TreeNode(aReader.GetString(1));
      PCTVItemData tid = new PCTVItemData();
      ntn.Tag = tid;
      tid.pPC.code = aReader.GetInt32(0);
      /// Added M.Tor 25.06.2008:
      tid.pText = aReader.GetString(1);
      if (aReader.GetInt32(2) > 0)
        ntn.Nodes.Add(new TreeNode());

      if (aTn == null)
      {
        Nodes.Add(ntn);
        tid.pPath = tid.pCode.ToString();
      }
      else
      {
        aTn.Nodes.Add(ntn);
        tid.pPath = ((PCTVItemData)aTn.Tag).pPath + pPathDelim + tid.pCode;
      }

      aSI = 3;

      return ntn;
    }
  }

  public class ucStringParamTV : ucParamTV
  {
    public ucStringParamTV(Performer aPerf)
      : base(aPerf)
    {
    }

    public override void SetParam(PCTVItemData aTID, Param aPrm)
    {
      //if (aTID != null)
      //  aPrm.CurrentValue = aTID.pText;
      //else
      //  aPrm.CurrentValue = string.Empty;
      /// Modified by M.Tor 25.06.2008:
      if (aTID != null)
        aPrm.CurrentValue = aTID.pPath + PlaceCode.DELIM.ToString() + aTID.pText;
      else
        aPrm.CurrentValue = PlaceCode.DELIM.ToString();
    }

    protected override TreeNode AddNodeItem(TreeNode aTn, OleDbDataReader aReader, ref int aSI)
    {
      /// В pSQLString должен быть определен примерно такой select:
      /// ...
      /// SELECT
      ///		refPath, refBName, 1, ...
      /// FROM
      ///		rbRef
      /// ...

      TreeNode ntn = new TreeNode(aReader.GetString(1));
      PCTVItemData tid = new PCTVItemData();
      ntn.Tag = tid;
      //tid.pText = aReader.GetString(0);
      /// Modified by M.Tor 25.06.2008:
      tid.pPath = aReader.GetString(0);
      tid.pText = aReader.GetString(1);
      if (aReader.GetInt32(2) > 0)
        ntn.Nodes.Add(new TreeNode());

      if (aTn == null)
      {
        Nodes.Add(ntn);
        //tid.pPath = tid.pText;
        /// Modified by M.Tor 25.06.2008:
      }
      else
      {
        aTn.Nodes.Add(ntn);
        //tid.pPath = ((PCTVItemData)aTn.Tag).pPath + pPathDelim + tid.pText;
        /// Modified by M.Tor 25.06.2008:
      }

      aSI = 3;

      return ntn;
    }
  }

  public class ucPCParamTV : ucParamTV
  {
    public ucPCParamTV(Performer aPerf)
      : base(aPerf)
    {
    }

    public override void SetParam(PCTVItemData aTID, Param aPrm)
    {
      //if (aTID != null)
      //  aPrm.CurrentValue = PlaceCode.PlaceCode2PDC(aTID.pPC);
      //else
      //  aPrm.CurrentValue = PlaceCode.PlaceCode2PDC(PlaceCode.Empty);
      /// Modified by M.Tor 25.06.2008:
      if (aTID != null)
        aPrm.CurrentValue = PlaceCode.PlaceCode2PDC(aTID.pPC) + PlaceCode.DELIM.ToString() + aTID.pText;
      else
        aPrm.CurrentValue = PlaceCode.PlaceCode2PDC(PlaceCode.Empty) + PlaceCode.DELIM.ToString();
    }

    protected override TreeNode AddNodeItem(TreeNode aTn, OleDbDataReader aReader, ref int aSI)
    {
      /// В pSQLString должен быть определен примерно такой select:
      /// ...
      /// SELECT 
      ///		objPlace, objCode, objName, CASE WHEN objTypeR=1420 THEN 0 ELSE 1 END, ...
      /// FROM 
      ///		iNode INNER JOIN iObj ON nodPObj=objPlace AND nodCObj=objCode
      /// ...

      TreeNode ntn = new TreeNode(aReader.GetString(2));
      PCTVItemData tid = new PCTVItemData();
      ntn.Tag = tid;
      tid.pPC.place = aReader.GetInt32(0);
      tid.pPC.code = aReader.GetInt32(1);
      /// Added M.Tor 25.06.2008:
      tid.pText = aReader.GetString(2);
      if (aReader.GetInt32(3) > 0)
        ntn.Nodes.Add(new TreeNode());

      if (aTn == null)
      {
        Nodes.Add(ntn);
        tid.pPath = tid.pCode.ToString();
      }
      else
      {
        aTn.Nodes.Add(ntn);
        tid.pPath = ((PCTVItemData)aTn.Tag).pPath + pPathDelim + tid.pCode; 
      }

      aSI = 4;

      return ntn;
    }
  }
}
