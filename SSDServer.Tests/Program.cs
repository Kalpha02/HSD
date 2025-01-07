using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using SSDServer.Tests;
using SSDServer.Tests.Extensions;

public static class Program
{ 
    public static void Main(string[] args)
    {
        ConsoleColor org = Console.ForegroundColor;
        Assembly localAssem = Assembly.GetExecutingAssembly();
        /*if (localAssem.IsAssemblyDebugBuild())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("The test project isn't build in release mode! Please build in release mode when testing!");
            Console.ForegroundColor = org;
            return;
        }*/

        // Get all types where atleast one method needs to be tested
        Type[] typesToTest = localAssem.GetTypes().Where(t => t.GetMethods().Any(m => m.GetCustomAttribute<TestAttribute>() != null)).ToArray();
        Stopwatch methodTimer = new Stopwatch(), classTimer = new Stopwatch();

        object instance = null;
        for (int i = 0; i < typesToTest.Length; ++i)
        {
            instance = Activator.CreateInstance(typesToTest[i]);
            MethodInfo[] testMethods = instance.GetType().GetMethods().Where(m => m.GetCustomAttribute<TestAttribute>() != null).ToArray();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(String.Format("Testing {0}...", typesToTest[i].Name));
            Console.ForegroundColor = org;
            classTimer.Restart();
            for (int j = 0; j < testMethods.Length; ++j)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(String.Format("\tTesting {0}...", testMethods[j].Name));
                    methodTimer.Restart();
                    testMethods[j].Invoke(instance, null);
                    methodTimer.Stop();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(String.Format("\t{0} passed! (Took {1} seconds)", testMethods[j].Name, methodTimer.ElapsedMilliseconds/1000.0));
                    Console.ForegroundColor = org;
                }
                catch (Exception ex)
                {
                    methodTimer.Stop();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(String.Format("\t{0} failed! {1} (Took {2} seconds)", testMethods[j].Name, ex.InnerException.Message, methodTimer.ElapsedMilliseconds/1000.0));
                    Console.ForegroundColor = org;
                }
            }
            instance = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            classTimer.Stop();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(String.Format("{0} took {1} seconds", typesToTest[i].Name, classTimer.ElapsedMilliseconds/1000.0));
            Console.ForegroundColor = org;

        }
    }
}