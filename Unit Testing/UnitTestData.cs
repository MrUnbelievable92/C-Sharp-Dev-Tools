using System.Collections.Generic;

namespace DevTools
{
    internal class UnitTestData
    {
        internal class Comparer : IComparer<UnitTestData>
        {
            public int Compare(UnitTestData x, UnitTestData y)
            {
                int compare = x.AssemblyName.CompareTo(y.AssemblyName);

                if (compare == 0)
                {
                    if (x.Categories.Length == 0)
                    {
                        return (y.Categories.Length != 0) ? -1 : x.Test.Method.Name.CompareTo(y.Test.Method.Name);
                    }
                    else
                    {
                        if (y.Categories.Length == 0)
                        {
                            return 1;
                        }
                        else
                        {
                            for (int i = 0;   ;   i++)
                            {
                                if (x.Categories.Length == i + 1    &&     y.Categories.Length == i + 1)
                                {
                                    if (0 == (compare = x.Categories[i].CompareTo(y.Categories[i])))
                                    {
                                        return x.Test.Method.Name.CompareTo(y.Test.Method.Name);
                                    }
                                    else return compare;
                                }
                                else
                                {
                                    if (x.Categories.Length == i + 1    &&     y.Categories.Length > i + 1)
                                    {
                                        return -1;
                                    }
                                    else
                                    {
                                        if (x.Categories.Length > i + 1     &&     y.Categories.Length == i + 1)
                                        {
                                            return 1;
                                        }
                                        else continue;
                                    }
                                }
                            }
                        }
                    }
                }
                else return compare;
            }
        }


        internal TestHandler Test;
        internal TestResult Result;
        internal string AssemblyName;
        internal string[] Categories;

        public override string ToString()
        {
            string final = AssemblyName + " ";

            foreach (string category in Categories)
            {
                final += category + " ";
            }

            return final + Test.Method.Name;
        }
    }
}