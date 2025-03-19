#include <Arduino.h>
#include "menu.h"

#include "serial_macros.h"

// Using a previously displayed menu and return the user's choice as a single character
char GetMenuChoice(int timeout)
{
    if(Serial)
    {
      // Allow <timeout> seconds to respond
      Serial_print("\t--- " );
      Serial_print(timeout);
      Serial_print(" secs to respond.");
      Serial_println( " ---" );
      Serial_print("\t");

      int maxCount = (int) ((timeout*1000)/MENU_DELAY); 
      int secCount = (int) (1000/MENU_DELAY);
      while (Serial.available() == 0) { 
        if((maxCount-- % secCount ) == 0){
          Serial_print("|");
        }
        else{
          Serial_print(".");
        }
        delay(MENU_DELAY); 
        if(maxCount<0)break;
      }
      Serial_println();

      String resStr = Serial.readString();
      resStr.toUpperCase();
      resStr.trim();
      Serial_println();
      if (resStr.length()>0)
      {
          char resCh = resStr[0];
          return resCh;   
      }
    }
    return (char)0;
}

int  GetMenuChoiceNum( int defaultNum, int timeout)
{
  char menuCh = GetMenuChoice(timeout);
  if ( ((char)0) != menuCh)
  {
    int selection = ((int)menuCh) - ((int)'0');
    if(selection > -1)
    {
      return selection;
    }
  }
  return defaultNum;
}

//Nb: Default is N
bool  GetMenuChoiceYN( bool defaultYN, int timeout)
{
  char menuCh = GetMenuChoice(timeout);
  if ('Y' == menuCh)
  {
    return true;
  }
  else if ('N' == menuCh)
  {
    return false;
  }
  else
    return defaultYN;
}

menu3State  GetMenuChoiceYNS( menu3State defaultYNS, int timeout)
{
  char menuCh = GetMenuChoice(timeout);
  if ('Y' == menuCh)
  {
    return ttrue;
  }
  else if ('N' == menuCh)
  {
    return tfalse;
  }
  else if ('S' == menuCh)
  {
    return trueButSkipMenus;
  }
  else
    return defaultYNS;
}