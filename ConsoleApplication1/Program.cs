using Matrix.Sensors;
using System;
using System.Collections.Generic;
using System.Management;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Timers;
using MatrixLogger;
using System.IO.Ports;

namespace Matrix
{
    class Program
    {
        static void Main(string[] args)
        {

            var logger = new Logger();
            while (true) { }

        }
    }

    // ACCESSIBILITY
    // public = anything with the dll can access it
    // internal = only stuff within the dll can access it
    // protected = only stuff that inherits form this class can access it
    // private = only this class can access it
    public class Site
    {
        // general information about the site
        public FrequencyEnum Frequency;
    }

    // Static
    // Static stuff exists once in memory no matter how many objects are created
    // Static stuff can be accessed without 'instantiating' up the class


    public class Logger
    {
        // The site this logger is part of
        public Site Site;

        // The sensors in this logger
        public DavisWeatherSensor DavisWeatherSensor;
        public FineOffsetWeatherSensor FineOffsetWeatherSensor;
        public XL2 XL2;

        // a sensors collection
        public List<Sensor> Sensors;

        // dispatches events to schedule
        public Timer Timer;

        public Logger()
        {
            // New up the sensors. This could be based on configuration e.g.
            // if (ConfigurationManager.AppSettings["HasDavisWeatherSensor"] == true) 
            DavisWeatherSensor = new DavisWeatherSensor();
            FineOffsetWeatherSensor = new FineOffsetWeatherSensor();
            XL2 = new XL2(
                    metrics: new List<XL2Metric>
                    {
                        new IDN(),
                        new MicType(),
                        new MicSens(),
                        //new LAEQ(),
                        new LAFMAX(),
                        new LAFMIN(),
                        new LAF01(),
                        new LAF05(),
                        new LAF10(),
                        new LAF50(),
                        new LAF90(),
                        new LAF95(),
                        new LAF99(),
                        new RTAEQ(),
                        new RTAE(),
                        new RTAMAX(),
                        new RTAMIN(),
                        new RTA01(),
                        new RTA05(),
                        new RTA10(),
                        new RTA50(),
                        new RTA90(),
                        new RTA95(),
                        new RTA99()
                    },
                    range: "LOW", // LOW MED HIGH
                    RTAFreq: "Z", // Z A C
                    RTATime: "F", // F S
                    phan: "ON", // ON OFF - should be on for microphone to work
                    klock: "OFF", // Key lock - should mostly be off
                    Spk: "OFF", // needs to be on for the headphone output to work
                    SpkLvl: "50", // level 0-60 for the output of the speaker or headphone
                    Input: "XLR", // XLR or RCA
                    RTARes: "TERZ" // OCT or TERZ - terz is 3rd octave
                );

            Sensors = new List<Sensor>()
            {
                DavisWeatherSensor,
                FineOffsetWeatherSensor,
                XL2,
            };

            Timer = new Timer(5 * 1000);
            Timer.Elapsed += OnTimer;
            Timer.AutoReset = true;
            Timer.Enabled = true;
        }

