using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Boid {
    public class Flock : MonoBehaviour, INeighbours {
        [SerializeField] private int count;
        [SerializeField] private GameObject boid;
        [SerializeField] private List<Boid> boids;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float spawnRadius;
        [SerializeField] private Transform targetProvider;
        public Vector3 AvgPosition => boids.Aggregate(Vector3.zero, (avg, cur) => avg + cur.transform.position, avg => avg / boids.Count);
        public int Count => boids.Count;
        public Boid this[int i] => boids[i];
        
        private static float _socialInput = 1.0f;
        public static int runCount { get; set; }

        private void Start() {
            foreach (var i in Enumerable.Range(0, count)) {
                var instance = Instantiate(boid, spawnPoint.transform.position + Random.insideUnitSphere * spawnRadius, Quaternion.identity);
                instance.GetComponent<Boid>().ID = i + 1;
                instance.GetComponent<Boid>().Neighbours = this;
                instance.GetComponent<Boid>().Social = _socialInput;
                instance.GetComponent<Boid>().Cognitive = 1.0f - _socialInput;
                instance.GetComponent<Boid>().flockRunCount = runCount;
                instance.GetComponent<Boid>().TargetProvider = targetProvider.GetComponent<ITargetProvider>();
                boids.Add(instance.GetComponent<Boid>());
            }

            _socialInput -= 0.1f;
            runCount++;
        }

        public List<Boid> Get(Vector3 position, float perception) {
            return boids.Where(i => Vector3.Distance(i.transform.position, position) < perception).ToList();
        }

        public List<Boid> Get() {
            return boids;
        }
        
    }
}