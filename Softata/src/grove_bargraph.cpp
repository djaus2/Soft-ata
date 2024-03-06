#include <Arduino.h>
#include "grove_displays.h"

bool Custom_Bargraph::Setup()
{

  return true;
}

bool Custom_Bargraph::Setup(byte * settings, byte numSettings)
{
  
  return true;
}

bool Custom_Bargraph::Clear()
{

  return true;
}

bool Custom_Bargraph::Backlight()
{
  return true;
}

bool Custom_Bargraph::Home()
{
    return true;
}


bool Custom_Bargraph::SetCursor(byte x, byte y)
{
  return true;
}

bool Custom_Bargraph::WriteString(String msg)
{
  return true;
}

bool Custom_Bargraph::CursorWriteStringAvailable()
{
	return true;
}

bool Custom_Bargraph::WriteString(byte x, byte y, String msg)
{
  return true;
}

bool Custom_Bargraph::Misc(byte cmd, byte * data, byte length)
{
  BARGRAPHMiscCmds Cmd = (BARGRAPHMiscCmds)cmd;
  switch(Cmd)
  {
    case flow:
      return true;
      break;
    default:
      return false;
      break;
  }
}