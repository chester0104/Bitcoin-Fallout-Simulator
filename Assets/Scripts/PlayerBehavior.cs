using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{    
    bool levelLost = false;

    public void Start()
    {
        levelLost = false;        
    }

    void Update()
    {
        // Check for falling off the map
        if (transform.position.y < -10 && !levelLost)
        {
            levelLost = true;
            LevelManager levelManager = FindAnyObjectByType<LevelManager>();
            if (levelManager)
            {
                levelManager.LevelLost();
            }
        }
    }
}