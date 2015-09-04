using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Collections;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Drawing;
using CommandAS.Tools;

namespace CommandAS.QueryLib
{
  public enum eSQLQueryType
  {
    Select = 0,
    Insert = 1,
    Update = 2,
    Delete = 3
  }

  /// <summary>
  /// ����������� �������� ������.
  /// </summary>
  /// <datecreate>24.08.2005</datecreate>
  /// <author>M.Tor</author>
  public class Performer
  {
    /// <summary>
    /// HISTORY
    /// 3.2 - �������� ����� ����������� � XSLT/HTML
    /// 3.3 - ������ ���������������� � XSLT ������� ��������� ������
    /// 3.4.1 - �������� ������ � "����������" ����������
    /// </summary>
    public const string VERSION = "3.4.1";
    public const string RESULT_TAB_NAME = "Query2Table";
    public const string RESULT_SET_NAME = "Query2DS";
    public const string CURRENT_SESSION_EXT = ".sq3";
    public const string CURRENT_QUERY_EXT = ".q3";
    public const string EXECUTE_NO_QUERY_MARK = "ExecuteNonQuery";
    public const string COL_IMG_DATA = "aCol_Img_Data";
    public const string COL_IMG_FILE = "aCol_Img_File";
    public const string COL_ZIP_DATA = "aCol_Zip_Data";
    public const string COL_UNZ_DATA = "aCol_Unz_Data";
    public const string COL_BIN_DATA = "aCol_Bin_Data";
    public const string COL_BIN_FILE = "aCol_Bin_File";
    public const string EXEC_SUBQUERY_nn = "EXEC_SUBQUERY_";

    #region PROPERTY

    protected TemporaryFiles mTempFiles;

    protected string mTempPath;

    /// <summary>
    /// ������
    /// </summary>
    protected Session mSes;
    /// <summary>
    /// ����� ������ ����������� �������
    /// </summary>
    protected DataSet mDS;
    /// <summary>
    /// ������� ����������� �������
    /// </summary>
    protected DataTable mTab;
    /// <summary>
    /// ��������� �� ������ �� ������ � ������� �������� �����������
    /// </summary>
    protected Query mCQ;
    /// <summary>
    /// ����� � ��
    /// </summary>
    protected WorkDB mDB;
    protected Error mErr;

