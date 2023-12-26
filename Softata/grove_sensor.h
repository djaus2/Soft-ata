#ifndef GROVE_SENSORH
#define GROVE_SENSORH
#include <Arduino.h>
#include "grove.h"


#define C(x) x,
enum GroveSensor { SENSORS SENSOR_NONE};
#undef C
#define C(x) #x,    
const char * const sensor_name[] = { SENSORS };
#undef C

class Grove_Sensor: public Grove
{
    public:
      static String GetListofGroveSensors()
      {
        String list ="SENSORS:";
        int numSensors = (int) SENSOR_NONE;
        for (int n=0;n<numSensors;n++)
        {
          list.concat(sensor_name[n]);
          if (n != (numSensors-1))
            list.concat(',');
        }
        return list;
      }

      static int GetGroveSensorIndex(String sensorName)
      {
        int numSensors = (int) SENSOR_NONE;
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

      virtual arduino::String GetPins();
      virtual bool Setup();
      virtual bool Setup(int * settings, int numSettings);
      virtual bool ReadAll(double * values);
      virtual double Read(int property);
      DeviceType deviceType = sensor;
    protected:
      int num_properties=0;

};
#endif
