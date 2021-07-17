using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.AddressableAssets;

namespace UnityRoyale
{
    public class GameManager : MonoBehaviour
    {
		[Header("Settings")]
		public bool autoStart = false;

		[Header("Public References")]
        public NavMeshSurface navMesh;
		public GameObject playersCastle, opponentCastle;
		public GameObject introTimeline;
        public PlaceableData castlePData;
		public ParticlePool appearEffectPool;

        private CardManager cardManager;
        private CPUOpponent CPUOpponent;
        private InputManager inputManager;
		private AudioManager audioManager;
		private UIManager UIManager;
		private CinematicsManager cinematicsManager;

        private List<ThinkingPlaceable> playerUnits, opponentUnits;
        private List<ThinkingPlaceable> playerBuildings, opponentBuildings;
        private List<ThinkingPlaceable> allPlayers, allOpponents; //contains both Buildings and Units
		private List<ThinkingPlaceable> allThinkingPlaceables;
		private List<Projectile> allProjectiles;
        private bool gameOver = false;
        private bool updateAllPlaceables = false; //used to force an update of all AIBrains in the Update loop
        private const float THINKING_DELAY = 2f;

        private void Awake()
        {
            cardManager = GetComponent<CardManager>();
            CPUOpponent = GetComponent<CPUOpponent>();
            inputManager = GetComponent<InputManager>();
			//audioManager = GetComponentInChildren<AudioManager>();
			cinematicsManager = GetComponentInChildren<CinematicsManager>();
			UIManager = GetComponent<UIManager>();

			if(autoStart)
				introTimeline.SetActive(false);

            //listeners on other managers
            cardManager.OnCardUsed += UseCard;
            CPUOpponent.OnCardUsed += UseCard;

            //initialise Placeable lists, for the AIs to pick up and find a target
            playerUnits = new List<ThinkingPlaceable>();
            playerBuildings = new List<ThinkingPlaceable>();
            opponentUnits = new List<ThinkingPlaceable>();
            opponentBuildings = new List<ThinkingPlaceable>();
            allPlayers = new List<ThinkingPlaceable>();
            allOpponents = new List<ThinkingPlaceable>();
			allThinkingPlaceables = new List<ThinkingPlaceable>();
			allProjectiles = new List<Projectile>();
        }

        private void Start()
        {
			//Insert castles into lists
			SetupPlaceable(playersCastle, castlePData, Faction.Player);
            SetupPlaceable(opponentCastle, castlePData, Faction.Opponent);

			cardManager.LoadDeck();
            CPUOpponent.LoadDeck();

			//audioManager.GoToDefaultSnapshot();

			if(autoStart)
				StartMatch();
        }

		//called by the intro cutscene
		public void StartMatch()
		{
			CPUOpponent.StartActing();
		}

        //the Update loop pings all the ThinkingPlaceables in the scene, and makes them act
        private void Update()
        {
            if(gameOver)
                return;

            ThinkingPlaceable targetToPass; //ref

            foreach (var thinkingPlaceable in allThinkingPlaceables)
			{
				if(updateAllPlaceables)
					thinkingPlaceable.state = ThinkingPlaceable.States.Idle; //forces the assignment of a target in the switch below

				switch(thinkingPlaceable.state)
				{
					case ThinkingPlaceable.States.Idle:
						//this if is for innocuous testing Units
						if(thinkingPlaceable.targetType == PlaceableTarget.None)
							break;

						//find closest target and assign it to the ThinkingPlaceable
						bool targetFound = FindClosestInList(thinkingPlaceable.transform.position, GetAttackList(thinkingPlaceable.faction, thinkingPlaceable.targetType), out targetToPass);
						if(!targetFound) Debug.LogError("No more targets!"); //this should only happen on Game Over
						thinkingPlaceable.SetTarget(targetToPass);
						thinkingPlaceable.Seek();
						break;


					case ThinkingPlaceable.States.Seeking:
						if(thinkingPlaceable.IsTargetInRange())
						{
							thinkingPlaceable.StartAttack();
						}
						break;


					case ThinkingPlaceable.States.Attacking:
						if(thinkingPlaceable.IsTargetInRange())
						{
							if(Time.time >= thinkingPlaceable.lastBlowTime + thinkingPlaceable.attackRatio)
							{
								thinkingPlaceable.DealBlow();
								//Animation will produce the damage, calling animation events OnDealDamage and OnProjectileFired. See ThinkingPlaceable
							}
						}
						break;

					case ThinkingPlaceable.States.Dead:
						Debug.LogError("A dead ThinkingPlaceable shouldn't be in this loop");
						break;
				}
			}

			Projectile currProjectile;
			float progressToTarget;
			for(int prjN=0; prjN<allProjectiles.Count; prjN++)
            {
				currProjectile = allProjectiles[prjN];
				progressToTarget = currProjectile.Move();
				if(progressToTarget >= 1f)
				{
					if(currProjectile.target.state != ThinkingPlaceable.States.Dead) //target might be dead already as this projectile is flying
					{
						float newHP = currProjectile.target.SufferDamage(currProjectile.damage);
						currProjectile.target.healthBar.SetHealth(newHP);
					}
					Destroy(currProjectile.gameObject);
					allProjectiles.RemoveAt(prjN);
				}
			}

            updateAllPlaceables = false; //is set to true by UseCard()
        }

