/// <summary>
/// Project : Easy Build System
/// Class : EditorGUIUtilityExtension.cs
/// Namespace : EasyBuildSystem.Features.Editor.Extensions
/// Copyright : © 2015 - 2022 by PolarInteractive
/// </summary>

using UnityEngine;

using UnityEditor;

using System.Collections.Generic;

namespace EasyBuildSystem.Features.Editor.Extensions
{
    public class EditorGUIUtilityExtension
    {
        public static void DrawHeader(string title, string description, bool drawButtons = false)
        {
            if (drawButtons)
            {
                EditorGUILayout.Separator();

                GUIContent label1 = new GUIContent(" Documentation", EditorGUIUtility.IconContent("UnityEditor.ConsoleWindow").image);
                GUIContent label2 = new GUIContent(" Contact Support", EditorGUIUtility.IconContent("_Help").image);
                GUIContent label3 = new GUIContent(" Write Review", EditorGUIUtility.IconContent("d_Favorite").image);
                int index = Toolbar(-1, new GUIContent[3] { label1, label2, label3 }, 22);

                if (index == 0)
                {
                    Application.OpenURL("https://polarinteractive.gitbook.io/easy-build-system");
                }
                else if (index == 1)
                {
                    Application.OpenURL("https://form.jotform.com/202960719544359");
                }
                else if (index == 2)
                {
                    Application.OpenURL("https://assetstore.unity.com/packages/templates/systems/easy-build-system-modular-building-system-45394");
                }

                EditorGUILayout.Separator();
            }
            else
            {
                EditorGUILayout.Separator();
            }

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (title != string.Empty)
            {
                if (EditorGUIUtility.isProSkin)
                {
                    EditorGUILayout.LabelField(title, EditorStyles.whiteLargeLabel, GUILayout.Width(350), GUILayout.Height(20));
                }
                else
                {
                    EditorGUILayout.LabelField(title, EditorStyles.largeLabel, GUILayout.Width(300), GUILayout.Height(20));
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (description != string.Empty)
            {
                GUI.enabled = false;
                foreach (string line in description.Split('\n'))
                {
                    GUILayout.Label(line, EditorStyles.wordWrappedMiniLabel);
                }
                GUI.enabled = true;
                EditorGUILayout.Separator();
            }
            else
            {
                EditorGUILayout.Separator();
            }

            Rect lineRect = EditorGUILayout.GetControlRect(GUILayout.Height(1));
            lineRect.x += 3;
            lineRect.width -= 3;
            lineRect.height = 1;

            EditorGUI.DrawRect(lineRect, Color.white / 2.5f);

            EditorGUILayout.Separator();

            GUILayout.EndVertical();
        }

        public static void LinkLabel(string caption, string url)
        {
            GUIStyle style = GUI.skin.label;
            style.richText = true;
            caption = string.Format("<color=#3386FF>{0}</color>", caption);

            bool bClicked = GUILayout.Button(caption, style);

            Rect rect = GUILayoutUtility.GetLastRect();
            rect.width = style.CalcSize(new GUIContent(caption)).x;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            if (bClicked)
            {
                Application.OpenURL(url);
            }
        }

        public static LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);

                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }

            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());

            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }

            layerMask.value = mask;

            return layerMask;
        }

        public static int Toolbar(int index, GUIContent[] tabs, float height = 25f)
        {
            BeginHorizontal();

            for (int i = 0; i < tabs.Length; i++)
            {
                GUILayout.FlexibleSpace();

                GUIStyle style = new GUIStyle(EditorStyles.label);

                if (index == -1)
                {
                    style.hover.textColor = Color.white;
                    style.normal.textColor = Color.white / 1.5f;
                }
                else
                {
                    GUI.color = index == i ? Color.white / 1.1f : Color.white / 1.5f;
                    style.fontStyle = index == i ? FontStyle.Bold : FontStyle.Normal;
                }

                style.hover.textColor = Color.white;

                style.alignment = TextAnchor.MiddleCenter;

                style.richText = true;

                GUILayout.BeginHorizontal(GUILayout.Width(200));

                if (GUILayout.Button(tabs[i], style, GUILayout.ExpandWidth(true), GUILayout.Height(height)))
                {
                    index = i;
                }

                GUILayout.EndHorizontal();

                GUI.color = Color.white;

                GUILayout.FlexibleSpace();

                if (i != tabs.Length - 1)
                {
                    Rect rect = EditorGUILayout.GetControlRect(false, 1f, GUILayout.Width(1f));

                    rect.width = 1;
                    rect.height = height;
                    EditorGUI.DrawRect(rect, Color.black / 4f);
                }
            }

            EndHorizontal();

            return index;
        }

        public static void HelpBox(string message, MessageType type)
        {
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal();

            string hexColor = "#dfdfdf";

            if (type == MessageType.Warning)
            {
                GUILayout.Label(EditorGUIUtility.IconContent("console.warnicon"), GUILayout.Width(32), GUILayout.Height(32));
                hexColor = "#FCBF07";
            }
            else if (type == MessageType.Info)
            {
                GUILayout.Label(EditorGUIUtility.IconContent("console.infoicon"), GUILayout.Width(32), GUILayout.Height(32));
            }
            else if (type == MessageType.Error)
            {
                GUILayout.Label(EditorGUIUtility.IconContent("console.erroricon"), GUILayout.Width(32), GUILayout.Height(32));
                hexColor = "#FF6E40";
            }

            GUILayout.BeginVertical();

            GUILayout.Space(message.Split('\n').Length > 1 ? 5f : 11f);

            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.fontSize = 10;
            style.alignment = TextAnchor.MiddleLeft;
            style.richText = true;

            foreach (string line in message.Split('\n'))
            {
                GUILayout.Label("<color=" + hexColor + ">" + line + "</color>", style);
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            GUI.color = Color.white;
        }

        public static bool BeginFoldout(GUIContent content, bool state, bool indent = true, float indentSpacing = 16f)
        {
            BeginVertical();

            GUILayout.BeginHorizontal(GUILayout.Width(300));
            GUILayout.Space(15);
            state = EditorGUILayout.Foldout(state, content, true, EditorStyles.foldoutHeader);
            GUILayout.EndHorizontal();

            GUILayout.Space(1);

            if (indent)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Space(indentSpacing);
            }

            GUILayout.BeginVertical();

            return state;
        }

        public static void EndFoldout(bool indent = true)
        {
            GUILayout.EndVertical();

            if (indent)
            {
                GUILayout.EndHorizontal();
            }
            
            EndVertical();
        }

        public static void BeginVertical(bool border = true, params GUILayoutOption[] layout)
        {
            int borderSize = 2;
            GUIStyle style = new GUIStyle
            {
                border = new RectOffset(borderSize, borderSize, borderSize, borderSize)
            };
            style.normal.background = Resources.Load<Texture2D>("UI/Editor/border");

            if (border)
            {
                GUI.color = Color.black / 3f;
                GUILayout.BeginVertical(style, layout);
            }
            else
            {
                GUILayout.BeginVertical(layout);
            }
            GUI.color = Color.white;
#if UNITY_2021_1_OR_NEWER
            GUILayout.Space(5f);
#else
            GUILayout.Space(3f);
#endif
            GUILayout.BeginHorizontal();
            GUILayout.Space(5f);

            GUILayout.BeginVertical();
        }

        public static void EndVertical()
        {
            GUILayout.EndVertical();
            GUILayout.Space(5f);
            GUILayout.EndHorizontal();

#if UNITY_2021_1_OR_NEWER
            GUILayout.Space(5f);
#else
            GUILayout.Space(3f);
#endif
            GUILayout.EndVertical();

            GUILayout.Space(3f);
        }

        public static void BeginHorizontal(params GUILayoutOption[] layout)
        {
            int borderSize = 2;
            GUIStyle style = new GUIStyle
            {
                border = new RectOffset(borderSize, borderSize, borderSize, borderSize)
            };
            style.normal.background = Resources.Load<Texture2D>("UI/Editor/border");

            GUI.color = Color.black / 3f;
            GUILayout.BeginHorizontal(style, layout);
            GUI.color = Color.white;
#if UNITY_2021_1_OR_NEWER
            GUILayout.Space(5f);
#else
            GUILayout.Space(3f);
#endif
            GUILayout.BeginHorizontal();
            GUILayout.Space(5f);

            GUILayout.BeginHorizontal();
        }

        public static void EndHorizontal()
        {
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);
            GUILayout.EndHorizontal();

#if UNITY_2021_1_OR_NEWER
            GUILayout.Space(5f);
#else
            GUILayout.Space(3f);
#endif
            GUILayout.EndHorizontal();

            GUILayout.Space(3f);
        }
    }
}