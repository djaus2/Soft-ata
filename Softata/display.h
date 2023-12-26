#ifndef GROVE_DISPLAYH
#define GROVE_DISPLAYH
#include "grove.h"

#define C(x) x,
enum GroveDisplay { DISPLAYS DISPLAYS_NONE};
#undef C
#define C(x) #x,    
const char * const display_name[] = { DISPLAYS };
#undef C

class Grove_Display: public Grove
{
    public:
      static String GetListofGroveDisplays()
      {
        String list ="DISPLAYS:";
        int numDisplays = (int) DISPLAYS_NONE;
        for (int n=0;n<numDisplays;n++)
        {
          list.concat(display_name[n]);
          if (n != (numActuators-1))
            list.concat(',');
        }
        return list;
      }

      static int GetGroveActuatorIndex(String displayName)
      {
        int numDisplays = (int) DISPLAY_NONE;
        for (int n=0;n<numDisplays;n++)
        {
          GroveDisplay s = (GroveDisplay)n;
          String name = String(display_name[s]);
          if (displayName.compareTo(name)==0)
          {
            return (int)s;
          }
        }
        return INT_MAX;
      }      

      //static virtual arduino::String GetPins();
      virtual bool Setup();
      virtual bool Setup(int * settings, int numSettings);
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
