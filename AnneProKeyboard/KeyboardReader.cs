using System;
using System.Threading.Tasks;

using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth.Background;
using Windows.Storage.Streams;

namespace AnneProKeyboard
{
	// Most of the below code was derived from https://github.com/DrJukka/BLETestStuffWindows/blob/0893ac37746cf26951d1b6ca19e4d8df85b0606a/HeartbeatBg/HeartbeatMonitor/HeartbeatMonitorBackgroundTask.cs
	// Keyboard logic is taken from the Obins Android app
	public sealed class KeyboardReader : IBackgroundTask
	{
		private IBackgroundTaskInstance _taskInstance = null;
		private BackgroundTaskDeferral _taskDeferral;

		public KeyboardReader()
		{
			System.Diagnostics.Debug.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA Reader init!");
		}

		public async void Run(IBackgroundTaskInstance task_instance)
		{
			System.Diagnostics.Debug.WriteLine("BBBBBBBBBBBBBBBBBBBBBBBBBBBB Reader running!");

			this._taskInstance = task_instance;
			this._taskDeferral = task_instance.GetDeferral();

			task_instance.Canceled += new BackgroundTaskCanceledEventHandler(this.OnTaskCancelled);

			GattCharacteristicNotificationTriggerDetails trigger_details = (GattCharacteristicNotificationTriggerDetails)task_instance.TriggerDetails;

			byte[] data = new byte[trigger_details.Value.Length];
			DataReader reader = DataReader.FromBuffer(trigger_details.Value);
			reader.ReadBytes(data);

			//TODO: do stuff with this data

			this._taskDeferral.Complete();
		}

		private void OnTaskCancelled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
		{
			//TODO: handle disconnection logic here
		}
	}
}
