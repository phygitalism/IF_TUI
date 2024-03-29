using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

using RecognitionService.Input.Touch;

namespace RecognitionService
{
	public class MenuViewController : IDisposable
	{
		private System.ComponentModel.IContainer _components;
		private NotifyIcon _notifyIcon;
		private ToolStripMenuItem _exitMenuItem;
		private IDeviceController _deviceController;

		// This allows code to be run on a GUI thread
		private System.Windows.Window _hiddenWindow;

		public MenuViewController(IDeviceController deviceController)
		{
			System.Diagnostics.Debug.Assert(deviceController != null);

			_deviceController = deviceController;
			_deviceController.OnStateChanged += OnStateChanged;

			_components = new System.ComponentModel.Container();
			_notifyIcon = new NotifyIcon(_components)
			{
				ContextMenuStrip = new ContextMenuStrip(),
				Icon = Properties.Resources.NotReadyIcon,
				Text = "Overlay is not found",
				Visible = true,
			};

			_notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;

			_hiddenWindow = new System.Windows.Window();
			_hiddenWindow.Hide();
		}

		public void Dispose()
		{
			_deviceController.OnStateChanged -= OnStateChanged;
		}

		private void OnStateChanged(DeviceState state)
		{
			switch (state)
			{
				case DeviceState.Initialized:
					_notifyIcon.Text = _deviceController.DeviceName + ": Initialized";
					_notifyIcon.Icon = Properties.Resources.NotReadyIcon;
					PushNotification("Initialized");
					break;
				case DeviceState.Starting:
					_notifyIcon.Text = _deviceController.DeviceName + ": Starting";
					_notifyIcon.Icon = Properties.Resources.NotReadyIcon;
					PushNotification("Starting");
					break;
				case DeviceState.Running:
					_notifyIcon.Text = _deviceController.DeviceName + ": Running";
					_notifyIcon.Icon = Properties.Resources.ReadyIcon;
					PushNotification("Running");
					break;
				case DeviceState.Uninitialized:
					_notifyIcon.Text = _deviceController.DeviceName + ": Not Ready";
					_notifyIcon.Icon = Properties.Resources.NotReadyIcon;
					PushNotification("Not initialized");
					break;
				case DeviceState.Error:
					_notifyIcon.Text = _deviceController.DeviceName + ": Error Detected";
					_notifyIcon.Icon = Properties.Resources.NotReadyIcon;
					break;
				default:
					_notifyIcon.Text = _deviceController.DeviceName + ": -";
					_notifyIcon.Icon = Properties.Resources.NotReadyIcon;
					break;
			}
		}

		private void PushNotification(string text)
		{
			_hiddenWindow.Dispatcher.Invoke(delegate
			{
				_notifyIcon.BalloonTipText = text;
				_notifyIcon.ShowBalloonTip(3000);
			});
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
