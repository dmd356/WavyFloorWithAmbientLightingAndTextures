using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using ImGuiNET;
namespace TestEngine
{
    public partial class MyWindow : SubWindowUI
    {
        public MyWindow(int width, int height) : base(width, height)
        {
            
        }

    }


    public partial class SubWindowUI
    {
        public Rectangle ClientRectangle { get; set; }
        public SubWindowUI(int width, int height)
        {
            ClientRectangle = new Rectangle(0, 0, width, height);
        }

        public virtual void DrawGUI(PaintEventArgs e)
        {
            // Create pen.
            Pen blackPen = new Pen(Color.Black, 3);

            // Create rectangle.
            
            // Draw rectangle to screen.
            e.Graphics.DrawRectangle(blackPen, ClientRectangle);
        }
    }
}
