using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour 
{
	public static MainMenu instance;

	public GameObject[] menuGroups = new GameObject[5];
	public MainMenuState currentState = MainMenuState.MAIN;
	public RectTransform hoverBox;
	public GameObject resumeButton, loadButton;
	public Slider musicSlider, ingameSlider;

	public void RequestMain() { RequestState(MainMenuState.MAIN); }
	public void RequestPlay() 
	{ 
		RequestState(MainMenuState.PLAY); 
		if (Core.GetNumSaveGames() == 0)
		{
			resumeButton.SetActive(false);
			loadButton.SetActive(false);		
		}
	}

	public void RequestOptions() 
	{ 
		musicSlider.value = Core.theCore.config.fMusicVolume;
		ingameSlider.value = Core.theCore.config.fInGameVolume;
		RequestState(MainMenuState.OPTIONS); 
	}

	public void RequestCreateGame() { RequestState(MainMenuState.CREATE_GAME); }
	public void RequestCredits() { RequestState(MainMenuState.CREDITS); }
	public void RequestLoadGame() { RequestState(MainMenuState.LOAD_GAME); PopulateLoadMenu(); }
	public void PlayClickSound() { Core.GetAudioManager().PlayGUIClick(); }

	public void RequestState(MainMenuState state)
	{
		currentState = state;
		foreach (GameObject g in menuGroups)
		{
			g.SetActive(false);
		}

		menuGroups [(int)state].SetActive(true);
	}
		
	void Start () 
	{
		if (instance == null)
			instance = this;
		else
			Debug.Assert(false, "Two main menus?");

		RequestMain();
	}

	void OnGUI()
	{
		if (Event.current != null)
		{
			Vector3 v = GUIUtility.ScreenToGUIPoint(Event.current.mousePosition);
			hoverBox.position = new Vector3 (v.x, 1080.0f - v.y, v.z);

			if (Event.current.keyCode == KeyCode.Escape)
			{
				Quit();
			}
		}
	}

	public void Quit()
	{
		Core.theCore.RequestState(Core.State.EXIT_GAME);
	}

	// PLAY MENU
	public void ResumeGame()
	{
		Core.ResumeLastSavedGame();
	}

	// CREATE GAME MENU ----------------------------

	public string newSaveName = "";

	public void SetNewSaveName(string saveName)
	{
		newSaveName = saveName;
	}

	public void ConfirmCreate()
	{
		if (newSaveName.Length == 0)
		{
			Core.GetAudioManager().PlayGUIReject();
			return;
		}

		char[] chars = newSaveName.ToCharArray();
		for (int i = 0; i < newSaveName.Length; i++)
		{
			if (!char.IsLetterOrDigit(chars [i]))
			{
				Core.GetAudioManager().PlayGUIReject();
				return;
			}
		}

		Core.CreateSaveGame(newSaveName);
	}


	// LOAD GAME MENU ----------------------------

	public RectTransform loadMenuRoot;
	public Text pageNumber, previous, next;
	public LoadGameEntry loadGameEntryPrefab;
	public List<LoadGameEntry> entries = new List<LoadGameEntry>();
	public int scroll = 0;
	public int maxScroll = 0;

	public void PopulateLoadMenu()
	{
		maxScroll = (Core.theCore.savedGames.Count - 1) / 5;

		for (int i = 0; i < Core.theCore.savedGames.Count; i++)
		{
			PlayerProfile profile = Core.theCore.savedGames [i];
			LoadGameEntry entry = Instantiate<LoadGameEntry>(loadGameEntryPrefab);
			entry.saveName.text = profile.name;
			entry.index = i;
			int iRow = i % 5;
			int iCol = i / 5;

			entry.transform.SetParent(loadMenuRoot.transform);
			entry.transform.localPosition = new Vector3 (660.0f + iCol * 1920.0f, 410.0f - iRow * 120.0f, 0.0f);

			entries.Add(entry);
		}

		SetScroll(0);
	}

	public void ConfirmLoadSlot(int index)
	{
		Core.LoadSavedGame(index);
	}

	public void IncrementScroll()
	{
		if (scroll < maxScroll)
			SetScroll(scroll + 1);
	}

	public void DecrementScroll()
	{
		if (scroll > 0)
			SetScroll(scroll - 1);
	}
				
	private void SetScroll(int index)
	{
		scroll = index;
		foreach (LoadGameEntry entry in entries)
		{
			entry.SetScroll(index);
		}	
			
		pageNumber.text = (scroll + 1) + "/" + (maxScroll + 1);
		pageNumber.enabled = maxScroll != 0;
		next.enabled = scroll < maxScroll;
		previous.enabled = scroll > 0;
	}

	// Options menu
	public void SetMusicVolume(float fSet)
	{
		Core.theCore.config.fMusicVolume = fSet;
		Core.theCore.ApplyConfig();
	}

	public void SetGameVolume(float fSet)
	{
		Core.theCore.config.fInGameVolume = fSet;
		Core.theCore.ApplyConfig();
	}
}
