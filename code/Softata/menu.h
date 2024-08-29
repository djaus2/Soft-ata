#ifndef MENUH
#define MENUH

#define MENU_DELAY 250
#define WIFI_CONNECT_TIMEOUT_SEC 15
#define DEFAULT_MENU_TIMEOUT_SEC 5

enum tristate {trueButSkipMenus,ttrue,tfalse};

char GetMenuChoice(int timeout = DEFAULT_MENU_TIMEOUT_SEC);
int  GetMenuChoiceNum(int defaultNum, int timeout = DEFAULT_MENU_TIMEOUT_SEC );
bool  GetMenuChoiceYN(bool defaultYN=false, int timeout = DEFAULT_MENU_TIMEOUT_SEC);
tristate  GetMenuChoiceYNS(tristate defaultYNS=trueButSkipMenus, int timeout = DEFAULT_MENU_TIMEOUT_SEC);

#endif