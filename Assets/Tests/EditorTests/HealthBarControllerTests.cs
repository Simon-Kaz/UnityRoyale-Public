using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityRoyale;

public class HealthBarControllerTests : MonoBehaviour
{
    [Test]
    public void InitialisesCorrectly_ForOpponentUnit()
    {
        var thinkingPlaceable = Substitute.For<IThinkingPlaceable>();
        thinkingPlaceable.faction.Returns(Faction.Opponent);
        thinkingPlaceable.position.Returns(new Vector3(0,0,0));
        thinkingPlaceable.pType.Returns(PlaceableType.Unit);

        var healthBarController = new HealthBarController(thinkingPlaceable);
        Color blueColor = new Color32(31, 132, 255, 255);

        Assert.That(healthBarController.BarColor, Is.EqualTo(blueColor));
        Assert.That(healthBarController.LocalPosition, Is.EqualTo(new Vector3(0, 3f, 0f)));
    }
}
