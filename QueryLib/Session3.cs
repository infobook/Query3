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
  /// Типы параметров
  /// </summary>
  public enum eQueryParamType
  {
    /// <summary>
    /// Не определен.
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// Строка.
    /// </summary>
    String = 1,
    /// <summary>
    /// Список (ComboBox) элементов.<BR/>
    /// Выбров один из них результатом будет строка (отображаемая в списке).
    /// </summary>
    StrSelectList = 2,
    /// <summary>
    /// Иерархия (TreeView) элементов.<BR/>
    /// Выбров один из них результатом будет строка (не отображаемая в списке).
    /// </summary>
    StrSelectTree = 3,
    /// <summary>
    /// Целое число.
    /// </summary>
    Integer = 4,
    /// <summary>
    /// Список элементов. 
    /// Выбров один из них результатом будет целое число (не отображаемое, как правило).
    /// </summary>
    IntSelectList = 5,
    /// <summary>
    /// Иерархия (TreeView) элементов.<BR/>
    /// Выбров один из них результатом будет целое число (не отображаемое в списке).
    /// </summary>
    IntSelectTree = 6,
    /// <summary>
    /// Переключатель - ДА/НЕТ.
    /// </summary>
    Boolean = 7,
    /// <summary>
    /// Дата
    /// </summary>
    Date = 8,
    /// <summary>
    /// Производный тип PlaceCode. Два целых числа записанных через разделитель ":".
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
  /// Параметр запроса.
  /// </summary>
  public class Param
  {
    /// <summary>
    /// Номер запроса. Может использоваться для упорядочивания параметров.
    /// </summary>
    public int Number;
    /// <summary>
    /// Внешнее (отображаемое) имя параметра.
    /// </summary>
    public string Title;
    /// <summary>
    /// Внутреннее (используемое в SQL) имя параметра.
    /// </summary>
    public string Name;
    /// <summary>
    /// Тип параметра. См. перечисление eQueryParamType.
    /// </summary>
    public eQueryParamType Type;
    /// <summary>
    /// Способ установки параметра.
    /// False - стандартная для OLEDB встатка параметров для SQL запроса.
    /// True - простая замена текста.
    /// Различие актуально только для SQL, в XSLT вставка всегда выполняется заменой текста. 
    /// </summary>
    public bool Inset;
    /// <summary>
    /// Текущее значение параметра.
    /// </summary>
    public string CurrentValue;
    /// <summary>
    /// Значение поумолчанию.
    /// </summary>
    public string DefaultValue;
    /// <summary>
    /// SQL запрос или перечисление допустимых значений параметра.
    /// </summary>
    public string SelectValue;

    /// <summary>
    /// Конструктор по умолчанию.
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
    /// Котрструктор преобразования с версии 2.
    /// </summary>
    /// <param name="aPrm">Параметр в формате версии 2.</param>
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

          // не совсем удачная попытка вставить GETDATE()
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
          //case eQueryParamType.IntSelectTree: // ПОКА !!!
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
          //case eQueryParamType.PCSelectTree: // ПОПРОБУЕМ ТАК!!! (было "ПОКА") !!!
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
  /// Запрос.
  /// </summary>
  public class Query
  {
    /// <summary>
    /// Уникальный идентификатор запроса.
    /// </summary>
    public int Code;
    /// <summary>
    /// Название запроса.
    /// </summary>
    public string Name;
    /// <summary>
    /// Дата создания запроса.
    /// </summary>
    public DateTime DateCreate;
    /// <summary>
    /// Дата последнего изменения запроса.
    /// </summary>
    public DateTime DateLastModified;
    /// <summary>
    /// Флаг видимости запроса. 
    /// true - видим в пользовательском режиме
    /// false - невидим в пользовательском режиме
    /// </summary>
    public bool Hidden;
    /// <summary>
    /// Автор запроса.
    /// </summary>
    public string Author;
    /// <summary>
    /// Замечания к запросу.
    /// </summary>
    public string Note;
    /// <summary>
    /// Текст запроса. SQL выражение.
    /// </summary>
    public string Text;
    /// <summary>
    /// Текст XSLT преобразования.
    /// </summary>
    public string XSLT;
    /// <summary>
    /// Имя файла с изображением (*.ico, *.jpg и т.п.) для запроса.
    /// </summary>
    public string ImageName;
    /// <summary>
    /// Коллекция параметров.
    /// </summary>
    [XmlArrayItem("Param", typeof(Param))]
    public ArrayList Params;

    /// <summary>
    /// Конструктор по умолчанию.
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
    /// Котрструктор преобразования с версии 2.
    /// </summary>
    /// <param name="aPrm">Запрос в формате версии 2.</param>
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
    /// Сохранение запроса в файле.
    /// </summary>
    /// <param name="aFileName">Имя файла.</param>
    public void SaveAs(string aFileName)
    {
      TextWriter tr = new StreamWriter(aFileName, false, Encoding.Default);
      XmlSerializer xs = new XmlSerializer(typeof(Query));
      xs.Serialize(tr, this);
      tr.Close();
    }
    /// <summary>
    /// Загрузка запроса из фйла.
    /// </summary>
    /// <param name="aFileName">Имя файла.</param>
    /// <returns>Загруженный запрос.</returns>
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
  /// Сессия - совокупность запросов объединенных, как правило, или одним соединением с БД
  /// или логикой задачи.
  /// </summary>
  public class Session
  {
    /// <summary>
    /// Уникальный идентификатор сессии.
    /// </summary>
    public int Code;
    /// <summary>
    /// Название сессии.
    /// </summary>
    public string Title;
    /// <summary>
    /// Комментарии к сессии.
    /// </summary>
    public string Note;
    /// <summary>
    /// Строка соединения с БД.
    /// </summary>
    public string DBConnection;
    /// <summary>
    /// Начальные символы параметров.
    /// </summary>
    public string ParamBegDelim;
    /// <summary>
    /// Конечные символы параметров.
    /// </summary>
    public string ParamEndDelim;
    /// <summary>
    /// Хеш сумма пароля для входа в режим редактирования/проектирования сессии.
    /// </summary>
    public string Hash;
    /// <summary>
    /// Местонахождение изображение используемых в сессии. 
    /// </summary>
    public string ImagePath;
    /// <summary>
    /// Имя файла с изображением (*.ico, *.jpg и т.п.) для сессии.
    /// </summary>
    public string ImageName;

    /// <summary>
    /// Коллекция общих параметров.
    /// </summary>
    [XmlArrayItem("Param", typeof(Param))]
    public ArrayList Params;

    [XmlArrayItem("Query", typeof(Query))]
    public ArrayList Queries;


    /// <summary>
    /// Название макроподстановки в текст XSLT преобразования
    /// </summary>
    //[XmlIgnoreAttribute]
    //public string				XSLTIncludeName;
    /// <summary>
    /// Содержание макроподстановки в текст XSLT преобразования
    /// </summary>
    //[XmlIgnoreAttribute]
    //public string				XSLTIncludeText;

    /// <summary>
    /// Конструктор по умолчанию.
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
    /// Котрструктор преобразования с версии 2.
    /// </summary>
    /// <param name="aPrm">Сессия в формате версии 2.</param>
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
