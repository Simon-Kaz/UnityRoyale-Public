using UnityEngine;
using UnityRoyale;

public interface IThinkingPlaceable
{
    public float HitPoints { get; }
    public Vector3 Position { get; }
    public Faction faction { get; } // TODO:refactor Placeable first for naming consistency
    public PlaceableType pType { get; } // TODO:refactor Placeable first for naming consistency
}