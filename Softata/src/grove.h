#ifndef GROVEH
#define GROVEH
#include <wString.h>
#include <Wire.h>

enum DeviceType  {sensor,actuator,communication,display};

//Add other sensors/actuators here C bracketed, on end.
#define SENSORS C(DHT11)C(BME280)C(LIGHT)C(SOUND)
#define ACTUATORS C(BUZZER)
#define I2CDISPLAYS C(OLED096)C(LCD1602)

class Grove
{
    public:
    //To be called before Wire.begin()/Wire1.begin()
    static bool SetI2CPins(int i2c)
    {
      if (i2c==0)
      {
        Wire.setSDA(8);
        Wire.setSCL(9);
        return true;
      }
      else if (i2c==1)
      {
        Wire1.setSDA(6);
        Wire1.setSCL(7); 
        return true;     
      }
      return false;
    }

    virtual bool Setup();
    virtual bool Setup(int * settings, int numSettings);
    DeviceType deviceType;
      
    protected:
      
      
};
#endif