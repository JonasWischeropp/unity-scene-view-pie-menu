using System;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace JonasWischeropp.Unity.EditorTools.SceneView {
    public class PieMenu {
        // const string IMAGES_PATH = "Assets/PieMenu/Editor/Images/";
        const string IMAGES_PATH = "Packages/wischeropp.jonas.scene-view-pie-menu/Editor/Images/";
        static Texture2D CIRCLE_TEXTURE;
        static Texture2D CIRCLE_DIRECTION_TEXTURE;

        const float CIRCLE_RADIUS = 15f;
        const float ENTRY_RADIUS = 80f;
        
        int _count => _entries.Length;
        float[] _angles;
        Vector2[] _positions;
        PieMenuEntry[] _entries;

        Vector2 _center;
        bool _show = false;
        int _selected = -1;
        float _angle = 0f;

        static PieMenu() {
            CIRCLE_TEXTURE = AssetDatabase.LoadAssetAtPath<Texture2D>($"{IMAGES_PATH}circle.png");
            CIRCLE_DIRECTION_TEXTURE = AssetDatabase.LoadAssetAtPath<Texture2D>($"{IMAGES_PATH}circle_direction.png");
        }
        
        public PieMenu(PieMenuEntry[] options) {
            _entries = options;
            _angles = Angles(_entries.Length);
            _positions = Positions(_angles);
        }
        
        static int CalculateSelectedBox(float angle, float[] angles) {
            int biggerAngleIndex = Array.BinarySearch(angles, angle);
            if (biggerAngleIndex >= 0) {
                return biggerAngleIndex;
            }
            biggerAngleIndex = ~biggerAngleIndex;
            int smallerAngleIndex = biggerAngleIndex - 1;

            float biggerAngle = biggerAngleIndex == angles.Length ? 360f : angles[biggerAngleIndex];
            biggerAngleIndex = biggerAngleIndex % angles.Length;
            return angle - angles[smallerAngleIndex] < biggerAngle - angle ? smallerAngleIndex : biggerAngleIndex;
        }

        void Draw() {
            Handles.BeginGUI();

            Rect circleRect = CenteredRect(_center, Vector2.one * CIRCLE_RADIUS * 2f);
            GUI.DrawTexture(circleRect, CIRCLE_TEXTURE);
            if (_selected != -1) {
                Matrix4x4 oldMatrix = GUI.matrix;
                GUIUtility.RotateAroundPivot(_angle + 90f, _center);
                GUI.DrawTexture(circleRect, CIRCLE_DIRECTION_TEXTURE);
                GUI.matrix = oldMatrix;
            }

            for (int i = 0; i < _count; i++) {
                DrawBox(_center, _positions[i], ENTRY_RADIUS, _entries[i].text, i, _entries[i].icon, i == _selected);
            }

            Handles.EndGUI();
        }

        void OnSceneGUI(UnityEditor.SceneView view) {
            if (_center == -Vector2.one) {
                _center = Event.current.mousePosition;
            }
            
            // Using hotControl to be able to capture MouseUp,
            // otherwise it would be captured by Unity.
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            GUIUtility.hotControl = controlID;
            switch (Event.current.GetTypeForControl(controlID)) {
                case EventType.MouseUp:
                    switch (Event.current.button) {
                        case 0: 
                            PerformSelection();
                            break;
                        default:
                            Hide();
                            break;
                    }
                    Event.current.Use();
                    break;
                case EventType.MouseDown:
                    Event.current.Use();
                    break;
                case EventType.Repaint:
                    Draw();
                    break;
                case EventType.MouseMove:
                    Vector2 mouseRelativeToCenter = Event.current.mousePosition - _center;
                    if (mouseRelativeToCenter.sqrMagnitude <= CIRCLE_RADIUS * CIRCLE_RADIUS) {
                        _selected = -1;
                    }
                    else {
                        _angle = Mathf.Rad2Deg * Mathf.Atan2(mouseRelativeToCenter.y, mouseRelativeToCenter.x);
                        if (_angle < 0f) {
                            _angle = 360f + _angle;
                        }

                        _selected = CalculateSelectedBox(_angle, _angles);
                    }
                    break;
                case EventType.KeyUp:
                    if (Char.IsDigit(Event.current.character)) {
                        Event.current.Use();
                        int number = Event.current.character - '0';
                        if (number < _count) {
                            _selected = number;
                            PerformSelection();
                        }
                    }
                    else if (Event.current.keyCode == KeyCode.Escape) {
                        Event.current.Use();
                        Hide();
                    }
                    break;
                case EventType.KeyDown:
                    if (Char.IsDigit(Event.current.character)
                        || Event.current.keyCode == KeyCode.Escape) {
                        Event.current.Use();
                    }
                    break;
            }
            GUIUtility.hotControl = 0;
        }
        
        void Show() {
            _show = true;
            UnityEditor.SceneView.duringSceneGui += OnSceneGUI;
        }

        void Hide() {
            _show = false;
            UnityEditor.SceneView.duringSceneGui -= OnSceneGUI;
        }

        void PerformSelection() {
            if (_selected != -1) {
                _entries[_selected].action();
            }
            Hide();
        }
        
        public void Perform(ShortcutArguments arguments) {
            if (!_show && arguments.stage == ShortcutStage.Begin) {
                Show();
                _center = -Vector2.one;
                _selected = -1;
            }
            else if (_show && _selected != -1) {
                PerformSelection();
            }
        }
        
        static Vector2 SceneViewCenter(UnityEditor.SceneView view) {
            return 0.5f / EditorGUIUtility.pixelsPerPoint * new Vector2(view.camera.pixelWidth, view.camera.pixelHeight);
        }
        
        static Vector2[] Positions(float[] angles) {
            Vector2[] positions = new Vector2[angles.Length];        
            for (int i = 0; i < positions.Length; i++) {
                float rad = angles[i] * Mathf.Deg2Rad;
                positions[i] = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            }
            return positions;
        }

        static float[] Angles(int count) {
            int log2 = Log2(count, out int remainder);

            float[] angles = new float[count];
            float segment = 360f / (1 << (log2 + 1));
            int segmentIndex = 0;
            int index = 0;
            while (index < angles.Length) {
                angles[index++] = segmentIndex * segment;            
                if (remainder > 0) {
                    angles[index++] = (segmentIndex + 1) * segment;
                    remainder--;
                }
                segmentIndex += 2;
            }
            return angles;
        }
        
        static int Log2(int x) {
            int result = -1;
            while (x > 0) {
                x >>= 1;
                result++;
            }
            return result;
        }
        
        static int Log2(int x, out int remainder) {
            int log2 = Log2(x);
            remainder = x & ~(1 << log2);
            return log2;
        }
        
        static Rect CenteredRect(Vector2 position, Vector2 size) => CenteredRect(position.x, position.y, size.x, size.y);

        static Rect CenteredRect(float x, float y, float width, float height) {
            return new Rect(x - 0.5f * width, y - 0.5f * height, width, height);
        }

        static void DrawBox(Vector2 center, Vector2 circlePosition, float radius, string text, int number, Texture icon, bool selected = false) {
            GUIContent content = new GUIContent($" {text}    {number,2}", icon);

            GUIStyle style = GUI.skin.button;
            Vector2 size = style.CalcSize(content);
            Vector2 position = center + circlePosition * radius;
            position.y -= 0.5f * size.y;
            if (Mathf.Abs(circlePosition.x) < 0.01f) {
                position.x -= 0.5f * size.x;
            }
            else if (circlePosition.x < 0f) {
                position.x -= size.x;
            }
            Rect rect = new Rect(position, size);

            Color oldColor = GUI.color;
            if (selected) {
                float v = 1.25f;
                GUI.color = new Color(v,v,v,1f);
            }
            GUI.Box(rect, content, style);

            GUI.color = oldColor;
        }
    }
}