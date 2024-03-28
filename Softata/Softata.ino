
// RPi Pico BSP placed in the public domain by Earle F. Philhower, III, 2022
#include "softata.h"
#include "rpiboards.h"
#include <WiFi.h>
#include "rpiwatchdog.h"
#include "src/grove.h"
#include "src/grove_sensor.h"
#include "src/grove_environsensors.h"
#include "src/grove_actuator.h"
#include "src/grove_displays.h"

#include <float.h>


#include "devicesLists.h"

struct CallbackInfo  BME280CBInfo;
// Used when first connected to change inbuilt LED blink rate.
bool hasConnected = false;


// Use one or other in testing
// Only used when Serial1/2 Setup is called from client.
// Comment out both normally.
//#define SERIAL1LOOPBACK
//#define SERIAL2LOOPBACK

//Grove_Sensor *grove_Sensor;

const char* ssid = STASSID;
const char* password = STAPSK;

#define BUFFSIZE 128
byte Buffer[BUFFSIZE];

int port = PORT;

WiFiServer server(port);

bool first = true;
void setup() {


  hasConnected = false; 
  Serial.begin(115200);
  while (!Serial);
  WiFi.mode(WIFI_STA);
  WiFi.setHostname("PicoW2");
  Serial.printf("Connecting to '%s' with '%s'\n", ssid, password);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    Serial.print(".");
    delay(100);
  }
  Serial.printf("\nConnected to WiFi\n\nConnect to server at %s:%d\n", WiFi.localIP().toString().c_str(), port);

  server.begin();
  
  InitSensorList();
  InitDisplayList();
  InitActuatorList();  
  first = false;
  // Just to be safe don't simulataneously setup server and client
  uint32_t val = initialSynch;
  Serial.print("Initial Core1-Core2 Synch value:");
  Serial.println(val);
  rp2040.fifo.push(val);
  uint32_t sync = rp2040.fifo.pop();
  if(val==sync)
    Serial.println("Core1-Core2 Setup Sync OK");
  else
    Serial.println("Core1-Core2 Setup Sync Fail");
  watchdog_enable(WATCHDOG_SECS * 1000, false);
}

//Ref: https://www.thegeekpub.com/276838/how-to-reset-an-arduino-using-code/
void (*resetFunc)(void) = 0;

bool RecvdCloud2DeviceMsg = false;
uint32_t popVal; 
byte C2DMessage[e_num+1] ={0};
int Check4RecvdCloud2DeviceMsg()
{
  // Slot Cloud2Device messages in here
  if(RecvdCloud2DeviceMsg)
  {
    Serial.println();
    RecvdCloud2DeviceMsg = false;
    if(popVal>0)
    { 
      uint32_t PopVal = popVal;
      Serial.println(PopVal,HEX);
      int length =0;
      //enum bitStuffingIndex: byte {e_cmd,e_pin,e_param,e_other,e_otherDataCount,e_data1,e_data2};
      C2DMessage[1] = PopVal % (bitStuffing[e_cmd]);
      length++;
      PopVal /= bitStuffing[e_cmd];
      C2DMessage[2] = PopVal % (bitStuffing[e_pin]);
      length++;
      PopVal /= bitStuffing[e_pin];
      C2DMessage[3] = PopVal % (bitStuffing[e_param]);
      length++;
      PopVal /= bitStuffing[e_param];
      C2DMessage[4] = PopVal % (bitStuffing[e_other]);
      length++;
      PopVal /= bitStuffing[e_other];
      C2DMessage[5] = PopVal % (bitStuffing[e_otherDataCount]);
      length++;
      PopVal /= bitStuffing[e_otherDataCount];
      if(PopVal>0)
      {
        C2DMessage[6] = PopVal % (bitStuffing[e_data1]);
        length++;
        PopVal /= bitStuffing[e_data1];
        if(PopVal>0)
        {
          C2DMessage[7] = PopVal % (bitStuffing[e_data2]);
          length++;
          PopVal /= bitStuffing[e_data2];
      } }
      C2DMessage[0] = length;
      RecvdCloud2DeviceMsg = false;
      return C2DMessage[0];
    }
  }
  return 0;
}

