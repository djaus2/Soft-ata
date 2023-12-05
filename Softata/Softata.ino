// Placed in the public domain by Earle F. Philhower, III, 2022
#include "rpiboards.h"
#include <WiFi.h>
#include "rpiwatchdog.h"

#ifndef STASSID
#define STASSID "APQLZM"
#define STAPSK "silly1371"
#endif

const char* ssid = STASSID;
const char* password = STAPSK;

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
    while (client.available()) {
      watchdog_update();
      String req = client.readStringUntil('\n');
      if(req.length()==0)
      {
        Serial.println("Null cmd.");
        return;
      }
      switch(req[0])
      {
        // Escape simple string commands here. 
        // These commands sto start in uppercase
        // Need first letter of these ASCII values to not match commands
        // ie Softata commands not to be between 65 to 90, 0x41 to 0x5A
        case 'B':  // Begin
          Serial.println("Ready.");
          client.print("Ready"); // Sent at first connection.
          break;
        case 'E': //End
          Serial.println("Done.");
          client.print("Done"); 
          // Force reset
          resetFunc();
          break;
        case 'N': //Null
          Serial.println("Null.");
          client.print("OK"); 
          return;
          break;
        case 'R': //Reset
          Serial.println("Resetting.");
          client.print("Reset"); 
          // Force reset
          resetFunc();
        default:
          // Get Softata command and parameters
          byte cmd = req[0];
          byte pin = 0xff;
          byte param = 0xff;
          byte other = 0xff;
          if(req.length()>1)
          {
            pin=req[1];
            if(req.length()>2)
            {
              param=req[2];
              if(req.length()>3)
              {
                other=req[3];
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
            case 0xE2: // Get a string
            case 0xE3: // Write a char
            case 0xE4: // Get float
              if(!IS_PIN_SERIAL(pin))
              {
                Serial.print("Pin not Serial");
                client.print("FAIL");
                continue;
              }
              else
              {
                char ch;
                String str;
                String msgSerial;
                float num;
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
                          Serial.println("Serial1.setup");
                          client.print("OK");
                        }
                        else if(other==2)
                        {
                          Serial2.setTX(Tx);
                          Serial2.setRX(Rx);
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
                    if(other==1)
                    {
                      while(!Serial1.available()){ delay(100);}
                      ch = Serial1.read();
                      Serial.print("Serial1.readChar:");
                      Serial.println(ch);
                      msgSerial = "SER1:";
                      msgSerial.concat(ch);
                      client.print(msgSerial);
                    }
                    else if(other==2)
                    {
                      while(!Serial2.available()){ delay(100);}
                      ch = Serial2.read();
                      Serial.print("Serial2.readChar:");
                      Serial.println(ch);
                      msgSerial = "SER2:";
                      msgSerial.concat(ch);
                      client.print(msgSerial);
                    }
                    break;
                  case 0xE2:
                    if(other==1)
                    {
                      while (Serial1.available() == 0) {delay(100);} 
                      str = Serial.readString();
                      Serial.print("Serial2.readString:");
                      Serial.println(str);
                      msgSerial = "SER1:";
                      msgSerial.concat(str);
                      client.print(msgSerial);
                    }
                    else  if(other==2)
                    {
                      while (Serial2.available() == 0) {delay(100);} 
                      str = Serial.readString();
                      Serial.print("Serial2.readString:");
                      Serial.println(str);
                      msgSerial = "SER2:";
                      msgSerial.concat(str);
                      client.print(msgSerial);
                    }
                    break;
                  case 0xE3:
                    if(other==1)
                    {
                      ch = (char) param;
                      Serial1.write(ch);
                      Serial.println("Serial1.write");
                      client.print("OK");
                    }
                    else if(other==2)
                    {
                      ch = (char) param;
                      Serial2.write(ch);
                      Serial.println("Serial2.write");
                      client.print("OK");
                    }
                    break;
                  case 0xE4:
                    if(other==1)
                    {
                      while(!Serial1.available()){ delay(100);}
                      num  = Serial1.parseFloat();
                      Serial.print("Serial1.readFloat:");
                      Serial.println(num);
                      str = String(num);
                      msgSerial = "FLT1:";
                      msgSerial.concat(str);
                      client.print(msgSerial);
                    }
                    else if(other==2)
                    {
                      while(!Serial2.available()){ delay(100);}
                      num  = Serial2.parseFloat();
                      Serial.print("Serial2.readFloat:");
                      Serial.println(num);
                      str = String(num);
                      msgSerial = "FLT2:";
                      msgSerial.concat(str);
                      client.print(msgSerial);
                    }
                    break;                                  
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
  }

  client.printf("Done from Pico-W\r\n");
  client.flush();
}
