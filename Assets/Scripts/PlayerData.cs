using UnityEngine;
using System.IO;
using System.Collections.Generic;
#if UNITY_ANALYTICS
using UnityEngine.Analytics.Experimental;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

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
    public Dictionary<Consumable.ConsumableType, int> consumables = new Dictionary<Consumable.ConsumableType, int>();   // Inventory of owned consumables and quantity.

    public int usedCharacter;                               // Currently equipped character.
    public int usedAccessory = -1;
    public List<string> characterAccessories = new List<string>();  // List of owned accessories, in the form "charName:accessoryName".
    public List<string> themes = new List<string>();                // Owned themes.
    public int usedTheme;                                           // Currently used theme.
    public List<MissionBase> missions = new List<MissionBase>();

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

    public void Consume(Consumable.ConsumableType type)
    {
        if (!consumables.ContainsKey(type))
            return;

        consumables[type] -= 1;
        if(consumables[type] == 0)
        {
            consumables.Remove(type);
        }
    }

    public void Add(Consumable.ConsumableType type)
    {
        if (!consumables.ContainsKey(type))
        {
            consumables[type] = 0;
        }

        consumables[type] += 1;
    }


    public void AddTheme(string theme)
    {
        themes.Add(theme);
    }

    public void AddAccessory(string name)
    {
        characterAccessories.Add(name);
    }

    // Mission management

    // Will add missions until we reach 2 missions.
    public void CheckMissionsCount()
    {
        while (missions.Count < 2)
            AddMission();
    }

    public void AddMission()
    {
        int val = Random.Range(0, (int)MissionBase.MissionType.MAX);
        
        MissionBase newMission = MissionBase.GetNewMissionFromType((MissionBase.MissionType)val);
        newMission.Created();

        missions.Add(newMission);
    }

    public void StartRunMissions(TrackManager manager)
    {
        for(int i = 0; i < missions.Count; ++i)
        {
            missions[i].RunStart(manager);
        }
    }

    public void UpdateMissions(TrackManager manager)
    {
        for(int i = 0; i < missions.Count; ++i)
        {
            missions[i].Update(manager);
        }
    }

    public bool AnyMissionComplete()
    {
        for (int i = 0; i < missions.Count; ++i)
        {
            if (missions[i].isComplete) return true;
        }

        return false;
    }

    public void ClaimMission(MissionBase mission)
    {        
        premium += mission.reward;
        
#if UNITY_ANALYTICS // Using Analytics Standard Events v0.3.0
        AnalyticsEvent.ItemAcquired(
            AcquisitionType.Premium, // Currency type
            "mission",               // Context
            mission.reward,          // Amount
            "anchovies",             // Item ID
            premium,                 // Item balance
            "consumable",            // Item type
            rank.ToString()          // Level
        );
#endif
        
        missions.Remove(mission);

        CheckMissionsCount();
    }

	/*
	public void Read()
	{
		
		BinaryReader r = new BinaryReader(new FileStream(saveFile, FileMode.Open));

		int ver = r.ReadInt32();

		if (ver < 6)
		{
			r.Close();

			NewSave();
			r = new BinaryReader(new FileStream(saveFile, FileMode.Open));
			ver = r.ReadInt32();
		}

		coins = r.ReadInt32();

		consumables.Clear();
		int consumableCount = r.ReadInt32();
		for (int i = 0; i < consumableCount; ++i)
		{
			consumables.Add((Consumable.ConsumableType)r.ReadInt32(), r.ReadInt32());
		}

		// Read character.
		characters.Clear();
		int charCount = r.ReadInt32();
		for (int i = 0; i < charCount; ++i)
		{
			string charName = r.ReadString();

			if (charName.Contains("Raccoon") && ver < 11)
			{//in 11 version, we renamed Raccoon (fixing spelling) so we need to patch the save to give the character if player had it already
				charName = charName.Replace("Racoon", "Raccoon");
			}

			characters.Add(charName);
		}

		usedCharacter = r.ReadInt32();

		// Read character accesories.
		characterAccessories.Clear();
		int accCount = r.ReadInt32();
		for (int i = 0; i < accCount; ++i)
		{
			characterAccessories.Add(r.ReadString());
		}

		// Read Themes.
		themes.Clear();
		int themeCount = r.ReadInt32();
		for (int i = 0; i < themeCount; ++i)
		{
			themes.Add(r.ReadString());
		}

		usedTheme = r.ReadInt32();

		// Save contains the version they were written with. If data are added bump the version & test for that version before loading that data.
		if (ver >= 2)
		{
			premium = r.ReadInt32();
		}

		// Added missions.
		if (ver >= 4)
		{
			missions.Clear();

			int count = r.ReadInt32();
			for (int i = 0; i < count; ++i)
			{
				MissionBase.MissionType type = (MissionBase.MissionType)r.ReadInt32();
				MissionBase tempMission = MissionBase.GetNewMissionFromType(type);

				tempMission.Deserialize(r);

				if (tempMission != null)
				{
					missions.Add(tempMission);
				}
			}
		}

		// Added highscore previous name used.
		if (ver >= 7)
		{
			previousName = r.ReadString();
		}

		if (ver >= 8)
		{
			licenceAccepted = r.ReadBoolean();
		}

		if (ver >= 9)
		{
			masterVolume = r.ReadSingle();
			musicVolume = r.ReadSingle();
			masterSFXVolume = r.ReadSingle();
		}

		if (ver >= 10)
		{
			ftueLevel = r.ReadInt32();
			rank = r.ReadInt32();
		}

		r.Close();
	}
	*/

	static public void Create()
	{
		if (m_Instance == null)
		{
			m_Instance = new PlayerData();

			//if we create the PlayerData, mean it's the very first call, so we use that to init the database
			//this allow to always init the database at the earlier we can, i.e. the start screen if started normally on device
			//or the Loadout screen if testing in editor
			AssetBundlesDatabaseHandler.Load();
		}

		m_Instance.CheckMissionsCount();


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
		m_Instance.missions.Clear();
		m_Instance.characterAccessories.Clear();
		m_Instance.consumables.Clear();

		m_Instance.usedCharacter = 0;
		m_Instance.usedTheme = 0;
		m_Instance.usedAccessory = -1;

        m_Instance.coins = 0;
        m_Instance.premium = 0;

		m_Instance.themes.Add("Day");

        m_Instance.ftueLevel = 0;
        m_Instance.rank = 0;

        m_Instance.CheckMissionsCount();
	}
}