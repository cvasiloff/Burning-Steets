using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePerfs : MonoBehaviour
{
    public UnityEngine.UI.InputField SensField;
    public UnityEngine.UI.InputField VolField;

    public void SaveValues()
    {
        PlayerPrefs.SetFloat("sensitivity", float.Parse(SensField.text));
        PlayerPrefs.SetFloat("volume", float.Parse(VolField.text));
        PlayerPrefs.Save();
        AudioListener.volume = PlayerPrefs.GetFloat("volume");

        NetworkPlayerController[] MyControllers = GameObject.FindObjectsOfType<NetworkPlayerController>();

        foreach (NetworkPlayerController p in MyControllers)
        {
            if (p.IsLocalPlayer)
            {
                p.sensitivity = PlayerPrefs.GetFloat("sensitivity");
            }
        }
    }
}
