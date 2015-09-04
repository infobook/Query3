using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
//using System.Data;
//using CommandAS.Tools;
//using CommandAS.Tools.DataGridColumnStyle;
using CommandAS.QueryLib;


namespace CommandAS.QueryForm
{
  /// <summary>
  /// Summary description for dlgParam.
  /// </summary>
  public class dlgParam : System.Windows.Forms.Form
  {
    private Query _query;
    private ucParamDef _uc;

    private System.Windows.Forms.Button _cmdCancel;
    private System.Windows.Forms.Button _cmdOk;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public dlgParam(Query aQuery)
    {
      _query = aQuery;

      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      _uc = new ucParamDef(true);
      _uc.SetData(_query.Params);
      _uc.Location = new Point(5, 5);
      _uc.Size = new Size(this.ClientSize.Width - 10, _cmdOk.Top - 10);
      _uc.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
      this.Controls.Add(_uc);

      this.Text = "Параметр(ы) запроса - " + _query.Name;

    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this._cmdCancel = new System.Windows.Forms.Button();
      this._cmdOk = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // _cmdCancel
      // 
      this._cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this._cmdCancel.Location = new System.Drawing.Point(568, 456);
      this._cmdCancel.Name = "_cmdCancel";
      this._cmdCancel.Size = new System.Drawing.Size(88, 32);
      this._cmdCancel.TabIndex = 8;
      this._cmdCancel.Text = "Отменить";
      // 
      // _cmdOk
      // 
      this._cmdOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this._cmdOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this._cmdOk.Location = new System.Drawing.Point(464, 456);
      this._cmdOk.Name = "_cmdOk";
      this._cmdOk.Size = new System.Drawing.Size(88, 32);
      this._cmdOk.TabIndex = 7;
      this._cmdOk.Text = "Ok";
      this._cmdOk.Click += new System.EventHandler(this._cmdOk_Click);
      // 
      // dlgParam
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
      this.ClientSize = new System.Drawing.Size(664, 496);
      this.Controls.Add(this._cmdCancel);
      this.Controls.Add(this._cmdOk);
      this.Name = "dlgParam";
      this.ResumeLayout(false);

    }
    #endregion

    private void _cmdOk_Click(object sender, System.EventArgs e)
    {
      _uc.SaveToCollection();
    }

  }
}
