/*
 * Write personal data of a MIFARE RFID card using a RFID-RC522 reader
 * Uses MFRC522 - Library to use ARDUINO RFID MODULE KIT 13.56 MHZ WITH TAGS SPI W AND R BY COOQROBOT. 
 * -----------------------------------------------------------------------------------------
 *             MFRC522      Arduino       Arduino   Arduino    Arduino          Arduino
 *             Reader/PCD   Uno/101       Mega      Nano v3    Leonardo/Micro   Pro Micro
 * Signal      Pin          Pin           Pin       Pin        Pin              Pin
 * -----------------------------------------------------------------------------------------
 * RST/Reset   RST          9             5         D9         RESET/ICSP-5     RST
 * SPI SS      SDA(SS)      10            53        D10        10               10
 * SPI MOSI    MOSI         11 / ICSP-4   51        D11        ICSP-4           16
 * SPI MISO    MISO         12 / ICSP-1   50        D12        ICSP-1           14
 * SPI SCK     SCK          13 / ICSP-3   52        D13        ICSP-3           15
 *
 * Hardware required:
 * Arduino
 * PCD (Proximity Coupling Device): NXP MFRC522 Contactless Reader IC
 * PICC (Proximity Integrated Circuit Card): A card or tag using the ISO 14443A interface, eg Mifare or NTAG203.
 * The reader can be found on eBay for around 5 dollars. Search for "mf-rc522" on ebay.com. 
 */

#include <SPI.h>
#include <MFRC522.h>


#define RST_PIN         9           // Configurable, see typical pin layout above
#define SS_PIN          10          // Configurable, see typical pin layout above

//eigene Definitionen
#define MAX_RESPONSE_LENGTH 100
#define MAX_STRING_LENGTH   100
#define MAX_BUFFER_LENGTH   18
#define UID_STRING_LENGTH   5
#define VALUE_STRING_LENGTH 5

#define INFO_BLOCK_ADDR     1
#define VALUE_BLOCK_ADDR    2

#define OPTION_RECORGNIZED  0
#define OPTION_READ_DATA    1

MFRC522 mfrc522(SS_PIN, RST_PIN);   // Create MFRC522 instance
MFRC522::MIFARE_Key key;
unsigned long UID, cardValue;
char infoBlockString[MAX_BUFFER_LENGTH], readCommandString[MAX_STRING_LENGTH];

void setup() {
  setKey();
  Serial.begin(9600);        // Initialize serial communications with the PC
  SPI.begin();               // Init SPI bus
  mfrc522.PCD_Init();        // Init MFRC522 card
}

/*
 * Funktion welches den Schlüssel setzt
 */
 void setKey(void)
 {
  // Prepare key - all keys are set to FFFFFFFFFFFFh at chip delivery from the factory.
  for (byte i = 0; i < 6; i++) key.keyByte[i] = 0xFF;
 }

/*
 *Funktion um den Block zu Authetifizieren um später lesen oder schreiben zu können 
 */
void authenticatingBlock(byte block)
{
  MFRC522::StatusCode status;
  
  setKey();

  //zur Info
  //Serial.println(F("Authenticating using key A..."));
  status = mfrc522.PCD_Authenticate(MFRC522::PICC_CMD_MF_AUTH_KEY_A, block, &key, &(mfrc522.uid));
  if (status != MFRC522::STATUS_OK) {
     //Serial.print(F("PCD_Authenticate() failed: "));
     //Serial.println(mfrc522.GetStatusCodeName(status));
     return;
  }
  //Authentifizierung war erfolgreich
  //else Serial.println(F("PCD_Authenticate() success: "));
}

/*
 * Schreibt einen Block auf das Display
 */
