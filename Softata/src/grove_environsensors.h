#ifndef GROVE_DH11H
#define GROVE_DH11H
#include "grove_sensor.h"

#include <float.h>
#define ERRORDBL  DBL_MAX;

class Grove_DHT11: public Grove_Sensor
{
    public:
      Grove_DHT11();
      virtual String GetPins();
      virtual String GetListofProperties();
      virtual bool Setup();
      virtual bool Setup(int * settings, int numSettings);
      virtual bool ReadAll(double * values);
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
      virtual String GetPins();
      virtual String GetListofProperties();
      virtual bool Setup();
      virtual bool Setup(int * settings, int numSettings);
      virtual bool ReadAll(double * values);
      virtual double Read(int property);

    protected:
      bool SetupBME280();
};
#endif