    protected string mTempXMLFileName
    {
      get
      {
        return
          pTempPath +
          Path.DirectorySeparatorChar +
          mCQ.Name.Replace(@"\", "_") +
          ".xml";
      }
    }

    protected TimeSpan mExecTime;

    /// <summary>
    /// ��� ����� ������
    /// </summary>
    public string pFileName;
    /// <summary>
    /// ���� � ����� (folder) ��� ��������� ������
    /// </summary>
    public string pTempPath
    {
      get { return mTempPath; }
      set
      {
        mTempPath = CASTools.PathWithEndSeparator(value);
        //if (value.Length > 1 && (value.ToCharArray()[value.Length-1] != Path.DirectorySeparatorChar))
        //	mTempPath += Path.DirectorySeparatorChar;
      }
    }
    /// <summary>
    /// ��������� �������� ������
    /// </summary>
    public ArrayList pQueries
    {
      get { return mSes.Queries; }
    }
    /// <summary>
    /// ������
    /// </summary>
    public Session pSes
    {
      get { return mSes; }
    }
    /// <summary>
    /// ��������� �� ������ �� ������ � ������� �������� �����������
    /// </summary>
    public Query pCurrentQuery
    {
      get { return mCQ; }
      set { mCQ = value; }
    }
    /// <summary>
    /// ���-�� �������� � ������
    /// </summary>
    public int pCountQuery
    {
      get
      {
        int ret = 0;
        if (mSes != null && mSes.Queries != null)
          ret = mSes.Queries.Count;
        return ret;
      }
    }
    /// <summary>
    /// ������� ��.
    /// </summary>
    public WorkDB pWDB
    {
      get { return mDB; }
    }
    /// <summary>
    /// ����� � ��
    /// </summary>
    public OleDbConnection pDBConnection
    {
      [DebuggerStepThrough]
      get { return mDB.pDBConnection; }
      [DebuggerStepThrough]
      set { mDB.pDBConnection = value; }
    }
    /// <summary>
    /// ������ ����������
    /// </summary>
    public Error pError
    {
      get { return mErr; }
    }

    public DataSet pResultSet
    {
      get { return mDS; }
    }
    public DataTable pResultTable
    {
      [DebuggerStepThrough]
      get
      {
        if (mTab == null)
          return new DataTable(RESULT_TAB_NAME);
        else
          return mTab;
      }
    }
    public string pResultMessage;

    public ArrayList pCommonParamCollection
    {
      get
      {
        ArrayList al = null;
        if (mSes != null)
          al = mSes.Params;
        return al;
      }
    }

    /// <summary>
    /// ����� ���������� �������.
    /// </summary>
    public TimeSpan pExecTime
    {
      get { return mExecTime; }
    }

    #endregion property

    public Performer() : this(new WorkDB()) { }
    public Performer(WorkDB aDB)
    {
      mSes = null;
      mDB = aDB;
      //pCommonParamCollection = null;

      mDS = null; //new DataSet(RESULT_SET_NAME);
      mTab = null; //new DataTable(RESULT_TAB_NAME);
      mErr = new Error();

      mTempFiles = new TemporaryFiles();
      pTempPath = Path.GetTempPath();
      pFileName = string.Empty;
      pResultMessage = string.Empty;

      mExecTime = new TimeSpan(0);
    }
    /// <summary>
    /// �������� ������ �� ����� [aFileName].
    /// pFileName = aFileName;
    /// </summary>
    /// <param name="aFileName">������ ��� ����� ������.</param>
    /// <returns>
    ///	true - ������ ������� ���������;
    ///	false - �� ���������, ������ � pError.
    /// </returns>
    public bool Load(string aFileName)
    {
      pFileName = aFileName;
      return Load();
    }
    /// <summary>
    /// �������� ������ �� ����� [pFileName].
    /// </summary>
    /// <returns>
    ///	true - ������ ������� ���������;
    ///	false - �� ���������, ������ � pError.
    /// </returns>
    public bool Load()
    {
      mErr.Clear();

      if (pFileName.Length == 0)
      {
        mErr.text = "�� ���������� ������ (��� �����)!";
        return false;
      }

      try
      {
        FileStream f = new FileStream(pFileName, FileMode.Open, FileAccess.Read);

        /// ���������� ������ ������ �� ����������:
        string ext = Path.GetExtension(pFileName).ToLower();
        if (ext.Equals(Performer.CURRENT_SESSION_EXT))
        { // ������� ������
          XmlSerializer xs = new XmlSerializer(typeof(Session));
          mSes = (Session)xs.Deserialize(f);
        }
        else if (ext.Equals(".sq2"))
        { // ����������� �� ������ 2 � ������� ������
          XmlSerializer xs = new XmlSerializer(typeof(CommandAS.QueryLib2.Session));
          CommandAS.QueryLib2.Session ses2 = (CommandAS.QueryLib2.Session)xs.Deserialize(f);
          mSes = new Session(ses2);
        }

        f.Close();
      }
      catch (Exception ex)
      {
        mErr.ex = ex;
      }

      return mErr.IsOk;
    }

    /// <summary>
    /// �������� ��� ����������  ��� ��� ������ ������ ������.
    /// </summary>
    /// <param name="aQueryCode">��� �������</param>
    /// <returns></returns>
    public bool IsProhibition(int aQueryCode)
    {
      return mDB.IsProhibition(aQueryCode);
    }

    /// <summary>
    /// ���������� ������� ������ �� ��������� ������ �� ����� [aName].
    /// </summary>
    /// <param name="aName">��� �������.</param>
    /// <returns>
    ///	true - ������ ����������;
    ///	false - �� ���������� (�� ������ ������).
    /// </returns>
    public bool SetCurrentQueryByName(string aName)
    {
      if (mSes != null)
        foreach (Query qq in mSes.Queries)
          if (qq.Name.Equals(aName))
          {
            mCQ = qq;
            return true;
          }

      //mCQ = null;
      return false;
    }
    /// <summary>
    /// ���������� ������� ������ �� ��������� ������ �� ���� [aCode].
    /// </summary>
    /// <param name="aCode">��� �������</param>
    /// <returns>
    ///	true - ������ ����������;
    ///	false - �� ���������� (�� ������ ������).
    /// </returns>
    public bool SetCurrentQueryByCode(int aCode)
    {
      Query qq = FoundQueryByCode(aCode);
      if (qq != null)
      {
        mCQ = qq;
        return true;
      }
      return false;
    }

    public Query FoundQueryByCode(int aCode)
    {
      Query ret = null;
      if (mSes != null)
      {
        foreach (Query qq in mSes.Queries)
          if (qq.Code == aCode)
          {
            ret = qq;
            break;
          }
      }
      return ret;
    }

    /// <summary>
    /// ���������� �������� [aValue] ��������� [aParamName].
    /// </summary>
    /// <param name="aParamName">��� ���������.</param>
    /// <param name="aValue">��������.</param>
    /// <returns>
    ///	true - �������� �����������;
    ///	false - �������� �� ����������� (���� �� ������ ��������, ���� ��� �������� �������).
    /// </returns>
    public bool SetCurrentQueryParam(string aParamName, string aValue)
    {
      Param prm = GetCurrentQueryParam(aParamName);

      if (prm != null)
      {
        prm.CurrentValue = aValue;
        return true;
      }
      else
        return false;
    }

    public Param GetCurrentQueryParam(string aParamName)
    {
      if (mCQ == null)
        return null;

      foreach (Param pp in mCQ.Params)
        if (pp.Name.Equals(aParamName))
          return pp;

      return null;
    }

    /// <summary>
    /// ��������� ������ aQuery ��� ���������� aCn. 
    /// ��� ������� ���� eSQLQueryType.Select ��������� ���������� � ������� mTab.
    /// pDBConnection=aCn
    /// pCurrentQuery=aQuery. 
    /// ������ ����������� ������ ���������� ���������� pDBConnection.
    /// </summary>
    /// <param name="aCn">����������.</param>
    /// <param name="aQuery">������.</param>
    /// <returns>
    ///	true - ������ �������� �������;
    ///	false - ������ �� ��������, ������. ������ � pError.
    /// </returns>
    public bool Execute(OleDbConnection aCn, Query aQuery)
    {
      mDB.pDBConnection = aCn;
      mCQ = aQuery;
      return Execute();
    }
    /// <summary>
    /// ��������� ������ aQuery � ��� ���� eSQLQueryType.Select 
    /// �������� ���������  � ������� mTab. pCurrentQuery=aQuery.
    /// ������ ����������� ������ ���������� ���������� pDBConnection.
    /// </summary>
    /// <param name="aQuery">������.</param>
    /// <returns>
    ///	true - ������ �������� �������;
    ///	false - ������ �� ��������, ������. ������ � pError.
    /// </returns>
    public bool Execute(Query aQuery)
    {
      mCQ = aQuery;
      return Execute();
    }
    /// <summary>
    /// ��������� ������ pCurrentQuery � ��� ���� eSQLQueryType.Select 
    /// �������� ���������  � ������� mTab.
    /// ������ ����������� ������ ���������� ���������� pDBConnection.
    /// </summary>
    /// <returns>
    ///	true - ������ �������� �������;
    ///	false - ������ �� ��������, ������. ������ � pError.
    /// </returns>
    public bool Execute()
    {
      mErr.Clear();

      if (mDB.pDBConnection == null)
      {
        mErr.text = "�� ����������� ���������� � ��.";
        return false;
      }
      if (mCQ == null)
      {
        mErr.text = "�� ��������� ������ �� ����������.";
        return false;
      }

      pResultMessage = string.Empty;
      DateTime dtBeg = DateTime.Now;
      mExecTime = new TimeSpan(0);

      mDS = new DataSet(RESULT_SET_NAME);

      mDB.TransactionBegin();
      try
      {
        mExecute(mDS, mCQ);
        mDB.TransactionCommit();

        if (mDS.Tables.Count > 0)
          mTab = mDS.Tables[0];
        else
          mTab = null;
      }
      catch (Exception ex)
      {
        mDB.TransactionRollback();
        mErr.ex = ex;
      }

      mExecTime = DateTime.Now.Subtract(dtBeg);

      return mErr.IsOk;
    }
    protected void mExecute(DataSet aDS, Query aQr)
    {
      OleDbCommand cmd = mDB.NewOleDbCommand(aQr.Text);

      #region  ��������� DSY 20.02.2008
      cmd.CommandTimeout = 120;   //  2 ����� ������ �������
      #endregion

      bool isSelect = (GetSQLQueryType(aQr.Text) == eSQLQueryType.Select);
      if (!isSelect)
        cmd.CommandText = cmd.CommandText.Replace(mSes.ParamBegDelim + EXECUTE_NO_QUERY_MARK + mSes.ParamEndDelim, string.Empty);

      ArrayList paramCol = mGetQueryParamCol(aQr, null);

      #region handling EXEC_SUBQUERY_nn param1, param2, ..., paramN

      int pos = 0;
      int len = EXEC_SUBQUERY_nn.Length;
      while ((pos = cmd.CommandText.IndexOf(EXEC_SUBQUERY_nn, pos)) != -1)
      {
        int cp = pos + len;
        // define query by code
        int cp2 = cmd.CommandText.IndexOf("(", cp);
        int qCode = CASTools.ConvertToInt32Or0(cmd.CommandText.Substring(cp, cp2 - cp).Trim());
        if (qCode == 0)
        {
          pos = cp;
          continue;
        }
        Query qq = FoundQueryByCode(qCode);
        if (qq != null)
        {
          // define qyery param
          cp = cp2 + 1;
          cp2 = cmd.CommandText.IndexOf(")", cp);
          string[] ss = cmd.CommandText.Substring(cp, cp2 - cp).Trim().Split(",".ToCharArray());
          for (int ii = 0; ii < ss.Length; ii++)
          {
            string sprm = ss[ii].Trim();
            if (sprm.Length == 0)
              continue;

            bool isVal = true;
            foreach (Param prm in paramCol)
            {
              if (sprm.Equals(mSes.ParamBegDelim + prm.Name + mSes.ParamEndDelim))
              {
                Param qp = (Param)qq.Params[ii];
                qp.CurrentValue = string.Empty;
                if (qp.Type == prm.Type)
                  qp.CurrentValue = prm.CurrentValue;
                else
                {
                  switch (qp.Type)
                  {
                    case eQueryParamType.Integer:
                    case eQueryParamType.IntSelectList:
                    case eQueryParamType.IntSelectTree:
                      qp.ValInt = prm.ValInt;
                      break;
                    case eQueryParamType.PlaceCode:
                    case eQueryParamType.PCSelectList:
                    case eQueryParamType.PCSelectTree:
                      qp.CurrentValue = prm.ValPC.PlaceDelimCode;
                      break;
                  }
                }
                isVal = false;
              }
            }
            if (isVal)
              ((Param)qq.Params[ii]).CurrentValue = sprm;
          }
          // 
          mExecute(aDS, qq);
        }
        // remove substring from CommandText
        cmd.CommandText = cmd.CommandText.Remove(pos, cp2 - pos + 1);
      }

      #endregion

      mGenerateCommandWithParam(cmd, paramCol);

      if (isSelect)
      {
        DataSet ds = new DataSet();
        OleDbDataAdapter da = new OleDbDataAdapter();
        da.SelectCommand = cmd;
        da.Fill(ds);

        #region Save image to temporary file or Unzip zipping data
        for (int jj = 0; jj < ds.Tables.Count; jj++)
        {
          DataColumn cid = null;
          DataColumn cif = null;
          DataColumn czd = null;
          DataColumn cud = null;
          DataColumn cbd = null;
          DataColumn cbf = null;
          foreach (DataColumn dc in ds.Tables[jj].Columns)
          {
            if (dc.ColumnName.Equals(COL_IMG_DATA))
              cid = dc;
            else if (dc.ColumnName.Equals(COL_IMG_FILE))
              cif = dc;
            else if (dc.ColumnName.Equals(COL_ZIP_DATA))
              czd = dc;
            else if (dc.ColumnName.Equals(COL_UNZ_DATA))
              cud = dc;
            else if (dc.ColumnName.Equals(COL_BIN_DATA))
              cbd = dc;
            else if (dc.ColumnName.Equals(COL_BIN_FILE))
              cbf = dc;
          }
          if (cid != null && cif != null)
          {
            foreach (DataRow dr in ds.Tables[jj].Rows)
            {
              byte[] bi = dr[cid] as byte[];
              if (bi != null && bi.Length > 0)
              {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(bi);
                try
                {
                  Image img = new Bitmap(ms);
                  string fn = mTempFiles.pGetTempFileName;
                  img.Save(fn, System.Drawing.Imaging.ImageFormat.Jpeg);
                  File.SetAttributes(fn, FileAttributes.Temporary);
                  //������� DiMoN 5.09.06
                  dr[cif] = fn.Replace(@"\", @"\\");
                }
                catch { }
              }
            }
          }
          if (czd != null && cud != null)
          {
            CasBinary bin = new CasBinary();
            foreach (DataRow dr in ds.Tables[jj].Rows)
            {
              byte[] bi = dr[czd] as byte[];
              if (bi != null && bi.Length > 0)
              {
                try
                {
                  MemoryStream ms = bin.TranslateFrom(new MemoryStream(bi));
                  //dr[cud] = System.Text.Encoding.Default.GetString(ms.ToArray()).Replace("\n", Environment.NewLine);
                  dr[cud] = System.Text.Encoding.Default.GetString(ms.ToArray());
                }
                catch {}
              }
            }
          }
          if (cbd != null && cbf != null)
          {
            foreach (DataRow dr in ds.Tables[jj].Rows)
            {
              byte[] bi = dr[cbd] as byte[];
              if (bi != null && bi.Length > 0)
              {
                try
                {
                  string fn = dr[cbf].ToString();
                  if (fn != null && fn.Length > 0)
                    fn = mTempFiles.GetTempFileName(fn);
                  else
                    fn = mTempFiles.pGetTempFileName;
                  FileStream file = new FileStream(fn, FileMode.Create, FileAccess.Write);
                  file.Write(bi, 0, bi.Length);
                  file.Close();
                  File.SetAttributes(fn, FileAttributes.Temporary);
                  dr[cbf] = fn; //.Replace(@"\", @"\\");
                }
                catch { }
              }
            }
          }
        }
        #endregion

        while (ds.Tables.Count > 0)
        {
          DataTable dt = ds.Tables[0];
          ds.Tables.Remove(dt);
          if (aDS.Tables.Count == 0)
            dt.TableName = RESULT_TAB_NAME;
          else
            dt.TableName = RESULT_TAB_NAME + (aDS.Tables.Count + 1).ToString();
          aDS.Tables.Add(dt);
          pResultMessage += "\r\n ���. " + dt.TableName + "\r\n\t";
          foreach (DataColumn dc in dt.Columns)
            pResultMessage += dc.ColumnName + ", ";

        }
      }
      else if (cmd.CommandText.Trim(" \n\r\t".ToCharArray()).Length > 0)
      {
        pResultMessage +=
          "\r\n �������� ������ \r\n" + cmd.CommandText +
          "\r\n ���������� " + mDB.ExecuteNonQuery(cmd) + " (���.)";
      }
    }
    /// <summary>
    /// ������� ������������ ��������� ����������� ���������� � SQL �����.
    /// </summary>
    //private string QueryInsetParam()
    //{
    //  string ret = mCQ.Text;
    //  foreach (Param prm in pCommonParamCollection)
    //  {
    //    if (!prm.Inset)
    //      continue;
    //    ret = ret.Replace(mSes.ParamBegDelim + prm.Name + mSes.ParamEndDelim, prm.ValCurrent);
    //  }
    //  foreach (Param prm in mCQ.Params)
    //  {
    //    if (!prm.Inset)
    //      continue;
    //    ret = ret.Replace(mSes.ParamBegDelim + prm.Name + mSes.ParamEndDelim, prm.ValCurrent);
    //  }
    //  return ret;
    //}

    #region QueryParam
    /// <summary>
    /// ��������� ��������� ���������� OleDbCommand.Parameters  �������.
    /// </summary>
    /// <param name="aCmd">������� (OleDbCommand) �������.</param>
    public void QueryParam(OleDbCommand aCmd)
    {
      QueryParam(aCmd, mCQ, null);
    }
    public void QueryParam(OleDbCommand aCmd, Query aQr)
    {
      QueryParam(aCmd, aQr, null);
    }
    public void QueryParam(OleDbCommand aCmd, ArrayList aParamOptions)
    {
      QueryParam(aCmd, mCQ, aParamOptions);
    }
    public void QueryParam(OleDbCommand aCmd, Query aQr, ArrayList aParamOptions)
    {
      mGenerateCommandWithParam(aCmd, mGetQueryParamCol(aQr, aParamOptions));
    }

    protected ArrayList mGetQueryParamCol(Query aQr, ArrayList aParamOptions)
    {
      ArrayList locParamCol = new ArrayList(16);
      Param pp;
      if (aQr != null)
        foreach (Param prm in aQr.Params)
        {
          pp = new Param();
          pp.CurrentValue = prm.CurrentValue;
          pp.Name = prm.Name;
          pp.Type = prm.Type;
          pp.Inset = prm.Inset;
          //pp.SelectValue = prm.SelectValue;
          //pp.Title = prm.Title;
          locParamCol.Add(pp);
        }

      if (pCommonParamCollection != null)
        foreach (Param prm in pCommonParamCollection)
        {
          pp = new Param();
          pp.CurrentValue = prm.CurrentValue;
          pp.Name = prm.Name;
          pp.Type = prm.Type;
          pp.Inset = prm.Inset;
          locParamCol.Add(pp);
        }

      if (aParamOptions != null)
        foreach (Param prm in aParamOptions)
        {
          pp = new Param();
          pp.CurrentValue = prm.CurrentValue;
          pp.Name = prm.Name;
          pp.Type = prm.Type;
          pp.Inset = prm.Inset;
          locParamCol.Add(pp);
        }

      return locParamCol;
    }
    #endregion

    /// <summary>
    /// ������������� ��������/��� ���������, ���� ���, ������� � ��������� � ���������.
    /// </summary>
    /// <param name="aName">��� ���������.</param>
    /// <param name="aType">��� ���������.</param>
    /// <param name="aVal">�������� ���������.</param>
    public void SetCommonParamVal(string aName, eQueryParamType aType, string aVal)
    {
      if (mSes == null)
        return;

      Param curParam = null;
      foreach (Param prm in mSes.Params)
        if (prm.Name.Equals(aName))
          curParam = prm;

      if (curParam == null)
      {
        curParam = new Param();
        curParam.Name = aName;
        mSes.Params.Add(curParam);
      }

      curParam.Type = aType;
      curParam.CurrentValue = aVal;
    }

    #region previous version
    /*private void QueryParam (OleDbCommand aCmd)
		{
			bool isContinue = true;
			int ib = 0;
			int ie = 0;
			string res = string.Empty;
			string wrd = string.Empty;

			try
			{
				while (isContinue)
				{
					ib = aCmd.CommandText.IndexOf(mSes.ParamBegDelim, ie);
					if (ib == -1)
					{
						res += 	aCmd.CommandText.Substring(ie);
						isContinue = false;
						break;
					}
					res += 	aCmd.CommandText.Substring(ie, ib-ie);
					ie = aCmd.CommandText.IndexOf(mSes.ParamEndDelim, ib+mSes.ParamBegDelim.Length);
					if (ie == -1)
					{
						// ������-�� ��� ������ !!!
						isContinue = false;
						break;
					}
					res += 	"?";

					OleDbParameter oleParam;
					wrd = aCmd.CommandText.Substring(ib+mSes.ParamBegDelim.Length, ie-ib-mSes.ParamBegDelim.Length).ToUpper();
					bool found = false;
					oleParam = new OleDbParameter();
					foreach (Param prm in mCQ.Params)
					{
						oleParam.ParameterName = "@"+prm.Name;
						switch (prm.Type)
						{
							case eQueryParamType.Boolean:
								if (prm.Name.ToUpper().Equals(wrd))
								{
									oleParam.OleDbType = OleDbType.Integer;
									oleParam.Value = Convert.ToBoolean(prm.CurrentValue) ? -1 : 0;
									found = true;
								}
								break;
							case eQueryParamType.Date:
								if (prm.Name.ToUpper().Equals(wrd))
								{
									oleParam.OleDbType = OleDbType.DBTimeStamp;
									oleParam.Value = Convert.ToDateTime(prm.CurrentValue);
									found = true;
								}
								break;
							case eQueryParamType.Integer:
							case eQueryParamType.IntSelectList:
							case eQueryParamType.IntSelectTree:
								if (prm.Name.ToUpper().Equals(wrd))
								{
									oleParam.OleDbType = OleDbType.Integer;
									oleParam.Value = Convert.ToInt32(prm.CurrentValue);
									found = true;
								}
								break;
							case eQueryParamType.PlaceCode:
							case eQueryParamType.PCSelectList:
							case eQueryParamType.PCSelectTree:
							{
								string [] ss = prm.Name.Split(":".ToCharArray());
								if (ss[0].ToUpper().Equals(wrd))
								{
									oleParam.OleDbType = OleDbType.Integer;
									oleParam.Value = Convert.ToInt32(prm.CurrentValue.Split(":".ToCharArray())[0]);
									found = true;
								}
								else if (ss[1].ToUpper().Equals(wrd))
								{
									oleParam.OleDbType = OleDbType.Integer;
									oleParam.Value = Convert.ToInt32(prm.CurrentValue.Split(":".ToCharArray())[1]);
									found = true;
								}
							}
								break;
							case eQueryParamType.String:
							case eQueryParamType.StrSelectList:
							case eQueryParamType.StrSelectTree:
								if (prm.Name.ToUpper().Equals(wrd))
								{
									oleParam.OleDbType = OleDbType.VarChar;
									oleParam.Size = prm.CurrentValue.Length;
									oleParam.Value = prm.CurrentValue;
									found = true;
								}
								break;
						}
						if (found)
						{
							aCmd.Parameters.Add(oleParam);
							break;
						}
					}
#if IBQuery
					if (!found)
					{
						if (_prmUser != null && _prmUser.Name.ToUpper().Equals(wrd))
						{
							oleParam.ParameterName = "@"+_prmUser.Name;
							oleParam.OleDbType = OleDbType.VarChar;
							oleParam.Size = _prmUser.CurrentValue.Length;
							oleParam.Value = _prmUser.CurrentValue;
							aCmd.Parameters.Add(oleParam);
						}
					}
#endif
					ie += mSes.ParamEndDelim.Length;
				}
			}
			catch {}

			aCmd.CommandText = res;
		}*/
    #endregion

    protected void mGenerateCommandWithParam(OleDbCommand aCmd, ArrayList aParamCol)
    {
      if (aParamCol.Count > 0)
      {
        foreach (Param prm in aParamCol)
        {
          if (prm.Inset)
            aCmd.CommandText = aCmd.CommandText.Replace(mSes.ParamBegDelim + prm.Name + mSes.ParamEndDelim, prm.ValCurrent);
        }
        GenerateCommandWithParam(aCmd, aParamCol, mSes.ParamBegDelim, mSes.ParamEndDelim);
      }
    }
    /// <summary>
    /// ��������� ��������� ���������� OleDbCommand.Parameters  �������.
    /// </summary>
    /// <param name="aCmd">������� (OleDbCommand) �������.</param>
    /// <param name="aParamsCollection">��������� ����������</param>
    /// <param name="aPBegDelim">����������� - ������ ���������</param>
    /// <param name="aPEndDelim">����������� - ��������� ���������</param>
    public static void GenerateCommandWithParam
      (OleDbCommand aCmd, ArrayList aParamsCollection, string aPBegDelim, string aPEndDelim)
    {
      bool isContinue = true;
      int ib = 0;
      int ie = 0;
      string res = string.Empty;
      string wrd = string.Empty;

      try
      {
        while (isContinue)
        {
          ib = aCmd.CommandText.IndexOf(aPBegDelim, ie);
          if (ib == -1)
          {
            res += aCmd.CommandText.Substring(ie);
            isContinue = false;
            break;
          }
          res += aCmd.CommandText.Substring(ie, ib - ie);
          ie = aCmd.CommandText.IndexOf(aPEndDelim, ib + aPBegDelim.Length);
          if (ie == -1)
          {
            // ������-�� ��� ������ !!!
            isContinue = false;
            break;
          }

          OleDbParameter oleParam;
          wrd = aCmd.CommandText.Substring(ib + aPBegDelim.Length, ie - ib - aPBegDelim.Length).ToUpper();
          bool found = false;
          oleParam = new OleDbParameter();
          foreach (Param prm in aParamsCollection)
          {
            oleParam.ParameterName = "@" + prm.Name;
            switch (prm.Type)
            {
              case eQueryParamType.Boolean:
                if (prm.Name.ToUpper().Equals(wrd))
                {
                  oleParam.OleDbType = OleDbType.Integer;
                  oleParam.Value = prm.ValBool ? -1 : 0;
                  found = true;
                }
                break;
              case eQueryParamType.Date:
                if (prm.Name.ToUpper().Equals(wrd))
                {
                  oleParam.OleDbType = OleDbType.DBTimeStamp;
                  oleParam.Value = prm.ValDate;
                  found = true;
                }
                break;
              case eQueryParamType.Integer:
              case eQueryParamType.IntSelectList:
              case eQueryParamType.IntSelectTree:
                if (prm.Name.ToUpper().Equals(wrd))
                {
                  oleParam.OleDbType = OleDbType.Integer;
                  oleParam.Value = prm.ValInt;
                  found = true;
                }
                break;
              case eQueryParamType.PlaceCode:
              case eQueryParamType.PCSelectList:
              case eQueryParamType.PCSelectTree:
                {
                  string[] ss = prm.Name.Split(":".ToCharArray());
                  PlaceCode pc = prm.ValPC;
                  if (ss[0].ToUpper().Equals(wrd))
                  {
                    oleParam.OleDbType = OleDbType.Integer;
                    oleParam.Value = pc.place;
                    found = true;
                  }
                  else if (ss[1].ToUpper().Equals(wrd))
                  {
                    oleParam.OleDbType = OleDbType.Integer;
                    oleParam.Value = pc.code;
                    found = true;
                  }
                }
                break;
              case eQueryParamType.String:
              case eQueryParamType.StrSelectList:
              case eQueryParamType.StrSelectTree:
                if (prm.Name.ToUpper().Equals(wrd))
                {
                  oleParam.OleDbType = OleDbType.VarChar;
                  string s = prm.ValStr;
                  oleParam.Value = s;
                  oleParam.Size = s.Length;
                  found = true;
                }
                break;
            }
            if (found)
            {
              res += "?";
              aCmd.Parameters.Add(oleParam);
              break;
            }
          }

          if (!found)
            res += aPBegDelim + wrd + aPEndDelim;

          ie += aPEndDelim.Length;
        }
      }
      catch { }

      aCmd.CommandText = res;
    }

    /// <summary>
    /// �� ������ SQL ������� ���������� ��� (�� ������������ eSQLQueryType).
    /// </summary>
    /// <param name="aSQLText"></param>
    /// <returns>��� (�� ������������ eSQLQueryType).</returns>
    private eSQLQueryType GetSQLQueryType(string aSQLText)
    {
      eSQLQueryType ret = eSQLQueryType.Select;
      aSQLText = aSQLText.ToUpper();
      if (aSQLText.IndexOf(mSes.ParamBegDelim + EXECUTE_NO_QUERY_MARK.ToUpper() + mSes.ParamEndDelim) >= 0)
        ret = eSQLQueryType.Insert;

      //			if (aSQLText.IndexOf("DELETE") >= 0)
      //				ret = eSQLQueryType.Delete;
      //			else if (aSQLText.IndexOf("UPDATE") >= 0)
      //				ret = eSQLQueryType.Update;
      //			else if (aSQLText.IndexOf("INSERT") >= 0)
      //			{
      //				ret = eSQLQueryType.Insert;
      //				if (aSQLText.IndexOf("INSERT @") >= 0)
      //					ret = eSQLQueryType.Select;
      //			}

      return ret;
    }

    /// <summary>
    /// ��������������� ������ ������� mTab � ������ �������:
    /// - ������ ��������� ��������� "\r\n" (������� ������� � ����� ������)
    /// - ���� ��������� �������� "\t" (���������)
    /// </summary>
    /// <returns>������ � ���������������� �������.</returns>
    public override string ToString()
    {
      string strData = mCQ.Name.Replace(@"\", "_") + "\r\n";
      string sep = string.Empty;
      if (mTab.Rows.Count > 0)
      {
        foreach (DataColumn c in mTab.Columns)
        {
          if (c.DataType != typeof(System.Guid) && c.DataType != typeof(System.Byte[]))
          {
            strData += sep + c.ColumnName;
            sep = "\t";
          }
        }
        strData += "\r\n";
        foreach (DataRow r in mTab.Rows)
        {
          sep = string.Empty;
          foreach (DataColumn c in mTab.Columns)
          {
            if (c.DataType != typeof(System.Guid) && c.DataType != typeof(System.Byte[]))
            {
              if (!Convert.IsDBNull(r[c.ColumnName]))
                strData += sep + r[c.ColumnName].ToString().Replace("\r\n", " ");
              else
                strData += sep + string.Empty;
              sep = "\t";
            }
          }
          strData += "\r\n";
        }
      }
      else
        strData += "\r\n---> Table was empty!";

      return strData;
    }

    /// <summary>
    /// ��������� ������ ������� mTab � ����� � ������ [aFileName] � XML �������.
    /// </summary>
    /// <param name="aFileName">������ ��� �����.</param>
    public void SaveTable2XMLFile(string aFileName)
    {
      //DataSet ds = new DataSet(RESULT_SET_NAME);
      //ds.Tables.Add(mTab);
      //ds.WriteXml(aFileName);
      //ds.Tables.Remove(mTab);
      mDS.WriteXml(aFileName);
    }

    /// <summary>
    /// ��������� ������ ������� mTab � ����� � XML �������.
    /// ��� ����� ����������� �� ��� ������� ('\' ���������� �� '_') 
    /// ���� ���������� XML � �������� � ����� pTempPath.
    /// </summary>
    /// <returns>������ ��� �����.</returns>
    public string Table2TempXMLFileName()
    {
      string fileName = mTempXMLFileName;
      try
      {
        SaveTable2XMLFile(fileName);
      }
      catch (Exception ex)
      {
        mErr.ex = ex;
        fileName = string.Empty;
      }
      return fileName;
    }

    /// <summary>
    /// ������ Excel � ������ ������ �� mTab.
    /// ������������ ����������� ������� ������ � XML �������.
    /// �������� ������� � Excel 2003.
    /// </summary>
    public void ToExcel()
    {
      ProcessStartInfo psi = new ProcessStartInfo();
      try
      {
        psi.FileName = @"Excel.exe";
        psi.Arguments = "\"" + Table2TempXMLFileName() + "\"";
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
          mErr.ex = ex;
        }
      }

    }

    /// <summary>
    /// ������� ��� ����� *.xml, *.xsl � �������� �� � ����� pTempPath � ������ 
    /// ������� (� ����� '\' ���������� �� '_'). � xml ���� ���������� ������:
    /// <?xml-stylesheet type="text/xsl"  href="*.xsl"?>
    /// ��� ����������� xsl �������������� ��� iExplorer.
    /// </summary>
    /// <remarks>
    /// ������ ������, ������ iExplorer ��� ��������� xml ����� � ����� � ��� ������ � ��������
    /// xslt, �������� �������������� � ���������. ��� mshtml (COM) ���� ������ �� ��������.
    /// </remarks>
    /// <returns>��� xml ����� (������, � �����).</returns>
    public string Get2FilesXML_XSL_Format1()
    {
      string fn = pTempPath + mCQ.Name.Replace(@"\", "_") + ".xml";
      string fn2 = pTempPath + mCQ.Name.Replace(@"\", "_") + "___.xml";

      try
      {
        SaveTable2XMLFile(fn2);

        StreamReader sr = new StreamReader(fn2);
        StreamWriter sw = new StreamWriter(fn); //, false, Encoding.Default);
        sw.WriteLine(sr.ReadLine());
        sw.WriteLine("<?xml-stylesheet type=\"text/xsl\"  href=\"" + mCQ.Name.Replace(@"\", "_") + ".xsl\"?>");
        sw.Write(sr.ReadToEnd());
        sr.Close();
        sw.Close();

        FileInfo ff = new FileInfo(fn2);
        ff.Delete();

        sw = new StreamWriter(
          pTempPath + mCQ.Name.Replace(@"\", "_") + ".xsl",
          false, Encoding.Default
          );
        sw.Write(XSLTInclude(mCQ.XSLT));
        sw.Close();
      }
      catch (Exception ex)
      {
        mErr.ex = ex;
        fn = string.Empty;
      }

      return fn;
    }

    /// <summary>
    /// �������� HTML ���� � ���������������� � ������� XSL ������� �� mTab.
    /// </summary>
    /// <returns>������ ��� HTML �����.</returns>
    public string GetFilefromXML_XSL()
    {
      #region added by DSY 19.06.2008
      //if (mCQ.XSLT.Length > 0 && !mCQ.XSLT.ToLower().Contains("<xsl:template match"))
      //{
      //  ExcelTemplateFilling();
      //  return string.Empty;
      //}
      #endregion

      //DataSet ds = new DataSet(RESULT_SET_NAME);
      //ds.Tables.Add(mTab);
      StringWriter sw = new StringWriter();
      XmlTextWriter xtw = new XmlTextWriter(sw);
      mDS.WriteXml(xtw);
      //ds.Tables.Remove(mTab);

      string fn = pTempPath + mCQ.Name.Replace(@"\", "_") + ".htm";
      try
      {
        StreamWriter fw = new StreamWriter(fn, false, Encoding.Default);
        fw.Write(MakeHTML(sw.ToString(), XSLTInclude(mCQ.XSLT)));
        fw.Close();
      }
      catch (Exception ex)
      {
        mErr.ex = ex;
        fn = string.Empty;
      }

      return fn;
    }

    /// <summary>
    /// �������� HTML ������ � ���������������� � ������� XSL ������� �� mTab.
    /// </summary>
    /// <returns>������ � HTML</returns>
    public string GetHTMLfromXML_XSL()
    {
      //DataSet ds = new DataSet(RESULT_SET_NAME);
      //ds.Tables.Add(mTab);
      StringWriter sw = new StringWriter();
      XmlTextWriter xtw = new XmlTextWriter(sw);
      mDS.WriteXml(xtw);
      //ds.Tables.Remove(mTab);

      #region added by DSY 19.06.2008
      //if (mCQ.XSLT.Length > 0 && !mCQ.XSLT.ToLower().Contains("<xsl:template match"))
      //{
      //  return ExcelTemplateFilling();
      //  //return string.Empty;
      //}
      #endregion

      return MakeHTML(sw.ToString(), XSLTInclude(mCQ.XSLT));
    }

    /// <summary>
    /// �������������� ������ � XML ��� ������ �������� XSL � ������ ���������� HTML.
    /// �� ������ "Utilize Internet Explorer to display report 
    /// using XML and XSL from a Windows application"
    /// by Patric_J /www.codeproject.com/
    /// </summary>
    /// <param name="xml">������ ������� � XML �������</param>
    /// <param name="xsl">������ � XSL ���������������</param>
    /// <returns>������ � HTML</returns>
    public string MakeHTML(string xml, string xsl)
    {
      string html = string.Empty;
      try
      {
        // Load the XML string into an XPathDocument.

        StringReader xmlStringReader = new StringReader(xml);
        XPathDocument xPathDocument = new XPathDocument(xmlStringReader);

        // Create a reader to read the XSL.

        StringReader xslStringReader = new StringReader(xsl);
        XmlTextReader xslTextReader = new XmlTextReader(xslStringReader);

        // Load the XSL into an XslTransform.

        XslTransform xslTransform = new XslTransform();
        xslTransform.Load(xslTextReader, null, GetType().Assembly.Evidence);

        // Perform the actual transformation and output an HTML string. 

        StringWriter htmlStringWriter = new StringWriter();
        xslTransform.Transform(xPathDocument, null, htmlStringWriter, null);
        html = htmlStringWriter.ToString();


        #region  ��������� dsy 28.04.2006
        ///  ������� ����������� �������� ����� WEB-������ (mht)
        //				string Header = 
        //					"MIME-Version: 1.0"+
        //					"\nContent-Type: multipart/related;boundary=\"----=_MyImage\";type=\"text/html\""+
        //					//  �������� ��������� ��������
        //					"\n\n------=_MyImage"+
        //					"\nContent-Type: text/html;charset=\"windows-1251\""+
        //					"\nContent-Transfer-Encoding: 7bit\n"+
        //					@"Content-Location: file://C:\_WORK\img\test.html"+
        //					"\n\n";

        //				html = Header + html;
        /*		
                string Footer =
                  "------=_MyImage"+
                  " Content-Type: image/jpeg"+
                  " Content-Transfer-Encoding: base64"+
                  @" Content-Location: file:///C:/_WORK/img/image1.jpeg ";

                html += Footer;
        */

        #endregion  ��������� dsy 28.04.2006

        // Close all our readers and writers.

        xmlStringReader.Close();
        xslStringReader.Close();
        xslTextReader.Close();
        htmlStringWriter.Close();

        // Done, return the created HTML code.
      }
      catch (Exception ex)
      {
        html = "<html><body>" + ex.Message + "<body></html>";
        mErr.ex = ex;
      }

      return html;
    }

    /// <summary>
    /// ������� ��� �������� ��������� ����� ��� ���������� �������.
    /// </summary>
    public void BeforeClosed()
    {
      try
      {
        FileInfo ff = new FileInfo(mTempXMLFileName);
        ff.Delete();
        ff = new FileInfo(pTempPath + mCQ.Name.Replace(@"\", "_") + ".xsl");
        ff.Delete();
      }
      catch { }
    }

    /// <summary>
    /// ��������� ��� ��������������� ��� ��������� SXL ��������������.
    /// </summary>
    /// <param name="xsl">����� SXL ��������������.</param>
    /// <returns>�����, � ������ �����������, xsl ��������������.</returns>
    private string XSLTInclude(string xsl)
    {
      string ret = xsl;

      //if (mSes.XSLTIncludeName.Length > 0)
      //	ret = xsl.Replace(mSes.ParamBegDelim+mSes.XSLTIncludeName+mSes.ParamEndDelim, mSes.XSLTIncludeText);

      if (pCommonParamCollection != null)
      {
        foreach (Param prm in pCommonParamCollection)
          ret = ret.Replace(mSes.ParamBegDelim + prm.Name + mSes.ParamEndDelim, prm.ValCurrent);
      }

      foreach (Param prm in mCQ.Params)
        ret = ret.Replace(mSes.ParamBegDelim + prm.Name + mSes.ParamEndDelim, prm.ValCurrent);

      return ret;
    }

    /// <summary>
    /// ������� ��� ��������� ����� �� ��������� mTempFiles � ������� ��.
    /// </summary>
    public void DeleteAllTemporaryFiles()
    {
      mTempFiles.DeleteAllFiles();
    }

    /// <summary>
    /// ���������� ������� Excel
    /// </summary>
    public string ExcelTemplateFilling(XmlDocument doc)
    {
      try
      {
        /*
        string path = string.Empty;
        foreach (Param prm in pSes.Params)
          //  TL - TempLate
          if (prm.Name.Equals("ISSTemplatePath"))
          {
            path = prm.ValCurrent;
            break;
          }
        if(path.Length>0)
        {
          if (path.EndsWith("/") || path.EndsWith(@"\"))
          { }
          else
            path += path.Contains("/") ? "/" : "\\" ;
        }
        */

        ExcelTemplateLogic etl = new ExcelTemplateLogic();
        etl.pDS = pResultSet;
        etl.pDoc = doc;
        //etl.pTempPath = pTempPath;
        return etl.TemplateProcess();
      }
      catch // (Exception ex)
      {
        //mErr.ex = ex;
        return string.Empty;
      }
    }
  }

  /// <summary>
  /// ����� ��� ���������� ������� Excel
  /// </summary>
  public class ExcelTemplateLogic
  {
    public string pXSL_fn;
    private XmlDocument _doc;
    public XmlDocument pDoc
    {
      set { _doc = value; }
    }
    private DataSet _ds;
    public DataSet pDS
    {
      get { return _ds; }
      set { _ds = value; }
    }
    /// <summary>
    /// ������ �������� �����-�������
    /// </summary>
    public string _tempName;
    private ArrayList _al;
    public string pTempPath;

    /// <summary>
    /// ��������� �������
    /// </summary>
    public string TemplateProcess()
    {
      _al = new ArrayList();
      //string s = DateTime.Now.ToShortDateString();
      //string outFN = string.Format("{0}_{1}.xml", tempName, s);
      //outFN = pTempPath + outFN;

      try
      {
        //_tempName = tempName;
        //_doc = new XmlDocument();
        //_doc.Load(tempPath);

        NameTable nt = new NameTable();
        // ������� �������� �����������
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
        nsmgr.AddNamespace("", "urn:schemas-microsoft-com:office:spreadsheet");
        nsmgr.AddNamespace("o", "urn:schemas-microsoft-com:office:office");
        nsmgr.AddNamespace("x", "urn:schemas-microsoft-com:office:excel");
        nsmgr.AddNamespace("ss", "urn:schemas-microsoft-com:office:spreadsheet");
        nsmgr.AddNamespace("html", "http://www.w3.org/TR/REC-html40");

        // ��������� ���� ���������
        XPathNavigator nav = _doc.CreateNavigator();
        XPathNavigator node = nav.SelectSingleNode("//Names");  //, nsmgr);

        #region �������� ������ ������ ���������� ������������� �����

        //  ��������� �����, �������� �� ������ ������, ������
        node.MoveToFirstChild();
        do
        {
          NamedRange nr = new NamedRange();
          nr.pName = node.GetAttribute("Name", "urn:schemas-microsoft-com:office:spreadsheet");
          //ss:RefersTo="=����1!R5C3"
          string str = node.GetAttribute("RefersTo", "urn:schemas-microsoft-com:office:spreadsheet");
          int indexR = str.IndexOf("R");
          int indexC = str.IndexOf("C");
          int rIndex = Convert.ToInt16(str.Substring(indexR + 1, (indexC - indexR - 1)));
          nr.pRowIndex = rIndex;
          nr.pNewRowIndex = rIndex;
          nr.pColumnIndex = Convert.ToInt16(str.Substring(indexC + 1));
          if (!(node.GetAttribute("direction", string.Empty).Length == 0))
            nr.pDirection = (Direction)Enum.Parse(typeof(Direction), node.GetAttribute("direction", string.Empty));
          if (!(node.GetAttribute("included", string.Empty).Length == 0))
            nr.pIncluded = Convert.ToBoolean(node.GetAttribute("included", string.Empty));
          if (!(node.GetAttribute("multiple", string.Empty).Length == 0))
            nr.pMultiple = Convert.ToBoolean(node.GetAttribute("multiple", string.Empty));
          if (!(node.GetAttribute("table", string.Empty).Length == 0))
            nr.pTableIndex = Convert.ToInt16(node.GetAttribute("table", string.Empty));
          if (!(node.GetAttribute("field", string.Empty).Length == 0))
            nr.pField = node.GetAttribute("field", string.Empty);
          if (!(node.GetAttribute("type", string.Empty).Length == 0))
            nr.pType = node.GetAttribute("type", string.Empty).ToLower();
          if (!(node.GetAttribute("empty", string.Empty).Length == 0))
            nr.pIsEmpty = Convert.ToBoolean(node.GetAttribute("empty", string.Empty));
          if (!(node.GetAttribute("styleInhereted", string.Empty).Length == 0))
            nr.pStyleInhereted = Convert.ToBoolean(node.GetAttribute("styleInhereted", string.Empty));

          //  ���������� ������� �� ������, ����� �� �������
          int i = 0;
          while (true)
          {
            if (_al.Count <= i)
              break;
            if (((NamedRange)_al[i]).pRowIndex > nr.pRowIndex ||
              (((NamedRange)_al[i]).pRowIndex == nr.pRowIndex && ((NamedRange)_al[i]).pColumnIndex > nr.pColumnIndex))
              break;
            i++;
          }
          _al.Insert(i, nr);
        }
        while (node.MoveToNext());

        //  ������������ ������ ��� ����� �������
        InheritStyle(nsmgr);

        #endregion

        #region ���������� ������� �� ������� ������

        for (int i = 0; i < _al.Count; i++)
        {
          NamedRange obj = _al[i] as NamedRange;
          if (obj.pIsEmpty)
            continue;

          //  ���� �������� ���, �� ������� ��������. ��� � ������, ����� ������ ����� �������, � ������ ������� ������
          try
          {
            string temp = _ds.Tables[obj.pTableIndex].Rows[0][obj.pField].ToString();
          }
          catch
          {
            continue;
          }
          //if (_ds.Tables[obj.pTableIndex].Rows[0][obj.pField] == null)

          switch ((int)obj.pDirection)
          {
            // ������
            case 0:
              {
                #region Inside
                bool Exists = false;
                bool isParent = true;
                XmlNode curNode = FindRowByIndex(nsmgr, obj.pNewRowIndex, out Exists, out isParent);  //obj.pName, 
                bool toUpdate;  //  ������ ��� �� ������ ������ �� ������������� ������
                XmlNode newRowNode = curNode;
                if (!Exists)
                  newRowNode = InsertRow(nsmgr, obj.pName, curNode, obj.pNewRowIndex, out toUpdate);
                isParent = true;
                XmlNode previousNode = null;  //  ���� ����� �����, ���� ������ (���� ��� ������)
                if (Exists)
                  previousNode = FindCellByIndex(nsmgr, newRowNode, obj.pColumnIndex, out Exists, out isParent);
                else
                  previousNode = newRowNode;
                if (!Exists)
                  //InsertCell(previousNode, isParent, obj.pColumnIndex,
                  //  _ds.Tables[obj.pTableIndex].Rows[0][obj.pField], obj.pType);
                  InsertCell(previousNode, isParent, obj, _ds.Tables[obj.pTableIndex].Rows[0][obj.pField]);
                else
                {
                  UpdateExistingCell(previousNode, _ds.Tables[obj.pTableIndex].Rows[0][obj.pField].ToString(),
                    obj.pType);
                  //previousNode.FirstChild.InnerText = _ds.Tables[obj.pTableIndex].Rows[0][obj.pField].ToString();
                }
                break;
                #endregion
              }
            //  �����
            case 1:
              break;

            //  ����
            case 2:
              {
                #region Down
                //  ������� ������
                int newRows = 0;

                if (obj.pMultiple)
                {
                  bool Exists = false;
                  bool isParent = true;
                  //  ����� ����, �� ����� ������ ������ ��� ����!
                  XmlNode curNode = FindRowByIndex(nsmgr, obj.pRowIndex, out Exists, out isParent); //obj.pName, 
                  XmlNode newRowNode = null;
                  foreach (DataRow dr in _ds.Tables[obj.pTableIndex].Rows)
                  {
                    newRows++;
                    if (!(newRowNode == null))
                      curNode = newRowNode;
                    bool toUpdate;  //  ������ ��� �� ������ ������ �� ������������� ������
                    newRowNode = InsertRow(nsmgr, obj.pName, curNode, obj.pRowIndex + newRows, out toUpdate);
                    isParent = true;
                    if (toUpdate)
                      UpdateNamedRangeRowIndex(obj.pRowIndex + newRows);

                    //InsertCell(newRowNode, isParent, obj.pColumnIndex, dr[obj.pField], obj.pType);
                    InsertCell(newRowNode, isParent, obj, dr[obj.pField]);
                  }
                }
                else
                {
                  //  ���� �������� == '', �� ������ �� �����
                  if (_ds.Tables[obj.pTableIndex].Rows[0][obj.pField].ToString().Length != 0)
                  {
                    bool Exists = false;
                    bool isParent = true;
                    XmlNode curNode = FindRowByIndex(nsmgr, obj.pRowIndex, out Exists, out isParent);   //obj.pName, 
                    bool toUpdate;
                    XmlNode newRowNode = InsertRow(nsmgr, obj.pName, curNode, obj.pRowIndex + 1, out toUpdate); //curNode, 
                    isParent = true;
                    if (toUpdate)
                      UpdateNamedRangeRowIndex(obj.pRowIndex + 1);

                    //InsertCell(newRowNode, isParent, obj.pColumnIndex,
                    //  _ds.Tables[obj.pTableIndex].Rows[0][obj.pField], obj.pType);
                    InsertCell(newRowNode, isParent, obj, _ds.Tables[obj.pTableIndex].Rows[0][obj.pField]);
                    //XmlNode previousNode = null;  //  ���� ����� �����, ���� ������ (���� ��� ������)
                    //if (Exists)
                    //  previousNode = FindCellByIndex(nsmgr, newRowNode, obj.pColumnIndex, out Exists, out isParent);
                    //else
                    //  previousNode = newRowNode;
                    //InsertCell(previousNode, isParent, obj.pColumnIndex, _ds.Tables[obj.pTableIndex].Rows[0][obj.pField]);
                  }
                }

                //IncreaseRowIndex(nsmgr, nList, obj.pRowIndex + 1, newRows);
                ModifyNRReference(nsmgr, obj.pName, obj.pRowIndex, newRows);   //, obj.pColumnIndex
                //i = _al.Count; //  ����� ����� �� �����
                break;
                #endregion
              }
            //  ������
            case 3:
              {
                #region Right
                //  ������ ������ ������������!
                bool Exists = false;
                bool isParent = true;
                XmlNode curNode = FindRowByIndex(nsmgr, obj.pNewRowIndex, out Exists, out isParent);  //obj.pName, 

                XmlNode previousNode = null;  //  ���� ����� �����, ���� ������ (���� ��� ������)
                previousNode = FindCellByIndex(nsmgr, curNode, obj.pColumnIndex, out Exists, out isParent);
                if (!Exists)
                  //InsertCell(previousNode, isParent, obj.pColumnIndex,
                  //  _ds.Tables[obj.pTableIndex].Rows[0][obj.pField], obj.pType);
                  InsertCell(previousNode, isParent, obj, _ds.Tables[obj.pTableIndex].Rows[0][obj.pField]);
                else
                {
                  string val = ReplaceDotByComma(obj.pType, _ds.Tables[obj.pTableIndex].Rows[0][obj.pField].ToString());
                  //previousNode.FirstChild.InnerText = _ds.Tables[obj.pTableIndex].Rows[0][obj.pField].ToString();
                  previousNode.FirstChild.InnerText = val;
                }

                //InsertCell(curNode, false, obj.pColumnIndex,
                //  _ds.Tables[obj.pTableIndex].Rows[0][obj.pField], obj.pType);
                break;
                #endregion
              }
            //  �����
            case 4:
              break;
            default:
              break;
          }
        }

        #endregion


        //XmlWriterSettings xws = new XmlWriterSettings();
        //xws.ConformanceLevel = ConformanceLevel.Document;
        //xw = XmlWriter.Create(outFN, xws);
        //xw.WriteNode(nav, true);
        //XmlAttribute att = node. (string.Empty, "xmlns", "urn:schemas-microsoft-com:office:spreadsheet", "urn:schemas-microsoft-com:office:spreadsheet");
        //doc.Attributes.InsertBefore(att);

        //  ��������� ��������� �� ���������, ������� ������-�� �� ������������ � �������������� ����
        //  xmlns="urn:schemas-microsoft-com:office:spreadsheet"

        IncreaseExpandedRowCount(nsmgr);

        //  ����� ������� �� ����������� ((
        //XmlAttribute att = _doc.CreateAttribute(string.Empty, "xmlns", "urn:schemas-microsoft-com:office:spreadsheet");
        //XmlNode mainNode = _doc.SelectSingleNode("Workbook");
        //mainNode.Attributes.Prepend(att);

        node = nav.SelectSingleNode("//Workbook");
        node.CreateAttribute(string.Empty, @"xmlns", "", "urn:schemas-microsoft-com:office:spreadsheet");

        //XPathNavigator fstAtt = node.Clone();
        //fstAtt.MoveToFirstAttribute();
        //fstAtt.InsertBefore("xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");

        return _doc.InnerXml;

        /*
        _doc.Save(outFN);
        //System.Windows.Forms.MessageBox.Show("������ ������� ��������");
        Process prc = new Process();
        ProcessStartInfo psi = new ProcessStartInfo("Excel", outFN);
        prc.StartInfo = psi;
        prc.Start();
        */
      }
      catch (XmlException ex)
      {
        throw new XmlException(ex.Message);
        //return @"Cannot load file\n" + ex.Message;
      }
      catch (System.ArgumentException ex)
      {
        throw new System.ArgumentException(ex.Message);
        //return @"Cannot find single node\n" + ex.Message;
      }
      catch (System.Xml.XPath.XPathException ex)
      {
        throw new System.Xml.XPath.XPathException(ex.Message);
        //return @"Cannot find path\n" + ex.Message;
      }
      catch (Exception ex)
      {
        throw new Exception(ex.Message);
        //return ex.Message;
      }
    }

    /// <summary>
    /// ������ ������� �� ����� � ��������� ���� Number
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private string ReplaceDotByComma(string type, string value)
    {
      if (type.ToLower().Equals("number"))
        value = value.Replace(",", ".");
      return value;
    }

    /// <summary>
    /// ������������ ������ �� ������ ������ ����� ����� � �������
    /// </summary>
    /// <param name="manager"></param>
    private void InheritStyle(XmlNamespaceManager manager)
    {
      bool exists = true, parent = false;
      for (int i = 0; i < _al.Count; i++)
      {
        NamedRange obj = (NamedRange)_al[i];
        if (obj.pDirection != Direction.bottom)
          continue;

        XmlNode curRow = FindRowByIndex(manager, obj.pRowIndex + 1, out exists, out parent);
        XmlNode curCell = null;
        if(curRow != null)
          curCell = FindCellByIndex(manager, curRow, obj.pColumnIndex, out exists, out parent);
        if (curCell != null && curCell.Attributes["StyleID", "urn:schemas-microsoft-com:office:spreadsheet"] != null)
        {
          obj.pStyleId = curCell.Attributes["StyleID", "urn:schemas-microsoft-com:office:spreadsheet"].Value;
        }
      }
    }

    /// <summary>
    /// ��� ���������� ������ ������ ������ �� ��� ����������� ������������� ������, ����� �����,
    /// ����� ��������� ���������� �����, ���� ��� �������� � �������� ������ � ���� ������������� �����
    /// </summary>
    /// <param name="newRowIndex">������ ����������� ������</param>
    private void UpdateNamedRangeRowIndex(int newRowIndex)
    {
      for (int i = 0; i < _al.Count; i++)
        if (((NamedRange)_al[i]).pNewRowIndex > newRowIndex)
          ((NamedRange)_al[i]).pNewRowIndex++;    //= ((NamedRange)_al[i]).pRowIndex + 1;
    }

    /// <summary>
    /// ����� ������ �� �������. ���� ����� ������ ���, ����� ����������� ������
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="index"></param>
    /// <param name="doesExists">���������� �� ������ � ����� ���������� �������</param>
    /// <returns></returns>
    private XmlNode FindRowByIndex(XmlNamespaceManager manager, int index, out bool doesExists, out bool isParent) //string aNamedRange, 
    {
      //index++;

      //XmlNodeList nList = _doc.SelectNodes("//Table/Row[.//NamedCell]");    //Table/Row[.//NamedCell
      XmlNodeList nList = _doc.SelectNodes("//Table/Row");    //Table/Row[.//NamedCell
      int curIndex = 0;
      int curRowIndex, curRowSpan = -1;
      foreach (XmlNode node in nList)
      {
        try
        {
          curRowIndex = Convert.ToInt32(node.Attributes["Index", "urn:schemas-microsoft-com:office:spreadsheet"].Value);
          if (curRowIndex == index)
          {
            doesExists = true;
            isParent = false;
            return node;
          }
          if (curRowIndex > index)
          {
            doesExists = false;
            if (node.PreviousSibling == null)
            {
              isParent = true;
              return node.ParentNode;     //  ���������� ��������
            }
            else
            {
              isParent = false;
              return node.PreviousSibling;  //  ���������� ���������� ������
            }
          }
          curIndex = curRowIndex;
        }
        catch
        {
          curRowIndex = -1;
          curIndex += (curRowSpan == -1 ? 1 : curRowSpan);
          if (curIndex == index)
          {
            doesExists = true;
            isParent = false;
            return node;
          }
        }

        try
        {
          curRowSpan = Convert.ToInt32(node.Attributes["Span", "urn:schemas-microsoft-com:office:spreadsheet"].Value) + 1;
        }
        catch
        {
          curRowSpan = -1;
        }
      }
      doesExists = false;
      isParent = false;
      return null;    // nList[nList.Count - 1];
    }

    /// <summary>
    /// ����������� ������� ������ (����)
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="row">������</param>
    /// <returns></returns>
    private int FindRowIndex(XmlNamespaceManager manager, XmlNode row)
    {
      XmlNodeList nList = _doc.SelectNodes("//Table/Row");    //Table/Row[.//NamedCell
      int curIndex = 0;
      int curRowIndex, curRowSpan = -1;

      foreach (XmlNode node in nList)
      {
        try
        {
          curRowIndex = Convert.ToInt32(node.Attributes["Index", "urn:schemas-microsoft-com:office:spreadsheet"].Value);
          curIndex = curRowIndex;
        }
        catch
        {
          //curRowIndex = -1;
          curIndex += (curRowSpan == -1 ? 1 : curRowSpan);
        }

        if (node.Equals(row))
          return curIndex += curRowSpan;

        try
        {
          curRowSpan = Convert.ToInt32(node.Attributes["Span", "urn:schemas-microsoft-com:office:spreadsheet"].Value) + 1;
        }
        catch
        {
          curRowSpan = -1;
        }
      }
      return curIndex;
    }

    /// <summary>
    /// ����� ������, ���� ������ � ����� ������
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="row"></param>
    /// <param name="columnIndex"></param>
    /// <param name="doesExists"></param>
    /// <param name="isParent"></param>
    /// <returns></returns>
    private XmlNode FindCellByIndex(XmlNamespaceManager manager, XmlNode row, int columnIndex, out bool doesExists, out bool isParent)
    {
      XmlNodeList nList = row.ChildNodes; // _doc.SelectNodes("//Table/Row");    //Table/Row[.//NamedCell
      int curIndex = 0;
      int curCellIndex, curCellSpan = -1;
      foreach (XmlNode node in nList)
      {
        try
        {
          curCellIndex = Convert.ToInt32(node.Attributes["Index", "urn:schemas-microsoft-com:office:spreadsheet"].Value);
          if (curCellIndex == columnIndex)
          {
            doesExists = true;
            isParent = false;
            return node;
          }
          if (curCellIndex > columnIndex)
          {
            doesExists = false;
            if (node.PreviousSibling == null)
            {
              isParent = true;
              return node.ParentNode;     //  ���������� ��������
            }
            else
            {
              isParent = false;
              return node.PreviousSibling;  //  ���������� ���������� ������
            }
          }
          curIndex = curCellIndex;
        }
        catch
        {
          curCellIndex = -1;
          curIndex += (curCellSpan == -1 ? 1 : curCellSpan);
          if (curIndex == columnIndex)
          {
            doesExists = true;
            isParent = false;
            return node;
          }
        }

        try
        {
          curCellSpan = Convert.ToInt32(node.Attributes["Span", "urn:schemas-microsoft-com:office:spreadsheet"].Value) + 1;
        }
        catch
        {
          curCellSpan = -1;
        }
      }
      doesExists = false;
      isParent = false;
      return null;
    }

    /// <summary>
    /// ������� ������ ����� ����� - ����� ��� ��������� �������� ss:ExpandedRowCount
    /// </summary>
    /// <returns></returns>
    private int TotalRowCount(XmlNamespaceManager manager)
    {
      XmlNodeList nList = _doc.SelectNodes("//Table/Row");
      XmlNode node = _doc.SelectSingleNode(@"//Row[@ss:Index and not(@ss:Index < //Row/@ss:Index)]", manager);
      // //Row[not(@Index &lt; //Row/@Index)]
      // //Row[@Index and not(@Index &lt; //Row/@Index)]
      // //Row[not(@index &gt; //Row/@Index)][1]

      if (!(node == null))
      {
        int index = Convert.ToInt32(node.Attributes["Index", "urn:schemas-microsoft-com:office:spreadsheet"].Value);
        int count = index, curRowSpan;
        node = node.NextSibling;
        while (node != null)
        {
          try
          {
            curRowSpan = Convert.ToInt32(node.Attributes["Span", "urn:schemas-microsoft-com:office:spreadsheet"].Value);
          }
          catch
          {
            curRowSpan = 0;
          }
          count += curRowSpan + 1;
          node = node.NextSibling;
        }
        return count;
      }
      else
        return nList.Count;
    }

    /// <summary>
    /// ��������� ������ � ������������� ������� (��� ������� �����), ������� ��������� ���� ����������� �����
    /// </summary>
    /// <param name="aDoc">xml-��������</param>
    /// <param name="aNameRange">��� ������</param>
    /// <param name="aRow">���-�� ����������� �����</param>
    /// <param name="aCell">����� �������</param>
    private void ModifyNRReference(XmlNamespaceManager manager, string aNameRange, int aRow, int aRowCount)  //XPathNavigator aNav, 
    {
      XmlNodeList nList = _doc.SelectNodes("//Names/NamedRange");
      int newRowIndex = 0, columnIndex = 0;
      foreach (XmlNode node in nList)
      {
        for (int i = 0; i < _al.Count; i++)
        {
          NamedRange obj = (NamedRange)_al[i];
          string str = node.Attributes["Name", "urn:schemas-microsoft-com:office:spreadsheet"].Value;
          if (obj.pName.Equals(node.Attributes["Name", "urn:schemas-microsoft-com:office:spreadsheet"].Value))
          {
            newRowIndex = obj.pNewRowIndex;
            columnIndex = obj.pColumnIndex;
            break;
          }
        }

        if (newRowIndex == 0)
          continue;
        string RefTo = string.Concat("=����1!R", newRowIndex.ToString(), "C", columnIndex.ToString());
        node.Attributes["RefersTo", "urn:schemas-microsoft-com:office:spreadsheet"].Value = RefTo;
      }


      //for (int i = 0; i < _al.Count; i++)
      //{
      //  NamedRange obj = (NamedRange)_al[i];
      //  if (obj.pRowIndex <= aRow)
      //  {
      //    continue;
      //  }
      //  XmlNode node = _doc.SelectSingleNode(string.Format("//Names/NamedRange[@ss:Name='{0}']", obj.pName), manager);
      //  if (node.Attributes != null)
      //  {
      //    string RefTo = string.Concat("=����1!R", (obj.pRowIndex + aRowCount).ToString(), "C", obj.pColumnIndex.ToString());
      //    node.Attributes["RefersTo", "urn:schemas-microsoft-com:office:spreadsheet"].Value = RefTo;
      //  }
      //}
    }

    /// <summary>
    /// ������� ����� ������
    /// </summary>
    /// <param name="node"></param>
    /// <param name="aRowIndex">����� ������� ������</param>
    /// <param name="isNew">������� ������ ��� ������� �����</param>
    /// <returns></returns>
    private XmlNode InsertRow(XmlNamespaceManager manager, string aNamedRange, XmlNode node, int aRowIndex, out bool toUpdate)   //XPathNavigator aNav, XmlNode node, 
    {
      //aRowIndex++;

      #region  �������� - ���� �� ����� ������
      XmlNode fstNode = _doc.SelectSingleNode(string.Format("//Table/Row[@ss:Index={0}]", aRowIndex), manager);  //.FirstChild;
      if (!(fstNode == null))
      {
        toUpdate = false;
        return fstNode;
      }
      #endregion

      XmlNode xn = _doc.CreateElement("Row");
      XmlAttribute att = _doc.CreateAttribute("Index", "urn:schemas-microsoft-com:office:spreadsheet");
      att.Value = aRowIndex.ToString();
      xn.Attributes.Append(att);
      att = _doc.CreateAttribute("Imported", "urn:schemas-microsoft-com:office:spreadsheet");
      //att.Value = "True";
      xn.Attributes.Append(att);

      if (node != null)
      {
        XmlNode root = node.ParentNode;
        root.InsertAfter(xn, node);
      }

      //  ���� ����� ����������� ������ ���� "������" ������, �� ����� ������ 
      XmlNode n = xn.NextSibling; //.ChildNodes.Item;
      if (n == null)
      {
        toUpdate = false;
        return xn;
      }
      if (n.Attributes["Imported", "urn:schemas-microsoft-com:office:spreadsheet"] == null)
      {
        //if (n.InnerXml.Contains("<NamedCell"))
        toUpdate = true;
        ChangeFormulaReference(manager, xn, aRowIndex);   //, firstInsertedRowIndex, increment);
        IncreaseAllRowsIndexes(manager, xn, aRowIndex);
      }
      else
      {
        toUpdate = false;
      }
      return xn;
    }

    /// <summary>
    /// ���������� ������� ���� "������" �����, ������ ����� �����������
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="insertedNode"></param>
    /// <param name="insertedRowIndex"></param>
    private void IncreaseAllRowsIndexes(XmlNamespaceManager manager, XmlNode insertedNode, int insertedRowIndex)
    {
      XmlNode node = insertedNode.NextSibling;
      while (node != null)
      {
        if (node.Attributes["Imported", "urn:schemas-microsoft-com:office:spreadsheet"] == null)
        {
          if (!(node.Attributes["Index", "urn:schemas-microsoft-com:office:spreadsheet"] == null))
          {
            int index = Convert.ToInt32(node.Attributes["Index", "urn:schemas-microsoft-com:office:spreadsheet"].Value);
            node.Attributes["Index", "urn:schemas-microsoft-com:office:spreadsheet"].Value = (index + 1).ToString();
          }
        }
        node = node.NextSibling;
      }

      //ChangeFormulaReference(insertedNode, insertedRowIndex);   //, firstInsertedRowIndex, increment);
      //IncreaseExpandedRowCount(manager, increment);
    }

    /// <summary>
    /// ������� ����� ������
    /// </summary>
    /// <param name="node"></param>
    /// <param name="aCellIndex"></param>
    /// <param name="value"></param>
    //private void InsertCell(XmlNode node, bool isParent, int aCellIndex, object value, string type)
    private void InsertCell(XmlNode node, bool isParent, NamedRange obj, object value)
    {
      XmlNode xnCell = _doc.CreateElement("Cell");
      XmlAttribute attCell = _doc.CreateAttribute("Index", "urn:schemas-microsoft-com:office:spreadsheet");
      //attCell.Value = aCellIndex.ToString();
      attCell.Value = obj.pColumnIndex.ToString();
      xnCell.Attributes.Append(attCell);
      if (obj.pStyleId.Length > 0)
      {
        attCell = _doc.CreateAttribute("StyleID", "urn:schemas-microsoft-com:office:spreadsheet");
        attCell.Value = obj.pStyleId;
        xnCell.Attributes.Append(attCell);
      }
      attCell = _doc.CreateAttribute("Imported", "urn:schemas-microsoft-com:office:spreadsheet");

      //attCell.Value = "True";
      xnCell.Attributes.Append(attCell);
      XmlNode xnData = _doc.CreateElement("Data");
      //if (obj.pType.ToLower().Equals("number"))
      //  value = value.ToString().Replace(",", ".");
      string val = ReplaceDotByComma(obj.pType, value.ToString());
      xnData.InnerText = val;
      XmlAttribute attData = _doc.CreateAttribute("Type", "urn:schemas-microsoft-com:office:spreadsheet");
      //attData.Value = type;
      string type = string.Empty;
      switch (obj.pType)
      {
        case "number":
          type = "Number";
          break;
        case "string":
          type = "String";
          break;
        default:
          type = "String";
          break;
      }
      attData.Value = type;
      xnData.Attributes.Append(attData);
      xnCell.AppendChild(xnData);
      if (isParent)
        node.AppendChild(xnCell);
      else
        node.ParentNode.InsertAfter(xnCell, node);
    }

    /// <summary>
    /// ��������� ����������� ������������ ������. ������� ���� Data, ���� ��� ���.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="value"></param>
    private void UpdateExistingCell(XmlNode node, string value, string type)
    {
      //  ���� ��� ���� Data, �� ��� �������
      if (!node.InnerXml.Contains("Data"))
      {
        XmlNode xnData = _doc.CreateElement("Data");
        xnData.InnerText = value;
        XmlAttribute attData = _doc.CreateAttribute("Imported", "urn:schemas-microsoft-com:office:spreadsheet");
        xnData.Attributes.Append(attData);
        attData = _doc.CreateAttribute("Type", "urn:schemas-microsoft-com:office:spreadsheet");
        attData.InnerText = type;
        xnData.Attributes.Append(attData);
        node.AppendChild(xnData);
        return;
      }

      foreach (XmlNode curNode in node.ChildNodes)
      {
        if (curNode.LocalName.Equals("Data"))
        {
          curNode.InnerText = curNode.InnerText + value;
          return;
        }
      }
    }

    /// <summary>
    /// ��������� �������� ���� �����, ������ ����� ����������� �����
    /// </summary>
    /// <param name="aMng">������������ ����������</param>
    /// <param name="firstInsertedRowIndex">������ ������ ����������� ������ - ����� �������� ������ 
    /// ����� ������� ���� ������� ��� ����� ������������� ������</param>
    /// <param name="increment">�������� ��������� �������</param>
    private void IncreaseRowIndex(XmlNamespaceManager manager, XmlNodeList nList, int firstInsertedRowIndex, int increment)
    {
      //XmlNodeList nList = _doc.SelectNodes("//Table", manager);
      //XmlNodeList nList = _doc.SelectNodes(string.Format("//Table/Row[@ss:Index>{0}]",
      //firstInsertedRowIndex.ToString()), manager);
      //  ���� ���� RowIndex, ����������� ��� �� �������� increment
      foreach (XmlNode node in nList)
      {
        //if (node.Attributes["Index", "urn:schemas-microsoft-com:office:spreadsheet"] == null)
        //  continue;
        int index = Convert.ToInt32(node.Attributes["Index", "urn:schemas-microsoft-com:office:spreadsheet"].Value);
        if (index < firstInsertedRowIndex)
          continue;
        node.Attributes["Index", "urn:schemas-microsoft-com:office:spreadsheet"].Value = (index + increment).ToString();
        //  ���� ���� ������� "Formula", ������ � ��� ������        
        //ChangeFormulaReference(node, index, firstInsertedRowIndex, increment);
      }
      //IncreaseExpandedRowCount(manager, increment);
    }

    /// <summary>
    /// ���������� ������ ����� ��������������� ����� � ���������
    /// </summary>
    /// <Table ss:ExpandedColumnCount="8" ss:ExpandedRowCount="8" x:FullColumns="1"
    ///    x:FullRows="1" ss:DefaultColumnWidth="54" ss:DefaultRowHeight="13.5">
    private void IncreaseExpandedRowCount(XmlNamespaceManager manager)
    {
      XmlNode node = _doc.SelectSingleNode("//Table");
      int curValue = Convert.ToInt32(node.Attributes["ExpandedRowCount", "urn:schemas-microsoft-com:office:spreadsheet"].Value);
      node.Attributes["ExpandedRowCount", "urn:schemas-microsoft-com:office:spreadsheet"].Value =
        TotalRowCount(manager).ToString();
    }

    /// <summary>
    /// ��������� ������ �� ������ � ��������
    /// </summary>
    /// <param name="node"></param>
    /// <param name="rowIndex"></param>
    /// <param name="insertedRowIndex"></param>
    /// <param name="increment"></param>
    private void ChangeFormulaReference(XmlNamespaceManager manager, XmlNode insertedNode, int insertedRowIndex)
    {
      //  ss:Formula="=SUM(R[-2]C:R[-1]C)"
      //XmlNode node = insertedNode.NextSibling;
      XmlNodeList nList = insertedNode.SelectNodes("//Table/Row/Cell[@ss:Formula]", manager);
      foreach (XmlNode node in nList)
      {
        if (node.Attributes["Formula", "urn:schemas-microsoft-com:office:spreadsheet"] == null)
          continue;
        string str = node.Attributes["Formula", "urn:schemas-microsoft-com:office:spreadsheet"].Value;
        if (!str.Contains("=SUM"))
          continue;
        //if (!node.Attributes["Formula", "urn:schemas-microsoft-com:office:spreadsheet"].Value.Contains("=Sum"))
        //  continue;

        XmlNode parent = node.ParentNode;
        int rowIndex = 0;
        try
        {
          rowIndex = Convert.ToInt32(parent.Attributes["Index", "urn:schemas-microsoft-com:office:spreadsheet"].Value);

        }
        catch
        {
          rowIndex = FindRowIndex(manager, parent);
        }
        //return;

        try
        {
          string attValue = node.Attributes["Formula", "urn:schemas-microsoft-com:office:spreadsheet"].Value;
          string[] rcArray = attValue.Split(":".ToCharArray());
          string newAttValue = attValue;

          int leftOpenBracketInd = rcArray[0].IndexOf("R[");
          int leftCloseBracketInd = rcArray[0].IndexOf("]");
          int rightOpenBracketInd = rcArray[1].IndexOf("R[");
          int rightCloseBracketInd = rcArray[1].IndexOf("]");

          int leftR = Convert.ToInt32(rcArray[0].Substring(leftOpenBracketInd + 2,
            (leftCloseBracketInd - leftOpenBracketInd - 2)));
          int rightR = Convert.ToInt32(rcArray[1].Substring(rightOpenBracketInd + 2,
            (rightCloseBracketInd - rightOpenBracketInd - 2)));

          int newLeftR = leftR;
          int newRightR = rightR;
          if (rowIndex + leftR <= insertedRowIndex)    //  ������ ����� ������
          {
            newLeftR--;
          }
          if (rowIndex + rightR > insertedRowIndex)    //  ������ ������ ������
          {
            newRightR--;
          }

          newAttValue = string.Concat(
            rcArray[0].Replace(string.Format("R[{0}]", leftR), string.Format("R[{0}]", newLeftR)),
            ":".ToString(), rcArray[1].Replace(string.Format("R[{0}]", rightR), string.Format("R[{0}]", newRightR)));

          //attValue.Replace(string.Format("R[{0}]", refRowIndex), string.Format("R[{0}]", refRowIndex + 1));

          //int refRowIndex = Convert.ToInt32(attValue.Substring(openBracketInd + 1, (closeBracketInd - openBracketInd - 1)));
          //newAttValue = attValue.Replace(string.Format("R[{0}]", refRowIndex), string.Format("R[{0}]", refRowIndex + 1));
          node.Attributes["Formula", "urn:schemas-microsoft-com:office:spreadsheet"].Value = newAttValue;
        }
        catch { }
      }


      /*
      while (node != null)
      {
        if (!(node.Attributes["Formula", "urn:schemas-microsoft-com:office:spreadsheet"] == null))
        {
          string attValue = node.Attributes["Formula", "urn:schemas-microsoft-com:office:spreadsheet"].Value;
          string[] rcArray = attValue.Split(":".ToCharArray());
          string newAttValue = attValue;
          
          int leftOpenBracketInd = rcArray[0].IndexOf("R[");
          int leftCloseBracketInd = rcArray[0].IndexOf("]");
          int rightOpenBracketInd = rcArray[1].IndexOf("R[");
          int rightCloseBracketInd = rcArray[1].IndexOf("]");

          int leftR = Convert.ToInt32(rcArray[0].Substring(leftOpenBracketInd + 1,
            (leftCloseBracketInd - leftOpenBracketInd - 1)));
          int rightR = Convert.ToInt32(rcArray[1].Substring(rightOpenBracketInd + 1,
            (rightCloseBracketInd - rightOpenBracketInd - 1)));

          int newLeftR = leftR;
          int newRightR = rightR;
          if (leftR > insertedRowIndex)    //  ������ ����� ������
          {
            newLeftR++;
          }
          if (rightR > insertedRowIndex)    //  ������ ������ ������
          {
            newRightR++;
          }
          newAttValue = string.Concat(rcArray[0].Replace(string.Format("R[{0}]", leftR), string.Format("R[{0}]", newLeftR)),
            ":".ToString(), string.Format("R[{0}]", rightR), string.Format("R[{0}]", newRightR));
            
            //attValue.Replace(string.Format("R[{0}]", refRowIndex), string.Format("R[{0}]", refRowIndex + 1));

          //int refRowIndex = Convert.ToInt32(attValue.Substring(openBracketInd + 1, (closeBracketInd - openBracketInd - 1)));
            //newAttValue = attValue.Replace(string.Format("R[{0}]", refRowIndex), string.Format("R[{0}]", refRowIndex + 1));
          node.Attributes["Formula", "urn:schemas-microsoft-com:office:spreadsheet"].Value = newAttValue;
        }       
        node = node.NextSibling;
      }
      */


      //  ������� ������ �������������� - ������ ��� ������ ���������
      //if (node.Attributes["Formula", "urn:schemas-microsoft-com:office:spreadsheet"] == null)
      //  return;
      //string attValue = node.Attributes["Formula", "urn:schemas-microsoft-com:office:spreadsheet"].Value;
      //string newAttValue = attValue;
      //string[] rcArray = attValue.Split(":".ToCharArray());
      //int openBracketInd = attValue.IndexOf("R[");
      //int closeBracketInd = attValue.IndexOf("]");
      //int refRowIndex = Convert.ToInt32(attValue.Substring(openBracketInd + 1, (closeBracketInd - openBracketInd - 1)));
      //if (rowIndex + refRowIndex > insertedRowIndex) //  ������ ��������
      //  newAttValue = attValue.Replace(string.Format("R[{0}]", refRowIndex), string.Format("R[{0}]", refRowIndex + increment));
      //node.Attributes["Formula", "urn:schemas-microsoft-com:office:spreadsheet"].Value = newAttValue;


      /*
      // ss:Formula="=SUM(R[-8]C6:R[-5]C6)"
      if (node.Attributes["Formula", "urn:schemas-microsoft-com:office:spreadsheet"] == null)
        return;

      string attValue = node.Attributes["Formula", "urn:schemas-microsoft-com:office:spreadsheet"].Value;
      string newAttValue = attValue;
      //  ���� �� ���� ������. ���� ��������� "R[", ��, ���� rowIndex + X <= insertedRowIndex,
      //  �� �� ������ ��������; � ��������� ������ ����������� �� �������� increment
      while(attValue.IndexOf("R[") != -1)
      {
        int openBracketInd = attValue.IndexOf("R[");
        int closeBracketInd = attValue.IndexOf("]");
        int refRowIndex = Convert.ToInt32(attValue.Substring(openBracketInd+1, (closeBracketInd - openBracketInd -1)));
        if(rowIndex + refRowIndex > insertedRowIndex) //  ������ ��������
          newAttValue = attValue.Replace(string.Format("R[{0}]", refRowIndex), string.Format("R[{0}]", refRowIndex + increment));
        //  ���� ��������!
        //attValue = attValue.Re
      }
      */

      //node.Attributes["index", "urn:schemas-microsoft-com:office:spreadsheet"].Value = (index + increment).ToString();
    }
  }

  /// <summary>
  /// ����� ��� �������� �������� ��������� �� ������������� ������
  /// </summary>
  public class NamedRange
  {
    /// <summary>
    /// ��� ������
    /// </summary>
    private string _name;
    public string pName
    {
      get { return _name; }
      set { _name = value; }
    }
    /// <summary>
    /// ��� ����������� ��������
    /// </summary>
    private string _type;
    public string pType
    {
      get { return _type; }
      set { _type = value; }
    }
    /// <summary>
    /// ������ ������
    /// </summary>
    private int _rowIndex;
    public int pRowIndex
    {
      get { return _rowIndex; }
      set { _rowIndex = value; }
    }
    /// <summary>
    /// ������ �������
    /// </summary>
    private int _columnIndex;
    public int pColumnIndex
    {
      get { return _columnIndex; }
      set { _columnIndex = value; }
    }
    /// <summary>
    /// ����������� ������ 
    /// </summary>
    private Direction _direction;
    public Direction pDirection
    {
      get { return _direction; }
      set { _direction = value; }
    }
    /// <summary>
    /// ������ �� � ������������� ������
    /// </summary>
    private bool _included;
    public bool pIncluded
    {
      get { return _included; }
      set { _included = value; }
    }
    /// <summary>
    /// ������������� ��������
    /// </summary>
    private bool _multiple;
    public bool pMultiple
    {
      get { return _multiple; }
      set { _multiple = value; }
    }
    /// <summary>
    /// �������, �� ������� ����� ��������
    /// </summary>
    private int _tableIndex;
    public int pTableIndex
    {
      get { return _tableIndex; }
      set { _tableIndex = value; }
    }
    /// <summary>
    /// ��� ����
    /// </summary>
    private string _field;
    public string pField
    {
      get { return _field; }
      set { _field = value; }
    }
    /// <summary>
    /// ����� ������ ������ - ����� ���������� �������������� �����
    /// </summary>
    private int _newRowIndex;
    public int pNewRowIndex
    {
      get { return _newRowIndex; }
      set { _newRowIndex = value; }
    }
    /// <summary>
    /// ����������� ��� �� ����������� ������
    /// </summary>
    private bool _isEmpty;
    public bool pIsEmpty
    {
      get { return _isEmpty; }
      set { _isEmpty = value; }
    }
    /// <summary>
    /// ����� ������ - ����������� �� �� ������ ����� ����� ������
    /// </summary>
    private bool _styleInhereted;
    public bool pStyleInhereted
    {
      get { return _styleInhereted; }
      set { _styleInhereted = value; }
    }
    /// <summary>
    /// ����� ������
    /// </summary>
    private string _styleId;
    public string pStyleId
    {
      get { return _styleId; }
      set { _styleId = value; }
    }

    public NamedRange()
    {
      _type = "String";
      _isEmpty = false;
      _direction = Direction.inside;
      _field = string.Empty;
      _included = true;
      _multiple = false;
      _tableIndex = 0;
      _styleInhereted = false;
      _styleId = string.Empty;
    }
  }

  public enum Direction
  {
    inside = 0,
    top,
    bottom,
    right,
    left
  }
}
