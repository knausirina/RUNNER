using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Pushed on top of the GameManager during gameplay. Takes care of initializing all the UI and start the TrackManager
/// Also will take care of cleaning when leaving that state.
/// </summary>
public class GameState : AState
{
	static int s_DeadHash = Animator.StringToHash("Dead");

    public Canvas canvas;
    public TrackManager trackManager;

    [Header("UI")]
    public Text coinText;
    public Text premiumText;
    public Text scoreText;
	public Text distanceText;
    public Text multiplierText;
	public Text countdownText;
    public RectTransform powerupZone;
	public RectTransform lifeRectTransform;

	public RectTransform pauseMenu;
	public RectTransform wholeUI;
	public Button pauseButton;

    public Image inventoryIcon;

    public GameObject gameOverPopup;
    public Button premiumForLifeButton;
    public GameObject adsForLifeButton;
    public Text premiumCurrencyOwned;

    [Header("Prefabs")]
    public GameObject PowerupIconPrefab;

	public Modifier currentModifier = new Modifier();

    public string adsPlacementId = "rewardedVideo";
#if UNITY_ANALYTICS
    public AdvertisingNetwork adsNetwork = AdvertisingNetwork.UnityAds;
#endif
    public bool adsRewarded = true;

    protected bool m_Finished;
    protected float m_TimeSinceStart;
	protected Image[] m_LifeHearts;

    protected RectTransform m_CountdownRectTransform;
    protected bool m_WasMoving;

    protected bool m_AdsInitialised = false;
    protected bool m_GameoverSelectionDone = false;

    protected int k_MaxLives = 3;

    public override void Enter(AState from)
    {
        m_CountdownRectTransform = countdownText.GetComponent<RectTransform>();

        m_LifeHearts = new Image[k_MaxLives];
        for (int i = 0; i < k_MaxLives; ++i)
        {
            m_LifeHearts[i] = lifeRectTransform.GetChild(i).GetComponent<Image>();
        }

        m_AdsInitialised = false;
        m_GameoverSelectionDone = false;

        StartGame();
    }

    public override void Exit(AState to)
    {
        canvas.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        canvas.gameObject.SetActive(true);
        pauseMenu.gameObject.SetActive(false);
        wholeUI.gameObject.SetActive(true);
        pauseButton.gameObject.SetActive(true);
        gameOverPopup.SetActive(false);

        if (!trackManager.isRerun)
        {
            m_TimeSinceStart = 0;
            trackManager.characterController.currentLife = trackManager.characterController.maxLife;
        }

        currentModifier.OnRunStart(this);
        trackManager.Begin();

        m_Finished = false;
    }

	public override string GetName()
	{
		return "Game";
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus) Pause();
	}

    void OnApplicationFocus(bool focusStatus)
    {
        if (!focusStatus) Pause();
    }

    public void Pause()
	{
		//check if we aren't finished OR if we aren't already in pause (as that would mess states)
		if (m_Finished || AudioListener.pause == true)
			return;

		AudioListener.pause = true;
		Time.timeScale = 0;

		pauseButton.gameObject.SetActive(false);
        pauseMenu.gameObject.SetActive (true);
		wholeUI.gameObject.SetActive(false);
		m_WasMoving = trackManager.isMoving;
		trackManager.StopMove();
	}

	public void Resume()
	{
		Time.timeScale = 1.0f;
		pauseButton.gameObject.SetActive(true);
		pauseMenu.gameObject.SetActive (false);
		wholeUI.gameObject.SetActive(true);
		if (m_WasMoving)
		{
			trackManager.StartMove(false);
		}

		AudioListener.pause = false;
	}

	public void QuitToLoadout()
	{
		// Used by the pause menu to return immediately to loadout, canceling everything.
		Time.timeScale = 1.0f;
		AudioListener.pause = false;
		trackManager.End();
		trackManager.isRerun = false;
		manager.SwitchState ("Loadout");
	}

    protected void UpdateUI()
    {
        coinText.text = trackManager.characterController.coins.ToString();
        premiumText.text = trackManager.characterController.premium.ToString();

		for (int i = 0; i < 3; ++i)
		{

			if(trackManager.characterController.currentLife > i)
			{
				m_LifeHearts[i].color = Color.white;
			}
			else
			{
				m_LifeHearts[i].color = Color.black;
			}
		}

        scoreText.text = trackManager.score.ToString();
        multiplierText.text = "x " + trackManager.multiplier;

		distanceText.text = Mathf.FloorToInt(trackManager.worldDistance).ToString() + "m";

		if (trackManager.timeToStart >= 0)
		{
			countdownText.gameObject.SetActive(true);
			countdownText.text = Mathf.Ceil(trackManager.timeToStart).ToString();
			m_CountdownRectTransform.localScale = Vector3.one * (1.0f - (trackManager.timeToStart - Mathf.Floor(trackManager.timeToStart)));
		}
		else
		{
			m_CountdownRectTransform.localScale = Vector3.zero;
		}
    }

	IEnumerator WaitForGameOver()
	{
		m_Finished = true;
		trackManager.StopMove();

		// Reseting the global blinking value. Can happen if game unexpectly exited while still blinking
		Shader.SetGlobalFloat("_BlinkingValue", 0.0f);

		yield return new WaitForSeconds(2.0f);
		if (currentModifier.OnRunEnd(this))
		{
			if (trackManager.isRerun)
				manager.SwitchState("GameOver");
			else
				OpenGameOverPopup();
		}
	}

    public void OpenGameOverPopup()
    {
        premiumForLifeButton.interactable = PlayerData.instance.premium >= 3;

		premiumCurrencyOwned.text = PlayerData.instance.premium.ToString();

        gameOverPopup.SetActive(true);
    }

    public void GameOver()
    {
        manager.SwitchState("GameOver");
    }

    public void PremiumForLife()
    {
        //This check avoid a bug where the video AND premium button are released on the same frame.
        //It lead to the ads playing and then crashing the game as it try to start the second wind again.
        //Whichever of those function run first will take precedence
        if (m_GameoverSelectionDone)
            return;

        m_GameoverSelectionDone = true;

        PlayerData.instance.premium -= 3;
        //since premium are directly added to the PlayerData premium count, we also need to remove them from the current run premium count
        // (as if you had 0, grabbed 3 during that run, you can directly buy a new chance). But for the case where you add one in the playerdata
        // and grabbed 2 during that run, we don't want to remove 3, otherwise will have -1 premium for that run!
        trackManager.characterController.premium -= Mathf.Min(trackManager.characterController.premium, 3);

        SecondWind();
    }

    public void SecondWind()
    {
        trackManager.characterController.currentLife = 1;
        trackManager.isRerun = true;
        StartGame();
    }
}
