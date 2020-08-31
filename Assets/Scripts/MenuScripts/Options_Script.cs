using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Options_Script: MonoBehaviour
{
    [SerializeField]
    private Slider _movementSpeed;

    [SerializeField]
    private Slider _zoomSpeed;

    [SerializeField]
    private TMP_Dropdown _resolutioDropdown;

    [SerializeField]
    private TMP_InputField _maxTemperatur;

    [SerializeField]
    private TMP_InputField _minTemperatur;

    private List<int[]> _resolutionList;


    private int[,] _baseresolutions = new int[,]
    {
        {3840, 2160},
        {3440, 1440},
        {2560, 1440},
        {2560, 1080},
        {2048, 1152},
        {1920, 1200},
        {1920, 1080},
        {1680, 1050},
        {1600, 900},
        {1536, 864},
        {1440, 900},
        {1366, 768},
        {1360, 768},
        {1280, 800},
        {1280, 720},
        {1024, 768},
        {800, 600},
        {640, 360}
    };

    // Start is called before the first frame update
    void Start()
    {
        _zoomSpeed.value = OptionsManager.CameraZoomSpeed;
        _movementSpeed.value = OptionsManager.MovementSpeed;

        _resolutionList = new List<int[]>();
        int[] newElement = new int[2];
        newElement[0] = Screen.currentResolution.width;
        newElement[1] = Screen.currentResolution.height;
        _resolutionList.Add(newElement);

        int i = 0;
        List<String> options = new List<String>();
        options.Add(newElement[0].ToString() + "x" + newElement[1].ToString());

        while(_baseresolutions[i,0] > newElement[0] || _baseresolutions[i,1] > newElement[1])
        {
            i++;
        }

        for(; i < _baseresolutions.GetLength(0); i++)
        {
            _resolutionList.Add(new int[]{
                _baseresolutions[i, 0],
                _baseresolutions[i, 1]
            });

            options.Add(_baseresolutions[i, 0].ToString() + "x" + _baseresolutions[i, 1].ToString());
        }

        _resolutioDropdown.AddOptions(options);

        _maxTemperatur.interactable = !OptionsManager.DynamicTemperatureScaling;
        _minTemperatur.interactable = !OptionsManager.DynamicTemperatureScaling;
        _maxTemperatur.text = OptionsManager.MaxTemperatur.ToString();
        _minTemperatur.text = OptionsManager.MinTemperatur.ToString();
    }

    public void WindowMode_Change(TMP_Dropdown dropdown)
    {
        switch(dropdown.value)
        {
            case 0:
                Screen.fullScreen = true;
                OptionsManager.WindowMode = 0;
                break;
            case 1:
                Screen.fullScreen = false;
                OptionsManager.WindowMode = 1;
                break;
        }
    }

    public void WindowSize_Change()
    {
        int width = _resolutionList[_resolutioDropdown.value][0];
        int heigth = _resolutionList[_resolutioDropdown.value][1];

        Screen.SetResolution(width, heigth, OptionsManager.WindowMode == 0 ? true : false);

        OptionsManager.WindowWidth = width;
        OptionsManager.WindowHeigth = heigth;

    }

    public void DynamicSkalar_toggel(Toggle toggle)
    {
        OptionsManager.DynamicTemperatureScaling = toggle.isOn;

        _maxTemperatur.interactable = !OptionsManager.DynamicTemperatureScaling;
        _minTemperatur.interactable = !OptionsManager.DynamicTemperatureScaling;

        if(!OptionsManager.DynamicTemperatureScaling)
        {
            OptionsManager.MaxTemperatur = float.Parse(_maxTemperatur.text);
            OptionsManager.MinTemperatur = float.Parse(_minTemperatur.text);
        }
    }

    public void EditMaxTemperatur()
    {
        OptionsManager.MaxTemperatur = float.Parse(_maxTemperatur.text);
    }

    public void EditMinTemperatur()
    {
        OptionsManager.MinTemperatur = float.Parse(_minTemperatur.text);
    }
}
