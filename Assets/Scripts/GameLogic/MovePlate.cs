using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;

    GameObject reference = null;

    // Board position not World Position
    int matrixX;
    int matrixY;

    // false: movement. true: attacking
    public bool attack = false;

    void Start()
    {
        if (attack)
        {
            // Change to Red
            GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
    }

    void OnMouseUp()
    {
        // Fixed: Proper assignment and GetComponent<Game>()
        controller = GameObject.FindGameObjectWithTag("GameController");

        if (attack)
        {
            // Fixed: Access GetPosition on Game component
            GameObject cp = controller.GetComponent<Game>().GetPosition(matrixX, matrixY);
            if (cp != null)
            {
                Destroy(cp);
            }
        }

        // Fixed: Access SetPositionEmpty on Game
        controller.GetComponent<Game>().SetPositionEmpty(reference.GetComponent<Chessman>().GetXBoard(), reference.GetComponent<Chessman>().GetYBoard());

        // Fixed: SetYBoard (was duplicate SetXBoard); call SetCoordinates()
        reference.GetComponent<Chessman>().SetXBoard(matrixX);
        reference.GetComponent<Chessman>().SetYBoard(matrixY);
        reference.GetComponent<Chessman>().SetCoordinates();

        // Fixed: Access SetPosition on Game
        controller.GetComponent<Game>().SetPosition(reference);

        reference.GetComponent<Chessman>().DestroyMovePlates();
    }

    public void SetCoordinates(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }

    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    public GameObject GetReference()
    {
        return reference;
    }
}