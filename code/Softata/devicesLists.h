#ifndef DEVICESLISTH
#define DEVICESLISTH
#include "Softata.h"
//#include "src/grove.h"


//Library Manager: Search for LinkedList.
// Select the Ivan Siedel version
class SensorListNode
{
public:
    SoftataDevice_Sensor * Sensor=NULL;
    int TelemetryStreamNo;
protected:
};

class DisplayListNode
{
public: 
    SoftataDevice_Display * Display=NULL;
protected:
};

class ActuatorListNode
{
public:
    SoftataDevice_Actuator * Actuator = NULL;
protected:
};

class DeviceInputListNode
{
public:
    SoftataDevice_DeviceInput * DeviceInput = NULL;
protected:
};


class Core2SensorListNode
{
public:
    CallbackInfo * callbackInfo = NULL;
protected:
};

////////////////////////////////////////////////////////////////////////////////////////

SensorListNode * SensorList[MAX_SENSORS];
int count =0;

int AddSensorToList(SoftataDevice_Sensor * sensor )
{
  if(count < (MAX_SENSORS-1))
  {
    SensorList[count++]->Sensor = sensor;
    return (count-1);
  }
  else
  {
    for(int i=0; i<MAX_SENSORS; i++)
    {
      if(SensorList[i]->Sensor == NULL)
      {
        SensorList[i]->Sensor = sensor;
        return i;
      }
    }
  }
  return -1;
}

bool RemoveSensorFromList(int index)
{
  if(index < MAX_SENSORS)
  {
    SensorList[index]->Sensor= NULL;
    return true;
  }
  else
    return false;
}

SoftataDevice_Sensor * GetSensorFromList(int index)
{
  if(SensorList[index]->Sensor != NULL)
    return SensorList[index]->Sensor;
  else
    return NULL;
}

SensorListNode * GetNode(int index)
{
  return SensorList[index];
}

int InitSensorList()
{
  for(int i=0; i<MAX_SENSORS; i++)
  {
    SensorList[i]= new SensorListNode() ;
  }
  count=0;
  return MAX_SENSORS;
}

////////////////////////////////////////////////////////////////////////////////////////

DisplayListNode * DisplayList[MAX_DISPLAYS];
int displaycount = 0;

int AddDisplayToList(SoftataDevice_Display * display)
{
  if(displaycount < (MAX_DISPLAYS-1))
  {
    DisplayList[displaycount++]->Display= display;
    Serial_print("@@@");
    Serial_print(displaycount-1);
    Serial_print("@@@");
    return (displaycount-1);
  }
  else
  {
    for(int i=0; i<MAX_DISPLAYS; i++)
    {
      if(DisplayList[i]->Display == NULL)
      {
        DisplayList[i]->Display = display;
        return i;
      }
    }
  }
  return -1;
}

bool RemoveDisplayFromList(int index)
{
  if(index < MAX_DISPLAYS)
  {
    delete DisplayList[index]->Display;
    DisplayList[index]->Display= NULL; //nullptr;
    if (index == displaycount-1)
      displaycount--;
    return true;
  }
  else
    return false;
}

SoftataDevice_Display * GetDisplayFromList(int index)
{
  if(DisplayList[index]->Display!= NULL)
    return DisplayList[index]->Display;
  else
    return NULL;
}

int InitDisplayList()
{
  for(int i=0; i<MAX_DISPLAYS; i++)
  {
    DisplayList[i]= new DisplayListNode() ;
  }
  displaycount=0;
  return MAX_DISPLAYS;
}

////////////////////////////////////////////////////////////////////////////////////////


ActuatorListNode * ActuatorList[MAX_ACTUATORS];
int countActuators =0;

int AddActuatorToList(SoftataDevice_Actuator * actuator)
{
  if(countActuators < (MAX_ACTUATORS-1))
  {
    ActuatorList[countActuators++]->Actuator= actuator;
    return (countActuators-1);
  }
  else
  {
    for(int i=0; i<MAX_ACTUATORS; i++)
    {
      if(ActuatorList[i]->Actuator == NULL)
      {
        ActuatorList[i]->Actuator = actuator;
        return i;
      }
    }
  }
  return -1;
}

