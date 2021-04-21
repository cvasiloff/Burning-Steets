using UnityEngine;
using System.Collections;

public class ValueUpdate : MonoBehaviour
{

    private UnityEngine.UI.Slider my_slider;
    private UnityEngine.UI.InputField my_field;

    void Start()
    {
        my_slider = gameObject.GetComponent<UnityEngine.UI.Slider>();
        my_field = gameObject.GetComponent<UnityEngine.UI.InputField>();
    }

    public void UpdateValueFromFloat(float value)
    {
        Debug.Log("float value changed: " + value);
        if (my_slider) { my_slider.value = value; }
        if (my_field) { my_field.text = value.ToString(); }
    }

    public void UpdateValueFromString(string value)
    {
        Debug.Log("string value changed: " + value);
        if (my_slider) { my_slider.value = float.Parse(value); }
        if (my_field) { my_field.text = value; }
    }


}
