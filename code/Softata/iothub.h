#ifndef IOTHUBH
#define IOTHUBH

///////////////////////// C O N N E C T I O N  &  S E T U P /////////////

// C99 libraries
#include <cstdlib>
#include <stdbool.h>
#include <string.h>
#include <time.h>

// Libraries for MQTT client, WiFi connection and SAS-token generation.

#include <WiFi.h>

#include <PubSubClient.h>
#include <WiFiClientSecure.h>
#include <base64.h>
#include <bearssl/bearssl.h>
#include <bearssl/bearssl_hmac.h>
#include <libb64/cdecode.h>

// Azure IoT SDK for C includes
#include <az_core.h>
#include <az_iot.h>
#include <azure_ca.h>
#include "NetworkSettings.h"
#include "Connect2Wifi.h"

// Additional sample headers
//#include "iot_configs.h"

// When developing for your own Arduino-based platform,
// please follow the format '(ard;<platform>)'. (Modified this)
#define AZURE_SDK_CLIENT_USER_AGENT "c%2F" AZ_SDK_VERSION_STRING "(ard;rpipico)"

// Utility macros and defines
#define sizeofarray(a) (sizeof(a) / sizeof(a[0]))
#define ONE_HOUR_IN_SECS 3600
#define NTP_SERVERS "pool.ntp.org", "time.nist.gov"
#define MQTT_PACKET_SIZE 1024

// Translate iot_configs.h defines into variables used by the sample
//static const char* ssid = IOT_CONFIG_WIFI_SSID;
//static const char* password = IOT_CONFIG_WIFI_PASSWORD;
static const char* host = "IOT_CONFIG_IOTHUB_FQDN"; 
static const char* device_id = IOT_CONFIG_DEVICE_ID;
static const char* device_key = IOT_CONFIG_DEVICE_KEY;
static const int mqttPort = 8883;



// Memory allocated for the sample's variables and structures.
static WiFiClientSecure secure_wifi_client;
static X509List cert((const char*)ca_pem);
static PubSubClient mqtt_client(secure_wifi_client);
static az_iot_hub_client client;
static char sas_token[200];
static uint8_t signature[512];
static unsigned char encrypted_signature[32];
static char base64_decoded_device_key[32];
static unsigned long next_telemetry_send_time_ms = 0;
static char telemetry_topic[128];
static uint8_t telemetry_payload[100];
static uint32_t telemetry_send_count = 0;

// Auxiliary functions

static void connectToWiFi()
{
  Serial_println();
  Serial_print("Connecting to WIFI SSID ");
  Serial_println(ssid);

  WiFi.mode(WIFI_STA);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial_print("+");
  }

  Serial_print("Core 2 WiFi connected, IP address: ");
  Serial_println(WiFi.localIP());
}

static void initializeTime()
{
  Serial_print("Setting time using SNTP");

  configTime(-5 * 3600, 0, "pool.ntp.org","time.nist.gov"); 

  time_t now = time(NULL);
  while (now < 1510592825)
  {
    delay(500);
    Serial_print(".");
    now = time(NULL);
  }
  Serial_println("done!");
}

static char* getCurrentLocalTimeString()
{
  time_t now = time(NULL);
  return ctime(&now);
}

static void printCurrentTime()
{
  Serial_print("Current time: ");
  Serial_print(getCurrentLocalTimeString());
}

enum Messages {msgtelemetryStart,msgtelemetryStop,msgtelemetryReset,msgtelemetrySet,msgtelemetryNone};

