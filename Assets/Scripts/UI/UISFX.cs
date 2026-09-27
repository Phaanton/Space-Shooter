using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISFX : MonoBehaviour
{
    [SerializeField] private AudioSource selectSFX;
    [SerializeField] private AudioSource hoverSFX;
    private bool hasSelectPlayed = false;
    private bool hasHoverPlayed = false;
    private float soundSelectDuration;
    private float soundHoverDuration;

    void Start()
    {
        if (hoverSFX != null)
        {
            soundSelectDuration = selectSFX.clip.length;
            soundHoverDuration = hoverSFX.clip.length;
        }
    }

    private void OnNormal()
    {
        if (hasHoverPlayed)
        {
            hasSelectPlayed = false;
            hasHoverPlayed = false;
        }
    }

    private void OnSelecButton()
    {
        if (selectSFX != null)
        {
            if (!hasSelectPlayed)
            {
                selectSFX.Play();
                Invoke(nameof(FlagPlayedSelect), soundSelectDuration);
            }
        }
    }

    private void OnHover()
    {
        if (hoverSFX != null)
        {
            if (!hasHoverPlayed)
            {
                hoverSFX.Play();
                Invoke(nameof(FlagPlayedHover), soundHoverDuration);
            }
        }
    }

    private void FlagPlayedSelect()
    {
        hasSelectPlayed = true;
    }

    private void FlagPlayedHover()
    {
        hasHoverPlayed = true;
    }
}