        private List<ThinkingPlaceable> GetAttackList(Faction f, PlaceableTarget t)
        {
            switch(t)
            {
                case PlaceableTarget.Both:
                    return (f == Faction.Player) ? allOpponents : allPlayers;
				case PlaceableTarget.OnlyBuildings:
                    return (f == Faction.Player) ? opponentBuildings : playerBuildings;
				default:
					Debug.LogError("What faction is this?? Not Player nor Opponent.");
					return null;
            }
        }

        private bool FindClosestInList(Vector3 p, List<ThinkingPlaceable> list, out ThinkingPlaceable t)
        {
            t = null;
            bool targetFound = false;
            float closestDistanceSqr = Mathf.Infinity; //anything closer than here becomes the new designated target

            foreach (var thinkingPlaceable in list)
            {
	            float sqrDistance = (p - thinkingPlaceable.transform.position).sqrMagnitude;
	            if(sqrDistance < closestDistanceSqr)
	            {
		            t = thinkingPlaceable;
		            closestDistanceSqr = sqrDistance;
		            targetFound = true;
	            }
            }

            return targetFound;
        }

        public void UseCard(CardData cardData, Vector3 position, Faction pFaction)
        {
            for(int pNum=0; pNum<cardData.placeablesData.Length; pNum++)
            {
                PlaceableData pDataRef = cardData.placeablesData[pNum];
                Quaternion rot = (pFaction == Faction.Player) ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
                //Prefab to spawn is the associatedPrefab if it's the Player faction, otherwise it's alternatePrefab. But if alternatePrefab is null, then first one is taken
                GameObject prefabToSpawn = (pFaction == Faction.Player) ? pDataRef.associatedPrefab : ((pDataRef.alternatePrefab == null) ? pDataRef.associatedPrefab : pDataRef.alternatePrefab);
                GameObject newPlaceableGO = Instantiate<GameObject>(prefabToSpawn, position + cardData.relativeOffsets[pNum], rot);

                SetupPlaceable(newPlaceableGO, pDataRef, pFaction);

				appearEffectPool.UseParticles(position + cardData.relativeOffsets[pNum]);
            }
			//audioManager.PlayAppearSFX(position);

            updateAllPlaceables = true; //will force all AIBrains to update next time the Update loop is run
        }


        //setups all scripts and listeners on a Placeable GameObject
        private void SetupPlaceable(GameObject go, PlaceableData pDataRef, Faction pFaction)
        {
            //Add the appropriate script
                switch(pDataRef.pType)
                {
                    case PlaceableType.Unit:
                        Unit uScript = go.GetComponent<Unit>();
                        uScript.Activate(pFaction, pDataRef); //enables NavMeshAgent
						uScript.OnDealDamage += OnPlaceableDealtDamage;
						uScript.OnProjectileFired += OnProjectileFired;
                        AddPlaceableToList(uScript); //add the Unit to the appropriate list
                        UIManager.AddHealthUI(uScript);
                        break;

                    case PlaceableType.Building:
                    case PlaceableType.Castle:
                        Building bScript = go.GetComponent<Building>();
                        bScript.Activate(pFaction, pDataRef);
						bScript.OnDealDamage += OnPlaceableDealtDamage;
						bScript.OnProjectileFired += OnProjectileFired;
                        AddPlaceableToList(bScript); //add the Building to the appropriate list
                        UIManager.AddHealthUI(bScript);

                        //special case for castles
                        if(pDataRef.pType == PlaceableType.Castle)
                        {
                            bScript.OnDie += OnCastleDead;
                        }

                        navMesh.BuildNavMesh(); //rebake the Navmesh
                        break;

                    case PlaceableType.Obstacle:
                        Obstacle oScript = go.GetComponent<Obstacle>();
                        oScript.Activate(pDataRef);
                        navMesh.BuildNavMesh(); //rebake the Navmesh
                        break;

                    case PlaceableType.Spell:
                        //Spell sScript = newPlaceable.AddComponent<Spell>();
                        //sScript.Activate(pFaction, cardData.hitPoints);
                        //TODO: activate the spell and… ?
                        break;
                }

                go.GetComponent<Placeable>().OnDie += OnPlaceableDead;
        }

