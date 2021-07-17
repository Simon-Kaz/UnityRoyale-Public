using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Playables;

namespace UnityRoyale
{
	public class CinematicsManager : MonoBehaviour
	{
		public PlayableDirector redCastleCollapse, blueCastleCollapse;

		public void PlayCollapseCutscene(Faction f)
		{
			if(f == Faction.Player)
			{
				redCastleCollapse.Play();
			}
			else
			{
				blueCastleCollapse.Play();
			}
		}
	}
}