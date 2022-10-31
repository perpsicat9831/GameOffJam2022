using AK.Wwise;
using UnityEngine;
using UnityEditor;
using AudioStudio.Components;
using AudioStudio.Tools;

namespace AudioStudio.Editor
{
    [CustomEditor(typeof(EmitterSound)), CanEditMultipleObjects]
    public class EmitterSoundInspector : AsComponentInspector
    {
        private EmitterSound _component;
        private int _curPointIndex = -1;
        private bool _hideDefaultHandle;

        private void OnEnable()
        {
            _component = target as EmitterSound;
            CheckDataBackedUp(_component);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            ShowPhysicalSettings(_component, true);
            ShowSpatialSettings();
            ShowPlaybackSettings();
            AsGuiDrawer.DrawList(serializedObject.FindProperty("AudioEvents"), "Audio Events:", WwiseObjectType.Event, AddEvent);
            if (_component.EnvironmentSource == EnvironmentSource.GameObject) AsGuiDrawer.CheckLinkedComponent<Collider>(_component);
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) CheckDataBackedUp(_component);
            ShowButtons(_component);
        }
        
        private void AddEvent(WwiseObjectReference reference)
        {
            var newEvent = new AudioEvent();
            newEvent.SetupReference(reference.ObjectName, reference.Guid);
            AsScriptingHelper.AddToArray(ref _component.AudioEvents, newEvent);
        }

        private void ShowSpatialSettings()
        {
            EditorGUILayout.LabelField("Spatial Settings", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MultiPositionType"), new GUIContent("Position Type: "));
                switch (_component.MultiPositionType)
                {
                    case MultiPositionType.SimpleMode:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("UpdateFrequency"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PositionOffset"));
                        break;
                    case MultiPositionType.MovingPointMode:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("UpdateFrequency"));
                        break;
                }
                AsGuiDrawer.DrawProperty(serializedObject.FindProperty("Listeners"), "Listeners (0 is default)", 150);
                ShowEnvironmentSettings(_component);
            }
        }