		private void OnProjectileFired(ThinkingPlaceable p)
		{
			Vector3 adjTargetPos = p.target.transform.position;
			adjTargetPos.y = 1.5f;
			Quaternion rot = Quaternion.LookRotation(adjTargetPos-p.projectileSpawnPoint.position);

			Projectile prj = Instantiate<GameObject>(p.projectilePrefab, p.projectileSpawnPoint.position, rot).GetComponent<Projectile>();
			prj.target = p.target;
			prj.damage = p.damage;
			allProjectiles.Add(prj);
		}

		private void OnPlaceableDealtDamage(ThinkingPlaceable p)
		{
			if(p.target.state != ThinkingPlaceable.States.Dead)
			{
				float newHealth = p.target.SufferDamage(p.damage);
				p.target.healthBar.SetHealth(newHealth);
			}
		}

		private void OnCastleDead(Placeable c)
		{
			cinematicsManager.PlayCollapseCutscene(c.faction);
            c.OnDie -= OnCastleDead;
            gameOver = true; //stops the thinking loop

			//stop all the ThinkingPlaceables
			foreach (var thinkingPlaceable in allThinkingPlaceables)
			{
				if(thinkingPlaceable.state != ThinkingPlaceable.States.Dead)
				{
					thinkingPlaceable.Stop();
					thinkingPlaceable.transform.LookAt(c.transform.position);
					UIManager.RemoveHealthUI(thinkingPlaceable);
				}
			}

			//audioManager.GoToEndMatchSnapshot();
			CPUOpponent.StopActing();
		}

		public void OnEndGameCutsceneOver()
		{
			UIManager.ShowGameOverUI();
		}

        private void OnPlaceableDead(Placeable p)
        {
            p.OnDie -= OnPlaceableDead; //remove the listener

            switch(p.pType)
            {
                case PlaceableType.Unit:
					Unit u = (Unit)p;
                    RemovePlaceableFromList(u);
					u.OnDealDamage -= OnPlaceableDealtDamage;
					u.OnProjectileFired -= OnProjectileFired;
					UIManager.RemoveHealthUI(u);
					StartCoroutine(Dispose(u));
                    break;

                case PlaceableType.Building:
                case PlaceableType.Castle:
					Building b = (Building)p;
                    RemovePlaceableFromList(b);
					UIManager.RemoveHealthUI(b);
					b.OnDealDamage -= OnPlaceableDealtDamage;
					b.OnProjectileFired -= OnProjectileFired;
                    StartCoroutine(RebuildNavmesh()); //need to fix for normal buildings

					//we don't dispose of the Castle
					if(p.pType != PlaceableType.Castle)
						StartCoroutine(Dispose(b));
                    break;

                case PlaceableType.Obstacle:
                    StartCoroutine(RebuildNavmesh());
                    break;

                case PlaceableType.Spell:
                    //TODO: can spells die?
                    break;
            }
        }

		private IEnumerator Dispose(ThinkingPlaceable p)
		{
			yield return new WaitForSeconds(3f);

			Destroy(p.gameObject);
		}

        private IEnumerator RebuildNavmesh()
        {
            yield return new WaitForEndOfFrame();

            navMesh.BuildNavMesh();
            //FIX: dragged obstacles are included in the navmesh when it's baked
        }

        private void AddPlaceableToList(ThinkingPlaceable p)
        {
	        allThinkingPlaceables.Add(p);

	        switch (p.faction)
	        {
		        case Faction.Player:
		        {
			        allPlayers.Add(p);

			        if(p.pType == PlaceableType.Unit)
				        playerUnits.Add(p);
			        else
				        playerBuildings.Add(p);
			        break;
		        }
		        case Faction.Opponent:
		        {
			        allOpponents.Add(p);

			        if(p.pType == PlaceableType.Unit)
				        opponentUnits.Add(p);
			        else
				        opponentBuildings.Add(p);
			        break;
		        }
		        case Faction.None:
			        break;
		        default:
			        Debug.LogError("Error in adding a Placeable in one of the player/opponent lists");
			        break;
	        }
        }

        private void RemovePlaceableFromList(ThinkingPlaceable p)
        {
	        allThinkingPlaceables.Remove(p);

	        switch (p.faction)
	        {
		        case Faction.Player:
		        {
			        allPlayers.Remove(p);

			        if(p.pType == PlaceableType.Unit)
				        playerUnits.Remove(p);
			        else
				        playerBuildings.Remove(p);
			        break;
		        }
		        case Faction.Opponent:
		        {
			        allOpponents.Remove(p);

			        if(p.pType == PlaceableType.Unit)
				        opponentUnits.Remove(p);
			        else
				        opponentBuildings.Remove(p);
			        break;
		        }
		        case Faction.None:
			        break;
		        default:
			        Debug.LogError("Error in removing a Placeable from one of the player/opponent lists");
			        break;
	        }
        }
    }
}