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

//////////////////////////////////////////////////////////////////
// Grove OLED 0.96" Display
// https://wiki.seeedstudio.com/Grove-OLED_Display_0.96inch/
//////////////////////////////////////////////////////////////////
// Install UBg2 by Oliver Olikraus
// https://github.com/olikraus/u8g2
//////////////////////////////////////////////////////////////////

#define FIRSTLINESTART 10
#define LINEHEIGHT 10
#define COLWIDTH 8

// Add Misc commands to list here, before last one.
// Implement in Grove_OLED096::Misc(int cmd


U8G2_SSD1306_128X64_NONAME_F_HW_I2C u8g2(U8G2_R0, /* clock=*/ SCL, /* data=*/ SDA, /* reset=*/ U8X8_PIN_NONE);  // High speed I2C

// U8G2_SSD1306_128X64_NONAME_F_SW_I2C u8g2(U8G2_R0, /* clock=*/ SCL, /* data=*/ SDA, /* reset=*/ U8X8_PIN_NONE);    //Low spped I2C


bool Grove_OLED096::Setup() {
  Grove::SetI2CPins(0);
  u8g2.begin();
  return true;
}

bool Grove_OLED096::Setup(int * settings, int numSettings)
{
  if(numSettings>0)
  {
    int i2c = settings[0];
    if((i2c==0) || (i2c==1))
    {
      // Note u8g2 code fixes to I2C0!
      Grove::SetI2CPins(i2c);
      u8g2.begin();
      return true;
    }
  }
  return false;
}


bool Grove_OLED096::Clear()
{
  u8g2.clearBuffer();
  return true;
}

bool Grove_OLED096::WriteString (int col, int line, String msg)
{
  int y = LINEHEIGHT*line + FIRSTLINESTART;
  int x = COLWIDTH*col;
  u8g2.drawStr(x,y,msg.c_str());
  u8g2.sendBuffer(); 
  return true;
}


bool Grove_OLED096::Misc(int cmd, int * data, int length)
{
  if (cmd<0)
    return false;
  else if(cmd>(int) OLEDMiscCmds_MAX)
    return false;
  OLEDMiscCmds Cmd = (OLEDMiscCmds)cmd;
  switch (Cmd)
  {
    drawCircle:
      u8g2.drawCircle(60,32,20);
      u8g2.sendBuffer();
      break;
    drawFrame:
      u8g2.drawFrame(30,5,60,55);
      u8g2.sendBuffer();
      break;
    OLEDMiscCmds_MAX:
      return false;
      break;
    default:
      return false;
      break;
  }
  return true;
}


// Not relevant for Grove_OLED096
bool Grove_OLED096::Backlight()
{
    return false;
}
bool Grove_OLED096::SetCursor(int x, int y)
{
    return false;
}
bool Grove_OLED096::WriteString(String msg)
{
    return false;
}




/////////////////////////////////////////////////////////////////////
// Grove-LCD RGB Backlight  V2.00
// https://wiki.seeedstudio.com/Grove-LCD_RGB_Backlight/
/////////////////////////////////////////////////////////////////////
// Install Grove-LCD RGB Backlight by Seeed Studio
// https://github.com/Seeed-Studio/Grove_LCD_RGB_Backlight
/////////////////////////////////////////////////////////////////////

#include "rgb_lcd.h"

rgb_lcd lcd;

// Default backlight
#define COLORRED  255
#define COLORGREEN  0
#define COLORBLUE  0



bool Grove_LCD1602::Setup()
{
  Grove::SetI2CPins(0);
  lcd.begin(16, 2);
  lcd.setRGB(COLORRED, COLORGREEN, COLORBLUE);
  return true;
}

bool Grove_LCD1602::Setup(int * settings, int numSettings)
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

bool Grove_LCD1602::Clear()
{
  lcd.clear();
  return true;
}


bool Grove_LCD1602::SetCursor(int x, int y)
{
  // Note: line 1 is the second row, since counting begins with 0:
  lcd.setCursor(x,y+1);
  return true;
}

bool Grove_LCD1602::WriteString(String msg)
{
  lcd.print(msg.c_str());
  return true;
}

bool Grove_LCD1602::WriteString(int x, int y, String msg)
{
  // Note: line 1 is the second row, since counting begins with 0:
  lcd.setCursor(x,y+1);
  lcd.print(msg.c_str());
  return true;
}

bool Grove_LCD1602::Misc(int cmd, int * data, int length)
{
  if (cmd<0)
    return false;
  else if(cmd>(int) LCD1602MiscCmds_MAX)
    return false;
  LCD1602MiscCmds Cmd = (LCD1602MiscCmds)cmd;
  switch (Cmd)
  {
    home:
      lcd.home();
      break;
    autoscroll:
      lcd.autoscroll();
      break;
    noautoscroll:
      lcd.noAutoscroll();
      break;
    blink:
      lcd.blink();
      break;
    nonlink:
      lcd.noBlink();
      break;
    LCD1602MiscCmds_MAX:
      return false;
      break;
    default:
      return false;
      break;
  }
  return true;
}


// Not relevant for Grove_LCD1602
bool Grove_LCD1602::Backlight()
{
    return false;
}







