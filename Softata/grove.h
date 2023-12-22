#ifndef GROVEH
#define GROVEH
#include <wString.h>

enum DeviceType  {sensor,actuator,communication};


class Grove
{
    public:
      //static virtual arduino::String GetPins();
      virtual bool Setup();
      virtual bool Setup(int * settings, int numSettings);
      DeviceType deviceType;
      
    protected:
      
      
};
#endif