void receivedCallback(char* topic, byte* payload, unsigned int length)
{
  String _Telemetry = "Telemetry";
  String _Pause = "Pause";
  String _Continue = "Continue";
  String _Stop = "Stop";
  String _Actuator = "Actuator";
  String _Raw ="Raw";

  Serial_print("Received [");
  Serial_print(topic);
  Serial_println("]: ");
  for (int i = 0; i < length; i++)
  {
    Serial_print((char)payload[i]);
  }
  Serial_println();
  
  char cmd[length+1];
  char pin[length+1];
  char param[length+1];
  char other[length+1];
  char dataLength[length+1];
  char data1[length+1];
  char data2[length+1];
  cmd[0] ='\0';
  pin[0] ='\0';
  param[0] = '\0';
  other[0] ='\0';
  dataLength[0] = '\0';
  data1[0] = '\0';
  data2[0] = '\0';
  byte bCmd = 0xff;
  byte bPin = 0xff;
  byte bParam = 0xff;
  byte bOther = 0xff;
  byte bDataLength = 0;
  byte bData1 = 0xff;
  byte bData2 = 0xff;

  int state = 0;
  int indx=0;
  for (int i = 0; i < length; i++)
  {
    //Permit space or comma separators
    if ((payload[i]==(byte)' ')||(payload[i]==(byte)','))
    {
      if(i>0)
      {
        //Allow/Combine multiple space/comma separators.
        if(payload[i-1]==payload[i])
          continue;
      }
      state++;
      indx=0;
      if(state > 6)
        break;
    }
    else
    {
      switch (state)
      {
        case 0:
          cmd[indx] = (char) payload[i];
          cmd[indx+1]='\0';
          break;
        case 1:
          pin[indx] = (char) payload[i];
          pin[indx+1]='\0';
          break;
        case 2:
          param[indx] = (char) payload[i];
          param[indx+1]='\0';
          break;
        case 3:
          other[indx] = (char) payload[i];
          other[indx+1]='\0';
          break;
        case 4:
          dataLength[indx] = (char) payload[i];
          dataLength[indx+1]='\0';
          break;
        case 5:
          data1[indx] = (char) payload[i];
          data1[indx+1]='\0';
          break;
        case 6:
          data2[indx] = (char) payload[i];
          data2[indx+1]='\0';
          break;
        default:
          break;
      }
      indx++;
    }
  }
  Serial_println("");

  String Cmd = String(cmd);
  String Pin = String(pin);
  String Param = String(param);
  String Other = String(other);
  String DataLength = String(dataLength);
  String Data1 = String(data1);
  String Data2 = String(data2);



  if(Cmd.length()>0)
  {
    //Check if first entity is a byte number. If so raw entries
    int val = Cmd.toInt();
    if ((val>0)&& (val<256))
    {
      Cmd="Raw";
      bCmd = (byte)val;
      if(Param.length()>0)
      {
        //Also Check if first para is a byte number. 
        int val = Param.toInt();
        if ((val>0)&& (val<256))
        {
          bParam = (byte)val;
        }
        else
        {
          bParam = 0;
        }
      }
    }
  }

  


  if(Pin.length()>0)
  {
    int val = Pin.toInt();
    if(Pin=="0")
      bPin = 0;
    else if (val!= 0)
    {
      bPin = (byte)val;
    }
  }

  if(Other.length()>0)
  {
    int val = Other.toInt();
    if(Other=="0")
      bOther = 0;
    else if (val!= 0)
    {
      bOther = (byte)val;
    }
  }

  if(DataLength.length()>0)
  {
    if(Data1.length()>0)
    {
      bDataLength = DataLength.toInt();
      if(bDataLength == 1)//Only allowing one byte of data thus
      {
        int val =  Data1.toInt();
        if((val>=0) && (val<=0xff))
        {
          bData1 = (byte)val;
          val =  Data2.toInt();
          if((val>=0) && (val<=0xff))
          {
            bData2 = (byte)val;
          }
        }
        else
        {
          bDataLength = 0;
        }
      }
      else
      {
        bDataLength = 0;
      }
    }
  }

  if(Cmd.equalsIgnoreCase(_Telemetry))
  {
    bCmd = 0xf0;
  }
  else if(Cmd.equalsIgnoreCase(_Actuator))
  { //Coming
    bCmd = 0xf2;
  }
  else if(Cmd.equalsIgnoreCase(_Raw))
  {
    //bCmd already parsed
  }
  else
  {
    Serial_print("Other Device");
    return;
  }

  Serial_print("cmd:");
  Serial_print(Cmd);
  Serial_print('=');
  Serial_print(bCmd,HEX);

  if (bPin!= 0xff)
  {
    Serial_print(" pin=0x");
    Serial_print(bPin,HEX);
  }

  if(!Cmd.equalsIgnoreCase(_Raw))
  {
    bParam=0;
    if(Param != "" ) 
    {
      if (Param.equalsIgnoreCase(_Pause))
      {
        bParam =s_pause_sendTelemetry;
      }
      else if (Param.equalsIgnoreCase(_Continue))
      {
        bParam =s_continue_sendTelemetry;
      }
      else if (Param.equalsIgnoreCase(_Stop))
      {
        bParam =s_stop_sendTelemetry;
      }
      else
      {
        Serial_println(" - No parameter");
      }
    }
    else
    {
      Serial_println(" - No parameter");
    }
  }
 
  Serial_print(" param:");
  Serial_print(Param);
  Serial_print("=0x");
  Serial_print(bParam,HEX);

  if (bOther != 0xff)
  {
    Serial_print(" other=0x");
    Serial_print(bOther,HEX);
  }

  if ((bDataLength>0) && (bDataLength <3)) //Only allowing one byte of data thus
  {
    if(bData1 != 0xff)
    {
      Serial_print(" otherData:");
      Serial_print('[');
      Serial_print(bDataLength);
      Serial_print(",");
      Serial_print(bData1,HEX);
      if(bData2 != 0xff)
      {
        Serial_print(",");
        Serial_print(bData2,HEX);
      }
      Serial_print(']');
    }
  }

  Serial_println();

  uint32_t cdmMsg=0;
  if(bDataLength==1)  //Only allow 1  extra bytes for now
  {
    if(bData1 != 0xff)
    {
      cdmMsg += bData1;
      cdmMsg *=bitStuffing[e_otherDataCount];
      cdmMsg += bDataLength;
    }
  }
  cdmMsg *=bitStuffing[e_other];
  if(bOther != 0xff)
    cdmMsg += bOther;
  cdmMsg *=bitStuffing[e_param];
  if(bParam != 0xff)
    cdmMsg += bParam;
  cdmMsg *=bitStuffing[e_pin];
  if(bParam != 0xff)
    cdmMsg += bPin;
  cdmMsg *=bitStuffing[e_cmd];
  if(bCmd != 0xff)
    cdmMsg += bCmd; 

  Serial_println(cdmMsg,HEX);
  rp2040.fifo.push(cdmMsg);
}

