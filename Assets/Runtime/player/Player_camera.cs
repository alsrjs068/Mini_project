using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player_move : MonoBehaviour
{
    #region 인스펙터
    [Header("3인칭 (오빗)")]
    [SerializeField] private Vector3 _thirdOffset = new Vector3(0f, 1.5f, -2f);
    [SerializeField] private float _thirdLookAtHeight = 1.5f;
    [Min(0f)]
    [SerializeField] private float _thirdSharpness = 18f;

    [Header("3인칭 오빗 옵션")]
    [SerializeField] private bool _thirdUseOrbit = true;
    [SerializeField] private float _orbitSensitivity = 3.0f;
    [SerializeField] private float _orbitPitchMin = -8.0f;
    [SerializeField] private float _orbitPitchMax = 20.0f;
    #endregion

    // 내부 변수
    private float _orbitYaw;
    private float _orbitPitch;

    Vector3 desirePos;
    Quaternion desireRot;

    private void InitThird(bool snap)
    {
        _orbitYaw = _player.eulerAngles.y;
        _orbitPitch = 12f;
        
    }


    private void TickThird()
    {
        BuildThirdPose(out desirePos, out desireRot);

        // 어떻게 갈 것인가 결정
        ApplyPose(desirePos, desireRot, _thirdSharpness, false);

        if (_drawDebug)
        {
            // 카메라 정면 확인
            CPrint.Ray(_cameraTr.position, _cameraTr.forward * 2.0f, Color.red);
        }
    }

    private void BuildThirdPose(out Vector3 desirePos, out Quaternion desireRot)
    {

        // 입력 처리
        if (_thirdUseOrbit && Input.GetMouseButton(1))
        {
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            _orbitYaw += mx * _orbitSensitivity;
            _orbitPitch += my * _orbitSensitivity;

            _orbitPitch = Mathf.Clamp(_orbitPitch, _orbitPitchMin, _orbitPitchMax);
        }


        if (_thirdUseOrbit)
        {
            Quaternion orbitRot = Quaternion.Euler(_orbitPitch, _orbitYaw, 0f);

            desirePos = _player.position + (orbitRot * _thirdOffset);

            Vector3 LookPos = _player.position + Vector3.up * _thirdLookAtHeight;

            desireRot = Quaternion.LookRotation(LookPos - desirePos, Vector3.up);
        }

        else
        {
            desirePos = _player.position + (_player.rotation * _thirdOffset);

            Vector3 lookPos = _player.position + Vector3.up * _thirdLookAtHeight;
            desireRot = Quaternion.LookRotation(lookPos - desirePos, Vector3.up);
        }

    }

    private Vector3 BuildMoveDirection(Vector3 input)
    {
        if (_cameraTr == null)
        {
            return input.normalized;
        }

        Vector3 camF = Vector3.ProjectOnPlane(_cameraTr.forward, Vector3.up).normalized;
        Vector3 camR = Vector3.ProjectOnPlane(_cameraTr.right, Vector3.up).normalized;

        Vector3 dir = camF * input.z + camR * input.x;
        return dir.normalized;
    }


}