bool RemoveActuatorFromList(int index)
{
  if(index < MAX_ACTUATORS)
  {
    ActuatorList[index]->Actuator= NULL;
    return true;
  }
  else
    return false;
}

SoftataDevice_Actuator * GetActuatorFromList(int index)
{
  if(ActuatorList[index]->Actuator != NULL)
    return ActuatorList[index]->Actuator;
  else
    return NULL;
}

int InitActuatorList()
{
  for(int i=0; i<MAX_ACTUATORS; i++)
  {
    ActuatorList[i]= new ActuatorListNode() ;
  }
  countActuators=0;
  return MAX_ACTUATORS;
}

////////////////////////////////////////////////////////////////////////////////////////

DeviceInputListNode * DeviceInputList[MAX_OF_DEVICE_TYPE];
int countDeviceInputs =0;

int AddDeviceInputToList(SoftataDevice_DeviceInput * deviceinput)
{
  if(countDeviceInputs < (MAX_OF_DEVICE_TYPE-1))
  {
    DeviceInputList[countDeviceInputs++]->DeviceInput= deviceinput;
    return (countDeviceInputs-1);
  }
  else
  {
    for(int i=0; i<MAX_ACTUATORS; i++)
    {
      if(DeviceInputList[i]->DeviceInput == NULL)
      {
        DeviceInputList[i]->DeviceInput = deviceinput;
        return i;
      }
    }
  }
  return -1;
}

bool RemoveDeviceInputFromList(int index)
{
  if(index < MAX_OF_DEVICE_TYPE)
  {
    DeviceInputList[index]->DeviceInput= NULL;
    return true;
  }
  else
    return false;
}

SoftataDevice_DeviceInput * GetDeviceInputFromList(int index)
{
  if(DeviceInputList[index]->DeviceInput != NULL)
    return DeviceInputList[index]->DeviceInput;
  else
    return NULL;
}

int InitDeviceInputList()
{
  for(int i=0; i<MAX_OF_DEVICE_TYPE; i++)
  {
    DeviceInputList[i]= new DeviceInputListNode() ;
  }
  countDeviceInputs=0;
  return MAX_OF_DEVICE_TYPE;
}


////////////////////////////////////////////////////////////////////////////////////////


Core2SensorListNode * Core2SensorList[MAX_SENSORS];
int countCore2Sensors =0;

int AddSensorToCore2List(CallbackInfo * callbackInfo)
{
  if(countCore2Sensors < (MAX_SENSORS-1))
  {
    Core2SensorList[countCore2Sensors++]->callbackInfo = callbackInfo;
    return (countCore2Sensors-1);
  }
  else
  {
    for(int i=0; i<MAX_SENSORS; i++)
    {
      if(Core2SensorList[i]->callbackInfo == NULL)
      {
        Core2SensorList[i]->callbackInfo = callbackInfo;
        return i;
      }
    }
  }
  return -1;
}

bool RemoveSensorFromCore2List(int index)
{
  if((index>=0) && (index < MAX_SENSORS))
  {
    CallbackInfo * info = Core2SensorList[index]->callbackInfo;
    if(Core2SensorList[index]->callbackInfo != NULL)
    {
      Core2SensorList[index]->callbackInfo= NULL;
      return true;
    }
    else
      return false;
  }
  else
    return false;
}

CallbackInfo * GetCallbackInfoFromCore2List(int index)
{
  if(Core2SensorList[index]->callbackInfo != NULL)
    return Core2SensorList[index]->callbackInfo;
  else
    return NULL;
}

bool SetCallbackInfoInCore2List(int index, CallbackInfo * info)
{
  if((index>=0) && (index < MAX_SENSORS))
  {
    Core2SensorList[index]->callbackInfo = info;
    return true;
  }
  else
    return false;
}


int InitCore2SensorList()
{
  for(int i=0; i<MAX_SENSORS; i++)
  {
    Core2SensorList[i]= new Core2SensorListNode() ;
  }
  countCore2Sensors=0;
  return MAX_SENSORS;
}

#endif





