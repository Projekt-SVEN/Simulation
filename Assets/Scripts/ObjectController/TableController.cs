using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableController : MonoBehaviour
{
    [SerializeField]
    private Sprite _blank;

    [SerializeField]
    private Sprite _student;

    [SerializeField]
    private Sprite _dozent;

    [SerializeField]
    private Sprite _lecturehall;

    [SerializeField]
    private SpriteRenderer _table;

    public void setSprite(string type)
    {
        switch(type)
        {
            case "Dozent":
                _table.sprite = _dozent;
                break;
            case "Student":
                _table.sprite = _student;
                break;
            case "Lecurehall":
                _table.sprite = _lecturehall;
                break;
            case "Blank":
                _table.sprite = _blank;
                break;
            default:
                _table.sprite = _blank;
                break;
        }
    }
}
