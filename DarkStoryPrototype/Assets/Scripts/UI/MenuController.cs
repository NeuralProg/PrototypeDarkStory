using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;



public class MenuController : MonoBehaviour

{

    /*
    [Header("Volume Settings")]
    [SerializeField] private TMP_TextElement volumeTextValue = null;
    [SerializeField] private Slider volumeSlider = null; 
    */

    [Header("Dialogs Load")]
    public string _newGamelevel;
    [SerializeField] private GameObject noSavedGameDialog = null;
    private string levelToLoad;


    public void NewGameDialogYes()
    {
        SceneManager.LoadScene(_newGamelevel);
    }

    public void LoadGameDialogYes()
    {
        if (PlayerPrefs.HasKey("SavedLevel"))
        {
            levelToLoad = PlayerPrefs.GetString("SavedLevel");
            SceneManager.LoadScene(levelToLoad);
        }
        else
        {
            noSavedGameDialog.SetActive(true); 
        }
    }

    public void EXITBUTTON()
    {
        Application.Quit();
    }
    /* Volume and settings shit, later
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        volumeTextValue. = volume.ToString(" 0.0");
    }

    public void VolumeSet()
    {
        PlayerPrefs.SetFloat("masterVolume", AudioListener.volume);
    }
    */
}
