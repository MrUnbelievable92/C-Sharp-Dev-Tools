#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace DevTools.Unity.Editor
{
    internal class DebugBurstIntrinsicsWindow : EditorWindow
    {
        private static string ProjectPath => Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".AsDirectory().Length).AsDirectory() + "LocalPackages";

        private const string SSE    = "Sse.IsSseSupported";
        private const string SSE2   = "Sse2.IsSse2Supported";
        private const string SSE3   = "Sse3.IsSse3Supported";
        private const string SSSE3  = "Ssse3.IsSsse3Supported";
        private const string SSE4_1 = "Sse4_1.IsSse41Supported";
        private const string SSE4_2 = "Sse4_2.IsSse42Supported";
        private const string AVX    = "Avx.IsAvxSupported";
        private const string AVX2   = "Avx2.IsAvx2Supported";
        private const string FMA    = "Fma.IsFmaSupported";
        private const string BMI1   = "Bmi1.IsBmi1Supported";
        private const string BMI2   = "Bmi2.IsBmi2Supported";
        private const string POPCNT = "Popcnt.IsPopcntSupported";
        private const string F16C   = "F16C.IsF16CSupported";

        private const string CONST = "Constant.IsConstantExpression";

        private const string TESTING = "#define TESTING";


        private static bool SkipFile(string file)
        {
            return Path.GetExtension(file) != ".cs" || Path.GetFileNameWithoutExtension(file) == "DebugBurstIntrinsicsWindow";
        }

        private static void DeactivateAll()
        {
            DeactivateFeatureSet(SSE);
            DeactivateFeatureSet(SSE2);
            DeactivateFeatureSet(SSE3);
            DeactivateFeatureSet(SSSE3);
            DeactivateFeatureSet(SSE4_1);
            DeactivateFeatureSet(SSE4_2);
            DeactivateFeatureSet(AVX);
            DeactivateFeatureSet(AVX2);
            DeactivateFeatureSet(FMA);
            DeactivateFeatureSet(BMI1);
            DeactivateFeatureSet(BMI2);
            DeactivateFeatureSet(POPCNT);
            DeactivateFeatureSet(F16C);
        }

        private static void DeactivatePreprocessorDirective(string define)
        {
            List<Task> tasks = new List<Task>(256);

            DirectoryExtensions.ForEachFile(ProjectPath,
            (file) =>
            {
                if (SkipFile(file))
                {
                    return;
                }

                tasks.Add(Task.Factory.StartNew(
                () =>
                {
                    string fileContent = File.ReadAllText(file);

                    if (!fileContent.Contains("//" + define))
                    {
                        fileContent = fileContent.Replace(define, "//" + define);
                        File.WriteAllText(file, fileContent);
                    }
                }));
            });

            Task.WaitAll(tasks.ToArray());
        }

        private static void ActivatePreprocessorDirective(string define)
        {
            List<Task> tasks = new List<Task>(256);

            DirectoryExtensions.ForEachFile(ProjectPath,
            (file) =>
            {
                if (SkipFile(file))
                {
                    return;
                }
                
                tasks.Add(Task.Factory.StartNew(
                () =>
                {
                    string fileContent = File.ReadAllText(file);

                    if (fileContent.Contains("//" + define))
                    {
                        fileContent = fileContent.Replace("//" + define, define);
                        File.WriteAllText(file, fileContent);
                    }
                }));
            });
            
            Task.WaitAll(tasks.ToArray());
        }

        private static void DeactivateFeatureSet(string featureSet)
        {
            List<Task> tasks = new List<Task>(256);

            DirectoryExtensions.ForEachFile(ProjectPath,
            (file) =>
            {
                if (SkipFile(file))
                {
                    return;
                }
                
                tasks.Add(Task.Factory.StartNew(
                () =>
                {
                    string fileContent = File.ReadAllText(file);

                    bool any = false;
                    int index = 0;
                    while ((index = fileContent.IndexOf("!" + featureSet, index + 1)) != -1)
                    {
                        any = true;
                        fileContent = fileContent.Remove(index, 1);
                    }

                    if (any)
                    {
                        File.WriteAllText(file, fileContent);
                    }
                }));
            });
            
            Task.WaitAll(tasks.ToArray());
        }

        private static void ActivateFeatureSet(string featureSet)
        {
            List<Task> tasks = new List<Task>(256);

            DirectoryExtensions.ForEachFile(ProjectPath,
            (file) =>
            {
                if (SkipFile(file))
                {
                    return;
                }
                
                tasks.Add(Task.Factory.StartNew(
                () =>
                {
                    string fileContent = File.ReadAllText(file);

                    bool any = false;
                    int index = 0;
                    while ((index = fileContent.IndexOf(featureSet, index + 1)) != -1)
                    {
                        any = true;
                        if (fileContent[index - 1] != '!')
                        {
                            fileContent = fileContent.Insert(index, "!");
                            index++;
                        }
                    }

                    if (any)
                    {
                        File.WriteAllText(file, fileContent);
                    }
                }));
            });
            
            Task.WaitAll(tasks.ToArray());
        }


        public static void ACTIVATE_TEST_MODE()
        {
            ActivatePreprocessorDirective(TESTING);

            AssetDatabase.Refresh();
        }

        public static void ACTIVATE_SSE()
        {
            DeactivateAll();

            ActivateFeatureSet(SSE);

            AssetDatabase.Refresh();
        }

        public static void ACTIVATE_SSE2()
        {
            DeactivateAll();

            ActivateFeatureSet(SSE);
            ActivateFeatureSet(SSE2);

            AssetDatabase.Refresh();
        }

        public static void ACTIVATE_SSE3()
        {
            DeactivateAll();

            ActivateFeatureSet(SSE);
            ActivateFeatureSet(SSE2);
            ActivateFeatureSet(SSE3);

            AssetDatabase.Refresh();
        }

        public static void ACTIVATE_SSSE3()
        {
            DeactivateAll();

            ActivateFeatureSet(SSE);
            ActivateFeatureSet(SSE2);
            ActivateFeatureSet(SSE3);
            ActivateFeatureSet(SSSE3);

            AssetDatabase.Refresh();
        }

        public static void ACTIVATE_SSE4_1()
        {
            DeactivateAll();

            ActivateFeatureSet(SSE);
            ActivateFeatureSet(SSE2);
            ActivateFeatureSet(SSE3);
            ActivateFeatureSet(SSSE3);
            ActivateFeatureSet(SSE4_1);

            AssetDatabase.Refresh();
        }

        public static void ACTIVATE_SSE4_2()
        {
            DeactivateAll();

            ActivateFeatureSet(SSE);
            ActivateFeatureSet(SSE2);
            ActivateFeatureSet(SSE3);
            ActivateFeatureSet(SSSE3);
            ActivateFeatureSet(SSE4_1);
            ActivateFeatureSet(SSE4_2);

            AssetDatabase.Refresh();
        }

        public static void ACTIVATE_AVX()
        {
            DeactivateAll();

            ActivateFeatureSet(SSE);
            ActivateFeatureSet(SSE2);
            ActivateFeatureSet(SSE3);
            ActivateFeatureSet(SSSE3);
            ActivateFeatureSet(SSE4_1);
            ActivateFeatureSet(SSE4_2);
            ActivateFeatureSet(AVX);

            AssetDatabase.Refresh();
        }

        public static void ACTIVATE_AVX2()
        {
            DeactivateAll();

            ActivateFeatureSet(SSE);
            ActivateFeatureSet(SSE2);
            ActivateFeatureSet(SSE3);
            ActivateFeatureSet(SSSE3);
            ActivateFeatureSet(SSE4_1);
            ActivateFeatureSet(SSE4_2);
            ActivateFeatureSet(AVX);
            ActivateFeatureSet(AVX2);
            ActivateFeatureSet(FMA);
            ActivateFeatureSet(BMI1);
            ActivateFeatureSet(BMI2);
            ActivateFeatureSet(POPCNT);
            ActivateFeatureSet(F16C);

            AssetDatabase.Refresh();
        }

        public static void ACTIVATE_CONST()
        {
            DeactivateFeatureSet(CONST);

            ActivateFeatureSet(CONST);

            AssetDatabase.Refresh();
        }

        public static void RELEASE_MODE()
        {
            DeactivatePreprocessorDirective(TESTING);

            DeactivateAll();

            DeactivateFeatureSet(CONST);

            AssetDatabase.Refresh();
        }

        private static void DrawLine()
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(1));
            r.height = 1;
            r.x -= 2 ;
            r.width += 6;
            EditorGUI.DrawRect(r, Color.black);
        }


        [MenuItem("Window/C# Dev Tools/Debug Burst Intrinsics (Local Packages Only)")]
        private static void ShowWindow()
        {
            GetWindow<DebugBurstIntrinsicsWindow>("Debug Burst Intrinsics");
        }

        private void OnGUI()
        {
            GUILayout.Label("Set Code Path:");

            if (GUILayout.Button("SSE", GUILayout.ExpandWidth(false)))
            {
                ACTIVATE_SSE();
            }
            if (GUILayout.Button("SSE2", GUILayout.ExpandWidth(false)))
            {
                ACTIVATE_SSE2();
            }
            if (GUILayout.Button("SSE3", GUILayout.ExpandWidth(false)))
            {
                ACTIVATE_SSE3();
            }
            if (GUILayout.Button("SSSE3", GUILayout.ExpandWidth(false)))
            {
                ACTIVATE_SSSE3();
            }
            if (GUILayout.Button("SSE4.1", GUILayout.ExpandWidth(false)))
            {
                ACTIVATE_SSE4_1();
            }
            if (GUILayout.Button("SSE4.2", GUILayout.ExpandWidth(false)))
            {
                ACTIVATE_SSE4_2();
            }
            if (GUILayout.Button("AVX", GUILayout.ExpandWidth(false)))
            {
                ACTIVATE_AVX();
            }
            if (GUILayout.Button("AVX2", GUILayout.ExpandWidth(false)))
            {
                ACTIVATE_AVX2();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("IsConstantExpression", GUILayout.ExpandWidth(false)))
            {
                ACTIVATE_CONST();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Activate 'TESTING' Preprocessor Directive", GUILayout.ExpandWidth(false)))
            {
                ACTIVATE_TEST_MODE();
            }

            GUILayout.Space(5);

            DrawLine();

            GUILayout.Space(5);

            if (GUILayout.Button("Reset to Release Mode", GUILayout.ExpandWidth(false)))
            {
                RELEASE_MODE();
            }
        }
    }
}
#endif