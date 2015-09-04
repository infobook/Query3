using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using CommandAS.Tools;
using CommandAS.QueryLib;

namespace CommandAS.QueryForm
{
	/// <summary>
	/// 
	/// </summary>
	public class dlgQProperty : System.Windows.Forms.Form
	{
		private Task													_tsk;
		private Query													_q;

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _txtName;
		private System.Windows.Forms.CheckBox _chkHidden;
		private System.Windows.Forms.Button cmdOk;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label _lblDate;
		private System.Windows.Forms.TextBox _txtAuthor;
		private System.Windows.Forms.TextBox _txtNote;
		private System.Windows.Forms.TextBox _txtName0;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox _txtCode;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox _txtImage;
		private System.Windows.Forms.Button _cmdNextCode;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Query																						pQuery
		{
			set 
			{ 
				_q = value;
				//if (value.Code > 0)
					_txtCode.Text = value.Code.ToString();
				//else
				//	_txtCode.Text = _tsk.GetNextQueryCode().ToString();
				int ld = value.Name.LastIndexOf(@"\");
				if (ld > 0)
				{
					_txtName0.Text = value.Name.Substring(0,ld);
					_txtName.Text = value.Name.Substring(ld+1);
				}
				else
				{
					_txtName0.Text = string.Empty;
					_txtName.Text = value.Name;
				}
				_txtAuthor.Text = value.Author;
				_txtImage.Text = value.ImageName;
				_txtNote.Text = value.Note;
				_lblDate.Text = "Дата создания: "+ value.DateCreate.ToShortDateString() +"\r\n"
						 +"Дата последнего изменения: "+ value.DateLastModified.ToShortDateString();
				_chkHidden.Checked = value.Hidden;
			}
		}

		public dlgQProperty(Task aTsk)
		{
			_tsk = aTsk;
			_q = null;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			_txtCode.Leave += new EventHandler(_txtCode_Leave);

		}

		public void SetValueTo(Query aQ)
		{
			aQ.Code = CASTools.ConvertToInt32Or0(_txtCode.Text);

			if (_txtName0.Text.Length == 0)
				aQ.Name = _txtName.Text;
			else
				aQ.Name = _txtName0.Text + @"\" + _txtName.Text;
			aQ.Author = _txtAuthor.Text;
			aQ.ImageName = _txtImage.Text;
			aQ.Note = _txtNote.Text;
			aQ.Hidden = _chkHidden.Checked;
		}

    private void cmdOk_Click(object sender, EventArgs e)
    {
      string name = _txtName0.Text.Length == 0 ? _txtName.Text : _txtName0.Text + @"\" + _txtName.Text;
      foreach (Query qq in _tsk.pQueries)
      {
        if (qq.Name.Equals(name) && !qq.Equals(_q))
        {
          MessageBox.Show("Полное имя запроса должно быть уникальным!","Ошибка",MessageBoxButtons.OK,MessageBoxIcon.Error);
          return;
        }
      }

      DialogResult = DialogResult.OK;
    }

		private void _cmdNextCode_Click(object sender, System.EventArgs e)
		{
			_txtCode.Text = _tsk.GetNextQueryCode().ToString();
		}

		private void _txtCode_Leave(object sender, EventArgs e)
		{
			int ii = CASTools.ConvertToInt32Or0(_txtCode.Text);
			_txtCode.Text = ii.ToString();
			if (!_tsk.IsCorrectQueryCode(ii, _q))
				MessageBox.Show("Не уникальный идентификатор запроса!","Предупреждение",MessageBoxButtons.OK,MessageBoxIcon.Warning);
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
      this._txtName = new System.Windows.Forms.TextBox();
      this._lblDate = new System.Windows.Forms.Label();
      this._chkHidden = new System.Windows.Forms.CheckBox();
      this.cmdOk = new System.Windows.Forms.Button();
      this.cmdCancel = new System.Windows.Forms.Button();
      this._txtAuthor = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this._txtNote = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this._txtName0 = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this._txtCode = new System.Windows.Forms.TextBox();
      this.label5 = new System.Windows.Forms.Label();
      this._txtImage = new System.Windows.Forms.TextBox();
      this._cmdNextCode = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(120, 27);
      this.label1.TabIndex = 0;
      this.label1.Text = "Название";
      // 
      // _txtName
      // 
      this._txtName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._txtName.Location = new System.Drawing.Point(344, 8);
      this._txtName.Name = "_txtName";
      this._txtName.Size = new System.Drawing.Size(152, 22);
      this._txtName.TabIndex = 0;
      // 
      // _lblDate
      // 
      this._lblDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._lblDate.Location = new System.Drawing.Point(16, 216);
      this._lblDate.Name = "_lblDate";
      this._lblDate.Size = new System.Drawing.Size(384, 56);
      this._lblDate.TabIndex = 2;
      // 
      // _chkHidden
      // 
      this._chkHidden.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this._chkHidden.Location = new System.Drawing.Point(16, 176);
      this._chkHidden.Name = "_chkHidden";
      this._chkHidden.Size = new System.Drawing.Size(112, 27);
      this._chkHidden.TabIndex = 3;
      this._chkHidden.Text = "скрыть";
      // 
      // cmdOk
      // 
      this.cmdOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdOk.Location = new System.Drawing.Point(408, 208);
      this.cmdOk.Name = "cmdOk";
      this.cmdOk.Size = new System.Drawing.Size(90, 26);
      this.cmdOk.TabIndex = 4;
      this.cmdOk.Text = "Ok";
      this.cmdOk.Click += new System.EventHandler(this.cmdOk_Click);
      // 
      // cmdCancel
      // 
      this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cmdCancel.Location = new System.Drawing.Point(408, 248);
      this.cmdCancel.Name = "cmdCancel";
      this.cmdCancel.Size = new System.Drawing.Size(90, 26);
      this.cmdCancel.TabIndex = 5;
      this.cmdCancel.Text = "Отмена";
      // 
      // _txtAuthor
      // 
      this._txtAuthor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._txtAuthor.Location = new System.Drawing.Point(344, 40);
      this._txtAuthor.Name = "_txtAuthor";
      this._txtAuthor.Size = new System.Drawing.Size(152, 22);
      this._txtAuthor.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(272, 40);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(72, 27);
      this.label2.TabIndex = 7;
      this.label2.Text = "Автор";
      // 
      // _txtNote
      // 
      this._txtNote.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._txtNote.Location = new System.Drawing.Point(136, 104);
      this._txtNote.Multiline = true;
      this._txtNote.Name = "_txtNote";
      this._txtNote.Size = new System.Drawing.Size(360, 96);
      this._txtNote.TabIndex = 2;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(8, 104);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(120, 27);
      this.label3.TabIndex = 9;
      this.label3.Text = "Замечания";
      // 
      // _txtName0
      // 
      this._txtName0.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this._txtName0.Location = new System.Drawing.Point(136, 8);
      this._txtName0.Name = "_txtName0";
      this._txtName0.Size = new System.Drawing.Size(208, 22);
      this._txtName0.TabIndex = 6;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(8, 40);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(120, 23);
      this.label4.TabIndex = 10;
      this.label4.Text = "Идентификатор";
      // 
      // _txtCode
      // 
      this._txtCode.Location = new System.Drawing.Point(136, 40);
      this._txtCode.MaxLength = 16;
      this._txtCode.Name = "_txtCode";
      this._txtCode.Size = new System.Drawing.Size(100, 22);
      this._txtCode.TabIndex = 11;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(8, 72);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(120, 23);
      this.label5.TabIndex = 12;
      this.label5.Text = "Изображение";
      // 
      // _txtImage
      // 
      this._txtImage.Location = new System.Drawing.Point(136, 72);
      this._txtImage.Name = "_txtImage";
      this._txtImage.Size = new System.Drawing.Size(360, 22);
      this._txtImage.TabIndex = 13;
      // 
      // _cmdNextCode
      // 
      this._cmdNextCode.Location = new System.Drawing.Point(240, 40);
      this._cmdNextCode.Name = "_cmdNextCode";
      this._cmdNextCode.Size = new System.Drawing.Size(24, 24);
      this._cmdNextCode.TabIndex = 14;
      this._cmdNextCode.Text = "...";
      this._cmdNextCode.Click += new System.EventHandler(this._cmdNextCode_Click);
      // 
      // dlgQProperty
      // 
      this.AcceptButton = this.cmdOk;
      this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
      this.CancelButton = this.cmdCancel;
      this.ClientSize = new System.Drawing.Size(506, 284);
      this.Controls.Add(this._cmdNextCode);
      this.Controls.Add(this._txtImage);
      this.Controls.Add(this._txtCode);
      this.Controls.Add(this._txtName0);
      this.Controls.Add(this._txtNote);
      this.Controls.Add(this._txtAuthor);
      this.Controls.Add(this._txtName);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.cmdCancel);
      this.Controls.Add(this.cmdOk);
      this.Controls.Add(this._chkHidden);
      this.Controls.Add(this._lblDate);
      this.Controls.Add(this.label1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "dlgQProperty";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Свойства запроса";
      this.ResumeLayout(false);
      this.PerformLayout();

		}
		#endregion
	}
}
