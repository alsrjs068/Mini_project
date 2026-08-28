using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class windmill : MonoBehaviour
{

    [Header("Ç³Â÷ È¸Àü Å¸°Ù")]
    [SerializeField] private Transform _windMill;

    [Header("¼Óµµ (deg / sec)")]
    [SerializeField] private float _speed = 90.0f;

    private void Update()
    {
        float rot = _speed * Time.deltaTime;

        transform.Rotate(Vector3.forward, Space.Self);
    }


}
