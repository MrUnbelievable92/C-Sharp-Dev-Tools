#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DevTools.Unity.Editor
{
    internal class SafetyCheckSettingsWindow : EditorWindow
    {
        private const string FOLDER_NAME = "C Sharp Dev Tools";
        private const string FILE_NAME = "Assert.cs";

        private const string __FILE__BOOLEAN_CONDITION_CHECKS   = "#define BOOLEAN_CONDITION_CHECKS";
        private const string __FILE__NULL_CHECKS                = "#define NULL_CHECKS";
        private const string __FILE__FILE_PATH_CHECKS           = "#define FILE_PATH_CHECKS";
        private const string __FILE__ARRAY_BOUNDS_CHECKS        = "#define ARRAY_BOUNDS_CHECKS";
        private const string __FILE__COMPARISON_CHECKS          = "#define COMPARISON_CHECKS";
        private const string __FILE__ARITHMETIC_LOGIC_CHECKS    = "#define ARITHMETIC_LOGIC_CHECKS";
        private const string __FILE__MEMORY_CHECKS              = "#define MEMORY_CHECKS";

        internal const string __NAME__ALL_CHECKS                 = "All Checks";
        internal const string __NAME__BOOLEAN_CONDITION_CHECKS   = "Boolean Condition Checks";
        internal const string __NAME__NULL_CHECKS                = "Null Checks";
        internal const string __NAME__FILE_PATH_CHECKS           = "File Path Checks";
        internal const string __NAME__ARRAY_BOUNDS_CHECKS        = "Array Bounds Checks";
        internal const string __NAME__COMPARISON_CHECKS          = "Comparison Checks";
        internal const string __NAME__ARITHMETIC_LOGIC_CHECKS    = "Arithmetic-Logic Checks";
        internal const string __NAME__MEMORY_CHECKS              = "Memory Checks";


        private bool anythingChanged;

        private bool allChecks;
        private bool booleanConditionChecks;
        private bool nullChecks;
        private bool filePathChecks;
        private bool arrayBoundsChecks;
        private bool comparisonChecks;
        private bool arithmeticLogicChecks;
        private bool memoryChecks;

        
        private static string ScriptPath 
        {
            get 
            {
                static string FindIn(string folder)
                {
                    string result = null;

                    string[] subDirectories = Directory.GetDirectories(folder);

                    foreach (string subDirectory in subDirectories)
                    {
                        if (subDirectory.EndsWith(FOLDER_NAME) && File.Exists(subDirectory + "/" + FILE_NAME))
                        {
                            result = subDirectory + "/" + FILE_NAME;
                        }
                    }

                    if (result == null)
                    {
                        foreach (string subDirectory in subDirectories)
                        {
                            result = FindIn(subDirectory);

                            if (result != null)
                            {
                                break;
                            }
                        }
                    }

                    return result;
                }

                string projectFolder = Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length);
                string defaultPath = projectFolder + "/LocalPackages/" + FOLDER_NAME + "/" + FILE_NAME;
                
                if (File.Exists(defaultPath))
                {
                    return defaultPath;
                }
                else
                {
                    return FindIn(projectFolder) ?? throw new DllNotFoundException($"'{ FOLDER_NAME }/{ FILE_NAME }' was modified and cannot be found.");
                }
            }
        }


        private void SetAll(bool value)
        {
            allChecks              =
            booleanConditionChecks =
            nullChecks             =
            filePathChecks         =
            arrayBoundsChecks      =
            comparisonChecks       =
            arithmeticLogicChecks  =
            memoryChecks           = value;
        }

        private void Update_allChecks()
        {
            allChecks = booleanConditionChecks &&
                        nullChecks             &&
                        filePathChecks         &&
                        arrayBoundsChecks      &&
                        comparisonChecks       &&
                        arithmeticLogicChecks  &&
                        memoryChecks;
        }

        private string UpdateCondition(string fileContent, string condition)
        {
            bool conditionValueInEditor = false;

            switch (condition)
            {
                case __FILE__BOOLEAN_CONDITION_CHECKS: conditionValueInEditor = booleanConditionChecks; break;
                case __FILE__NULL_CHECKS:              conditionValueInEditor = nullChecks;             break;
                case __FILE__FILE_PATH_CHECKS:         conditionValueInEditor = filePathChecks;         break;
                case __FILE__ARRAY_BOUNDS_CHECKS:      conditionValueInEditor = arrayBoundsChecks;      break;
                case __FILE__COMPARISON_CHECKS:        conditionValueInEditor = comparisonChecks;       break;
                case __FILE__ARITHMETIC_LOGIC_CHECKS:  conditionValueInEditor = arithmeticLogicChecks;  break;
                case __FILE__MEMORY_CHECKS:            conditionValueInEditor = memoryChecks;           break;

                default: break;
            }

            if (conditionValueInEditor && !IsEnabled(fileContent, condition))
            {
                fileContent = Enable(fileContent, condition);
            }
            else if (!conditionValueInEditor && IsEnabled(fileContent, condition))
            {
                fileContent = Disable(fileContent, condition);
            }

            return fileContent;
        }

        private void Apply()
        {
            string filePath = ScriptPath;
            StreamReader reader = new StreamReader(filePath);

            string fileContent = reader.ReadToEnd();

            fileContent = UpdateCondition(fileContent, __FILE__BOOLEAN_CONDITION_CHECKS);
            fileContent = UpdateCondition(fileContent, __FILE__NULL_CHECKS);
            fileContent = UpdateCondition(fileContent, __FILE__FILE_PATH_CHECKS);
            fileContent = UpdateCondition(fileContent, __FILE__ARRAY_BOUNDS_CHECKS);
            fileContent = UpdateCondition(fileContent, __FILE__COMPARISON_CHECKS);
            fileContent = UpdateCondition(fileContent, __FILE__ARITHMETIC_LOGIC_CHECKS);
            fileContent = UpdateCondition(fileContent, __FILE__MEMORY_CHECKS);
            
            anythingChanged = false;

            reader.Dispose();
            File.WriteAllText(filePath, fileContent);

            AssetDatabase.Refresh();
        }

        private bool IsEnabled(string fileContent, string condition)
        {
            return fileContent[fileContent.IndexOf(condition) - 1]  == '\n';
        }

        private string Enable(string fileContent, string condition)
        {
            return fileContent.Remove(fileContent.IndexOf(condition) - 2, 2);
        }
        
        private string Disable(string fileContent, string condition)
        {
            return fileContent.Insert(fileContent.IndexOf(condition), "//");
        }

        [MenuItem("Window/C# Dev Tools/Manage Safety Checks...")]
        private static void ShowWindow()
        {
            GetWindow<SafetyCheckSettingsWindow>("C# Dev Tools Safety Checks");
        }

        private void OnEnable()
        {   
            using StreamReader reader = new StreamReader(ScriptPath);
            string fileContent = reader.ReadToEnd();

            booleanConditionChecks = IsEnabled(fileContent, __FILE__BOOLEAN_CONDITION_CHECKS);
            nullChecks             = IsEnabled(fileContent, __FILE__NULL_CHECKS);
            filePathChecks         = IsEnabled(fileContent, __FILE__FILE_PATH_CHECKS);
            arrayBoundsChecks      = IsEnabled(fileContent, __FILE__ARRAY_BOUNDS_CHECKS);
            comparisonChecks       = IsEnabled(fileContent, __FILE__COMPARISON_CHECKS);
            arithmeticLogicChecks  = IsEnabled(fileContent, __FILE__ARITHMETIC_LOGIC_CHECKS);
            memoryChecks           = IsEnabled(fileContent, __FILE__MEMORY_CHECKS);

            Update_allChecks();
        }

        private void OnGUI()
        {
            bool _allChecks = GUILayout.Toggle(allChecks, __NAME__ALL_CHECKS);

            GUILayout.Space(15);

            bool _booleanConditionChecks = GUILayout.Toggle(booleanConditionChecks, __NAME__BOOLEAN_CONDITION_CHECKS);
            bool _nullChecks             = GUILayout.Toggle(nullChecks,             __NAME__NULL_CHECKS);
            bool _filePathChecks         = GUILayout.Toggle(filePathChecks,         __NAME__FILE_PATH_CHECKS);
            bool _arrayBoundsChecks      = GUILayout.Toggle(arrayBoundsChecks,      __NAME__ARRAY_BOUNDS_CHECKS);
            bool _comparisonChecks       = GUILayout.Toggle(comparisonChecks,       __NAME__COMPARISON_CHECKS);
            bool _arithmeticLogicChecks  = GUILayout.Toggle(arithmeticLogicChecks,  __NAME__ARITHMETIC_LOGIC_CHECKS);
            bool _memoryChecks           = GUILayout.Toggle(memoryChecks,           __NAME__MEMORY_CHECKS);

            GUILayout.Space(15);

            bool _anythingChanged = _allChecks != allChecks;

            if (_anythingChanged)
            {
                SetAll(_allChecks);
            }

            if (!_anythingChanged)
            {
                _anythingChanged |= (_booleanConditionChecks != booleanConditionChecks);
                _anythingChanged |= (_nullChecks != nullChecks);
                _anythingChanged |= (_filePathChecks != filePathChecks);
                _anythingChanged |= (_arrayBoundsChecks != arrayBoundsChecks);
                _anythingChanged |= (_comparisonChecks != comparisonChecks);
                _anythingChanged |= (_arithmeticLogicChecks != arithmeticLogicChecks);
                _anythingChanged |= (_memoryChecks != memoryChecks);

                booleanConditionChecks = (_booleanConditionChecks != booleanConditionChecks) ? _booleanConditionChecks : booleanConditionChecks;
                nullChecks             = (_nullChecks != nullChecks)                         ? _nullChecks             : nullChecks;
                filePathChecks         = (_filePathChecks != filePathChecks)                 ? _filePathChecks         : filePathChecks;
                arrayBoundsChecks      = (_arrayBoundsChecks != arrayBoundsChecks)           ? _arrayBoundsChecks      : arrayBoundsChecks;
                comparisonChecks       = (_comparisonChecks != comparisonChecks)             ? _comparisonChecks       : comparisonChecks;
                arithmeticLogicChecks  = (_arithmeticLogicChecks != arithmeticLogicChecks)   ? _arithmeticLogicChecks  : arithmeticLogicChecks;
                memoryChecks           = (_memoryChecks != memoryChecks)                     ? _memoryChecks           : memoryChecks;
            }

            Update_allChecks();

            anythingChanged |= _anythingChanged;

            if (anythingChanged & GUILayout.Button("Apply", GUILayout.ExpandWidth(false)))
            {
                Apply();
            }
        }
    }
}
#endif