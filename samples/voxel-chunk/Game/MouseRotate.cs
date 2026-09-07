using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MouseRotate
{
    public float XSensitivity = 2f;     // x 민감도
    public float YSensitivity = 2f;     // y 민감도
    public bool clampVR = true;         // Vertical Rotation Limited
    public float MinimumX = -70f;       // x 최소 앵글 각도
    public float MaximumX = 70f;        // x 최대 앵글 각도
    public bool smooth = true;          // 부드러움 설정 여부
    public float smoothTime = 5f;       // 부드러움 ( *Time.deltaTime)

    private Quaternion CharacterTargetRotate;
    private Quaternion CameraTargetRotate;

    public void Init(Transform character, Transform camera)
    {
        CharacterTargetRotate = character.localRotation;
        CameraTargetRotate = camera.localRotation;
    }

    public void LookRotation(Transform character, Transform camera)
    {
        float yRotate = Input.GetAxis("Mouse X") * XSensitivity;
        float xRotate = Input.GetAxis("Mouse Y") * YSensitivity;

        CharacterTargetRotate *= Quaternion.Euler(0f, yRotate, 0f);
        CameraTargetRotate *= Quaternion.Euler(-xRotate, 0f, 0f);

        if (clampVR)
            CameraTargetRotate = ClampRotationX(CameraTargetRotate);

        if (smooth)
        {
            character.localRotation = Quaternion.Slerp(character.localRotation, CharacterTargetRotate,
                smoothTime * Time.deltaTime);
            camera.localRotation = Quaternion.Slerp(camera.localRotation, CameraTargetRotate,
                smoothTime * Time.deltaTime);
        }
        else
        {
            character.localRotation = CharacterTargetRotate;
            camera.localRotation = CameraTargetRotate;
        }
    }

    private Quaternion ClampRotationX(Quaternion quat)
    {
        quat.x /= quat.w;
        quat.y /= quat.w;
        quat.z /= quat.w;
        quat.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(quat.x);
        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);
        quat.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return quat;
    }
}