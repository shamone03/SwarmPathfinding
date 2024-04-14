using System.Collections.Generic;
using System.IO;
using System.Linq;
using Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Waypoints {
    public class Waypoints : MonoBehaviour, ITargetProvider {
        private readonly Stack<Waypoint> _targets = new();

        private static int count;

        private float _timeTaken;
        private float _timeStarted;
        public Vector3? Target {
            get {
                if (!_targets.TryPeek(out var target)) return null;
                target.IsActive = true;
                return target.transform.position;
            }
        } 
    
        private void Start() {
            _timeStarted = Time.time;
            foreach (var waypoint in GetComponentsInChildren<Waypoint>().Reverse()) {
                waypoint.TargetProvider = this;
                _targets.Push(waypoint);
            }
        }

        public void OnTargetComplete() {
            var waypoint = _targets.Pop();
            waypoint.IsActive = true;
            if (_targets.Count == 0) {
                _timeTaken = Time.time - _timeStarted;
                Debug.Log($"Time Taken: {_timeTaken}");
                string outputFile;
                if (count > 10)
                {
                    outputFile = "Assets/Scripts/Data/individualOutput.txt";
                }
                else
                {
                    outputFile = "Assets/Scripts/Data/globalOutput.txt";
                }
                
                // Write timeTaken to the output file
                using (StreamWriter writer = new StreamWriter(outputFile, true))
                {
                    writer.WriteLine(_timeTaken);
                }
                
                count++;
                
                if (count != 20)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }

    }
}