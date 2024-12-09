using UnityEngine;

public class FishHookController : MonoBehaviour
{
    public Transform rodTip;             // ��Ͷ��˵�λ��
    public float castDistance = 5f;      // �׸͵�������
    public float castSpeed = 3f;         // �׸��ٶ�
    public float reelSpeed = 2f;         // �����ٶ�

    private Vector3 initialPosition;     // �㹳�ĳ�ʼλ��
    private bool isCasting = false;      // �Ƿ������׸�
    private bool isReeling = false;      // �Ƿ���������

    void Start()
    {
        // �����㹳�ĳ�ʼλ��
        initialPosition = transform.position;
    }

    void Update()
    {
        // ������������׸�
        if (Input.GetMouseButtonDown(0) && !isCasting && !isReeling)
        {
            isCasting = true;
        }

        // �׸��߼�
        if (isCasting)
        {
            // ���㹳���ƶ����׸͵�������λ��
            Vector3 targetPosition = rodTip.position + rodTip.forward * castDistance;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, castSpeed * Time.deltaTime);

            // �ﵽ�������ʼ����
            if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
            {
                isCasting = false;
                isReeling = true;
            }
        }
        // �����߼�
        else if (isReeling)
        {
            // ���㹳�𽥷��ص���ʼλ��
            transform.position = Vector3.MoveTowards(transform.position, initialPosition, reelSpeed * Time.deltaTime);

            // ���س�ʼλ�ú�ֹͣ����
            if (Vector3.Distance(transform.position, initialPosition) <= 0.1f)
            {
                isReeling = false;
            }
        }
    }
}
