#ifndef SOFTATAH
#define SOFTATAH

#define APP_VERSION "2.01"

//WiFi
#ifndef STASSID
#define STASSID "APQLZM"
#define STAPSK "silly1371"
#endif

#define PORT 4242

#define MAX_SENSOR_PROPERTIES 10

#define DEFAULT_RPIPICO_I2C0_SDA 4
#define DEFAULT_RPIPICO_IC20_SCL (DEFAULT_RPIPICO_I2C0_SDA+1)

#define GROVE_I2C0_SDA_SDA   8
#define GROVE_I2C0_SDA_SCL   (GROVE_I2C0_SDA_SDA +1)

#define GROVE_I2C1_SDA_SDA   6
#define GROVE_I2C1_SDA_SCL   (GROVE_I2C1_SDA_SDA +1)


//Maximum number of each that can be instatiated...added to list in deviceLists.h
#define MAX_SENSORS   10
#define MAX_ACTUATORS 10
#define MAX_DISPLAYS  10

#define G_USE_GROVE_RPIPICO_SHIELD

#define G_DEVICETYPES C(sensor)C(actuator)C(communication)C(display)

//Add other sensors/actuators here C bracketed, on end.
#define G_SENSORS C(DHT11)C(BME280)C(LIGHT)C(SOUND)
#define G_ACTUATORS C(BUZZER)
#define G_I2CDISPLAYS C(OLED096)C(LCD1602)

#endif