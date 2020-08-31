using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimulationSceneMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject _menu;

    [SerializeField]
    private GameObject _userInterface;

    [SerializeField]
    private GameObject _CameraControll;

    [SerializeField]
    private GameObject _OptionPane;

    public void Continu_OnClick()
    {
        _CameraControll.GetComponent<CameraController>().EnabelMovement();
        _menu.SetActive(false);
        _userInterface.SetActive(true);
    }

    public void Quit_OnClick()
    {
        SceneManager.LoadScene("StartScene", LoadSceneMode.Single);
    }

    public void Options_OnClick()
    {
        _menu.SetActive(false);
        _OptionPane.SetActive(true);
    }

    public void Back_OnClick()
    {
        _menu.SetActive(true);
        _OptionPane.SetActive(false);
    }

    public void MovementSpeed_Slider(Slider slider)
    {
        OptionsManager.MovementSpeed = slider.value;
    }

    public void ZoomSpeed_Slider (Slider slider)
    {
        OptionsManager.CameraZoomSpeed = slider.value;
    }
}
