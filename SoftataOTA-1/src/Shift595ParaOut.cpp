#include <Arduino.h>
#include "grove_actuator.h"

Shift595ParaOut::Shift595ParaOut()
{
    Setup();
}

Shift595ParaOut::Shift595ParaOut(byte * settings)
{
    Setup(settings,NUM_SETTINGS);
}

bool Shift595ParaOut::Setup()
{
  ic595 = new IC_74HC595_ShiftRegister();
  num_bytes = 1;
  return true;
}

bool Shift595ParaOut::Setup(byte * settings, byte numSettings)
{
  ic595 = new IC_74HC595_ShiftRegister(settings,numSettings);
  if(num_settings>3)
    num_bytes = settings[3];
  return true;
}

// Index for if there are an array of actuators here.
// Instance of is collected elsewhere.
// No longer need. 2Do Remove

bool Shift595ParaOut::Write(double value, int index)
{
    return true;
}
bool Shift595ParaOut::Write(int value, int index, int numBytes )
{
    ic595->Write(value,numBytes);
    return true;
}


bool Shift595ParaOut::SetBitState(bool state,int bitNo)
{
    ic595->SetBitState(state,bitNo);
    return true;
}

bool Shift595ParaOut::SetBit(int bitNo )
{
    ic595->SetBit(bitNo);
    return true;
}

bool Shift595ParaOut::ClearBit(int bitNo)
{
    ic595->ClearBit(bitNo);
    return true;
}

bool Shift595ParaOut::ToggleBit(int bitNo)
{
    ic595->ToggleBit(bitNo);
    return true;
}