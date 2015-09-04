using System;
using System.Windows.Forms;
using CommandAS.QueryForm;

namespace CommandAS.Query
{
  /// <summary>
  /// Summary description for Start.
  /// </summary>
  public class Start
  {
    public Start()
    {
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
      frmQuery frm = new frmQuery();

      bool dm = false;
      try
      {
        foreach (string prm in args)
        {
          if (prm.ToLower().IndexOf("mode=") == 0)
            dm = prm.ToLower().Substring(5).Equals("designer");
          else if (prm.ToLower().IndexOf("load=") == 0)
            frm.pLoadDefaultSession = prm.ToLower().Substring(5);
          else
            frm.pLoadDefaultSession = prm.ToLower();
        }
      }
      catch { }

      frm.pDesignerMode = dm;
      frm.About += new EventHandler(frm_About);

      Application.Run(frm);
    }

    private static void frm_About(object sender, EventArgs e)
    {
      dlgAbout dlg = new dlgAbout();
      dlg.pVersion = QueryLib.Performer.VERSION;

      dlg.ShowDialog();
    }
  }
}
