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
    [Header("Char UI")]
	public RectTransform charSelect;
	public Transform charPosition;

	[Header("PowerUp UI")]
	public RectTransform powerupSelect;
	public Image powerupIcon;
	public Text powerupCount;
    public Sprite noItemIcon;

	[Header("Accessory UI")]
    public RectTransform accessoriesSelector;
    public Text accesoryNameDisplay;
	public Image accessoryIconDisplay;

	[Header("Other Data")]
	public Button runButton;

	public MeshFilter skyMeshFilter;
	public MeshFilter UIGroundFilter;

    protected GameObject m_Character;
	protected int m_UsedPowerupIndex;
    protected bool m_IsLoadingCharacter;

	protected Modifier m_CurrentModifier = new Modifier();

    protected const float k_CharacterRotationSpeed = 45f;
    protected const string k_ShopSceneName = "shop";
    protected const float k_OwnedAccessoriesCharacterOffset = -0.1f;
    protected int k_UILayer;
    protected readonly Quaternion k_FlippedYAxisRotation = Quaternion.Euler (0f, 180f, 0f);

    public override void Enter(AState from)
    {
		 k_UILayer = LayerMask.NameToLayer("UI");

        skyMeshFilter.gameObject.SetActive(true);
        UIGroundFilter.gameObject.SetActive(true);

        // Reseting the global blinking value. Can happen if the game unexpectedly exited while still blinking
        Shader.SetGlobalFloat("_BlinkingValue", 0.0f);

        runButton.interactable = false;
        runButton.GetComponentInChildren<Text>().text = "Loading...";

        Refresh();
    }

    public override void Exit(AState to)
    {
        if (m_Character != null) Destroy(m_Character);

        GameState gs = to as GameState;

        skyMeshFilter.gameObject.SetActive(false);
        UIGroundFilter.gameObject.SetActive(false);

        if (gs != null)
        {
			gs.currentModifier = m_CurrentModifier;
			
            // We reset the modifier to a default one, for next run (if a new modifier is applied, it will replace this default one before the run starts)
			m_CurrentModifier = new Modifier();
        }
    }

    public void Refresh()
    {
        StartCoroutine(PopulateCharacters());
    }

	public override string GetName()
	{
		return "Loadout";
	}

    public void ChangeCharacter(int dir)
    {
        StartCoroutine(PopulateCharacters());
    }

    public IEnumerator PopulateCharacters()
    {
		if (!m_IsLoadingCharacter)
		{
			m_IsLoadingCharacter = true;
			GameObject newChar = null;
			while (newChar == null)
			{
				Character c = CharacterDatabase.GetCharacter();

				if (c != null)
				{
					Vector3 pos = charPosition.transform.position;
					pos.x = 0.0f;
					charPosition.transform.position = pos;

					newChar = Instantiate(c.gameObject);
					Helpers.SetRendererLayerRecursive(newChar, k_UILayer);
					newChar.transform.SetParent(charPosition, false);
					newChar.transform.rotation = k_FlippedYAxisRotation;

					if (m_Character != null)
						Destroy(m_Character);

					m_Character = newChar;

					m_Character.transform.localPosition = Vector3.right * 1000;
					//animator will take a frame to initialize, during which the character will be in a T-pose.
					//So we move the character off screen, wait that initialised frame, then move the character back in place.
					//That avoid an ugly "T-pose" flash time
					yield return new WaitForEndOfFrame();
					m_Character.transform.localPosition = Vector3.zero;
				}
				else
					yield return new WaitForSeconds(1.0f);
			}
			m_IsLoadingCharacter = false;
		}
	}

	public void SetModifier(Modifier modifier)
	{
		m_CurrentModifier = modifier;
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
