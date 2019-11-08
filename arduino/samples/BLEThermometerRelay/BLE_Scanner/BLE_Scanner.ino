#include <Arduino.h>
#include <sstream>

#include "soc/soc.h"
#include "soc/rtc_cntl_reg.h"

#include "BLEDevice.h"
#include "BLEUtils.h"
#include "BLEScan.h"
#include "BLEAdvertisedDevice.h"


#define SCAN_TIME  20 // seconds
#define DELAY_TIME  10 // seconds

// comment the follow line to disable serial message
#define SERIAL_PRINT


class MyAdvertisedDeviceCallbacks : public BLEAdvertisedDeviceCallbacks
{
    void onResult(BLEAdvertisedDevice advertisedDevice)
    {
//#ifdef SERIAL_PRINT
//      Serial.printf("Advertised Device: %s \n", advertisedDevice.toString().c_str());
//#endif
    }
};

void setup()
{
  WRITE_PERI_REG(RTC_CNTL_BROWN_OUT_REG, 0); //disable brownout detector

#ifdef SERIAL_PRINT
  Serial.begin(115200);
  Serial.println("ESP32 BLE Scanner");

  Serial.printf("BLE scan for %d seconds every %d\n", SCAN_TIME, DELAY_TIME);
#endif

  BLEDevice::init("");
  
}

void loop()
{
  BLEScan *pBLEScan = BLEDevice::getScan(); //create new scan
  pBLEScan->setAdvertisedDeviceCallbacks(new MyAdvertisedDeviceCallbacks());
  pBLEScan->setActiveScan(true); //active scan uses more power, but get results faster
  pBLEScan->setInterval(0x50);
  pBLEScan->setWindow(0x30);
  
  BLEScanResults foundDevices = pBLEScan->start(SCAN_TIME);
  int count = foundDevices.getCount();

  std::stringstream ss;
  
  ss << "[";
  for (int i = 0; i < count; i++)
  {
    if (i > 0) {
      ss << ",";
   }
    BLEAdvertisedDevice d = foundDevices.getDevice(i);
    ss << "{\"Address\":\"" << d.getAddress().toString() << "\"";

    if (d.haveName())
    {
      ss << ",\"Name\":\"" << d.getName() << "\"";
    }

    if (d.haveServiceData())
    {
      ss << ",\"ServiceDataCount\":\"" << d.getServiceDataCount() << "\"";
      
      //ss << ",\"ServiceDataUUID\":\"" << d.getServiceDataUUID().toString() << "\"";
      //
      //std::string strServiceData = d.getServiceData();
      //uint8_t cServiceData[100];
      //char charServiceData[100];
      //
      //strServiceData.copy((char *)cServiceData, strServiceData.length(), 0);
      //for (int i=0;i<strServiceData.length();i++) {
      //  sprintf(&charServiceData[i*2], "%02x", cServiceData[i]);
      //}
      //ss << ",\"ServiceData\":\"" << charServiceData << "\"" ;
    }

    ss << "}";
  }
  ss << "]";

  Serial.println(ss.str().c_str());

  delay(DELAY_TIME * 1000);
 
}
