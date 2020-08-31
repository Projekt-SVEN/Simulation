using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Abstractions;
using UnityEngine;

public class TemperatureController : MonoBehaviour
{
    private float _temperature = 0f;

    [SerializeField]
    private SpriteRenderer _airSprite;

    public Vector2 Position { get; set; }

    public float Temperature
    {
        get => _temperature;
        set => _temperature = value;
    }

    public void SetColor(Color32 col)
    {
        _airSprite.color = col;
    }
}
