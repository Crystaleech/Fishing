using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;  // 这里是摄像机的Transform
    public float mouseSensitivity = 100f;

    private float xRotation = 0f;  // 记录上下旋转的角度

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  // 锁定鼠标光标在屏幕中央
    }

    void Update()
    {
        // 获取鼠标的上下移动量
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 仅允许摄像机上下旋转
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -45f, 45f);  // 限制上下旋转范围为 -45 到 45 度

        // 设置摄像机的本地旋转，只能上下移动
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
