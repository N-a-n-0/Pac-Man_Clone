using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public GameObject pacman;

    public GameObject leftWarpNode;
    public GameObject rightWarpNode;

    public AudioSource siren;
    public AudioSource munch1;
    public AudioSource munch2;

   
    public float currentPowerPelletTime = 0;
    public float powerPelletTimer = 8f;
    public int powerPelletMultiplyer = 1;


    public int currentMunch = 0;

    public int score;
    public TextMeshProUGUI scoreText;

    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;

    public GameObject redGhost;
    public GameObject pinkGhost;
    public GameObject blueGhost;
    public GameObject orangeGhost;

    public EnemyController redGhostController;
    public EnemyController pinkGhostController;
    public EnemyController blueGhostController;
    public EnemyController orangeGhostController;

    public int totalPellets;
    public int pelletsLeft;
    public int pelletsCollectedOnThisLife;

    public bool hadDeathOnThisLevel = false;

    public bool gameIsRunning;

    public List<NodeController> nodeControllers = new List<NodeController>();

    public bool newGame;
    public bool clearedLevel;

    public AudioSource startGameAudio;
    public AudioSource death;

    public int lives;
    public int currentLevel;

    public Image blackBackground;

    public Text gameOverText;

    public AudioSource powerPelletAudio;
    public AudioSource respawningAudio;
    public AudioSource ghostEatenAudio;

    public bool isPowerPelletRunning = false;
    public enum GhostMode
    {
        chase, scatter
    }

    public GhostMode currentGhostMode;


    public int[] ghostModeTimers = new int[] { 7, 20, 7, 20, 5, 20, 5 };
    public int ghostModeTimerIndex;
    public float ghostModeTimer = 0;
    public bool runningTimer;
    public bool completedTimer;


    
    // Start is called before the first frame update
    void Awake()
    {
        newGame = true;
        clearedLevel = false;
        blackBackground.enabled = false;

        redGhostController = redGhost.GetComponent<EnemyController>();
         pinkGhostController = pinkGhost.GetComponent<EnemyController>();
        blueGhostController = blueGhost.GetComponent<EnemyController>();
        orangeGhostController = orangeGhost.GetComponent<EnemyController>();

        ghostNodeStart.GetComponent<NodeController>().isGhostStartingNode = true;

        pacman = GameObject.Find("Player");
        StartCoroutine(Setup());
      //THE COROTUINE CASUSE THE ERROR AND I DONT KNOW WHY
    }

    public IEnumerator Setup()
    {
        ghostModeTimerIndex = 0;
        ghostModeTimer = 0;
        completedTimer = false;
        runningTimer = true;

        gameOverText.enabled = false;
        //If pacman clears a level, a background will appears covering the level, and the game will pause for 0.1 seconds
        if(clearedLevel)
        {
            blackBackground.enabled = true;
            //Activate background
            yield return new WaitForSeconds(0.1f);
        }

        blackBackground.enabled = false;

        pelletsCollectedOnThisLife = 0;
        currentGhostMode = GhostMode.scatter;
        gameIsRunning = false;
        currentMunch = 0;

        float waitTimer = 1f;

       
        if(clearedLevel || newGame)
        {
            pelletsLeft = totalPellets;
             waitTimer = 4f;


            //Pellets will respawn when pacman clears the level or starts a new game
            for (int i = 0; i < nodeControllers.Count; i++)
            {
                nodeControllers[i].RespawnPellet();
            }


        }


        if(newGame)
        {
            startGameAudio.Play();
            score = 0;
            scoreText.text = "Score: " + score.ToString();
            lives = 3;
            currentLevel = 1;
        }


        yield return new WaitForSeconds(.01f);

        pacman.GetComponent<PlayerController>().Setup();

        redGhostController.Setup();
        pinkGhostController.Setup();
        blueGhostController.Setup();
        orangeGhostController.Setup();

        newGame = false;
        clearedLevel = false;
        yield return new WaitForSeconds(waitTimer);

        print("Game should start here");
        StartGame();

    }

    void StartGame()
    {
        gameIsRunning = true;
        siren.Play();
    }

    void StopGame()
    {
        gameIsRunning = false;
        siren.Stop();
        powerPelletAudio.Stop();
        pacman.GetComponent<PlayerController>().Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if(!gameIsRunning)
        {
            return;
        }


        if(redGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning
            || pinkGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning
            || blueGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning
            || orangeGhostController.ghostNodeState == EnemyController.GhostNodeStatesEnum.respawning)
        {
            if(!respawningAudio.isPlaying)
            {
                respawningAudio.Play();
            }

        }
        else
        {
            if(respawningAudio.isPlaying)
            {
                respawningAudio.Stop();
            }
        }

        if(!completedTimer && runningTimer)
        {
            ghostModeTimer += Time.deltaTime;

            if (ghostModeTimer >= ghostModeTimers[ghostModeTimerIndex])
            {
                ghostModeTimer = 0;
                ghostModeTimerIndex++;
                if(currentGhostMode == GhostMode.chase)
                {
                    currentGhostMode = GhostMode.scatter;
                }
                else
                {
                    currentGhostMode = GhostMode.chase;
                }

                if(ghostModeTimerIndex == ghostModeTimers.Length)
                {
                    completedTimer = true;
                    runningTimer = false;
                    currentGhostMode = GhostMode.chase;
                }

            }
        }

        if(isPowerPelletRunning)
        {
            currentPowerPelletTime += Time.deltaTime;
            if(currentPowerPelletTime >= powerPelletTimer)
            {
                isPowerPelletRunning = false;
                currentPowerPelletTime = 0;

                powerPelletAudio.Stop();
                siren.Play();
                powerPelletMultiplyer = 1;
            }
        }

    }


    public void GotPelletFromNodeController(NodeController nodeController)
    {
        nodeControllers.Add(nodeController);

        totalPellets++;
        pelletsLeft++;
    }
    public void AddToScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: "  + score.ToString();

    }

    public IEnumerator CollectedPellet(NodeController nodeController)
    {
        if(currentMunch == 0)
        {
            munch1.Play();
            currentMunch = 1;
        }
        else if(currentMunch == 1)
        {
            munch2.Play();
            currentMunch = 0;
        }

        pelletsLeft--;
        pelletsCollectedOnThisLife++;

        int requireBluePellets = 0;
        int requireOrangePellets = 0;

        if(hadDeathOnThisLevel)
        {
            requireBluePellets = 12;
            requireOrangePellets = 32;
        }
        else
        {
            requireBluePellets = 30;
            requireOrangePellets = 60;
        }

        if(pelletsCollectedOnThisLife >= requireBluePellets && !blueGhost.GetComponent<EnemyController>().leftHomeBefore)
        {
            blueGhost.GetComponent<EnemyController>().readyToLeaveHome = true;

        }

        if (pelletsCollectedOnThisLife >= requireOrangePellets && !orangeGhost.GetComponent<EnemyController>().leftHomeBefore)
        {
            orangeGhost.GetComponent<EnemyController>().readyToLeaveHome = true;
        }

            AddToScore(10);


        //pellets left
        if(pelletsLeft == 0)
        {
            currentLevel++;
            clearedLevel = true;
            StopGame();
            yield return new WaitForSeconds(1);
            StartCoroutine(Setup());

        }
         

        //power pellet
        if(nodeController.isPowerPellet)
        {
            siren.Stop();
            powerPelletAudio.Play();
            isPowerPelletRunning = true;
            currentPowerPelletTime = 0;
          //  powerPelletMultiplyer += 1;

            redGhostController.SetFrightened(true);
            pinkGhostController.SetFrightened(true);
            blueGhostController.SetFrightened(true);
            orangeGhostController.SetFrightened(true);
        }
    }

    public IEnumerator PauseGame(float timeToPause)
    {
        gameIsRunning = false;
        yield return new WaitForSeconds(timeToPause);
        gameIsRunning = true;
    }

    public void GhostEaten()
    {
        ghostEatenAudio.Play();
        AddToScore(400 * powerPelletMultiplyer);
        powerPelletMultiplyer++;
        StartCoroutine(PauseGame(1));
    }

    public IEnumerator playerEaten()
    {
        hadDeathOnThisLevel = true;
        StopGame();
        yield return new WaitForSeconds(1);

        redGhostController.SetVisible(false);
        pinkGhostController.SetVisible(false);
        blueGhostController.SetVisible(false);
        orangeGhostController.SetVisible(false);


        pacman.GetComponent<PlayerController>().Death();
        death.Play();
        yield return new WaitForSeconds(3);

        lives--;

        if(lives <= 0)
        {
            newGame = true;
            //Display gameover text
            gameOverText.enabled = true;
            yield return new WaitForSeconds(3);


        }

        StartCoroutine(Setup());
    }
}
