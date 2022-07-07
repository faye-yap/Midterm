using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{ 
    public int m_NumRoundsToWin = 5;            
    public float m_StartDelay = 3f;             
    public float m_EndDelay = 3f;               
    public CameraControl m_CameraControl;       
    public Text m_MessageText;                  
    public GameObject[] m_TankPrefabs;
    public TankManager[] m_Tanks;               
    public List<Transform> wayPointsForAI;

    private int m_RoundNumber;                  
    private WaitForSeconds m_StartWait;         
    private WaitForSeconds m_EndWait;           
    private TankManager m_RoundWinner;          
    private TankManager m_GameWinner;    
    public bool isPaused;
    public GameObject pauseMenuPrefab;
    private GameObject pauseMenu;
    
    public Dictionary<string,bool> gottenAchievements = new Dictionary<string,bool>();
    private float elapsedTime = 0f;
    
    


    private void Start()
    {
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        
        
        gottenAchievements.Add("Win a round in 60 seconds",false);
        gottenAchievements.Add("Win a round in 30 seconds",false);
        

        SpawnAllTanks();
        SetCameraTargets();

        StartCoroutine(GameLoop());
    }

    //Update() only used to pause the game and to add to elapsedTime
    private void Update(){

        if (Input.GetKeyDown(KeyCode.Escape)){
            
            if(!isPaused) {
                Time.timeScale = 0;
                pauseMenu = Instantiate(pauseMenuPrefab);
                
                string gottenAchievementsText = "";
                
                foreach(KeyValuePair<string,bool> pair in gottenAchievements){
                    if (pair.Value){
                        gottenAchievementsText += pair.Key + "\n\n";
                    }
                }
                //update the list of gotten achievements in the pause menu
                pauseMenu.transform.GetChild(1).GetComponent<Text>().text = gottenAchievementsText;
                
                isPaused = true;
            } else {
                
                Destroy(pauseMenu);             
                Time.timeScale = 1;
                isPaused = false;
            }
        }
        elapsedTime += Time.deltaTime;
    }

   

    private void SpawnAllTanks()
    {
        m_Tanks[0].m_Instance =
            Instantiate(m_TankPrefabs[0], m_Tanks[0].m_SpawnPoint.position, m_Tanks[0].m_SpawnPoint.rotation) as GameObject;
        m_Tanks[0].m_PlayerNumber = 1;
        m_Tanks[0].SetupPlayerTank();

        for (int i = 1; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefabs[i], m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].SetupAI(wayPointsForAI);
        }
    }


    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];

        for (int i = 0; i < targets.Length; i++)
            targets[i] = m_Tanks[i].m_Instance.transform;

        m_CameraControl.m_Targets = targets;
    }


    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameWinner != null) SceneManager.LoadScene(0);
        else StartCoroutine(GameLoop());
    }


    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();

        m_CameraControl.SetStartPositionAndSize();

        m_RoundNumber++;
        m_MessageText.text = $"ROUND {m_RoundNumber}";

        yield return m_StartWait;
    }


    private IEnumerator RoundPlaying()
    {
        EnableTankControl();

        m_MessageText.text = string.Empty;

        while (!OneTankLeft()) yield return null;
    }


    private IEnumerator RoundEnding()
    {
        DisableTankControl();

        m_RoundWinner = null;

        m_RoundWinner = GetRoundWinner();
        if (m_RoundWinner != null) m_RoundWinner.m_Wins++;
        //check if player won
        if (m_RoundWinner == m_Tanks[0] && elapsedTime < 60){
            gottenAchievements["Win a round in 60 seconds"] = true;
            if (elapsedTime < 30){
                gottenAchievements["Win a round in 30 seconds"] = true;
            }
        }
        elapsedTime = 0f;

        m_GameWinner = GetGameWinner();
        CalculateHandicaps();
        string message = EndMessage();
        m_MessageText.text = message;

        yield return m_EndWait;
    }

    private void CalculateHandicaps(){
        int currentMin = m_NumRoundsToWin + 1;
        List<int> wins = new List<int>();
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            wins.Add(m_Tanks[i].m_Wins);               
        }
        //get minimum number of wins
        foreach (int i in wins){
            if (i < currentMin){
                currentMin = i;
            }
        }
        //handicap still == 1 for player with least wins, but increases as the difference in number of wins increases
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            int difference = m_Tanks[i].m_Wins - currentMin;
            m_Tanks[i].m_Instance.GetComponent<TankHealth>().handicap = 1 + difference * 0.1f;              
        }

    }


    private bool OneTankLeft()
    {
        int numTanksLeft = 0;

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf) numTanksLeft++;
        }

        return numTanksLeft <= 1;
    }

    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        return null;
    }

    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        return null;
    }


    private string EndMessage()
    {
        var sb = new StringBuilder();

        if (m_RoundWinner != null) sb.Append($"{m_RoundWinner.m_ColoredPlayerText} WINS THE ROUND!");
        else sb.Append("DRAW!");

        sb.Append("\n\n\n\n");

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            sb.AppendLine($"{m_Tanks[i].m_ColoredPlayerText}: {m_Tanks[i].m_Wins} WINS");
        }

        if (m_GameWinner != null)
            sb.Append($"{m_GameWinner.m_ColoredPlayerText} WINS THE GAME!");

        return sb.ToString();
    }


    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].Reset();
    }


    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].EnableControl();
    }


    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].DisableControl();
    }
}