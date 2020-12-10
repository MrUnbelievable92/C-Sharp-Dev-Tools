using System;
using System.Diagnostics;

namespace DevTools
{
    internal class TestReporter : IDisposable
    {
        private Stopwatch time;
        private bool dispose;


        public TestReporter() : this(true)
        { }

        public TestReporter(bool disposeOfTests)
        {
            dispose = disposeOfTests;
            time = new Stopwatch();


            if (TestRunner.Tests == null)
            {
                TestRunner.LoadTests();
            }
            else
            {
                TestRunner.NumFailedTests = 0;
            }


            UnityEngine.Debug.Log($"Commencing { TestRunner.Tests.Count } Tests");

            time.Start();
        }


        public void Dispose()
        {
            time.Stop();


            if (TestRunner.Tests.Count > 0)     // else the final (nonsensical) result would be logged to the console
            {
                foreach (UnitTestData test in TestRunner.Tests)
                {
                    // tests that were specified NOT to run should not be logged to the console

                    if (test.Result == TestResult.Passed)
                    {
                        UnityEngine.Debug.Log("<color=green>PASSED</color> - " + test.ToString());
                    }
                    else if (test.Result == TestResult.Failed)
                    {
                        UnityEngine.Debug.LogError("<color=red>FAILED</color> - " + test.ToString());
                    }
                    else continue;
                }


                bool zeroPassedTests = TestRunner.NumPassedTests == 0;
                bool zeroFailedTests = TestRunner.NumFailedTests == 0;
                bool passedTestsSingular = TestRunner.NumPassedTests == 1;
                bool failedTestsSingular = TestRunner.NumFailedTests == 1;

                float passedTime = (float)time.ElapsedMilliseconds / 1000f;
                bool passedTimeSingular = passedTime == 1f;


                UnityEngine.Debug.Log($"<color={ (zeroPassedTests ? "red" : "green") }>{ TestRunner.NumPassedTests } passed</color> test{(passedTestsSingular ? "" : "s")} and " +
                                      $"<color={ (zeroFailedTests ? "green" : "red") }>{ TestRunner.NumFailedTests } failed</color> test{(failedTestsSingular ? "" : "s")} " +
                                      $"in { passedTime } second{(passedTimeSingular ? "" : "s")}");
            }
            else return;


            if (dispose)
            {
                TestRunner.NumFailedTests = 0;
                TestRunner.Tests = null;
                GC.Collect();
            }
            else return;
        }
    }
}