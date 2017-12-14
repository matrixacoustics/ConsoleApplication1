using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Matrix.Sensors
{
    
    public class FineOffsetWeatherSensor : WeatherSensor
    {
        protected override SensorReading ReadValues()
        {
            return new FineOffsetWeatherReading() { };
        }
    }

    public class FineOffsetWeatherReading : SensorReading
    { }


    // abstract = must be overwritten in a child class
    // virtual = can be overwritten in a child class

    public abstract class Sensor
    {
        private Exception CommenceException = null;
        private Exception CompleteException = null;
        private Exception ReadException = null;

        public void CommenceMeasurementPeriod()
        {
            try
            {
                CommenceMeasurement();
                CommenceException = null;
            }
            catch (Exception e)
            {
                CommenceException = e;
            }
        }

        public void CompleteMeasurementPeriod()
        {
            try
            {
                CompleteMeasurement();
                CompleteException = null;
            }
            catch (Exception e)
            {
                CompleteException = e;
            }
        }

        public SensorReading ReadMeasuredValue()
        {

            SensorReading reading = null;
            try
            {
                reading = ReadValues();
                ReadException = null;
            }
            catch (Exception e)
            {
                ReadException = e;
            }

            if (CommenceException != null || CompleteException != null || ReadException != null)
            {
                return new ErrorReadingSensor
                {
                    CommenceException = CommenceException,
                    CompleteException = CompleteException,
                    ReadException = ReadException,
                    Data = reading
                };
            }

            return reading;
        }

        protected virtual void CommenceMeasurement() { }
        protected virtual void CompleteMeasurement() { }
        protected abstract SensorReading ReadValues();
    }

   

    public abstract class WeatherSensor : Sensor
    {

    }

    public class DavisWeatherSensor : WeatherSensor
    {
        protected override SensorReading ReadValues()
        {
            return new FineOffsetWeatherReading() { };

        }
    }

    public class SensorReading
    {

    }


    public class DavisWeatherSensorReading : SensorReading
    {

    }

    public class XL2 : Sensor
    {
        private XL2Reading _lastReading;
        private SerialPort ComPort;
        private List<XL2Metric> MetricsToRead;

        public XL2(List<XL2Metric> metrics, string range)
        {
            MetricsToRead = metrics;
            ComPort = new SerialPort("COM4");
            ComPort.Open();
            //ComPort.Write($"INPUT:RANGE {ConfigurationManager.AppSettings["XL2Range"]} \n");
            ComPort.Write($"INPUT:RANGE {range} \n");
        }

        protected override void CommenceMeasurement()
        {
            // Start
            if (!ComPort.IsOpen)
            {
                ComPort.Open();
            }
            ComPort.Write("INIT START\n");
        }

        protected override void CompleteMeasurement()
        {
            // Stop
            ComPort.Write("INIT STOP\n");

            // Get ready to collect measurments
            ComPort.Write("MEAS:INIT\n");
        }

        protected override SensorReading ReadValues()
        {
            // Read
            return new XL2Reading
            {
                MetricReadings = MetricsToRead.Select(
                    x =>
                        new XL2MetricReading
                        {
                            Metric = x,
                            Measurement = GetMeasurement(x)
                        }
                ).ToList()
            };
        }

        private string GetMeasurement(XL2Metric metric)
        {
            ComPort.Write(metric.Command);
            return ComPort.ReadLine();
        }
    }

    public abstract class XL2Metric
    {
        public abstract string Command { get; }
    }

    public class LAEQ : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? LAEQ\n";
    }

    public class LAFMAX : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? LAFMAX\n";

    }

    public class XL2Reading : SensorReading
    {
        public List<XL2MetricReading> MetricReadings;
    }

    public class ErrorReadingSensor : SensorReading
    {
        public Exception CommenceException;
        public Exception CompleteException;
        public Exception ReadException;
        public SensorReading Data;
    }

    public class XL2MetricReading
    {
        public XL2Metric Metric;
        public string Measurement;
    }
}
