using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class SceneTranslationUI : MonoBehaviour
{
    #region 인스펙터
    [Header("페이드")]
    [SerializeField] private CanvasGroup _fadeGroup;
    [SerializeField] private float _defaultFadeDuration = 0.25f;
    [SerializeField] private bool _useUnscaledTime = true;

    [Header("로딩 텍스트")]
    [SerializeField] private Text _loadingTEXT;
    [SerializeField] private TMP_Text _loadingTMP;

    [Header("옵션")]
    [SerializeField] private bool _hideTextWhenEmpty = true;


    #endregion

    // 내부변수
    private Coroutine _fadeRoutine;

    public void Initialize()
    {
        if (_fadeGroup == null)
        {
            CPrint.Warn("인스펙터 확인");

            return;

        }

        _fadeGroup.alpha = 0.0f;                
        _fadeGroup.blocksRaycasts = false;     
        _fadeGroup.interactable = false;       

        SetLoadingText("");   

        CPrint.Log("초기화 완료");

    }
    public void SetLoadingText(string msg)
    {

        if (_loadingTMP != null)
        {
            _loadingTMP.text = msg;

            if (_hideTextWhenEmpty)
            {
                _loadingTMP.enabled = !string.IsNullOrEmpty(msg);
            }
        }
    }

    public IEnumerator Co_FadeTo(float targetAlpha, float duration = -1f, bool blockRayCastWhileFading = true)
    {
        if (_fadeGroup == null)
        {
            CPrint.Warn("그룹이 비어있으니 인스펙터 확인");
            yield break;
        }

        if (duration < 0f)
        {
            duration = _defaultFadeDuration;
        }

        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);

            _fadeRoutine = null;
        }

        _fadeRoutine = StartCoroutine(Co_Fade_Internal(targetAlpha, duration, blockRayCastWhileFading));

        yield return _fadeRoutine;

        _fadeRoutine = null; 

    }

    private IEnumerator Co_Fade_Internal(float targetAlpha, float duration, bool blockRayCastWhileFading)
    {
     
        float startAlpha = _fadeGroup.alpha;


        _fadeGroup.blocksRaycasts = blockRayCastWhileFading;


        _fadeGroup.interactable = false;

        if (duration <= 0f)
        {
            _fadeGroup.alpha = targetAlpha;

            _fadeGroup.blocksRaycasts = (targetAlpha >= 0.99f);

            yield break;
        }

        float t = 0f;

        while (t < duration)
        {
         
            float dt = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            t += dt; 
            float lerp = Mathf.Clamp01(t / duration);

            _fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, lerp);

            yield return null;

            _fadeGroup.blocksRaycasts = (targetAlpha >= 0.99f);
        }

        _fadeGroup.alpha = targetAlpha;

        _fadeGroup.blocksRaycasts = (targetAlpha >= 0.99f);


    }

}
