using System;
using System.Data;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using CommandAS.Tools;

namespace CommandAS.QueryLib
{
  /// <summary>
  /// ���� ����������
  /// </summary>
  public enum eQueryParamType
  {
    /// <summary>
    /// �� ���������.
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// ������.
    /// </summary>
    String = 1,
    /// <summary>
    /// ������ (ComboBox) ���������.<BR/>
    /// ������ ���� �� ��� ����������� ����� ������ (������������ � ������).
    /// </summary>
    StrSelectList = 2,
    /// <summary>
    /// �������� (TreeView) ���������.<BR/>
    /// ������ ���� �� ��� ����������� ����� ������ (�� ������������ � ������).
    /// </summary>
    StrSelectTree = 3,
    /// <summary>
    /// ����� �����.
    /// </summary>
    Integer = 4,
    /// <summary>
    /// ������ ���������. 
    /// ������ ���� �� ��� ����������� ����� ����� ����� (�� ������������, ��� �������).
    /// </summary>
    IntSelectList = 5,
    /// <summary>
    /// �������� (TreeView) ���������.<BR/>
    /// ������ ���� �� ��� ����������� ����� ����� ����� (�� ������������ � ������).
    /// </summary>
    IntSelectTree = 6,
    /// <summary>
    /// ������������� - ��/���.
    /// </summary>
    Boolean = 7,
    /// <summary>
    /// ����
    /// </summary>
    Date = 8,
    /// <summary>
    /// ����������� ��� PlaceCode. ��� ����� ����� ���������� ����� ����������� ":".
    /// </summary>
    PlaceCode = 9,
    /// <summary>
    /// 
    /// </summary>
    PCSelectList = 10,
    /// <summary>
    ///
    /// </summary>
    PCSelectTree = 11
  }

  /// <summary>
  /// �������� �������.
  /// </summary>
  public class Param
  {
    /// <summary>
    /// ����� �������. ����� �������������� ��� �������������� ����������.
    /// </summary>
    public int Number;
    /// <summary>
    /// ������� (������������) ��� ���������.
    /// </summary>
    public string Title;
    /// <summary>
    /// ���������� (������������ � SQL) ��� ���������.
    /// </summary>
    public string Name;
    /// <summary>
    /// ��� ���������. ��. ������������ eQueryParamType.
    /// </summary>
    public eQueryParamType Type;
    /// <summary>
    /// ������ ��������� ���������.
    /// False - ����������� ��� OLEDB ������� ���������� ��� SQL �������.
    /// True - ������� ������ ������.
    /// �������� ��������� ������ ��� SQL, � XSLT ������� ������ ����������� ������� ������. 
    /// </summary>
    public bool Inset;
    /// <summary>
    /// ������� �������� ���������.
    /// </summary>
    public string CurrentValue;
    /// <summary>
    /// �������� �����������.
    /// </summary>
    public string DefaultValue;
    /// <summary>
    /// SQL ������ ��� ������������ ���������� �������� ���������.
    /// </summary>
    public string SelectValue;

    /// <summary>
    /// ����������� �� ���������.
    /// </summary>
    public Param()
    {
      Number = 0;
      Title = string.Empty;
      Name = string.Empty;
      Type = eQueryParamType.Undefined;
      Inset = false;
      CurrentValue = string.Empty;
      DefaultValue = string.Empty;
      SelectValue = string.Empty;
    }
    /// <summary>
    /// ������������ �������������� � ������ 2.
    /// </summary>
    /// <param name="aPrm">�������� � ������� ������ 2.</param>
    public Param(CommandAS.QueryLib2.Param aPrm)
    {
      Number = 0;
      Title = aPrm.Title;
      Name = aPrm.Name;
      Type = aPrm.Type;
      Inset = false;
      CurrentValue = aPrm.DefaultValue;
      DefaultValue = aPrm.DefaultValue;
      SelectValue = aPrm.SelectValue;
    }

