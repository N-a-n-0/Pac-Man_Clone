using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public enum GhostNodeStatesEnum
    {
        respawning,
        leftNode,
        rightNode,
        centerNode,
        startNode,
        movingInNodes
    }

    public GhostNodeStatesEnum ghostNodeState;
    public GhostNodeStatesEnum startGhostNodeState;
    public GhostNodeStatesEnum respawnState;
    public enum GhostType
    {
        red,
        blue,
        pink,
        orange
    }

    public GhostType ghostType;
   

    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;

  

    public MovementController movementController;

    public GameObject startingNode;

    public bool readyToLeaveHome = false;

    public GameManager gameManager;

    public bool testRespawn = false;

    public bool isFrightened = false;

    public GameObject[] scatterNodes;
    public int scatterNodeIndex;

    public bool leftHomeBefore = false;

    public bool isVisible = true;

    public SpriteRenderer ghostSprite;
    public SpriteRenderer eyesSprite;
    // Start is called before the first frame update
    void Awake()
    {
        ghostSprite = GetComponent<SpriteRenderer>();
       

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        movementController = GetComponent<MovementController>();

        if (ghostType == GhostType.red)
        {
            startGhostNodeState = GhostNodeStatesEnum.startNode;

            respawnState = GhostNodeStatesEnum.centerNode;
            startingNode = ghostNodeStart;
           
        }
        else if(ghostType == GhostType.pink)
        {
            startGhostNodeState = GhostNodeStatesEnum.centerNode;
            startingNode = ghostNodeCenter;
            respawnState = GhostNodeStatesEnum.centerNode;
            
        }
        else if (ghostType == GhostType.blue)
        {
            startGhostNodeState = GhostNodeStatesEnum.leftNode;

            respawnState = GhostNodeStatesEnum.leftNode;
            startingNode = ghostNodeLeft;
        }
        else if (ghostType == GhostType.orange)
        {
            startGhostNodeState = GhostNodeStatesEnum.rightNode;

            respawnState = GhostNodeStatesEnum.rightNode;
            startingNode = ghostNodeRight;
        }
         
        
    
    }


    public void Setup()
    {
        ghostNodeState = startGhostNodeState;
        readyToLeaveHome = false;
        //Reset our ghosts back to their home position
        movementController.currentNode = startingNode; //error happens here 
        transform.position = startingNode.transform.position;

        movementController.direction = "";
        movementController.lastMovingDirection = "";

        //Set their scatter node index back to 0
        scatterNodeIndex = 0;
        //Set isFrightened
        isFrightened = false;

        leftHomeBefore = false;

        //Set readyToLeaveHome to be false if they are blue or pink
        if(ghostType == GhostType.red)
        {
            readyToLeaveHome = true;
            leftHomeBefore = true;
        }
        else if(ghostType == GhostType.pink)
        {
            readyToLeaveHome = true;
        }
        SetVisible(true);
    }

    // Update is called once per frame
    void Update()
    {
        //Show our sprites
        if(isVisible)
        {
            ghostSprite.enabled = true;
            eyesSprite.enabled = true;
        }
        //Hide our sprites
        else
        {
            ghostSprite.enabled = false;
            eyesSprite.enabled = false;
        }

        if(!gameManager.gameIsRunning)
        {
            return;
        }

        if(testRespawn == true)
        {
            readyToLeaveHome = false;
            ghostNodeState = GhostNodeStatesEnum.respawning;
            testRespawn = false;
        }

        if(movementController.currentNode.GetComponent<NodeController>().isSlideNode)
        {
            movementController.SetSpeed(1);
        }    
        else
        {
            movementController.SetSpeed(1);
        }
    }



    public void ReachedCenterOfNode(NodeController nodeController)
    {
        if(ghostNodeState == GhostNodeStatesEnum.movingInNodes)
        {
            leftHomeBefore = true;
            //Scatter mode
            if (gameManager.currentGhostMode == GameManager.GhostMode.scatter)
            {

                DetermineGhostScatterModeDirection();
              
            }
            //Frightened mode
            else if (isFrightened)
            {
                string direction = GetRandomDirection();
                movementController.SetDirection(direction);
            }
            else
            {
                //Determine next game node to go to
                if (ghostType == GhostType.red)
                {
                    DetermineRedGhostDirection();
                }
                else if (ghostType == GhostType.pink)
                {
                    DeterminePinkGhostDirection();
                }
                else if(ghostType == GhostType.blue)
                {
                    DetermineBlueGhostDirection();
                }
                else if(ghostType == GhostType.orange)
                {
                    DetermineOrangeGhostDirection();
                }

               
            }


        }
        else if(ghostNodeState == GhostNodeStatesEnum.respawning)
        {
            string direction = "";

            //We have reached our start node, move to the center node
            if(transform.position.x == ghostNodeStart.transform.position.x && transform.position.y == ghostNodeStart.transform.position.y)
            {
                direction = "down";
            }
            //We have reached our center node, either finish respawn, or move to the left/right node
            else if(transform.position.x == ghostNodeCenter.transform.position.x && transform.position.y == ghostNodeCenter.transform.position.y)
            {
                if(respawnState == GhostNodeStatesEnum.centerNode)
                {
                    ghostNodeState = respawnState;
                }
                else if(respawnState == GhostNodeStatesEnum.leftNode)
                {
                    direction = "left";
                }
                else if (respawnState == GhostNodeStatesEnum.rightNode)
                {
                    direction = "right";
                }

            }
            //If our respawn state is either the left or right node, and we got to that node, leave homee again
            else if((transform.position.x == ghostNodeLeft.transform.position.x && transform.position.y == ghostNodeLeft.transform.position.y)
                || (transform.position.x == ghostNodeRight.transform.position.x && transform.position.y == ghostNodeRight.transform.position.y)
                )
            {
                ghostNodeState = respawnState;
            }
          else
          //We are in the gameboarrd still, locate our start node
            {
                //Determine quickest direction to home 
                direction = GetClosestDirection(ghostNodeStart.transform.position);
            }
            
           //  direction = GetClosestDirection(ghostNodeStart.transform.position);
            movementController.SetDirection(direction);
        }
        else
        {
            //if we are ready to leave our home 
            if(readyToLeaveHome)
            {
                //we are in the left home node then move to center
                if(ghostNodeState == GhostNodeStatesEnum.leftNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.centerNode;
                    movementController.SetDirection("right");
                }

                //we are in the right home node then move to center
                else if (ghostNodeState == GhostNodeStatesEnum.rightNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.centerNode;
                    movementController.SetDirection("left");
                }
                //we are in the center go to start node
                else if(ghostNodeState == GhostNodeStatesEnum.centerNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.startNode;
                    movementController.SetDirection("up");
                }
                //we are in the start node begin moving around the game
                else if(ghostNodeState == GhostNodeStatesEnum.startNode)
                {
                    ghostNodeState = GhostNodeStatesEnum.movingInNodes;
                    movementController.SetDirection("left");
                }
            }
        }
    }

    string GetRandomDirection()
    {
        List<string> possibleDirections = new List<string>();
        NodeController nodeController = movementController.currentNode.GetComponent<NodeController>();

        if (nodeController.canMoveDown && movementController.lastMovingDirection != "up")
        {
            possibleDirections.Add("down");
        }
         if (nodeController.canMoveDown && movementController.lastMovingDirection != "down")
        {
            possibleDirections.Add("up");
        }
         if (nodeController.canMoveRight && movementController.lastMovingDirection != "left")
        {
            possibleDirections.Add("right");
        }
         if (nodeController.canMoveLeft && movementController.lastMovingDirection != "right")
           {
            possibleDirections.Add("left");
          }

        string direction = " ";

        int randomDirectionIndex = Random.Range(0, possibleDirections.Count - 1);
        direction = possibleDirections[randomDirectionIndex];
        return direction;
    }

    void DetermineGhostScatterModeDirection()
    {
        //If we reached the scatter node, add on to our scatter node index
        if (transform.position.x == scatterNodes[scatterNodeIndex].transform.position.x && transform.position.y == scatterNodes[scatterNodeIndex].transform.position.y)
        {
            scatterNodeIndex++;

            if (scatterNodeIndex == scatterNodes.Length - 1)
            {
                scatterNodeIndex = 0;
            }
        }

        string direction = GetClosestDirection(scatterNodes[scatterNodeIndex].transform.position);
        movementController.SetDirection(direction);
    }
    public void  DetermineRedGhostDirection()
    {
        string direction = GetClosestDirection(gameManager.pacman.transform.position);
        movementController.SetDirection(direction);
    }

    public void DeterminePinkGhostDirection()
    {
        string pacmansDirection = gameManager.pacman.GetComponent<MovementController>().lastMovingDirection;
        float distanceBetweenNode = 0.35f;

        Vector2 target = gameManager.pacman.transform.position;
        if(pacmansDirection == "left")
        {
            target.x = target.x - (distanceBetweenNode * 2);
        }
        else if(pacmansDirection == "right")
        {
            target.x += (distanceBetweenNode * 2);
        }
        else if(pacmansDirection == "up")
        {
            target.y += (distanceBetweenNode * 2);
        }
        else if (pacmansDirection == "down")
        {
            target.y -= distanceBetweenNode * 2;
        }

        string direction = GetClosestDirection(target);
        movementController.SetDirection(direction);

    }

    public void DetermineBlueGhostDirection()
    {
        string pacmansDirection = gameManager.pacman.GetComponent<MovementController>().lastMovingDirection;
        float distanceBetweenNode = 0.35f;

        Vector2 target = gameManager.pacman.transform.position;
        if (pacmansDirection == "left")
        {
            target.x = target.x - (distanceBetweenNode * 2);
        }
        else if (pacmansDirection == "right")
        {
            target.x += (distanceBetweenNode * 2);
        }
        else if (pacmansDirection == "up")
        {
            target.y += (distanceBetweenNode * 2);
        }
        else if (pacmansDirection == "down")
        {
            target.y -= distanceBetweenNode * 2;
        }
        GameObject redGhost = gameManager.redGhost;
        float xDistance = target.x - redGhost.transform.position.x;
        float yDistance = target.y - redGhost.transform.position.y;

        Vector2 blueTarget = new Vector2(target.x + xDistance, target.y + yDistance);
        string direction = GetClosestDirection(blueTarget);


        //string direction = GetClosestDirection(target);
        movementController.SetDirection(direction);
    }
    public void DetermineOrangeGhostDirection()
    {
        float distance =  Vector2.Distance(gameManager.pacman.transform.position, transform.position);
        float distanceBetweenNodes = 0.35f;

        if(distance < 0)
        {
            distance *= -1;
        }

        //if we are within 8 nodes of pacman chase him using reds logic
        if(distance <= distanceBetweenNodes)
        {
            DetermineRedGhostDirection();
        }
        //otherwise use scatter logic
        else
        {
            //Scatter mode
            DetermineGhostScatterModeDirection();
        }
    }

    string GetClosestDirection(Vector2 target)
    {
        float shortestDistance = 0;
        string lastMovingDirection = movementController.lastMovingDirection;
        string newDirection = "";

        NodeController nodeController = movementController.currentNode.GetComponent<NodeController>();

        //if we can move up and we are not reversing 
        if(nodeController.canMoveUp && lastMovingDirection != "down")
        {
            //Get the node above us 
            GameObject nodeUp = nodeController.nodeUp;
            //Get the distance between our top node, and pacman
            float distance = Vector2.Distance(nodeUp.transform.position, target);

            //this is the shortest distance so far, set our direction
            if(distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "up";
            }
        }

        if (nodeController.canMoveDown && lastMovingDirection != "up")
        {
            //Get the node above us 
            GameObject nodeDown = nodeController.nodeDown;
            //Get the distance between our top node, and pacman
            float distance = Vector2.Distance(nodeDown.transform.position, target);

            //this is the shortest distance so far, set our direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "down";
            }
        }


        if (nodeController.canMoveLeft && lastMovingDirection != "right")
        {
            //Get the node above us 
            GameObject nodeLeft = nodeController.nodeLeft;
            //Get the distance between our top node, and pacman
            float distance = Vector2.Distance(nodeLeft.transform.position, target);

            //this is the shortest distance so far, set our direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "left";
            }
        }

        if (nodeController.canMoveRight && lastMovingDirection != "left")
        {
            //Get the node above us 
            GameObject nodeRight = nodeController.nodeRight;
            //Get the distance between our top node, and pacman
            float distance = Vector2.Distance(nodeRight.transform.position, target);

            //this is the shortest distance so far, set our direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "right";
            }
        }

        return newDirection;
    }

    public void SetVisible(bool newIsVisible)
    {
        isVisible = newIsVisible;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            //Get eaten
            if(isFrightened)
            {

            }
            //Eat Player 
            else
            {
                StartCoroutine(gameManager.playerEaten());
            }
        }
    }
}
