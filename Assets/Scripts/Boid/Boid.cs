using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Boid {
    [Serializable]
    public struct Multipliers {
        [field: SerializeField] public float Alignment { get; set; }
        [field: SerializeField] public float Cohesion { get; set; }
        [field: SerializeField] public float Separation { get; set; }
        [field: SerializeField] public float Pathfinding { get; set; }
    }

    public class Boid : MonoBehaviour {
        [field: SerializeField] public Multipliers Multipliers { private get; set; }
        [field: SerializeField] public float Speed { private get; set; }
        [field: SerializeField] public Transform Flock { private get; set; }
        
        private INeighbours _neighbours;
        [field: SerializeField] public float Cognitive { get; set; } = .8f;
        [field: SerializeField] public float Social { get; set; } = .2f;
        [field: SerializeField] public Transform Target { private get; set; }

        private Vector3 PersonalBest { get; set; }

        private Vector3 GlobalBest {
            get {
                var neighbors = _neighbours.Get();
                return neighbors.Aggregate(neighbors.First(), (min, boid) =>
                        Vector3.Distance(min.PersonalBest, Target.position) <
                        Vector3.Distance(boid.PersonalBest, Target.position)
                    ? min
                    : boid,
                    boid => boid.PersonalBest);
            }
        }

        private Transform _worldBounds;
        private float _worldBoundsXMax;
        private float _worldBoundsXMin;
        private float _worldBoundsZMax;
        private float _worldBoundsZMin;
        private float _worldBoundsYMax;
        
        [SerializeField] private float perception;

        [field: SerializeField] public Vector3 Velocity { get; private set; }
        
        private void Start() {
            // Multipliers = new Multipliers { Alignment = 1, Separation = 1, Cohesion = 1 };
            
            // Get the boundary around terrain and set the bound
            _worldBounds = Flock.GetComponent<Flock>().Bound.transform;
            foreach (Transform plane in _worldBounds)
            {
                // Get Max and Min in X Axis
                if (plane.position.x > _worldBoundsXMax)
                {
                    _worldBoundsXMax = plane.transform.position.x;
                }
                else if (plane.position.x < _worldBoundsXMin)
                {
                    _worldBoundsXMin = plane.transform.position.x;
                }
                
                // Get Max and Min in Z Axis
                if (plane.transform.position.z > _worldBoundsZMax)
                {
                    _worldBoundsZMax = plane.transform.position.z;
                }
                else if (plane.position.z < _worldBoundsZMin)
                {
                    _worldBoundsZMin = plane.transform.position.z;
                }
            }

            // Ceiling
            _worldBoundsYMax = 40;
            
            float randomX = Random.Range(_worldBoundsXMin, _worldBoundsXMax);
            float randomY = Random.Range(0, _worldBoundsYMax);
            float randomZ = Random.Range(_worldBoundsZMin, _worldBoundsZMax);
            
            Velocity = new Vector3(randomX, randomY, randomZ);
            
            _neighbours = Flock.GetComponent<INeighbours>();
            PersonalBest = transform.position;
        }

        private void Update() {

            Velocity += GetAcceleration(_neighbours.Get(transform.position, perception));
            Velocity = Vector3.ClampMagnitude(Velocity, Speed);
            transform.position += Velocity * Time.deltaTime;

            if (Vector3.Distance(transform.position, Target.position) < Vector3.Distance(PersonalBest, Target.position)) {
                PersonalBest = transform.position;
            }
            
            Bounds();
        }

        private Vector3 GetAcceleration(List<Boid> neighbours) {
            return Separation(neighbours) * Multipliers.Separation + 
                   Alignment(neighbours) * Multipliers.Alignment + 
                   Cohesion(neighbours) * Multipliers.Cohesion + 
                   Pathfinding(neighbours) * Multipliers.Pathfinding;
        }

        private Vector3 Pathfinding(List<Boid> neighbours) {
            var global = Social * (GlobalBest - transform.position);
            var personal = Cognitive * (PersonalBest - transform.position);
            
            var pathfinding = personal + global;
            
            pathfinding -= Velocity;
            pathfinding = pathfinding.normalized * Speed;
            pathfinding = Vector3.ClampMagnitude(pathfinding, 0.25f);

            return pathfinding;
        }

        private Vector3 Alignment(List<Boid> neighbours) {
            var alignment = Vector3.zero;

            foreach (var boid in neighbours) {
                if (this != boid) {
                    alignment += boid.Velocity;
                }
            }

            if (neighbours.Count - 1 > 0) {
                alignment /= neighbours.Count - 1;
                alignment = alignment.normalized * Speed;
                alignment -= Velocity;
                alignment = Vector3.ClampMagnitude(alignment, 0.25f);
            }
            return alignment;
        }

        private Vector3 Cohesion(List<Boid> neighbours) {
            var cohesion = Vector3.zero;
            foreach (var boid in neighbours) {
                if (this != boid) {
                    cohesion += boid.transform.position;
                }
            }
            if (neighbours.Count - 1 > 0) {
                cohesion /= neighbours.Count - 1;
                cohesion -= transform.position;
                cohesion = cohesion.normalized * Speed;
                cohesion -= Velocity;
                cohesion = Vector3.ClampMagnitude(cohesion, 0.25f);
            }
            return cohesion;
        }

        private Vector3 Separation(List<Boid> neighbours) {
            var separation = Vector3.zero;
            foreach (var boid in neighbours) {
                if (this != boid) {
                    var offset = transform.position - boid.transform.position;
                    separation += offset / offset.sqrMagnitude;
                }
            }
            
            if (neighbours.Count - 1 > 0) {
                separation /= neighbours.Count - 1;
                separation = separation.normalized * Speed;
                separation -= Velocity;
                separation = Vector3.ClampMagnitude(separation, 0.25f);
            }
            return separation;
        }

        
        
        private void Bounds() {
            if (transform.position.x > _worldBoundsXMax) transform.position = new Vector3(_worldBoundsXMin, transform.position.y, transform.position.z);
            if (transform.position.x < _worldBoundsXMin) transform.position = new Vector3(_worldBoundsXMax, transform.position.y, transform.position.z);
            if (transform.position.y > _worldBoundsYMax) transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            if (transform.position.y < 0) transform.position = new Vector3(transform.position.x, _worldBoundsYMax, transform.position.z);
            if (transform.position.z > _worldBoundsZMax) transform.position = new Vector3(transform.position.x, transform.position.y, _worldBoundsZMin);
            if (transform.position.z < _worldBoundsZMin) transform.position = new Vector3(transform.position.x, transform.position.y, _worldBoundsZMax);
        }
    }
}