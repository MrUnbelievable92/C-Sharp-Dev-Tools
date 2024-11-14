#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace DevTools.Unity.Editor
{
    internal class SafetyCheckSettingsWindow : EditorWindow
    {
        private const string FOLDER_NAME = "C Sharp Dev Tools";
        private const string FILE_NAME = "Assert.cs";
        private const string ALL_CHECKS = "All Checks";
        private const string LOADING = "???";
        private const string APPLY = "Apply";

        private bool firstLoad;
        private bool anythingChanged;

        private Dictionary<Assert.GroupAttribute, uint> methodCallCounts = null;
        private Dictionary<Assert.GroupAttribute, bool> definesToCheckMarks;


        private static string ProjectPath => Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".AsDirectory().Length);
        private static string ScriptPath
        {
            get
            {
                string defaultPath = ProjectPath.AsDirectory() + "LocalPackages".AsDirectory() + FOLDER_NAME.AsDirectory() + FILE_NAME;

                if (File.Exists(defaultPath))
                {
                    return defaultPath;
                }
                else
                {
                    return DirectoryExtensions.FindInFolder(ProjectPath, FILE_NAME) ?? throw new DllNotFoundException($"'{ FOLDER_NAME.AsDirectory() + FILE_NAME }' was modified and cannot be found. Enabling/Disabling assertions in the editor is not supported.");
                }
            }
        }

        private ulong TotalMethodCalls
        {
            get
            {
                ulong count = 0;

                foreach (KeyValuePair<Assert.GroupAttribute, uint> assertion in methodCallCounts)
                {
                    count += assertion.Value;
                }

                return count;
            }
        }

        private bool AllChecks
        {
            get
            {
                bool result = true;

                foreach (KeyValuePair<Assert.GroupAttribute, bool> checkMark in definesToCheckMarks.ToList())
                {
                    result &= definesToCheckMarks[checkMark.Key];
                }

                return result;
            }

            set
            {
                foreach (KeyValuePair<Assert.GroupAttribute, bool> checkMark in definesToCheckMarks.ToList())
                {
                    definesToCheckMarks[checkMark.Key] = value;
                }
            }
        }


        private string UpdateCondition(string fileContent, Assert.GroupAttribute assertionGroup)
        {
            bool conditionValueInEditor = definesToCheckMarks[assertionGroup];

            if (conditionValueInEditor && !IsEnabled(fileContent, assertionGroup))
            {
                fileContent = Enable(fileContent, assertionGroup);
            }
            else if (!conditionValueInEditor && IsEnabled(fileContent, assertionGroup))
            {
                fileContent = Disable(fileContent, assertionGroup);
            }

            return fileContent;
        }

        private void WriteToFile()
        {
            StreamReader reader = new StreamReader(ScriptPath);
            string fileContent = reader.ReadToEnd();

            foreach (KeyValuePair<Assert.GroupAttribute, bool> map in definesToCheckMarks)
            {
                fileContent = UpdateCondition(fileContent, map.Key);
            }

            anythingChanged = false;

            reader.Dispose();
            File.WriteAllText(ScriptPath, fileContent);
            AssetDatabase.Refresh();
        }

        private bool IsEnabled(string fileContent, Assert.GroupAttribute assertionGroup)
        {
            return fileContent.Substring(fileContent.IndexOf("#define " + assertionGroup.FileContent) - 2, 2) != "//";
        }

        private string Enable(string fileContent, Assert.GroupAttribute assertionGroup)
        {
Assert.IsFalse(IsEnabled(fileContent, assertionGroup));

            return fileContent.Remove(fileContent.IndexOf("#define " + assertionGroup.FileContent) - 2, 2);
        }

        private string Disable(string fileContent, Assert.GroupAttribute assertionGroup)
        {
Assert.IsTrue(IsEnabled(fileContent, assertionGroup));

            return fileContent.Insert(fileContent.IndexOf("#define " + assertionGroup.FileContent), "//");
        }

        private string GetAssertionGroupCallCountSuffix(Assert.GroupAttribute key = null)
        {
            bool loading = methodCallCounts == null;
            ulong calls = key == null ? (loading ? 0 : TotalMethodCalls) : (loading ? 0 : methodCallCounts[key]);

            return " (" +
                   (loading ? LOADING : calls.ToString()) +
                   " call" + (calls == 1 ? string.Empty : "s") +
                   ")";
        }


        [MenuItem("Window/C# Dev Tools/Manage Safety Checks...")]
        private static void ShowWindow()
        {
            GetWindow<SafetyCheckSettingsWindow>("C# Dev Tools Safety Checks");
        }

        private void OnEnable()
        {
            string projectPath = ProjectPath; // <- Only allowed on the main thread in Unity
            string scriptPath = ScriptPath; // <- Only allowed on the main thread in Unity

            Task.Run(() =>
            {
                try
                {
                    StreamReader reader = new StreamReader(scriptPath);
                    string fileContent = reader.ReadToEnd();
                    reader.Dispose();

                    Assert.GroupAttribute[] defines = Assert.GroupAttribute.Defines;
                    definesToCheckMarks = new Dictionary<Assert.GroupAttribute, bool>(defines.Length);
                    for (int i = 0; i < defines.Length; i++)
                    {
                        definesToCheckMarks.Add(defines[i], IsEnabled(fileContent, defines[i]));
                    }

                    firstLoad = true;

                    Task.Run(async () => methodCallCounts = await Assert.GroupAttribute.CountMethodCallsAsync(projectPath));
                }
                catch (Exception ex)
                {
                    ex.Log();
                    firstLoad = false;
                }
            });
        }

        unsafe private void OnGUI()
        {
            if (!firstLoad) return;


            bool _allChecks = GUILayout.Toggle(AllChecks, "  " + ALL_CHECKS + GetAssertionGroupCallCountSuffix(null));

            GUILayout.Space(10);

            const byte OFFSET_BETWEEN_CHECKBOXES = 2;
            bool* conditions = stackalloc bool[definesToCheckMarks.Count];
            int iterations = 0;
            foreach (KeyValuePair<Assert.GroupAttribute, bool> item in definesToCheckMarks.ToList())
            {
                conditions[iterations++] = GUILayout.Toggle(item.Value, "  " + item.Key.PublicName + GetAssertionGroupCallCountSuffix(item.Key));
                GUILayout.Space(OFFSET_BETWEEN_CHECKBOXES);
            }

            GUILayout.Space(15 - OFFSET_BETWEEN_CHECKBOXES);

            bool _anythingChanged = _allChecks != AllChecks;

            if (_anythingChanged)
            {
                AllChecks = _allChecks;
            }
            else
            {
                iterations = 0;

                foreach (KeyValuePair<Assert.GroupAttribute, bool> item in definesToCheckMarks.ToList())
                {
                    _anythingChanged |= (conditions[iterations] != item.Value);
                    definesToCheckMarks[item.Key] = (conditions[iterations] != item.Value) ? conditions[iterations] : item.Value;
                    iterations++;
                }
            }

            bool pressedApplyButton = GUILayout.Button(APPLY, GUILayout.ExpandWidth(false));
            anythingChanged |= _anythingChanged;
            if (anythingChanged & pressedApplyButton)
            {
                WriteToFile();
            }

            GUILayout.Space(10);
        }
    }
}
#endif