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
	public class dlgPassword : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button _cmdOk;
		private System.Windows.Forms.Button _cmdCancel;
		public System.Windows.Forms.TextBox pTxtPassword;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public dlgPassword()
		{
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
			this.pTxtPassword = new System.Windows.Forms.TextBox();
			this._cmdOk = new System.Windows.Forms.Button();
			this._cmdCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "Пароль";
			// 
			// pTxtPassword
			// 
			this.pTxtPassword.Location = new System.Drawing.Point(128, 16);
			this.pTxtPassword.Name = "pTxtPassword";
			this.pTxtPassword.PasswordChar = '*';
			this.pTxtPassword.Size = new System.Drawing.Size(240, 22);
			this.pTxtPassword.TabIndex = 1;
			this.pTxtPassword.Text = "";
			// 
			// _cmdOk
			// 
			this._cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this._cmdOk.Location = new System.Drawing.Point(192, 56);
			this._cmdOk.Name = "_cmdOk";
			this._cmdOk.Size = new System.Drawing.Size(72, 32);
			this._cmdOk.TabIndex = 6;
			this._cmdOk.Text = "Ok";
			// 
			// _cmdCancel
			// 
			this._cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cmdCancel.Location = new System.Drawing.Point(280, 56);
			this._cmdCancel.Name = "_cmdCancel";
			this._cmdCancel.Size = new System.Drawing.Size(88, 32);
			this._cmdCancel.TabIndex = 7;
			this._cmdCancel.Text = "Отмена";
			// 
			// dlgPassword
			// 
			this.AcceptButton = this._cmdOk;
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.CancelButton = this._cmdCancel;
			this.ClientSize = new System.Drawing.Size(378, 100);
			this.Controls.Add(this._cmdCancel);
			this.Controls.Add(this._cmdOk);
			this.Controls.Add(this.pTxtPassword);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "dlgPassword";
			this.Text = "Идентификация";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
