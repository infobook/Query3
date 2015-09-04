using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using CommandAS.Tools;

namespace CommandAS.QueryLib
{
	/// <summary>
	/// ������� ("�������") ���� ������ (��).
	/// </summary>
	public class WorkDB
	{
		/// <summary>
		/// ���������� � ��
		/// </summary>
		protected OleDbConnection							mCn;
		/// <summary>
		/// ����������
		/// </summary>
		protected OleDbTransaction						mTxn;
		/// <summary>
		/// ������
		/// </summary>
		protected Error												mErr;

		/// <summary>
		/// ���������� � ��
		/// </summary>
		public  OleDbConnection								pDBConnection
		{
			get { return mCn;  }
			set { mCn = value; }
		}

		/// <summary>
		/// ������ ���������� � ��.
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
		/// ������ ����������
		/// </summary>
		public Error													pError
		{
			get { return mErr; }
		}


		/// <summary>
		/// ��������� �� ����������� ��������.
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
		/// ����������� �� ���������
		/// </summary>
		public WorkDB() : this (new OleDbConnection()) {}
		/// <summary>
		/// �����������
		/// </summary>
		/// <param name="aCn">����������</param>
		public WorkDB(OleDbConnection aCn)
		{
			mCn = aCn;
			mErr = new Error();
			//pQP = null;
      mTxn = null;
      mIsInternalTransaction = false;
    }
		/// <summary>
		/// �����������
		/// </summary>
		/// <param name="aConnectionString">������ ����������</param>
		public WorkDB(string aConnectionString)
		{
			mCn = new OleDbConnection(aConnectionString);
			mErr = new Error();
      mTxn = null;
      mIsInternalTransaction = false;
    }

		/// <summary>
		/// ������ ����������.
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
		/// ��������� ����������.
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
		/// "��������" ����������.
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
		/// ����� SQL �������.
		/// </summary>
		/// <returns></returns>
		public OleDbCommand NewOleDbCommand()
		{
			return NewOleDbCommand(string.Empty);
		}
		/// <summary>
		/// ����� SQL �������.
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
		/// ��������� SQL �������, �� ������������ DataSet/DataTable.
		/// </summary>
		/// <returns></returns>
		public virtual int ExecuteNonQuery(OleDbCommand aCmd)
		{
			return aCmd.ExecuteNonQuery();
		}

		/// <summary>
		/// ������� ���������� � ��.
		/// </summary>
		/// <param name="aConnectionString">������ ����������</param>
		/// <returns>true - �������; false - ���������� �� �����������</returns>
		public virtual bool ConnectionOpen(string aConnectionString)
		{
			pConnectionString = aConnectionString;
			return ConnectionOpen();
		}
		/// <summary>
		/// ������� ���������� � ��.
		/// </summary>
		/// <returns>true - �������; false - ���������� �� �����������</returns>
		public virtual bool ConnectionOpen()
		{
			mErr.Clear();

			if (mCn.ConnectionString.Length == 0)
				mErr.text = "�� ���������� ������ ���������� � ��";

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
		/// ������� ����������.
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
