using System;
using System.Data;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using CommandAS.QueryLib;

namespace CommandAS.QueryLib2
{
	/// <summary>
	/// Параметр запроса.
	/// </summary>
	public class Param
	{
		public string						Title;
		public string						Name;
		public eQueryParamType	Type;
		public string						DefaultValue;
		public string						SelectValue;

		public Param()
		{
			Title		= string.Empty;
			Name		= string.Empty;
			Type		= eQueryParamType.Undefined;
			DefaultValue	= string.Empty;
			SelectValue		= string.Empty;
		}
	}

	/// <summary>
	/// Запрос.
	/// </summary>
	public class Query
	{
		public string			Name;
		public DateTime		DateCreate;
		public DateTime		DateLastModified;
		public bool				Hidden;
		public string			Author;
		public string			Note;
		public string			Text;
		public string			XSLT;
		public string			HTML;
		/// <summary>
		/// Коллекция параметров.
		/// </summary>
		[XmlArrayItem("Param", typeof(Param))]
		public ArrayList	Params;

		public Query()
		{
			Name							= "NewQuery";
			DateCreate				= DateTime.Today;
			DateLastModified	= DateTime.Today;
			Hidden						= false;
			Text							= string.Empty;
			XSLT							= string.Empty;
			HTML							= string.Empty;

			Params						= new ArrayList(4);
		}

		public void SaveAs(string aFileName)
		{
			TextWriter tr = new StreamWriter(aFileName, false, Encoding.Default);
			XmlSerializer xs = new XmlSerializer(typeof(Query));
			xs.Serialize(tr, this);
			tr.Close();
		}

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
		public const string PARAM_BEG_DELIM_DEFAULT = "#?";
		public const string PARAM_END_DELIM_DEFAULT = "?#";

		public string				Title;
		public string				DBConnection;
		public string				ParamBegDelim;
		public string				ParamEndDelim;
		[XmlArrayItem("Query", typeof(Query))]
		public ArrayList		Queries;

		public Session()
		{
			Title	= string.Empty;
			DBConnection = string.Empty;
			ParamBegDelim = PARAM_BEG_DELIM_DEFAULT;
			ParamEndDelim = PARAM_END_DELIM_DEFAULT;
			Queries = new ArrayList(8);
		}

	}
}
