/////////////////////////////////////////////////////////////////
//       ESP32 & Xiaomi Bluetooth sensor -> MQTT  v0.01        //
/////////////////////////////////////////////////////////////////

#include "soc/soc.h"
#include "soc/rtc_cntl_reg.h"

#include <BLEDevice.h>
#include <BLEScan.h>
#include <BLEAdvertisedDevice.h>

#include <WiFi.h>
#include <PubSubClient.h>

#include "heltec.h"

#define wifiSsid "bsakel"
#define wifiPassword "349a5ef303d4"

#define mqttPort 13583
#define mqttServer "farmer.cloudmqtt.com"
#define mqttUser "hbqgbpnm"
#define mqttPassword "7cfv__CwwpBO"
#define mqttClientId "ESP32Client"
#define mqttHelloPublishChannel "/ESP32/presence/"
#define mqttHelloPublishMessage "ok"
#define mqttPublishChannel "/ESP32/pub/"
#define mqttReceiveChannel "/ESP32/sub/"

#define bleScanTime 20      //scan for (x) seconds
#define bleScanDelayTime 40 //scan every (x) seconds

WiFiClient wifiClient;
PubSubClient mqttClient(wifiClient);
BLEScan *pBLEScan;
int deviceCount;

void oled_init()
{
  Heltec.begin(true /*DisplayEnable Enable*/, false /*LoRa Disable*/, false /*Serial Enable*/);

  Heltec.display->clear();
  Heltec.display->flipScreenVertically();

  Heltec.display->setTextAlignment(TEXT_ALIGN_LEFT);
  Heltec.display->setFont(ArialMT_Plain_10);
  Heltec.display->drawString(0, 0, "ESP32 XIAOMI MQTT");
  Heltec.display->display();
}

void oled_display_status()
{
  Heltec.display->clear();

  char wifiMessage[40];
  if (WiFi.status() == WL_CONNECTED)
  {
    sprintf(wifiMessage, "WiFi: %s", WiFi.localIP().toString().c_str());
  }
  else
  {
    sprintf(wifiMessage, "WiFi: Disconnected");
  }
  Heltec.display->drawString(0, 0, wifiMessage);

  char mqttMessage[40];
  if (mqttClient.connected())
  {
    sprintf(mqttMessage, "MQTT: Connected");
  }
  else
  {
    sprintf(mqttMessage, "MQTT: Disconnected");
  }
  Heltec.display->drawString(0, 10, mqttMessage);

  char bleMessage[40];
  if (deviceCount > 0)
  {
    sprintf(bleMessage, "BLE: %d device(s)", deviceCount);
  }
  else
  {
    sprintf(bleMessage, "BLE: No Devices");
  }
  Heltec.display->drawString(0, 20, bleMessage);

  Heltec.display->display();
}

void wifi_connect()
{
  delay(10);

  Serial.print("\nWIFI: Connecting to ");
  Serial.println(wifiSsid);

  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
  }

  randomSeed(micros());

  Serial.print("\nWIFI connected ");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
}

void mqtt_broker_connect()
{
  while (!mqttClient.connected())
  {
    Serial.print("MQTT: Connecting to ");
    Serial.println(mqttServer);

    if (mqttClient.connect(mqttClientId, mqttUser, mqttPassword))
    {
      mqttClient.publish(mqttHelloPublishChannel, mqttHelloPublishMessage);
      mqttClient.subscribe(mqttReceiveChannel);
      mqttClient.setCallback(mqtt_broker_callback);

      Serial.println("MQTT connected");
    }
    else
    {
      Serial.print(".");
      delay(1000);
    }
  }
}

void mqtt_broker_callback(char *topic, byte *payload, unsigned int length)
{
  Serial.println("-------new message from broker-----");
  Serial.print("channel:");
  Serial.println(topic);
  Serial.print("data:");
  Serial.write(payload, length);
  Serial.println();
}

void mqtt_broker_publish(char *data)
{
  mqttClient.publish(mqttPublishChannel, data);

  //Serial.printf("TEMPERATURE_EVENT: %s, %.1f\n", deviceAddress.c_str(), current_temperature);
}

