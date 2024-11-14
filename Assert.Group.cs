using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DevTools
{
    public static partial class Assert
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        internal class GroupAttribute : Attribute
        {
            internal const string __FILE__BOOLEAN_CONDITION_CHECKS   = "BOOLEAN_CONDITION_CHECKS";
            internal const string __FILE__NULL_CHECKS                = "NULL_CHECKS";
            internal const string __FILE__FILE_PATH_CHECKS           = "FILE_PATH_CHECKS";
            internal const string __FILE__ARRAY_BOUNDS_CHECKS        = "ARRAY_BOUNDS_CHECKS";
            internal const string __FILE__COMPARISON_CHECKS          = "COMPARISON_CHECKS";
            internal const string __FILE__ARITHMETIC_LOGIC_CHECKS    = "ARITHMETIC_LOGIC_CHECKS";
            internal const string __FILE__MEMORY_CHECKS              = "MEMORY_CHECKS";

            /// <summary> Boolean Condition Checks </summary>
            internal const string __NAME__BOOLEAN_CONDITION_CHECKS   = "Boolean Condition Checks";
            /// <summary> Null Checks </summary>
            internal const string __NAME__NULL_CHECKS                = "Null Checks";
            /// <summary> File Path Checks </summary>
            internal const string __NAME__FILE_PATH_CHECKS           = "File Path Checks";
            /// <summary> Array Bounds Checks </summary>
            internal const string __NAME__ARRAY_BOUNDS_CHECKS        = "Array Bounds Checks";
            /// <summary> Comparison Checks </summary>
            internal const string __NAME__COMPARISON_CHECKS          = "Comparison Checks";
            /// <summary> Arithmetic-Logic Checks </summary>
            internal const string __NAME__ARITHMETIC_LOGIC_CHECKS    = "Arithmetic-Logic Checks";
            /// <summary> Memory Checks </summary>
            internal const string __NAME__MEMORY_CHECKS              = "Memory Checks";

            private GroupAttribute() { }
            internal GroupAttribute(string publicName)
            {
                PublicName = publicName;
                FileContent = Defines.Where(grp => grp.PublicName == publicName).First().FileContent;
            }

            internal string FileContent { get; private set; }
            internal string PublicName { get; private set; }

            internal static Assert.GroupAttribute[] Defines => new Assert.GroupAttribute[]
            {
                new Assert.GroupAttribute{ FileContent = __FILE__BOOLEAN_CONDITION_CHECKS, PublicName = __NAME__BOOLEAN_CONDITION_CHECKS },
                new Assert.GroupAttribute{ FileContent = __FILE__NULL_CHECKS,              PublicName = __NAME__NULL_CHECKS              },
                new Assert.GroupAttribute{ FileContent = __FILE__FILE_PATH_CHECKS,         PublicName = __NAME__FILE_PATH_CHECKS         },
                new Assert.GroupAttribute{ FileContent = __FILE__ARRAY_BOUNDS_CHECKS,      PublicName = __NAME__ARRAY_BOUNDS_CHECKS      },
                new Assert.GroupAttribute{ FileContent = __FILE__COMPARISON_CHECKS,        PublicName = __NAME__COMPARISON_CHECKS        },
                new Assert.GroupAttribute{ FileContent = __FILE__ARITHMETIC_LOGIC_CHECKS,  PublicName = __NAME__ARITHMETIC_LOGIC_CHECKS  },
                new Assert.GroupAttribute{ FileContent = __FILE__MEMORY_CHECKS,            PublicName = __NAME__MEMORY_CHECKS            }
            };
            private static string[] KnownUsingsWithAssertClasses => new string[]
            {
                "using NUnit.Framework;",
                "using UnityEngine.Assertions",
                "using static System.Diagnostics.Debug;"
            };

            internal static async Task<Dictionary<Assert.GroupAttribute, uint>> CountMethodCallsAsync(string projectPath)
            {
                try
                {
                    Dictionary<MethodInfo, Assert.GroupAttribute> methodToGroupMap = GetAssertionsMappedToGroups();
                    List<Task<Dictionary<Assert.GroupAttribute, uint>>> jobs = CreateCountingTasks(methodToGroupMap, projectPath);

                    return await CombineResults(jobs);
                }
                catch (Exception ex)
                {
                    ex.Log();
                    return null;
                }
            }
            private static bool ContainsOverload(Dictionary<MethodInfo, Assert.GroupAttribute> result, MethodInfo method)
            {
                foreach (KeyValuePair<MethodInfo, Assert.GroupAttribute> item in result)
                {
                    if (item.Key.Name == method.Name)
                    {
                        return true;
                    }
                }

                return false;
            }
            private static Dictionary<MethodInfo, Assert.GroupAttribute> GetAssertionsMappedToGroups()
            {
                Dictionary<MethodInfo, Assert.GroupAttribute> result = new Dictionary<MethodInfo, Assert.GroupAttribute>();

                foreach (MethodInfo method in typeof(Assert).GetMethods())
                {
                    Assert.GroupAttribute attribute = method.GetCustomAttribute<Assert.GroupAttribute>(false);
                    if (attribute != null && !ContainsOverload(result, method))
                    {
                        result.Add(method, attribute);
                    }
                }

                return result;
            }
            private static string GetMethodPrefixFromUsingStatements(string script)
            {
                if (script.Contains("using static DevTools.Assert;"))
                {
                    return string.Empty;
                }
                else if (script.Contains("using DevTools;") && KnownUsingsWithAssertClasses.All(__using => !script.Contains(__using)))
                {
                    return "Assert.";
                }
                else
                {
                    return "DevTools.Assert.";
                }
            }
            private static uint CountSubstrings(string instance, string value)
            {
Assert.IsFalse(string.IsNullOrEmpty(value));

                uint count = 0;
                int index = instance.IndexOf(value);
                while (index != -1 & index < instance.Length)
                {
                    count++;
                    index = instance.IndexOf(value, index + value.Length);
                }

                return count;
            }
            private static List<Task<Dictionary<Assert.GroupAttribute, uint>>> CreateCountingTasks(Dictionary<MethodInfo, Assert.GroupAttribute> methodToGroupMap, string path)
            {
                List<Task<Dictionary<Assert.GroupAttribute, uint>>> tasks = new List<Task<Dictionary<Assert.GroupAttribute, uint>>>(256);

                DirectoryExtensions.ForEachFile(path,
                (file) =>
                {
                    if (Path.GetExtension(file) != ".cs")
                    {
                        return;
                    }

                    tasks.Add(Task<Dictionary<Assert.GroupAttribute, uint>>.Factory.StartNew(
                    () =>
                    {
                        Dictionary<Assert.GroupAttribute, uint> callCounts = new Dictionary<Assert.GroupAttribute, uint>();
                        string script = File.ReadAllText(file);
                        string prefix = GetMethodPrefixFromUsingStatements(script);

                        foreach (KeyValuePair<MethodInfo, Assert.GroupAttribute> methodMapping in methodToGroupMap)
                        {
                            Assert.GroupAttribute group = methodMapping.Value;
                            uint numCalls = CountSubstrings(script, prefix + methodMapping.Key.Name);

                            if (callCounts.ContainsKey(group))
                            {
                                callCounts[group] += numCalls;
                            }
                            else
                            {
                                callCounts.Add(group, numCalls);
                            }
                        }

                        return callCounts;
                    }));
                });

                return tasks;
            }
            private static async Task<Dictionary<Assert.GroupAttribute, uint>> CombineResults(List<Task<Dictionary<Assert.GroupAttribute, uint>>> jobs)
            {
                Dictionary<Assert.GroupAttribute, uint> result = await jobs[0]; // at the very least this very script is assigned a job

                for (int i = 1; i < jobs.Count; i++)
                {
                    foreach (KeyValuePair<Assert.GroupAttribute, uint> callCount in await jobs[i])
                    {
                        result[callCount.Key] += callCount.Value;
                    }
                }

                return result;
            }
        }
    }
}
