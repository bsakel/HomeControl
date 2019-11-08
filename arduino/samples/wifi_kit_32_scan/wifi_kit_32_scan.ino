//////////////////////////////////////////////////////////////////////////////////////////////////////////////
//                                                                                                          //
//                                     Heltech WiFi Kit 32 Wifi Scanner                                     //
//                                                                                                          //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////


//////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Includes.
//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include                              "WiFi.h"
#include                              <U8g2lib.h>

//////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Variables.
//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////

U8G2_SSD1306_128X64_NONAME_F_HW_I2C   u8g2(U8G2_R0, 16, 15, 4);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 
// Setup.
//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////

void setup()
{
  // Set the wifi mode to station.
  
  WiFi.mode(WIFI_STA);

  // Initialize the graphics library.
  
  u8g2.begin();
  u8g2.setFont(u8g2_font_6x10_tf);
  u8g2.setFontRefHeightExtendedText();
  u8g2.setDrawColor(1);
  u8g2.setFontPosTop();
  u8g2.setFontDirection(0);
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Main loop.
//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////

void  loop()
{
    // Obtain network count.
    
    int nNetworkCount = WiFi.scanNetworks();

    // Clear the display buffer.
    
    u8g2.clearBuffer();

    // Display networks.
    
    if(nNetworkCount == 0) 
    {
        // No networks found.
        
        u8g2.drawStr(0, 0, "0 networks found.");
    } 
    else 
    {
        // Networks found.
        
        char    chBuffer[128];
        char    chEncryption[64];
        char    chRSSI[64];
        char    chSSID[64];
      
        // Display network count.
        
        sprintf(chBuffer, "%d networks found:", nNetworkCount);
        u8g2.drawStr(0, 0, chBuffer);

        // Display the networks.
        
        for(int nNetwork = 0; nNetwork < nNetworkCount; nNetwork ++) 
        {
            // Obtain ssid for this network.
      
            WiFi.SSID(nNetwork).toCharArray(chSSID, 64);

            // Obtain the rssi for this network.
            
            sprintf(chRSSI, "(%d)", WiFi.RSSI(nNetwork));

            // Obtain the encryption type for this network.
            
            sprintf(chEncryption, "%s", WiFi.encryptionType(nNetwork) == WIFI_AUTH_OPEN ? " ": "*");

            // Display the results.
            
            sprintf(chBuffer, "%d: %s %s %s", nNetwork + 1, chSSID, chRSSI, chEncryption);
            u8g2.drawStr(0, 8 + ((nNetwork + 1) * 8), chBuffer);
        }
    }

  // Send the display buffer to the oled.
  
  u8g2.sendBuffer();

  // Delay.
  
  delay(2000);
}



