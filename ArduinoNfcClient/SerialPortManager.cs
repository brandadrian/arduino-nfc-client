using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoNfcClient
{
    public class SerialPortManager
    {
        public event EventHandler<SerialPortManagerDataReceivedEventArgs> DataReceivedEvent;
        public event EventHandler<SerialPortManagerStateUpdateEventArgs> StateUpdateEvent;

        private List<string> receivingBuffer;
        private SerialPort arduinoPort;

        public SerialPortManager()
        {
            receivingBuffer = new List<string>();
        }

        public string ComPort
        {
            get;
            private set;
        }

        public int Baudrate
        {
            get;
            private set;
        }

        public bool IsConnected
        {
            get
            {
                return ((arduinoPort != null) && (arduinoPort.IsOpen));
            }
        }

        public void SendData(string data)
        {
            try
            {
                if (data != null)
                {
                    byte[] message = System.Text.Encoding.UTF8.GetBytes(data);
                    arduinoPort.Write(message, 0, message.Length);
                }
            }
            catch (Exception ex)
            {
                OnUpdateStateEvent(this, new SerialPortManagerStateUpdateEventArgs("Error: " + ex.Message));
            }
        }

        public void Connect(string comPort, int baudrate)
        {
            try
            {
                if (!string.IsNullOrEmpty(comPort))
                {
                    arduinoPort = new SerialPort(comPort, baudrate);
                    arduinoPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    arduinoPort.Open();
                    OnUpdateStateEvent(this, new SerialPortManagerStateUpdateEventArgs("Connected to ComPort: " + comPort + " Baudrate: " + baudrate));
                }
                else
                {
                    OnUpdateStateEvent(this, new SerialPortManagerStateUpdateEventArgs("Error: No Com Port!"));
                }
            }
            catch (Exception ex)
            {
                OnUpdateStateEvent(this, new SerialPortManagerStateUpdateEventArgs("Error: " + ex.Message));
            }
        }

        public void Disconnect()
        {
            try
            {
                if (arduinoPort != null)
                {
                    arduinoPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
                    arduinoPort.Close();
                    OnUpdateStateEvent(this, new SerialPortManagerStateUpdateEventArgs("Not connected"));
                }
                else
                {
                    OnUpdateStateEvent(this, new SerialPortManagerStateUpdateEventArgs("Error: Not connected!"));
                }
            }
            catch (Exception ex)
            {
                OnUpdateStateEvent(this, new SerialPortManagerStateUpdateEventArgs("Error: " + ex.Message));
            }
        }

        public List<string> GetAvailableComPorts()
        {
            return new List<string>(SerialPort.GetPortNames());
        }

        private void OnDataReceivedEvent(object sender, SerialPortManagerDataReceivedEventArgs eventArgs)
        {
            if (DataReceivedEvent != null)
                DataReceivedEvent(this, eventArgs);
        }

        private void OnUpdateStateEvent(object sender, SerialPortManagerStateUpdateEventArgs eventArgs)
        {
            if (StateUpdateEvent != null)
                StateUpdateEvent(this, eventArgs);
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort serialPort = (SerialPort)sender;
                FillRecevingBuffer(serialPort.ReadExisting());
            }
            catch (Exception ex)
            {
                OnUpdateStateEvent(this, new SerialPortManagerStateUpdateEventArgs("Error: " + ex.Message));
            }
        }

        /// <summary>
        /// Fill the receiving buffer and waint until ';#' occures. This indicates the end of the input string.
        /// THis method has to be done because only 32 byte can be read at once.
        /// </summary>
        /// <param name="data"></param>
        private void FillRecevingBuffer(string data)
        {
            receivingBuffer.Add(data);

            if (data.Contains(";#"))
            {
                OnDataReceivedEvent(this, new SerialPortManagerDataReceivedEventArgs(string.Join("", receivingBuffer)));
                receivingBuffer.Clear();
            }
        }
    }

    public class SerialPortManagerStateUpdateEventArgs : EventArgs
    {
        public SerialPortManagerStateUpdateEventArgs(string message)
        {
            Message = message;
        }

        public string Message
        {
            get;
            private set;
        }
    }

    public class SerialPortManagerDataReceivedEventArgs : EventArgs
    {
        public SerialPortManagerDataReceivedEventArgs(string data)
        {
            Data = data;
        }

        public string Data
        {
            get;
            private set;
        }
    }
}
