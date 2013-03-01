using System;
using System.Windows.Forms;

namespace PPF2DataEditor
{
    public class PPF2DataEditor
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}