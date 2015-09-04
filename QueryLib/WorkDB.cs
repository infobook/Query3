using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using CommandAS.Tools;

namespace CommandAS.QueryLib
{
	/// <summary>
	/// Текущая ("рабочая") база данных (БД).
	/// </summary>
	public class WorkDB
	{
		/// <summary>
		/// Соединение с БД
		/// </summary>
		protected OleDbConnection							mCn;
		/// <summary>
		/// Транзакция
		/// </summary>
		protected OleDbTransaction						mTxn;
		/// <summary>
		/// Ошибка
		/// </summary>
		protected Error												mErr;

		/// <summary>
		/// Соединение с БД
		/// </summary>
		public  OleDbConnection								pDBConnection
		{
			get { return mCn;  }
			set { mCn = value; }
		}

		/// <summary>
		/// Строка соединения с БД.
		/// </summary>
		public string													pConnectionString
		{
			get { return mCn.ConnectionString;  }
			set 
			{
				if (!mCn.ConnectionString.Equals(value))
				{
					ConnectionClose ();
					mCn.ConnectionString = value;
				}
			}
		}
		/// <summary>
		/// Ошибка выполнения
		/// </summary>
		public Error													pError
		{
			get { return mErr; }
		}


		/// <summary>
		/// Указатель на исполнитель запросов.
		/// </summary>
		//public Performer											pQP;

    public OleDbTransaction pTransaction
    {
      get { return mTxn; }
      set
      {
        mTxn = value;
        mIsInternalTransaction = false;
      }
    }

    protected bool mIsInternalTransaction;

		/// <summary>
		/// Конструктор по умолчанию
		/// </summary>
		public WorkDB() : this (new OleDbConnection()) {}
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="aCn">соединение</param>
		public WorkDB(OleDbConnection aCn)
		{
			mCn = aCn;
			mErr = new Error();
			//pQP = null;
      mTxn = null;
      mIsInternalTransaction = false;
    }
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="aConnectionString">строка соединения</param>
		public WorkDB(string aConnectionString)
		{
			mCn = new OleDbConnection(aConnectionString);
			mErr = new Error();
      mTxn = null;
      mIsInternalTransaction = false;
    }

		/// <summary>
		/// Начать транзакцию.
		/// </summary>
		public virtual void TransactionBegin()
		{
      if (mCn != null && mCn.State == ConnectionState.Open)
      {
        if (mTxn == null)
        {
          mTxn = mCn.BeginTransaction();
          mIsInternalTransaction = true;
        }
      }
      else
        mTxn = null;
		}

		/// <summary>
		/// Закончить транзакцию.
		/// </summary>
		public virtual void TransactionCommit()
		{
      if (mIsInternalTransaction)
      {
        if (mTxn != null)
          mTxn.Commit();

        pTransaction = null;
      }
    }
		/// <summary>
		/// "Откатить" транзакцию.
		/// </summary>
		public virtual void TransactionRollback()
		{
      if (mIsInternalTransaction)
      {
        if (mTxn != null)
          mTxn.Rollback();

        pTransaction = null;
      }
    }
		
		/// <summary>
		/// Новая SQL команда.
		/// </summary>
		/// <returns></returns>
		public OleDbCommand NewOleDbCommand()
		{
			return NewOleDbCommand(string.Empty);
		}
		/// <summary>
		/// Новая SQL команда.
		/// </summary>
		/// <returns></returns>
		public virtual OleDbCommand NewOleDbCommand(string aSQLTxt)
		{
			OleDbCommand cmd = null;
			if (aSQLTxt.Length > 0)
			{
				cmd = new OleDbCommand(aSQLTxt, mCn, mTxn);
			}
			else
			{
				cmd = new OleDbCommand();
				cmd.Connection = mCn;
				cmd.Transaction = mTxn;
			}
			return cmd;
		}

		/// <summary>
		/// Выполнить SQL команду, не возвращающую DataSet/DataTable.
		/// </summary>
		/// <returns></returns>
		public virtual int ExecuteNonQuery(OleDbCommand aCmd)
		{
			return aCmd.ExecuteNonQuery();
		}

		/// <summary>
		/// Открыть соединение с БД.
		/// </summary>
		/// <param name="aConnectionString">строка соединения</param>
		/// <returns>true - успешно; false - соединение не установлено</returns>
		public virtual bool ConnectionOpen(string aConnectionString)
		{
			pConnectionString = aConnectionString;
			return ConnectionOpen();
		}
		/// <summary>
		/// Открыть соединение с БД.
		/// </summary>
		/// <returns>true - успешно; false - соединение не установлено</returns>
		public virtual bool ConnectionOpen()
		{
			mErr.Clear();

			if (mCn.ConnectionString.Length == 0)
				mErr.text = "Не определена строка соединения с БД";

			if (mErr.IsOk)
			{
				try	{ mCn.Open(); }
				catch (Exception ex)
				{
					mErr.ex = ex;
				}
			}

			//return (mErr.IsOk && mCn.State == ConnectionState.Open);
			return (mCn.State == ConnectionState.Open);
		}

		/// <summary>
		/// Закрыть соединение.
		/// </summary>
		public virtual void ConnectionClose ()
		{
			if (mCn.State == ConnectionState.Open)
				mCn.Close();
		}

    public virtual void LoadRights(int aSessionCode)
    {
    }

    public virtual bool IsProhibition(int aQueryCode)
    {
      return false;
    }


  }
}
