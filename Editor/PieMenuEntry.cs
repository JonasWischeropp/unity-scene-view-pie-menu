using System;
using UnityEditor;
using UnityEngine;

namespace JonasWischeropp.Unity.EditorTools.SceneView {
    public class PieMenuEntry {
        public readonly string text;
        public readonly Texture icon;
        public readonly Action action;
        
        public PieMenuEntry(string text, Texture icon, Action action) {
            this.text = text;
            this.icon = icon;
            this.action = action;
        }

        public PieMenuEntry(string text, string iconName, Action action)
            : this(text, EditorGUIUtility.IconContent(iconName).image, action) {}
    }
}
