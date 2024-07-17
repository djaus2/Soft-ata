#include "grove_environsensors.h"
#include <arduino.h>
#include <Wire.h>
#include <float.h>
#include "../Softata.h"

#include <BME280I2C.h> 

#undef NUM_PROPERTIES
#define NUM_PROPERTIES 3

BME280I2C::Settings settings(
   BME280::OSR_X1,
   BME280::OSR_X1,
   BME280::OSR_X1,
   BME280::Mode_Forced,
   BME280::StandbyTime_1000ms,
   BME280::Filter_Off,
   BME280::SpiEnable_False,
#ifdef BME280_I2C_ADDRESS76
   BME280I2C::I2CAddr_0x76 
#elif defined(BME280_I2C_ADDRESS77)
   BME280I2C::I2CAddr_0x77
#endif
);
BME280I2C bme(settings);

double Round2Places(float value)
{
    if (value == NAN)
      return NAN;
    double ret = (double) value; // 2Do:Round(value,2);
    return ret;
}

Grove_BME280::Grove_BME280()
{
    Grove_Sensor::num_properties = NUM_BME280_PROPERTIES;
    Grove_Sensor::sensorType = BME280;
}


//#include <float.h>



bool Grove_BME280::Setup(byte * settings, byte numSettings)
{
    if(numSettings>0)
    {
    int i2c = settings[0];
    if((i2c==0) || (i2c==1))
    {
        // Nb: Code uses Wire/IC0
        Grove::SetI2CPins(i2c);
        return SetupBME280();
    }
    else
        return false;
    }
    else
        return false;
}

//Default to I2C0 on RPi Pico Shield
bool Grove_BME280::Setup()
{
    Grove::SetI2CPins(0);
    return SetupBME280();
}

bool Grove_BME280::SetupBME280()
{
    while (!Serial);

    Wire.begin();
    //begin() checks the Interface, reads the sensor ID (to differentiate between BMP280 and BME280)
    //and reads compensation parameters.
    if (!bme.begin())
    {
        Serial_println("begin() failed. check your BMx280 Interface and I2C Address.");
        while (1);
    }


    //reset sensor to default parameters.
    //bme.resetToDefaults();

    //by default sensing is disabled and must be enabled by setting a non-zero
    //oversampling setting.
    //set an oversampling setting for pressure and temperature measurements. 
    //bme.writeOversamplingPressure(BMx280MI::OSRS_P_x16);
    ////bme.writeOversamplingTemperature(BMx280MI::OSRS_T_x16);

    //if sensor is a BME280, set an oversampling setting for humidity measurements.
    //if (bme.isBME280())
        //bme.writeOversamplingHumidity(BMx280MI::OSRS_H_x16);
    return true;
}


bool Grove_BME280::ReadAll(double * values)
{
  float temp(NAN), hum(NAN), press(NAN);

   BME280::TempUnit tempUnit(BME280::TempUnit_Celsius);
   BME280::PresUnit presUnit(BME280::PresUnit_Pa);

   bme.read(press, temp, hum, tempUnit, presUnit);

    values[0] = Round2Places(temp);
    values[1] = Round2Places(press);   
    values[2] = Round2Places(hum);
    return true;
}


String Grove_BME280::GetTelemetry()
{
  double values[3];
  if (ReadAll(values))
  {
   String msg ="{\"temperature\":";
    msg.concat(values[0]);
    msg.concat(',');
    msg.concat("\"pressure\":");
    msg.concat(values[1]);
    msg.concat(',');
    msg.concat("\"humidity\":");
    msg.concat(values[2]);
    msg.concat('}');
    return msg;
  }
  else
    return "ERRORDBL";
}

CallbackInfo * Grove_BME280::GetCallbackInfo()
{
    info.period = 314159;
    info.next = 137;
    info.SensorIndex = 123456;
    info.sendBT = false;
    info.isSensor=false;
    info.isRunning=false;
    return &info;
}

double Grove_BME280::Read(int property)
{

   BME280::TempUnit tempUnit(BME280::TempUnit_Celsius);
   BME280::PresUnit presUnit(BME280::PresUnit_Pa);

    // Send DATA
    switch(property)
    {
    case 0:
        return Round2Places(bme.temp(tempUnit));
        break;
    case 1: 
        return Round2Places(bme.pres(presUnit));
        break; 
    case 2: 
        return Round2Places(bme.hum());
        break; 
    default:
        return ERRORDBL;
        break;
    }
}


