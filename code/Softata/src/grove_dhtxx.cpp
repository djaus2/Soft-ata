#include "grove_environsensors.h"
#include <arduino.h>
#include <dhtnew.h>
#include "../Softata.h"


#ifdef GROVE_RPI_PICO_SHIELD
#define XNUM_PINS 3
#define XDEFAULT_PIN DHT11Pin_D
#define XPINS 16,18,20
#define Xpins "16,18,20"
int XPins[] = {XPINS};
#elif defined(RPI_PICO_DEFAULT)
#define DEFAULT_PIN DHT11Pin_D
#define PIN_START 0
#define PIN_END 26
#define pins "Any of GPIO 0 to 26"
#endif


    
   DHTNEW * mySensor = NULL;
   static int minTimeBtwReads_ms = DHTXX_MIN_TIME_BTW_READS_MS;

    Grove_DHTXX::Grove_DHTXX()
    {
      Grove_Sensor::num_properties = NUM_DHT11_PROPERTIES;
      Grove_Sensor::sensorType = DHTXX;
    }

    Grove_DHTXX::Grove_DHTXX(int pin)
    {
      Pin = pin;
      SetupPin(Pin);
      Grove_Sensor::num_properties = NUM_DHT11_PROPERTIES;
      Grove_Sensor::sensorType = DHTXX;
    }

    /*String Grove_DHTXX::GetPins()
    {
      String _pins = String("OK:Grove RPi Pico Shield: ");
      _pins.concat(Xpins);
      _pins.concat(" Default:");
      _pins.concat(DEFAULT_PIN);
      Serial_println(_pins);
      return String(_pins); //pins;
    }*/


    bool Grove_DHTXX::SetupPin(int pin)
    {
      // Check that the nominated pin is valid
      bool found = false;
#ifdef GROVE_RPI_PICO_SHIELD
      for (int i=0;i<XNUM_PINS;i++)
      {
        if(pin == XPins[i])
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
      mySensor = new DHTNEW(Pin);
      whileNotSerial(){delay(100);};
      Serial_print("DHT");
      Serial_print(mySensor->getType());
      Serial_println(" Grove");
      Serial_print("LIBRARY VERSION: ");
      Serial_println(DHTNEW_LIB_VERSION);
      Serial_println();
      //Serial_println("Type,\tstatus,\tHumidity (%),\tTemperature (C)");
      return true;
    }

  

    bool Grove_DHTXX::Setup(byte * settings, byte numSettings)
    {
      if(numSettings>0)
      {
        Pin = settings[0];
        return SetupPin(Pin);
      }
      else
        return false;
    }
 
    bool Grove_DHTXX::Setup()
    {
      Pin = XDEFAULT_PIN;
      return SetupPin(Pin);
      num_properties = 2;
    }



    bool Grove_DHTXX::ReadAll(double * values)
    {
      // READ DATA
      Serial_print("DHT");
      Serial_println(mySensor->getType());
      // Allow 3 retries
      bool OK = false;
      // Don't reread too soon
      if((millis() - mySensor->lastRead()) >= minTimeBtwReads_ms)
      {
        for (int i=0;i<10;i++)
        {
          // Wait
          while((millis() - mySensor->lastRead()) < minTimeBtwReads_ms)
            delay(50);
          Serial_println("Reading DHTXX");
          int chk = mySensor->read();
          Serial_print("Returned: ");
          Serial.println(chk);
          switch (chk)
          {
            case DHTLIB_OK:
              Serial_println("OK,\t");
              OK=true;
              break;
            case DHTLIB_ERROR_CHECKSUM:
              Serial_print("Checksum error,\t");
              break;
            case DHTLIB_ERROR_TIMEOUT_A:
              Serial_print("Time out A error,\t");
              break;
            case DHTLIB_ERROR_TIMEOUT_B:
              Serial_print("Time out B error,\t");
              break;
            case DHTLIB_ERROR_TIMEOUT_C:
              Serial_print("Time out C error,\t");
              break;
            case DHTLIB_ERROR_TIMEOUT_D:
              Serial_print("Time out D error,\t");
              break;
            case DHTLIB_ERROR_SENSOR_NOT_READY:
              Serial_print("Sensor not ready,\t");
              break;
            case DHTLIB_ERROR_BIT_SHIFT:
              Serial_print("Bit shift error,\t");
              break;
            case DHTLIB_WAITING_FOR_READ:
              Serial_print("Waiting for read,\t");
              break;
            default:
              Serial_print("Unknown: ");
              Serial_print(chk);
              Serial_print(",\t");
              break;
          }
          if(OK)
            break;
          else
          // If fail then increase minTimeBtwReads_ms
          minTimeBtwReads_ms += DHTXX_MIN_TIME_BTW_READS_MS_INCR;
          Serial_println(minTimeBtwReads_ms);
          delay(100);
        }

      }
      else {
        OK = true;
      }
      if(!OK)
        return false;
      // DISPLAY DATA
      values[0] = mySensor->getTemperature();
      values[1] = mySensor->getHumidity();
      return true;
    }

    String Grove_DHTXX::GetTelemetry()
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

    CallbackInfo * Grove_DHTXX::GetCallbackInfo()
    {
        info.period = 314159;
        info.next = 137;
        info.SensorIndex = 123456;
        info.sendBT = false;
        info.isSensor=false;
        info.isRunning=false;
        return &info;
    }

    double Grove_DHTXX::Read(int property)
    {
      Serial_println("ReadAll()");
      if((property<0) || (property>1))
      {
        Serial_println("ReadAll() Invalid property");
        return ERRORDBL;
      }
      double values[2];
      if(ReadAll(values))
      {
        Serial_println("ReadAll() Done");
        return values[property];
      }
      else {
        Serial_println("ReadAll() Fail");
        return ERRORDBL;
      }
    }

