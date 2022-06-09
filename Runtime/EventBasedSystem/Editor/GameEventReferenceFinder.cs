using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace pow.aidkit
{
    public class GameEventReferenceFinder : EditorWindow
    {
        private static Dictionary<GameEvent, List<EventNode>> _gameEventsDict =
            new Dictionary<GameEvent, List<EventNode>>();

        private List<string> _toolbarStrings = new List<string>();
        private Vector2 _scrollPos;
        private int _toolbarInt;

        [MenuItem("POW SDK/Event Based System/Find Game Event References")]
        static void Do()
        {
            GetWindow<GameEventReferenceFinder>();
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();
            {
                if (GUILayout.Button("Search All Game Events"))
                {
                    _gameEventsDict = new Dictionary<GameEvent, List<EventNode>>();
                    _toolbarStrings = new List<string>();
                    List<GameEvent> gameEvents = Resources.FindObjectsOfTypeAll<GameEvent>().ToList();
                    foreach (var gameEvent in gameEvents)
                    {
                        _gameEventsDict.Add(gameEvent, new List<EventNode>());
                        _toolbarStrings.Add(gameEvent.name);
                    }

                    _toolbarStrings.Sort();
                    NamesExample();
                }

                if (_gameEventsDict.Count != 0)
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginVertical(GUILayout.Width(200));
                        {
                            _toolbarInt = GUILayout.SelectionGrid(_toolbarInt, _toolbarStrings.ToArray(), 1);
                        }
                        GUILayout.EndVertical();
                        GUILayout.BeginArea(new Rect(new Vector2(400, 50), new Vector2(1000, 600)));
                        {
                            DrawGameEventContent();
                        }
                        GUILayout.EndArea();
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawGameEventContent()
        {
            var pair = _gameEventsDict.SingleOrDefault(p => p.Key.name == _toolbarStrings[_toolbarInt]);
            foreach (var val in pair.Value)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(
                    $"{val.scenePath.Split('/').Last()}/{val.GetHierarchy()}/{val.methodName}");
                if (GUILayout.Button("Open Scene", GUILayout.Width(100), GUILayout.Height(20)))
                {
                    EditorSceneManager.OpenScene(val.scenePath);
                }

                if (GUILayout.Button("Highlight GameObject", GUILayout.Width(100), GUILayout.Height(20)))
                {
                    var objects = Resources.FindObjectsOfTypeAll<GameObject>()
                        .Where(obj => obj.name == val.parentGameObject);
                    foreach (var obj in objects)
                    {
                        bool isCorrectGameObject = true;
                        GameObject referenceObject = obj;
                        List<string> findingParents = new List<string>();
                        GameObject go = referenceObject;
                        findingParents.Add(go.name);
                        while (go != null && go.transform.parent != null)
                        {
                            var findingParent = go.transform.parent;
                            findingParents.Add(findingParent.name);
                            go = findingParent.gameObject;
                        }

                        if (findingParents.Count == val.parents.Count)
                        {
                            for (int i = 0; i < val.parents.Count; i++)
                            {
                                if (findingParents[i] != val.parents[i])
                                {
                                    isCorrectGameObject = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            isCorrectGameObject = false;
                        }

                        if (isCorrectGameObject)
                        {
                            if (obj.activeInHierarchy)
                            {
                                EditorGUIUtility.PingObject(obj);
                                break;
                            }
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void NamesExample()
        {
            Debug.Log("*** FINDING ASSETS BY NAME ***");
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    EditorSceneManager.OpenScene(scene.path);
                    GameObject[] objs = FindObjectsOfType<GameObject>();
                    foreach (GameObject obj in objs)
                    {
                        GameEventListener[] listeners = obj.GetComponents<GameEventListener>();
                        foreach (var listener in listeners)
                        {
                            if (listener != null)
                            {
                                foreach (var gameEvent in listener.Events)
                                {
                                    for (int i = 0; i < listener.Response.GetPersistentEventCount(); i++)
                                    {
                                        string method =
                                            $"{listener.Response.GetPersistentMethodName(i)} / {listener.gameObject.name}";
                                        List<string> parents = new List<string>();
                                        GameObject go = listener.gameObject;
                                        parents.Add(go.name);
                                        while (go != null && go.transform.parent != null)
                                        {
                                            var parent = go.transform.parent;
                                            parents.Add(parent.name);
                                            go = parent.gameObject;
                                        }

                                        EventNode eventNode = new EventNode(SceneManager.GetActiveScene().path,
                                            listener.gameObject.name,
                                            method, parents);
                                        _gameEventsDict[gameEvent].Add(eventNode);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public class EventNode
    {
        public string scenePath;
        public string parentGameObject;
        public string methodName;
        public List<string> parents = new List<string>();

        public EventNode(string scenePath, string parentGameObject, string methodName, List<string> parents)
        {
            this.scenePath = scenePath;
            this.parentGameObject = parentGameObject;
            this.methodName = methodName;
            this.parents = parents;
        }

        public string GetHierarchy()
        {
            string parentsString = "";
            var tmpParents = new List<string>();
            foreach (var parent in parents)
            {
                tmpParents.Add(parent);
            }

            tmpParents.Reverse();
            foreach (var parent in tmpParents)
            {
                parentsString += parent + "/";
            }

            return parentsString;
        }
    }
}