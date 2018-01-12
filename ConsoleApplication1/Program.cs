using Matrix.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Timers;
using MySql.Data.MySqlClient;

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

    public class MysqlInsert
    {
        MySqlConnection connection = new MySqlConnection("datasource=localhost; port=3306; username=root; password=matrix");
    }


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
                        new LAEQ(),
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
                    range: "MID"        
                );

            Sensors = new List<Sensor>()
            {
                DavisWeatherSensor,
                FineOffsetWeatherSensor,
                XL2,
            };

            Timer = new Timer(30 * 1000);
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
                Sensors.ForEach(x => x.CommenceMeasurementPeriod());

                foreach (var s in Sensors)
                {
                    var measurement = s.ReadMeasuredValue();

                    if (measurement is XL2Reading)
                    {
                        var xl2Reading = measurement as XL2Reading;

                        // post to Simon :)
                        xl2Reading.MetricReadings.ForEach(x => Console.WriteLine($"XL2's {x.Metric.GetType().Name} Measurment is {x.Measurement}"));
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



}