void printDataBlock(byte blockAddr)
{
  //Um 16 Byte zu lesen muss der Buffer 18 gross sein
  byte bufferSize = MAX_BUFFER_LENGTH, buffer[16];
  char singleChar;
  
  MFRC522::StatusCode status;

  authenticatingBlock(blockAddr);
  
  status = mfrc522.MIFARE_Read(blockAddr, buffer, &bufferSize);
  if (status != MFRC522::STATUS_OK) {
      //Serial.print(F("MIFARE_Read() failed: "));
      //Serial.println(mfrc522.GetStatusCodeName(status));
      return;
  }
  //konnte gelesen werden
  //else Serial.println(F("MIFARE_Read() success: "));

  //Ausgabe der gelesenen Daten
  Serial.print("block: ");
  Serial.print(blockAddr);
  Serial.print(", ");
  Serial.print("read data: ");
  for (byte i = 0; i < 16; i++) {
    Serial.print(buffer[i], HEX);
    Serial.print(" ");
  }
  Serial.print("   String: ");
  for (byte i = 0; i < 16; i++) {
    singleChar = buffer[i];
    Serial.print(singleChar);
  }
  Serial.print(";#");
}

/*
 * Liest von der RFID Karte. Es kann die grösse des Blockes angegeben werden. Zusätzlich wird dieser auf der Konsole ausgegeben
 */
void readDataBlock(byte blockAddr, byte *buffer)
{
  //Um 16 Byte zu lesen muss der Buffer 18 gross sein
  byte bufferSize = MAX_BUFFER_LENGTH;
  char singleChar;
  
  MFRC522::StatusCode status;

  authenticatingBlock(blockAddr);
  
  status = mfrc522.MIFARE_Read(blockAddr, buffer, &bufferSize);
  if (status != MFRC522::STATUS_OK) {
      //Serial.print(F("MIFARE_Read() failed: "));
      //Serial.println(mfrc522.GetStatusCodeName(status));
      Serial.println("readDataErrorResponse;#");
      return;
  }
  //konnte gelesen werden
  //else Serial.println(F("MIFARE_Read() success: "));
}

/*
 * Schreibt auf die RFID Karte. Es kann die Grösse innerhalb des Blockes angegeben werden. Zusäzlich wird die Daten, welche rausgeschriben werden auf dem Display angezeigt.
 */
void writeDataBlock(byte blockAddr, byte *buffer)
{
  char singleChar;
  int bufferSize = 16;
  MFRC522::StatusCode status;

  authenticatingBlock(blockAddr);
  
  status = mfrc522.MIFARE_Write(blockAddr, buffer, bufferSize);
  if (status != MFRC522::STATUS_OK) {
    //Serial.print(F("MIFARE_Write() failed: "));
    //Serial.println(mfrc522.GetStatusCodeName(status));
    Serial.println("writeDataErrorResponse;#");
    //Serial.println("");
    return;
  }
  //Konnte den Block schreiben
  //else Serial.println(F("MIFARE_Write() success: "));
}

/*
 * Setzt den Info Block zusammen. Es wird ein Info String auf die Karte geschrieben
 */
void writeInfoBlock(char *pInfoString)
{
  byte buffer[16];
  int strlenInfoString;

  strlenInfoString = strlen(pInfoString);
  for(int i = 0; i < strlenInfoString; i++) buffer[i] = pInfoString[i];
  buffer[strlenInfoString] = ';';
  for(int i = strlenInfoString + 1; i < 16; i++)  buffer[i] = 0;

  writeDataBlock(INFO_BLOCK_ADDR, buffer);
}

/*
 * Liest den Info String aus
 */
void getInfoBlock(void)
{
  byte buffer[16];
  byte searchKeyChar = 0;

  readDataBlock(INFO_BLOCK_ADDR, buffer);
  for(byte i = 0; i < 16; i++) infoBlockString[i] = (char)buffer[i];
  for(byte i = 0; i < 16; i++) if(infoBlockString[i] == ';') infoBlockString[i] = 0;
}

/*
 * Schreibt den Kartenwert
 */
void writeCardBalance(unsigned long cardValue)
{
  byte buffer[16];
  
  buffer[0] = (cardValue >> 24) & 0xFF;
  buffer[1] = (cardValue >> 16) & 0xFF;
  buffer[2] = (cardValue >> 8) & 0xFF;
  buffer[3] = cardValue & 0xFF;
  for(byte i = 4; i < 16; i++) buffer[i] = 0;

  writeDataBlock(VALUE_BLOCK_ADDR, buffer);
}

