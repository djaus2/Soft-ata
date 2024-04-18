#ifndef SOFTATAH
#define SOFTATAH

// Core1 -> Core2 Synched Commands
enum SyncedCommands : byte {pauseTelemetryorBT=0,continueTelemetryorBT=1,stopTelemetryorBT=2,svrConnected=10, initialSynch=137};
#define SynchMultiplier 1000

#define APP_VERSION "6.78"  //Nb: As SoftataLib is updated, this is incremented even if no changes to Arduino code.

//#define RPI_PICO_DEFAULT
#define GROVE_RPI_PICO_SHIELD

//WiFi
#ifndef STASSID
#define STASSID "APQLZM"
#define STAPSK "silly1371"
#endif

#define IOT_CONFIG_WIFI_SSID STASSID
#define IOT_CONFIG_WIFI_PASSWORD STAPSK 

// Azure IoT
// Uncomment following if using Azure IoT Hub
// For Bluetooth do not define USINGIOTHUB
//#define USINGIOTHUB




#define IOT_CONFIG_IOTHUB_FQDN  "<hubname>.azure-devices.net"
#define IOT_CONFIG_DEVICE_ID  "<devicename>"
#define IOT_CONFIG_DEVICE_KEY  "<device connection key>" 


enum bitStuffingIndex: byte {e_cmd,e_pin,e_param,e_other,e_otherDataCount,e_data1,e_data2, e_num};
static int bitStuffing[] = {256,16,16,16,16,256,16};


//Server Port
#define PORT 4242

// Inbuilt Blink ON Period
#define UNCONNECTED_BLINK_ON_PERIOD 4000
// Connected blink rate is 1/4 of this.

#define MAX_SENSOR_PROPERTIES 10

#ifdef RPI_PICO_DEFAULT
#define DHT11Pin_D  13 // Whatever 0 to 26
#define UART0TX   0  // GPIO0, GPIO16, GPIO22
#define UART0RX   (UART0TX+1)
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

//Maximum number of each that can be instantiated...added to list in deviceLists.h
#define MAX_SENSORS   10
#define MAX_ACTUATORS 10
#define MAX_DISPLAYS  10

// Max number of bytes in msgs from client to service here
#define maxRecvdMsgBytes 32

#define G_DEVICETYPES C(sensor)C(actuator)C(communication)C(display)C(serial)

//Add other sensors/actuators here C bracketed, on end.
#define G_SENSORS C(DHT11)C(BME280)C(UltrasonicRanger)
#define G_ACTUATORS C(SERVO)
#define G_DISPLAYS C(OLED096)C(LCD1602)C(NEOPIXEL)C(BARGRAPH)
#define G_SERIAL C(LOOPBACK)C(GPS)

///////////////////////// A C T U A T O R S /////////////////////////////////////////////////

enum GroveActuatorCmds{a_getpinsCMD, a_getValueRangeCMD, a_setupDefaultCMD, a_setupCMD, a_writeDoubleValueCMD, a_writeByteValueCMD, a_getActuatorsCMD=255 };

#ifdef RPI_PICO_DEFAULT
#define SERVO_PINNOUT "GPIO: Pin 11 or whatever 0 to 26"
#elif defined(GROVE_RPI_PICO_SHIELD)
#define SERVO_PINNOUT "Pin 16 (default), 18 or 20"
#endif

#define DEFAULT_SERVO_PIN 16
#define SERVO_RANGE "0...180 Angle" 

///////////////////////// S E N S O R S /////////////////////////////////////////////////

enum GroveSensorCmds{s_getpinsCMD, s_getPropertiesCMD, s_setupdefaultCMD, s_setupCMD, s_readallCMD, s_readCMD,
                     s_getTelemetry,s_sendTelemetryBT, s_sendTelemetryToIoTHub, s_pause_sendTelemetry, s_continue_sendTelemetry,
                     s_stop_sendTelemetry, s_getSensorsCMD=255 };

// Adds about 1sec to telemtry transmit (in second core).
#define TELEMETRY_DOUBLE_FLASH_INBUILT_LED

#define DHT11_PROPERTIES "Temperature,Humidity"
#define BME280_PROPERTIES "Temperature,Pressure,Humidity"
#define URANGE_PROPERTIES "mm,cm,inches"

