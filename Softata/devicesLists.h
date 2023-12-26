#ifndef DEVICESLISTH
#define DEVICESLISTH
#include "grove.h"

#define MAX_SENSORS 20
#define MAX_ACTUATORS 20


//Library Manager, and search for LinkedList.
// Select the Ivan Siedel version
class SensorListNode
{
public:
    GroveSensor Type;
    Grove_Sensor * Sensor=NULL;
protected:
};

class ActuatorListNode
{
public:
    GroveActuator Type;
    Grove_Actuator * Actuator = NULL;
protected:
};

////////////////////////////////////////////////////////////////////////////////////////

SensorListNode * SensorList[MAX_SENSORS];
int count =0;

int AddSensorToList(Grove_Sensor * sensor)
{
  if(count < (MAX_SENSORS-1))
  {
    SensorList[count++]->Sensor= sensor;
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





