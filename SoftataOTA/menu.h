#ifndef MENUH
#define MENUH

#define MENU_DELAY 250
#define DEFAULT_MENU_TIMEOUT_SEC 5

char GetMenuChoice(int timeout = DEFAULT_MENU_TIMEOUT_SEC);
int  GetMenuChoiceNum(int defaultNum, int timeout = DEFAULT_MENU_TIMEOUT_SEC );
bool  GetMenuChoiceYN(bool defaultY=false, int timeout = DEFAULT_MENU_TIMEOUT_SEC);

#endif