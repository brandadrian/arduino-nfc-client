using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ArduinoNfcClient
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //Card loading and withdrawal information.
        private int amountToWithdraw;
        private int amountToLoad;
        private NfcCardInformation nfcCardInformationReadout;
        private NfcCardInformation nfcCardInformationToWrite;

        private bool isCardReady;

        //Communication members.
        private SerialPortManager serialPortManager;
        private List<string> availablePorts;
        private string comPort;
        private int baudrate;
        private string state;
        private string response;

        public MainWindowViewModel()
        {
            WriteCommand = new ActionCommand(OnWriteCommand, OnCanExecute);
            LoadCommand = new ActionCommand(OnLoadCommand, OnCanExecute);
            WithdrawCommand = new ActionCommand(OnWithdrawCommand, OnCanExecute);
            ReadCommand = new ActionCommand(OnReadCommand, OnCanExecute);
            DisconnectCommand = new ActionCommand(OnDisconnectCommand);
            ConnectCommand = new ActionCommand(OnConnectCommand);

            serialPortManager = new SerialPortManager();
            serialPortManager.DataReceivedEvent += SerialPortManager_DataReceivedEvent;
            serialPortManager.StateUpdateEvent += SerialPortManager_StateUpdateEvent;
            AvailablePorts = serialPortManager.GetAvailableComPorts();
            ComPort = availablePorts.FirstOrDefault();
            Baudrate = 9600;
            State = "Not connected";
            IsCardReady = false;
        }

        public List<string> AvailablePorts
        {
            get
            {
                return availablePorts;
            }
            set
            {
                availablePorts = value;
                NotifyPropertyChanged("AvailablePorts");
            }
        }

        public string Response
        {
            get
            {
                return response;
            }
            set
            {
                response = value;
                NotifyPropertyChanged("Response");
            }
        }

        public string State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
                NotifyPropertyChanged("State");
            }
        }

        public int AmountToWithdraw
        {
            get
            {
                return amountToWithdraw;
            }
            set
            {
                amountToWithdraw = value;
                NotifyPropertyChanged("AmountToWithdraw");
            }
        }

        public int AmountToLoad
        {
            get
            {
                return amountToLoad;
            }
            set
            {
                amountToLoad = value;
                NotifyPropertyChanged("AmountToLoad");
            }
        }

        public bool IsCardReady
        {
            get
            {
                return isCardReady;
            }
            set
            {
                isCardReady = value;
                NotifyPropertyChanged("IsCardReady");
            }
        }

        public NfcCardInformation NfcCardInformationReadout
        {
            get
            {
                return nfcCardInformationReadout;
            }
            set
            {
                nfcCardInformationReadout = value;
                NotifyPropertyChanged("NfcCardInformationReadout");
            }
        }

        public NfcCardInformation NfcCardInformationToWrite
        {
            get
            {
                return nfcCardInformationToWrite;
            }
            set
            {
                nfcCardInformationToWrite = value;
                NotifyPropertyChanged("NfcCardInformationToWrite");
            }
        }

        public string ComPort
        {
            get
            {
                return comPort;
            }
            set
            {
                comPort = value;
                NotifyPropertyChanged("ComPort");
            }
        }

        public int Baudrate
        {
            get
            {
                return baudrate;
            }
            set
            {
                baudrate = value;
                NotifyPropertyChanged("Baudrate");
            }
        }

        public ICommand DisconnectCommand
        {
            get;
            set;
        }

        public ICommand ConnectCommand
        {
            get;
            set;
        }

        public ICommand WriteCommand
        {
            get;
            set;
        }

        public ICommand LoadCommand
        {
            get;
            set;
        }

        public ICommand WithdrawCommand
        {
            get;
            set;
        }

        public ICommand ReadCommand
        {
            get;
            set;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnDisconnectCommand(object parameter)
        {
            serialPortManager.Disconnect();
        }

        private void OnConnectCommand(object parameter)
        {
            serialPortManager.Connect(ComPort, Baudrate);
        }

        private void OnWriteCommand(object parameter)
        {
            try
            {
                if (NfcCardInformationToWrite != null)
                {
                    NfcCardInformation cardInformationToWrite = new NfcCardInformation(NfcCardInformationToWrite.Uid, NfcCardInformationToWrite.Information, NfcCardInformationToWrite.CardValue);
                    string data = NfcCardInformation.BuildNfcCardInformationToWrite(cardInformationToWrite);
                    serialPortManager.SendData(data);
                }

                IsCardReady = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnWithdrawCommand(object parameter)
        {
            try
            {
                if (NfcCardInformationReadout != null)
                {
                    if (NfcCardInformationReadout.CardValue >= AmountToWithdraw)
                    {
                        NfcCardInformation cardInformationToWrite = new NfcCardInformation(NfcCardInformationReadout.Uid, NfcCardInformationReadout.Information, NfcCardInformationReadout.CardValue - AmountToWithdraw);
                        string data = NfcCardInformation.BuildNfcCardInformationToWrite(cardInformationToWrite);
                        serialPortManager.SendData(data);
                    }
                    else
                    {
                        MessageBox.Show("Not enough money! Please Load first", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }

                IsCardReady = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnLoadCommand(object parameter)
        {
            try
            {
                if (NfcCardInformationReadout != null)
                {
                    NfcCardInformation cardInformationToWrite = new NfcCardInformation(NfcCardInformationReadout.Uid, NfcCardInformationReadout.Information, NfcCardInformationReadout.CardValue + AmountToLoad);
                    string data = NfcCardInformation.BuildNfcCardInformationToWrite(cardInformationToWrite);
                    serialPortManager.SendData(data);
                }

                IsCardReady = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnReadCommand(object parameter)
        {
            serialPortManager.SendData("readDataCommand#");
        }

        private bool OnCanExecute(object arg)
        {
            return serialPortManager.IsConnected;
        }

        private void SerialPortManager_StateUpdateEvent(object sender, SerialPortManagerStateUpdateEventArgs e)
        {
            State = e.Message;
        }

        private void SerialPortManager_DataReceivedEvent(object sender, SerialPortManagerDataReceivedEventArgs e)
        {
            if ((e.Data.Contains("readDataResponse") || (e.Data.Contains("cardRecognizedEvent")) || (e.Data.Contains("writeDataResponse"))))
            {
                NfcCardInformationReadout = NfcCardInformation.BuildReceivedNfcCardInformation(e.Data);
                NfcCardInformationToWrite = new NfcCardInformation(NfcCardInformationReadout.Uid, NfcCardInformationReadout.Information, NfcCardInformationReadout.CardValue);
            }

            IsCardReady = false;

            if (e.Data.Contains("cardRecognizedEvent"))
                IsCardReady = true;

            Response = DateTime.Now.ToString("HH:mm:ss") + "; " + e.Data + "\n" + Response;
        }
    }
}
