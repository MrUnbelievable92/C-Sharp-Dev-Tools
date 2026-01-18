using System.Collections.Generic;
using System;
using System.Reflection;

namespace DevTools
{
    public static class GenericExtensions
    {
        /// <summary>   Logs a message of type '<typeparamref name="T"/>' to the console.   </summary>
        public static void Log<T>(this T obj)
        {
#if DEBUG
    #if UNITY_EDITOR
            UnityEngine.Debug.Log(obj);
    #else
            Console.WriteLine(obj);
    #endif
#endif
        }

        /// <summary>   Logs a message of type '<typeparamref name="T"/>' to the console.   </summary>
        public static void Log<T>(this T obj, string format, IFormatProvider formatProvider = null)
            where T : IFormattable
        {
            Log(obj.ToString(format, formatProvider));
        }

        /// <summary>   Logs all '<typeparamref name="T"/>' field and property names and their respective values to the console.   </summary>
        public static void LogState<T>(this T obj, bool inherited = false)
        {
            static bool IsInherited<U>(U info, U[][] baseInfos)
                where U : MemberInfo
            {
                for (int j = 0; j < baseInfos.Length; j++)
                {
                    for (int k = 0; k < baseInfos[j].Length; k++)
                    {
                        if (baseInfos[j][k].Name == info.Name)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            static string Add<U>(U info, object obj)
                where U : MemberInfo
            {
                string result;
                if (info is FieldInfo)
                {
                    object value = (info as FieldInfo).GetValue(obj);
                    result = value == null ? "null" : value.ToString();
                }
                else
                {
                    object value = (info as PropertyInfo).GetValue(obj);
                    result = value == null ? "null" : value.ToString();
                }

                return "\t\t" + info.Name + "\t: " + result + "\r\n";
            }

            static string AddMembers<U>(U[] infos, U[][] baseInfos, object obj, bool inherited)
                where U : MemberInfo
            {
                string result = string.Empty;
                for (int i = 0; i < infos.Length; i++)
                {
                    if (baseInfos == null || inherited || !IsInherited(infos[i], baseInfos))
                    {
                        if (!(infos[i] is PropertyInfo) || (infos[i] as PropertyInfo).GetIndexParameters() == null)
                        {
                            result += Add(infos[i], obj);
                        }
                    }
                }

                return result;
            }

            static string AddAllMembers(FieldInfo[] staticFields, PropertyInfo[] staticProperties, FieldInfo[] instanceFields, PropertyInfo[] instanceProperties, FieldInfo[][] staticBaseFields, PropertyInfo[][] staticBaseProperties, FieldInfo[][] instanceBaseFields, PropertyInfo[][] instanceBaseProperties, object obj, bool inherited)
            {
                string result = string.Empty;

                if (staticFields.Length + staticProperties.Length != 0)
                {
                    result += "static-------------------------------------------\r\n";
                    if (staticFields.Length != 0)
                    {
                        result += AddMembers(staticFields, staticBaseFields, obj, inherited);
                    }
                    if (staticProperties.Length != 0)
                    {
                        result += AddMembers(staticProperties, staticBaseProperties, obj, inherited);
                    }
                }
                if (instanceFields.Length + instanceProperties.Length != 0)
                {
                    result += "Instance-----------------------------------------:\r\n";
                    if (instanceFields.Length != 0)
                    {
                        result += AddMembers(instanceFields, instanceBaseFields, obj, inherited);
                    }
                    if (instanceProperties.Length != 0)
                    {
                        result += AddMembers(instanceProperties, instanceBaseProperties, obj, inherited);
                    }
                }

                return result;
            }


            const BindingFlags instanceAccessFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            const BindingFlags staticAccessFlags = (instanceAccessFlags ^ BindingFlags.Instance) | BindingFlags.Static;
            FieldInfo[] instanceFields = typeof(T).GetFields(instanceAccessFlags);
            FieldInfo[] staticFields = typeof(T).GetFields(staticAccessFlags);
            PropertyInfo[] instanceProperties = typeof(T).GetProperties(instanceAccessFlags);
            PropertyInfo[] staticProperties = typeof(T).GetProperties(staticAccessFlags);

            List<Type> baseTypes = new List<Type>(2);
            Type currentType = typeof(T);
            while (currentType.BaseType != null
                && currentType.BaseType != typeof(ValueType)
                && currentType.BaseType != typeof(object))
            {
                currentType = currentType.BaseType;
                baseTypes.Add(currentType);
            }

            FieldInfo[][] instanceBaseFields = new FieldInfo[baseTypes.Count][];
            FieldInfo[][] staticBaseFields = new FieldInfo[baseTypes.Count][];
            PropertyInfo[][] instanceBaseProperties = new PropertyInfo[baseTypes.Count][];
            PropertyInfo[][] staticBaseProperties = new PropertyInfo[baseTypes.Count][];
            for (int i = 0; i < baseTypes.Count; i++)
            {
                instanceBaseFields[i] = baseTypes[i].GetFields(instanceAccessFlags);
                staticBaseFields[i] = baseTypes[i].GetFields(staticAccessFlags);
                instanceBaseProperties[i] = baseTypes[i].GetProperties(instanceAccessFlags);
                staticBaseProperties[i] = baseTypes[i].GetProperties(staticAccessFlags);
            }

            string result = typeof(T).Name + ":\r\n";
            result += AddAllMembers(staticFields, staticProperties, instanceFields, instanceProperties, staticBaseFields, staticBaseProperties, instanceBaseFields, instanceBaseProperties, obj, false);

            if (inherited)
            {
                for (int i = 0; i < baseTypes.Count; i++)
                {
                    if (staticBaseFields[i].Length +
                        staticBaseProperties[i].Length +
                        instanceBaseFields[i].Length +
                        instanceBaseProperties[i].Length != 0)
                    {
                        result += baseTypes[i].Name + " (inherited):\r\n";
                        result += AddAllMembers(staticBaseFields[i], staticBaseProperties[i], instanceBaseFields[i], instanceBaseProperties[i], null, null, null, null, obj, true);
                    }
                }
            }

            result.Log();
        }

    }
}
