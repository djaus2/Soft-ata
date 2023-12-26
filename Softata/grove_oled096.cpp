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

#include "grove_display.h"



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
    int num = settings[0];
    if((num==0) || (num==1))
    {
      Grove::SetI2CPins(num);
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
  int y = lineHeight*line + line1VerticalStartPixel;
  int x = charWidth*col;
  u8g2.drawStr(x,y,msg.c_str());
  u8g2.sendBuffer(); 
  return true;
}


bool Grove_OLED096::Misc(int cmd, int * data)
{
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
    default:
      return false;
  }
  return true;
}




