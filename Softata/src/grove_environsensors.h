#ifndef GROVE_DH11H
#define GROVE_DH11H
#include "grove_sensor.h"
#include "../Softata.h"

#include <float.h>
#define ERRORDBL  DBL_MAX;

class Grove_DHT11: public Grove_Sensor
{
    public:
      Grove_DHT11();
      static String GetPins()
      {
        String msg="OK:";
        msg.concat(DHT11_PINNOUT);
        return msg;        
      }
      static String GetListofProperties()
      {
        String msg="OK:";
        msg.concat(DHT11_PROPERTIES);
        return msg; 
      }
      virtual bool Setup();
      virtual bool Setup(byte * settings, byte numSettings=1);
      virtual bool ReadAll(double * values);
      virtual String GetTelemetry();
      virtual double Read(int property);
    protected:
      int Pin;
      bool SetupPin(int pin);
};
#endif

#ifndef GROVE_BME280H
#define GROVE_BME280H
#include "grove_sensor.h"

class Grove_BME280: public Grove_Sensor
{
    public:
      Grove_BME280();
      static String GetPins()
      {
        String msg="OK:";
        msg.concat(BME280_PINNOUT);
        return msg;       
      }
      static String GetListofProperties()
      {
        String msg="OK:";
        msg.concat(BME280_PROPERTIES);
        Serial.println(msg);
        return msg; 
      }
      virtual bool Setup();
      virtual bool Setup(byte * settings, byte numSettings=1);
      virtual bool ReadAll(double * values);
      virtual String GetTelemetry();
      virtual double Read(int property);
    protected:
      bool SetupBME280();
};
#endif

#ifndef GROVE_ULTRASONICRANGESENSORH
#define GROVE_ULTRASONICRANGESENSORH
#include "grove_sensor.h"


class Grove_URangeSensor : public Grove_Sensor
{
public:
    Grove_URangeSensor(int SetupPin);
    static String GetPins()
    {
        String msg = "OK:";
        msg.concat(URANGE_PINNOUT);
        return msg;
    }
    static String GetListofProperties()
    {
        String msg = "OK:";
        msg.concat(URANGE_PROPERTIES);
        return msg;
    }

    virtual bool Setup();
    virtual bool Setup(byte* settings, byte numSettings = 1);
    virtual bool ReadAll(double* values);
    virtual String GetTelemetry();
    virtual double Read(int property);
protected:
    bool SetupURangeSensor();
};
#endif