    #region Current Value
    [XmlIgnoreAttribute]
    public bool ValBool
    {
      get
      {
        bool ret = false;
        try { ret = Convert.ToBoolean(CurrentValue); }
        catch { }
        return ret;
      }
      set { CurrentValue = value.ToString(); }
    }
    [XmlIgnoreAttribute]
    public DateTime ValDate
    {
      get
      {
        //return Convert.ToDateTime(CurrentValue); 
        DateTime ret = DateTime.Today;
        try { ret = DateTime.Parse(CurrentValue); }
        catch { }
        return ret;
      }
      set
      {
        if (SelectValue.Length == 0)
          CurrentValue = value.ToShortDateString();
        else
        {
          //bool isDateFormat = false;
          try
          {
            CurrentValue = value.ToString(SelectValue);
            //isDateFormat = true;
          }
          catch {}

          // �� ������ ������� ������� �������� GETDATE()
          //if (!isDateFormat)
          //{
          //  if (SelectValue.ToLower().IndexOf("today") >= 0 || SelectValue.ToLower().IndexOf("getdate") >= 0)
          //    CurrentValue = DateTime.Today.ToShortDateString();
          //  else if (SelectValue.ToLower().IndexOf("firstmonthday") >= 0)
          //    CurrentValue = new DateTime(value.Year, value.Month, 1).ToShortDateString();
          //  else if (SelectValue.ToLower().IndexOf("lastmonthday") >= 0)
          //    CurrentValue = CASTools.DateLastDayOfMonth(value.Year, value.Month).ToShortDateString();
          //}
        }
      }
    }
    [XmlIgnoreAttribute]
    public string ValStr
    {
      get
      {
        string ret = string.Empty;
        switch (Type)
        {
          case eQueryParamType.String:
            ret = CurrentValue;
            break;
          case eQueryParamType.StrSelectTree:
          case eQueryParamType.StrSelectList:
            {
              _ListBoxTextItem pci = new _ListBoxTextItem(CurrentValue);
              ret = pci.text1;
              break;
            }
        }
        return ret;
      }
      set { CurrentValue = value; }
    }
    [XmlIgnoreAttribute]
    public int ValInt
    {
      get
      {
        int ret = 0;
        switch (Type)
        {
          /// Modified by M.Tor 25.06.2008:
          //case eQueryParamType.IntSelectTree: // ���� !!!
          case eQueryParamType.Integer:
            ret = CASTools.ConvertToInt32Or0(CurrentValue);
            break;
          /// Modified by M.Tor 25.06.2008:
          case eQueryParamType.IntSelectTree:
          case eQueryParamType.IntSelectList:
            {
              _ListBoxItem pci = new _ListBoxItem(CurrentValue);
              ret = pci.code;
              break;
            }
        }
        return ret;
      }
      set { CurrentValue = value.ToString(); }
    }
    [XmlIgnoreAttribute]
    public PlaceCode ValPC
    {
      get
      {
        PlaceCode ret = PlaceCode.Empty;
        switch (Type)
        {
          /// Modified by M.Tor 25.06.2008:
          //case eQueryParamType.PCSelectTree: // ��������� ���!!! (���� "����") !!!
          //  {
          //    string[] s = CurrentValue.Split(PlaceCode.DELIM);
          //    if (s.Length > 1)
          //    {
          //      ret.place = CASTools.ConvertToInt32Or0(s[0]);
          //      ret.code = CASTools.ConvertToInt32Or0(s[1]);
          //    }
          //  }
          //  break;
		      case eQueryParamType.PlaceCode:
            ret = PlaceCode.PDC2PlaceCode(CurrentValue);
            break;
          /// Modified by M.Tor 25.06.2008:
          case eQueryParamType.PCSelectTree:
          case eQueryParamType.PCSelectList:
            {
              _ListBoxPCItem pci = new _ListBoxPCItem(CurrentValue);
              ret = pci.pPC;
              break;
            }
        }
        return ret;
      }
      //set { CurrentValue = PlaceCode.PlaceCode2PDC(value); }
    }
    [XmlIgnoreAttribute]
    public string ValCurrent
    {
      get
      {
        string ret = string.Empty;
        switch (Type)
        {
          case eQueryParamType.Boolean:
          case eQueryParamType.Date:
          case eQueryParamType.String:
          case eQueryParamType.Integer:
          case eQueryParamType.PlaceCode:
            ret = CurrentValue;
            break;
          case eQueryParamType.IntSelectList:
          case eQueryParamType.IntSelectTree:
            ret = ValInt.ToString();
            break;
          case eQueryParamType.PCSelectList:
          case eQueryParamType.PCSelectTree:
            ret = ValPC.ToString();
            break;
          case eQueryParamType.StrSelectList:
          case eQueryParamType.StrSelectTree:
            ret = ValStr;
            break;
        }
        return ret;
      }
    }
    #endregion value

  }