class MyAdvertisedDeviceCallbacks : public BLEAdvertisedDeviceCallbacks
{
  void onResult(BLEAdvertisedDevice advertisedDevice)
  {
    int serviceDataCount = advertisedDevice.getServiceDataCount();
    for (int serviceDataIndex = 0; serviceDataIndex < serviceDataCount; serviceDataIndex++)
    {
      uint8_t cServiceData[100];
      char charServiceData[100];
      std::string strServiceData = advertisedDevice.getServiceData(serviceDataIndex);
      strServiceData.copy((char *)cServiceData, strServiceData.length(), 0);
      for (int i = 0; i < strServiceData.length(); i++)
      {
        sprintf(&charServiceData[i * 2], "%02x", cServiceData[i]);
      }
      unsigned long value;
      char charValue[5] = {
          0,
      };
      std::string deviceAddress = advertisedDevice.getAddress().toString();
      switch (cServiceData[11])
      {
      case 0x04:
        sprintf(charValue, "%02X%02X", cServiceData[15], cServiceData[14]);
        value = strtol(charValue, 0, 16);
        ble_handle_temprature_event(deviceAddress, value);
        break;
      case 0x06:
        sprintf(charValue, "%02X%02X", cServiceData[15], cServiceData[14]);
        value = strtol(charValue, 0, 16);
        ble_handle_humidity_event(deviceAddress, value);
        break;
      case 0x0A:
        sprintf(charValue, "%02X", cServiceData[14]);
        value = strtol(charValue, 0, 16);
        ble_handle_battery_event(deviceAddress, value);
        break;
      case 0x0D:
        sprintf(charValue, "%02X%02X", cServiceData[15], cServiceData[14]);
        value = strtol(charValue, 0, 16);
        ble_handle_temprature_event(deviceAddress, value);
        sprintf(charValue, "%02X%02X", cServiceData[17], cServiceData[16]);
        value = strtol(charValue, 0, 16);
        ble_handle_humidity_event(deviceAddress, value);
        break;
      }
    }
  }
};

void ble_init()
{
  BLEDevice::init("");
  pBLEScan = BLEDevice::getScan(); //create new scan
  pBLEScan->setAdvertisedDeviceCallbacks(new MyAdvertisedDeviceCallbacks());
  pBLEScan->setActiveScan(true); //active scan uses more power, but get results faster
  pBLEScan->setInterval(0x50);
  pBLEScan->setWindow(0x30);

  BLEScanResults foundDevices = pBLEScan->start(bleScanTime);
  deviceCount = foundDevices.getCount();
}

void ble_handle_temprature_event(std::string deviceAddress, unsigned long value)
{
  float current_temperature;
  current_temperature = (float)value / 10;

  Serial.printf("TEMPERATURE_EVENT: %s, %.1f\n", deviceAddress.c_str(), current_temperature);
}

void ble_handle_humidity_event(std::string deviceAddress, unsigned long value)
{
  float current_humidity = (float)value / 10;

  Serial.printf("HUMIDITY_EVENT: %s, %.1f\n", deviceAddress.c_str(), current_humidity);
}

void ble_handle_battery_event(std::string deviceAddress, unsigned long value)
{
  Serial.printf("BATTERY_EVENT: %s, %d\n", deviceAddress.c_str(), value);
}

void setup()
{
  WRITE_PERI_REG(RTC_CNTL_BROWN_OUT_REG, 0); //disable brownout detector

  Serial.begin(115200);
  Serial.println("ESP32 XIAOMI MQTT RELAY");

  WiFi.begin(wifiSsid, wifiPassword);
  mqttClient.setServer(mqttServer, mqttPort);

  oled_init();

  wifi_connect();
  
  mqtt_broker_connect();

  ble_init();

  oled_display_status();
}

void loop()
{
  if (WiFi.status() != WL_CONNECTED)
  {
    Serial.printf("\nWifi disconnected. Reconnecting...\n");

    oled_display_status();

    wifi_connect();
  }

  if (!mqttClient.connected())
  {
    Serial.printf("\nMQTT disconnected. Reconnecting...\n");

    oled_display_status();

    mqtt_broker_connect();
  }

  Serial.printf("\nBLE scan for %d seconds...\n", bleScanTime);

  BLEScanResults foundDevices = pBLEScan->start(bleScanTime);

  deviceCount = foundDevices.getCount();
  Serial.printf("Found %d devices\n", deviceCount);

  oled_display_status();

  Serial.printf("\nWait before next scan for %d seconds...\n", bleScanDelayTime);
  delay(bleScanDelayTime * 1000);
}
