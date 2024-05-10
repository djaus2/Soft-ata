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


      //static virtual arduino::String GetPins();;
      //static virtual arduino::String GetValueRange()
      virtual bool Setup();
      virtual bool Setup(byte * settings, byte numSettings);
      // Index for if there are an array of actuators here.
      virtual bool Write(double value, int index);
      virtual bool Write(int value, int index, int numBytes = 1);
      virtual bool SetBitState(bool state,int index);
      virtual bool SetBit(int index = 0);
      virtual bool ClearBit(int index = 0);
      virtual bool ToggleBit(int index = 0);
      DeviceType deviceType = actuator;
    protected:
      int num_properties=0;
      // Use following to indicate an error
};
#endif

#ifndef SERVOH
#define SERVOH

#include <Servo.h>

#define MAX_PW 2400
#define MIN_PW 544
#define PERIOD 20000

class Grove_Servo: public Grove_Actuator
{
    public:
      Grove_Servo();
      Grove_Servo(int pin);
      static arduino::String GetPins()
      {
        String msg = "OK:";
        msg.concat(SERVO_PINNOUT);
        return msg;
      }
      static arduino::String GetValueRange()
      {
        String msg = "OK:";
        msg.concat(SERVO_RANGE);
        return msg;
      }
      virtual bool Setup();
      virtual bool Setup(byte * settings, byte numSettings);

      // Index for if there are an array of actuators here.
      virtual bool Write(double value, int index);
      virtual bool Write(int value, int index, int numBytes = 1);
      virtual bool SetBitState(bool state,int index);
      virtual bool SetBit(int index );
      virtual bool ClearBit(int index );
      virtual bool ToggleBit(int index );
      DeviceType deviceType = actuator;
    protected:
      virtual bool SetupServo(int pin, int min, int max, int period);
      int num_properties=0;
      Servo myservo;
};
#endif

#ifndef Shift595ParaOutH
#define Shift595ParaOutH

#include "ic_74hc595_shiftRegister.h"

#define NUM_SETTINGS 4 // 3 pins plus number of bytes

class Shift595ParaOut: public Grove_Actuator
{
    public:
      Shift595ParaOut();
      Shift595ParaOut(byte * settings);
      static arduino::String GetPins()
      {
        String msg = "OK:";
        msg.concat(SHIFT595PARAOUT_PINNOUT);
        return msg;
      }
      static arduino::String GetValueRange()
      {
        String msg = "OK:";
        msg.concat(SHIFT595PARAOUT_RANGE);
        return msg;
      }
      virtual bool Setup();
      virtual bool Setup(byte * settings, byte numSettings);

      // Index for if there are an array of actuators here.
      virtual bool Write(double value, int index);
      virtual bool Write(int value, int index, int numBytes = 1);
      virtual bool SetBitState(bool state,int bitNo);
      virtual bool SetBit(int bitNo );
      virtual bool ClearBit(int bitNo );
      virtual bool ToggleBit(int bitNo );
      DeviceType deviceType = actuator;
    protected:
      int num_settings=NUM_SETTINGS;
      int num_bytes=1;
      IC_74HC595_ShiftRegister * ic595;
};
#endif


