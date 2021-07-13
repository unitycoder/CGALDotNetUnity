﻿using System;
using UnityEditor;
using UnityEngine;

using Common.Core.Numerics;

// IngredientDrawer
[CustomPropertyDrawer(typeof(Vector4f))]
public class Vector4fDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);


        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        const int xWidth = 100;
        const int yWidth = 100;
        const int zWidth = 100;
        const int wWidth = 100;
        const int offset = 5;

        // Calculate rects
        var xRect = new Rect(position.x, position.y, xWidth, position.height);
        var yRect = new Rect(position.x + xWidth + offset, position.y, yWidth, position.height);
        var zRect = new Rect(position.x + xWidth + yWidth + offset * 2, position.y, zWidth, position.height);
        var wRect = new Rect(position.x + xWidth + yWidth + zWidth + offset * 3, position.y, wWidth, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(xRect, property.FindPropertyRelative("x"), GUIContent.none);
        EditorGUI.PropertyField(yRect, property.FindPropertyRelative("y"), GUIContent.none);
        EditorGUI.PropertyField(zRect, property.FindPropertyRelative("z"), GUIContent.none);
        EditorGUI.PropertyField(wRect, property.FindPropertyRelative("w"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}

