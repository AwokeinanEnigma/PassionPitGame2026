using UnityEditor;
using UnityEngine;

namespace Enigmaware.General
{
#if UNITY_EDITOR
    [CustomEditor(typeof(FPS))]
    public class FPSEditor : Editor
    {
        private void OnEnable()
        {
            SetMaxFPS(500);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Set Max FPS to 30"))
            {
                SetMaxFPS(30);
            }
            
            if (GUILayout.Button("Set Max FPS to 50"))
            {
                SetMaxFPS(50);
            }
            
            if (GUILayout.Button("Set Max FPS to 60"))
            {
                SetMaxFPS(60);
            }
            
            if (GUILayout.Button("Set Max FPS to 88"))
            {
                SetMaxFPS(88);
            }

            if (GUILayout.Button("Set Max FPS to 120"))
            {
                SetMaxFPS(120);
            }
            
            if (GUILayout.Button("Set Max FPS to 88"))
            {
                SetMaxFPS(152);
            }

            
            if (GUILayout.Button("Set Max FPS to 240"))
            {
                SetMaxFPS(240);
            }
        }

        private void SetMaxFPS(int max)
        {
            FPS fp = target as FPS;
            fp.SetFPS(max);
        }
    }
#endif
}