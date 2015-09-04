using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using CommandAS.Tools;
using CommandAS.Tools.DataGridColumnStyle;
using CommandAS.QueryLib;

namespace CommandAS.QueryForm
{
  /// <summary>
  /// Summary description for ucParamDef.
  /// </summary>
  public class ucParamDef : System.Windows.Forms.UserControl
  {
    private ArrayList _params;
    private DataTable _dt;
    private AutoResizeDataGridTableStyle _dgs;

    private System.Windows.Forms.DataGrid _dgr;
    private System.Windows.Forms.Splitter _split;
    private System.Windows.Forms.TextBox _txt;
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public ucParamDef(bool aModeDlg)
    {
      _params = null;

      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();


      #region Create data table and filling it:
      _dt = new DataTable();
      _dt.Columns.Add("Number", typeof(int));
      _dt.Columns.Add("Title", typeof(string));
      _dt.Columns.Add("Name", typeof(string));
      _dt.Columns.Add("Type", typeof(int));
      _dt.Columns.Add("Inset", typeof(bool));
      _dt.Columns.Add("DefaultValue", typeof(string));
      _dt.Columns.Add("CurrentValue", typeof(string));
      _dt.Columns.Add("SelectValue", typeof(string));

      _dt.Columns["Inset"].DefaultValue = false;
      _dt.Columns["Type"].DefaultValue = (int)eQueryParamType.String;

      #endregion

      _dt.DefaultView.Sort = "Number ASC";

      _dgr.DataSource = _dt;

      #region Create DateGridStyle
      _dgs = new AutoResizeDataGridTableStyle();
      //DataGridTableStyle _dgs = new DataGridTableStyle();
      _dgs.AllowSorting = false;

      DataGridTextBoxColumn dgTextColumn = new DataGridTextBoxColumn();
      dgTextColumn.MappingName = "Number";
      dgTextColumn.HeaderText = "NN";
      dgTextColumn.NullText = string.Empty;
      _dgs.GridColumnStyles.Add(dgTextColumn);

      dgTextColumn = new DataGridTextBoxColumn();
      dgTextColumn.MappingName = "Title";
      dgTextColumn.HeaderText = "Название";
      dgTextColumn.NullText = string.Empty;
      _dgs.GridColumnStyles.Add(dgTextColumn);

      dgTextColumn = new DataGridTextBoxColumn();
      dgTextColumn.MappingName = "Name";
      dgTextColumn.HeaderText = "Имя";
      dgTextColumn.NullText = string.Empty;
      _dgs.GridColumnStyles.Add(dgTextColumn);

      DataGridComboBoxColumnByCode dgComboColumn = new DataGridComboBoxColumnByCode();
      dgComboColumn.MappingName = "Type";
      dgComboColumn.HeaderText = "Тип";
      if (aModeDlg)
      {
        foreach (eQueryParamType ep in Enum.GetValues(typeof(eQueryParamType)))
          dgComboColumn.pCbo.Items.Add(new _ListBoxItem((int)ep, ep.ToString()));
      }
      else
      {
        dgComboColumn.pCbo.Items.Add(new _ListBoxItem((int)eQueryParamType.Date, eQueryParamType.Date.ToString()));
        dgComboColumn.pCbo.Items.Add(new _ListBoxItem((int)eQueryParamType.String, eQueryParamType.String.ToString()));
        dgComboColumn.pCbo.Items.Add(new _ListBoxItem((int)eQueryParamType.Integer, eQueryParamType.Integer.ToString()));
        //dgComboColumn.pCbo.Items.Add(new _ListBoxItem((int)eQueryParamType.PlaceCode, eQueryParamType.PlaceCode.ToString()));
      }
      _dgs.GridColumnStyles.Add(dgComboColumn);

      DataGridBoolColumn dgsBool = new DataGridBoolColumn();
      dgsBool.AllowNull = false;
      dgsBool.MappingName = "Inset";
      dgsBool.HeaderText = "Текст вст.";
      //dgsBool.NullText = string.Empty;
      _dgs.GridColumnStyles.Add(dgsBool);

      if (aModeDlg)
      {
        dgTextColumn = new DataGridTextBoxColumn();
        dgTextColumn.MappingName = "DefaultValue";
        dgTextColumn.HeaderText = "По умолчанию";
        dgTextColumn.NullText = string.Empty;
        _dgs.GridColumnStyles.Add(dgTextColumn);
      }

      _dgr.TableStyles.Clear();
      _dgr.TableStyles.Add(_dgs);
      #endregion

      _txt.AcceptsReturn = true;
      _txt.AcceptsTab = true;
      _txt.Font = new Font(_txt.Font.Name, _txt.Font.Size, FontStyle.Bold);

      Binding bnd = _txt.DataBindings.Add("Text", _dt, aModeDlg ? "SelectValue" : "CurrentValue");
      bnd.Format += new ConvertEventHandler(bnd_Format);
      bnd.Parse += new ConvertEventHandler(bnd_Parse);
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

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this._dgr = new System.Windows.Forms.DataGrid();
      this._split = new System.Windows.Forms.Splitter();
      this._txt = new System.Windows.Forms.TextBox();
      ((System.ComponentModel.ISupportInitialize)(this._dgr)).BeginInit();
      this.SuspendLayout();
      // 
      // _dgr
      // 
      this._dgr.CaptionVisible = false;
      this._dgr.DataMember = "";
      this._dgr.Dock = System.Windows.Forms.DockStyle.Top;
      this._dgr.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this._dgr.Location = new System.Drawing.Point(0, 0);
      this._dgr.Name = "_dgr";
      this._dgr.Size = new System.Drawing.Size(280, 112);
      this._dgr.TabIndex = 1;
      // 
      // _split
      // 
      this._split.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this._split.Dock = System.Windows.Forms.DockStyle.Top;
      this._split.Location = new System.Drawing.Point(0, 112);
      this._split.Name = "_split";
      this._split.Size = new System.Drawing.Size(280, 7);
      this._split.TabIndex = 4;
      this._split.TabStop = false;
      // 
      // _txt
      // 
      this._txt.Dock = System.Windows.Forms.DockStyle.Fill;
      this._txt.Location = new System.Drawing.Point(0, 119);
      this._txt.Multiline = true;
      this._txt.Name = "_txt";
      this._txt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this._txt.Size = new System.Drawing.Size(280, 105);
      this._txt.TabIndex = 5;
      this._txt.Text = "";
      // 
      // ucParamDef
      // 
      this.Controls.Add(this._txt);
      this.Controls.Add(this._split);
      this.Controls.Add(this._dgr);
      this.Name = "ucParamDef";
      this.Size = new System.Drawing.Size(280, 224);
      ((System.ComponentModel.ISupportInitialize)(this._dgr)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    public void SetData(ArrayList aParams)
    {
      _params = aParams;

      _dt.Rows.Clear();
      _dt.AcceptChanges();
      DataRow dr;
      foreach (Param prm in _params)
      {
        dr = _dt.NewRow();
        dr["Number"] = prm.Number;
        dr["Title"] = prm.Title;
        dr["Name"] = prm.Name;
        dr["Type"] = prm.Type;
        dr["Inset"] = prm.Inset;
        dr["DefaultValue"] = prm.DefaultValue;
        dr["CurrentValue"] = prm.CurrentValue;
        dr["SelectValue"] = prm.SelectValue;
        _dt.Rows.Add(dr);
      }

      _dgs.OnDataGridResize(_dgs, new EventArgs());
    }

    public void SaveToCollection()
    {
      if (_params == null)
        return;

      _dt.AcceptChanges();
      _params.Clear();
      Param prm;

      DataView dv = _dt.DefaultView;
      DataRowView dr;
      for (int ii = 0; ii < dv.Count; ii++)
      {
        dr = dv[ii];
        prm = new Param();
        prm.Number = CASTools.ConvertToInt32Or0(dr["Number"]);
        prm.Title = dr["Title"].ToString();
        prm.Name = dr["Name"].ToString();
        try { prm.Type = (eQueryParamType)dr["Type"]; }
        catch { }
        prm.Inset = Convert.ToBoolean(dr["Inset"]);
        prm.DefaultValue = dr["DefaultValue"].ToString();
        prm.CurrentValue = dr["CurrentValue"].ToString();
        prm.SelectValue = dr["SelectValue"].ToString();
        _params.Add(prm);
      }
    }

    private void bnd_Format(object sender, ConvertEventArgs e)
    {
      e.Value = e.Value.ToString().Replace("\n", Environment.NewLine);
    }

    private void bnd_Parse(object sender, ConvertEventArgs e)
    {
      e.Value = e.Value.ToString().Replace(Environment.NewLine, "\n");
    }

  }
}
