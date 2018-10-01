using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RecognitionService
{
    public class STAApplicationContext : ApplicationContext
    {
        private TouchOverlay _touchOverlay;
        private MenuViewController _menuViewController;

        public STAApplicationContext()
        {
            _touchOverlay = new TouchOverlay();
            _menuViewController = new MenuViewController(_touchOverlay);
        }

        // Called from the Dispose method of the base class
        protected override void Dispose(bool disposing)
        {
            _touchOverlay = null;
            _menuViewController = null;
        }
    }
}
