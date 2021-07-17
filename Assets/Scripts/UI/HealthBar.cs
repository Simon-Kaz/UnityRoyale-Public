using UnityEngine;
using UnityEngine.UI;
using UnityRoyale;

namespace UnityRoyale
{
    public class HealthBar : MonoBehaviour
    {
        public RectTransform bar;
        public GameObject wholeWidget;
        private HealthBarController healthBarController;
        private bool _isHidden = true;

        public void Initialise(ThinkingPlaceable placeable)
        {
            healthBarController = new HealthBarController(placeable);

            bar.GetComponent<Image>().color = healthBarController.BarColor;
            wholeWidget.transform.localPosition = healthBarController.LocalPosition;
            wholeWidget.SetActive(false);
        }

        public void SetHealth(float newHP)
        {
            if (_isHidden)
            {
                wholeWidget.SetActive(true);
                _isHidden = false;
            }

            var ratio = healthBarController.CalculateRatio(newHP);

            bar.localScale = new Vector3(ratio, 1f, 1f);
        }

        public void Move()
        {
            transform.position = healthBarController.PositionToFollow;
        }
    }
}

public class HealthBarController
{
    public Vector3 PositionToFollow { get; }
    public Color BarColor { get; }
    public Vector3 LocalPosition { get; }

    private float _currentHp;
    private readonly float _originalHp;

    private readonly Color _red = new Color32(252, 35, 13, 255);
    private readonly Color _blue = new Color32(31, 132, 255, 255);

    public HealthBarController(IThinkingPlaceable placeable)
    {
        _originalHp = _currentHp = placeable.hitPoints;
        PositionToFollow = placeable.position;
        BarColor = placeable.faction == Faction.Player ? _red : _blue;
        LocalPosition = new Vector3(0f,
            (placeable.pType == PlaceableType.Unit) ? 3f : 6f,
            (placeable.pType == PlaceableType.Unit)
                ? 0f
                : -2f); //set the vertical position based on the type of Placeable
    }

    public float CalculateRatio(float newHp)
    {
        return newHp > 0f ? newHp / _originalHp : 0f;
    }
}

public interface IThinkingPlaceable
{
    public float hitPoints { get; }
    public Vector3 position { get; }
    public Faction faction { get; }
    public PlaceableType pType { get; }
}