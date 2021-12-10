using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class load : MonoBehaviour
{

    public bool isGenerated = false;
    private int typeMethod;

    public GameObject prefabMAM;
    // Start is called before the first frame update
    void Start()
    {
        LevelLoader levelLoader = FindObjectOfType<LevelLoader>();
        typeMethod = (int) levelLoader.dataScene[0];

        switch (typeMethod)
        {
            case 0:

                break;
            case 1:
                GameObject MAM = Instantiate(prefabMAM);
                break;

            case 2:

                break;
        }
        isGenerated = true;
    }


}
