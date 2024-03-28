void receivedCallback(char* topic, byte* payload, unsigned int length)
{
  String _Telemetry = "Telemetry";
  String _Pause = "Pause";
  String _Continue = "Continue";
  String _Stop = "Stop";
  String _Actuator = "Actuator";
  String _Raw ="Raw";

  Serial.print("Received [");
  Serial.print(topic);
  Serial.println("]: ");
  for (int i = 0; i < length; i++)
  {
    Serial.print((char)payload[i]);
  }
  Serial.println();
  
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
  Serial.println("");

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

  uint32_t cdmMsg=0;
  if(Cmd.equalsIgnoreCase(_Telemetry))
  {
    bCmd = 0xf0;
  }
  /*else if(Cmd.equalsIgnoreCase(_Actuator))
  { //Coming
    bCmd = 0xf2;
  }*/
  else if(Cmd.equalsIgnoreCase(_Raw))
  {
    //bCmd already parsed
  }
  else
  {
    Serial.print("Other Device");
    return;
  }

  cdmMsg = bCmd;
  Serial.print("cmd:");
  Serial.print(Cmd);
  Serial.print('=');
  Serial.print(bCmd,HEX);

  cdmMsg *=bitStuffing[e_cmd];
  if (bPin!= 0xff)
  {
    cdmMsg += bPin;
    Serial.print(" pin=0x");
    Serial.print(bPin,HEX);
  }

  cdmMsg *=bitStuffing[e_pin];
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
        Serial.println(" - No parameter");
      }
    }
    else
    {
      Serial.println(" - No parameter");
    }
  }
 
  cdmMsg += bParam;
  cdmMsg *=bitStuffing[e_param];
  Serial.print(" param:");
  Serial.print(Param);
  Serial.print("=0x");
  Serial.print(bParam,HEX);

  
  if (bOther != 0xff)
  {
    cdmMsg += bOther;
    Serial.print(" other=0x");
    Serial.print(bOther,HEX);
  }
  cdmMsg *=bitStuffing[e_other];



cdmMsg = 0;
//e_otherDataCount,e_data1,e_data2};
  if (bDataLength==1) //Only allowing one byte of data thus
  {
    if(bData1 != 0xff)
    {
      if(bData2 != 0xff)
      {
        cdmMsg += bData2;
        cdmMsg *=bitStuffing[e_data1];
      }
      cdmMsg += bdata1;
      cdmMsg *= bitStuffing[e_otherDataCount];
      cdmMsg += bDataLength;
    }
    cdmMsg *=bitStuffing[e_other];
    cdmMsg += bOther;
    cdmMsg *=bitStuffing[e_param];
    cdmMsg += bParam;
    cdmMsg *=bitStuffing[e_pin];
    cdmMsg += bPin;
    cdmMsg *=bitStuffing[e_cmd];
    cdmMsg += bCmd; 
  }

  

  Serial.println();
  Serial.println(cdmMsg);
  //rp2040.fifo.push(cdmMsg);
}