static void initializeClients()
{

  az_iot_hub_client_options options = az_iot_hub_client_options_default();
  options.user_agent = AZ_SPAN_FROM_STR(AZURE_SDK_CLIENT_USER_AGENT);

  secure_wifi_client.setTrustAnchors(&cert);
  if (az_result_failed(az_iot_hub_client_init(
          &client,
          az_span_create((uint8_t*)host, strlen(host)),
          az_span_create((uint8_t*)device_id, strlen(device_id)),
          &options)))
  {
    Serial_println("Failed initializing Azure IoT Hub client");
    return;
  }

  mqtt_client.setServer(host, mqttPort);
  mqtt_client.setCallback(receivedCallback);
}


/*
 * @brief           Gets the number of seconds since UNIX epoch until now.
 * @return uint32_t Number of seconds.
 */
static uint32_t getSecondsSinceEpoch() { return (uint32_t)time(NULL); }

static int generateSasToken(char* sas_token, size_t size)
{
  az_span signature_span = az_span_create((uint8_t*)signature, sizeofarray(signature));
  az_span out_signature_span;
  az_span encrypted_signature_span
      = az_span_create((uint8_t*)encrypted_signature, sizeofarray(encrypted_signature));

  uint32_t expiration = getSecondsSinceEpoch() + ONE_HOUR_IN_SECS;

  // Get signature
  if (az_result_failed(az_iot_hub_client_sas_get_signature(
          &client, expiration, signature_span, &out_signature_span)))
  {
    Serial_println("Failed getting SAS signature");
    return 1;
  }

  // Base64-decode device key
  int base64_decoded_device_key_length
      = base64_decode_chars(device_key, strlen(device_key), base64_decoded_device_key);

  if (base64_decoded_device_key_length == 0)
  {
    Serial_println("Failed base64 decoding device key");
    return 1;
  }

  // SHA-256 encrypt
  br_hmac_key_context kc;
  br_hmac_key_init(
      &kc, &br_sha256_vtable, base64_decoded_device_key, base64_decoded_device_key_length);

  br_hmac_context hmac_ctx;
  br_hmac_init(&hmac_ctx, &kc, 32);
  br_hmac_update(&hmac_ctx, az_span_ptr(out_signature_span), az_span_size(out_signature_span));
  br_hmac_out(&hmac_ctx, encrypted_signature);

  // Base64 encode encrypted signature
  String b64enc_hmacsha256_signature = base64::encode(encrypted_signature, br_hmac_size(&hmac_ctx));

  az_span b64enc_hmacsha256_signature_span = az_span_create(
      (uint8_t*)b64enc_hmacsha256_signature.c_str(), b64enc_hmacsha256_signature.length());

  // URl-encode base64 encoded encrypted signature
  if (az_result_failed(az_iot_hub_client_sas_get_password(
          &client,
          expiration,
          b64enc_hmacsha256_signature_span,
          AZ_SPAN_EMPTY,
          sas_token,
          size,
          NULL)))
  {
    Serial_println("Failed getting SAS token");
    return 1;
  }

  return 0;
}

