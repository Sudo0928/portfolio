using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GenshinImpactMovementSystem
{
    [Serializable]
    public class PlayerCameraUtility
    {
        [field: SerializeField] public CinemachineVirtualCamera VirtualCamera { get; private set; }
        [field: SerializeField] public float DefaultHorizontalWaitTime { get; private set; } = 0f;
        [field: SerializeField] public float DefaultHorizontalRecenteringTime { get; private set; } = 4f;
        [SerializeField] private GameObject CameraLookPoint;

        private CinemachineVirtualCamera virtualCamera;
        private CinemachinePOV cinemachinePov;

        public void Initialize()
        {
            virtualCamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
            
            if (virtualCamera == null)
            {
                Debug.LogWarning("CinemachineVirtualCamera not found during PlayerCameraUtility initialization");
                return;
            }
            
            cinemachinePov = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
            
            if (cinemachinePov == null)
            {
                Debug.LogWarning("CinemachinePOV component not found on virtual camera");
                return;
            }

            if(CameraLookPoint != null)
            {
                virtualCamera.LookAt = CameraLookPoint.transform;
                virtualCamera.Follow = CameraLookPoint.transform;
            }
        }

        public void EnableRecentering(float waitTime = -1f, float recenteringTime = -1f, float baseMovementSpeed = 1f, float movementSpeed = 1f)
        {
            if (cinemachinePov == null)
            {
                Debug.LogWarning("CinemachinePOV is null in EnableRecentering - camera may not be initialized");
                return;
            }
            
            cinemachinePov.m_HorizontalRecentering.m_enabled = true;

            cinemachinePov.m_HorizontalRecentering.CancelRecentering();

            if(waitTime == -1f)
            {
                waitTime = DefaultHorizontalWaitTime;
            }

            if(recenteringTime == -1f)
            {
                recenteringTime = DefaultHorizontalRecenteringTime;
            }

            recenteringTime = recenteringTime * baseMovementSpeed / movementSpeed;

            cinemachinePov.m_HorizontalRecentering.m_WaitTime = waitTime;
            cinemachinePov.m_HorizontalRecentering.m_RecenteringTime = recenteringTime;
        }

        public void DisableRecentering()
        {
            if (cinemachinePov == null)
            {
                Debug.LogWarning("CinemachinePOV is null in DisableRecentering - camera may not be initialized");
                return;
            }
            
            cinemachinePov.m_HorizontalRecentering.m_enabled = false;
        }
    }
}
