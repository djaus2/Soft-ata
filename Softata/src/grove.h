#ifndef GROVEH
#define GROVEH
#include "../softata.h"
#include <wString.h>
#include <Wire.h>

// As per softata.h:
//enum DeviceType  {sensor,actuator,communication,display};
#define DEVICETYPES G_DEVICETYPES
#define SENSORS     G_SENSORS
#define ACTUATORS   G_ACTUATORS
#define I2CDISPLAYS G_I2CDISPLAYS

#define C(x) x,
enum DeviceType { DEVICETYPES DEVICETYPE_NONE};
#undef C
#define C(x) #x,    
const char * const device_name[] = { DEVICETYPES };
#undef C

class Grove
{
    public:
    //To be called before Wire.begin()/Wire1.begin()
    static bool SetI2CPins(int i2c)
    {
      if (i2c==0)
      {
#ifdef G_USE_GROVE_RPIPICO_SHIELD
        Wire.setSDA(GROVE_I2C0_SDA_SDA);
        Wire.setSCL(GROVE_I2C0_SDA_SCL);
#endif
        //Default to 4 and 5 respectively on RPi Pico
        return true;
      }
      else if (i2c==1)
      {
#ifdef G_USE_GROVE_RPIPICO_SHIELD
        Wire1.setSDA(GROVE_I2C1_SDA_SDA);
        Wire1.setSCL(GROVE_I2C1_SDA_SCL); 
#endif
        // Default to ?? TBD
        return true;     
      }
      return false;
    }

    static String GetListofDevices()
    {
      String list ="DEVICES:";
      int numDevices = (int) DEVICETYPE_NONE;
      for (int n=0;n<numDevices;n++)
      {
        list.concat(device_name[n]);
        if (n != (numDevices-1))
          list.concat(',');
      }
      return list;
    }

    virtual bool Setup();
    virtual bool Setup(int * settings, int numSettings);
    DeviceType deviceType;
      
    protected:
      
      
};
#endif