#ifndef GROVE_DISPLAYHI2CDISPLAYS
#define GROVE_DISPLAYH
#include "grove.h"

#define C(x) x,
enum GroveDisplay { I2CDISPLAYS I2CDISPLAY_NONE};
#undef C
#define C(x) #x,    
const char * const display_name[] = { I2CDISPLAYS };
#undef C

class Grove_Display: public Grove
{
    public:
      static String GetListof()
      {
        String list ="I2CDisplays:";
        int numDISPLAYS = (int) I2CDISPLAY_NONE;
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
        int numDisplays = (int) I2CDISPLAY_NONE;
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
      virtual bool Setup(int * settings, int numSettings);
      // Index for if there are an array of actuators here.
      virtual bool Clear();
      virtual bool Backlight();
      virtual bool SetCursor(int x, int y);
      virtual bool WriteString(String msg);
      virtual bool WriteString(int x, int y, String msg);
      virtual bool Misc(int cmd, int * data);

      DeviceType deviceType = display;
    protected:
};
#endif

#ifndef OLED096
#define OLED096


class Grove_OLED096: public Grove_Display
{
      virtual bool Setup();
      virtual bool Setup(int * settings, int numSettings);
      virtual bool Clear();
      virtual bool WriteString(int col, int line, String msg);
      bool Misc(int cmd, int * data);
};

#endif

#ifndef LCD1602
#define LCD1602



class Grove_LCD1602: public Grove_Display
{
      virtual bool Setup();
      virtual bool Setup(int * settings, int numSettings);
      // Index for if there are an array of actuators here.
      virtual bool Clear();
      virtual bool SetCursor(int x, int y);
      virtual bool WriteString(String msg);
      virtual bool WriteString(int x, int y, String msg);
      virtual bool Misc(int cmd, int * data);
};

#endif
