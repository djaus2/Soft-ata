#ifndef SOFTATAH
#define SOFTATAH

#define APP_VERSION "2.30"

//#define RPI_PICO_DEFAULT
#define GROVE_RPI_PICO_SHIELD

//WiFi
#ifndef STASSID
#define STASSID "APQLZM"
#define STAPSK "silly1371"
#endif

#define PORT 4242

#define MAX_SENSOR_PROPERTIES 10

#ifdef RPI_PICO_DEFAULT
#define DHT11Pin_D  13 // Whatever 0 to 26
#define UART9TX   0  // GPIO0, GPIO16, GPIO22
#define UART9RX   (UART0TX+1)
#define UART1TX   4  // GPIO4, GPIO8, GPIO12
#define UART1RX   (UART1TX+1)
// Default I2C is first listed pin
#define I2C0_SDA  4 // GPIO0, GPIO4, GPIO8, GPIO12, GPIO16, GPIO20
#define I2C0_SCL  (I2C0_SDA +1)
// Next default TBD:
#define I2C1_SDA  6 // GPIO2, GPIO6, GPIO10, GPIO14, GPIO18, GPIO26
#define I2C1_SCL  (I2C1_SDA +1)
#elif defined(GROVE_RPI_PICO_SHIELD)
// Note following are fixed for the Shield
#define DHT11Pin_D  16 // or 18,20
#define UART0TX   0
#define UART0RX   (UART0TX+1)
#define UART1TX   4
#define UART1RX   (UART1TX+1)
#define I2C0_SDA  8
#define I2C0_SCL  (I2C0_SDA +1)
#define I2C1_SDA  6
#define I2C1_SCL  (I2C1_SDA +1)
#endif

//Maximum number of each that can be instatianted...added to list in deviceLists.h
#define MAX_SENSORS   10
#define MAX_ACTUATORS 10
#define MAX_DISPLAYS  10

// Max number of bytes in msgs from client to service here
#define maxRecvdMsgBytes 32

#define G_DEVICETYPES C(sensor)C(actuator)C(communication)C(display)

//Add other sensors/actuators here C bracketed, on end.
#define G_SENSORS C(DHT11)C(BME280)C(LIGHT)C(SOUND)
#define G_ACTUATORS C(BUZZER)
#define G_DISPLAYS C(OLED096)C(LCD1602)C(NEOPIXEL)


///////////////////////// S E N S O R S /////////////////////////////////////////////////

enum GroveSensorCmds{s_getpinsCMD, s_getPropertiesCMD, s_setupdefaultCMD, s_setupCMD, s_readallCMD, s_readCMD, s_getSensorsCMD=255 };

#define DHT11_PROPERTIES "Temperature,Humidity"
#define BME280_PROPERTIES "Temperature,Pressure,Humidity"

#define NUM_DHT11_PROPERTIES 2
#define NUM_BME280_PROPERTIES 3

//#define BME280_I2C_ADDRESS76
#define BME280_I2C_ADDRESS77
#define BME280_ADDR        0x77  // or 0x76

//Sensor connections

#ifdef RPI_PICO_DEFAULT
#define BME280_PINNOUT  "I2C0 (Pins4/5 (SDA/SCL) default): Address 0x77 (Alt 0x76). Embedded in driver though."
#define DHT11_PINNOUT "OneWire: Pin 11 or whatever 0 to 26"
#elif defined(GROVE_RPI_PICO_SHIELD)
#define BME280_PINNOUT  "I2C0 (Pins8/9 (SDA/SCL) fixed): Address 0x77 (Alt 0x76). Embedded in driver though."
#define DHT11_PINNOUT  "OneWire: Pin 16,18 or 20"
#endif


///////////////////////// D I S P L A Y S /////////////////////////////////////////////////

enum GroveDisplayCmds{d_getpinsCMD, d_tbdCMD, d_setupDefaultCMD, d_setupCMD, d_clearCMD,d_backlightCMD,d_setCursorCMD,d_writestrngCMD,d_cursor_writestringCMD,d_miscCMD, d_getDisplaysCMD=255 };
//enum GroveDisplayCmds{d_getpinsCMD=0, d_tbdCMD=1, d_setupDefaultCMD=2, d_setupCMD=3, d_clearCMD=4,d_backlightCMD=5,d_setCursorCMD=6, d_writestrngCMD=7,d_cursor_writestringCMD=8,d_miscCMD=9, d_getDisplaysCMD=255 }

// Note: Adresses are typically defined in the device library which typically use I2C0
#define OLEDDISPLAY_ADDR   0x78
#define LCD1602LCD_V2_ADDR 0X3E
#define LCD1602RGB_V2_ADDR 0X62

#define NEOPIXEL_NUMPIXELS 8
#ifdef RPI_PICO_DEFAULT
#define NEOPIXEL_PIN 12 // Whatever 0 to 26
#elif defined(GROVE_RPI_PICO_SHIELD)
#define NEOPIXEL_PIN 16 //18,20 (Grove yellow cable).. 17,19,21 (Grove white cable)
#endif

//Display connections

#ifdef RPI_PICO_DEFAULT
#define OLED096_PINNOUT  "I2C0 (Pins4/5 (SDA/SCL) default): Address 0x78. Embedded in driver though."
#define LCD1602_PINNOUT  "I2C0 (Pins4/5 (SDA/SCL) default): Addresses (LCD)0X3E and (RGB)0X62. Embedded in driver though."
#define NEOPIXEL_PINNOUT "OneWrire: Pin 12 // Whatever 0 to 26"
#elif defined(GROVE_RPI_PICO_SHIELD)
#define OLED096_PINNOUT  "I2C0 (Pins8/9 (SDA/SCL)fixed): Address 0x78. Embedded in driver though."
#define LCD1602_PINNOUT  "I2C0 (Pins8/9 (SDA/SCL) fixed): Addresses (LCD)0X3E and (RGB)0X62. Embedded in driver though."
#define NEOPIXEL_PINNOUT "OneWire: Pin 16 Alt 18 or 20 (Grove yellow cable).. 17,19,21 (Grove white cable)"
#endif


// Add Misc commands to list here, before last one.
// Implement in Grove_XXXX::Misc(int cmd, int * data, int length)
enum LCD1602MiscCmds {home,autoscroll,noautoscroll,blink,noblink,LCD1602MiscCmds_MAX};
enum NEOPIXELMiscCmds {setpixelcolor,setpixelcolorAll,setpixelcolorOdds,setpixelcolorEvens,setBrightness,NEOPIXELMiscCmds_MAX};
enum OLEDMiscCmds {drawCircle,drawFrame,OLEDMiscCmds_MAX};


#endif