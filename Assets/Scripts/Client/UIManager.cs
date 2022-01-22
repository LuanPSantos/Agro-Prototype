using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Slider forcePercentSlider;
    void Start()
    {
        BowController.LaunchForcePercentChanged += OnLaunchForcePercentChanged;
    }

    void OnDestroy()
    {
        BowController.LaunchForcePercentChanged -= OnLaunchForcePercentChanged;
    }
    void OnLaunchForcePercentChanged(float forcePercent)
    {
        forcePercentSlider.value = forcePercent;
    }
}
