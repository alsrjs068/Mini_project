using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Player_move : MonoBehaviour
{
    #region 인스펙터
    [Header("플레이어")]
    [SerializeField] private Transform _player;
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _controller;

    [Header("카메라 기준 이동(옵션)")]
    [SerializeField] private Transform _cameraTr;

    [Header("이동")]
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _rotateSharpness = 15.0f;

    [Header("점프")]
    [SerializeField] private float _jumpHeight = 1.2f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private float _groundStick = -2.0f;

    [Header("애니메이터 파라미터")]
    [SerializeField] private string _paramSpeed = "fSpeed";
    [SerializeField] private string _paramRun = "bRun";
    [SerializeField] private string _paramJump = "tJump";

    [Header("애니메이터 튜닝")]
    [SerializeField] private float _speedDamp = 0.12f;

    [Header("디버그")]
    [SerializeField] private bool _drawDebug = true;

    #endregion

    // 내부 변수

    private float _verticalValue;
    private int _hashSpeed;
    private int _hashRun;
    private int _hashJump;
    private bool _hasRunParam;
    private bool _hasJumpParam;
    private string _WinTrigger = "tWin";
    

    private void Reset()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        
        if (_controller == null)
        {
            _controller = GetComponent<CharacterController>();
        }

        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        if (_cameraTr == null || Camera.main != _cameraTr)
        {
            _cameraTr = Camera.main.transform;
        }


        _hashSpeed = Animator.StringToHash(_paramSpeed);

        _hasRunParam = !string.IsNullOrEmpty(_paramRun);
        if (_hasRunParam)
        {
            _hashRun = Animator.StringToHash(_paramRun);
        }

        _hasJumpParam = !string.IsNullOrEmpty(_paramJump);
        if (_hasJumpParam)
        {
            _hashJump = Animator.StringToHash(_paramJump);
        }
    }

    void Start()
    {
        CPrint.Group("플레이어 애니메이션", () =>
        {
            bool useCameraRelative = (_cameraTr != null);

            if (_cameraTr != null)
            {
                CPrint.Log($"카메라 = {_cameraTr.name}");
            }

            else
            {
                CPrint.Warn("카메라가 없다 / 월드 기준 이동");
            }
        });

        InitThird(true);
        
        _cameraTr = _cameraTr.transform;

    }

    void Update()
    {
        if (_player == null || _controller == null || _animator == null)
        {
            CPrint.Once("참조 누락", "인스펙터 확인");
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0, v);

        input = Vector3.ClampMagnitude(input.normalized, 1.0f);


        bool JumpKeyDown = Input.GetKeyDown(KeyCode.Space);

        bool jumpedThisFrame = TickJumpAndGravity(JumpKeyDown);


        if (_hasJumpParam && jumpedThisFrame)
        {
            _animator.SetTrigger(_hashJump);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            _animator.SetTrigger("tAttack");
        }

        if (Input.GetMouseButtonDown(0))
        {
            _animator.SetTrigger(_WinTrigger);
        }

        Vector3 moveDir = (input.sqrMagnitude > 0.0001f) ? BuildMoveDirection(input) : Vector3.zero;

        Vector3 velocity = moveDir * _speed;
        velocity.y = _verticalValue;

        _controller.Move(velocity * Time.deltaTime);

        TickRotate(moveDir);

        float speed01 = moveDir.magnitude;

        _animator.SetFloat(_hashSpeed, speed01, _speedDamp, Time.deltaTime);

    }

    private void LateUpdate()
    {
        TickThird();   
    }

    private bool TickJumpAndGravity(bool jumpKeyDown)
    {
        bool jumped = false;

        if(_controller.isGrounded)
        {
            if(_verticalValue < 0.0f )
            {
                _verticalValue = _groundStick;
            }

            if (jumpKeyDown)
            {
                _verticalValue = Mathf.Sqrt(_jumpHeight * -2.0f * _gravity);

                jumped = true;

            }

        }

        _verticalValue += _gravity * Time.deltaTime;

        return jumped;
    }

    private void TickRotate(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);

        transform.rotation = Quaternion.Slerp
            (
                transform.rotation,
                targetRot,
                1.0f - Mathf.Exp(-_rotateSharpness * Time.deltaTime)

            );
    }



    private float GetSmoothT(float sharpness)
    {
        return 1f - Mathf.Exp(-sharpness * Time.deltaTime);
    }

    private void ApplyPose(Vector3 desirePos, Quaternion desireRot, float sharpness, bool snap)
    {
        if (snap)
        {
            _cameraTr.position = desirePos;
            _cameraTr.rotation = desireRot;

            return;
        }

        float t = GetSmoothT(sharpness);

        _cameraTr.position = Vector3.Lerp(_cameraTr.position, desirePos, t);
        _cameraTr.rotation = Quaternion.Slerp(_cameraTr.rotation, desireRot, t);

    }
}
