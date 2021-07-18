using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityRoyale;

public class HealthBarControllerTests : MonoBehaviour
{
    [Test]
    public void HealthBar_ForOpponentUnit_InitialisesAtCorrectPositionWithOpponentColor()
    {
        var thinkingPlaceable = Substitute.For<IThinkingPlaceable>();
        thinkingPlaceable.faction.Returns(Faction.Opponent);
        thinkingPlaceable.Position.Returns(new Vector3(0,0,0));
        thinkingPlaceable.pType.Returns(PlaceableType.Unit);

        var healthBarController = new HealthBarController(thinkingPlaceable);
        var expectedBarColor = Faction.Opponent.GetColor();

        Assert.That(healthBarController.BarColor, Is.EqualTo(expectedBarColor));
        Assert.That(healthBarController.LocalPosition, Is.EqualTo(new Vector3(0, 3f, 0f)));
    }

    [Test]
    public void HealthBar_ForPlayerUnit_InitialisesAtCorrectPositionWithPlayerColor()
    {
        var thinkingPlaceable = Substitute.For<IThinkingPlaceable>();
        thinkingPlaceable.faction.Returns(Faction.Player);
        thinkingPlaceable.Position.Returns(new Vector3(0,0,0));
        thinkingPlaceable.pType.Returns(PlaceableType.Unit);

        var healthBarController = new HealthBarController(thinkingPlaceable);
        var expectedBarColor = Faction.Player.GetColor();

        Assert.That(healthBarController.BarColor, Is.EqualTo(expectedBarColor));
        Assert.That(healthBarController.LocalPosition, Is.EqualTo(new Vector3(0, 3f, 0f)));
    }

    [Test]
    public void HealthBar_ForOpponentBuilding_InitialisesAtCorrectPositionWithOpponentColor()
    {
        var thinkingPlaceable = Substitute.For<IThinkingPlaceable>();
        thinkingPlaceable.faction.Returns(Faction.Opponent);
        thinkingPlaceable.Position.Returns(new Vector3(0,0,0));
        thinkingPlaceable.pType.Returns(PlaceableType.Building);

        var healthBarController = new HealthBarController(thinkingPlaceable);
        var expectedBarColor = Faction.Opponent.GetColor();

        Assert.That(healthBarController.BarColor, Is.EqualTo(expectedBarColor));
        Assert.That(healthBarController.LocalPosition, Is.EqualTo(new Vector3(0, 6f, -2f)));
    }
}
