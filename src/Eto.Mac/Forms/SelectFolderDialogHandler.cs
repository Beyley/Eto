using System;
using Eto.Forms;

namespace Eto.Mac.Forms
{
	public class SelectFolderDialogHandler : WidgetHandler<NSOpenPanel, SelectFolderDialog>, SelectFolderDialog.IHandler
	{
		protected override NSOpenPanel CreateControl()
		{
			return new NSOpenPanel();
		}

		protected override void Initialize()
		{
			Control.CanChooseDirectories = true;
			Control.CanChooseFiles = false;
			Control.CanCreateDirectories = true;

			base.Initialize();
		}

		public DialogResult ShowDialog(Window parent)
		{
			MacView.InMouseTrackingLoop = false;
			var ret = Control.RunModal();
			return ret == 1 ? DialogResult.Ok : DialogResult.Cancel;
		}

		public string Title
		{
			get => Control.Message;
			set => Control.Message = value ?? string.Empty;
		}

		public string Directory
		{
			get => Control.Url?.Path ?? Control.DirectoryUrl.Path;
			set => Control.DirectoryUrl = NSUrl.FromFilename(value);
		}

	}
}

