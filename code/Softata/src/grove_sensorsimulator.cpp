#include "grove_environsensors.h"
#include <arduino.h>

#include "Softata.h"


#ifdef GROVE_RPI_PICO_SHIELD
// Insert, see DHT11 and BME280
#elif defined(RPI_PICO_DEFAULT)
// Insert, see DHT11 and BME280
#endif






    Grove_SensorSimulator::Grove_SensorSimulator()
    {
      //Any class instantiation
      sensorType = Simulator;
      num_properties = NUM_SIMULATOR_PROPERTIES;
    }

  

    bool Grove_SensorSimulator::Setup(byte * settings, byte numSettings)
    {
      randomSeed(analogRead(0));
      return true;
    }
 
    bool Grove_SensorSimulator::Setup()
    {
      randomSeed(analogRead(0));
      return true;
    }

    
    bool Grove_SensorSimulator::ReadAll(double * values)
    {
      //Read all values and assign to values array eg:
      values[0] = Read(0);
      return true;
    }

    String Grove_SensorSimulator::GetTelemetry()
    {
      double values[NUM_SIMULATOR_PROPERTIES];
      if(ReadAll(values))
      {
        String msg="{\"temperature\":";
        //Construct json string from num_properties
        // See DHT11 and BME280
        msg.concat(values[0]);
        msg.concat("}");
        return msg;
      }
      else
        return "ERRORDBL";
}

CallbackInfo * Grove_SensorSimulator::GetCallbackInfo()
{
    info.period = 314159;
    info.next = 137;
    info.SensorIndex = 123456;
    info.sendBT = false;
    info.isSensor=false;
    info.isRunning=false;
    return &info;
}

  double Grove_SensorSimulator::Read(int property)
  {
    // Read and return the nth property
    // Might need to readall then select int
    // See DHT11 and BME280
    // Return 10.00 +- upto 1.00
    double val = 10.00;
    double diff = random(0,100)-50;
    diff /= 50;
    val += diff;
    return val;
  }


