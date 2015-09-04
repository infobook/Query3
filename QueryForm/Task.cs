using System;
using System.Data;
using System.Data.OleDb;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.Diagnostics;
using CommandAS.Tools;
using CommandAS.QueryLib;

namespace CommandAS.QueryForm
{
  /// <summary>
  /// 
  /// </summary>
  public class Task : Performer
  {
    //public static string  Version
    //{
    //	get { return Performer.VERSION; }
    //}

    public string pCode
    {
      [DebuggerStepThrough]
      get { return mSes.Code.ToString(); }
      set { mSes.Code = CASTools.ConvertToInt32Or0(value); }
    }
    public string pTitle
    {
      [DebuggerStepThrough]
      get { return mSes.Title; }
      [DebuggerStepThrough]
      set { mSes.Title = value; }
    }
    public string pNote
    {
      [DebuggerStepThrough]
      get { return mSes.Note.Replace("\n", Environment.NewLine); }
      [DebuggerStepThrough]
      set { mSes.Note = value; }
    }
    public string pDBConnectionString
    {
      [DebuggerStepThrough]
      get { return mSes.DBConnection.Replace("\n", Environment.NewLine); }
      set
      {
        mSes.DBConnection = value;
        mDB.pConnectionString = value;
      }
    }
    public string pParamBegDelim
    {
      [DebuggerStepThrough]
      get { return mSes.ParamBegDelim; }
      [DebuggerStepThrough]
      set { mSes.ParamBegDelim = value; }
    }
    public string pParamEndDelim
    {
      [DebuggerStepThrough]
      get { return mSes.ParamEndDelim; }
      [DebuggerStepThrough]
      set { mSes.ParamEndDelim = value; }
    }
    public string pImageName
    {
      [DebuggerStepThrough]
      get { return mSes.ImageName; }
      [DebuggerStepThrough]
      set { mSes.ImageName = value; }
    }
    public string pImagePath
    {
      [DebuggerStepThrough]
      get { return mSes.ImagePath; }
      [DebuggerStepThrough]
      set { mSes.ImagePath = value; }
    }
    //public string				pXSLTIncludeName
    //{
    //	[DebuggerStepThrough]
    //	get { return mSes.XSLTIncludeName; }
    //	[DebuggerStepThrough]
    //	set { mSes.XSLTIncludeName = value;}
    //}
    //public string				pXSLTIncludeText
    //{
    //	[DebuggerStepThrough]
    //	get { return mSes.XSLTIncludeText.Replace("\n", Environment.NewLine); }
    //	[DebuggerStepThrough]
    //	set { mSes.XSLTIncludeText = value;}
    //}
    public string pHash
    {
      [DebuggerStepThrough]
      get { return mSes.Hash; }
      [DebuggerStepThrough]
      set { mSes.Hash = value; }
    }

    public Task() : this(new WorkDB()) { }
    public Task(WorkDB aDB)
      : base(aDB)
    {
      mSes = new Session();
      pFileName = string.Empty;
    }

    public bool DBConnection()
    {
      mErr.Clear();
      mDB.ConnectionClose();
      if (!mDB.ConnectionOpen())
        mErr = mDB.pError;
      return mErr.IsOk;
    }

    public bool New()
    {
      bool ret = true;
      mSes = new Session();
      pFileName = string.Empty;
      return ret;
    }

    public void Save()
    {
      SaveAs(pFileName);
    }
    public void SaveAs(string aFileName)
    {
      pFileName = aFileName;
      mErr.Clear();

      if (pFileName.Length > 0)
      {
        try
        {
          pFileName =
            Path.GetDirectoryName(pFileName)
            + Path.DirectorySeparatorChar.ToString()
            + Path.GetFileNameWithoutExtension(pFileName)
            + Performer.CURRENT_SESSION_EXT;
          TextWriter tr = new StreamWriter(pFileName, false, Encoding.Default);
          XmlSerializer xs = new XmlSerializer(typeof(Session));
          xs.Serialize(tr, mSes);
          tr.Close();
        }
        catch (Exception ex)
        {
          mErr.ex = ex;
        }
      }
    }

    public void AddQuery(Query aQr)
    {
      mSes.Queries.Add(aQr);
    }

    public void DeleteQuery(Query aQr)
    {
      mSes.Queries.Remove(aQr);
    }

    public OleDbCommand NewOleDbCommand(string aSQL)
    {
      return mDB.NewOleDbCommand(aSQL);
    }

    public int GetNextQueryCode()
    {
      return mSes.GetNextQueryCode();
    }
    public bool IsCorrectQueryCode(int aCode, Query aQ)
    {
      return mSes.IsCorrectQueryCode(aCode, aQ);
    }

