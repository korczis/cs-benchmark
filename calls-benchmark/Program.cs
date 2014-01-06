/* 
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
        private const int Cycles = (int) 1e7;

        public int Result = 0;

        public Benchmark()
        {
            Result = 0;
        }

        public void MethodNormal()
        {
            Result += 1;
        }

        public virtual void MethodVirtual()
        {
            Result += 1;
        }

        private double RunNormal()
        {
            var sw = new Stopwatch();

            sw.Restart();
            for (int idx = 0; idx < Cycles; ++idx)
            {
                MethodNormal();
            }

            sw.Stop();
            double res = sw.ElapsedMilliseconds*0.001;

            PrintInfo("NORMAL", res, res);

            return res;
        }

        private double RunVirtual(double referenceTime)
        {
            var sw = new Stopwatch();

            sw.Restart();
            for (var idx = 0; idx < Cycles; ++idx)
            {
                MethodVirtual();
            }

            sw.Stop();
            var res = sw.ElapsedMilliseconds*0.001;

            PrintInfo("VIRTUAL", res, referenceTime);
            return res;
        }

        public double RunLambda(double referenceTime)
        {
            var sw = new Stopwatch();

            Action lambda = () => { Result += 1; };

            sw.Restart();
            for (int idx = 0; idx < Cycles; ++idx)
            {
                lambda();
            }

            sw.Stop();
            var res = sw.ElapsedMilliseconds * 0.001;

            PrintInfo("LAMBDA", res, referenceTime);
            return res;
        }

        public double RunDirectDelegate(double referenceTime)
        {
            var sw = new Stopwatch();

            Action directDelegate = MethodNormal;
            sw.Restart();
            for (var idx = 0; idx < Cycles; ++idx)
            {
                directDelegate();
            }

            sw.Stop();
            var res = sw.ElapsedMilliseconds * 0.001;

            PrintInfo("DIRECT DELEGATE", res, referenceTime);
            return res;
        }

        public void RunSystemAction(double referenceTime)
        {
            var sw = new Stopwatch();

            var reflectAction = GetAction("MethodNormal");
            sw.Restart();
            for (var idx = 0; idx < Cycles; ++idx)
            {
                reflectAction();
            }

            sw.Stop();
            var res = sw.ElapsedMilliseconds*0.001;

            PrintInfo("REFLECT DELEGATE (System.Action)", res, referenceTime);
        }

        private double RunSystemDelegate(double referenceTime)
        {
            var sw = new Stopwatch();

            Delegate reflectDelegate = GetAction("MethodNormal");
            sw.Restart();
            for (var idx = 0; idx < Cycles; ++idx)
            {
                reflectDelegate.DynamicInvoke();
            }

            var res = sw.ElapsedMilliseconds*0.001;
            sw.Stop();

            PrintInfo("REFLECT DELEGATE (System.Delegate)", res, referenceTime);
            return res;
        }

        public double RunInvokeAction(double referenceTime)
        {
            var sw = new Stopwatch();

            MethodInfo method = GetMethod("MethodNormal");
            sw.Restart();
            for (int idx = 0; idx < Cycles; ++idx)
            {
                method.Invoke(this, null);
            }

            sw.Stop();
            double res = sw.ElapsedMilliseconds*0.001;

            PrintInfo("REFLECT INVOKE", res, referenceTime);
            return res;
        }

        public void Run()
        {
            var referenceTime = RunNormal();

            RunVirtual(referenceTime);

            RunLambda(referenceTime);

            RunDirectDelegate(referenceTime);

            RunSystemAction(referenceTime);

            RunSystemDelegate(referenceTime);

            RunInvokeAction(referenceTime);
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

            var msg = string.Join(" ", lines);

            Debug.WriteLine(msg);
            Console.WriteLine(msg);
        }
    }

    internal class Program
    {
        private static int Main(string[] args)
        {
            var instance = new Benchmark();

            instance.Run();

            Console.WriteLine("Result: {0}", instance.Result);

            return 0;
        }
    }
}
