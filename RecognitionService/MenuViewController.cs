using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace RecognitionService
{
    public class MenuViewController
    {
        private System.ComponentModel.IContainer _components;
        private NotifyIcon _notifyIcon;
        private ToolStripMenuItem _exitMenuItem;
        IDeviceController _deviceController;

        public MenuViewController(IDeviceController deviceController)
        {
            System.Diagnostics.Debug.Assert(deviceController != null);

            _deviceController = deviceController;

            _components = new System.ComponentModel.Container();
            _notifyIcon = new NotifyIcon(_components)
            {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = Properties.Resources.NotReadyIcon,
                Text = "Overlay is not found",
                Visible = true,
            };

            _notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
        }

        System.Windows.Media.ImageSource AppIcon
        {
            get
            {
                // System.Drawing.Icon icon = (_deviceController.Status == DeviceStatus.Running) ? Properties.Resources.ReadyIcon : Properties.Resources.NotReadyIcon;
                System.Drawing.Icon icon = Properties.Resources.NotReadyIcon;
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    System.Windows.Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
        }

        private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;

            if (_notifyIcon.ContextMenuStrip.Items.Count == 0)
            {
                _exitMenuItem = ToolStripMenuItemWithHandler("&Exit", "Exits Recognition Service", exitItem_Click);
                _notifyIcon.ContextMenuStrip.Items.Add(_exitMenuItem);
            }
        }

        private ToolStripMenuItem ToolStripMenuItemWithHandler(string displayText, string tooltipText, EventHandler eventHandler)
        {
            var item = new ToolStripMenuItem(displayText);
            if (eventHandler != null)
            {
                item.Click += eventHandler;
            }

            item.ToolTipText = tooltipText;
            return item;
        }

        private void exitItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
