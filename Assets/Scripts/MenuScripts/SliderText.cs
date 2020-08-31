using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _valueText;


    // Update is called once per frame
    void Update()
    {
        _valueText.text = GetComponent<Slider>().value.ToString();
    }
}
