using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataCloneTool
{
    public partial class TransparentPanel : UserControl
    {
        public TransparentPanel()
        {
            InitializeComponent();
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle bounds = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            SolidBrush brush = new SolidBrush(Color.FromArgb(100, Color.White));
            g.FillRectangle(brush, bounds);

            brush.Dispose();

            base.OnPaint(e);
        }
    }
}