/*
 * Lest den Kartenwert aus
 */
unsigned long readCardBalance(void)
{
  byte buffer[16];
  byte cardValueByteArray[4];
  unsigned long cardValue;
  
  readDataBlock(VALUE_BLOCK_ADDR, buffer);

  cardValue =  buffer[0] << 24;
  cardValue += buffer[1] << 16;
  cardValue += buffer[2] << 8;
  cardValue += buffer[3];

  return cardValue;
}

/*
 * Schreibt die UID auf das Display
 */
 void readUID(void)
 {
  unsigned long UID_unsigned;
  UID_unsigned =  mfrc522.uid.uidByte[0] << 24;
  UID_unsigned += mfrc522.uid.uidByte[1] << 16;
  UID_unsigned += mfrc522.uid.uidByte[2] <<  8;
  UID_unsigned += mfrc522.uid.uidByte[3];

  //Hex Format
  Serial.print("Printing HEX UID : ");
  for (byte i = 0; i < mfrc522.uid.size; i++) {
    Serial.print(mfrc522.uid.uidByte[i] < 0x10 ? " 0" : " ");
    Serial.print(mfrc522.uid.uidByte[i], HEX);
  } 

  delay(100);
  //Int Format
  Serial.print("    ");
  Serial.print("UID Unsigned int: "); 
  Serial.print(UID_unsigned);
  Serial.println(";#");
  delay(100);
 }

 /*
 * Gibt die UID zurück
 */
 unsigned long getUID(void)
 {
  unsigned long UID_unsigned;
  UID_unsigned =  mfrc522.uid.uidByte[0] << 24;
  UID_unsigned += mfrc522.uid.uidByte[1] << 16;
  UID_unsigned += mfrc522.uid.uidByte[2] <<  8;
  UID_unsigned += mfrc522.uid.uidByte[3];

  return UID_unsigned;
 }

/*
 * Schreibt einen String auf das Display, welche alle Karten Infos beeinhaltet
 */
 void printCardInfos(byte Option)
 {
  int responseStringLength;
  char UIDString[UID_STRING_LENGTH], valueString[VALUE_STRING_LENGTH], responseString[MAX_RESPONSE_LENGTH], singleChar;

  if(Option == OPTION_RECORGNIZED)
  {
    //aktuellen Daten der Karte abfragen
    UID = getUID();
    sprintf(UIDString, "%lu", UID); 
    getInfoBlock();
    cardValue = readCardBalance();
    sprintf(valueString, "%lu", cardValue); 
  
    strcpy(responseString, "cardRecognizedEvent;UID;");         
    strcat(responseString, UIDString);
    strcat(responseString, ";INFO;");
    strcat(responseString, infoBlockString);
    strcat(responseString, ";VALUE;");
    strcat(responseString, valueString);
    strcat(responseString, ";#");
  }
  else
  {
    //Daten werden nicht neu ausgelesen sondern die aktuellen Werte werden heraus geschrieben
    sprintf(UIDString, "%lu", UID);
    sprintf(valueString, "%lu", cardValue);
    
    strcpy(responseString, "readDataResponse;UID;");
    strcat(responseString, UIDString);
    strcat(responseString, ";INFO;");
    strcat(responseString, infoBlockString);
    strcat(responseString, ";VALUE;");
    strcat(responseString, valueString);
    strcat(responseString, ";#");
  }

  //Soll die aktuellen Daten der Karte schicken
  responseStringLength = strlen(responseString);
  for (byte i = 0; i < responseStringLength; i++) 
  {
    singleChar = (char)responseString[i];
    Serial.print(singleChar);
  }
 }

