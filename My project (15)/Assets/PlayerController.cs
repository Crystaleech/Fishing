using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;  // �������������Transform
    public float mouseSensitivity = 100f;

    private float xRotation = 0f;  // ��¼������ת�ĽǶ�

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  // �������������Ļ����
    }

    void Update()
    {
        // ��ȡ���������ƶ���
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // �����������������ת
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -45f, 45f);  // ����������ת��ΧΪ -45 �� 45 ��

        // ����������ı�����ת��ֻ�������ƶ�
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
