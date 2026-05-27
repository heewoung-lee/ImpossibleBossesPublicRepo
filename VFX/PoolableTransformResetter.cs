using System.Collections.Generic;
using UnityEngine;

public class PoolableTransformResetter : MonoBehaviour
{
    private List<Transform> _targetTransforms;
    private List<Rigidbody> _targetRigidbodies;
    private List<Vector3> _originPositions;
    private List<Quaternion> _originRotations;

    private void Awake()
    {
        _targetTransforms = new List<Transform>();
        _targetRigidbodies = new List<Rigidbody>();
        _originPositions = new List<Vector3>();
        _originRotations = new List<Quaternion>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.TryGetComponent(out Rigidbody childRigidbody) == false)
            {
                continue;
            }

            _targetTransforms.Add(child);
            _targetRigidbodies.Add(childRigidbody);
            _originPositions.Add(child.position);
            _originRotations.Add(child.rotation);
        }
    }
    private void OnDisable()
    {
        transform.position = Vector3.zero;
        for (int i = 0; i < _originPositions.Count; i++)
        {
            Transform childTR = _targetTransforms[i];
            Rigidbody childRigidbody = _targetRigidbodies[i];
            childTR.position = _originPositions[i];
            childTR.rotation = _originRotations[i];

            childRigidbody.isKinematic = false;
            childRigidbody.useGravity = false;
            childRigidbody.linearVelocity = Vector3.zero;
            childRigidbody.angularVelocity = Vector3.zero;
        }

        GetComponent<Collider>().enabled = true;
        
    }
}
