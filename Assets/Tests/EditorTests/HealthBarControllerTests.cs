using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityRoyale;

public class HealthBarControllerTests : MonoBehaviour
{
    [Test]
    public void InitialisesWithOpponentColor_ForOpponentUnit()
    {
        var thinkingPlaceable = Substitute.For<IThinkingPlaceable>();
        thinkingPlaceable.faction.Returns(Faction.Opponent);
        thinkingPlaceable.Position.Returns(new Vector3(0, 0, 0));
        thinkingPlaceable.pType.Returns(PlaceableType.Unit);

        var healthBarController = new HealthBarController(thinkingPlaceable);

        Assert.That(healthBarController.BarColor, Is.EqualTo(Faction.Opponent.GetColor()));
    }

    [Test]
    public void InitialisesWithPlayerColor_ForPlayerUnit()
    {
        var thinkingPlaceable = Substitute.For<IThinkingPlaceable>();
        thinkingPlaceable.faction.Returns(Faction.Player);
        thinkingPlaceable.Position.Returns(new Vector3(0, 0, 0));
        thinkingPlaceable.pType.Returns(PlaceableType.Unit);

        var healthBarController = new HealthBarController(thinkingPlaceable);

        Assert.That(healthBarController.BarColor, Is.EqualTo(Faction.Player.GetColor()));
    }

    [Test]
    public void InitialisesAtCorrectPosition_ForUnit()
    {
        var thinkingPlaceable = Substitute.For<IThinkingPlaceable>();
        thinkingPlaceable.faction.Returns(Faction.Opponent);
        thinkingPlaceable.Position.Returns(new Vector3(0, 0, 0));
        thinkingPlaceable.pType.Returns(PlaceableType.Unit);

        var healthBarController = new HealthBarController(thinkingPlaceable);

        Assert.That(healthBarController.LocalPosition, Is.EqualTo(new Vector3(0, 3f, 0f)));
    }

    [Test]
    public void InitialisesAtCorrectPosition_ForBuilding()
    {
        var thinkingPlaceable = Substitute.For<IThinkingPlaceable>();
        thinkingPlaceable.faction.Returns(Faction.Opponent);
        thinkingPlaceable.Position.Returns(new Vector3(0, 0, 0));
        thinkingPlaceable.pType.Returns(PlaceableType.Building);

        var healthBarController = new HealthBarController(thinkingPlaceable);

        Assert.That(healthBarController.LocalPosition, Is.EqualTo(new Vector3(0, 6f, -2f)));
    }
}