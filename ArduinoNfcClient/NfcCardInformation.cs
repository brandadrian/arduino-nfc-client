using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoNfcClient
{
    public class NfcCardInformation
    {
        public NfcCardInformation(string uid, string information, int cardValue)
        {
            Uid = uid;
            Information = information;
            CardValue = cardValue;
        }

        public string Uid
        {
            get;
            set;
        }

        public string Information
        {
            get;
            set;
        }

        public int CardValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the NFC card information.
        /// Content will be received in following format.
        /// receivedContent[0] = responseText
        /// receivedContent[1] = "UID"
        /// receivedContent[2] = Data of UID
        /// receivedContent[3] = "INFO"
        /// receivedContent[4] = Data of INFO
        /// receivedContent[5] = "VALUE"
        /// receivedContent[6] = Data of VALUE
        /// </summary>
        /// <param name="rawData">The raw data.</param>
        /// <returns></returns>
        public static NfcCardInformation BuildReceivedNfcCardInformation(string rawData)
        {
            NfcCardInformation nfcCardInformation = null;
            string[] receivedContent = rawData.Split(';');
            nfcCardInformation = new NfcCardInformation(receivedContent[2], receivedContent[4], Int32.Parse(receivedContent[6]));
            return nfcCardInformation;
        }

        /// <summary>
        /// Builds the NFC card information to write to the arduino.
        /// Will be in following format: writeData;UID;uidData;INFO;infoData;VALUE;valueData;# 
        /// </summary>
        /// <param name="nfcCardInformation">The NFC card information.</param>
        /// <returns></returns>
        public static string BuildNfcCardInformationToWrite(NfcCardInformation nfcCardInformation)
        {
            return "writeDataCommand;" +
                "UID;" + nfcCardInformation.Uid + ";" +
                "INFO;" + nfcCardInformation.Information + ";" +
                "VALUE;" + nfcCardInformation.CardValue.ToString() + ";#";
        }
    }
}
