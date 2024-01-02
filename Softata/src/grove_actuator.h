#ifndef GROVE_ACTUATORH
#define GROVE_ACTUATORH
#include "grove.h"

#define C(x) x,
enum GroveActuator { ACTUATORS ACTUATOR_NONE};
#undef C
#define C(x) #x,    
const char * const actuator_name[] = { ACTUATORS };
#undef C

class Grove_Actuator: public Grove
{
    public:
      static String GetListof()
      {
        String list ="ACTUATORS:";
        int numActuators = (int) ACTUATOR_NONE;
        for (int n=0;n<numActuators;n++)
        {
          list.concat(actuator_name[n]);
          if (n != (numActuators-1))
            list.concat(',');
        }
        return list;
      }

      static int GetIndexOf(String actuatorName)
      {
        int numActuators = (int) ACTUATOR_NONE;
        for (int n=0;n<numActuators;n++)
        {
          GroveActuator s = (GroveActuator)n;
          String name = String(actuator_name[s]);
          if (actuatorName.compareTo(name)==0)
          {
            return (int)s;
          }
        }
        return INT_MAX;
      }      

      virtual String GetListofx()
      {
        return Grove_Actuator::GetListof();
      }

      //static virtual arduino::String GetPins();
      virtual bool Setup();
      virtual bool Setup(byte * settings, byte numSettings);
      // Index for if there are an array of actuators here.
      virtual bool Write(double value, int index=0);
      virtual bool Write(int value, int index=0);
      virtual bool Set(bool state,int index=0);
      virtual bool Toggle(int index = 0);
      DeviceType deviceType = actuator;
    protected:
      int num_properties=0;
      // Use following to indicate an error
};
#endif
