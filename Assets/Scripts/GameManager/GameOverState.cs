using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif
using System.Collections.Generic;
 
/// <summary>
/// state pushed on top of the GameManager when the player dies.
/// </summary>
public class GameOverState : AState
{
    public TrackManager trackManager;
    public Canvas canvas;
    public MissionUI missionPopup;

    public GameObject addButton;

    public override void Enter(AState from)
    {
        canvas.gameObject.SetActive(true);

		if (PlayerData.instance.AnyMissionComplete())
            missionPopup.Open();
        else
            missionPopup.gameObject.SetActive(false);
    }

	public override void Exit(AState to)
    {
        canvas.gameObject.SetActive(false);
        FinishRun();
    }

    public override string GetName()
    {
        return "GameOver";
    }

    public void GoToLoadout()
    {
        trackManager.isRerun = false;
		manager.SwitchState("Loadout");
    }

    public void RunAgain()
    {
        trackManager.isRerun = false;
        manager.SwitchState("Game");
    }

	protected void FinishRun()
    {
        trackManager.End();
    }

	public override void Tick()
	{
		
	}
}