#include <Arduino.h>
#include "menu.h"

// Using a previously displayed menu and return the user's choice as a single character
char GetMenuChoice(int timeout)
{
    if(Serial)
    {
      // Allow <timeout> seconds to respond
      Serial.print("   --- " );
      Serial.print(timeout);
      Serial.print(" secs to respond.");
      Serial.println( " ---" );

      int maxCount = (int) ((timeout*1000)/MENU_DELAY); 
      
      int count=0;
      while (Serial.available() == 0) { delay(MENU_DELAY);count++; if(count>maxCount)break;}

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

int  GetMenuChoiceNum(int timeout)
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
  return -1;
}

//Nb: Default is N
bool  GetMenuChoiceYN(int timeout)
{
  char menuCh = GetMenuChoice(timeout);
  if ('Y' == menuCh)
  {
    return true;
  }
  return false;
}