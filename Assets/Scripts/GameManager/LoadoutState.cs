using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
#if UNITY_ANALYTICS
using UnityEngine.Analytics;
#endif

/// <summary>
/// State pushed on the GameManager during the Loadout, when player select player, theme and accessories
/// Take care of init the UI, load all the data used for it etc.
/// </summary>
public class LoadoutState : AState
{
	public Button runButton;

	public MeshFilter skyMeshFilter;
	public MeshFilter UIGroundFilter;

    protected GameObject _character;

	protected Modifier _currentModifier = new Modifier();

    protected const float CHARACTER_ROTATE_SPEED = 45f;

    public override void Enter(AState from)
    {
        skyMeshFilter.gameObject.SetActive(true);
        UIGroundFilter.gameObject.SetActive(true);

        // Reseting the global blinking value. Can happen if the game unexpectedly exited while still blinking
        Shader.SetGlobalFloat("_BlinkingValue", 0.0f);

        runButton.interactable = false;
        runButton.GetComponentInChildren<Text>().text = "Loading...";
    }

    public override void Exit(AState to)
    {
        if (_character != null) Destroy(_character);

        GameState gs = to as GameState;

        skyMeshFilter.gameObject.SetActive(false);
        UIGroundFilter.gameObject.SetActive(false);

        if (gs != null)
        {
			gs.currentModifier = _currentModifier;
			
            // We reset the modifier to a default one, for next run (if a new modifier is applied, it will replace this default one before the run starts)
			_currentModifier = new Modifier();
        }
    }

    public override string GetName()
    {
        return "Loadout";
    }

	public override void Tick()
	{
		if (!runButton.interactable)
		{
			bool interactable = ThemeDatabase.loaded;
			if (interactable)
			{
				runButton.interactable = true;
				runButton.GetComponentInChildren<Text>().text = "Run!";
			}
		}

		if (_character != null)
		{
			_character.transform.Rotate(0, CHARACTER_ROTATE_SPEED * Time.deltaTime, 0, Space.Self);
		}
	}

	public void SetModifier(Modifier modifier)
	{
		_currentModifier = modifier;
	}

    public void StartGame()
    {
        if(PlayerData.instance.ftueLevel == 1)
        {
            PlayerData.instance.ftueLevel = 2;
        }

        manager.SwitchState("Game");

		runButton.gameObject.SetActive(false);
    }
}
