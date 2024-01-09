#include <Arduino.h>
#include "grove_actuator.h"

Grove_Servo::Grove_Servo()
{
    Setup();
}

Grove_Servo::Grove_Servo(int pin)
{
    SetupServo(pin,MIN_PW,MAX_PW,PERIOD);
}

bool Grove_Servo::Setup()
{
    SetupServo(DEFAULT_SERVO_PIN,MIN_PW,MAX_PW,PERIOD);
    return true;
}

bool Grove_Servo::Setup(byte * settings, byte numSettings)
{
    if(numSettings>0)
    {
        int pin = settings[0];
        if(numSettings>1)
        {
            int min = settings[1];
            if(numSettings>2)
            {
                int max = settings[2];
                if(numSettings>3)
                {
                    int period = settings[3];
                    return SetupServo(pin,min,max,period);
                }
                else
                    return SetupServo(pin,min,max,PERIOD);
            }
            else
                return SetupServo(pin,min,MAX_PW,PERIOD);
        }
        else
            return SetupServo(pin,MIN_PW,MAX_PW,PERIOD);
    }
    else
        return SetupServo(DEFAULT_SERVO_PIN,MIN_PW,MAX_PW,PERIOD);
    return true;
}

bool Grove_Servo::SetupServo(int pin, int min, int max, int period)
{
    myservo.attach(pin,min,max);
    return true;
}


// Index for if there are an array of actuators here.
bool Grove_Servo::Write(double value, int index=0)
{
    int angle = (int)value;
    if((angle<0)||(angle>180))
        return false;
    myservo.write(angle);
    return true;
}

bool Grove_Servo::Write(int angle, int index=0)
{
    if((index<0)||(index>180))
        return false;
    myservo.write(angle);
    return true;
}

bool Grove_Servo::Set(bool state,int index=0)
{
    return true;
}

bool Grove_Servo::Toggle(int index = 0)
{
    return true;
}