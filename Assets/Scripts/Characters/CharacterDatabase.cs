using UnityEngine;
using AssetBundles;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This allows us to store a database of all characters currently in the bundles, indexed by name.
/// </summary>
public class CharacterDatabase
{
    static protected Character _character;

    static protected bool m_Loaded = false;
    static public bool loaded { get { return m_Loaded; } }

    static public Character GetCharacter()
    {
		return _character;
	}

	static public IEnumerator LoadDatabase(string packages)
	{
		packages = "characters/cat";
		if (_character == null)
		{
			AssetBundleLoadAssetOperation op = AssetBundleManager.LoadAssetAsync(packages, "character", typeof(GameObject));
			yield return CoroutineHandler.StartStaticCoroutine(op);

			Character c = op.GetAsset<GameObject>().GetComponent<Character>();
			if (c != null)
			{
				_character =  c;
			}
		}
		m_Loaded = true;
	}
}