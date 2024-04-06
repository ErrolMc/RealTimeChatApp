using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] Vector3 rotation;

    private Transform _trans;

    private void OnEnable()
    {
        _trans = transform;
    }

    private void Update()
    {
        _trans.Rotate(rotation);
    }    
}
