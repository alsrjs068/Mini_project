using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MoveScript : MonoBehaviour
{
    #region 인스펙터
    [Header("플레이어")]
    [SerializeField] private Transform _player;
    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody _rb;

    [Header("카메라 기준 이동(옵션)")]
    [SerializeField] private Transform _cameraTr;

    [Header("이동")]
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _runSpeed = 6f;
    [SerializeField] private float _rotateSharpness = 15.0f;

    [Header("점프")]
    [SerializeField] private float _jumpForce = 3f;

    [Header("애니메이터 파라미터")]
    [SerializeField] private string _paramSpeed = "fSpeed";
    [SerializeField] private string _paramRun = "bRun";
    [SerializeField] private string _paramJump = "tJump";

    [Header("애니메이터 튜닝")]
    [SerializeField] private float _speedDamp = 0.12f;

    [Header("레이어 마스크")]
    [SerializeField] private LayerMask _groundLayer = default;

    [Header("디버그")]
    [SerializeField] private bool _drawDebug = true;

    #endregion

    // 인스펙터

    private bool _isGrounded;
    private bool _isRunning;
    private int _hashSpeed;
    private int _hashRun;
    private int _hashJump;
    private bool _hasRunParam;
    private bool _hasJumpParam;



    private void Reset()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {

        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
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
        if (_player == null || _rb == null || _animator == null)
        {
            CPrint.Once("참조 누락", "인스펙터 확인");
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0, v);

        input = Vector3.ClampMagnitude(input.normalized, 1.0f);


        _isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.2f, _groundLayer);

        if (_drawDebug)
        {
            Debug.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * 0.2f, _isGrounded ? Color.green : Color.red);
        }

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z); _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);

            if (_hasJumpParam)
            {
                _animator.SetTrigger(_hashJump);
            }
        }

        Vector3 moveDir = (input.sqrMagnitude > 0.0001f) ? BuildMoveDirection(input) : Vector3.zero;

        _isRunning = Input.GetKey(KeyCode.LeftShift);

        float speed = 1f;

        if (!_isRunning)
        {
            speed = _walkSpeed;

        }
            
        else if (_isRunning)
        {

            speed = _runSpeed;
            
        }

        Vector3 velocity = moveDir * speed;

        _rb.velocity = new Vector3(velocity.x, _rb.velocity.y, velocity.z);

        TickRotate(moveDir);

        float speed01 = moveDir.magnitude;

        _animator.SetBool(_hashRun, _isRunning);

        _animator.SetFloat(_hashSpeed, speed01, _speedDamp, Time.deltaTime);
    }

    

    private void LateUpdate()
    {
        TickThird();
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
