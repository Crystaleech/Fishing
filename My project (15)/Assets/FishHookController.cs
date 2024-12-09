using UnityEngine;

public class FishHookController : MonoBehaviour
{
    public Transform rodTip;             // 鱼竿顶端的位置
    public float castDistance = 5f;      // 抛竿的最大距离
    public float castSpeed = 3f;         // 抛竿速度
    public float reelSpeed = 2f;         // 收线速度

    private Vector3 initialPosition;     // 鱼钩的初始位置
    private bool isCasting = false;      // 是否正在抛竿
    private bool isReeling = false;      // 是否正在收线

    void Start()
    {
        // 保存鱼钩的初始位置
        initialPosition = transform.position;
    }

    void Update()
    {
        // 按下左键进行抛竿
        if (Input.GetMouseButtonDown(0) && !isCasting && !isReeling)
        {
            isCasting = true;
        }

        // 抛竿逻辑
        if (isCasting)
        {
            // 将鱼钩逐渐移动到抛竿的最大距离位置
            Vector3 targetPosition = rodTip.position + rodTip.forward * castDistance;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, castSpeed * Time.deltaTime);

            // 达到最大距离后开始收线
            if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
            {
                isCasting = false;
                isReeling = true;
            }
        }
        // 收线逻辑
        else if (isReeling)
        {
            // 将鱼钩逐渐返回到初始位置
            transform.position = Vector3.MoveTowards(transform.position, initialPosition, reelSpeed * Time.deltaTime);

            // 返回初始位置后停止收线
            if (Vector3.Distance(transform.position, initialPosition) <= 0.1f)
            {
                isReeling = false;
            }
        }
    }
}