/*
 * Schreibt die Info und den Kartenwert auf die Karte
 */
 void writeCardInfos(void)
 {
  int strlenSubReadCommandString;
  char *pSubReadCommandString, UIDString[UID_STRING_LENGTH], subReadCommandString[MAX_STRING_LENGTH], valueString[VALUE_STRING_LENGTH];
  
  //Die Informationen aus dem String holen
  //UID
  pSubReadCommandString = strstr(readCommandString, "UID;");
  strlenSubReadCommandString = strlen(pSubReadCommandString);

  for(byte i = 0; i < strlenSubReadCommandString; i++)  subReadCommandString[i] = pSubReadCommandString[i];
  subReadCommandString[strlenSubReadCommandString] = 0;
  strlenSubReadCommandString = strlen(subReadCommandString);
  
  for(byte i = 4; i < strlenSubReadCommandString; i++) 
  {
    if(subReadCommandString[i] != ';')  UIDString[i-4] = subReadCommandString[i];
    else                                UIDString[i-4] = 0;
  }
  UID = atol(UIDString);

  //Info
  pSubReadCommandString = strstr(subReadCommandString, "INFO;");
  strlenSubReadCommandString = strlen(pSubReadCommandString);
  for(byte i = 0; i < strlenSubReadCommandString; i++)  subReadCommandString[i] = pSubReadCommandString[i];

  for(byte i = 5; i < strlenSubReadCommandString; i++) 
  {
    if(subReadCommandString[i] != ';')  infoBlockString[i-5] = subReadCommandString[i];
    else                                infoBlockString[i-5] = 0;
  }

  //Value
  pSubReadCommandString = strstr(subReadCommandString, "VALUE;");
  strlenSubReadCommandString = strlen(pSubReadCommandString);
  for(byte i = 0; i < strlenSubReadCommandString; i++)  subReadCommandString[i] = pSubReadCommandString[i];
  subReadCommandString[strlenSubReadCommandString] = 0;
  
  for(byte i = 6; i < strlenSubReadCommandString; i++) 
  {
    if(subReadCommandString[i] != ';')  
    {
      valueString[i-6] = subReadCommandString[i];
    }
    else  
    {
      valueString[i-6] = 0;
      break;
    }
  }
  cardValue = atol(valueString);
  
  //jetzt die Werte auf der Karte verändern
  writeInfoBlock(infoBlockString);
  writeCardBalance(cardValue);

  //wurde bearbeitet
  delay(100);
  readCommandString[0] = 0;
 }

/*
 * Schreibt den gesamten Inhalt der Karte aufs Display
 */
 void printCardData(void)
 {
  readUID();
  
  int printBlockSize = 64;
  for(byte i = 0; i < printBlockSize; i++)
  {
    authenticatingBlock(i);
    printDataBlock(i);
    delay(100);
  }
  //wurde ausgeführt
 }


void loop() {
  byte block = 0, buffer[MAX_BUFFER_LENGTH], readCommandByte[MAX_STRING_LENGTH];
  int len = 0;
  
  //Wenn Karte erkannt wurde sollen die Karten Informationen gesendet werden
  if (mfrc522.PICC_IsNewCardPresent())
  {
    if (mfrc522.PICC_ReadCardSerial())
    {
      printCardInfos(OPTION_RECORGNIZED);
      while(strstr(readCommandString, "writeDataCommand") == NULL) 
      {
        len = Serial.readBytesUntil('#', (char *) readCommandByte, MAX_STRING_LENGTH); 
        for (int i = 0; i < len; i++) readCommandString[i] = readCommandByte[i];     
        readCommandString[len] = 0;
        if(strcmp(readCommandString, "readDataCommand") == 0) 
        {
          printCardInfos(OPTION_READ_DATA);
          //wurde ausgeführt
          readCommandString[0] = 0;
        }
        else if(strcmp(readCommandString, "readCardCommand") == 0)
        {
          printCardData();
          readCommandString[0] = 0;
          break;
        }    
      }
    }
  }

  //Bei einem Schreibefehl sollen die neuen Karteninformation abgespeichert werden
  if(strstr(readCommandString, "writeDataCommand") != NULL) writeCardInfos();

  
  mfrc522.PICC_HaltA(); // Halt PICC
  mfrc522.PCD_StopCrypto1();  // Stop encryption on PCD 
}  
