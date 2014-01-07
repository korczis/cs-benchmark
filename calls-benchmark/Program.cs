/* 0
 Copyright, 2013, by Tomas Korcak. <korczis@gmail.com> & Vladimir Zadrazil aka Zadr007

 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:

 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
 */

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace cs_calls_benchmark
{
    public class Benchmark
    {
        /// <summary>
        ///     Count of iterations to perform
        /// </summary>
        private const int Cycles = (int) 1e7;

        /// <summary>
        ///     Counter to prevent optimization
        /// </summary>
        public int Result = 0;

        /// <summary>
        ///     Constructor
        /// </summary>
        public Benchmark()
        {
            Result = 0;
        }

        /// <summary>
        ///     Simple non-virtual benchmark method
        /// </summary>
        public void MethodNormal()
        {
            Result += 1;
        }

        /// <summary>
        ///     Virtual benchmark method
        /// </summary>
        public virtual void MethodVirtual()
        {
            Result += 1;
        }

        public static double Measure(Action func)
        {
            //  Create new stop watch instance ...
            var sw = new Stopwatch();

            // Start stop watch instance ...
            sw.Start();

            // Invoke lambda benchmark ...
            func.Invoke();

            // Stop stop watches ...
            sw.Stop();

            // Return elapsed time in seconds ...
            return sw.ElapsedMilliseconds * 0.001;
        }

        public static double MeasurePrintAndReturn(string name, Action func)
        {
            return MeasurePrintAndReturn(name, func, null);
        }

        public static double MeasurePrintAndReturn(string name, Action func, double? referenceTime)
        {
            GC.Collect();

            // Measure ...
            var res = Measure(func);

            var value = res;
            if (referenceTime.HasValue)
            {
                value = referenceTime.Value;
            }

            // Print ..
            PrintInfo(name, res, value);
            
            // And return ...
            return res;
        }

        public double RunNormal(double? referenceTime)
        {
            return MeasurePrintAndReturn("NORMAL", () =>
                {
                    for (var idx = 0; idx < Cycles; ++idx)
                    {
                        MethodNormal();
                    }
                }, referenceTime);
        }

        public double RunVirtual(double? referenceTime)
        {
            return MeasurePrintAndReturn("VIRTUAL", () =>
                {
                    for (var idx = 0; idx < Cycles; ++idx)
                    {
                        MethodVirtual();
                    }
                }, referenceTime);
        }

        public double RunLambda(double? referenceTime)
        {
            return MeasurePrintAndReturn("LAMBDA", () =>
                {
                    Action lambda = () => { Result += 1; };

                    for (var idx = 0; idx < Cycles; ++idx)
                    {
                        lambda();
                    }
                }, referenceTime);
        }

        public double RunDirectDelegate(double? referenceTime)
        {
            return MeasurePrintAndReturn("DIRECT DELEGATE", () =>
                {
                    Action directDelegate = MethodNormal;

                    for (var idx = 0; idx < Cycles; ++idx)
                    {
                        directDelegate();
                    }
                }, referenceTime);
        }

        public double RunSystemAction(double? referenceTime)
        {
            return MeasurePrintAndReturn("REFLECT DELEGATE (System.Action)", () =>
                {
                    var reflectAction = GetAction("MethodNormal");

                    for (var idx = 0; idx < Cycles; ++idx)
                    {
                        reflectAction();
                    }
                }, referenceTime);
        }

        public double RunSystemDelegate(double? referenceTime)
        {
            return MeasurePrintAndReturn("REFLECT DELEGATE (System.Delegate)", () =>
                {
                    Delegate reflectDelegate = GetAction("MethodNormal");

                    for (var idx = 0; idx < Cycles; ++idx)
                    {
                        reflectDelegate.DynamicInvoke();
                    }
                }, referenceTime);
        }

        public double RunInvokeAction(double? referenceTime)
        {
            return MeasurePrintAndReturn("REFLECT INVOKE", () =>
                {
                    var method = GetMethod("MethodNormal");

                    for (var idx = 0; idx < Cycles; ++idx)
                    {
                        method.Invoke(this, null);
                    }
                }, referenceTime);
        }

        public static int WarmUp()
        {
            var res = 0;

            // Log("Engine is warming up ...");

            var sw = new Stopwatch();
            sw.Start();
            while ((sw.ElapsedMilliseconds * 0.001) < 5)
            {
                res = new Random().Next(Cycles);
            }
            sw.Stop();

            // Log("Done ...");

            return res;
        }

        public double Run()
        {
            var res = 0.0;

            var iterations = 0;

            while (iterations++ < 10)
            {
                Log(string.Format("Round #{0}", iterations));

                var referenceTime = res = RunNormal(null);

                res += RunVirtual(referenceTime);

                res += RunLambda(referenceTime);

                res += RunDirectDelegate(referenceTime);

                res += RunSystemAction(referenceTime);

                res += RunSystemDelegate(referenceTime);

                res += RunInvokeAction(referenceTime);

                Log(string.Empty);
            }

            return res;
        }

        /// <summary>
        /// Get Action by name
        /// </summary>
        /// <param name="method">Name of the action</param>
        /// <returns>Action itself</returns>
        private Action GetAction(string method)
        {
            return (Action) Delegate.CreateDelegate(typeof (Action), this, GetMethod(method));
        }

        /// <summary>
        /// Get Method by name
        /// </summary>
        /// <param name="method">Name of the method</param>
        /// <returns>Method itseld</returns>
        private MethodInfo GetMethod(string method)
        {
            // Some binding flags ...
            const BindingFlags flags =
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            
            // Return methody by name and flags ....
            return GetType().GetMethod(method, flags);
        }

        private static void Log(string msg)
        {
            var ts = DateTime.UtcNow.ToString("yyyy/MM/dd hh:mm:ss", CultureInfo.InvariantCulture);

            var fmtMessage = string.Format("[{0}] {1}", ts, msg);

            // Write them to the debug console ...
            Debug.WriteLine(fmtMessage);

            // Write them to the regular command-line console
            Console.WriteLine(fmtMessage);

            // And now just be fine ;-)
        }

        /// <summary>
        /// Print brief named info about execution time(s)
        /// </summary>
        /// <param name="name">Name of the main test</param>
        /// <param name="duration">Duration of the main test</param>
        /// <param name="reference">Duration of the refence test</param>
        private static void PrintInfo(string name, double duration, double reference)
        {
            // TODO: Where is the NiceFormat Method, korczis?

            // Construct the lines
            var lines = new[]
                {
                    name,
                    "::",
                    string.Format("{0:0.0000}s", duration),
                    string.Format("({0:0.00}mil. calls/sec)", (Cycles/duration)/1000000),
                    string.Format("({0:0.0}%)", (duration/reference)*100)
                };

            // Joins them to the message
            var msg = string.Join(" ", lines);

            Log(msg);
        }
    }

    /// <summary>
    /// C# Calls Performance Benchmark Main Class
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Application Entrypoint
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Return values</returns>
        private static int Main(string[] args)
        {
            // Create instance of benchmark class
            var instance = new Benchmark();

            Benchmark.WarmUp();

            // Run that instance 
            var res = instance.Run();

            // Print results ... ( => that stuff which prevented optimizer to optimize too much)
            Console.WriteLine("Result: {0} => {1}", instance.Result, res);

            // Yeah, everything is all right (EXIT_SUCCESS as we say in c/c++)
            return 0;
        }
    }
}
