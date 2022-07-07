using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{

    public GameObject pauseMenuPrefab;
    public GameObject achievementMenuPrefab;
    private GameObject pauseMenu;
    private GameObject achievementMenu;
    private bool isPaused = false;    

    private List<string> achievements = new List<string>(); 
    // Start is called before the first frame update
    void Start()
    {
        achievements.Add("Win a round in 60 seconds");
        achievements.Add("Win a round in 30 seconds");
    }

    // Update is called once per frame
    private void Update(){
        if (Input.GetKeyDown(KeyCode.Escape)){
            
            
            if(isPaused) {
                if(pauseMenu != null) Destroy(pauseMenu);
                if(achievementMenu != null) Destroy(achievementMenu);                
                Time.timeScale = 1;
                isPaused = false;
            }
        }
    }

    public void LoadAchievements(){
        Destroy(pauseMenu);
        achievementMenu = Instantiate(achievementMenuPrefab);
        Text achievedAchievementListText = achievementMenu.transform.GetChild(3).gameObject.GetComponent<Text>();
        foreach (string achievement in achievements) {
            achievedAchievementListText.text += achievement + "\n\n";
        }

    }

    public void BackToPause(){
        Destroy(achievementMenu);
        pauseMenu = Instantiate(pauseMenuPrefab);
    }

}
