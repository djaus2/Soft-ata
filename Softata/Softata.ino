// Placed in the public domain by Earle F. Philhower, III, 2022
#include "rpiboards.h"
#include <WiFi.h>
#include "rpiwatchdog.h"

#ifndef STASSID
#define STASSID "APQLZM"
#define STAPSK "silly1371"
#endif

// Use one or other in testing
// Only used when Serial1/2 Setup is called from client.
// Commment out both normally.
//#define SERIAL1LOOPBACK
//#define SERIAL2LOOPBACK

const char* ssid = STASSID;
const char* password = STAPSK;

#define BUFFSIZE 128
byte Buffer[BUFFSIZE];

int port = 4242;

WiFiServer server(port);

void setup() {
  Serial.begin(115200);
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
  watchdog_enable(WATCHDOG_SECS*1000, false);
}

//Ref: https://www.thegeekpub.com/276838/how-to-reset-an-arduino-using-code/
void(* resetFunc) (void) = 0;

void loop() {
  watchdog_update();
  static int i;
  delay(100);
  //Serial.printf("--loop %d\n", ++i);
  //delay(10);
  WiFiClient client = server.available();
  if (!client) {
    return;
  }
  Serial.println("WiFi Up");
  while(true)
  {
    watchdog_update();
    Serial.print("Get next command.");
    while (!client.available()) {
      delay(100);
      watchdog_update();
    }

    Serial.println("...Is connected.");
    byte length = client.read();
    Serial.println(length);
    int count=0;
    byte msg[10];
    if(length == 0)
    {
      Serial.println("Null msg.");
      return;
    }
    while (client.available() && (count!=length))
    {
      msg[count] = client.read();
      Serial.print(msg[count],HEX);
      watchdog_update();
      count++;
    }
    Serial.println();
    if(count != length)
    {
      Serial.println("Msg invalid.");
      return;
    }
    switch(msg[0])
    {
      // Escape simple string commands here. 
      // These commands sto start in uppercase
      // Need first letter of these ASCII values to not match commands
      // ie Softata commands not to be between 65 to 90, 0x41 to 0x5A
      case (byte)'B':  // Begin
        Serial.println("Ready.");
        client.print("Ready"); // Sent at first connection.
        break;
      case (byte)'E': //End
        Serial.println("Done.");
        client.print("Done"); 
        // Force reset
        resetFunc();
        break;
      case (byte)'N': //Null
        Serial.println("Null.");
        client.print("OK"); 
        return;
        break;
      case (byte)'R': //Reset
        Serial.println("Resetting.");
        client.print("Reset"); 
        // Force reset
        resetFunc();
      default:
        // Get Softata command and parameters
        byte cmd = msg[0];
        byte pin = 0xff;
        byte param = 0xff;
        byte other = 0xff;
        if(length>1)
        {
          pin=msg[1];
          if(length>2)
          {
            param=msg[2];
            if(length>3)
            {
              other=msg[3];
        } } }

        //Print command and paramaters
        String str;
        byte vaue;
        Serial.print("cmd:");
        Serial.print(cmd);Serial.print(' ');
        if (pin!=0xff)
        {
          Serial.print("pin:");
          Serial.print(pin,HEX);Serial.print(' ');
          if (param!=0xff)
          {
            Serial.print("param:");
            Serial.print(param);Serial.print(' ');
            if (other!= 0xff)
            {
              Serial.print("other:");
              Serial.print(other);
        } } }
        Serial.println();

        // Action cmds
        int value;
        switch(cmd)
        {
          case 0xD0:
          case 0xD1:
          case 0xD2:
          case 0xD3:
          { 
            // Digital
            if(!IS_PIN_DIGITAL(pin))
            {
              Serial.print("Pin is not digital");
              client.print("FAIL");
              continue;
            }
            switch (cmd)
            {
              case 0xD0:
                Serial.print("pinMode:");
                Serial.println(param);
                pinMode(pin, (PinMode)param);
                client.print("OK");
                break;
              case 0xD2:
                Serial.println("digitalRead");
                value = digitalRead(pin);
                if(value)
                  client.print("ON");
                else
                  client.print("OFF");
                break;
              case 0xD1:
                Serial.print("digitalWrite:");
                Serial.println(param);
                if(param)
                  digitalWrite(pin, HIGH); 
                else
                  digitalWrite(pin, LOW);
                client.print("OK");
                break; 
              case 0xD3:
                Serial.println("digitalToggle");
                value = digitalRead(pin);
                if(value)
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
          case 0xA2: // Analog place holder
            if(!IS_PIN_ANALOG(pin))
            {
              Serial.print("Pin not Analog");
              client.print("FAIL");
              continue;
            }
            else
            {
              if(cmd==0xA2)
              {
                  Serial.print("analogRead:");
                  value = analogRead(pin);
                  String valueADStr = String(value);
                  String msgAD = "AD:";
                  msgAD.concat(valueADStr);
                  Serial.println(valueADStr);
                  client.print(msgAD);
                  //break;
              }
            }
            break;
          case 0xB1:
            if(!IS_PIN_PWM(pin))
            {
              Serial.print("Pin not PWM");
              client.print("FAIL");
              continue;
            }
            if (cmd==0xB1)
            {
                Serial.print("PWM");
                analogWrite(pin,param);
                Serial.println("PWM:analogWrite()");
                client.print("OK");
            }
            break;
          case 0xC0:
          case 0xC1:
          case 0xC2: // Servo place holder
            if(!IS_PIN_SERVO(pin))
            {
              Serial.print("Pin not Servo");
              client.print("FAIL");
              continue;
            }
            Serial.println("OK-SERVO 2D cmds");
            client.print("OK-SERVO 2D cmds"); 
            break;
          case 0xE0: // Setup Serial1/2
          case 0xE1: // Get a char
          //case 0xE2: // Get a string
          //case 0xE3: // Get a string until char
          case 0xE4: // Write a char
          //case 0xE5: // Get Flost
          //case 0xE6: // Get Int
            if(!((other==1)||(other==2)))
            {
              //Nb: For serial except for setup, pin parameter is ignored
              // But need to specify Serial1 or Serial2 as 1 or 2 in other parameter
              Serial.print("Not Serial 1 or 2 (other)");
              client.print("FAIL");
              continue;
            }
            else
            {
              Stream &Comport = Serial1;
              if(other==2)
              {
                Serial.println("Serial2");
                Comport = Serial2;
              }
              else
              {
                Serial.println("Serial1");
              }

              char ch;
              String str;
              String msgSerial;
              float fNum;
              int iNum;
              Serial.println("#291");
              switch(cmd)
              {
                case 0xE0: // Set Pins (Provide Tx, Determine Rx) and set Baudrate from list
                  {
                    if(IS_PIN_SERIAL_TX(pin))
                    {
                      byte Tx = pin;
                      byte Rx = pin + 1;
                      int baudrate = Baudrates[param];
                      if(other==1)
                      {
                        Serial1.setTX(Tx);
                        Serial1.setRX(Rx);
                        //Serial1.SetRTS()
                        //Serial1.setCTS();
                        Serial1.begin(baudrate,SERIAL_8N1);
#ifdef SERIAL1LOOPBACK
                        Serial.println("Serial1 Loopback");
                        Serial.print("Tx:");
                        Serial.print(Tx);
                        Serial.print("  Rx:");
                        Serial.println(Rx);
                        Serial.println("Serial1 Loopback Test Sending 64");
                        delay(500);
                        Serial2.write(64);
                        while(!Serial1.available()){delay(100);};
                        byte chw = Serial1.read();
                        Serial.print("Serial1 Loopback Test Got:0x");
                        Serial.println(chw,HEX);
#endif
                        Serial.println("Serial1.setup");
                        client.print("OK");
                      }
                      else if(other==2)
                      {
                        Serial2.setTX(Tx);
                        Serial2.setRX(Rx);
                        Serial2.begin(baudrate,SERIAL_8N1);
#ifdef SERIAL2LOOPBACK
                        Serial.println("Serial2 Loopback");
                        Serial.print("Tx:");
                        Serial.print(Tx);
                        Serial.print("  Rx:");
                        Serial.println(Rx);
                        Serial2.begin(baudrate,SERIAL_8N1);
                        Serial.println("Serial2 Loopback Test Sending 64");
                        delay(500);
                        Serial2.write(64);
                        while(!Serial2.available()){delay(100);};
                        byte chw = Serial2.read();
                        Serial.print("Serial2 Loopback Test Got:0x");
                        Serial.println(chw,HEX);
#endif
                        Serial.println("Serial2.setup");
                        client.print("OK");
                      }
                    }
                    else
                    {
                      Serial.println("Serial.setup Fail");
                      client.print("Fail");
                    }
                  }
                  break;
                case 0xE1:
                  Serial.println("#357");
                  while(!Comport.available()){ delay(100);}
                  //delay(500);
                  Serial.println("#359");
                  ch = Comport.read();
                  Serial.println("#361");
                  Serial.print("Serialn.readChar:");
                  Serial.println(((byte)ch),HEX);
                  msgSerial = "SER";
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
                  ch = (char) param;
                  Comport.write(ch);
                  Serial.println("Serialn.writeChar");
                  client.print("OK");
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
              }
            }
            break;
          case 0xF0:
          case 0xF1:
          case 0xF2: // I2C place holder
            if(!IS_PIN_I2C(pin))
            {
              Serial.print("Pin not I2C");
              client.print("FAIL");
              continue;
            }
            Serial.println("OK-I2C 2D cmds");
            client.print("OK-I2C 2D cmds"); 
            break;
          case 0xF3:
          case 0xF4:
          case 0xF5: // SPI place holder
            if(!IS_PIN_SPI(pin))
            {
              Serial.print("Pin not SPI");
              client.print("FAIL");
              continue;
            }
            Serial.println("OK-SPI 2D cmds");
            client.print("OK-SPI 2D cmds"); 
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
