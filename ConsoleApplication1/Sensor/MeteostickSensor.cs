using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Management;

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
        private SerialPort MeteoComPort;

        private string LastLine;

        private int x;

        public MeteostickSensor(List<MeteostickSensorMetric> metrics)
        {
            MetricsToRead = metrics;
            


            foreach (ManagementObject device in new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_SerialPort").Get())
            {
                // Select device based on VendorID.
                if (device["PNPDeviceID"].ToString().Contains("VID_0403"))
                {
                    MeteoComPort = new SerialPort(device["DeviceID"].ToString());
                    Console.WriteLine("created Meteostick port");
                    Console.WriteLine(MeteoComPort.PortName);
                }
                else
                {
                    Console.WriteLine("didn't find Meteostick Port");
                    MeteoComPort = new SerialPort("COM5", 115200);
                    Console.WriteLine(MeteoComPort.PortName);
                }
                MeteoComPort.Open();
                MeteoComPort.WriteLine("r");
                //System.Threading.Thread.Sleep(200);
                LastLine = MeteoComPort.ReadLine();
                Console.WriteLine(LastLine);
                MeteoComPort.WriteLine("t64");
                //System.Threading.Thread.Sleep(200);
                LastLine = MeteoComPort.ReadLine();
                Console.WriteLine(LastLine);
                MeteoComPort.WriteLine("m2");
                //System.Threading.Thread.Sleep(200);
                LastLine = MeteoComPort.ReadLine();
                Console.WriteLine(LastLine);
                MeteoComPort.WriteLine("f1");
                //System.Threading.Thread.Sleep(200);
                LastLine = MeteoComPort.ReadLine();
                Console.WriteLine(LastLine);
                MeteoComPort.WriteLine("o2");
                //System.Threading.Thread.Sleep(200);
                LastLine = MeteoComPort.ReadLine();
                Console.WriteLine(LastLine);
                //foreach (var i in Enumerable.Range(0, 50))
                //{
                //    LastLine = ComPort.ReadLine();
                //    Console.WriteLine(LastLine);
                //}
                Console.WriteLine("MeteoStick Initialisation Finished");
                
            }
        }

        protected override void CommenceMeasurement()
            {
                Console.WriteLine ("Meteostick Commenced Measurement");
                foreach (var i in Enumerable.Range(0, 50))
                {
                    LastLine = MeteoComPort.ReadLine();
                    Console.WriteLine(LastLine);
                }
            }

            protected override void CompleteMeasurement()
            {
                Console.WriteLine ("Meteostick Completed Measurement");
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
            MeteoComPort.Write(metric.Command);
            return MeteoComPort.ReadLine();
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