  /// <summary>
  /// ������.
  /// </summary>
  public class Query
  {
    /// <summary>
    /// ���������� ������������� �������.
    /// </summary>
    public int Code;
    /// <summary>
    /// �������� �������.
    /// </summary>
    public string Name;
    /// <summary>
    /// ���� �������� �������.
    /// </summary>
    public DateTime DateCreate;
    /// <summary>
    /// ���� ���������� ��������� �������.
    /// </summary>
    public DateTime DateLastModified;
    /// <summary>
    /// ���� ��������� �������. 
    /// true - ����� � ���������������� ������
    /// false - ������� � ���������������� ������
    /// </summary>
    public bool Hidden;
    /// <summary>
    /// ����� �������.
    /// </summary>
    public string Author;
    /// <summary>
    /// ��������� � �������.
    /// </summary>
    public string Note;
    /// <summary>
    /// ����� �������. SQL ���������.
    /// </summary>
    public string Text;
    /// <summary>
    /// ����� XSLT ��������������.
    /// </summary>
    public string XSLT;
    /// <summary>
    /// ��� ����� � ������������ (*.ico, *.jpg � �.�.) ��� �������.
    /// </summary>
    public string ImageName;
    /// <summary>
    /// ��������� ����������.
    /// </summary>
    [XmlArrayItem("Param", typeof(Param))]
    public ArrayList Params;

    /// <summary>
    /// ����������� �� ���������.
    /// </summary>
    public Query()
    {
      Code = 0;
      Name = "New Query (v3)";
      DateCreate = DateTime.Now;
      DateLastModified = DateTime.Now;
      Hidden = false;
      Author = string.Empty;
      Note = string.Empty;
      Text = string.Empty;
      XSLT = string.Empty;
      ImageName = string.Empty;

      Params = new ArrayList(4);
    }
    /// <summary>
    /// ������������ �������������� � ������ 2.
    /// </summary>
    /// <param name="aPrm">������ � ������� ������ 2.</param>
    public Query(CommandAS.QueryLib2.Query aQr)
    {
      Code = 0;
      Name = aQr.Name;
      DateCreate = aQr.DateCreate;
      DateLastModified = aQr.DateLastModified;
      Hidden = aQr.Hidden;
      Author = aQr.Author;
      Note = aQr.Note;
      Text = aQr.Text;
      XSLT = aQr.XSLT;
      ImageName = string.Empty;

      Params = new ArrayList(aQr.Params.Count);
      foreach (CommandAS.QueryLib2.Param p2 in aQr.Params)
        Params.Add(new Param(p2));

    }
    /// <summary>
    /// ���������� ������� � �����.
    /// </summary>
    /// <param name="aFileName">��� �����.</param>
    public void SaveAs(string aFileName)
    {
      TextWriter tr = new StreamWriter(aFileName, false, Encoding.Default);
      XmlSerializer xs = new XmlSerializer(typeof(Query));
      xs.Serialize(tr, this);
      tr.Close();
    }
    /// <summary>
    /// �������� ������� �� ����.
    /// </summary>
    /// <param name="aFileName">��� �����.</param>
    /// <returns>����������� ������.</returns>
    public static Query Load(string aFileName)
    {
      Query q = new Query();

      FileStream f = new FileStream(aFileName, FileMode.Open);
      XmlSerializer xs = new XmlSerializer(typeof(Query));
      q = (Query)xs.Deserialize(f);

      f.Close();

      return q;
    }
  }

