using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

public class StartButton : MonoBehaviour
{
    public void StartGame()
	{ 
		PlayerData.Create();//my bad
		if (PlayerData.instance.ftueLevel == 0)
        {
            PlayerData.instance.ftueLevel = 1;
        }

#if UNITY_PURCHASING
        var module = StandardPurchasingModule.Instance();
#endif
        SceneManager.LoadScene("main");
    }
}
