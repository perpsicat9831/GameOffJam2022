using AudioStudio.Components;
using UnityEditor;
using UnityEngine;

namespace AudioStudio.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AudioRoomConnector))]
    public class AudioRoomConnectorInspector : AsComponentInspector
    {
        private readonly int[] _selectedIndex = new int[2];
        private readonly RoomPriorityList[] _roomList = { new RoomPriorityList(), new RoomPriorityList() };

        private AudioRoomConnector _component;

        private void OnEnable()
        {
            _component = target as AudioRoomConnector;
            CheckDataBackedUp(_component);
            _component.FindOverlappingRooms(_roomList);
            for (var i = 0; i < 2; i++)
            {
                var index = _roomList[i].BinarySearch(_component.Rooms[i]);
                _selectedIndex[i] = index == -1 ? 0 : index;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Separator();
            serializedObject.Update();
            _component.FindOverlappingRooms(_roomList);

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var labels = new [] { "Back", "Front" };
                for (var i = 0; i < 2; i++)
                {
                    var roomListCount = _roomList[i].Rooms.Count;
                    var roomLabels = new string[roomListCount];
                    for (var j = 0; j < roomListCount; j++)
                        roomLabels[j] = j + 1 + ". " + _roomList[i].Rooms[j].name;
                    _selectedIndex[i] = EditorGUILayout.Popup(labels[i] + " Room", Mathf.Clamp(_selectedIndex[i], 0, roomListCount - 1), roomLabels);
                    _component.Rooms[i] = _selectedIndex[i] < 0 || _selectedIndex[i] >= roomListCount ? null : _roomList[i].Rooms[_selectedIndex[i]];
                }
            }
            serializedObject.ApplyModifiedProperties();
            ShowButtons(_component);
        }
    }
}