#define NUM_DHT11_PROPERTIES 2
#define NUM_BME280_PROPERTIES 3
#define NUM_URANGE_PROPERTIES 3

//#define BME280_I2C_ADDRESS76
#define BME280_I2C_ADDRESS77
#define BME280_ADDR        0x77  // or 0x76

//Sensor connections

#ifdef RPI_PICO_DEFAULT
#define BME280_PINNOUT  "I2C0 (Pins4/5 (SDA/SCL) default): Address 0x77 (Alt 0x76). Embedded in driver though."
#define DHT11_PINNOUT "OneWire: Pin 11 or whatever 0 to 26"
#define URANGE_PINNOUT "Digital and PWM: Pin 11 or whatever 0 to 26"
#elif defined(GROVE_RPI_PICO_SHIELD)
#define BME280_PINNOUT  "I2C0 (Pins8/9 (SDA/SCL) fixed): Address 0x77 (Alt 0x76). Embedded in driver though."
#define DHT11_PINNOUT  "OneWire: Pin 16 (default), 18 or 20"
#define URANGE_PINNOUT "Digital and PWM: Pin 16 (default), 18 or 20"
#endif

#define DEFAULT_DHT11_PIN 16
#define DEFAULT_URANGE_PIN 16



///////////////////////// D I S P L A Y S /////////////////////////////////////////////////

enum GroveDisplayCmds{d_getpinsCMD, d_tbdCMD, d_setupDefaultCMD, d_setupCMD, d_clearCMD,d_backlightCMD,d_setCursorCMD,
                                    d_writestrngCMD,d_cursor_writestringCMD, d_home, d_miscCMD, d_dispose, d_getDisplaysCMD=255 };
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

#define maxNumDisplaySettings 4

//Display connections

#ifdef RPI_PICO_DEFAULT
#define OLED096_PINNOUT  "I2C0 (Pins4/5 (SDA/SCL) default): Address 0x78. Embedded in driver though."
#define LCD1602_PINNOUT  "I2C0 (Pins4/5 (SDA/SCL) default): Addresses (LCD)0X3E and (RGB)0X62. Embedded in driver though."
#define NEOPIXEL_PINNOUT "OneWrire: Pin 12 // Whatever 0 to 26"
#elif defined(GROVE_RPI_PICO_SHIELD)
#define OLED096_PINNOUT  "I2C0 (Pins8/9 (SDA/SCL)fixed): Address 0x78. Embedded in driver though."
#define LCD1602_PINNOUT  "I2C0 (Pins8/9 (SDA/SCL) fixed): Addresses (LCD)0X3E and (RGB)0X62. Embedded in driver though."
#define NEOPIXEL_PINNOUT "OneWire: Pin 16 (default) Alt 18 or 20 (Grove yellow cable).. 17,19,21 (Grove white cable)"
#define BARGRAPH_PINNOUT "16 (DS of 74HC595-Pin14),20 (ST_CP of 74HC595-Pin12),21 (SH_CP of 74HC595-Pin11)(default)"
#endif


// Add Misc commands to list here, before last one.
// Implement in Grove_XXXX::Misc(int cmd, int * data, int length)
enum LCD1602MiscCmds {home,autoscroll,noautoscroll,blink,noblink,LCD1602MiscCmds_MAX};
enum NEOPIXELMiscCmds {setpixelcolor,setpixelcolorAll,setpixelcolorOdds,setpixelcolorEvens,setBrightness,setN,NEOPIXELMiscCmds_MAX};
enum OLEDMiscCmds {drawCircle,drawFrame,test,OLEDMiscCmds_MAX};
enum BARGRAPHMiscCmds {flow,flow2,BARGRAPHMiscCmds_MAX};

enum range_units {mm,cm,inch};

///////////////////////////////////


typedef String(*call_back)(void);


struct CallbackInfo
{
  unsigned long next;
  unsigned long period;
  int SensorIndex;
  bool isSensor; // False for blinking LED
  bool sendBT;   //Alt: IoT Hub, coming
  bool isRunning; // Can stop and continue Telemetry
  call_back back;
};

int AddCallBack(CallbackInfo * info);
bool PauseTelemetrySend(int index);
bool ContinueTelemetrySend(int index);

#endif
