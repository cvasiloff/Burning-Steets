using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSliders : MonoBehaviour
{
    public UnityEngine.UI.InputField SensField;
    public UnityEngine.UI.InputField VolField;
    public UnityEngine.UI.Slider SensSlider;
    public UnityEngine.UI.Slider VolSlider;
    ValueUpdate SensF;
    ValueUpdate VolF;
    ValueUpdate SensS;
    ValueUpdate VolS;

    public void LoadSliderValues()
    {
        if (PlayerPrefs.GetFloat("sensitivity") > 0)
        {
            SensField.text = (PlayerPrefs.GetFloat("sensitivity")).ToString();
            SensSlider.value = (PlayerPrefs.GetFloat("sensitivity"));
        }

        VolField.text = (PlayerPrefs.GetFloat("volume")).ToString();
        VolSlider.value = (PlayerPrefs.GetFloat("volume"));
    }
}
