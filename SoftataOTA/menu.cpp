#include <Arduino.h>
#include "menu.h"

// Using a previously displayed menu and return the user's choice as a single character
char GetMenuChoice(int timeout)
{
    if(Serial)
    {
      // Allow <timeout> seconds to respond
      Serial.print("\t--- " );
      Serial.print(timeout);
      Serial.print(" secs to respond.");
      Serial.println( " ---" );
      Serial.print("\t");

      int maxCount = (int) ((timeout*1000)/MENU_DELAY); 
      
      int count=0;
      while (Serial.available() == 0) { Serial.print('-');delay(MENU_DELAY);count++; if(count>maxCount)break;}
      Serial.println();

      String resStr = Serial.readString();
      resStr.toUpperCase();
      resStr.trim();
      Serial.println();
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
bool  GetMenuChoiceYN( bool defaultY, int timeout)
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
    return defaultY;
}