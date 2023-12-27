#include "grove_environsensors.h"
#include <arduino.h>
#include <dht.h>

#define NUM_PROPERTIES 2
#define NUM_PINS 3
#define DEFAULT_PIN 16
#define PINS 16,18,20
#define pins "16,18,20"





int Pins[] = {PINS};

    String Grove_DHT11::GetPins()
    {
      String _pins = String(pins);
      _pins.concat(" Default:");
      _pins.concat(DEFAULT_PIN);
      Serial.println(_pins);
      return String(_pins); //pins;
    }

    bool Grove_DHT11::Setup(int * settings, int numSettings)
    {
      if(numSettings>0)
      {
        int pin = settings[0];
        return SetupPin(pin);
      }
      else
        return false;
    }

    bool Grove_DHT11::Setup()
    {
      return SetupPin(DEFAULT_PIN);
    }

    bool Grove_DHT11::SetupPin(int pin)
    {
      // Check that the nominated pin is valid
      bool found = false;
      for (int i=0;i<NUM_PINS;i++)
      {
        if(pin == Pins[i])
        {
          found= true;
          break;
        }
      }
      if(!found)
        return false;

      //Set the DHT11 pin
      Pin = pin;
      while(!Serial){delay(100);};
      Serial.println("DHT Grove");
      Serial.print("LIBRARY VERSION: ");
      Serial.println(DHT_LIB_VERSION);
      Serial.println();
      //Serial.println("Type,\tstatus,\tHumidity (%),\tTemperature (C)");
      return true;
    }

    bool Grove_DHT11::ReadAll(double * values)
    {
      dht DHT;
      // READ DATA
      Serial.print("DHT11 ");
      int chk = DHT.read11(Pin);
      switch (chk)
      {
        case DHTLIB_OK:  
        Serial.print("OK,"); 
        break;
        case DHTLIB_ERROR_CHECKSUM: 
        Serial.println("Checksum error"); 
        return false;
        break;
        case DHTLIB_ERROR_TIMEOUT: 
        Serial.println("Time out error"); 
        return false;
        break;
        default: 
        Serial.println("Unknown error"); 
        return false;
        break;
      }
      // DISPLAY DATA
      values[0] = DHT.temperature;
      values[1]= DHT.humidity;
      return true;
    }

    double Grove_DHT11::Read(int property)
    {
      dht DHT;
      // READ DATA
      Serial.print("DHT11");
      int chk = DHT.read11(Pin);
      switch (chk)
      {
        case DHTLIB_OK:  
        Serial.print("OK,"); 
        break;
        case DHTLIB_ERROR_CHECKSUM: 
        Serial.println("Checksum error"); 
        return ERRORDBL;
        break;
        case DHTLIB_ERROR_TIMEOUT: 
        Serial.println("Time out error"); 
        return ERRORDBL;
        break;
        default: 
        Serial.println("Unknown error"); 
        return ERRORDBL;
        break;
      }
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


