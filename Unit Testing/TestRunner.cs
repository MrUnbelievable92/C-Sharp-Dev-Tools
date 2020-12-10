using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DevTools
{
    public static class TestRunner
    {
        internal static List<UnitTestData> Tests;

        internal static int NumPassedTests;
        internal static int NumFailedTests;


        internal static void LoadTests()
        {
            Tests = new List<UnitTestData>();
            List<UnitTestData> wrongSignature = new List<UnitTestData>();

            NumFailedTests = NumPassedTests = 0;


            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyName = assembly.GetName().Name;

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

                                    Tests.Add(data);
                                }
                            }
                            else continue;
                        }
                    }
                    }
            }

            Tests.Sort(new UnitTestData.Comparer());
            wrongSignature.Sort(new UnitTestData.Comparer());

            foreach (UnitTestData data in wrongSignature)
            {
                UnityEngine.Debug.LogError("<color=red>WRONG METHOD SIGNATURE</color> - Methods must be static, parameterless and return a boolean, representing the test result: " + data.ToString());
            }
        }


        private static bool RunSingleTest(int index, TestReporter reporter)
        { 
            try
            {
                if ((bool)Tests[index].Test.Method.Invoke(null, null))
                {
                    Tests[index].Result = TestResult.Passed;
                    NumPassedTests++;
                }
                else
                {
                    Tests[index].Result = TestResult.Failed;
                    NumFailedTests++;
                }

                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"<color=red>UNHANDLED EXCEPTION</color>\n{ e.ToString() + " " + e.InnerException.Message }");

                reporter.exception = true;

                return false;
            }
        }

        public static void RunAllTests()
        {
            using (TestReporter reporter = new TestReporter())
            {
                for (int i = 0; i < Tests.Count; i++)
                {
                    if(!RunSingleTest(i, reporter))
                    {
                        return;
                    }
                }
            }
        }

        public static void RunAllTests(string assemblyName, params string[] categories)
        {
            using (TestReporter reporter = new TestReporter())
            {
                int firstIndex = 0;


                while (Tests[firstIndex].AssemblyName != assemblyName)
                {
                    firstIndex++;
                }


                while (Tests[firstIndex].AssemblyName == assemblyName)
                {
                    bool allCategoriesPresent = true;

                    if (categories != null)
                    {
                        foreach (string category in categories)
                        {
                            allCategoriesPresent &= Tests[firstIndex].Categories.Contains(category);
                        }
                    }
                    else { }

                    
                    if (allCategoriesPresent)
                    {
                        if (RunSingleTest(firstIndex, reporter))
                        {
                            firstIndex++;
                        }
                        else return;
                    }
                    else { }
                }
            }
        }
    }
}