        public void OnTimer(Object source, ElapsedEventArgs eventArgs)
        {
            Console.WriteLine("Timer event triggered...");

            try
            {
                Sensors.ForEach(x => x.CompleteMeasurementPeriod());
                System.Threading.Thread.Sleep(500);
                Sensors.ForEach(x => x.CommenceMeasurementPeriod());

                foreach (var s in Sensors)
                {
                    var measurement = s.ReadMeasuredValue();

                    if (measurement is XL2Reading)
                    {
                        var xl2Reading = measurement as XL2Reading;

                        var SLMSerialNo = "";//
                        var micSens = "";//
                        var micType = "";//
                        var RTAs = "";//
                        var measStat = "";
                        var LAEQ = "";//

                        // post to Simon :)
                        //Console.WriteLine(xl2Reading.MetricReadings.Count());
                        //xl2Reading.MetricReadings.ForEach(x => Console.WriteLine($"XL2's {x.Metric.GetType().Name} Measurment is {x.Measurement}"));
                        //xl2Reading.MetricReadings.ForEach(delegate(String xl2reading.MetricReadings.Metric);

                        //RTAs = xl2Reading.MetricReadings.Where(n => n.Metric.GetType().Name.Contains("RTA"));
                        LoggerEntities LE = new LoggerEntities();
                        List<XL2Spectrum> specPost = new List<XL2Spectrum>();

                        List<string> measChannels = new List<string>();
                        measChannels.Add("L");
                        measChannels.Add("RTA");

                        List<string> freqWeight = new List<string>();
                        freqWeight.Add("A");
                        freqWeight.Add("C");
                        freqWeight.Add("Z");

                        List<string> timeWeight = new List<string>();
                        timeWeight.Add("F");
                        timeWeight.Add("S");
                        timeWeight.Add("I");

                        List<string> metrics = new List<string>();
                        metrics.Add("EQ");
                        metrics.Add("MAX");
                        metrics.Add("MIN");
                        metrics.Add("01");
                        metrics.Add("05");
                        metrics.Add("10");
                        metrics.Add("50");
                        metrics.Add("90");
                        metrics.Add("95");
                        metrics.Add("99");

                        foreach (var x in xl2Reading.MetricReadings)
                        {
                            Console.WriteLine(x.Metric.GetType().Name);
                            foreach (string m in measChannels)
                            {
                                //Console.WriteLine(n);
                                foreach (string n in freqWeight)
                                {
                                    //Console.WriteLine(m);
                                    foreach (string o in timeWeight)
                                    {
                                        //Console.WriteLine(o);
                                        foreach (string p in metrics)
                                        {
                                            //Console.WriteLine(p);
                                            if (x.Metric.GetType().Name.Contains(m))
                                            {
                                                if (x.Metric.GetType().Name.Contains(n))
                                                {
                                                    if (x.Metric.GetType().Name.Contains(o))
                                                    {
                                                        if (x.Metric.GetType().Name.Contains(p))
                                                        {
                                                            //Console.WriteLine(x.Metric.GetType().Name);
                                                            //Console.WriteLine(x.Measurement);
                                                            Console.WriteLine(m + n + o + p);
                                                            List<string> q = x.Measurement.Split(',').ToList<string>();
                                                            measStat = q[1];
                                                            //Console.WriteLine(measStat);
                                                            Console.WriteLine(char.ToString(measStat[0]));
                                                            specPost.Add(new XL2Spectrum()
                                                            {
                                                                XL2id = 1,
                                                                InChn = n,
                                                                FreqWeight = m,
                                                                Metric = o,
                                                                Overall = q[0]
                                                            }
                                                            );

                                                            specPost.ForEach(Console.WriteLine);

                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //LE.XL2Spectrum.AddRange(specPost);
                            //LE.SaveChanges();
                            //if (x.Metric.GetType().Name.Contains("RTA"))
                            //{
                            //    //Console.WriteLine("Found RTA");
                            //    if(x.Metric.GetType().Name.Contains("50"))
                            //    {
                            //        //Console.WriteLine("Found 50");
                            //        //split data
                            //        List<string> datas = x.Measurement.Split(',').ToList<string>();

                            //    };
                            //    //Console.WriteLine(x.Metric.GetType().Name);
                            //    //Console.WriteLine(x.Measurement);
                            //};
                            //if (x.Metric.GetType().Name.Contains("IDN"))
                            //{
                            //    //Console.WriteLine(x.Metric.GetType().Name);
                            //    List<string> p = x.Measurement.Split(',').ToList<string>();
                            //    SLMSerialNo = p[2];
                            //};
                            //if (x.Metric.GetType().Name.Contains("MicSens"))
                            //{
                            //    List<string> p = x.Measurement.Split(',').ToList<string>();
                            //    micSens = p[0];
                            //}
                            //if (x.Metric.GetType().Name.Contains("MicType"))
                            //{
                            //    List<string> p = x.Measurement.Split(',').ToList<string>();
                            //    micType = p[0];
                            //}


                        }

                        //put in database
                        //using (LoggerEntities context = new LoggerEntities())
                        //{
                        //    XL2Table CurMes = new XL2Table
                        //    {
                        //        SLMSerial = SLMSerialNo,
                        //        MicType = micType,
                        //        MicSens = micSens,
                        //        MeasureStatus = measStat

                        //    };
                        //    context.XL2Table.Add(CurMes);
                        //    context.SaveChanges();
                        //}
                    }

                    if (measurement is FineOffsetWeatherReading)
                    {

                    }

                    if (measurement is ErrorReadingSensor)
                    {
                        var error = measurement as ErrorReadingSensor;
                        Console.WriteLine($"Error reading sensor {s.GetType().Name}");
                        if (error.CommenceException != null)
                        {
                            Console.WriteLine($"CommenceException {error.CommenceException.Message}");
                        }
                        if (error.CompleteException != null)
                        {
                            Console.WriteLine($"CompleteException {error.CompleteException.Message}");
                        }
                        if (error.ReadException != null)
                        {
                            Console.WriteLine($"ReadException {error.ReadException.Message}");
                        }
                    }
                }
            }
            catch (Exception)
            {
                // decide to try something here.
            }
            
        }

    }

    public class Scheduler
    {

    }

    public enum FrequencyEnum
    {
        FIFTEEN_MINUTES,
        ONE_HOUR
    }

    public class StrConcate
    {
        public static void StrCatList()
        {
            String numberlist = "";
            for (int i=0; i <=100; i++)
            {
                numberlist += i + " ";
            }
            Console.WriteLine(numberlist);
        }
    }

    public class specPost
    {
        public string InChn { get; set; }
        public string XL2id { get; set; }
        public string FreqWeight { get; set; }
        public string TimeWeight { get; set; }
        public string Metric { get; set; }
        public string Hz6 { get; set; }
        public string Hz8 { get; set; }
        public string Hz10 { get; set; }
        public string Hz12 { get; set; }
        public string Hz16 { get; set; }
        public string Hz20 { get; set; }
        public string Hz25 { get; set; }
        public string Hz31 { get; set; }
        public string Hz40 { get; set; }
        public string Hz50 { get; set; }
        public string Hz63 { get; set; }
        public string Hz80 { get; set; }
        public string Hz100 { get; set; }
        public string Hz125 { get; set; }
        public string Hz160 { get; set; }
        public string Hz200 { get; set; }
        public string Hz250 { get; set; }
        public string Hz315 { get; set; }
        public string Hz400 { get; set; }
        public string Hz500 { get; set; }
        public string Hz630 { get; set; }
        public string Hz800 { get; set; }
        public string Hz1000 { get; set; }
        public string Hz1250 { get; set; }
        public string Hz1600 { get; set; }
        public string Hz2000 { get; set; }
        public string Hz2500 { get; set; }
        public string Hz3150 { get; set; }
        public string Hz4000 { get; set; }
        public string Hz5000 { get; set; }
        public string Hz6300 { get; set; }
        public string Hz8000 { get; set; }
        public string Hz10000 { get; set; }
        public string Hz12500 { get; set; }
        public string Hz16000 { get; set; }
        public string Hz20000 { get; set; }
        public string Overall { get; set; }
    }

    public class XL2Post
    {
        public string StartTimeOfMeasurement { get; set; }
        public string GPSStartTimeOfMeasurement{ get; set; }
        public string SensorStartTimeOfMeasurement{ get; set; }
        public string SLMSerial{ get; set; }
        public string MicType{ get; set; }
        public string MeasureStatus{ get; set; }
        public string MicSens{ get; set; }
}

}
