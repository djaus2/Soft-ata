#include <Arduino.h>
#include <U8g2lib.h>
//Navigate to Sketch -> Include Library -> Manage Libraries... 
//Search and Install U8g2 library in the Library Manager.
#ifdef U8X8_HAVE_HW_SPI
#include <SPI.h>
#endif
#ifdef U8X8_HAVE_HW_I2C
#include <Wire.h>
#endif

#include "grove_displays.h"


/////////////////////////////////////////////////////////////////////
// Grove-LCD RGB Backlight  V2.00
// https://wiki.seeedstudio.com/Grove-LCD_RGB_Backlight/
/////////////////////////////////////////////////////////////////////
// Install Grove-LCD RGB Backlight by Seeed Studio
// https://github.com/Seeed-Studio/Grove_LCD_RGB_Backlight
/////////////////////////////////////////////////////////////////////
//https://github.com/DFRobot/DFRobot_RGBLCD1602

#include "rgb_lcd.h"

rgb_lcd lcd;

// Default backlight
#define COLORRED  128
#define COLORGREEN  128
#define COLORBLUE  128



bool Grove_LCD1602::Setup()
{
  Grove::SetI2CPins(0);
  lcd.begin(16, 2);
  lcd.setRGB(COLORRED, COLORGREEN, COLORBLUE);
  return true;
}

bool Grove_LCD1602::Setup(byte * settings, byte numSettings)
{
  if (numSettings>1)
  {
    int i2c = settings[0];
    if((i2c==0) || (i2c==1))
    {
      int colorRed = COLORRED;
      int colorGreen = COLORGREEN;
      int colorBlue = COLORBLUE;
      Grove::SetI2CPins(i2c);
      lcd.begin(16, 2);
      if (numSettings>3)
      {
          bool testRed = (settings[1]>-1)&&(settings[1]<256);
          bool testGreen = (settings[2]>-1)&&(settings[2]<256);
          bool testBlue = (settings[3]>-1)&&(settings[3]<256);
          if(testRed&&testGreen&&testBlue)
          {
            colorRed = settings[1];
            colorGreen = settings[2];
            colorBlue = settings[3];
          }
          lcd.setRGB(colorRed, colorGreen, colorBlue);
      }
      return false;
    }
    else
      return false;
  }
  return true;
}

String Grove_LCD1602::GetListofMiscCmds()
{
    String list ="Display Misc  Cmds:";
    list.concat(NEOPIXELMiscCmdsStr);
    return list;
}

Tristate Grove_LCD1602::Dummy()
{
  return notImplemented;
}

Tristate Grove_LCD1602::Clear()
{
  lcd.clear();
  return (Tristate)true;;
}

Tristate Grove_LCD1602::Home()
{
  lcd.home();
  return (Tristate)true;
}


Tristate Grove_LCD1602::SetCursor(byte x, byte y)
{
  // Note: line 1 is the second row, since counting begins with 0:
  lcd.setCursor(x,y);
  return (Tristate)true;
}

Tristate Grove_LCD1602::WriteString(String msg)
{
  lcd.print(msg.c_str());
  return (Tristate)true;
}

// No setcursor and/or no writestring
Tristate Grove_LCD1602::CursorWriteStringAvailable()
{
	return (Tristate)true;
}

Tristate Grove_LCD1602::WriteString(byte x, byte y, String msg)
{
  // Note: line 1 is the second row, since counting begins with 0:
  lcd.setCursor(x,y);
  lcd.print(msg.c_str());
  return (Tristate)true;
}

Tristate Grove_LCD1602::Misc(byte cmd, byte * data, byte length)
{
  if (cmd<0)
    return (Tristate)false;
  else if(cmd>(int) LCD1602MiscCmds_MAX)
    return (Tristate)false;
  LCD1602MiscCmds Cmd = (LCD1602MiscCmds)cmd;
  switch (cmd)
  {
  case home:
      lcd.home(); // Could remove this from here now
      break;
  case autoscroll:
      Serial_print("Autoscroll:");
      Serial.println(cmd);
      lcd.autoscroll();
      break;
  case noautoscroll:
      Serial_print("noautoscroll:");
      Serial.println(cmd);
      lcd.noAutoscroll();
      break;
  case blink:
      Serial_print("blink:");
      Serial.println(cmd);
      lcd.blink();
      break;
  case noblink:
      Serial_print("nonlink:");
      Serial.println(cmd);
      lcd.noBlink();
      break;
  case LCD1602MiscCmds_MAX:
      Serial_print("Max:");
      Serial.println(cmd);
      return (Tristate)false;
      break;
  default:
      Serial_print("Default:");
      Serial.println(cmd);
      return (Tristate)false;
      break;
  }
  return (Tristate)true;
}


// Not relevant for Grove_LCD1602
Tristate Grove_LCD1602::Backlight()
{
  return notImplemented;;
}







