#ifndef GROVE_DH11H
#define GROVE_DH11H
#include "grove_sensor.h"

class Grove_DHT11: public Grove_Sensor
{
    public:
      virtual String GetPins();
      virtual bool Setup();
      virtual bool Setup(int * settings, int numSettings);
      virtual bool ReadAll(double * values);
      virtual double Read(int property);

    protected:
      int Pin;
      bool SetupPin(int pin);
};
#endif