using PdfSharp.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            GlobalFontSettings.FontResolver = FontResolver.Instance;
            
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}

