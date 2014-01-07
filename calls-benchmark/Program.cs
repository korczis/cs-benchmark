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
            var sw = new Stopwatch();

            sw.Start();

            func.Invoke();

            sw.Stop();

            return sw.ElapsedMilliseconds*0.001;
        }

        public static double MeasurePrintAndReturn(string name, Action func)
        {
            double res = Measure(func);

            PrintInfo(name, res, res);

            return res;
        }

        private double RunNormal()
        {
            return MeasurePrintAndReturn("NORMAL", () =>
                {
                    for (int idx = 0; idx < Cycles; ++idx)
                    {
                        MethodNormal();
                    }
                });
        }

        public double RunVirtual(double referenceTime)
        {
            return MeasurePrintAndReturn("VIRTUAL", () =>
                {
                    for (int idx = 0; idx < Cycles; ++idx)
                    {
                        MethodVirtual();
                    }
                });
        }

        public double RunLambda(double referenceTime)
        {
            return MeasurePrintAndReturn("LAMBDA", () =>
                {
                    Action lambda = () => { Result += 1; };

                    for (int idx = 0; idx < Cycles; ++idx)
                    {
                        lambda();
                    }
                });
        }

        public double RunDirectDelegate(double referenceTime)
        {
            return MeasurePrintAndReturn("DIRECT DELEGATE", () =>
                {
                    Action directDelegate = MethodNormal;

                    for (int idx = 0; idx < Cycles; ++idx)
                    {
                        directDelegate();
                    }
                });
        }

        public double RunSystemAction(double referenceTime)
        {
            return MeasurePrintAndReturn("REFLECT DELEGATE (System.Action)", () =>
                {
                    Action reflectAction = GetAction("MethodNormal");

                    for (int idx = 0; idx < Cycles; ++idx)
                    {
                        reflectAction();
                    }
                });
        }

        private double RunSystemDelegate(double referenceTime)
        {
            return MeasurePrintAndReturn("REFLECT DELEGATE (System.Delegate)", () =>
                {
                    Delegate reflectDelegate = GetAction("MethodNormal");

                    for (int idx = 0; idx < Cycles; ++idx)
                    {
                        reflectDelegate.DynamicInvoke();
                    }
                });
        }

        public double RunInvokeAction(double referenceTime)
        {
            return MeasurePrintAndReturn("REFLECT INVOKE", () =>
                {
                    MethodInfo method = GetMethod("MethodNormal");

                    for (int idx = 0; idx < Cycles; ++idx)
                    {
                        method.Invoke(this, null);
                    }
                });
        }

        public double Run()
        {
            double res = 0.0;

            double referenceTime = RunNormal();

            res += referenceTime;

            res += RunVirtual(referenceTime);

            res += RunLambda(referenceTime);

            res += RunDirectDelegate(referenceTime);

            res += RunSystemAction(referenceTime);

            res += RunSystemDelegate(referenceTime);

            res += RunInvokeAction(referenceTime);

            return res;
        }

        private Action GetAction(string method)
        {
            return (Action) Delegate.CreateDelegate(typeof (Action), this, GetMethod(method));
        }

        private MethodInfo GetMethod(string method)
        {
            const BindingFlags flags =
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            return GetType().GetMethod(method, flags);
        }

        private static void PrintInfo(string name, double duration, double reference)
        {
            var lines = new[]
                {
                    name,
                    "::",
                    string.Format("{0:0.0000}s", duration),
                    string.Format("({0:0.00}mil. calls/sec)", (Cycles/duration)/1000000),
                    string.Format("({0:0.0}%)", (duration/reference)*100)
                };

            string msg = string.Join(" ", lines);

            Debug.WriteLine(msg);
            Console.WriteLine(msg);
        }
    }

    internal class Program
    {
        private static int Main(string[] args)
        {
            var instance = new Benchmark();

            double res = instance.Run();

            Console.WriteLine("Result: {0} => {1}", instance.Result, res);

            return 0;
        }
    }
}
