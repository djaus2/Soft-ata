#ifndef SERIAL_MACROS_H
#define SERIAL_MACROS_H

// Enable only one (or none) of the following: 
// Normally just use the following (Debug mode)
#ifdef SOFTATA_DEBUG_MODE
 #define USE_USB_SERIAL
#else
// #define INSTEAD_OF_USB_SERIAL_DELAY 10
// Don't use the next (3) if used in Softata tests.
// For example if doing Serial Loopback with Serial1 don't #define INSTEAD_OF_USB_USE_SERIAL1 ...
// If send Telemetry via Bluettooth don't #define INSTEAD_OF_USB_USE_BTSERIAL etc
// #define INSTEAD_OF_USB_USE_SERIAL1
// #define INSTEAD_OF_USB_USE_SERIAL2     // ??This failes?? Device boots then USB potrt fails. Serial1 is OK though
// #define INSTEAD_OF_USB_USE_BTSERIAL
#endif

#ifdef USE_USB_SERIAL
#define Serial_begin(...) Serial.begin(__VA_ARGS__)
#define ifSerial()   if(Serial)
#define whileNotSerial() while(!Serial)
#define Serial_print(...) Serial.print(__VA_ARGS__)
#define Serial_printf(...) Serial.printf(__VA_ARGS__)
#define Serial_println(...) Serial.println(__VA_ARGS__)
#elif INSTEAD_OF_USB_SERIAL_DELAY
#define Serial_begin(...) {delay(INSTEAD_OF_USB_SERIAL_DELAY);}
#define ifSerial()  {delay(INSTEAD_OF_USB_SERIAL_DELAY);}if(true)
#define whileNotSerial()  {delay(INSTEAD_OF_USB_SERIAL_DELAY);}while(false)
#define Serial_print(...) {delay(INSTEAD_OF_USB_SERIAL_DELAY);}
#define Serial_printf(...) {delay(INSTEAD_OF_USB_SERIAL_DELAY);}
#define Serial_println(...) {delay(INSTEAD_OF_USB_SERIAL_DELAY);}
#elif defined(INSTEAD_OF_USB_USE_SERIAL1)
// UART0RX etc defined in softata.h
#define Serial_begin(...) {Serial1.setRX(UART0RX);Serial1.setTX(UART0TX);}Serial1.begin(__VA_ARGS__)
#define ifSerial(...)   if(Serial1)
#define whileNotSerial() while(!Serial1)
#define Serial_print(...) Serial1.print(__VA_ARGS__)
#define Serial_printf(...) Serial1.printf(__VA_ARGS__)
#define Serial_println(...) Serial1.println(__VA_ARGS__)
#elif defined(INSTEAD_OF_USB_USE_SERIAL2)
// UART1RX etc defined in softata.h
#define Serial_begin(...) {Serial1.setRX(UART1RX);Serial1.setTX(UART1TX);}Serial1.begin(__VA_ARGS__)
#define ifSerial(...)   if(Serial1)
#define whileNotSerial(...) while(!Serial1)
#define Serial_print(...) Serial1.print(__VA_ARGS__)
#define Serial_printf(...) Serial1.printf(__VA_ARGS__)
#define Serial_println(...) Serial1.println(__VA_ARGS__)
#elif defined(INSTEAD_OF_USB_USE_BTSERIAL)
#include <SerialBT.h>
// Nb: Start this Sketch. Connect from BT Terminal and press [Enter] there.
#define Serial_begin(...) {if(!SerialBT){SerialBT.begin();while (!SerialBT);while(!SerialBT.available());}}
#define ifSerial(...)   if(SerialBT)
#define whileNotSerial() while (!SerialBT)
#define Serial_print(...) SerialBT.print(__VA_ARGS__)
#define Serial_printf(...) SerialBT.printf(__VA_ARGS__)
#define Serial_println(...) SerialBT.println(__VA_ARGS__)
#else
#define Serial_begin(...) {}
#define ifSerial(...)  if(false)
#define whileNotSerial(...)  while(false)
#define Serial_print(...) {}
#define Serial_printf(...) {}
#define Serial_println(...) {}
#endif

#endif