using System;
using UnityEditor;
using UnityEngine;

namespace JonasWischeropp.Unity.EditorTools.SceneView {
    public class PieMenuEntry {
        public readonly string text;
        public readonly Texture icon;
        public readonly Action action;
        public readonly Func<bool> isActive;

        public PieMenuEntry(string text, Texture icon, Action action, Func<bool> isActive = null) {
            this.text = text;
            this.icon = icon;
            this.action = action;
            this.isActive = isActive == null ? () => false : isActive;
        }

        public PieMenuEntry(string text, string iconName, Action action, Func<bool> isActive = null)
            : this(text, EditorGUIUtility.IconContent(iconName).image, action, isActive) { }
    }
}
