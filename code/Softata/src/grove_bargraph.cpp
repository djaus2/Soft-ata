#include <Arduino.h>
#include "grove_displays.h"
#include <Grove_LED_Bar.h>
#include "../rpiwatchdog.h"

// Grove_Bargraph
Grove_LED_Bar * pBar;
int clk = 17;
int dio = 16;

// Default Setup
bool Grove_Bargraph::Setup()
{
  clk = 17;
  dio = 16;
  pBar = new Grove_LED_Bar(clk, dio, 0, LED_BAR_10);
  pBar->begin();
  delay(200);
  pBar->setBits(0);
  return true;
}

bool Grove_Bargraph::Setup(byte * settings, byte numSettings)
{
  if(numSettings<2)
    return false;
  clk = settings[0];
  dio = settings[1];
  pBar = new Grove_LED_Bar(clk, dio, 0, LED_BAR_10);
  pBar->begin();
  delay(200);
  pBar->setBits(0);
  return true;
}

bool Grove_Bargraph::Clear()
{
  // Switch off all LEDs
  pBar->setLevel(0);
  return true;
}

bool Grove_Bargraph::Backlight()
{
  return true;
}

bool Grove_Bargraph::Home()
{
    return true;
}


bool Grove_Bargraph::SetCursor(byte x, byte y)
{
  return true;
}


bool Grove_Bargraph::WriteString(String msg)
{
  int len = msg.length();
  if (len>10) len = 10;
  int numVal=0;
  String prefix = msg.substring(0,2);
  numStringType typ = _NONE;
  if(prefix == "0b" ){
    typ = _BIN;
  } else if( prefix == "0x"){
    typ = _HEX;
  }
  switch (typ){
    case _BIN:
      for (int i=0; i<len-2; i++){
        // shift over what's there
        numVal = numVal << 1;
        if(msg[len-i] == 1){
            numVal |= 1;
        }
      };
      break;
    case _HEX:
      numVal = (int) strtoull(msg.c_str(), 0, 16);
      numVal &= 0b00001111111111;
      break;
    default:
      Serial.println(msg);
      numVal = msg.toInt();
      numVal &= 0b00001111111111;
      break;
  }
  pBar->setBits(numVal);
  return true;
}

bool Grove_Bargraph::CursorWriteStringAvailable()
{
	return true;
}

bool Grove_Bargraph::WriteString(byte x, byte y, String msg)
{
  return true;
}

bool Grove_Bargraph::Misc(byte cmd, byte * data, byte length)
{
  BARGRAPHMiscCmds Cmd = (BARGRAPHMiscCmds)cmd;
  switch(Cmd)
  {
    case flow:
      //Just set every odd segment for now.
      Serial.println("XX flow()");
       // pBar->setBits(0b000000000000101);
      pBar->setBits(0b000000101010101);
      Serial_println("flow()");
      break;
    case flow2:
      //Just set even  segment for now.
            Serial.println("XX flow2()");
      pBar->setBits(0b000001010101010);
      Serial_println("flow2()");;
      break;
    case setLed:
    case clrLed:
    case toggleLed:
      {
        // Set or toggle a specific LED
        if (length < 1)
          return false;

        int bargraphSegment = data[0];

        if (Cmd == setLed)
          pBar->setLed(bargraphSegment,1);
        else if (Cmd == clrLed)
          pBar->setLed(bargraphSegment,0);
        else
          pBar->toggleLed(bargraphSegment);
      }
      break;
    case setLevel:
      // Set the level of the bar
      if (length < 1)
        return false;
      pBar->setLevel(data[0]);
      break;
    case exercise:
        // Turn on all LEDs
        pBar->setBits(0x3ff);
        delay(500);

        // Turn off all LEDs
        pBar->setBits(0x0);
        delay(500);
    #ifdef ENABLE_WATCHDOG
      watchdog_update();
    #endif
        // Turn on LED 1
        // 0b000000000000001 can also be written as 0x1:
        pBar->setBits(0b000000000000001);
        delay(500);

        // Turn on LEDs 1 and 3
        // 0b000000000000101 can also be written as 0x5:
        pBar->setBits(0b000000000000101);
        delay(500);
    #ifdef ENABLE_WATCHDOG
      watchdog_update();
    #endif
        // Turn on LEDs 1, 3, 5, 7, 9
        pBar->setBits(0x155);
        delay(500);

        // Turn on LEDs 2, 4, 6, 8, 10
        pBar->setBits(0x2AA);
        delay(500);
    #ifdef ENABLE_WATCHDOG
      watchdog_update();
    #endif
        // Turn on LEDs 1, 2, 3, 4, 5
        // 0b000000000011111 == 0x1F
        pBar->setBits(0b000000000011111);
        delay(500);

        // Turn on LEDs 6, 7, 8, 9, 10
        // 0b000001111100000 == 0x3E0
        pBar->setBits(0b000001111100000);
        delay(500);
    #ifdef ENABLE_WATCHDOG
      watchdog_update();
    #endif
        pBar->setBits(0);
      break;
    default:
      return false;
      break;
  }
  return true;
}