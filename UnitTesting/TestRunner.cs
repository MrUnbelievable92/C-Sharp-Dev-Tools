using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DevTools
{
    public class TestRunner
    {
        private List<UnitTestData> tests;
        private List<UnitTestData> wrongSignature;

        public int numPassedTests;
        public int numFailedTests;


        public void LoadTests()
        {
            tests = new List<UnitTestData>();
            wrongSignature = new List<UnitTestData>();


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
                UnityEngine.Debug.LogError("<color=red>WRONG SIGNATURE</color> - Methods must be static, parameterless and return a boolean, representing the test result: " + data.ToString());
            }
        }


        private void Run(int index)
        {
            if ((bool)tests[index].Test.Method.Invoke(null, null))
            {
                UnityEngine.Debug.Log("<color=green>PASSED</color> - " + tests[index].ToString());

                tests[index].Result = TestResult.Passed;
                numPassedTests++;
            }
            else
            {
                UnityEngine.Debug.LogError("<color=red>FAILED</color> - " + tests[index].ToString());

                tests[index].Result = TestResult.Failed;
                numFailedTests++;
            }
        }
        public void RunTests()
        {
            numFailedTests = numPassedTests = 0;

            UnityEngine.Debug.Log($"Commencing { tests.Count } tests");

            Stopwatch time = new Stopwatch();
            time.Start();

            for (int i = 0; i < tests.Count; i++)
            {
                Run(i);
            }

            time.Stop();

            UnityEngine.Debug.Log($"{ numPassedTests } passed - { numFailedTests } failed tests in { (float)time.ElapsedMilliseconds / 1000f } seconds");
        }
        public void RunTests(string assemblyName, params string[] categories)
        {
            int firstIndex = 0;

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
                    Run(firstIndex);
                }
                else { }


                firstIndex++;
            }
        }
    }
}