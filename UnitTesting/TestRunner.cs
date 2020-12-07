using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DevTools
{
    public static class TestRunner
    {
        private class Reporter : IDisposable
        {
            private Stopwatch time;
            private bool dispose;


            public Reporter() : this(true)
            { }

            public Reporter(bool disposeOfTests)
            {
                dispose = disposeOfTests;
                time = new Stopwatch();


                if (TestRunner.tests == null)
                {
                    LoadTests();
                }
                else
                {
                    TestRunner.numFailedTests = 0;
                }


                UnityEngine.Debug.Log($"Commencing { TestRunner.tests.Count } tests");

                time.Start();
            }


            public void Dispose()
            {
                time.Stop();


                foreach (UnitTestData test in TestRunner.tests)
                {
                    if (test.Result > TestResult.None)
                    {
                        UnityEngine.Debug.Log(((test.Result == TestResult.Passed) ? "<color=green>PASSED</color> - " : "<color=red>FAILED</color> - ") 
                                              + test.ToString());
                    }
                    else continue;
                }

                UnityEngine.Debug.Log($"<color=green>{ numPassedTests } passed</color>" +
                                      $" and <color={ ((numFailedTests == 0) ? "green" : "red") }>{ numFailedTests } failed</color> tests" +
                                      $" in { (float)time.ElapsedMilliseconds / 1000f } seconds");

                if (dispose)
                {
                    TestRunner.numFailedTests = 0;
                    TestRunner.tests = null;
                    GC.Collect();
                }
                else return;
            }
        }


        private static List<UnitTestData> tests;
        private static List<UnitTestData> wrongSignature;

        public static int numPassedTests => tests.Count - numFailedTests;
        public static int numFailedTests;


        public static void LoadTests()
        {
            tests = new List<UnitTestData>();
            wrongSignature = new List<UnitTestData>();

            numFailedTests = 0;


            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyName = assembly.GetName().Name;

                if (assemblyName.Length < 6 || (assemblyName.Substring(0, 5) != "Unity" && assemblyName.Substring(0, 6) != "System"))
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        foreach (MethodInfo method in type.GetMethods())
                        {
                            foreach (Attribute attribute in method.GetCustomAttributes(false))
                            {
                                if (attribute is UnitTestAttribute)
                                {
                                    UnitTestData data = new UnitTestData
                                    {
                                        AssemblyName = assemblyName,
                                        Categories = (attribute as UnitTestAttribute).Categories,
                                        Result = TestResult.None
                                    };


                                    bool invalid = method.GetParameters().Length != 0;
                                    invalid |= method.ReturnType != typeof(bool);
                                    invalid |= !method.IsStatic;


                                    if (invalid)
                                    {
                                        wrongSignature.Add(data);
                                    }
                                    else
                                    {
                                        data.Test = method.CreateDelegate(typeof(TestHandler)) as TestHandler;

                                        tests.Add(data);
                                    }
                                }
                                else continue;
                            }
                        }
                    }
                }
                else continue;
            }

            tests.Sort(new UnitTestData.Comparer());
            wrongSignature.Sort(new UnitTestData.Comparer());

            foreach (UnitTestData data in wrongSignature)
            {
                UnityEngine.Debug.LogError("<color=red>WRONG METHOD SIGNATURE</color> - Methods must be static, parameterless and return a boolean, representing the test result: " + data.ToString());
            }

            wrongSignature = null;
        }


        private static void RunSingleTest(int index)
        {
            if ((bool)tests[index].Test.Method.Invoke(null, null))
            {
                tests[index].Result = TestResult.Passed;
            }
            else
            {
                tests[index].Result = TestResult.Failed;
                numFailedTests++;
            }
        }

        public static void RunAllTests()
        {
            using (new TestRunner.Reporter())
            {
                for (int i = 0; i < tests.Count; i++)
                {
                    RunSingleTest(i);
                }
            }
        }

        public static void RunAllTests(string assemblyName, params string[] categories)
        {
            int firstIndex = 0;

            using (new TestRunner.Reporter())
            {
                while (tests[firstIndex].AssemblyName != assemblyName)
                {
                    firstIndex++;
                }

                while (tests[firstIndex].AssemblyName == assemblyName)
                {
                    bool allCategoriesPresent = true;

                    if (categories != null)
                    {
                        foreach (string category in categories)
                        {
                            allCategoriesPresent &= tests[firstIndex].Categories.Contains<string>(category);
                        }
                    }
                    else { }


                    if (allCategoriesPresent)
                    {
                        RunSingleTest(firstIndex);
                    }
                    else { }


                    firstIndex++;
                }
            }
        }
    }
}