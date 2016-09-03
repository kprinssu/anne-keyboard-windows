using System;
using System.Threading.Tasks;

using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Background;
using Windows.Storage.Streams;

namespace AnneProKeyboardMonitor
{
	// Most of the below code was derived from https://github.com/DrJukka/BLETestStuffWindows/blob/0893ac37746cf26951d1b6ca19e4d8df85b0606a/HeartbeatBg/HeartbeatMonitor/HeartbeatMonitorBackgroundTask.cs
	// Keyboard logic is taken from the Obins Android app
	public sealed class KeyboardMonitor : IBackgroundTask
	{
		private IBackgroundTaskInstance _taskInstance = null;
		private BackgroundTaskDeferral _taskDeferral;

		//READ IF DEBUGGING
		//NOTE: If you have debug points set and yet the task is not getting executed, you need to *uninstall* the app first and then debug
		// see https://stackoverflow.com/questions/13052242/cant-register-a-time-triggered-background-task

		public KeyboardMonitor()
		{
			System.Diagnostics.Debug.WriteLine("AnneProkeyboardMonitor init!");
		}

		public void Run(IBackgroundTaskInstance taskInstance)
		{
			System.Diagnostics.Debug.WriteLine("AnneProkeyboardMonitor running!");

			this._taskInstance = taskInstance;
			//this._taskDeferral = taskInstance.GetDeferral();

			taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(this.OnTaskCancelled);

			GattCharacteristicNotificationTriggerDetails trigger_details = (GattCharacteristicNotificationTriggerDetails)taskInstance.TriggerDetails;

			byte[] data = new byte[trigger_details.Value.Length];
			DataReader reader = DataReader.FromBuffer(trigger_details.Value);
			reader.ReadBytes(data);

			//TODO: do stuff with this data
			return;
			//this._taskDeferral.Complete();
		}

		private void OnTaskCancelled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
		{
			//TODO: handle disconnection logic here
		}
	}
}
