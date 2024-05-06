#ifndef IC_74HC595_ShitRegisterH
#define IC_74HC595_ShitRegisterH

// Ref: https://docs.arduino.cc/tutorials/communication/guide-to-shift-out/
//      https://www.arduino.cc/reference/en/language/functions/advanced-io/shiftout/

class IC_74HC595_ShiftRegister
{

  public:
      IC_74HC595_ShiftRegister();
      IC_74HC595_ShiftRegister(byte * settings, byte numSettings);
      bool Setup();
      bool Setup(byte * settings, byte numSettings);
      virtual bool SetBitState(bool state,int index);
      virtual bool SetBit(int index );
      virtual bool ClearBit(int index );
      virtual bool ToggleBit(int index );
      virtual bool Write8(byte num);
      // Can daisy chain two 74HC595s as per top link. //Assume data is 16 bits
      virtual bool Write16(unsigned int num);

  
};

#endif