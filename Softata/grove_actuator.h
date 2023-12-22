#ifndef GROVE_ACTUATORH
#define GROVE_ACTUATORH
#include "grove.h"

class Grove_Actuator: Grove
{
    public:
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
