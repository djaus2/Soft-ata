#ifndef GROVE_SENSORH
#define GROVE_SENSORH
#include <Arduino.h>
#include "grove.h"

#define MAX_SENSOR_PROPERTIES 10


#define C(x) x,
enum GroveSensor { SENSORS SENSOR_NONE};
#undef C
#define C(x) #x,    
const char * const sensor_name[] = { SENSORS };
#undef C

class Grove_Sensor: public Grove
{
    public:

      static String GetListof()
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

      static int GetIndexOf(String sensorName)
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

      static String GetListofx()
      {
        return Grove_Sensor::GetListof();
      }

      virtual arduino::String GetPins();
      virtual arduino::String GetListofProperties();
      virtual bool Setup();
      virtual bool Setup(int * settings, int numSettings);
      virtual bool ReadAll(double * values);
      virtual double Read(int property);
      DeviceType deviceType = sensor;
      GroveSensor sensorType = SENSOR_NONE;
      int num_properties=0;
    protected:
      

};
#endif