static int connectToAzureIoTHub()
{
  size_t client_id_length;
  char mqtt_client_id[128];
  if (az_result_failed(az_iot_hub_client_get_client_id(
          &client, mqtt_client_id, sizeof(mqtt_client_id) - 1, &client_id_length)))
  {
    Serial_println("Failed getting client id");
    return 1;
  }

  mqtt_client_id[client_id_length] = '\0';

  char mqtt_username[128];
  // Get the MQTT user name used to connect to IoT Hub
  if (az_result_failed(az_iot_hub_client_get_user_name(
          &client, mqtt_username, sizeofarray(mqtt_username), NULL)))
  {
    printf("Failed to get MQTT clientId, return code\n");
    return 1;
  }
  mqtt_client.setBufferSize(MQTT_PACKET_SIZE);

  while (!mqtt_client.connected())
  {
    time_t now = time(NULL);

    Serial_print("MQTT connecting ... ");

    if (mqtt_client.connect(mqtt_client_id, mqtt_username, sas_token))
    {
      Serial_println("connected.");
    }
    else
    {
      Serial_print("failed, status code =");
      Serial_print(mqtt_client.state());
      Serial_println(". Trying again in 5 seconds.");
      // Wait 5 seconds before retrying
      delay(5000);
    }
  }

  mqtt_client.subscribe(AZ_IOT_HUB_CLIENT_C2D_SUBSCRIBE_TOPIC);

  return 0;
}
static bool WiFiStarted = false;
static bool TimeInited = false;
static bool ClientInited = false;
static void establishConnection()
{
  if(!WiFiStarted)
    FlashStorage::WiFiConnect();
  if(!TimeInited)
    initializeTime();
  printCurrentTime();
  if(!ClientInited)
  {
    host = FlashStorage::GetIOT_CONFIG_IOTHUB_FQDN().c_str();
    device_id = FlashStorage::GetDeviceHostname().c_str();
    device_key = FlashStorage::GetDeviceConnectionString().c_str();
    initializeClients();
  }

  WiFiStarted = true;
  TimeInited = true;
  ClientInited = true;

  // The SAS token is valid for 1 hour by default in this sample.
  // After one hour the sample must be restarted, or the client won't be able
  // to connect/stay connected to the Azure IoT Hub.
  if (generateSasToken(sas_token, sizeofarray(sas_token)) != 0)
  {
    Serial_println("Failed generating MQTT password");
  }
  else
  {
    connectToAzureIoTHub();
  }

  digitalWrite(LED_BUILTIN, LOW);
}

//////////////// T E L E M E T R Y ///////////////////////

static char* getTelemetryPayload(char * jsonStr)
{
  az_span temp_span = az_span_create_from_str(jsonStr);
  az_span_to_str((char *)telemetry_payload, sizeof(telemetry_payload), temp_span);
  return (char*)telemetry_payload;
}

static void sendTelemetry(String jsonStr)
{
  digitalWrite(LED_BUILTIN, HIGH);
  Serial_print(millis());
  
  Serial_print(" RPI Pico (Arduino) Sending telemetry . . . ");
 
  if (az_result_failed(az_iot_hub_client_telemetry_get_publish_topic(
          &client, NULL, telemetry_topic, sizeof(telemetry_topic), NULL)))
  {
    Serial_println("Failed az_iot_hub_client_telemetry_get_publish_topic");
    return;
  }
  char * json = const_cast<char*>(jsonStr.c_str());
  mqtt_client.publish(telemetry_topic, getTelemetryPayload(json), false);
  Serial_println("OK");
}

#endif