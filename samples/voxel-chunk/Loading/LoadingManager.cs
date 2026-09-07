using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class LoadingManager : MonoBehaviourPunCallbacks
{
    public Image loadImage;
    public Text loadText;

    private AsyncOperation asyncOperation;
    private float nowTime;
    private bool isLoad = false;
    private int ready = 0;

    private void Start()
    {
        nowTime = Time.time;
    }

    IEnumerator LoadScene()
    {
        bool isDone = false;

        if(!isDone)
        {
            isDone = true;

            PhotonNetwork.IsMessageQueueRunning = false;
            asyncOperation = SceneManager.LoadSceneAsync("Game");

            asyncOperation.allowSceneActivation = false;

            while(asyncOperation.progress < 0.9f)
            {
                if(loadImage.fillAmount < asyncOperation.progress + 0.1f)
                {
                    loadImage.fillAmount = asyncOperation.progress + 0.1f;
                    loadText.text = ((int)(loadImage.fillAmount * 100)).ToString() + "%";
                }
            }
        }

        loadImage.fillAmount = 1;
        loadText.text = "100%";
        photonView.RPC("Ready", RpcTarget.MasterClient, null);

        yield return 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - nowTime < 0.5f)
        {
            loadImage.fillAmount += Time.deltaTime;
            loadText.text = ((int)(loadImage.fillAmount * 100)).ToString() + "%";
        }

        if(!isLoad && Time.time - nowTime > 0.5f)
        {
            isLoad = true;
            StartCoroutine(LoadScene());
        }

        if(Time.time - nowTime > 1f)
        {
            asyncOperation.allowSceneActivation = true;
        }
    }

    [PunRPC]
    void Ready()
    {
        ready++;
    }
}
