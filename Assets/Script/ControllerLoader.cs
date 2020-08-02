using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerLoader : MonoBehaviour
{
	public GameObject database;
	public GameObject gameRule;
	public GameObject tileController;
	public GameObject Player;

	void Awake()
	{
		if (GameRule.Instance == null)
		{
			Instantiate(gameRule);
		}
		if (Database.Instance == null)
		{
			Instantiate(database);
		}
		if (TileController.Instance == null)
		{
			Instantiate(tileController);
		}
			
		Instantiate(Player);
	}
}