#include "grove_environsensors.h"
#include <arduino.h>
#include <Wire.h>
#include <float.h>

#include <BME280I2C.h> 

//#define I2C_ADDRESS76 0x76
#define I2C_ADDRESS77 0x77


BME280I2C::Settings settings(
   BME280::OSR_X1,
   BME280::OSR_X1,
   BME280::OSR_X1,
   BME280::Mode_Forced,
   BME280::StandbyTime_1000ms,
   BME280::Filter_Off,
   BME280::SpiEnable_False,
#ifdef I2C_ADDRESS76
   BME280I2C::I2CAddr_0x76 
#else
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



//#include <float.h>



String Grove_BME280::GetPins()
{
    return String("OK:Grove RPi Pico Shield: IC0 SDA=8,SCL=9");
}

String Grove_BME280::GetListofProperties()
{
    return String("OK:Temperature,Humidity,Pressure");
}

bool Grove_BME280::Setup(int * settings, int numSettings)
{
    if(numSettings>0)
    {
    int i2c = settings[0];
    if((i2c==0) || (i2c==1))
    {
        Grove::SetI2CPins(i2c);
        return SetupBME280();
    }
    else if(i2c==-1)
    {
        // Use RPI Pico Shield IC defaults (pins3 and 4);
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
        Serial.println("begin() failed. check your BMx280 Interface and I2C Address.");
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


