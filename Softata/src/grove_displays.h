#ifndef GROVE_DISPLAYHI2CDISPLAYS
#define GROVE_DISPLAYH
#include "grove.h"

#define C(x) x,
enum GroveDisplay { DISPLAYS DISPLAY_NONE};
#undef C
#define C(x) #x,    
const char * const display_name[] = { DISPLAYS };
#undef C





// Add Misc commands to list here, before last one.
// Implement in Grove_XXXX::Misc(int cmd, int * data, int length)
enum LCD1602MiscCmds {home,autoscroll,noautoscroll,blink,noblink,LCD1602MiscCmds_MAX};
enum NEOPIXELMiscCmds {setpixelcolor,setpixelcolorAll,setpixelcolorOdds,setpixelcolorEvens,setBrightness,NEOPIXELMiscCmds_MAX};
enum OLEDMiscCmds {drawCircle,drawFrame,OLEDMiscCmds_MAX};

class Grove_Display: public Grove
{
    public:
      static String GetListof()
      {
        String list ="Displays:";
        int numDISPLAYS = (int) DISPLAY_NONE;
        for (int n=0;n<numDISPLAYS;n++)
        {
          list.concat(display_name[n]);
          if (n != (numDISPLAYS-1))
            list.concat(',');
        }
        return list;
      }

      static int GetIndexOf(String displayName)
      {
        int numDisplays = (int) DISPLAY_NONE;
        for (int n=0;n<numDisplays;n++)
        {
          GroveDisplay s = (GroveDisplay)n;
          String name = String(display_name[s]);
          if (displayName.compareTo(name)==0)
          {
            return (int)s;
          }
        }
        return INT_MAX;
      }      

      virtual String GetListofx()
      {
        return Grove_Display::GetListof();
      }

  
      virtual bool Setup();
      virtual bool Setup(int * settings, int numSettings=1);
      // Index for if there are an array of actuators here.
      virtual bool Clear();
      virtual bool Backlight();
      virtual bool SetCursor(int x, int y);
      virtual bool WriteString(String msg);
      virtual bool WriteString(int x, int y, String msg);
      virtual bool Misc(int cmd, int * data, int length=0);

      DeviceType deviceType = display;
    protected:
};
#endif

#ifndef OLED096H
#define OLED096H


class Grove_OLED096: public Grove_Display
{
  public:
      static String GetPins()
      {
        String msg="OK:";
        msg.concat(OLED096_PINNOUT);
        return msg;
      }
      virtual bool Setup();
      virtual bool Setup(int * settings, int numSettings);
      virtual bool Clear();

      virtual bool Backlight();
      virtual bool SetCursor(int x, int y);
      virtual bool WriteString(String msg);

      virtual bool WriteString(int col, int line, String msg);
      bool Misc(int cmd, int * data, int length);
  protected:
};

#endif

#ifndef LCD1602H
#define LCD1602H



class Grove_LCD1602: public Grove_Display
{
  public:
      static String GetPins()
      {
        String msg="OK:";
        msg.concat(LCD1602_PINNOUT);
        return msg;
      }
      virtual bool Setup();
      virtual bool Setup(int * settings, int numSettings);
      // Index for if there are an array of actuators here.
      virtual bool Clear();

      virtual bool Backlight();

      virtual bool SetCursor(int x, int y);
      virtual bool WriteString(String msg);
      virtual bool WriteString(int x, int y, String msg);
      virtual bool Misc(int cmd, int * data, int length);
  protected:
};

#endif

#ifndef NEOPIXEL8H
#define NEOPIXEL8H



class Adafruit_NeoPixel8: public Grove_Display
{
  public:
      static String GetPins()
      {
        String msg="OK:";
        msg.concat(NEOPIXEL_PINNOUT);
        return msg;
      }
      virtual bool Setup();
      virtual bool Setup(int * settings, int numSettings);
      // Index for if there are an array of actuators here.
      virtual bool Clear();
      virtual bool Misc(int cmd, int * data, int length);

      virtual bool Backlight();
      virtual bool SetCursor(int x, int y);
      virtual bool WriteString(String msg);
      virtual bool WriteString(int x, int y, String msg); 

      int numPixels = NEOPIXEL_NUMPIXELS;
      int grovePin = NEOPIXEL_PIN;
      DeviceType deviceType = display;
    protected:
};
#endif
