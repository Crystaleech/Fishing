using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FishingRodController : MonoBehaviour
{
    public float raiseSpeed = 30f;
    public float lowerSpeed = 60f;
    public float maxRodLiftAngle = 30f; // ̧�����Ƕ�Ϊ30��
    public float releaseBoostAngle = 5f;
    public float boostDuration = 0.1f;
    public float arcSlideDuration = 0.3f;
    public float backwardAngle = 10f;
    public float forwardStretchAngle = 15f;
    public static FishingRodController Instance { get; private set; }

    public float CurrentAngle { get; private set; } // �������ڻ�ȡ��ǰ̧�˽Ƕ�

    public GameObject progressBarContainer; // ����������
    public Image fillImage; // ��������䲿��

    private Quaternion initialRodRotation;
    private float currentAngle = 0f;
    private bool isRaising = false;
    private bool isLowering = false;
    private bool isBoosting = false;
    private bool isMousePreviouslyHeld = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // ȷ��ֻ��һ��ʵ��
        }
    }

    void Start()
    {
        initialRodRotation = transform.localRotation;

        // ��ʼ�������������Ŀɼ���
        if (progressBarContainer != null)
        {
            progressBarContainer.SetActive(false); // ��ʼ״̬������
        }
    }

    void Update()
    {
        if (isBoosting) return;

        if (Input.GetMouseButton(0))
        {
            isMousePreviouslyHeld = true;

            // ��ʾ����������
            if (!progressBarContainer.activeSelf)
            {
                progressBarContainer.SetActive(true);
            }

            if (!isRaising && !isLowering)
            {
                isRaising = true;
                currentAngle = 0f; // ���ýǶ�
            }

            if (isRaising)
            {
                currentAngle += raiseSpeed * Time.deltaTime;
                currentAngle = Mathf.Clamp(currentAngle, 0f, maxRodLiftAngle);
                transform.localRotation = initialRodRotation * Quaternion.Euler(-currentAngle, 0, 0);

                // ���µ�ǰ�Ƕ�
                CurrentAngle = currentAngle;

                // ���½��������
                if (fillImage != null)
                {
                    fillImage.fillAmount = currentAngle / maxRodLiftAngle;
                }

                if (currentAngle >= maxRodLiftAngle)
                {
                    isRaising = false;
                    isLowering = true;
                }
            }
            else if (isLowering)
            {
                currentAngle -= lowerSpeed * Time.deltaTime;
                currentAngle = Mathf.Clamp(currentAngle, 0f, maxRodLiftAngle);
                transform.localRotation = initialRodRotation * Quaternion.Euler(-currentAngle, 0, 0);

                // ���µ�ǰ�Ƕ�
                CurrentAngle = currentAngle;

                // ���½��������
                if (fillImage != null)
                {
                    fillImage.fillAmount = currentAngle / maxRodLiftAngle;
                }

                if (currentAngle <= 0f)
                {
                    isLowering = false;
                    isRaising = true;
                }
            }
        }
        else if (isMousePreviouslyHeld)
        {
            // ���ؽ���������
            if (progressBarContainer != null)
            {
                progressBarContainer.SetActive(false);
            }

            if (!isBoosting)
            {
                StartCoroutine(ReleaseBoost());
                isMousePreviouslyHeld = false;
            }
        }

        // �����ǰ�ǶȽ��е���
        Debug.Log($"Current Rod Angle: {CurrentAngle}");
    }

    private IEnumerator ReleaseBoost()
    {
        isBoosting = true;

        float elapsedTime = 0f;
        while (elapsedTime < boostDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / boostDuration;

            float upAngle = Mathf.Lerp(currentAngle, currentAngle + releaseBoostAngle, t);
            float backTilt = Mathf.Lerp(0, -backwardAngle, t);
            transform.localRotation = initialRodRotation * Quaternion.Euler(-upAngle, 0, 0) * Quaternion.Euler(0, backTilt, 0);

            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < arcSlideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / arcSlideDuration;

            float arcAngle = Mathf.Lerp(currentAngle + releaseBoostAngle, -releaseBoostAngle * 2, t);
            float forwardStretch = Mathf.Lerp(-backwardAngle, forwardStretchAngle, t);
            transform.localRotation = initialRodRotation * Quaternion.Euler(-arcAngle, 0, 0) * Quaternion.Euler(0, forwardStretch, 0);

            yield return null;
        }

        transform.localRotation = initialRodRotation;
        currentAngle = 0f;
        isBoosting = false;
    }
}
