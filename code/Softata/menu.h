#ifndef MENUH
#define MENUH

#define MENU_DELAY 250
#define WIFI_CONNECT_TIMEOUT_SEC 15
#define DEFAULT_MENU_TIMEOUT_SEC 2

enum menu3State {trueButSkipMenus,ttrue,tfalse};

char GetMenuChoice(int timeout = DEFAULT_MENU_TIMEOUT_SEC);
int  GetMenuChoiceNum(int defaultNum, int timeout = DEFAULT_MENU_TIMEOUT_SEC );
bool  GetMenuChoiceYN(bool defaultYN=false, int timeout = DEFAULT_MENU_TIMEOUT_SEC);
menu3State  GetMenuChoiceYNS(menu3State defaultYNS=trueButSkipMenus, int timeout = DEFAULT_MENU_TIMEOUT_SEC);

#endif