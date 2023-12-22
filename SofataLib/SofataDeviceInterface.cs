using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SofataLib.tempSense;

namespace SofataLib
{
    public enum DeviceType
    {
        sensor,
        actuator,
        textDisplay,
        display,
        hid,
        rfid,
        distanceSensor,
        motionSensor,
        encoder,
        communicationDevice,
        _switch,
        led,
        environmentalSensor,
        environmentalActuator,
        relay,
        motor,
        servo,
        stepperMotor,
        audioDevice,
        medicalSensor,
        other
    }

    public enum stateProperty { off,on,forward,reverse,disabled,enabled,tristate, other}

    public abstract class _DigitalDevice<T,S> 
        where T : Enum
        where S : Enum
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DeviceType _DeviceType { get; set; } = T;

        public virtual bool Setup(List<int> settings)
        {
            var setupValues = Enum.GetValues(typeof(S));

            foreach (var c in setupValues)
            {
                if (settings.Count() < (int)c)
                {
                    S prop = (S)c;
                    var value = settings[(int)prop];
                    // Do whatever
                }
            }
        }

        public virtual bool Restart()
        {
            throw new NotImplementedException();
        }

        public virtual void Shutdown()
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<T, double> ReadAll()
        {
            throw new NotImplementedException();
        }

        public virtual double ReadValue(T key)
        {
            throw new NotImplementedException();
        }

        public virtual bool ReadboolValue(T key)
        {
            throw new NotImplementedException();
        }

        public virtual int ReadintValue(T key)
        {
            throw new NotImplementedException();
        }

        public virtual string ReadstringValue(T key)
        {
            throw new NotImplementedException();
        }

        public virtual bool WriteValue(T key, double value)
        {
            throw new NotImplementedException();
        }

        public virtual bool WriteValue(T key, bool value)
        {
            throw new NotImplementedException();
        }

        public virtual bool WriteValue(T key, int value)
        {
            throw new NotImplementedException();
        }                 
    }

    public enum environmentProperty { temperature, pressure, humidity, co2, co, other }
    public enum setups { }
    public class tempSense : _DigitalDevice<environmentProperty>
    {
        public override bool Setup (List<int> settings)
        {
            base:Setup(settings);

            return true;
        }

        public override bool Restart()
        {
            throw new NotImplementedException();
        }

        public override void Shutdown()
        {
            throw new NotImplementedException();
        }

        public override Dictionary<environmentProperty, double> ReadAll()
        {
            var props =  new Dictionary<environmentProperty, double>();

            return props;
        }

        public override double ReadValue(environmentProperty key)
        {
            throw new NotImplementedException();
        }

        public override bool ReadboolValue(environmentProperty key)
        {
            throw new NotImplementedException();
        }

        public override int ReadintValue(environmentProperty key)
        {
            throw new NotImplementedException();
        }

        public override string ReadstringValue(environmentProperty key)
        {
            throw new NotImplementedException();
        }


        public override bool WriteValue(environmentProperty key, double value)
        {;
            double sensorValue = value;
            return true;
        }

        public override bool WriteValue(environmentProperty key, bool value)
        {
            bool sensorValue = value;
            return true;
        }

        public override bool WriteValue(environmentProperty key, int value)
        {
            int sensorValue = value;
            return true;
        }
    }   
}
