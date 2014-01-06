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

namespace cs_calls_benchmark
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    public class Benchmark
    {
        private const int CYCLES = (int)1e7;

        public int Result = 0;

        public Benchmark()
        {
            this.Result = 0;
        }

        public void MethodNormal()
        {
            this.Result += 1;
        }

        public virtual void MethodVirtual()
        {
            this.Result += 1;
        }

        public void Run()
        {
            var sw = new Stopwatch();

            sw.Restart();
            for (int idx = 0; idx < CYCLES; ++idx)
            {
                MethodNormal();
            }
            sw.Stop();
            double normal_time = sw.ElapsedMilliseconds*0.001;
            PrintInfo("NORMAL", normal_time, normal_time);


            sw.Restart();
            for (int idx = 0; idx < CYCLES; ++idx)
            {
                MethodVirtual();
            }
            sw.Stop();
            PrintInfo("VIRTUAL", sw.ElapsedMilliseconds*0.001, normal_time);


            Action lambda = () => { this.Result += 1; };

            sw.Restart();
            for (int idx = 0; idx < CYCLES; ++idx)
            {
                lambda();
            }
            sw.Stop();
            PrintInfo("LAMBDA", sw.ElapsedMilliseconds*0.001, normal_time);


            Action direct_delegate = MethodNormal;
            sw.Restart();
            for (int idx = 0; idx < CYCLES; ++idx)
            {
                direct_delegate();
            }
            sw.Stop();
            PrintInfo("DIRECT DELEGATE", sw.ElapsedMilliseconds*0.001, normal_time);


            Action reflect_action = GetAction("MethodNormal");
            sw.Restart();
            for (int idx = 0; idx < CYCLES; ++idx)
            {
                reflect_action();
            }
            sw.Stop();
            PrintInfo("REFLECT DELEGATE (System.Action)", sw.ElapsedMilliseconds*0.001, normal_time);


            Delegate reflect_delegate = GetAction("MethodNormal");
            sw.Restart();
            for (int idx = 0; idx < CYCLES; ++idx)
            {
                reflect_delegate.DynamicInvoke();
            }
            sw.Stop();
            PrintInfo("REFLECT DELEGATE (System.Delegate)", sw.ElapsedMilliseconds*0.001, normal_time);


            MethodInfo method = GetMethod("MethodNormal");
            sw.Restart();
            for (int idx = 0; idx < CYCLES; ++idx)
            {
                method.Invoke(this, null);
            }
            sw.Stop();
            PrintInfo("REFLECT INVOKE", sw.ElapsedMilliseconds*0.001, normal_time);


            /*
            sw.Restart();
            for (int idx = 0; idx < CYCLES; ++idx)
            {
                SendMessage("Normal");
            }
            sw.Stop();
            PrintInfo("SEND MESSAGE", sw.ElapsedMilliseconds * 0.001, normal_time);


            sw.Restart();
            for (int idx = 0; idx < CYCLES; ++idx)
            {
                BroadcastMessage("Normal");
            }
            sw.Stop();
            PrintInfo("BROADCAST MESSAGE", sw.ElapsedMilliseconds * 0.001, normal_time);
            */
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

        private void PrintInfo(string name, double duration, double reference)
        {
            var lines = new[]
                {
                    name,
                    "::",
                    string.Format("{0:0.0000}s", duration),
                    string.Format("({0:0.00}mil. calls/sec)", (CYCLES/duration)/1000000),
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
