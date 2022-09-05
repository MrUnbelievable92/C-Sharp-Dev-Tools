#if DEBUG

#define BOOLEAN_CONDITION_CHECKS
#define NULL_CHECKS
#define FILE_PATH_CHECKS
#define ARRAY_BOUNDS_CHECKS
#define COMPARISON_CHECKS
#define ARITHMETIC_LOGIC_CHECKS
#define MEMORY_CHECKS

#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// CONDITIONAL ATTRIBUTE DOESN'T WORK AS EXPECTED WITH UNITY

// strings cannot be passed as arguments if the functions are to work with Unity.Burst
namespace DevTools
{
    public static class Assert
    {
        #region Reflection Utils
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
        

            internal static async Task<Dictionary<Assert.GroupAttribute, ulong>> CountMethodCallsAsync(string projectPath)
            {
                static Dictionary<MethodInfo, Assert.GroupAttribute> GetAssertionsMappedToGroups()
                {
                    static bool ContainsOverload(Dictionary<MethodInfo, Assert.GroupAttribute> result, MethodInfo method)
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
    
                static List<Task<Dictionary<Assert.GroupAttribute, ulong>>> CreateTasks(Dictionary<MethodInfo, Assert.GroupAttribute> methodToGroupMap, string path)
                {
                    static string GetMethodPrefixFromUsingStatements(string script)
                    {
                        if (script.Contains("using static DevTools.Assert;"))
                        {
                            return string.Empty;
                        }
                        else if (script.Contains("using DevTools;") && 
                                !script.Contains("using NUnit.Framework;") && 
                                !script.Contains("using static System.Diagnostics.Debug;"))
                        {
                            return "Assert.";
                        }
                        else
                        {
                            return "DevTools.Assert.";
                        }
                    }
                    
                    static uint CountSubstrings(string instance, string value)
                    {
    Assert.IsFalse(string.IsNullOrEmpty(value));
    
                        uint count = 0;
                        int index = instance.IndexOf(value);
                        while (index != -1)// & index < instance.Length)
                        {
                            count++;
                            index = instance.IndexOf(value, index + value.Length);
                        }
                        
                        return count;
                    }
    

                    List<Task<Dictionary<Assert.GroupAttribute, ulong>>> tasks = new List<Task<Dictionary<Assert.GroupAttribute, ulong>>>(256);

                    DirectoryExtensions.ForEachFile(path,
                    (file) =>
                    {
                        if (Path.GetExtension(file) != ".cs")
                        {
                            return;
                        }
    
                        tasks.Add(Task<Dictionary<Assert.GroupAttribute, ulong>>.Factory.StartNew(
                        () => 
                        {
                            Dictionary<Assert.GroupAttribute, ulong> callCounts = new Dictionary<Assert.GroupAttribute, ulong>();
                            string script = File.ReadAllText(file);
                            string prefix = GetMethodPrefixFromUsingStatements(script);
    
                            foreach (KeyValuePair<MethodInfo, Assert.GroupAttribute> methodMapping in methodToGroupMap)
                            {
                                Assert.GroupAttribute group = methodMapping.Value;
                                ulong numCalls = CountSubstrings(script, prefix + methodMapping.Key.Name);
    
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
    
                static async Task<Dictionary<Assert.GroupAttribute, ulong>> CombineResults(List<Task<Dictionary<Assert.GroupAttribute, ulong>>> jobs, Dictionary<MethodInfo, Assert.GroupAttribute> methodToGroupMap)
                {
                    static void SubtractMethodDefinitions(Dictionary<Assert.GroupAttribute, ulong> result, Dictionary<MethodInfo, Assert.GroupAttribute> methodToGroupMap)
                    {
                        foreach (KeyValuePair<MethodInfo, Assert.GroupAttribute> mapping in methodToGroupMap)
                        {
                            result[methodToGroupMap[mapping.Key]] -= 1;
                        }
                    }
    
    
                    Dictionary<Assert.GroupAttribute, ulong> result = await jobs[0]; // at the very least this very script is assigned a job
                    SubtractMethodDefinitions(result, methodToGroupMap);
                    
                    for (int i = 1; i < jobs.Count; i++)
                    {
                        foreach (KeyValuePair<Assert.GroupAttribute, ulong> callCount in await jobs[i])
                        {
                            result[callCount.Key] += callCount.Value;
                        }
                    }
    
                    return result;
                }
                
    
                try
                {
                    Dictionary<MethodInfo, Assert.GroupAttribute> methodToGroupMap = GetAssertionsMappedToGroups();
                    List<Task<Dictionary<Assert.GroupAttribute, ulong>>> jobs = CreateTasks(methodToGroupMap, projectPath);
    
                    return await CombineResults(jobs, methodToGroupMap);
                }
                catch (Exception ex)
                { 
                    ex.Log();    
                    return null;
                }
            }
        }
        #endregion

        
        #region BOOLEAN_CONDITION_CHECKS
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__BOOLEAN_CONDITION_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__BOOLEAN_CONDITION_CHECKS)]
        public static void IsTrue(bool condition)
        {
#if BOOLEAN_CONDITION_CHECKS
            if (!condition)
            {
                throw new Exception("Expected 'true'.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__BOOLEAN_CONDITION_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__BOOLEAN_CONDITION_CHECKS)]
        public static void IsFalse(bool condition)
        {
#if BOOLEAN_CONDITION_CHECKS
            if (condition)
            {
                throw new Exception("Expected 'false'.");
            }
#endif
        }
        #endregion

        #region NULL_CHECKS
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__NULL_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__NULL_CHECKS)]
        public static void IsNull(object obj)
        {
#if NULL_CHECKS
            if (obj != null)
            {
                throw new InvalidDataException("Expected null.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__NULL_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__NULL_CHECKS)]
        public static void IsNull<T>(T? obj)
            where T : struct
        {
#if NULL_CHECKS
            if (obj != null)
            {
                throw new InvalidDataException("Expected null.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__NULL_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__NULL_CHECKS)]
        unsafe public static void IsNull(void* ptr)
        {
#if NULL_CHECKS
            if (ptr != null)
            {
                throw new InvalidDataException("Expected null.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__NULL_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__NULL_CHECKS)]
        public static void IsNotNull(object obj)
        {
#if NULL_CHECKS
            if (obj == null)
            {
                throw new NullReferenceException("Expected not-null.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__NULL_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__NULL_CHECKS)]
        public static void IsNotNull<T>(T? obj)
            where T : struct
        {
#if NULL_CHECKS
            if (obj == null)
            {
                throw new NullReferenceException("Expected not-null.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__NULL_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__NULL_CHECKS)]
        unsafe public static void IsNotNull(void* ptr)
        {
#if NULL_CHECKS
            if (ptr == null)
            {
                throw new NullReferenceException("Expected not-null.");
            }
#endif
        }
        #endregion

        #region FILE_PATH_CHECKS
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__FILE_PATH_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__FILE_PATH_CHECKS)]
        public static void FileExists(string path) 
        {
#if FILE_PATH_CHECKS
            IsNotNull(path); // File.Exists only returns 'false' in case 'path' is null (no explicit throw, which is what I want)

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }
#endif
        }
        #endregion

        #region ARRAY_BOUNDS_CHECKS
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS)]
        public static void IsWithinArrayBounds(long index, long arrayLength)
        {
#if ARRAY_BOUNDS_CHECKS
            IsNonNegative(arrayLength);

            if ((ulong)index >= (ulong)arrayLength)
            {
                throw new IndexOutOfRangeException($"{ index } is out of range (length { arrayLength } - 1).");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS)]
        public static void IsWithinArrayBounds(ulong index, ulong arrayLength)
        {
#if ARRAY_BOUNDS_CHECKS
            if (index >= arrayLength)
            {
                throw new IndexOutOfRangeException($"{ index } is out of range (length { arrayLength } - 1).");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS)]
        public static void IsValidSubarray(int index, int numEntries, int arrayLength)
        {
#if ARRAY_BOUNDS_CHECKS
            AreNotEqual(numEntries, 0);
            IsWithinArrayBounds(index, arrayLength);
            IsNonNegative(numEntries);

            if (index + numEntries > arrayLength)
            {
                throw new IndexOutOfRangeException($"{ nameof(index) } + { nameof(numEntries) } is { index + numEntries }, which is larger than length { arrayLength }.");
            }
#endif
        }

        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__ARRAY_BOUNDS_CHECKS)]
        public static void SubarraysDoNotOverlap(int firstIndex, int secondIndex, int firstNumEntries, int secondNumEntries)
        {
#if ARRAY_BOUNDS_CHECKS
            if (firstIndex < secondIndex)
            {
                if (firstIndex + firstNumEntries > secondIndex)
                {
                    throw new IndexOutOfRangeException($"Subarray from { firstIndex } to { firstIndex + firstNumEntries - 1} overlaps with subarray from { secondIndex } to { secondIndex + secondNumEntries - 1 }.");
                }
            }
            else
            {
                if (secondIndex + secondNumEntries > firstIndex)
                {
                    throw new IndexOutOfRangeException($"Subarray from { secondIndex } to { secondIndex + secondNumEntries - 1} overlaps with subarray from { firstIndex } to { firstIndex + firstNumEntries - 1 }.");
                }
            } 
#endif
        }
        #endregion

        #region COMPARISON_CHECKS
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsPositive(long value)
        {
#if COMPARISON_CHECKS
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be positive.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsPositive(float value)
        {
#if COMPARISON_CHECKS
            if (value <= 0f)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be positive.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsPositive(double value)
        {
#if COMPARISON_CHECKS
            if (value <= 0d)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be positive.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsPositive(decimal value)
        {
#if COMPARISON_CHECKS
            if (value <= 0m)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be positive.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNegative(long value)
        {
#if COMPARISON_CHECKS
            if (value >= 0)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be negative.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNegative(float value)
        {
#if COMPARISON_CHECKS
            if (value >= 0f)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be negative.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNegative(double value)
        {
#if COMPARISON_CHECKS
            if (value >= 0d)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be negative.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNegative(decimal value)
        {
#if COMPARISON_CHECKS
            if (value >= 0m)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be negative.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNonNegative(long value)
        {
#if COMPARISON_CHECKS
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be positive or equal to zero.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNonNegative(float value)
        {
#if COMPARISON_CHECKS
            if (value < 0f)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be positive or equal to zero.");
            }
#endif
        }

        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNonNegative(double value)
        {
#if COMPARISON_CHECKS
            if (value < 0d)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be positive or equal to zero.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNonNegative(decimal value)
        {
#if COMPARISON_CHECKS
            if (value < 0m)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be positive or equal to zero.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNotPositive(long value)
        {
#if COMPARISON_CHECKS
            if (value > 0)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be negative or equal to zero.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNotPositive(float value)
        {
#if COMPARISON_CHECKS
            if (value > 0f)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be negative or equal to zero.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNotPositive(double value)
        {
#if COMPARISON_CHECKS
            if (value > 0d)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be negative or equal to zero.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       Remember: Zero is neither positive nor negative.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNotPositive(decimal value)
        {
#if COMPARISON_CHECKS
            if (value > 0m)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be negative or equal to zero.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void AreEqual<T>(T a, T b)
            where T : IEquatable<T>
        {
#if COMPARISON_CHECKS
            if (!a.Equals(b))
            {
                throw new ArgumentOutOfRangeException($"{ a } was expected to be equal to { b }.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void AreNotEqual<T>(T a, T b)
            where T : IEquatable<T>
        {
#if COMPARISON_CHECKS
            if (a.Equals(b))
            {
                throw new ArgumentOutOfRangeException($"{ a } was expected not to be equal to { b }.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        /// <remarks>       The comparison is inclusive.       </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsBetween<T>(T value, T min, T max)
            where T : IComparable<T>
        {
#if COMPARISON_CHECKS
            if ((value.CompareTo(min) < 0) || (value.CompareTo(max) > 0))
            {
                throw new ArgumentOutOfRangeException($"Min: { min }, Max: { max }, Value: { value }.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsSmallerOrEqual<T>(T value, T limit)
            where T : IComparable<T>
        {
#if COMPARISON_CHECKS
            if (value.CompareTo(limit) == 1)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be smaller than or equal to { limit }.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsSmaller<T>(T value, T limit)
            where T : IComparable<T>
        {
#if COMPARISON_CHECKS
            if (value.CompareTo(limit) != -1)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be smaller than { limit }.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsGreaterOrEqual<T>(T value, T limit)
            where T : IComparable<T>
        {
#if COMPARISON_CHECKS
            if (value.CompareTo(limit) == -1)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be greater than or equal to { limit }.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsGreater<T>(T value, T limit)
            where T : IComparable<T>
        {
#if COMPARISON_CHECKS
            if (value.CompareTo(limit) != 1)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected to be greater than { limit }.");
            }
#endif
        }
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNotSmaller<T>(T value, T limit)
            where T : IComparable<T>
        {
#if COMPARISON_CHECKS
            if (value.CompareTo(limit) == -1)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected not to be smaller than { limit }.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__COMPARISON_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__COMPARISON_CHECKS)]
        public static void IsNotGreater<T>(T value, T limit)
            where T : IComparable<T>
        {
#if COMPARISON_CHECKS
            if (value.CompareTo(limit) == 1)
            {
                throw new ArgumentOutOfRangeException($"{ value } was expected not to be greater than { limit }.");
            }
#endif
        }
        #endregion

        #region ARITHMETIC_LOGIC_CHECKS
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__ARITHMETIC_LOGIC_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__ARITHMETIC_LOGIC_CHECKS)]
        unsafe public static void IsSafeBoolean(bool x)
        {
#if ARITHMETIC_LOGIC_CHECKS
            if (*(byte*)&x > 1)
            {
                throw new InvalidDataException($"The numerical value of the bool { nameof(x) } is { *(byte*)&x } which can lead to undefined behavior.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__ARITHMETIC_LOGIC_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__ARITHMETIC_LOGIC_CHECKS)]
        unsafe public static void IsDefinedBitShift<T>(int amount)
            where T : unmanaged
        {
#if ARITHMETIC_LOGIC_CHECKS
            if ((uint)amount >= (uint)sizeof(T) * 8u)
            {
                throw new ArgumentOutOfRangeException($"Shifting a { typeof(T) } by { amount } results in undefined behavior.");
            }
#endif
        }
        
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__ARITHMETIC_LOGIC_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__ARITHMETIC_LOGIC_CHECKS)]
        public static void IsDefinedBitShift<T>(uint amount)
            where T : unmanaged
        {
            IsDefinedBitShift<T>((int)amount);
        }
        #endregion

        #region MEMORY_CHECKS
        /// <summary>       Part of: <inheritdoc cref="Assert.GroupAttribute.__NAME__MEMORY_CHECKS"/>         </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Assert.Group(Assert.GroupAttribute.__NAME__MEMORY_CHECKS)]
        unsafe public static void IsMemoryAligned<T>(T* ptr)
            where T : unmanaged
        {
#if MEMORY_CHECKS
            switch (sizeof(T))
            {
                case 2:
                case 4: 
                case 8:
                case 16:
                case 32:
                case 64:
                {
                    if ((ulong)ptr % (uint)sizeof(T) != 0)
                    {
                        throw new DataMisalignedException($"The address { Dump.Hex((ulong)ptr) } of a { typeof(T) } of size { sizeof(T) } is misaligned by { (ulong)ptr % (uint)sizeof(T) } bytes.");
                    }

                    return;
                }

                default: return;
            }
#endif
        }
        #endregion
    }
}
