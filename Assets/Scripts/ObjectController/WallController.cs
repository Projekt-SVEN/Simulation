using UnityEngine;

public class WallController : MonoBehaviour
{
    [SerializeField]
    private Sprite _wallBase;

    [SerializeField]
    private Sprite _wallNorth;

    [SerializeField]
    private Sprite _wallEast;

    [SerializeField]
    private Sprite _wallSouth;
    
    [SerializeField]
    private Sprite _wallVest;

    [SerializeField]
    private Sprite _wallNorthEast;

    [SerializeField]
    private Sprite _wallNorthSouth;

    [SerializeField]
    private Sprite _wallNorthVest;

    [SerializeField]
    private Sprite _wallEastSouth;

    [SerializeField]
    private Sprite _wallEastVest;

    [SerializeField]
    private Sprite _wallSouthVest;

    [SerializeField]
    private Sprite _wallNorthEastSouth;

    [SerializeField]
    private Sprite _wallNorthEastVest;

    [SerializeField]
    private Sprite _wallNorthSouthVest;

    [SerializeField]
    private Sprite _wallEastSouthVest;

    [SerializeField]
    private Sprite _wallNorthEastSouthVest;

    [SerializeField]
    private SpriteRenderer _wall;

    public void SetWalls(bool[] neighbours)
    {
        //Alle Set
        if(!neighbours[0] && !neighbours[1] && !neighbours[2] && !neighbours[3])
        {
            _wall.sprite = _wallNorthEastSouthVest;
        }
        //NorthEastSouth
        else if (!neighbours[0] && !neighbours[1] && !neighbours[2] && neighbours[3])
        {
            _wall.sprite = _wallNorthEastSouth;
        }
        //NorthEastVest
        else if (!neighbours[0] && !neighbours[1] && neighbours[2] && !neighbours[3])
        {
            _wall.sprite = _wallNorthEastVest;
        }
        //NorthSouthVest
        else if (!neighbours[0] && neighbours[1] && !neighbours[2] && !neighbours[3])
        {
            _wall.sprite = _wallNorthSouthVest;
        }
        //EastSouthVest
        else if (neighbours[0] && !neighbours[1] && !neighbours[2] && !neighbours[3])
        {
            _wall.sprite = _wallEastSouthVest;
        }
        //NorthEast
        else if (!neighbours[0] && !neighbours[1] && neighbours[2] && neighbours[3])
        {
            _wall.sprite = _wallNorthEast;
        }
        //NorthSouth
        else if (!neighbours[0] && neighbours[1] && !neighbours[2] && neighbours[3])
        {
            _wall.sprite = _wallNorthSouth;
        }
        //NorthVest
        else if (!neighbours[0] && neighbours[1] && neighbours[2] && !neighbours[3])
        {
            _wall.sprite = _wallNorthVest;
        }
        //EastSouth
        else if (neighbours[0] && !neighbours[1] && !neighbours[2] && neighbours[3])
        {
            _wall.sprite = _wallEastSouth;
        }
        //EastVest
        else if (neighbours[0] && !neighbours[1] && neighbours[2] && !neighbours[3])
        {
            _wall.sprite = _wallEastVest;
        }
        //SouthVest
        else if (neighbours[0] && neighbours[1] && !neighbours[2] && !neighbours[3])
        {
            _wall.sprite = _wallSouthVest;
        }
        //North
        else if (!neighbours[0] && neighbours[1] && neighbours[2] && neighbours[3])
        {
            _wall.sprite = _wallNorth;
        }
        //East
        else if (neighbours[0] && !neighbours[1] && neighbours[2] && neighbours[3])
        {
            _wall.sprite = _wallEast;
        }
        //South
        else if (neighbours[0] && neighbours[1] && !neighbours[2] && neighbours[3])
        {
            _wall.sprite = _wallSouth;
        }
        //Vest
        else if (neighbours[0] && neighbours[1] && neighbours[2] && !neighbours[3])
        {
            _wall.sprite = _wallVest;
        }
    }
}
