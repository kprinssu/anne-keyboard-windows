using System;
using System.Diagnostics;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.UI.Core;
using Windows.Storage.Streams;

namespace AnneProKeyboard
{
    public class KeyboardWriter
    {
        public EventHandler OnWriteFinished;
        public EventHandler OnWriteFailed;

        private CoreDispatcher Dispatcher;

        private GattCharacteristic WriteGATT;
        private byte[] MetaData;
        private byte[] SendData;
        private byte[] SendBuffer;

        private int BlocksSent;
        private int MaxBlocks;
        private int BytesSent;

        private const int OAD_BLOCK_SIZE = 14;
        private const int OAD_BUFFER_SIZE = 20;

        public KeyboardWriter(CoreDispatcher Dispatcher, GattCharacteristic WriteGATT, byte[] MetaData, byte[] SendData)
        {
            if (MetaData.Length != 3)
            {
                throw new Exception("Meta Data byte array longer than 4 bytes!");
            }

            this.Dispatcher = Dispatcher;

            this.WriteGATT = WriteGATT;
            this.MetaData = MetaData;
            this.SendData = SendData;

            this.SendBuffer = new byte[OAD_BUFFER_SIZE];
            this.BlocksSent = 0;
            this.BytesSent = 0;
            this.MaxBlocks = (int)Math.Ceiling((SendData.Length * 1.0) / OAD_BLOCK_SIZE);
        }

        public async void WriteToKeyboard()
        {
            if (this.BlocksSent < this.MaxBlocks)
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                {
                    try
                    {
                        Array.Copy(this.MetaData, this.SendBuffer, this.MetaData.Length);

                        if (this.SendData.Length < OAD_BLOCK_SIZE)
                        {
                            Array.Copy(this.SendData, 0, this.SendBuffer, 3, this.SendData.Length);
                        }
                        else
                        {
                            if (BytesSent + OAD_BLOCK_SIZE < this.SendData.Length)
                            {
                                this.SendBuffer[3] = 16;
                            }
                            else
                            {
                                this.SendBuffer[3] = (byte)((this.SendData.Length - this.BytesSent) + 2);
                            }

                            this.SendBuffer[4] = (byte)this.MaxBlocks;
                            this.SendBuffer[5] = (byte)this.BlocksSent;

                            Array.Copy(this.SendData, BytesSent, this.SendBuffer, 6, this.SendBuffer[3] - 2);
                        }

                        var writer = new DataWriter();
                        writer.WriteBytes(this.SendBuffer);
                        var result = await WriteGATT.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);

                        // throw an error?
                        if (result != GattCommunicationStatus.Success)
                        {
                            return;
                        }

                        this.BlocksSent += 1;
                        this.BytesSent += OAD_BLOCK_SIZE;

                        WriteToKeyboard();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);

                        EventHandler handler = this.OnWriteFailed;
                        if (handler != null)
                        {
                            handler(ex, EventArgs.Empty);
                        }
                    }
                });
            }
            else
            {
                EventHandler handler = this.OnWriteFinished;
                if (handler != null)
                {
                    handler(null, EventArgs.Empty);
                }
            }
        }
    }
}
