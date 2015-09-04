using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using CommandAS.Tools.Security;

namespace CommandAS.QueryForm
{
	/// <summary>
	/// Summary description for dlgPassword.
	/// </summary>
	public class dlgPasswordSet : System.Windows.Forms.Form
	{
		private Task								_tsk;

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _txtPasswordPrev;
		private System.Windows.Forms.TextBox _txtPasswordNew1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox _txtPasswordNew2;
		private System.Windows.Forms.Button _cmdOk;
		private System.Windows.Forms.Button _cmdCancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public dlgPasswordSet(Task aTsk)
		{
			_tsk = aTsk;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this._txtPasswordPrev = new System.Windows.Forms.TextBox();
			this._txtPasswordNew1 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this._txtPasswordNew2 = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this._cmdOk = new System.Windows.Forms.Button();
			this._cmdCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(208, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "Текущий пароль";
			// 
			// _txtPasswordPrev
			// 
			this._txtPasswordPrev.Location = new System.Drawing.Point(232, 16);
			this._txtPasswordPrev.Name = "_txtPasswordPrev";
			this._txtPasswordPrev.PasswordChar = '*';
			this._txtPasswordPrev.Size = new System.Drawing.Size(288, 22);
			this._txtPasswordPrev.TabIndex = 1;
			this._txtPasswordPrev.Text = "";
			// 
			// _txtPasswordNew1
			// 
			this._txtPasswordNew1.Location = new System.Drawing.Point(232, 56);
			this._txtPasswordNew1.Name = "_txtPasswordNew1";
			this._txtPasswordNew1.PasswordChar = '*';
			this._txtPasswordNew1.Size = new System.Drawing.Size(288, 22);
			this._txtPasswordNew1.TabIndex = 3;
			this._txtPasswordNew1.Text = "";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(208, 23);
			this.label2.TabIndex = 2;
			this.label2.Text = "Новый пароль";
			// 
			// _txtPasswordNew2
			// 
			this._txtPasswordNew2.Location = new System.Drawing.Point(232, 88);
			this._txtPasswordNew2.Name = "_txtPasswordNew2";
			this._txtPasswordNew2.PasswordChar = '*';
			this._txtPasswordNew2.Size = new System.Drawing.Size(288, 22);
			this._txtPasswordNew2.TabIndex = 5;
			this._txtPasswordNew2.Text = "";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 88);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(208, 23);
			this.label3.TabIndex = 4;
			this.label3.Text = "Повтор нового пароля";
			// 
			// _cmdOk
			// 
			this._cmdOk.Location = new System.Drawing.Point(344, 128);
			this._cmdOk.Name = "_cmdOk";
			this._cmdOk.Size = new System.Drawing.Size(72, 32);
			this._cmdOk.TabIndex = 6;
			this._cmdOk.Text = "Ok";
			this._cmdOk.Click += new System.EventHandler(this._cmdOk_Click);
			// 
			// _cmdCancel
			// 
			this._cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cmdCancel.Location = new System.Drawing.Point(432, 128);
			this._cmdCancel.Name = "_cmdCancel";
			this._cmdCancel.Size = new System.Drawing.Size(88, 32);
			this._cmdCancel.TabIndex = 7;
			this._cmdCancel.Text = "Отмена";
			// 
			// dlgPasswordSet
			// 
			this.AcceptButton = this._cmdOk;
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.CancelButton = this._cmdCancel;
			this.ClientSize = new System.Drawing.Size(528, 168);
			this.Controls.Add(this._cmdCancel);
			this.Controls.Add(this._cmdOk);
			this.Controls.Add(this._txtPasswordNew2);
			this.Controls.Add(this._txtPasswordNew1);
			this.Controls.Add(this._txtPasswordPrev);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "dlgPasswordSet";
			this.Text = "Установка/изменение пароля";
			this.ResumeLayout(false);

		}
		#endregion

		private void _cmdOk_Click(object sender, System.EventArgs e)
		{
			if (_tsk.pHash.Length > 0 && !CasHash.HashMD5hex(_txtPasswordPrev.Text).Equals(_tsk.pHash))
			{
				MessageBox.Show("Прежний пароль неверный !!!");
				return;
			}
			//if (_txtPasswordNew1.Text.Length < 5)
			//{
			//	MessageBox.Show("Длина пароля должна быть более 4 символов.");
			//	return;
			//}
			if (!_txtPasswordNew1.Text.Equals(_txtPasswordNew2.Text))
			{
				MessageBox.Show("Новый пароль и повтор не идентичны !!!");
				return;
			}

			if (_txtPasswordNew1.Text.Length > 0)
				_tsk.pHash = CasHash.HashMD5hex(_txtPasswordNew1.Text);
			else
				_tsk.pHash = string.Empty;

			DialogResult = DialogResult.OK;
		}

	}
}
