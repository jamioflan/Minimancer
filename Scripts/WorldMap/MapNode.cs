using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapNode : MonoBehaviour 
{
	public enum NodeState
	{
		INACCESSIBLE,
		ACCESSIBLE,
		PARTIALLY_COMPLETED,
		COMPLETED,

		NUM_NODE_STATES
	}

	public enum HoverState
	{
		NOT_HOVER,
		HOVER,
		CLICK,

		NUM_HOVER_STATES
	}

	// UNITY, y u no serialize 2D arrays?
	public Sprite[] iconsInaccessible, iconsAccessible, iconsCompleted, iconsPartial = new Sprite[(int)HoverState.NUM_HOVER_STATES];
	public Sprite[] completionSprites = new Sprite[4];
	public Image image;
	public Button button;
	public RectTransform popup;
	public Text popupText, descText;
	public Button popupButton;
	public Location location;
	public string debugText;
	public Image completionInfo;
	public NodeState nodeState = NodeState.NUM_NODE_STATES;
	public HoverState hoverState = HoverState.NUM_HOVER_STATES;
	public float fTimeSinceClick = 0.0f;

	private Sprite GetIcon(NodeState nodeState, HoverState hoverState)
	{
		switch (nodeState)
		{
			case NodeState.PARTIALLY_COMPLETED:
				return iconsPartial [(int)hoverState];
			case NodeState.INACCESSIBLE:
				return iconsInaccessible [(int)hoverState];
			case NodeState.ACCESSIBLE:
				return iconsAccessible [(int)hoverState];
			case NodeState.COMPLETED:
				return iconsCompleted [(int)hoverState];
		}
		return null;
	}

	void Start () 
	{
		
	}

	void Update () 
	{
		fTimeSinceClick += Time.deltaTime;
	}

	public static void CloseAllMapNodes()
	{
		WorldMapSlider.instance.popup.SetInactive ();
		//foreach (MapNode node in FindObjectsOfType<MapNode>())
		//{
		//	node.popup.gameObject.SetActive(false);
		//}
	}

	public void ClickNode()
	{
		Core.GetAudioManager().PlayGUIClick();

		// Double click functionality
		if (WorldMapSlider.instance.popup.location == location)
		{
			if (fTimeSinceClick <= 0.5f && Core.GetWorldMap().GetState(location) != NodeState.INACCESSIBLE)
			{
				if (Core.IsDemo() && location.element != Element.PHYSICAL)
				{
					Core.GetAudioManager().PlayGUIReject();
					return;
				}

				Core.GetWorldMap().Select(location);
				Core.LaunchSelectedLevel();
			}
			else
			{
				CloseAllMapNodes();
			}
		}
		else
		{
			CloseAllMapNodes();
			WorldMapSlider.instance.popup.SetActive (this);
			Core.GetWorldMap().Select(location);
		}

		fTimeSinceClick = 0.0f;
	}

	public void Launch()
	{
		if (Core.IsDemo() && location.element != Element.PHYSICAL)
		{
			Core.GetAudioManager().PlayGUIReject();
			return;
		}

		Core.GetAudioManager().PlayGUIClick();
		Core.LaunchSelectedLevel();
	}

	public void Refresh()
	{
		NodeState newState = Core.GetWorldMap().GetState(location);
		HoverState newHover = HoverState.NOT_HOVER; // TODO

		if (Core.IsDemo() && location.element != Element.PHYSICAL)
		{
			image.color = Color.grey;
			if(completionInfo != null)
				completionInfo.color = Color.grey;
			newState = NodeState.INACCESSIBLE;
		}

		if (nodeState != newState || hoverState != newHover)
		{
			nodeState = newState;
			hoverState = newHover;
			image.sprite = GetIcon(nodeState, hoverState);
			SpriteState spriteState = button.spriteState;
				
			spriteState.highlightedSprite = GetIcon(nodeState, HoverState.HOVER);
			spriteState.pressedSprite = GetIcon(nodeState, HoverState.CLICK);
			spriteState.disabledSprite = GetIcon(nodeState, HoverState.NOT_HOVER);

			button.spriteState = spriteState;
		}

		int iCompletion = Core.GetWorldMap().GetHighestCompletedWave(location) + 1;
		if(completionInfo != null)
			completionInfo.sprite = completionSprites[iCompletion];
	}

	public static void RefreshAllNodes()
	{
		foreach (MapNode node in FindObjectsOfType<MapNode>())
		{
			node.Refresh();
		}
	}
}
