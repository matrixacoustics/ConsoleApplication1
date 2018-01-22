using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace Matrix.Sensors
{
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

    public class SensorReading
    {

    }

    public class ErrorReadingSensor : SensorReading
    {
        public Exception CommenceException;
        public Exception CompleteException;
        public Exception ReadException;
        public SensorReading Data;
    }

    public abstract class WeatherSensor : Sensor
    {
    }

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

    public class DavisWeatherSensor : WeatherSensor
    {
        protected override SensorReading ReadValues()
        {
            return new FineOffsetWeatherReading() { };

        }
    }


    public class DavisWeatherSensorReading : SensorReading
    {

    }

    public class XL2 : Sensor
    {
        private XL2Reading _lastReading;
        private SerialPort ComPort;
        private List<XL2Metric> MetricsToRead;



        public XL2(List<XL2Metric> metrics, string range, string RTAFreq, string RTATime, string phan, string klock, string Spk, string SpkLvl, string Input, string RTARes)
        {
            MetricsToRead = metrics;

            // Create XL2 Com Port based on Vendor ID and Product ID
            // Scan through each SerialPort registered in the WMI.
            foreach (ManagementObject device in new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_SerialPort").Get())
            {
                // Select device based on VendorID.
                if (device["PNPDeviceID"].ToString().Contains("VID_1A2B"))
                {
                    ComPort = new SerialPort(device["DeviceID"].ToString());
                    //Console.WriteLine("created Xl2 port");
                    //Console.WriteLine(ComPort.PortName);
                }
            }

            //ComPort = new SerialPort(comName);//for use if the other method isn't working

            //Send initialisation settings to the logger.
            ComPort.Open();
            ComPort.Write("INIT STOP\n");
            ComPort.Write($"INPUT:RANGE {range} \n");
            ComPort.Write($"MEAS:SLM:RTA:WEIG {RTAFreq}{RTATime} \n"); 
            ComPort.Write($"INPU:PHAN {phan} \n");
            ComPort.Write($"SYST:KLOC {klock} \n");
            ComPort.Write($"SYST:SPEA:ONOF {Spk}");
            ComPort.Write($"SYST:SPEA:LEVE {SpkLvl} \n");
            ComPort.Write($"INPU:SELE {Input} \n");
            ComPort.Write($"MEA:SLM:RTA:RESO {RTARes} \n");
            Console.WriteLine("XL2 initialse strings sent");

        }

        protected override void CommenceMeasurement()
        {
            // Start
            if (!ComPort.IsOpen)
            {
                ComPort.Open();
            }
            System.Threading.Thread.Sleep(100);
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

    //Start of data collection classes from XL2
    public class IDN : XL2Metric
    {
        public override string Command => "*IDN?\n";
    }

    public class MicType : XL2Metric
    {
        public override string Command => "CALI:MIC:TYPE?\n";
    }
    public class MicSens : XL2Metric
    {
        public override string Command => "CALIB:MIC:SENS:VALU?\n";
    }

    public class LAEQ : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? LAEQ\n";
    }

    public class LAFMAX : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? LAFMAX\n";
    }

    public class LAFMIN : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? LAFMIN\n";
    }

    public class LAF01 : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? L1%\n";
    }

    public class LAF05 : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? L5%\n";
    }

    public class LAF10 : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? L10%\n";
    }

    public class LAF50 : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? L50%\n";
    }

    public class LAF90 : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? L90%\n";
    }

    public class LAF95 : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? L95%\n";
    }

    public class LAF99 : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? L99%\n";
    }

    public class LCEQ : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? LCEQ\n";
    }

    public class LCFMAX : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? LCFMAX\n";
    }

    public class LCFMIN : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? LCFMIN\n";
    }

    public class LZEQ : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? LZEQ\n";
    }

    public class LZFMAX : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? LZFMAX\n";
    }

    public class LZFMIN : XL2Metric
    {
        public override string Command => "MEAS:SLM:123? LZFMIN\n";
    }

    // RTA results from XL2
    public class RTAE : XL2Metric
    {
        public override string Command => "MEAS:SLM:RTA? E\n";
    }
    public class RTAEQ : XL2Metric
    {
        public override string Command => "MEAS:SLM:RTA? EQ\n";
    }

    public class RTAMAX : XL2Metric
    {
        public override string Command => "MEAS:SLM:RTA? MAX\n";
    }

    public class RTAMIN : XL2Metric
    {
        public override string Command => "MEAS:SLM:RTA? MIN\n";
    }

    public class RTA01 : XL2Metric
    {
        public override string Command => "MEAS:SLM:RTA? 1%\n";
    }

    public class RTA05 : XL2Metric
    {
        public override string Command => "MEAS:SLM:RTA? 5%\n";
    }

    public class RTA10 : XL2Metric
    {
        public override string Command => "MEAS:SLM:RTA? 10%\n";
    }

    public class RTA50 : XL2Metric
    {
        public override string Command => "MEAS:SLM:RTA? 50%\n";
    }

    public class RTA90 : XL2Metric
    {
        public override string Command => "MEAS:SLM:RTA? 90%\n";
    }

    public class RTA95 : XL2Metric
    {
        public override string Command => "MEAS:SLM:RTA? 95%\n";
    }

    public class RTA99 : XL2Metric
    {
        public override string Command => "MEAS:SLM:RTA? 99%\n";
    }


    
    public class XL2Reading : SensorReading
    {
        public List<XL2MetricReading> MetricReadings;
    }



    public class XL2MetricReading
    {
        public XL2Metric Metric;
        public string Measurement;
    }
}