    #region Old ...
    /*

			mDSS = new DataSet();
			System.Reflection.Assembly aID = System.Reflection.Assembly.GetExecutingAssembly();
			System.IO.Stream sm = aID.GetManifestResourceStream ("CommandAS.Query2.Session.xsd");
			mDSS.ReadXmlSchema(sm);

			mDSS = new DataSet();
			mDSS.ReadXml(_fileName);
		
		private void Load()
		{
			XmlDocument	xmlDoc = new XmlDocument();
			xmlDoc.Load(_sesFileName);
			
			XmlNode xp = xmlDoc.SelectSingleNode("//Session");

			XmlNode xn = xp.SelectSingleNode("//Title");
			if (xn != null)
				_txtTitle.Text = xn.InnerText;
			xn = xp.SelectSingleNode("//Connection");
			if (xn != null)
				_txtConnection.Text = xn.InnerText;
			xn = xp.SelectSingleNode("//ShowQueryText");
			if (xn != null)
			{
				_chkShowQueryText.Checked = bool.Parse(xn.InnerText);
				DoHideSQLTextCheckChanged(_chkShowQueryText, new EventArgs());
			}
			else
				_chkShowQueryText.Checked = true;
			xn = xp.SelectSingleNode("//ShowHiddenQuery");
			if (xn != null)
				_chkShowHidden.Checked = bool.Parse(xn.InnerText);
			else
				_chkShowHidden.Checked = true;
			xn = xp.SelectSingleNode("//ListView");
			if (xn != null)
			{
				_tvQueries.View = (View)Enum.Parse(typeof(View), xn.InnerText);
				switch(_tvQueries.View)
				{
					case View.SmallIcon:
						_cboLView.SelectedIndex = 0;
						break;
					case View.LargeIcon:
						_cboLView.SelectedIndex = 1;
						break;
					case View.List:
						_cboLView.SelectedIndex = 2;
						break;
				}
			}
			else
				_tvQueries.View = View.Details;

			XmlNode xx = xp.SelectSingleNode("//Font");
			if (xn != null)
			{
				try
				{
					_sesFont = new Font(
						xx.SelectSingleNode("Name").InnerText,
						float.Parse(xx.SelectSingleNode("Size").InnerText),
						(FontStyle) Enum.Parse(typeof(FontStyle), xx.SelectSingleNode("Style").InnerText)
						);
					ChangeFont();
				}
				catch {}
			}

			_alQuery.Clear();
			_lastSelQryIndex = -1;
			qItem qi = null;
			int ii = 0;
			foreach(XmlElement qxn in xp.SelectNodes("//Query"))
			{
				xx = qxn.SelectSingleNode("Name");
				if (xx != null && xx.InnerText.Length > 0)
				{
					qi = new qItem();
					qi.pID = ++ii;
					qi.pName = xx.InnerText;
					xx = qxn.SelectSingleNode("Date//Create");
					if (xx != null)
						qi.pDateCreate = xx.InnerText;
					xx = qxn.SelectSingleNode("Date//LastModified");
					if (xx != null) // 2
						qi.pDateModified = xx.InnerText;
					xx = qxn.SelectSingleNode("Hidden");
					if (xx != null) // 3
						qi.pHidden = bool.Parse(xx.InnerText);
					xx = qxn.SelectSingleNode("Text");
					if (xx != null) // 4
						qi.pSQLText = xx.InnerText;
					_alQuery.Add(qi);
				}
			}
			ShowQueryListView();
			_chkDefaultSession.Checked = _sesFileName.Equals(_sesDefault);
			_modified = false;
			_mnuExec.Enabled = false;
			_tbbExec.Enabled = false;
			_mnuDeleteQuery.Enabled = false;
			_tbbDelQuery.Enabled = false;
			_sb.Text = "Данные сессии успешно загружены.";
		}

		private void Save()
		{
			XmlDocument	xmlDoc = new XmlDocument();
			xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"Windows-1251\"?>\r\n<Session></Session>");

			XmlElement elem = xmlDoc.CreateElement("Title");
			elem.InnerText = _txtTitle.Text;
			xmlDoc.DocumentElement.AppendChild(elem);

			elem = xmlDoc.CreateElement("Connection");
			elem.InnerText = _txtConnection.Text;
			xmlDoc.DocumentElement.AppendChild(elem);

			elem = xmlDoc.CreateElement("ShowQueryText");
			elem.InnerText = _chkShowQueryText.Checked.ToString();
			xmlDoc.DocumentElement.AppendChild(elem);

			elem = xmlDoc.CreateElement("ShowHiddenQuery");
			elem.InnerText = _chkShowHidden.Checked.ToString();
			xmlDoc.DocumentElement.AppendChild(elem);
				
			elem = xmlDoc.CreateElement("ListView");
			elem.InnerText = _tvQueries.View.ToString();
			xmlDoc.DocumentElement.AppendChild(elem);

			XmlElement qelem = xmlDoc.CreateElement("Font");
			xmlDoc.DocumentElement.AppendChild(qelem);

			elem = xmlDoc.CreateElement("Name");
			elem.InnerText = _sesFont.FontFamily.Name;
			qelem.AppendChild(elem);
			elem = xmlDoc.CreateElement("Size");
			elem.InnerText = _sesFont.Size.ToString();
			qelem.AppendChild(elem);
			elem = xmlDoc.CreateElement("Style");
			elem.InnerText = _sesFont.Style.ToString();
			qelem.AppendChild(elem);

			XmlElement delem;
			DoSelectedQueryChanged(this, new EventArgs());
			foreach(qItem qi in _alQuery)
			{
				qelem = xmlDoc.CreateElement("Query");
				xmlDoc.DocumentElement.AppendChild(qelem);

				elem = xmlDoc.CreateElement("Name");
				elem.InnerText = qi.pName;
				qelem.AppendChild(elem);

				delem = xmlDoc.CreateElement("Date");
				qelem.AppendChild(delem);

				elem = xmlDoc.CreateElement("Create");
				elem.InnerText = qi.pDateCreate;
				delem.AppendChild(elem);

				elem = xmlDoc.CreateElement("LastModified");
				elem.InnerText = qi.pDateModified;
				delem.AppendChild(elem);

				elem = xmlDoc.CreateElement("Hidden");
				elem.InnerText = qi.pHidden.ToString();
				qelem.AppendChild(elem);

				elem = xmlDoc.CreateElement("Text");
				elem.InnerText = qi.pSQLText;
				qelem.AppendChild(elem);
			}

			xmlDoc.Save(_sesFileName);
			_modified = false;
			_sb.Text = "Данные сессии успешно сохранены.";
		}
		*/
    #endregion
  }
}
