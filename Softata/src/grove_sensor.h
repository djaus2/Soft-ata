#ifndef GROVE_SENSORH
#define GROVE_SENSORH
#include <Arduino.h>
#include "grove.h"
#include "../Softata.h"

#define NUM_PROPERTIES 0



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

      virtual bool Setup();
      virtual bool Setup(byte * settings, byte numSettings=1);
      virtual bool ReadAll(double * values);
      virtual String GetTelemetry();
      virtual double Read(int property);
      virtual CallbackInfo * GetCallbackInfo();
      DeviceType deviceType = sensor;
      GroveSensor sensorType = SENSOR_NONE;
      int num_properties;
    protected:
      

};
#endif
