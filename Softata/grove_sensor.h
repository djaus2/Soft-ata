#ifndef GROVE_SENSORH
#define GROVE_SENSORH
#include <Arduino.h>
#include "grove.h"

//Add other sensors hereC bracketed, on end eg C(POTENTIOMETER)
#define SENSORS C(DHT11)C(BME280)
#define C(x) x,
enum GroveSensor { SENSORS NONE};
#undef C
#define C(x) #x,    
const char * const sensor_name[] = { SENSORS };


int GetGroveSensorIndex(String sensorName);

class Grove_Sensor: public Grove
{
    public:

      static int GetGroveSensorIndex(String sensorName)
      {
        int numSensors = (int) NONE;
        for (int n=0;n<numSensors;n++)
        {
          GroveSensor s = (GroveSensor)n;
          String name = String(sensor_name[s]);
          if (sensorName.compareTo(name)==0)
          {
            return (int)s;
          }
        }
        return INT_MAX;
      }
      //static virtual arduino::String GetPins();
      virtual bool Setup();
      virtual bool Setup(int * settings, int numSettings);
      virtual bool ReadAll(double * values);
      virtual double Read(int property);
      DeviceType deviceType = sensor;
    protected:
      int num_properties=0;

};
#endif
