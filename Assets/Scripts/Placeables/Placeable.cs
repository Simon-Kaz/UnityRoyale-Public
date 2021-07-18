using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityRoyale
{
    //Base class for all objects that can be placed on the play area: units, obstacles, structures, etc.
    public class Placeable : MonoBehaviour
    {
        public PlaceableType pType { get; protected set; }
        public Faction faction { get; protected set; }
        [HideInInspector] public PlaceableTarget targetType; //TODO: move to ThinkingPlaceable?
		[HideInInspector] public AudioClip dieAudioClip;

        public UnityAction<Placeable> OnDie;
    }

    public enum Faction
    {
        Player,
        Opponent,
        None,
    }

    public enum PlaceableTarget
    {
        OnlyBuildings,
        Both,
        None,
    }

    public enum PlaceableType
    {
        Unit,
        Obstacle,
        Building,
        Spell,
        Castle, //special type of building
    }

    public enum FactionColor
    {
        Red,
        Blue
    }

    public static class FactionExtension
    {
        public static Color GetColor(this Faction faction)
        {
            return faction switch
            {
                Faction.Opponent => new Color32(252, 35, 13, 255),
                Faction.Player => new Color32(31, 132, 255, 255),
                Faction.None => throw new ArgumentOutOfRangeException(nameof(faction), faction,
                    "Specified faction's color has not been provided"),
                _ => throw new ArgumentOutOfRangeException(nameof(faction), faction,
                    "Specified faction's color has not been provided")
            };
        }
    }
}