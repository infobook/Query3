using System;
using System.Drawing;
using System.IO;
//using System.Data;
using CommandAS.Tools;
using CommandAS.QueryLib;


namespace CommandAS.QueryForm
{
	public enum eOpeningFileContent
	{
		Undefined	= 0,
		SQL				= 1,
		XSLT			= 2
	}
	
	public class OpeningFile
	{
		public string										pFileName;
		public eOpeningFileContent			pOFContent;
		public Query										pQuery;

		public OpeningFile(string aFileName, eOpeningFileContent aOFContent, Query aQuery)
		{
			pFileName		= aFileName;
			pOFContent	= aOFContent;
			pQuery			= aQuery;
		}
	}

	//public class Tools
	//{
	//	private Tools()
	//	{
	//	}
	//}

	public class QueryIconCollection : IconCollection
	{
		public const string QueryListItem				=	"QueryListItem";
		public const string Excel								= "Excel";
		public const string HideStatus					= "HideStatus";
		public const string ViewStatus					= "ViewStatus";
		public const string DocNew							= "DocNew";
		public const string DocOpen							= "DocOpen";
		public const string Run									= "Run";
		public const string TextSQL							= "TextSQL";
		public const string TextParam						= "TextParam";
		public const string TextHTML						= "TextHTML";
		public const string TextXSLT						= "TextXSLT";
		public const string XSLTView						= "XSLTView";
		public const string ExecRTab						= "ExecRTab";
		public const string ExecRExcel					= "ExecRExcel";
		public const string ExecRHTML						= "ExecRHTML";
		public const string ExecQuery						= "ExecQuery";

		public QueryIconCollection() : this (48) {}
		public QueryIconCollection(int aIconSize) : base (aIconSize)
		{
			const string pref="Images.Icons.";
			Add(load_Image(pref+"QueryListItem.ico"), QueryListItem);
			Add(load_Image(pref+"Excel.ico"), Excel);
			Add(load_Image(pref+"HideStatus.ico"), HideStatus);
			Add(load_Image(pref+"ViewStatus.ico"), ViewStatus);
			Add(load_Image(pref+"DocNew.ico"), DocNew);
			Add(load_Image(pref+"DocOpen.ico"), DocOpen);
			Add(load_Image(pref+"Run.ico"), Run);
			Add(load_Image(pref+"TextSQL.ico"), TextSQL);
			Add(load_Image(pref+"TextParam.ico"), TextParam);
			Add(load_Image(pref+"TextHTML.ico"), TextHTML);
			Add(load_Image(pref+"TextXSLT.ico"), TextXSLT);
			Add(load_Image(pref+"XSLTView.ico"), XSLTView);
			Add(load_Image(pref+"ExecQuery.ico"), ExecQuery);

			Image imgTmp = ImageTools.Summary2Image(Image(QueryIconCollection.HideStatus), Image(QueryIconCollection.ExecQuery), iconAlign.leftButtom); //, 3/4, 3/4);
			Add(imgTmp, ExecRTab);
			imgTmp = ImageTools.Summary2Image(Image(QueryIconCollection.Excel), Image(QueryIconCollection.ExecQuery), iconAlign.leftButtom); //, 3/4, 3/4);
			Add(imgTmp, ExecRExcel);
			imgTmp = ImageTools.Summary2Image(Image(QueryIconCollection.XSLTView), Image(QueryIconCollection.ExecQuery), iconAlign.leftButtom); //, 3/4, 3/4);
			Add(imgTmp, ExecRHTML);
		}

		protected Image load_Image(string resource)
		{
			return loadImage(typeof(QueryIconCollection),resource);
		}
	}
}
