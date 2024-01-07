#include "grove_environsensors.h"
#include <arduino.h>
#include "Ultrasonic.h"

#include <float.h>
#include "../Softata.h"

Ultrasonic * ultrasonic;

Grove_URangeSensor::Grove_URangeSensor(int SetupPin)
{
    ultrasonic = new Ultrasonic(SetupPin);
    Grove_Sensor::num_properties = NUM_URANGE_PROPERTIES;
    Grove_Sensor::sensorType = URANGE;
}

bool Grove_URangeSensor::Setup(byte * settings, byte numSettings)
{
    return true;
}

bool Grove_URangeSensor::Setup()
{
    return true;
}


bool Grove_URangeSensor::ReadAll(double* values)
{
    values[0] = (double)ultrasonic->MeasureInMillimeters();
    if(values[0]==0)
        values[0] = (double)ultrasonic->MeasureInMillimeters();
    values[1] = (double)ultrasonic->MeasureInCentimeters();
    if(values[1]==0)
        values[1] = (double)ultrasonic->MeasureInCentimeters();
    values[2] = (double)ultrasonic->MeasureInInches();
    if(values[2]==0)
        values[2] = (double)ultrasonic->MeasureInInches();
    return true;
}

double Grove_URangeSensor::Read(int property)
{
    if(property==0)
    {
        return (double)ultrasonic->MeasureInMillimeters();
    }
    else if(property==1)
    {
        return (double)ultrasonic->MeasureInCentimeters();
    }
    else if(property==2)
    {
        return (double)ultrasonic->MeasureInInches();
    }
    else
        return DBL_MAX;
}