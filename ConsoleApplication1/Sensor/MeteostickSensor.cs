using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace Matrix.Sensors
{
    public class MeteostickWeatherSensor : WeatherSensor
    {
        protected override SensorReading ReadValues()
        {
            return new MeteostickWeatherReading() { };
        }
    }

    public class MeteostickWeatherReading : SensorReading
    { }

    public class MeteostickSensor: Sensor
    {
        private MeteosticSensorReading _lastReading;
        private List<MeteostickSensorMetric> MetricsToRead;
        private SerialPort ComPort;

        public MeteostickSensor(List<MeteostickSensorMetric> metrics)
        {
            MetricsToRead = metrics;
        }
            
        protected override void CommenceMeasurement()
        {
            base.CommenceMeasurement();
        }

        protected override void CompleteMeasurement()
        {
            base.CompleteMeasurement();
        }

        protected override SensorReading ReadValues()
        {
            return new MeteosticSensorReading
            {
                MetricReadings = MetricsToRead.Select(
                    x =>
                        new MeteostickSensorMetricReading
                        {
                            Metric = x,
                            Measurement = GetMeasurement(x)
                        }
                    ).ToList()
            };
           
        }
        private string GetMeasurement(MeteostickSensorMetric metric)
        {
            ComPort.Write(metric.Command);
            return ComPort.ReadLine();
        }
    }
    
    

    public abstract class MeteostickSensorMetric
    {
        public abstract string Command { get; }
    }

    public class windSpeed : MeteostickSensorMetric
    {
        public override string Command => "something";
    }

    public class MeteosticSensorReading : SensorReading
    {
        public List<MeteostickSensorMetricReading> MetricReadings;
    }

    public class MeteostickSensorMetricReading
    {
        public MeteostickSensorMetric Metric;
        public string Measurement;
    }

}
