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
 
  u8g2.clearBuffer();
  u8g2.setFlipMode(1);                   
  u8g2.setFont(u8g2_font_ncenB08_tr);   // choose a suitable font
  //u8g2.drawStr(0,10,"Hello World!");    // write something to the internal memory
  //u8g2.sendBuffer();                    // transfer internal memory to the display
  return true;
}

bool Grove_OLED096::Setup(byte * settings, byte numSettings)
{
  if(numSettings>0)
  {
    int i2c = settings[0];
    if((i2c==0) || (i2c==1))
    {
      // Note u8g2 code fixes to I2C0!
      Grove::SetI2CPins(i2c);
      u8g2.begin();
      u8g2.setFlipMode(1);
      u8g2.clearBuffer();                   // clear the internal memory
      u8g2.setFont(u8g2_font_ncenB08_tr);   // choose a suitable font

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

bool Grove_OLED096::CursorWriteStringAvailable()
{
	return true;
}

bool Grove_OLED096::SetCursor(byte col, byte line)
{
    int y = LINEHEIGHT*line + FIRSTLINESTART;
    int x = COLWIDTH*col;
    u8g2.setCursor(x,y);
    return true;
}
bool Grove_OLED096::WriteString(String msg)
{
    //u8g2.setFont(u8g2_font_ncenB08_tr);   // choose a suitable font
    u8g2.print(msg.c_str());
    u8g2.sendBuffer();
    return true;
}

bool Grove_OLED096::WriteString (byte col, byte line, String msg)
{
  int y = LINEHEIGHT*line + FIRSTLINESTART;
  int x = COLWIDTH*col;
  //u8g2.setFont(u8g2_font_ncenB08_tr); 
  u8g2.drawStr(x,y,msg.c_str());
  u8g2.sendBuffer(); 
  return true;
}

bool Grove_OLED096::Home ()
{
  u8g2.home(); 
  return true;
}


bool Grove_OLED096::Misc(byte cmd, byte * data, byte length)
{
  if (cmd<0)
    return false;
  else if(cmd>(int) OLEDMiscCmds_MAX)
    return false;
  OLEDMiscCmds Cmd = (OLEDMiscCmds)cmd;
  switch (Cmd)
  {
    case drawCircle:
    {
      byte x = 60;
      byte y = 32;
      byte r = 20;
      if(length>2)
      {
        x = data[0];
        y = data[1];
        r = data[2];
      }
      u8g2.drawCircle(x,y,r);
      u8g2.sendBuffer();
    }
      break;
    case drawFrame:
    {
      byte x = 30;
      byte y = 5;
      byte w = 60;
      byte h = 55;
      if(length>3)
      {
        x = data[0];
        y = data[1];
        w = data[2];
        h = data[3];
      }
      u8g2.drawFrame(x,y,w,h);
      u8g2.sendBuffer();
    }
      break;
    case test:
      //Dummy test to see that Misc commands can be reached.
      break;
    case OLEDMiscCmds_MAX:
      Serial_println("MAX");
      return false;
      break;
    default:
    Serial_println("DEFAULT");
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

bool Grove_LCD1602::Clear()
{
  lcd.clear();
  return true;
}

bool Grove_LCD1602::Home()
{
    lcd.home();
    return true;
}


bool Grove_LCD1602::SetCursor(byte x, byte y)
{
  // Note: line 1 is the second row, since counting begins with 0:
  lcd.setCursor(x,y);
  return true;
}

bool Grove_LCD1602::WriteString(String msg)
{
  lcd.print(msg.c_str());
  return true;
}

bool Grove_LCD1602::CursorWriteStringAvailable()
{
	return true;
}

bool Grove_LCD1602::WriteString(byte x, byte y, String msg)
{
  // Note: line 1 is the second row, since counting begins with 0:
  lcd.setCursor(x,y);
  lcd.print(msg.c_str());
  return true;
}

bool Grove_LCD1602::Misc(byte cmd, byte * data, byte length)
{
  if (cmd<0)
    return false;
  else if(cmd>(int) LCD1602MiscCmds_MAX)
    return false;
  LCD1602MiscCmds Cmd = (LCD1602MiscCmds)cmd;
  switch (Cmd)
  {
    home:
      lcd.home(); // Could remove this from here now
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







