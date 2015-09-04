using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace CommandAS.Query
{
	/// <summary>
	/// dlgAbout.
	/// </summary>
	public class dlgAbout : System.Windows.Forms.Form
	{
		private Image							_img;

		public string							pVersion;
		
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public dlgAbout()
		{
			string [] ss = Application.ProductVersion.Split(".".ToCharArray());
			pVersion = ss[0]+"."+ss[1];
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			_img = Image.FromHbitmap(new Bitmap(GetType(),"Images.drop.jpg").GetHbitmap());
			//_lblTitle.BringToFront();

		}

		private void DoPaint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			Graphics grfx = e.Graphics;
			//354; 289
			grfx.DrawImage(_img, 0, 0, ClientSize.Width, ClientSize.Height);

			StringFormat strfmt = new StringFormat(); 
			strfmt.Alignment = StringAlignment.Center;
			strfmt.LineAlignment = StringAlignment.Near;
			Font font = new Font("Times New Roman", 16, FontStyle.Bold);
			grfx.DrawString("Query", font, Brushes.DeepSkyBlue, new Rectangle(10,20,100,30), strfmt);
			font = new Font("Times New Roman", 10, FontStyle.Bold);

			grfx.DrawString("Version "+pVersion, font, Brushes.DeepSkyBlue, 
				new Rectangle(ClientSize.Width - 150,20,130,20), strfmt);
			grfx.DrawString("Copyright© 2004-08", font, Brushes.DeepSkyBlue,
				new Rectangle(ClientSize.Width/4,ClientSize.Height-30,ClientSize.Width/2,20), strfmt);
		}


		private void DoClick(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void DoKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			this.Close();
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
			// 
			// dlgAbout
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(354, 289);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.KeyPreview = true;
			this.Name = "dlgAbout";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DoKeyDown);
			this.Click += new System.EventHandler(this.DoClick);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.DoPaint);

		}
		#endregion

	}
}
