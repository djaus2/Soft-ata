#include "grove_environsensors.h"
#include <arduino.h>
#include <Wire.h>

#include <BMx280I2C.h>

#define I2C_ADDRESS76 0x76
#define I2C_ADDRESS77 0x77

int I2C_ADDRESS = I2C_ADDRESS77;
BMx280I2C bmx280(I2C_ADDRESS);
//BMx280I2C bmx280;


//#include <float.h>



String Grove_BME280::GetPins()
{
    return String("Grove IC0: SDA=8,SCL=9 IC1: SDA=6, SCL=7");
}

bool Grove_BME280::Setup(int * settings, int numSettings)
{
    if(numSettings>0)
    {
    int i2c = settings[0];
    if((i2c==0) || (i2c==1))
    {
        if(numSettings>1)
        {
            int addr = settings[1];
            if((addr==0x76) || (addr==0x77))
            {
                I2C_ADDRESS = addr;
            }
            else
                return false;
        }
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
    I2C_ADDRESS = I2C_ADDRESS77;
    return SetupBME280();
}

bool Grove_BME280::SetupBME280()
{
    while (!Serial);

    Wire.begin();
    //begin() checks the Interface, reads the sensor ID (to differentiate between BMP280 and BME280)
    //and reads compensation parameters.
    if (!bmx280.begin())
    {
        Serial.println("begin() failed. check your BMx280 Interface and I2C Address.");
        while (1);
    }

    if (bmx280.isBME280())
        Serial.println("sensor is a BME280");
    else
        Serial.println("sensor is a BMP280");

    //reset sensor to default parameters.
    bmx280.resetToDefaults();

    //by default sensing is disabled and must be enabled by setting a non-zero
    //oversampling setting.
    //set an oversampling setting for pressure and temperature measurements. 
    bmx280.writeOversamplingPressure(BMx280MI::OSRS_P_x16);
    bmx280.writeOversamplingTemperature(BMx280MI::OSRS_T_x16);

    //if sensor is a BME280, set an oversampling setting for humidity measurements.
    if (bmx280.isBME280())
        bmx280.writeOversamplingHumidity(BMx280MI::OSRS_H_x16);
    return true;
}

bool readBME280()
{
    if (!bmx280.measure())
    {
        Serial.println("could not start measurement, is a measurement already running?");
        return false;
    }

    //wait for the measurement to finish
    do
    {
        delay(100);
    } while (!bmx280.hasValue());
    return true;
}

bool Grove_BME280::ReadAll(double * values)
{
    if (!readBME280())
        return false;

    // Get DATA
    values[0] = bmx280.getTemperature();
    values[1]= bmx280.getPressure();
    values[2] = bmx280.getHumidity();
    return true;
}

double Grove_BME280::Read(int property)
{
    if (!readBME280())
        return false;

    // DISPLAY DATA
    switch(property)
    {
    case 0:
        return bmx280.getTemperature();
        break;
    case 1: 
        return bmx280.getPressure();
        break; 
    case 2: 
        return bmx280.getHumidity();
        break; 
    default:
        return ERRORDBL;
        break;
    }
}


