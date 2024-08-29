#include "grove_environsensors.h"
#include <arduino.h>
#include <dht.h>
#include "../Softata.h"


#ifdef GROVE_RPI_PICO_SHIELD
#define NUM_PINS 3
#define DEFAULT_PIN DHT11Pin_D
#define PINS 16,18,20
#define pins "16,18,20"
int Pins[] = {PINS};
#elif defined(RPI_PICO_DEFAULT)
#define DEFAULT_PIN DHT11Pin_D
#define PIN_START 0
#define PIN_END 26
#define pins "Any of GPIO 0 to 26"
#endif


    



    Grove_DHT11::Grove_DHT11()
    {
      Grove_Sensor::num_properties = NUM_DHT11_PROPERTIES;
      Grove_Sensor::sensorType = DHT11;
    }

    Grove_DHT11::Grove_DHT11(int pin)
    {
      Pin = pin;
      SetupPin(Pin);
      Grove_Sensor::num_properties = NUM_DHT11_PROPERTIES;
      Grove_Sensor::sensorType = DHT11;
    }

    /*String Grove_DHT11::GetPins()
    {
      String _pins = String("OK:Grove RPi Pico Shield: ");
      _pins.concat(pins);
      _pins.concat(" Default:");
      _pins.concat(DEFAULT_PIN);
      Serial_println(_pins);
      return String(_pins); //pins;
    }*/


    bool Grove_DHT11::SetupPin(int pin)
    {
      // Check that the nominated pin is valid
      bool found = false;
#ifdef GROVE_RPI_PICO_SHIELD
      for (int i=0;i<NUM_PINS;i++)
      {
        if(pin == Pins[i])
        {
          found= true;
          break;
        }
      }
#elif defined(RPI_PICO_DEFAULT)
      if ((pin>=PIN_START) && (pin<=PIN_END))
        found = true;
#endif
      if(!found)
        return false;

      //Set the DHT11 pin
      Pin = pin;
      whileNotSerial(){delay(100);};
      Serial_println("DHT Grove");
      Serial_print("LIBRARY VERSION: ");
      Serial_println(DHT_LIB_VERSION);
      Serial_println();
      //Serial_println("Type,\tstatus,\tHumidity (%),\tTemperature (C)");
      return true;
    }

  

    bool Grove_DHT11::Setup(byte * settings, byte numSettings)
    {
      if(numSettings>0)
      {
        Pin = settings[0];
        return SetupPin(Pin);
      }
      else
        return false;
    }
 
    bool Grove_DHT11::Setup()
    {
      Pin = DEFAULT_PIN;
      return SetupPin(Pin);
      num_properties = 2;
    }



    bool Grove_DHT11::ReadAll(double * values)
    {
      dht DHT;
      // READ DATA
      Serial_print("DHT11:");
      // Allow 3 retries
      bool OK = false;
      for (int i=0;i<4;i++)
      {
        int chk = DHT.read11(Pin);
        switch (chk)
        {
          case DHTLIB_OK:  
          Serial_println("OK"); 
          OK = true;
          break;
          case DHTLIB_ERROR_CHECKSUM: 
          Serial_println("Checksum error"); 
          break;
          case DHTLIB_ERROR_TIMEOUT: 
          Serial_println("Time out error"); 
          break;
          default: 
          Serial_println("Unknown error"); 
          break;
        }
        if(OK)
          break;
        delay(100);
        if(OK)
          break;
      }
      if(!OK)
        return false;
      // DISPLAY DATA
      values[0] = DHT.temperature;
      values[1] = DHT.humidity;
      return true;
    }

    String Grove_DHT11::GetTelemetry()
    {
      double values[2];
      if(ReadAll(values))
      {
        String msg ="{\"temperature\":";
        msg.concat(values[0]);
        msg.concat(',');
        msg.concat("\"humidity\":");
        msg.concat(values[1]);
        msg.concat('}');
        return msg;
      }
      else
        return "ERRORDBL";
}

CallbackInfo * Grove_DHT11::GetCallbackInfo()
{
    info.period = 314159;
    info.next = 137;
    info.SensorIndex = 123456;
    info.sendBT = false;
    info.isSensor=false;
    info.isRunning=false;
    return &info;
}

    double Grove_DHT11::Read(int property)
    {
      dht DHT;
      // READ DATA
      Serial_print("DHT11:");
      
      // Allow 3 retries
      bool OK = false;
      for (int i=0;i<4;i++)
      {
        int chk = DHT.read11(Pin);
        switch (chk)
        {
          case DHTLIB_OK:  
          Serial_println("OK,"); 
          OK = true;
          break;
          case DHTLIB_ERROR_CHECKSUM: 
          Serial_println("Checksum error"); 
          break;
          case DHTLIB_ERROR_TIMEOUT: 
          Serial_println("Time out error"); 
          break;
          default: 
          Serial_println("Unknown error"); 
          break;
        }
        if(OK)
          break;
        delay(100);
      }
      if(!OK)
        return ERRORDBL;
      // DISPLAY DATA
      switch(property)
      {
        case 0:
          return DHT.temperature;
          break;
        case 1: 
          return DHT.humidity;
          break; 
        default:
          return ERRORDBL;
          break;
      }
    }


