/////////////////////////////
// Code run in second Core //
/////////////////////////////

//enum SyncedCommands : byte {pauseTelemetryorBT=0,continueTelemetryorBT=1,stopTelemetryorBT=2,svrConnected=10, initialSynch=137};

#include <stdio.h>
#include <stdlib.h>
#include <SerialBT.h>
#include "iothub.h"

#include "serial_macros.h"

bool Led_State = false;
bool doingIoTHub = false;


String call_callback_func(String(*call_back)(void))
{
    return call_back();
}

int numSensors =0;
int AddCallBack(CallbackInfo * info)
{
  info->isRunning = true;
  info->next = millis() + info->period;
  int LEDListIndex=AddSensorToCore2List(info);
  numSensors = LEDListIndex +1;
  // Turn on mqtt loop if IoT Hub Sends
  if((info->isSensor)&&(!info->sendBT))
  {
    doingIoTHub=true;
  }
  return LEDListIndex;
}

bool DoActuator(int index, GroveActuatorCmds cmd )
{
  Grove_Actuator * grove_Actuator = GetActuatorFromList(index);
  if(grove_Actuator != NULL)
  {
    Serial_print("Actuator: ");
    Serial_print('-');
    Serial_print(index);
    switch(cmd)
    {
      case a_SetBitCMD:
        if(grove_Actuator->SetBit())
          /*client*/Serial.print("OK:Actuator-SetBit");
        else
          /*client*/Serial.print("Fail:Actuator-SetBit");
        break;                      
      case a_ClearBitCMD:
          if(grove_Actuator->ClearBit())
          /*client*/Serial.print("OK:Actuator-ClearBit");
        else
          /*client*/Serial.print("Fail:Actuator-ClearBit");
        break;                     
      case a_ToggleBitCMD:
        if(grove_Actuator->ToggleBit())
          /*client*/Serial.print("OK:Actuator-ToggleBit");
        else
          /*client*/Serial.print("Fail:Actuator-ToggleBit");
        break;
    }
  } 
  else
  {
    return false;
  } 
  return true;                                                                   

}

bool StopTelemetrySend(int index)
{
  return false;
}

bool ToggleActuator(int index)
{
  return DoActuator(index,a_ToggleBitCMD);
}

bool SetActuator(int index)
{
  return DoActuator(index,a_SetBitCMD);
}

bool ResetActuator(int index)
{
  return DoActuator(index,a_ClearBitCMD);
}

bool PauseTelemetrySend(int index)
{
  CallbackInfo * info = GetCallbackInfoFromCore2List(index);
  if (info!= NULL)
  {
    Serial_print("\t\t==== PauseTelemetrySend:");
    Serial_print(index);
    Serial_println(" ====");
    info->isRunning = false;
    return true;
  }
  else
    return false;
}

bool ContinueTelemetrySend(int index)
{
  CallbackInfo * info = GetCallbackInfoFromCore2List(index);
  if (info!= NULL)
  {
    Serial_println("\t\t==== ContinueTelemetrySend:");
    Serial_print(index);
    Serial_println(" ====");
    info->isRunning = true;
    return true;
  }
  else
    return false;
}

String Toggle_InbuiltLED()
{
    Led_State = !Led_State;
    digitalWrite(LED_BUILTIN, (PinStatus)Led_State);
    if(Led_State)
      return "ON";
    else
      return "OFF";
}

struct CallbackInfo InbuiltLED;

int LEDListIndex = -1;


void setup1() {
  numSensors=0;

  // Wait for WiFi to be started to do Setup1
  uint32_t sync = rp2040.fifo.pop();
  rp2040.fifo.push(sync);
  // Should now be running. Check anyway.
  whileNotSerial();
  while (WiFi.status() != WL_CONNECTED) {
    Serial_print(".");
    delay(250);
  }
  Serial_println();
  

  /////////////////////////////////////////////////
  // Defined in Softata.h
  // Perhaps make software setable??
  // App starts quicker if not defined
  #ifdef USINGIOTHUB
    doingIoTHub = true;
    Serial_println("\t\t==== USING IoT Hub ====");
  #else
    doingIoTHub = false;
    Serial_println("\t\t==== NOT using IoT Hub ====");
  #endif
  ////////////////////////////////////////////////


  Serial_println("\t\t==== 2nd Core Started ====");


  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, LOW);
  Led_State = false;

  InitCore2SensorList();
  InbuiltLED.period = UNCONNECTED_BLINK_ON_PERIOD;
  InbuiltLED.isRunning = true;
  InbuiltLED.isSensor = false;
  InbuiltLED.back = Toggle_InbuiltLED;
  int LEDListIndex = AddCallBack(&InbuiltLED);


  // Wait for other Setup to finish
  uint32_t sync2 = rp2040.fifo.pop();

  if(doingIoTHub)
  {
    establishConnection();
  }
  Serial_println();
  Serial_println("\t\t==== 2nd Core Ready ====");
  rp2040.fifo.push(sync2);
}