        private void ShowPlaybackSettings()
        {
            EditorGUILayout.LabelField("Playback Settings", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PlayMode"));
                if (_component.PlayMode == EventPlayMode.PeriodTrigger)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("InitialDelay"));
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Trigger Interval", GUILayout.Width(116));
                    EditorGUILayout.LabelField("Min", GUILayout.Width(30));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("MinInterval"), GUIContent.none, GUILayout.Width(30));
                    EditorGUILayout.LabelField("Max", GUILayout.Width(30));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxInterval"), GUIContent.none, GUILayout.Width(30));
                    GUILayout.EndHorizontal();
                }
                AsGuiDrawer.DrawProperty(serializedObject.FindProperty("FadeOutTime"), "Fade Out Time On Disable", 160);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PauseIfInvisible"));
            }
        }
        
        private void ShowPositionManager(int windowID)
        {
            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            GUILayout.BeginHorizontal();
            GUI.contentColor = Color.green;
            if (GUILayout.Button("Add", EditorStyles.toolbarButton))
                _component.MultiPositionArray.Add(_component.transform.InverseTransformPoint(_component.transform.position));

            GUI.contentColor = Color.red;
            if (_curPointIndex >= 0 && GUILayout.Button("Delete", EditorStyles.toolbarButton))
            {
                _component.MultiPositionArray.RemoveAt(_curPointIndex);
                _curPointIndex = _component.MultiPositionArray.Count - 1;
            }

            GUI.contentColor = Color.white;
            if (_component.MultiPositionArray.Count > 0)
            {
                if (GUILayout.Button("◀", EditorStyles.miniButtonLeft, GUILayout.Width(20f)))
                {
                    _curPointIndex--;
                    if (_curPointIndex < 0) _curPointIndex = _component.MultiPositionArray.Count - 1;
                }

                if (GUILayout.Button("▶", EditorStyles.miniButtonRight, GUILayout.Width(20f)))
                {
                    _curPointIndex++;
                    if (_curPointIndex >= _component.MultiPositionArray.Count) _curPointIndex = 0;
                }
            }
            GUILayout.EndHorizontal();

            if (_component.MultiPositionArray.Count > 0 && _curPointIndex >= 0)
            {
                GUILayout.BeginHorizontal();
                var pointPositionX = Mathf.Round(_component.MultiPositionArray[_curPointIndex].x * 1000) / 1000;
                var pointPositionY = Mathf.Round(_component.MultiPositionArray[_curPointIndex].y * 1000) / 1000;
                var pointPositionZ = Mathf.Round(_component.MultiPositionArray[_curPointIndex].z * 1000) / 1000;
                EditorGUILayout.LabelField("X", GUILayout.Width(12));
                pointPositionX = EditorGUILayout.FloatField(pointPositionX, GUILayout.Width(44));
                EditorGUILayout.LabelField("Y", GUILayout.Width(12));
                pointPositionY = EditorGUILayout.FloatField(pointPositionY, GUILayout.Width(44));
                EditorGUILayout.LabelField("Z", GUILayout.Width(12));
                pointPositionZ = EditorGUILayout.FloatField(pointPositionZ, GUILayout.Width(44));
                _component.MultiPositionArray[_curPointIndex] = new Vector3(pointPositionX, pointPositionY, pointPositionZ);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            GUI.contentColor = Color.yellow;
            if (_hideDefaultHandle)
            {
                if (GUILayout.Button("Show Main Transform", EditorStyles.toolbarButton))
                {
                    _hideDefaultHandle = false;
                    DefaultHandles.Hidden = _hideDefaultHandle;
                }
            }
            else if (GUILayout.Button("Hide Main Transform", EditorStyles.toolbarButton))
            {
                _hideDefaultHandle = true;
                DefaultHandles.Hidden = _hideDefaultHandle;
            }
            GUI.contentColor = Color.white;
        }

        private void OnSceneGUI()
        {
            if (_component.MultiPositionType != MultiPositionType.LargeMode)
                return;

            var someHashCode = GetHashCode();
            Handles.matrix = Matrix4x4.TRS(_component.transform.position, Quaternion.identity, Vector3.one);

            for (var i = 0; i < _component.MultiPositionArray.Count; i++)
            {
                var pos = _component.transform.rotation * _component.MultiPositionArray[i];
                Handles.Label(pos, "Point_" + i);

                var handleSize = HandleUtility.GetHandleSize(pos);

                // Get the needed data before the handle
                var controlIDBeforeHandle = GUIUtility.GetControlID(someHashCode, FocusType.Passive);
                var isEventUsedBeforeHandle = UnityEngine.Event.current.type == EventType.Used;

                Handles.color = GetColor(i);
                Handles.CapFunction capFunc = Handles.SphereHandleCap;
                Handles.ScaleValueHandle(0, pos, Quaternion.identity, handleSize, capFunc, 0);

                if (_curPointIndex == i)
                    pos = Handles.PositionHandle(pos, Quaternion.identity);

                // Get the needed data after the handle
                var controlIDAfterHandle = GUIUtility.GetControlID(someHashCode, FocusType.Passive);
                var isEventUsedByHandle = !isEventUsedBeforeHandle && UnityEngine.Event.current.type == EventType.Used;

                if (controlIDBeforeHandle < GUIUtility.hotControl &&
                    GUIUtility.hotControl < controlIDAfterHandle || isEventUsedByHandle)
                    _curPointIndex = i;

                _component.MultiPositionArray[i] = Quaternion.Inverse(_component.transform.rotation) * pos;
            }

            Handles.BeginGUI();

            var size = new Rect(0, 0, 200, 90);
            GUI.Window(0,
                new Rect(Screen.width - size.width - 10, Screen.height - size.height - 30,
                    size.width, size.height), ShowPositionManager, "Position Manager");

            Handles.EndGUI();
        }

        private Color GetColor(int index)
        {
            switch (index % 12)
            {
                case 0:
                    return new Color(1, 0, 1);
                case 1:
                    return new Color(1, 0, 0.5f);
                case 2:
                    return new Color(1, 0.5f, 0.5f);
                case 3:
                    return new Color(1, 0.5f, 0);
                case 4:
                    return new Color(1, 1, 0);
                case 5:
                    return new Color(0.5f, 1, 0.5f);
                case 6:
                    return new Color(0, 1, 0.5f);
                case 7:
                    return new Color(0, 1, 1);
                case 8:
                    return new Color(0, 0.5f, 1);
                case 9:
                    return new Color(1, 0.5f, 1);
                case 10:
                    return new Color(0.5f, 0.5f, 1);
                case 11:
                    return new Color(0.5f, 0f, 1f);
            }

            return Color.white;
        }

        private class DefaultHandles
        {
            public static bool Hidden
            {
                get
                {
                    var type = typeof(UnityEditor.Tools);
                    var field = type.GetField("s_Hidden",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    return (bool) field.GetValue(null);
                }
                set
                {
                    var type = typeof(UnityEditor.Tools);
                    var field = type.GetField("s_Hidden",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    field.SetValue(null, value);
                }
            }
        }
    }
}