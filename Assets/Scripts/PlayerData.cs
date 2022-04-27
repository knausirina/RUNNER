using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Save data for the game. This is stored locally in this case, but a "better" way to do it would be to store it on a server
/// somewhere to avoid player tampering with it. Here potentially a player could modify the binary file to add premium currency.
/// </summary>
public class PlayerData
{
    static protected PlayerData m_Instance;
    static public PlayerData instance { get { return m_Instance; } }

    protected string saveFile = "";


    public int coins;
    public int premium;

    public int usedCharacter;                               // Currently equipped character.
    public int usedAccessory = -1;
    public List<string> characterAccessories = new List<string>();  // List of owned accessories, in the form "charName:accessoryName".
    public List<string> themes = new List<string>();                // Owned themes.
    public int usedTheme;                                           // Currently used theme.

	public string previousName = "Trash Cat";

    public bool licenceAccepted;

	public float masterVolume = float.MinValue, musicVolume = float.MinValue, masterSFXVolume = float.MinValue;

    //ftue = First Time User Expeerience. This var is used to track thing a player do for the first time. It increment everytime the user do one of the step
    //e.g. it will increment to 1 when they click Start, to 2 when doing the first run, 3 when running at least 300m etc.
    public int ftueLevel = 0;
    //Player win a rank ever 300m (e.g. a player having reached 1200m at least once will be rank 4)
    public int rank = 0;

    // This will allow us to add data even after production, and so keep all existing save STILL valid. See loading & saving for how it work.
    // Note in a real production it would probably reset that to 1 before release (as all dev save don't have to be compatible w/ final product)
    // Then would increment again with every subsequent patches. We kept it to its dev value here for teaching purpose. 
    static int s_Version = 11; 

    public void AddTheme(string theme)
    {
        themes.Add(theme);
    }

    public void AddAccessory(string name)
    {
        characterAccessories.Add(name);
    }

	static public void Create()
	{
		if (m_Instance == null)
		{
			m_Instance = new PlayerData();

			//if we create the PlayerData, mean it's the very first call, so we use that to init the database
			//this allow to always init the database at the earlier we can, i.e. the start screen if started normally on device
			//or the Loadout screen if testing in editor
		}

		if (File.Exists(m_Instance.saveFile))
		{
			// If we have a save, we read it.
			//m_Instance.Read();
		}
		else
		{
			// If not we create one with default data.
			NewSave();
		}
	}

	static public void NewSave()
	{
		m_Instance.themes.Clear();
		m_Instance.characterAccessories.Clear();

		m_Instance.usedCharacter = 0;
		m_Instance.usedTheme = 0;
		m_Instance.usedAccessory = -1;

        m_Instance.coins = 0;
        m_Instance.premium = 0;

		m_Instance.themes.Add("Day");

        m_Instance.ftueLevel = 0;
        m_Instance.rank = 0;
	}
}