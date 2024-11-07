#include <Arduino.h>
#include <Adafruit_NeoPixel.h>
#include "grove_displays.h"


// When setting up the NeoPixel library, we tell it how many pixels,
// and which pin to use to send signals. Note that for older NeoPixel
// strips you might need to change the third parameter -- see the
// strandtest example for more information on possible values.

Adafruit_NeoPixel * pixels;


bool Adafruit_NeoPixel8::Setup() {
  pixels = new Adafruit_NeoPixel(numPixels, grovePin, NEO_GRB + NEO_KHZ800);
  
  pixels->begin(); // This initializes the NeoPixel library.
  return true;
}

bool Adafruit_NeoPixel8::Setup(byte * settings, byte numSettings)
{
    Serial_print("Pixel numSettings:");
    Serial_println(numSettings);
    if (numSettings > 0)
    {
        grovePin = settings[0];
        Serial_print("Pixel grovePin:");
        Serial_println(grovePin);
        if (numSettings > 1)
        {
            numPixels = settings[1];
            Serial_print("Pixel numPixels:");
            Serial_println(numPixels);
        }
    }
    return Setup();
}

bool Adafruit_NeoPixel8::Clear()
{
    pixels->clear();
    pixels->show();
    return true;
}

bool Adafruit_NeoPixel8::Home()
{
    return false;
}

bool Adafruit_NeoPixel8::Misc(byte cmd, byte * data, byte length)
{
    if (cmd<0)
        return false;
    else if(cmd>(byte) NEOPIXELMiscCmds_MAX)
        return false;
    NEOPIXELMiscCmds Cmd = (NEOPIXELMiscCmds)cmd;
    //while(pixels->canShow()){delay(1);}
    switch(Cmd)
    {
        case setpixelcolor: 
            if (length<4)
                return false;
            pixels->setPixelColor(data[0], pixels->Color(data[1], data[2], data[3]));
            pixels->show(); 
            break;
        case setpixelcolorAll: ;
            if (length<3)
                return false;
            for(int i=0;i<numPixels;i++)
            {
                pixels->setPixelColor(i, pixels->Color(data[0], data[1], data[2]));
            }
            pixels->show(); 
            break;
        case setpixelcolorOdds:
            if (length<3)
                return false;
            for(int i=0;i<numPixels;i=i+2)
            {
                pixels->setPixelColor(i, pixels->Color(0,0,0));
            }
            for(int i=1;i<numPixels;i=i+2)
            {
                pixels->setPixelColor(i, pixels->Color(data[0], data[1], data[2]));
            }
            pixels->show(); 
            break;
        case setpixelcolorEvens: 
            if (length<3)
                return false;
            for(int i=1;i<numPixels;i=i+2)
            {
                pixels->setPixelColor(i, pixels->Color(0,0,0));
            }
            for(int i=0;i<numPixels;i=i+2)
            {
                pixels->setPixelColor(i, pixels->Color(data[0], data[1], data[2]));
            }
            pixels->show(); 
            break;
        case setBrightness:
            if (length<1)
                return false;
            Serial_print("SetBrightness ");
            Serial_println(data[0]);
            pixels->setBrightness((byte)data[0]);
            pixels->show(); 
            break;
        case setN: 
            //Display a 0..8 level:
            if (length<4)
                return false;
            int num = data[3];
            // Truncate "over value rather than fail."
            if(num>numPixels)
                num = numPixels;
            // On
            for(int i=0;i<num;i++)
            {
                pixels->setPixelColor(i, pixels->Color(data[0], data[1], data[2]));
            }
            // Off
            for(int i=num;i<numPixels;i++)
            {
                pixels->setPixelColor(i, pixels->Color(0, 0, 0));
            }
            pixels->show(); 
            break;
    }
    return true;
}


// Not relevant for NeoPixels
bool Adafruit_NeoPixel8::Backlight()
{
    return false;
}
bool Adafruit_NeoPixel8::SetCursor(byte x, byte y)
{
    return false;
}

bool Adafruit_NeoPixel8::CursorWriteStringAvailable()
{
	return false;
}

bool Adafruit_NeoPixel8::WriteString(String msg)
{
    int val = atoi(msg.c_str());
    Serial_print("WriteString:");
    Serial_println(val);
    for (int i=0;i<numPixels;i++)
    {
        if (val & (1<<i))
            pixels->setPixelColor(i, pixels->Color(255, 255, 255));
        else
            pixels->setPixelColor(i, pixels->Color(0, 0, 0));
    }
    pixels->show();
    return true;
}


bool Adafruit_NeoPixel8::WriteString(byte x, byte y, String msg)
{
    return false;
}