/*
rp2040.fifo:
enum SyncedCommands : byte {pauseTelemetryorBT=0,continueTelemetryorBT=1,stopTelemetryorBT=2,svrConnected=10, initialSynch=137};
START:
- At start push 137 [initialSynch] from Setup() (near end) in Core1 and wait
- Core2 in Setup1() waits for that before establishing connection to MQTT if required
- Core2 pushes it back and completes Setup1().
- Core1 completes Setup()

Synched commands from Core1 to Core2
- svrConnected: Service in Core1 has been connected:
  - Cmd passed is 137
  - Remove LED from list
  - Init Sensor list
  - Set blink rate 1/4 of long rate
  - Add LED back into list
  - In built LED now flashes quicker to indicate connected
- pauseTelemetryorBT: Pause indexed Telemetry or BT
  - Cmd passed 0
- continueTelemetryorBT: Continue indexed Telemetry of BT
  - Cmd passed 1
- stopTelemetryorBT: Stop indexed Telemetry or BT 
  - Cmd passed 2

*/
void loop1() 
{
  if (rp2040.fifo.available())
  {
    uint32_t val = rp2040.fifo.pop();
    int index = val / SynchMultiplier;
    SyncedCommands cmd = (SyncedCommands)(val % SynchMultiplier);
    bool res = false;
    if(cmd==svrConnected)
    {
      // Double LED blink speed if connected
      // should be done before any sensors etc added
      RemoveSensorFromCore2List(LEDListIndex); //Ignore index
      InitCore2SensorList();
      InbuiltLED.period = InbuiltLED.period /4;
      LEDListIndex = AddCallBack(&InbuiltLED);
      res = true;
    }
    else if (cmd == pauseTelemetryorBT)
      res =  PauseTelemetrySend(index);
    else if(cmd==continueTelemetryorBT)
      res =  ContinueTelemetrySend(index);
    else if(cmd==stopTelemetryorBT)
    {
      Serial_println("\t\t==== Stopping Telemetry =====");
      Grove_Sensor * grove_Sensor = GetSensorFromList(index);
      delete grove_Sensor;
      RemoveSensorFromCore2List(index);
      res = true;
    }

    if(res)
      rp2040.fifo.push(1);
    else
      rp2040.fifo.push(0);
  }
  for (int i=0; i< numSensors;i++)
  {
    CallbackInfo * info = GetCallbackInfoFromCore2List(i);
    if (info == NULL)
      continue;

    unsigned long currentTime = millis();             
    if ( currentTime > info->next )
    {
      info->next = millis() + info->period;
      if(!info->isRunning)
      {
        Serial_println("\t\t==== Telemetry Not running ====");
        continue;
      }
      if(!info->isSensor)
      {
        // Toggle the inbuilt LED
        String res = call_callback_func(info->back);
        if(!(res == String("")))
        {
          //SerialBT.println(res);
        }
      }
      else if (info->sendBT)
      {
        if(!SerialBT)
        {
          Serial_println("\t\t==== Starting SerialBT in 2nd Core ====");
          SerialBT.begin();
          while (!SerialBT);// Perhaps a timeout??
          Serial_println("\t\t==== SerialBT Started in 2nd Core ====");
        }
        if (SerialBT) 
        {      
          int index = info->SensorIndex;
          Grove_Sensor * grove_Sensor = GetSensorFromList(index);
          String res = grove_Sensor->GetTelemetry();
          if(!(res == String("")))
          {
            //Fwd json
            SerialBT.println(res);
            Serial_println(res);
#ifdef TELEMETRY_DOUBLE_FLASH_INBUILT_LED
            // 1s delay with double flash with transmit
            delay(100);
            digitalWrite(LED_BUILTIN, LOW);
            delay(200);
            digitalWrite(LED_BUILTIN, HIGH);
            delay(200);
            digitalWrite(LED_BUILTIN, LOW);
            delay(200);
            digitalWrite(LED_BUILTIN, HIGH);
            delay(200);
            digitalWrite(LED_BUILTIN, LOW);
#endif
            delay(100);
          }
          
        }
      }
      else if (!info->sendBT) 
      {
        int index = info->SensorIndex;
        Grove_Sensor * grove_Sensor = GetSensorFromList(index);
        String res = grove_Sensor->GetTelemetry();
        if(!(res == String("")))
        {
          if (!mqtt_client.connected())
          {
            establishConnection();
          }
          // Send Telemetry to IoT Hub
          sendTelemetry(res);
  #ifdef TELEMETRY_DOUBLE_FLASH_INBUILT_LED
        // 1s delay with double flash with transmit
        delay(100);
        digitalWrite(LED_BUILTIN, LOW);
        delay(200);
        digitalWrite(LED_BUILTIN, HIGH);
        delay(200);
        digitalWrite(LED_BUILTIN, LOW);
        delay(200);
        digitalWrite(LED_BUILTIN, HIGH);
        delay(200);
        digitalWrite(LED_BUILTIN, LOW);
#endif
        delay(100);
        }
      }
    }
    if(doingIoTHub)
    {
       // MQTT loop must be called to process Device-to-Cloud and Cloud-to-Device.
      mqtt_client.loop();
    }
  }
}