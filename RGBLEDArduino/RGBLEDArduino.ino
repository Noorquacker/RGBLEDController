/*
	name: RGB LED Controller w/ Arduino v2
	desc: controls rgb with arduino...because 2017 is year of the rgb...
	author: Noorquacker
	
	This does some RGB thing with Arduino where it makes a much better
	thing for controlling an RGB LED strip over Arduino instead of the
	darn Firmata protocol, which never works when I turn on my PC,
	and requires a program to always be on when I'm doing anything
	
	Copyright (C) Noorquacker Ind. 2017 All rights reserved
	This code is under the WTFPL license at http://www.wtfpl.net/txt/copying/
	
	2021-09-08: This code was abandoned. This is meant to be an update
*/

#include <Arduino.h>
#include <EEPROM.h>

#define CMODE_GET 0x00
#define CMODE_SET 0x01
#define STAT_OK 0x02
#define STAT_ERR 0x03
#define SETVEC 0x04

#define CMODE_STATIC 0x05
#define CMODE_BREATHE 0x06
#define CMODE_STROBE 0x07
#define CMODE_DYNAMIC 0x09
#define CMODE_CYCLE 0x0A

#define INT 0x20
#define VEC 0x21
#define DELAY 0x22

//i swear, if "y'aint" usin' n++ and are using the gosh darn arduino ide, you get 15 whippings
//anyways change these values 

int pinR = 11;
int pinG = 6;
int pinB = 3;

//dont change these
int cmode = EEPROM[3];
int[3] cRGB = {EEPROM[0], EEPROM[1], EEPROM[2]};
//note that we make a new variable for RGB and then write to this and EEPROM but only read from this to abstract the eeprom
//that way we don't have unnecessary reads to EEPROM
//FFS I JUST FOUND OUT THAT EEPROM ONLY HAS 100K WRITE CYCLES
//WHY DOES EVERY GOOD STORAGE MEDIUM HAVE SHORT...
//Nothing gold can stay

void setup() {
	Serial.begin(115200);
	while(!Serial) {}
	pinMode(pinR,OUTPUT);
	pinMode(pinG,OUTPUT);
	pinMode(pinB,OUTPUT);
	analogWrite(pinR,0);
	analogWrite(pinG,0);
	analogWrite(pinB,0);
}
void loop() {
	if(Serial.available()) {
		byte inByte = Serial.read();
		switch(inByte) {
			case CMODE_GET:
				Serial.write(cmode);
				break;
			case CMODE_SET:
				if(!Serial.available()) {
					break;
				}
				cmode = Serial.read();
				break;
			
			default:
				Serial.print("ERR: Unknown input byte ");
				Serial.write(inByte,HEX);
				break;
		}
	}
	
}
