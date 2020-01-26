using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamRosterPreview : MonoBehaviour 
{
	public Transform[] playerSpawnPoints = new Transform[(int)MinionSlot.NUM_MINION_SLOTS];
	public int rosterIndex;
	private TeamRoster roster;
	private List<RenderActor> renderers = new List<RenderActor>();
	public Camera cam;

	void Start () 
	{
	}

	void Update () 
	{
		if (Core.GetPlayerProfile() == null)
			return;
		
		roster = Core.GetPlayerProfile().GetRoster(rosterIndex);

		if (roster != null && (roster.bDirty || renderers.Count == 0))
		{
			UpdatePreview();
			roster.bDirty = false;
		}

		foreach (RenderActor renderActor in renderers)
		{
			renderActor.UpdateAnimation(Time.deltaTime);
		}
	}

	public void UpdatePreview()
	{
		foreach (RenderActor ra in renderers)
		{
			Destroy(ra.gameObject);
		}
		renderers.Clear();

		for (int i = 0; i < (int)MinionSlot.NUM_MINION_SLOTS; i++)
		{
			RenderActor renderActor = Instantiate<RenderActor>(roster.minions [i].template.render);
			renderActor.transform.SetParent(playerSpawnPoints [i]);
			renderActor.transform.localPosition = Vector3.zero;
			renderActor.transform.localScale = new Vector3 (-10.0f, 10.0f, 10.0f);
			renderActor.SetAnimState(AnimState.IDLE, true);
			renderers.Add(renderActor);
		}
	}
}
