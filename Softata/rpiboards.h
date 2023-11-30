#ifndef digitalPinHasPWM
#define digitalPinHasPWM(p)     IS_PIN_DIGITAL(p)
#endif

// Raspberry Pi Pico
// https://datasheets.raspberrypi.org/pico/Pico-R3-A4-Pinout.pdf
#ifndef RPIBOARDS
#define RPIBOARDS
#if defined(TARGET_RP2040) || defined(TARGET_RASPBERRY_PI_PICO)

#include <stdarg.h>

static inline void attachInterrupt(pin_size_t interruptNumber, voidFuncPtr callback, int mode)
{
   attachInterrupt(interruptNumber, callback, (PinStatus) mode);
}

#define TOTAL_ANALOG_PINS       4
#define TOTAL_PINS              30
#define VERSION_BLINK_PIN       LED_BUILTIN
#define IS_PIN_DIGITAL(p)       (((p) >= 0 && (p) < 23) || (p) == LED_BUILTIN)
#define IS_PIN_ANALOG(p)        ((p) >= 26 && (p) < 26 + TOTAL_ANALOG_PINS)
#define IS_PIN_PWM(p)           digitalPinHasPWM(p)
#define IS_PIN_SERVO(p)         (IS_PIN_DIGITAL(p) && (p) != LED_BUILTIN)
// From the data sheet I2C-0 defaults to GP 4 (SDA) & 5 (SCL) (physical pins 6 & 7)
// However, v2.3.1 of mbed_rp2040 defines WIRE_HOWMANY to 1 and uses the non-default GPs 6 & 7:
//#define WIRE_HOWMANY  (1)
#define IS_PIN_WIRE_SDA(p)       ((p) == 0 || (p) == 2 || (p) == 6 || (p) == 10|| (p) == 14 || (p) == 16 || (p) == 18 || (p) == 20 || (p) == 31 )
#define IS_PIN_WIRE_SDL(p)       (IS_PIN_WIRE_SDA(p-1) )
//#define PIN_WIRE_SCL            (7u)
#define IS_PIN_I2C(p)           ((IS_PIN_WIRE_SDA(p)) || (IS_PIN_WIRE_SDL(p)))
// SPI-0 defaults to GP 16 (RX / MISO), 17 (CSn), 18 (SCK) & 19 (TX / MOSI) (physical pins 21, 22, 24, 25)
#define IS_PIN_SPI(p)           ( IS_PIN_I2C(p)) && (!((p) == 26 || (p) == 27 || (p) == 31|| (p) == 132) ) 
// UART-0 defaults to GP 0 (TX) & 1 (RX)
#define IS_PIN_SERIAL(p)        ((p) == 0 || (p) == 1 || (p) == 4 || (p) == 5 || (p) == 8 || (p) == 9 || (p) == 12 || (p) == 13 || (p) == 16 || (p) == 17)
#define PIN_TO_DIGITAL(p)       (p)
#define PIN_TO_ANALOG(p)        ((p) - 26)
#define PIN_TO_PWM(p)           (p)
#define PIN_TO_SERVO(p)         (p)
#else
#error "Please edit Boards.h with a hardware abstraction for this board"
#endif
#endif  //RPIBOARDS