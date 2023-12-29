#ifndef DEVICESLISTH
#define DEVICESLISTH
#include "Softata.h"
#include "src/grove.h"


//Library Manager: Search for LinkedList.
// Select the Ivan Siedel version
class SensorListNode
{
public:
    Grove_Sensor * Sensor=NULL;
protected:
};

class DisplayListNode
{
public:
    Grove_Display * Display=NULL;
protected:
};

class ActuatorListNode
{
public:
    Grove_Actuator * Actuator = NULL;
protected:
};

////////////////////////////////////////////////////////////////////////////////////////

SensorListNode * SensorList[MAX_SENSORS];
int count =0;

int AddSensorToList(Grove_Sensor * sensor )
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

Grove_Sensor * GetSensorFromList(int index)
{
  if(SensorList[index]->Sensor != NULL)
    return SensorList[index]->Sensor;
  else
    return NULL;
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

int AddDisplayToList(Grove_Display * display)
{
  if(displaycount < (MAX_DISPLAYS-1))
  {
    DisplayList[displaycount++]->Display= display;
    return (count-1);
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
    DisplayList[index]->Display= NULL;
    return true;
  }
  else
    return false;
}

Grove_Display * GetDisplayFromList(int index)
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
  count=0;
  return MAX_DISPLAYS;
}

////////////////////////////////////////////////////////////////////////////////////////


ActuatorListNode * ActuatorList[MAX_ACTUATORS];
int countActuators =0;

int AddActuatorToList(Grove_Actuator * actuator)
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

Grove_Actuator * GetActuatorFromList(int index)
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

#endif





