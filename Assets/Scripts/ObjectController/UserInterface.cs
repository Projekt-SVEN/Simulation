using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    [SerializeField]
    private GameObject _textContainer;

    [SerializeField]
    private GameObject _regulators;

    [SerializeField]
    private Button _minmizeRegulators_Button;

    [SerializeField]
    private Button _maximizeRegulators_Button;

    private float _minTempStart;
    private float _maxTempStart;

    // Start is called before the first frame update
    void Start()
    {
        _minTempStart = 25.0f;
        _maxTempStart = 40.0f;
    }

    // Update is called once per frame
    void Update()
    {
        _maxTempStart = OptionsManager.MaxTemperatur;
        _minTempStart = OptionsManager.MinTemperatur;

        float temp = _maxTempStart;
        float div = (_maxTempStart - _minTempStart) / (_textContainer.transform.childCount - 1);


        foreach (Transform child in _textContainer.transform)
        {
            child.gameObject.GetComponent<TextMeshProUGUI>().text = temp.ToString("0.0");
            temp -= div;
        }
    }


    public void MinimzeRegulatorsOnClick()
    {
        _minmizeRegulators_Button.interactable = false;
        _maximizeRegulators_Button.interactable = true;
        _regulators.SetActive(false);
    }

    public void MaximzeRegulatorsOnClick()
    {
        _minmizeRegulators_Button.interactable = true;
        _maximizeRegulators_Button.interactable = false;
        _regulators.SetActive(true);
    }

    public void MaxUserSpeedSliderChange(Slider slider)
    {
        slider.GetComponentInChildren<TextMeshProUGUI>().text = slider.value.ToString();
        OptionsManager.MaxUserSpeed = slider.value;
    }

    public void MinUserSpeedSliderChange(Slider slider)
    {
        slider.GetComponentInChildren<TextMeshProUGUI>().text = slider.value.ToString();
        OptionsManager.MinUserSpeed = slider.value;
    }

    public void UpperMaxOKUserTemperaturEditFinish(TMP_InputField input)
    {
        OptionsManager.UpperMaxOkUserTemperature = float.Parse(input.text);
    }

    public void UpperMinOKUserTemperaturEditFinish(TMP_InputField input)
    {
        OptionsManager.UpperMinOkUserTemperature = float.Parse(input.text);
    }

    public void LowerMaxOKUserTemperaturEditFinish(TMP_InputField input)
    {
        OptionsManager.LowerMaxOkUserTemperature = float.Parse(input.text);
    }

    public void LowerMinOKUserTemperaturEditFinish(TMP_InputField input)
    {
        OptionsManager.LowerMinOkUserTemperature = float.Parse(input.text);
    }

    public void ProbabilityOfUserStandingUpInPauseSliderChange(Slider slider)
    {
        slider.GetComponentInChildren<TextMeshProUGUI>().text = slider.value.ToString() + "%";
        OptionsManager.ProbabilityOfUserStandingUpInPause = slider.value;
    }
}