  /// <summary>
  /// ������ - ������������ �������� ������������, ��� �������, ��� ����� ����������� � ��
  /// ��� ������� ������.
  /// </summary>
  public class Session
  {
    /// <summary>
    /// ���������� ������������� ������.
    /// </summary>
    public int Code;
    /// <summary>
    /// �������� ������.
    /// </summary>
    public string Title;
    /// <summary>
    /// ����������� � ������.
    /// </summary>
    public string Note;
    /// <summary>
    /// ������ ���������� � ��.
    /// </summary>
    public string DBConnection;
    /// <summary>
    /// ��������� ������� ����������.
    /// </summary>
    public string ParamBegDelim;
    /// <summary>
    /// �������� ������� ����������.
    /// </summary>
    public string ParamEndDelim;
    /// <summary>
    /// ��� ����� ������ ��� ����� � ����� ��������������/�������������� ������.
    /// </summary>
    public string Hash;
    /// <summary>
    /// ��������������� ����������� ������������ � ������. 
    /// </summary>
    public string ImagePath;
    /// <summary>
    /// ��� ����� � ������������ (*.ico, *.jpg � �.�.) ��� ������.
    /// </summary>
    public string ImageName;

    /// <summary>
    /// ��������� ����� ����������.
    /// </summary>
    [XmlArrayItem("Param", typeof(Param))]
    public ArrayList Params;

    [XmlArrayItem("Query", typeof(Query))]
    public ArrayList Queries;


    /// <summary>
    /// �������� ���������������� � ����� XSLT ��������������
    /// </summary>
    //[XmlIgnoreAttribute]
    //public string				XSLTIncludeName;
    /// <summary>
    /// ���������� ���������������� � ����� XSLT ��������������
    /// </summary>
    //[XmlIgnoreAttribute]
    //public string				XSLTIncludeText;

    /// <summary>
    /// ����������� �� ���������.
    /// </summary>
    public Session()
    {
      Code = 0;
      Title = "New Session (v3)";
      Note = string.Empty;
      DBConnection = string.Empty;
      ParamBegDelim = CommandAS.QueryLib2.Session.PARAM_BEG_DELIM_DEFAULT;
      ParamEndDelim = CommandAS.QueryLib2.Session.PARAM_END_DELIM_DEFAULT;
      Hash = string.Empty;
      //XSLTIncludeName = string.Empty;
      //XSLTIncludeText	= string.Empty;
      ImagePath = string.Empty;
      ImageName = string.Empty;

      Params = new ArrayList(4);
      Queries = new ArrayList(8);
    }
    /// <summary>
    /// ������������ �������������� � ������ 2.
    /// </summary>
    /// <param name="aPrm">������ � ������� ������ 2.</param>
    public Session(CommandAS.QueryLib2.Session aSes)
    {
      Code = 0;
      Title = aSes.Title;
      Note = string.Empty;
      DBConnection = aSes.DBConnection;
      ParamBegDelim = aSes.ParamBegDelim;
      ParamEndDelim = aSes.ParamEndDelim;
      Hash = string.Empty;
      //XSLTIncludeName = string.Empty;
      //XSLTIncludeText	= string.Empty;
      ImagePath = string.Empty;
      ImageName = string.Empty;

      Params = new ArrayList(4);
      Queries = new ArrayList(8);
      foreach (CommandAS.QueryLib2.Query q2 in aSes.Queries)
        Queries.Add(new Query(q2));

    }

    public int GetNextQueryCode()
    {
      int ret = 0;

      foreach (Query q in Queries)
        if (q.Code > ret)
          ret = q.Code;

      return ret + 1;
    }

    public bool IsCorrectQueryCode(int aCode, Query aQ)
    {
      bool ret = true;

      foreach (Query q in Queries)
        if (q.Code == aCode && aQ != q)
        {
          ret = false;
          break;
        }

      return ret;
    }
  }
}
