using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Reflection;

namespace RecognitionService
{
    public class MenuViewController
    {
        public MenuViewController(IDeviceController deviceController)
        {
            System.Diagnostics.Debug.Assert(deviceController != null);

            _deviceController = deviceController;

            _components = new System.ComponentModel.Container();
            _notifyIcon = new System.Windows.Forms.NotifyIcon(_components)
            {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = RecognitionService.Properties.Resources.NotReadyIcon,
                Text = "Overlay is not found",
                Visible = true,
            };

            // _notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
            // _notifyIcon.DoubleClick += notifyIcon_DoubleClick;
            // _notifyIcon.MouseUp += notifyIcon_MouseUp;

            //_aboutViewModel = new WpfFormLibrary.ViewModel.AboutViewModel();
            //_statusViewModel = new WpfFormLibrary.ViewModel.StatusViewModel();

            // _statusViewModel.Icon = AppIcon;
            // _aboutViewModel.Icon = _statusViewModel.Icon;

            // _hiddenWindow = new System.Windows.Window();
            // _hiddenWindow.Hide();
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

        // This allows code to be run on a GUI thread
        // private System.Windows.Window _hiddenWindow;

        private System.ComponentModel.IContainer _components;
        // The Windows system tray class
        private NotifyIcon _notifyIcon;
        IDeviceController _deviceController;
    }
}
