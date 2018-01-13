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

            //using (RecipiesEntities context = new RecipiesEntities())
            //{
                //// Insert
                ////Recipe recipe = new Recipe
                ////{
                ////    Name = "chicken salad"
                ////};

                //context.Recipes.Add(recipe);
                //context.SaveChanges();



                // Select
                //Recipe recipe = context.Recipes.FirstOrDefault(r => r.Name == "chicken salad");

                //Console.WriteLine(recipe.Id);

                // Update
                //Recipe recipe = context.Recipes.FirstOrDefault(r => r.Name == "chicken salad");
                //recipe.Name = "Burger";
                //context.SaveChanges();

                //context.Categories.Add(new Category { Name = "Breakfast" });
                //context.Categories.Add(new Category { Name = "Lunch" });

                //context.SaveChanges();

                //linking tabled data
                //Category category = context.Categories.FirstOrDefault(c => c.Name == "Breakfast");
                //context.Recipes.Add(new Recipe { Name = "Cereal", CategoryId = category.Id });
                //context.SaveChanges();

                // using navigation property
                //Category category = context.Categories.FirstOrDefault(c => c.Name == "Lunch");
                //context.Recipes.Add(new Recipe { Name = "Pizza", Category = category });
                //context.SaveChanges();

                // using category navigation property
                //Category category = context.Categories.FirstOrDefault(c => c.Name == "Lunch");
                //category.Recipes.Add(new Recipe { Name = "Soup" });
                //context.SaveChanges();

                //Query
                //Category category = context.Categories.FirstOrDefault(c => c.Name == "Lunch");
                //List<Recipe> recipes = category.Recipes.ToList();
                //recipes.ForEach(r => Console.WriteLine(r.Name));

            //}

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
                    range: "LOW",
                    RTAFreq: "Z",
                    RTATime: "F",
                    phan: "ON",
                    klock: "OFF",
                    Spk: "OFF",
                    SpkLvl: "50",
                    Input: "XLR",
                    RTARes: "TERZ"
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
                //System.Threading.Thread.Sleep(500);
                Sensors.ForEach(x => x.CommenceMeasurementPeriod());

                foreach (var s in Sensors)
                {
                    var measurement = s.ReadMeasuredValue();

                    if (measurement is XL2Reading)
                    {
                        var xl2Reading = measurement as XL2Reading;

                        var SLMSerialNo = "";
                        var micSens = "";
                        var micType = "";
                        var RTAs = "";
                        var measStat = "";

                        // post to Simon :)
                        //Console.WriteLine(xl2Reading.MetricReadings.Count());
                        //xl2Reading.MetricReadings.ForEach(x => Console.WriteLine($"XL2's {x.Metric.GetType().Name} Measurment is {x.Measurement}"));
                        //xl2Reading.MetricReadings.ForEach(delegate(String xl2reading.MetricReadings.Metric);

                        //RTAs = xl2Reading.MetricReadings.Where(n => n.Metric.GetType().Name.Contains("RTA"));

                        foreach (var x in xl2Reading.MetricReadings)
                        {
                            if (x.Metric.GetType().Name.Contains("L"))
                            {
                                if (x.Metric.GetType().Name.Contains("EQ"))
                                {
                                    //Console.WriteLine(x.Metric.GetType().Name);
                                    //Console.WriteLine(x.Measurement);
                                    List<string> p = x.Measurement.Split(',').ToList<string>();
                                    measStat = p[1];
                                    Console.WriteLine(measStat);
                                }
                            };
                            if (x.Metric.GetType().Name.Contains("RTA"))
                            {
                                Console.WriteLine("Found RTA");
                                if(x.Metric.GetType().Name.Contains("50"))
                                {
                                    //Console.WriteLine("Found 50");
                                    //split data
                                    List<string> datas = x.Measurement.Split(',').ToList<string>();

                                };
                                //Console.WriteLine(x.Metric.GetType().Name);
                                //Console.WriteLine(x.Measurement);
                            };
                            if (x.Metric.GetType().Name.Contains("IDN"))
                            {
                                //Console.WriteLine(x.Metric.GetType().Name);
                                List<string> p = x.Measurement.Split(',').ToList<string>();
                                SLMSerialNo = p[2];
                            };
                            if (x.Metric.GetType().Name.Contains("MicSens"))
                            {
                                List<string> p = x.Measurement.Split(',').ToList<string>();
                                micSens = p[0];
                            }
                            if (x.Metric.GetType().Name.Contains("MicType"))
                            {
                                List<string> p = x.Measurement.Split(',').ToList<string>();
                                micType = p[0];
                            }

                        }

                        //put in database
                        using (LoggerEntities context = new LoggerEntities())
                        {
                            XL2Table CurMes = new XL2Table
                            {
                                SLMSerial = SLMSerialNo,
                                MicType = micType,
                                MicSens = micSens,
                                MeasureStatus = measStat

                            };
                            context.XL2Table.Add(CurMes);
                            context.SaveChanges();
                        }
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


}
