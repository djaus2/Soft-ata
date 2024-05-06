#include <Arduino.h>
#include "ic_74hc595_shiftRegister.h"

//Ref: https://docs.arduino.cc/tutorials/communication/guide-to-shift-out/
//     https://www.arduino.cc/reference/en/language/functions/advanced-io/shiftout/

        int dddatapin = 16;  
        int latchPin = 20;
        int clockPin = 21;
        unsigned int value = 0;

void _writeTo595_16(BitOrder order, unsigned int data ) {
  // Output low level to latchPin
    digitalWrite(latchPin, LOW);
    // Send serial data to 74HC595
    if(order == MSBFIRST)
    {
        // shift out high byte
        shiftOut(dddatapin, clockPin, MSBFIRST, (data >> 8));
        // shift out low byte
        shiftOut(dddatapin, clockPin, MSBFIRST, data);
    }
    else
    {
        // shift out low byte
        shiftOut(dddatapin, clockPin, LSBFIRST, data);
        // shift out high byte
        shiftOut(dddatapin, clockPin, LSBFIRST, (data >> 8));
    }

    // Output high level to latchPin, and 74HC595 will update the data to the parallel output port.
    digitalWrite(latchPin, HIGH);
    value = data;
}

void _writeTo595_8(BitOrder order, byte data ) {
  // Output low level to latchPin
  digitalWrite(latchPin, LOW);
  // Send serial data to 74HC595
  shiftOut(dddatapin, clockPin, order, data);
  // Output high level to latchPin, and 74HC595 will update the data to the parallel output port.
  digitalWrite(latchPin, HIGH);
  value = data;
}

bool IC_74HC595_ShiftRegister::Setup()
{
  dddatapin = 16;  
  latchPin = 20;
  clockPin = 21;
  return Setup(NULL,0);
}

bool IC_74HC595_ShiftRegister::Setup(byte * settings, byte numSettings)
{
  if (numSettings >0)
  {
    dddatapin = (int)settings[0];
    if (numSettings >1)
    {
        latchPin = (int)settings[1];
        if (numSettings >2)
        {
            clockPin = (int)settings[2];
        };
    };
  };
  pinMode(latchPin, OUTPUT);
  pinMode(clockPin, OUTPUT);
  pinMode(dddatapin, OUTPUT);
  value = 0;
  _writeTo595_8(LSBFIRST, value);
  return true;
}


// Default Setup
IC_74HC595_ShiftRegister::IC_74HC595_ShiftRegister()
{
  Setup();
}

IC_74HC595_ShiftRegister::IC_74HC595_ShiftRegister(byte * settings, byte numSettings)
{
  Setup(settings,numSettings);
}


bool IC_74HC595_ShiftRegister::Write8(byte num)
{
  _writeTo595_8(LSBFIRST, num);
  return true;
}

// Can daisy chain two 74HC595s as per top link
bool IC_74HC595_ShiftRegister::Write16(unsigned int num)
{
  //Assume data is 16 bits
  _writeTo595_16(LSBFIRST, num);
  return true;
}

bool IC_74HC595_ShiftRegister::SetBitState(bool state,int index)
{
    byte mask = 1 << index;
    if (state)
    {
        value |= mask;
    }
    else
    {
        value &= ~mask;
    }
    _writeTo595_8(LSBFIRST, value);
    return true;
}


bool IC_74HC595_ShiftRegister::SetBit(int index)
{
    byte mask = 1 << index;
    value |= mask;
    _writeTo595_8(LSBFIRST, value);
    return true;
}

bool IC_74HC595_ShiftRegister::ClearBit(int index)
{
    byte mask = 1 << index;
    value &= ~mask;
    _writeTo595_8(LSBFIRST, value);
    return true;
}

bool IC_74HC595_ShiftRegister::ToggleBit(int index )
{
    byte mask = 1 << index;
    value ^= mask;
    _writeTo595_8(LSBFIRST, value);
    return true;
}