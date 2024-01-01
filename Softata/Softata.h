#ifndef SOFTATAH
#define SOFTATAH

#define APP_VERSION "2.03"

#define RPI_PICO_DEFAULT
//#define GROVE_RPI_PICO_SHIELD

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


// Note: Adresses are typically defined in the device library which typically use I2C0
#define OLEDDISPLAY_ADDR   0x78
#define LCD1602LCD_V2_ADDR 0X3E
#define LCD1602RGB_V2_ADDR 0X62

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


//Maximum number of each that can be instatianted...added to list in deviceLists.h
#define MAX_SENSORS   10
#define MAX_ACTUATORS 10
#define MAX_DISPLAYS  10


#define G_DEVICETYPES C(sensor)C(actuator)C(communication)C(display)

//Add other sensors/actuators here C bracketed, on end.
#define G_SENSORS C(DHT11)C(BME280)C(LIGHT)C(SOUND)
#define G_ACTUATORS C(BUZZER)
#define G_DISPLAYS C(OLED096)C(LCD1602)C(NEOPIXEL)

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





#endif