void loop() {
  watchdog_update();
  static int i;
  int count = 0;
  byte msg[maxRecvdMsgBytes];
  /*
  if(first){
                Serial.println(Grove_Sensor::GetListof());
                Serial.println(Grove_Actuator::GetListof());
                Serial.println(Grove_Display::GetListof());
                    Serial.println(Grove_Sensor::GetIndexOf("DHT11"));   
                    Serial.println(Grove_Sensor::GetIndexOf("BME280"));   
                    Serial.println(Grove_Sensor::GetIndexOf("JONES")); 
                    Serial.println(Grove_Display::GetIndexOf("OLED096"));
                    Serial.println(Grove_Display::GetIndexOf("LCD1602")); 
                first = false;
  }*/
                              
  delay(500);
  WiFiClient client = server.available();
  if (!client) {
    return;
  }
  Serial.println("WiFi-Server Up.");
  while (true) {
    watchdog_update();
    Serial.print("Get next command.");
    while (!client.available()) {
      delay(100);
      watchdog_update();
      if(rp2040.fifo.available())
        break;
    }
    if(rp2040.fifo.available())
    {
      popVal = rp2040.fifo.pop();
      RecvdCloud2DeviceMsg = true;
    }
    byte length=0;
    count = 0;
    if(RecvdCloud2DeviceMsg)
    {
      count=Check4RecvdCloud2DeviceMsg();
    }
    if (count>0)
    {
      length = C2DMessage[0];
      Serial.print("C2DMsg:");
      Serial.print("length=0x");
      Serial.print(C2DMessage[0],HEX);
      Serial.print(' ');
      Serial.print("cmd=0x");
      Serial.print(C2DMessage[1],HEX);
      Serial.print(' ');
      Serial.print("pin=0x");
      Serial.print(C2DMessage[2],HEX);
      Serial.print(' ');
      Serial.print("param=0x");
      Serial.print(C2DMessage[3],HEX);
      Serial.print(' ');
      Serial.print("other=0x");
      Serial.print(C2DMessage[4],HEX);
      if(length>5)
      {
        Serial.print(" [");
        Serial.print(C2DMessage[5],HEX);  
        if(true)
        {
          Serial.print(' ');
          Serial.print(C2DMessage[6],HEX);      
          if(length>7)
          {
            Serial.print(' ');
            Serial.print(C2DMessage[7],HEX);      
            if(length>8)
            {
              Serial.print(' ');
              Serial.print(C2DMessage[8],HEX);
            }
          }
        }
        Serial.print("]");
      }
      Serial.println();
      for(int i=0; i<length; i++)
      {
        msg[i] = C2DMessage[i+1];
      }
      //continue;
    }
    else
    {
      count = 0;
      length = 0;
      Serial.println("...Is connected.");
      if(!hasConnected)
      {
        hasConnected = true;
        rp2040.fifo.push( SynchMultiplier + (int)svrConnected); //Make Inbuilt LED flash faster
        while (!rp2040.fifo.available())
        {
            watchdog_update();
        }
        uint32_t sync = rp2040.fifo.pop();
      }
      length = client.read();
      Serial.print(length);
      Serial.print('-');
      count = 0;
      //byte msg[maxRecvdMsgBytes];
      if (length == 0) {
        Serial.println("Null msg.");
        return;
      }
      while (client.available() && (count != length)) {
        msg[count] = client.read();
        if(msg[0]<0xD0) 
        {
          if(count==0)
          {
            Serial.print((char)msg[count]);
            Serial.print(' ');
          }
        }
        else 
        {
          Serial.print(msg[count], HEX);
          //if(count<2)
            Serial.print(' ');
        }
        watchdog_update();
        count++;
      }
    }
    Serial.println();
    if (count != length) {
      Serial.print("count:");
      Serial.println(count);
      Serial.print("length:");
      Serial.println(length);
      Serial.println("Msg invalid.");
      return;
    }
    switch (msg[0]) {
      // Escape simple string commands here.
      // These commands to start in uppercase
      // Need first letter of these ASCII values to not match commands
      // ie Softata commands not to be between 65 to 90, 0x41 to 0x5A
      // 'A' to 'Z'
      case (byte)'B':  // Begin
        Serial.println("Ready.");
        client.print("Ready");  // Sent at first connection.
        break;
      case (byte)'E':  //End
        Serial.println("Done.");
        client.print("Done");
        // Force reset
        resetFunc();
        break;
      case (byte)'N':  //Null
        Serial.println("Null.");
        client.print("OK");
        return;
        break;
      case (byte)'R':  //Reset
        Serial.println("Resetting.");
        client.print("Reset");
        // Force reset
        resetFunc();
        break;
       case (byte)'V':  //Get Version
        Serial.println(APP_VERSION);
        client.print(APP_VERSION);
        break;
       case (byte)'D':  //Get Device Types
       {
        String devicesCSV = Grove::GetListofDevices();
        Serial.println(devicesCSV);
        client.print(devicesCSV);
       }
        break;
       default:
        // Get Softata command and parameters
        byte cmd = msg[0];
        byte pin = 0xff;
        byte param = 0xff;
        byte other = 0xff;
        byte * otherData = NULL;
        if (length > 1) {
          pin = msg[1];
          if (length > 2) {
            param = msg[2];
            if (length > 3) {
              other = msg[3];
              if(length>4)
              {
                otherData = msg+4;
              }
            }
          }
        }

        //Print command and paramaters
        String str;
        byte vaue;
        Serial.print("cmd:");
        Serial.print(cmd, HEX);
        Serial.print(' ');
        if (pin != 0xff) {
          Serial.print("pin:");
          Serial.print(pin, HEX);
          Serial.print(' ');
          if (param != 0xff) {
            Serial.print("param:");
            Serial.print(param,HEX);
            Serial.print(' ');
            if (other != 0xff) {
              Serial.print("other:");
              Serial.print(other,HEX);
            }
          }
        }
        Serial.print(' ');
        if(otherData!= NULL)
        {
          Serial.print(" otherData:[");
          for(int i=0; i<=otherData[0];i++)
          {
            Serial.print(otherData[i],HEX);
            if(i<otherData[0])
              Serial.print(',');
            else
              Serial.print(']');
          }
        }
        Serial.println();

        // Action cmds
        int value;
        switch (cmd) {
          case 0xD0:
          case 0xD1:
          case 0xD2:
          case 0xD3:
            {
              // Digital
              if (!IS_PIN_DIGITAL(pin)) {
                Serial.print("Pin is not digital");
                client.print("FAIL");
                continue;
              }
              switch (cmd) {
                case 0xD0:
                  Serial.print("pinMode:");
                  Serial.println(param);
                  pinMode(pin, (PinMode)param);
                  client.print("OK:");
                  break;
                case 0xD2:
                  Serial.println("digitalRead");
                  value = digitalRead(pin);
                  if (value)
                    client.print("ON");
                  else
                    client.print("OFF");
                  break;
                case 0xD1:
                  Serial.print("digitalWrite:");
                  Serial.println(param);
                  if (param)
                    digitalWrite(pin, HIGH);
                  else
                    digitalWrite(pin, LOW);
                  client.print("OK");
                  break;
                case 0xD3:
                  Serial.println("digitalToggle");
                  value = digitalRead(pin);
                  if (value)
                    digitalWrite(pin, LOW);
                  else
                    digitalWrite(pin, HIGH);
                  client.print("OK");
                  break;
                default:
                  Serial.println("Unknown digital cmd");
                  client.print("Unknown digital cmd");
                  break;
              }
              break;
            }
          case 0xA2:  // Analog place holder
            if (!IS_PIN_ANALOG(pin)) {
              Serial.print("Pin not Analog");
              client.print("FAIL");
              continue;
            } else {
              if (cmd == 0xA2) {              
                value = analogRead(pin);
                String msgAD = "AD:";
                msgAD += value;
                Serial.println(msgAD);
                client.print(msgAD);
                //break;
              }
            }
            break;
          case 0xB1:
            if (!IS_PIN_PWM(pin)) {
              Serial.print("Pin not PWM");
              client.print("FAIL");
              continue;
            }
            if (cmd == 0xB1) {
              Serial.print("PWM");
              analogWrite(pin, param);
              Serial.println("PWM:analogWrite()");
              client.print("OK");
            }
            break;
          case 0xC0:
          case 0xC1:
          case 0xC2:  // Servo place holder
            if (!IS_PIN_SERVO(pin)) {
              Serial.print("Pin not Servo");
              client.print("FAIL");
              continue;
            }
            Serial.println("OK-SERVO 2D cmds");
            client.print("OK-SERVO 2D cmds");
            break;
          case 0xE0:  // Setup Serial1/2
          case 0xE1:  // Get a char
          //case 0xE2: // Get a string
          //case 0xE3: // Get a string until char
          case 0xE4:  // Write a char
                      //case 0xE5: // Get Flost
                      //case 0xE6: // Get Int
          case 0xE7:
            if (!((other == 1) || (other == 2))) {
              //Nb: For serial except for setup, pin parameter is ignored
              // But need to specify Serial1 or Serial2 as 1 or 2 in other parameter
              Serial.print("Not Serial 1 or 2 (other)");
              client.print("FAIL");
              continue;
            } else {
              Stream& Comport = Serial1;  //Comport doesn't work for some aspects of Serial2
              if (other == 2) {
                Serial.println("Serial2");
                Comport = Serial2;
              } else {
                Serial.println("Serial1");
              }

              char ch;
              String str;
              String msgSerial;
              float fNum;
              int iNum;
              switch (cmd) {
                case 0xE0:  // Set Pins (Provide Tx, Determine Rx) and set Baudrate from list
                  {

#ifdef GROVE_RPI_PICO_SHIELD
                  byte Tx = UART0TX;
                  byte Rx = UART0RX;
                  if(other==2) {
                    Tx = UART1TX;
                    Rx = UART1RX;
                  }
#elif defined(RPI_PICO_DEFAULT)
                    byte Tx = pin;
                    byte Rx = pin + 1;
#endif
                    if (IS_PIN_SERIAL_TX(Tx)) {
                      int baudrate = Baudrates[param];
                      if (other == 1) {
                        Serial1.setTX(Tx);
                        Serial1.setRX(Rx);
                        //Serial1.SetRTS()
                        //Serial1.setCTS();
                        Serial1.begin(baudrate, SERIAL_8N1);
#ifdef SERIAL1LOOPBACK
                        Serial.println("Serial1 Loopback");
                        Serial.print("Tx:");
                        Serial.print(Tx);
                        Serial.print("  Rx:");
                        Serial.println(Rx);
                        Serial.println("Serial1 Loopback Test Sending 64");
                        delay(500);
                        Serial2.write(64);
                        while (!Serial1.available()) { delay(100); };
                        byte chw = Serial1.read();
                        Serial.print("Serial1 Loopback Test Got:0x");
                        Serial.println(chw, HEX);
#endif
                        Serial.println("Serial1.setup");
                        client.print("OK:");
                      } else if (other == 2) {
                        Serial2.setTX(Tx);
                        Serial2.setRX(Rx);
                        Serial2.begin(baudrate, SERIAL_8N1);
#ifdef SERIAL2LOOPBACK
                        Serial.println("Serial2 Loopback");
                        Serial.print("Tx:");
                        Serial.print(Tx);
                        Serial.print("  Rx:");
                        Serial.println(Rx);
                        Serial2.begin(baudrate, SERIAL_8N1);
                        Serial.println("Serial2 Loopback Test Sending 64");
                        delay(500);
                        Serial2.write(64);
                        while (!Serial2.available()) { delay(100); };
                        byte chw = Serial2.read();
                        Serial.print("Serial2 Loopback Test Got:0x");
                        Serial.println(chw, HEX);
#endif
                        Serial.println("Serial2.setup");
                        client.print("OK:");
                      }
                    } else {
                      Serial.println("Serial.setup Fail");
                      client.print("Fail");
                    }
                  }
                  break;
                case 0xE1:
                  if(other==1)
                  {
                    while (!Serial1.available()) { delay(100); }
                    ch = Serial1.read();
                    Serial.print("Serial2.readChar:");
                  }
                  else if(other==2)
                  {
                    while (!Serial2.available()) { delay(100); }
                    ch = Serial2.read();
                    Serial.print("Serial2.readChar:");                                       
                  }
                  msgSerial = "OK:";
                  msgSerial.concat((byte)ch);
                  client.print(msgSerial);
                  break;
                  /*               case 0xE2:
                  // Read String Until Timeout
                  while (Comport.available() == 0) {delay(100);} 
                  str = Serial.readString();
                  Serial.print("Serialn.readStringUntilTimeout:");
                  Serial.println(str);
                  msgSerial = "SER:";
                  msgSerial.concat(str);
                  client.print(msgSerial);
                  break;
                case 0xE3:
                  // Read string until
                  while (Serial1.available() == 0) {delay(100);} 
                  str = Comport.readStringUntil((char)param);
                  Serial.print("Serialn.readStringUntil:");
                  Serial.println(str);
                  msgSerial = "SER:";
                  msgSerial.concat(str);
                  client.print(msgSerial);
                  break;   */
                case 0xE4:
                  // Write char
                  ch = (char)param;
                  if (other == 1)
                  {
                    Serial1.write(ch);
                    Serial.println("Serial1.writeChar");
                  }
                  else if(other==2)
                  {
                    Serial2.write(ch);
                    Serial.println("Serial2.writeChar");                   
                  }
                  client.print("OK:");
                  break;
                  /*               case 0xE5:
                  while(!Comport.available()){ delay(100);}
                  fNum  = Comport.parseFloat();
                  Serial.print("Serialn.readFloat:");
                  Serial.println(fNum);
                  str = String(fNum);
                  msgSerial = "FLT:";
                  msgSerial.concat(str);
                  client.print(msgSerial);
                  break; 
                case 0xE6:
                  while(!Comport.available()){ delay(100);}
                  iNum  = Comport.parseInt();
                  Serial.print("Serialn.readInt:");
                  Serial.println(iNum);
                  str = String(iNum);
                  msgSerial = "INT:";
                  msgSerial.concat(str);
                  client.print(msgSerial);
                  break;    */
                case 0xE7:
                  {
                    String msgSerial = "OK:";
                    String line="";
                    if (other == 1)
                    {
                      while(!Serial1.available()){ watchdog_update();delay(100);}
                      line = Serial1.readStringUntil('\n');
                    }
                    else if(other==2)
                    {
                      while(!Serial2.available()){ watchdog_update();delay(100);}
                      line = Serial2.readStringUntil('\n');               
                    }
                    else
                    {
                      client.print("Fail:Serial.Readlin()");
                      break;
                    }
                    msgSerial.concat(line);
                    Serial.println(msgSerial);
                    client.print(msgSerial);
                  }
                  break;
              }
            }
            break;
          case 0xF0: //Grove Sensors
            { 
              // Cmd = 0xF0,pin=Index of cmd in list,param=SubCmd,Other=
              // GroveSensorCmds{
              // s_getpinsCMD, s_getPropertiesCMD, 
              // s_setupdefaultCMD, s_setupCMD, s_readallCMD, s_readCMD, s_getSensorsCMD=255 
              //};
              switch (param)
              {
                case s_getpinsCMD: 
                  {            
                    switch ((GroveSensor)other)
                    {
                      //#define G_SENSORS C(DHT11)C(BME280)C(UltrasonicRanger)
                      case DHT11:
                        {
                          client.print(Grove_DHT11::GetPins());
                        }
                        break;
                      case BME280:
                        {
                          client.print(Grove_BME280::GetPins());
                        }
                        break;
                      case UltrasonicRanger:
                        {
                          client.print(Grove_URangeSensor::GetPins());
                        }
                        break;
                      // Add more here
                      default:
                        String msgFail = "Fail:Not a sensor:";
                        msgFail += other;
                        client.print(msgFail);
                        break;
                    }
                  }
                  break;
                case s_getPropertiesCMD:
                  {
                    switch ((GroveSensor)other)
                    {
                      //#define SENSORS C(DHT11)C(SWITCH)C(SOUND)C(BME280)
                      case DHT11:
                        {
                          client.print(Grove_DHT11::GetListofProperties());
                        }
                        break;
                      case BME280:
                        {
                          client.print(Grove_BME280::GetListofProperties());
                        }
                        break;
                      case UltrasonicRanger:
                        {
                          client.print(Grove_URangeSensor::GetListofProperties());
                        }
                        break;                       
                      // Add more here
                      default:
                      client.print(Grove_URangeSensor::GetListofProperties());
                        String msgFail = "Fail:Not a sensor:";
                        msgFail += other;
                        client.print(msgFail);
                        break;
                    }

                  }
                  break;
                case s_setupdefaultCMD:
                case s_setupCMD:
                  {
                    Grove_Sensor * grove_Sensor;
                    GroveSensor groveSensor;
                    bool _done=false;
                    switch ((GroveSensor)other)
                    {
                      case DHT11:
                        {
                          grove_Sensor  = new Grove_DHT11();
                          _done = true;
                        }
                        break;
                      case BME280:
                        {
                          grove_Sensor  = new Grove_BME280();
                          _done = true;
                        }
                        break;
                      case UltrasonicRanger:
                        {
                          grove_Sensor  = new Grove_URangeSensor(DEFAULT_URANGE_PIN);
                          _done = true;
                        }
                        break;
                      // Add more here
                      default:
                        client.print("Fail:Not a sensor");
                        break;
                    }
                    if(_done)
                    {
                      if(param==s_setupdefaultCMD)
                      {
                        //Default setup
                        if(grove_Sensor->Setup())
                        {
                          int index = AddSensorToList(grove_Sensor);
                          String msgSettings1 = "OK";
                          msgSettings1.concat(':');
                          msgSettings1.concat(index);
                          client.print(msgSettings1);
                        }
                        else
                          client.print("Fail:Setup");
                        }
                      else
                      {
                        // Non-default setup
                        byte settings[1];
                        settings[0] = pin;
                        if(grove_Sensor->Setup(settings,1))
                        {
                          int index = AddSensorToList(grove_Sensor);
                          String msgSettings2 = "OK";
                          msgSettings2.concat(':');
                          msgSettings2.concat(index);
                          client.print(msgSettings2);
                        }
                        else
                          client.print("Fail:Setup");
                      }
                    }
                  }
                  break;
                case s_readallCMD:
                  {
                    int index = other;
                    Grove_Sensor * grove_Sensor = GetSensorFromList(index);
                    int numProps = grove_Sensor->num_properties;
                    double values[MAX_SENSOR_PROPERTIES];
                    if(grove_Sensor->ReadAll(values))
                    {                 
                      String msgGetAll = "OK:";
                      for (int i=0;i< numProps;i++)
                      {
                        msgGetAll.concat(values[i]);
                        if(i!= (numProps-1))
                          msgGetAll.concat(',');
                      }
                      client.print(msgGetAll);
                    }
                    else
                    {
                      client.print("Fail");
                    }
                  }
                  break;
                case s_readCMD:
                  {

                    Grove_Sensor * grove_Sensor = GetSensorFromList(other);
                    // A bit of reuse of real-estate here:
                    byte property = pin;
                    if(property>(grove_Sensor->num_properties-1))
                    {
                      client.print("Fail:Read Property no. > no. properties");
                    }
                    else
                    {
                      double value = grove_Sensor->Read(property);
                      if (value != DBL_MAX)
                      {
                        String msgGetOne = "OK:";
                        msgGetOne.concat(value);
                        client.print(msgGetOne);
                      }
                      else
                      {
                        client.print("Fail:Read");
                      }
                    }
                  }
                  break;
                case s_getTelemetry:
                  {
                    int index = other;
                    Grove_Sensor * grove_Sensor = GetSensorFromList(index);
                    String json = grove_Sensor->GetTelemetry();
                    String ret = "OK:";
                    ret.concat(json);
                    client.print(ret);
                  }
                  break; 
                case s_sendTelemetryBT:
                  {
                    //TelemetryStreamNo
                    int index = other;
                    SensorListNode * node = GetNode(index);
                    String msg;
                    if(node != NULL)
                    {
                      Grove_Sensor * grove_Sensor  = node->Sensor;
                      // Note: Each sensor instance has a private CallbackInfo property
                      CallbackInfo * info = grove_Sensor->GetCallbackInfo();
                      if(info==NULL)
                      {
                        Serial.println("IsNull");
                      }
                      else
                      {

                      }

                      unsigned long period=5000l;
                      byte settings[maxNumDisplaySettings]; 
                      int numSettings=0;
                      if (otherData!= NULL)
                      {
                        for (int i=0; ((i< otherData[0]) &&(i<=maxNumDisplaySettings));i++)
                        {
                          settings[i] = otherData[i+1];
                          numSettings++;
                        }
#ifdef TELEMETRY_DOUBLE_FLASH_INBUILT_LED
                        // Double flash takes about 1 sec of processing, in second core
                        if (settings[0]<2)
                          settings[0] = 2;
#endif 
                        period = settings[0] * 1000l; // Milliseconds
                      }

                      Serial.print("period BT");
                      Serial.println(period);

                      info->period=period;
                      info->isSensor=true;
                      info->sendBT=true;
                      info->SensorIndex = index;
                      int SensorListIndex = AddCallBack(info);
                      node->TelemetryStreamNo = SensorListIndex;
                      msg = String("OK:");
                      msg.concat(SensorListIndex);
                    }
                    else
                    {
                      msg=String("Fail:SendTelemetryBT-Sensor not found.");
                    }
                    client.print(msg);
                    Serial.println(msg);
                  }
                  break; 
                case s_sendTelemetryToIoTHub:
                  {
                    int index = other;
                    SensorListNode * node = GetNode(index);                   
                    String msg;
                    if(node != NULL)
                    {
                      Grove_Sensor * grove_Sensor  = node->Sensor;
                      // Note: Each sensor instance has a private CallbackInfo property
                      CallbackInfo * info = grove_Sensor->GetCallbackInfo();

                      unsigned long period=5000l;
                      byte settings[maxNumDisplaySettings]; // Allow 4 settings for nw.
                      int numSettings=0;
                      if (otherData!= NULL)
                      {
                        for (int i=0; ((i< otherData[0]) &&(i<=maxNumDisplaySettings));i++)
                        {
                          settings[i] = otherData[i+1];
                          numSettings++;
                        }
#ifdef TELEMETRY_DOUBLE_FLASH_INBUILT_LED
                        // Double flash takes about 1 sec of processing in second core
                        if (settings[0]<2)
                          settings[0] = 2;
#endif                       
                        period = settings[0]  * 1000l; // Milliseconds
                      }

                      info->period=period;
                      info->isSensor=true;
                      info->sendBT=false;
                      info->SensorIndex = index;

                      // Lock around Callbacks
                      rp2040.idleOtherCore();
                      int SensorListIndex = AddCallBack(info);
                      node->TelemetryStreamNo = SensorListIndex;
                      rp2040.resumeOtherCore();

                      msg = String("OK:");
                      msg.concat(SensorListIndex);
                    }
                    else
                    {
                      msg=String("Fail:SendTelemetryToIoTHub-Sensor not found");
                    }
                    Serial.println(msg);
                    client.print(msg);
                  } 
                  break; 
                case s_pause_sendTelemetry:
                  {
                    int index = other;
                    SensorListNode * node = GetNode(index);
                    String msg;
                    if(node != NULL)
                    {
                      // Encapsulate the TelemetryStreamNo and oause (ie 0) in one value
                      int num = node->TelemetryStreamNo*SynchMultiplier + pauseTelemetryorBT;
                      rp2040.fifo.push(num);
                      while (!rp2040.fifo.available())
                      {
                          watchdog_update();
                      }
                      uint32_t sync = rp2040.fifo.pop();
                      if(sync == 1)
                      {
                        msg = String("OK:");
                      }
                      else
                      {
                        msg = String("Fail:Pause_sendTelemetry()-Pause error");
                      }
                    }
                    else
                    {
                      msg = String("Fail:Pause_sendTelemetry()-Sensor not found");
                    }
                    Serial.println(msg);
                    client.print(msg);
                  } 
                  break;  
                case s_continue_sendTelemetry:
                  {
                    int index = other;
                    SensorListNode * node = GetNode(index);
                    String msg;
                    if(node != NULL)
                    {
                      // Encapsulate the TelemetryStreamNo and run/continue (ie 1) in one value
                      int num = node->TelemetryStreamNo*1000 + continueTelemetryorBT;
                      Serial.print("s_continue_sendTelemetry num:");
                      Serial.println(num);
                      rp2040.fifo.push(num);
                      while (!rp2040.fifo.available())
                      {
                          watchdog_update();
                      }
                      uint32_t sync = rp2040.fifo.pop();
                      if(sync == 1)
                      {
                        msg = String("OK:");
                      }
                      else
                      {
                        msg = String("Fail:Pause_sendTelemetry()-Pause error");
                      }
                    }
                    else
                    {
                      msg = String("Fail:Pause_sendTelemetry()-Sensor not found");
                    }
                    Serial.println(msg);
                    client.print(msg);
                  } 
                  break; 
                case s_stop_sendTelemetry:
                  {
                    int index = other;
                    SensorListNode * node = GetNode(index);
                    String msg;
                    if(node != NULL)
                    {
                      // Encapsulate the TelemetryStreamNo and oause (ie 0) in one value
                      int num = node->TelemetryStreamNo*SynchMultiplier + stopTelemetryorBT;
                      Serial.print("Stop Telemetry num:");
                      Serial.println(num);
                      rp2040.fifo.push(num);
                      while (!rp2040.fifo.available())
                      {
                          watchdog_update();
                      }
                      uint32_t sync = rp2040.fifo.pop();
                      if(sync == 1)
                      {
                        msg = String("OK:Stop Telemtry");
                      }
                      else
                      {
                        msg = String("Fail:Stop_sendTelemetry()-Pause error");
                      }
                    }
                    else
                    {
                      msg = String("Fail:Pause_sendTelemetry()-Sensor not found");
                    }
                    Serial.println(msg);
                    client.print(msg);
                  }
                  break;        
                case s_getSensorsCMD:
                  {
                    String msg = String("OK:");
                    msg.concat(Grove_Sensor::GetListof());
                    Serial.println(msg);
                    client.print(msg);
                  }
                  break;
              }
            }
            break;
          case 0xF1: //Grove Displays
            {
              //#define G_DISPLAYS C(OLED096)C(LCD1602)C(NEOPIXEL)
              // enum GroveDisplayCmds{
              // d_getpinsCMD, d_tbdCMD, d_setupDefaultCMD, d_setupCMD, 
              // d_clearCMD,d_backlightCND,d_setCursorCMD,homeCMD,d_miscCMD, d_getDisplaysCMD=255 
              // }
              GroveDisplay display = (GroveDisplay)other;
              switch (param)
              {
                case d_getpinsCMD: //getPins
                {
                  switch (display)
                  {
                    case OLED096:
                      client.print(Grove_OLED096::GetPins());
                      break;
                    case LCD1602:
                      client.print(Grove_LCD1602::GetPins());
                      break;
                    case NEOPIXEL:
                      client.print(Adafruit_NeoPixel8::GetPins());
                      break;
                    case BARGRAPH:
                      client.print(Custom_Bargraph::GetPins());
                      break;
                    default:
                      client.print("Fail:Not a display");
                      break;                 }
                }
                  break;
                case d_tbdCMD: //Get list of Msic commands for device
                {
                  Serial.println("Get Display.Miscs.");
                  String msg ="";
                  switch (display)
                  {
                    case OLED096:
                      msg = "OK:Misc:drawCircle,drawFrame,test";
                      break;
                    case LCD1602:
                      msg = "OK:Misc:home,autoscroll,noautoscroll,blink,noblink";
                      break;
                    case NEOPIXEL:
                      msg = "OK:Misc:setpixelcolor,setpixelcolorAll,setpixelcolorOdds,setpixelcolorEvens,setBrightness,setN";
                      break;
                    case BARGRAPH:
                      msg = "OK:Misc:flow,flow2";
                      break;
                    default:
                        msg = "Fail:Not a display";
                        break;
                  }
                  Serial.println(msg);
                  client.print(msg);              
                }
                  break;
                case d_setupDefaultCMD: //Setupdefault
                case d_setupCMD: //Setup(params)
                  {
                    Grove_Display * grove_Display;
                    //GroveDisplay groveDisplay;
                    bool _done=false;
                    switch ((GroveDisplay)other)
                    {
                      case OLED096:
                        {
                          grove_Display  = new Grove_OLED096();
                          _done = true;
                        }
                        break;
                      case LCD1602:
                        {
                          grove_Display  = new Grove_LCD1602();
                          _done = true;
                        }
                        break;
                      case NEOPIXEL:
                        {
                          grove_Display  = new Adafruit_NeoPixel8();
                          _done = true;
                        }
                        break;
                      case BARGRAPH:
                        {
                          grove_Display  = new Custom_Bargraph();
                          _done = true;
                        }
                        break;
                      // Add more here
                      default:
                        client.print("Fail:Not a display");
                        break;
                    }
                    if(_done)
                    {
                      if(param==d_setupDefaultCMD)
                      {
                        if(grove_Display->Setup())
                        {
                          int index = AddDisplayToList(grove_Display);
                          String msgSettingsD1 = "OK";
                          msgSettingsD1.concat(':');
                          msgSettingsD1.concat(index);
                          client.print(msgSettingsD1);
                        }
                        else
                          client.print("Fail:Display.Setup");
                        }
                      else
                      {

                        byte settings[maxNumDisplaySettings]; // Allow 4 settings for nw.
                        settings[0] = pin;
                        int numSettings=1;
                        Serial.print("Settings non default pin:");
                        Serial.println(pin);
                        if (otherData!= NULL)
                        {
                          for (int i=0; ((i< otherData[0]) &&(i<=maxNumDisplaySettings));i++)
                          {
                            settings[i+1] = otherData[i+1];
                            numSettings++;
                          }
                        }
                        if(grove_Display->Setup(settings,numSettings))
                        {
                          int index = AddDisplayToList(grove_Display);
                          String msgSettingsD2 = "OK";
                          msgSettingsD2.concat(':');
                          msgSettingsD2.concat(index);
                          client.print(msgSettingsD2);
                        }
                        else
                          client.print("Fail:Display.Setup");
                      }
                    }
                  }
                  break;
                case d_clearCMD:
                  {
                    int index = other;
                    Grove_Display * grove_Display = GetDisplayFromList(index);
                    if(grove_Display->Clear())
                    {
                      client.print("OK:Clear");
                    }
                    else
                    {
                      client.print("Fail:Clear");
                    }
                  } 
                  break;
                case d_backlightCMD:
                  {
                    int index = other;
                    Grove_Display * grove_Display = GetDisplayFromList(index);
                    if(grove_Display->Backlight())
                    {
                      client.print("OK:Backlight");
                    }
                    else
                    {
                      client.print("Fail;Backlight");
                    }
                  }
                  break;
                case d_setCursorCMD:
                  {
                    int index = other;
                    Grove_Display * grove_Display = GetDisplayFromList(index);
                    if(otherData[0]<2)
                    {
                      client.print("Fail:SetCursor needs (x,y)");
                    }
                    else
                    {
                      byte x = otherData[1];
                      byte y = otherData[2];
                      if(grove_Display->SetCursor(x,y))
                      {
                        client.print("OK:SetCursor");
                      }
                      else
                      {
                        client.print("Fail:SetCursor");
                      }
                    }
                  }
                  break;
                case d_writestrngCMD:
                {
                  int index = other;
                  Grove_Display * grove_Display = GetDisplayFromList(index);
                  if(otherData[0]<0)
                  {
                    client.print("Fail:WriteString needs data");
                  }
                  else
                  {
                    String msgStr =String("");
                    if(otherData[0]>1)
                    {
                      char * msg = (char *) (otherData + 1);
                      msgStr = String(msg);
                    }
                    Serial.print("Message:");
                    Serial.println(msgStr);
                    if(grove_Display->WriteString(msgStr))
                    {
                      client.print("OK:WriteString");
                    }
                    else
                    {
                      client.print("Fail:WriteString");
                    }
                  }
                }
                break;
                case d_cursor_writestringCMD:
                {
                  int index = other;
                  Grove_Display * grove_Display = GetDisplayFromList(index);
                  if(otherData[0]<2)
                  {
                    Serial.print("Fail:SetCursor-WriteString needs (x,y)");
                    client.print("Fail:SetCursor-WriteString needs (x,y)");
                  }
                  else
                  {
                    byte x = otherData[1];
                    byte y = otherData[2];
                    String msgStr =String("");
                    if(otherData[0]>2)
                    {
                      char * msg = (char *) (otherData + 3);
                      msgStr = String(msg);
                    }
                    // Nb: Use blank string if there is an issue.
                    Serial.print("Message:");
                    Serial.println(msgStr);
                    if(!grove_Display->CursorWriteStringAvailable())
                    {
                      if(grove_Display->SetCursor(x,y))
                      {
                        if(grove_Display->WriteString(msgStr))
                        {
                          client.print("OK:SetCursor-then-WriteString");
                        }
                        else
                        {
                          client.print("Fail:SetCursor-then-WriteString-at-WriteString");
                        }
                      }
                      else
                      {
                        client.print("Fail:SetCursor-then-WriteString-at-SetCursor");
                      }
                    }
                    else 
                    {
                      if(grove_Display->WriteString(x,y,msgStr))
                      {
                        client.print("OK:Cursor_WriteString");
                      }
                      else
                      {
                        client.print("Fail:SetCursor-WriteString");
                      }
                    }
                  }              
                }
                break;              
                case d_miscCMD:
                  {
                    Serial.print("d_miscCMD");Serial.print("-");Serial.println(d_miscCMD);
                    if(otherData[0]<1)
                    {
                      client.print("Fail:Display.Misc needs a command)");
                    }
                    else
                    {
                      int index=other;
                      Serial.println("Display->Misc cmd");
                      Grove_Display * grove_Display = GetDisplayFromList(index);
                      if(grove_Display==NULL)
                      {
                        client.print("Fail:Display.Misc() NULL");
                      }
                      else
                      {
                        byte miscCMD = otherData[1];
                        Serial.print("miscCMD:");
                        Serial.println(miscCMD);
                        byte * miscData = NULL;
                        byte miscDataLength = otherData[0]-1;
                        if (miscDataLength>0)
                        {
                          miscData = otherData +2;
                        }
                        Serial.println("miscDataLength:");
                        Serial.println(miscDataLength);
                        if(grove_Display->Misc(miscCMD,miscData,miscDataLength))
                        {
                          Serial.println("Misc OK");
                          client.print("OK:");
                        }
                        else
                        {
                          Serial.println("Misc Fail");
                          client.print("Fail:Display.Misc()");
                        }
                      }
                    }
                  }  
                  break;
                case d_dispose:
                  {
                    int index=other;
                    Grove_Display * grove_Display = GetDisplayFromList(index);
                    RemoveDisplayFromList(index);
                  } 
                  break;                             
                case d_getDisplaysCMD:
                  {
                    Serial.println("Get Displays.");
                    String msg = String("OK:");
                    msg.concat(Grove_Display::GetListof());
                    Serial.println(msg);
                    client.print(msg);
                  }
                  break;
                
              }
            }
            break;
          case 0xF2: //Grove Actuators
            {
              GroveActuator actuator = (GroveActuator)other;
              switch (param)
              {
                case a_getpinsCMD: //getPins
                {
                  switch (actuator)
                  {
                    case SERVO:
                      client.print(Grove_Servo::GetPins());
                      break;
                    default:
                      client.print("Fail:Not a display");
                      break;                 }
                  }
                  break;
                case a_getValueRangeCMD:
                {
                  switch (actuator)
                  {
                    case SERVO:
                      client.print(Grove_Servo::GetValueRange());
                      break;
                    default:
                        client.print("Fail:Not an actuator");
                        break;
                  }
                }
                  break;
                case a_setupDefaultCMD: //Setupdefault
                case a_setupCMD: //Setup(params)
                  {
                    Grove_Actuator * grove_Actuator;
                    //GroveActuator groveActuator;
                    bool _done=false;
                    switch ((GroveActuator)other)
                    {
                      case SERVO:
                        {
                          grove_Actuator  = new Grove_Servo();
                          _done = true;
                        }
                        break;
                      // Add more here
                      default:
                        client.print("Fail:Not an actuator");
                        break;
                    }
                    if(_done)
                    {
                      if(param==d_setupDefaultCMD)
                      {
                        if(grove_Actuator->Setup())
                        {
                          int index = AddActuatorToList(grove_Actuator);
                          String msgSettingsA1 = "OK";
                          msgSettingsA1.concat(':');
                          msgSettingsA1.concat(index);
                          client.print(msgSettingsA1);
                        }
                        else
                          client.print("Fail:Actuator.Setup");
                        }
                      else
                      {
                        byte settings[1];
                        settings[0] = pin;
                        if(grove_Actuator->Setup(settings,1))
                        {
                          int index = AddActuatorToList(grove_Actuator);
                          String msgSettingsA2 = "OK";
                          msgSettingsA2.concat(':');
                          msgSettingsA2.concat(index);
                          client.print(msgSettingsA2);
                        }
                        else
                          client.print("Fail:Actuator.Setup");
                      }
                    }
                  }
                  break;                                       
                case a_writeDoubleValueCMD:
                {
                  int index = other;
                  Grove_Actuator * grove_Actuator = GetActuatorFromList(index);
                  if(otherData[0]<1)
                  {
                    client.print("Fail:Actuator-WriteDoubleValue needs (a value");
                  }
                  else
                  {
                    double value = (double)otherData[1];
                    if(grove_Actuator->Write(value,index))
                      client.print("OK:Actuator-WriteDoubleValue");
                    else
                      client.print("Fail:Actuator-WriteDoubleValue");
                  }
                }
                break;
              case a_writeByteValueCMD:
                {
                  int index = other;
                  Grove_Actuator * grove_Actuator = GetActuatorFromList(index);
                  if(otherData[0]<1)
                  {
                    client.print("Fail:Actuator-WriteIntValue needs (a value");
                  }
                  else
                  {
                    byte value = otherData[1];
                    Serial.print("Servo: ");
                    Serial.print(value);
                    Serial.print('-');
                    Serial.println(index);
                    if(grove_Actuator->Write(value,index))
                      client.print("OK:Actuator-WriteIntValue");
                    else
                      client.print("Fail:Actuator-WriteIntValue");
                  }
                }
                break;            
                case a_getActuatorsCMD:
                  {
                    String msg = String("OK:");
                    msg.concat(Grove_Actuator::GetListof());
                    client.print(msg);
                  }
                  break;
                
              }
            }
            break;        
 
          default:
            Serial.println("Unknown cmd");
            client.print("Unknown cmd");
            break;
        }
        break;
    }
    delay(500);
  }


  client.printf("Done from Pico-W\r\n");
  client.flush();
}

// Core2 code moved from here to:
#include "SoftataCore2.h"






