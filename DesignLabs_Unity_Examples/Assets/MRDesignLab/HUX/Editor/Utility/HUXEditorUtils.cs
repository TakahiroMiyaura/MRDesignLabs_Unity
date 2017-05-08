﻿//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class HUXEditorUtils
{
    public readonly static Color DefaultColor = new Color(1f, 1f, 1f);
    public readonly static Color DisabledColor = new Color(0.6f, 0.6f, 0.6f);
    public readonly static Color HelpBoxColor = new Color(0.8f, 0.8f, 0.8f);
    public readonly static Color WarningColor = new Color(1f, 0.85f, 0.6f);
    public readonly static Color ErrorColor = new Color(1f, 0.55f, 0.5f);
    public readonly static Color SuccessColor = new Color(0.8f, 1f, 0.75f);

    /// <summary>
    /// Draws a field for scriptable object profiles
    /// Includes a button for creating new profiles
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="profile"></param>
    /// <returns></returns>
    public static T DrawProfileField<T>(T profile) where T : UnityEngine.ScriptableObject
    {
        Color prevColor = GUI.color;
        GUI.color = Color.Lerp(Color.white, Color.gray, 0.5f);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.color = Color.Lerp(Color.white, Color.gray, 0.25f);
        EditorGUILayout.LabelField("Select a " + typeof(T).Name + " or create a new profile", EditorStyles.miniBoldLabel);
        T newProfile = profile;
        EditorGUILayout.BeginHorizontal();
        newProfile = (T)EditorGUILayout.ObjectField(profile, typeof(T), false);
        if (GUILayout.Button("Create new profile"))
        {
            newProfile = CreateProfile<T>();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        if (profile == null)
        {
            ErrorMessage("You must choose a button profile.", null);
        }

        GUI.color = prevColor;
        return newProfile;
    }

    public static T CreateProfile<T>() where T : UnityEngine.ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, "Assets/New" + typeof(T).Name + ".asset");
        AssetDatabase.SaveAssets();
        return asset;
    }

    public static void DrawFilterTagField(SerializedObject serializedObject, string propertyName)
    {
        SerializedProperty p = serializedObject.FindProperty(propertyName);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(p);
        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }

    public static T DropDownComponentField<T>(string label, T obj, Transform transform) where T : UnityEngine.Component
    {
        T[] optionObjects = transform.GetComponentsInChildren<T>(true);
        int selectedIndex = 0;
        string[] options = new string[optionObjects.Length + 1];
        options[0] = "(None)";
        for (int i = 0; i < optionObjects.Length; i++)
        {
            options[i + 1] = optionObjects[i].name;
            if (obj == optionObjects[i])
            {
                selectedIndex = i + 1;
            }
        }

        EditorGUILayout.BeginHorizontal();
        int newIndex = EditorGUILayout.Popup(label, selectedIndex, options);
        if (newIndex == 0)
        {
            // Zero means '(None)'
            obj = null;
        }
        else
        {
            obj = optionObjects[newIndex - 1];
        }

        //draw the object field so people can click it
        obj = (T)EditorGUILayout.ObjectField(obj, typeof(T), true);
        EditorGUILayout.EndHorizontal();

        return obj;
    }

    /// <summary>
    /// Draws enum values as a set of toggle fields
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="label"></param>
    /// <param name="enumObj"></param>
    /// <returns></returns>
    public static int EnumCheckboxField<T>(string label, T enumObj) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("T must be an enum.");
        }
        return EnumCheckboxField<T>(label, enumObj, string.Empty, (T)Activator.CreateInstance(typeof(T)));
    }

    /// <summary>
    /// Draws enum values as a set of toggle fields
    /// Also draws a button the user can click to set to a 'default' value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="label"></param>
    /// <param name="enumObj"></param>
    /// <param name="defaultName"></param>
    /// <param name="defaultVal"></param>
    /// <returns></returns>
    public static int EnumCheckboxField<T>(string label, T enumObj, string defaultName, T defaultVal) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("T must be an enum.");
        }

        // Convert enum value to an int64 so we can treat it as a flag set
        int enumFlags = Convert.ToInt32(enumObj);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(label, EditorStyles.miniLabel);
        DrawDivider();
        foreach (T enumVal in Enum.GetValues(typeof(T)))
        {
            int flagVal = Convert.ToInt32(enumVal);
            bool selected = (flagVal & enumFlags) != 0;
            selected = EditorGUILayout.Toggle(enumVal.ToString(), selected);
            // If it's selected add it to the enumObj, otherwise remove it
            if (selected)
            {
                enumFlags |= flagVal;
            }
            else
            {
                enumFlags &= ~flagVal;
            }
        }
        if (!string.IsNullOrEmpty(defaultName))
        {
            if (GUILayout.Button(defaultName, EditorStyles.miniButton))
            {
                enumFlags = Convert.ToInt32(defaultVal);
            }
        }
        EditorGUILayout.EndVertical();

        return enumFlags;
    }

    public static int EnumCheckboxField<T>(string label, T enumObj, string defaultName, T defaultVal, T valOnZero) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("T must be an enum.");
        }

        // Convert enum value to an int64 so we can treat it as a flag set
        int enumFlags = Convert.ToInt32(enumObj);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(label, EditorStyles.miniLabel);
        DrawDivider();
        foreach (T enumVal in Enum.GetValues(typeof(T)))
        {
            int flagVal = Convert.ToInt32(enumVal);
            if (flagVal == 0)
            {
                continue;
            }
            bool selected = (flagVal & enumFlags) != 0;
            selected = EditorGUILayout.Toggle(enumVal.ToString(), selected);
            // If it's selected add it to the enumObj, otherwise remove it
            if (selected)
            {
                enumFlags |= flagVal;
            }
            else
            {
                enumFlags &= ~flagVal;
            }
        }
        if (!string.IsNullOrEmpty(defaultName))
        {
            if (GUILayout.Button(defaultName, EditorStyles.miniButton))
            {
                enumFlags = Convert.ToInt32(defaultVal);
            }
        }
        EditorGUILayout.EndVertical();

        if (enumFlags == 0)
        {
            enumFlags = Convert.ToInt32(valOnZero);
        }
        return enumFlags;
    }

    public static string MaterialPropertyName (string property, Material mat, ShaderUtil.ShaderPropertyType type)
    {
        // Create a list of available color and value properties
        List<string> props = new List<string>();

        int selectedPropIndex = 0;

        props.Add("(None)");

        if (mat != null)
        {
            int propertyCount = ShaderUtil.GetPropertyCount(mat.shader);
            string propName = string.Empty;
            for (int i = 0; i < propertyCount; i++)
            {
                if (ShaderUtil.GetPropertyType(mat.shader, i) == type)
                {
                    propName = ShaderUtil.GetPropertyName(mat.shader, i);
                    if (propName == property)
                    {
                        // We've found our current property
                        selectedPropIndex = props.Count;
                    }
                    props.Add(propName);
                }
            }

            GUI.color = string.IsNullOrEmpty(property) ? HUXEditorUtils.DisabledColor : HUXEditorUtils.DefaultColor;
            int newPropIndex = EditorGUILayout.Popup(type.ToString(), selectedPropIndex, props.ToArray());
            property = (newPropIndex > 0 ? props[newPropIndex] : string.Empty);
            return property;
        } else
        {
            WarningMessage("Can't get material " + type.ToString() + " properties because material is null.");
            return string.Empty;
        }
    }

    public static void WarningMessage(string warning, string buttonMessage = null, Action buttonAction = null)
    {
        Color tColor = GUI.color;
        HUXEditorUtils.BeginSectionBox("Warning", HUXEditorUtils.WarningColor);
        EditorGUILayout.LabelField(warning, EditorStyles.wordWrappedLabel);
        if (!string.IsNullOrEmpty (buttonMessage) && buttonAction != null)
        {
            if (GUILayout.Button (buttonMessage))
            {
                buttonAction.Invoke();
            }
        }
        HUXEditorUtils.EndSectionBox();
        GUI.color = tColor;
    }

    public static void ErrorMessage (string error, Action action)
    {
        Color tColor = GUI.color;
        HUXEditorUtils.BeginSectionBox("Error", HUXEditorUtils.ErrorColor);
        EditorGUILayout.LabelField(error, EditorStyles.wordWrappedLabel);
        if (action != null && GUILayout.Button("Fix now"))
        {
            action.Invoke();
        }
        HUXEditorUtils.EndSectionBox();
        GUI.color = tColor;
    }

    public static void BeginSectionBox(string label)
    {
        GUI.color = DefaultColor;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSubtleMiniLabel(label + ":");
    }

    public static void BeginSectionBox (string label, Color color)
    {
        GUI.color = color;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        DrawSubtleMiniLabel(label + ":");
    }

    public static void EndSectionBox()
    {
        EditorGUILayout.EndVertical();
    }

    public static void BeginSubSectionBox(string label)
    {
        GUI.color = DefaultColor;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(label + ":", EditorStyles.boldLabel);
    }

    public static void EndSubSectionBox()
    {
        EditorGUILayout.EndVertical();
    }

    public static void DrawSubtleMiniLabel (string label)
    {
        Color tColor = GUI.color;
        GUI.color = Color.Lerp (tColor, Color.grey, 0.5f);
        EditorGUILayout.LabelField(label, EditorStyles.miniLabel);
        GUI.color = tColor;
    }

    public static void DrawDivider()
    {
        GUIStyle styleHR = new GUIStyle(GUI.skin.box);
        styleHR.stretchWidth = true;
        styleHR.fixedHeight = 2;
        GUILayout.Box("", styleHR);
    }

    public static void SaveChanges(UnityEngine.Object target)
    {
        if (Application.isPlaying)
            return;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }

    public static void SaveChanges(UnityEngine.Object target1, UnityEngine.Object target2)
    {
        if (Application.isPlaying)
            return;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target1);
            EditorUtility.SetDirty(target2);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }

    public static string[] getMethodOptions(GameObject comp, List<System.Type> ignoreTypes = null)
    {
        List<string> methods = new List<string>();

        if (comp != null)
        {
            Component[] allComponents = comp.GetComponents<Component>();
            List<System.Type> doneTypes = new List<System.Type>();

            for (int index = 0; index < allComponents.Length; index++)
            {
                System.Type compType = allComponents[index].GetType();
                if (!doneTypes.Contains(compType) && (ignoreTypes == null || !ignoreTypes.Contains(compType)))
                {
                    MethodInfo[] allMemebers = compType.GetMethods();
                    for (int memberIndex = 0; memberIndex < allMemebers.Length; memberIndex++)
                    {
                        if (allMemebers[memberIndex].IsPublic
                            && allMemebers[memberIndex].GetParameters().Length == 0
                            && !methods.Contains(allMemebers[memberIndex].Name)
                            && allMemebers[memberIndex].ReturnType == typeof(void))
                        {
                            methods.Add(allMemebers[memberIndex].Name);
                        }
                    }

                    doneTypes.Add(compType);
                }
            }
        }

        return methods.ToArray();
    }

    /// <summary>
    /// Adds a prefab to the scene.
    /// </summary>
    /// <param name="prefabPath"></param>
    /// <param name="ignoreAlreadyInScene">If false the prefab will not be added if it exists in the hierarchy.</param>
    /// <returns>A refernce to the newly created prefab instance or one that exists in the scene if ignoreAlreadyInScene is false.</returns>
    public static GameObject AddToScene(string prefabPath, bool ignoreAlreadyInScene = true)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
        GameObject instance = null;
        if (prefab != null)
        {
            instance = FindFirstPrefabInstance(prefab);

            if (instance == null || ignoreAlreadyInScene)
            {
                instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            }
            else
            {
                Debug.LogWarning("Instance already exits in the scene: " + prefabPath);
            }
        }
        else
        {
            Debug.LogError("Could not load prefab: " + prefabPath);
        }

        return instance;
    }

    /// <summary>
    /// Finds the first instance of a preface in the Hierarchy.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns>First instance of the prefab or null if one is not found.</returns>
    public static GameObject FindFirstPrefabInstance(GameObject prefab)
    {
        GameObject result = null;
        GameObject[] allObjects = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject obj in allObjects)
        {
            PrefabType type = PrefabUtility.GetPrefabType(obj);
            if (type == PrefabType.PrefabInstance)
            {
                UnityEngine.Object GO_prefab = PrefabUtility.GetPrefabParent(obj);
                if (prefab == GO_prefab)
                {
                    result = obj;
                    break;
                }
            }
        }
        return result;
    }

    public static void CorrectAmbientLightingInScene()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientIntensity = 1.0f;

        // Normalize and set ambient light to default.
        Vector4 c = new Vector4(51.0f, 51.0f, 51.0f, 255.0f);
        c.Normalize();
        RenderSettings.ambientLight = new Color(c.x, c.y, c.z, c.w);


        RenderSettings.reflectionBounces = 1;
        RenderSettings.reflectionIntensity = 1.0f;

        RenderSettings.skybox = null;
        RenderSettings.fog = false;
    }

}