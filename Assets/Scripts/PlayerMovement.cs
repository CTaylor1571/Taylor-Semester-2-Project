using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    public static GameObject LocalPlayerInstance;

    AudioManager audioManager;

    [SerializeField]
    private GameObject mainCamera;

    public Healthbar healthbar;
    [SerializeField]
    private float maxHealth;
    private float health;
    public Staminabar staminabar;
    [SerializeField]
    public float maxStamina;
    public float stamina;
    [SerializeField]
    public Image healthBackground;
    [SerializeField]
    public Image staminaBackground;

    private Vector3 moveDirection;

    private CharacterController controller;
    public Transform enemy;
    public bool bothPlayersJoined;

    [SerializeField]
    private GameObject self;
    [SerializeField]
    private GameObject mesh;


    public Animator anim;
    public Rigidbody rb;

    private int attacks;
    private bool attacking;
    private bool attackQueued;

    private bool blocking;
    private bool justBlocked;
    private bool startedBlocking;
    private float blockTimer;

    private bool isHit;

    private bool isIdle;
    private bool isDancing;
    private bool isAlive;
    public bool regenStamina;

    public bool gameOver;
    private bool buttonHeld;
    private float timer;
    TextMeshProUGUI escapeText;

    [SerializeField]
    Material[] materials;


    private void Start()
    {
        gameOver = false;
        bothPlayersJoined = false;
        escapeText = GameObject.Find("Leave Room Text").GetComponent<TextMeshProUGUI>();
        escapeText.text = "";
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        if (!photonView.IsMine)
        {
            mainCamera.SetActive(false);
        }
        if (photonView.IsMine)
        {
            self.SetActive(false);
            PlayerMovement.LocalPlayerInstance = this.gameObject;

            health = maxHealth;
            healthbar.UpdateHealthBar(maxHealth, health);

        }
        stamina = maxStamina;
        staminabar.UpdateStaminaBar(maxStamina, stamina);


        if (!photonView.IsMine)
        {
            staminabar.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            healthbar.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            staminaBackground.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            healthBackground.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            healthBackground.gameObject.SetActive(false);
            healthbar.gameObject.SetActive(false);
        }

        isHit = false;
        isAlive = true;
        isIdle = true;
        isDancing = false;
        attacks = 0;
        attacking = false;
        attackQueued = false;
        blocking = false;
        justBlocked = false;



        rb = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();

        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("EnteredRoom", RpcTarget.All, "jup", "and jup.");
            bothPlayersJoined = true;
            enemy = GameObject.Find("Enemy").GetComponent<Transform>();
        }


        if ((photonView.IsMine && PhotonNetwork.IsMasterClient) || (!photonView.IsMine && !PhotonNetwork.IsMasterClient))    // set to correct color
        {
            mesh.GetComponent<SkinnedMeshRenderer>().material = materials[StaticScript.hostColor];
            Debug.Log("Setting host's color to " + StaticScript.hostColor);
        }
        else
        {
            mesh.GetComponent<SkinnedMeshRenderer>().material = materials[StaticScript.guestColor];
            Debug.Log("Setting guest's color to " + StaticScript.guestColor);
        }
        if (photonView.IsMine && !PhotonNetwork.IsMasterClient)                                                     // I'm not quite sure why this is required but it works now so I won't think about it more
        {
            Debug.Log("Is guest. Setting guest's color to " + StaticScript.guestColor);
            mesh.GetComponent<SkinnedMeshRenderer>().material = materials[StaticScript.guestColor];
        }
    }



    private void OnHit()
    {

        health -= 20;
        healthbar.UpdateHealthBar(maxHealth, health);

        if (health <= 0)
        {
            staminabar.UpdateStaminaBar(maxStamina, 0);
            Die();
        }
        else
        {
            attacking = false;
            attacks = 0;
            isHit = true;
            this.anim.SetBool("IsHit", true);  // test animation
            StartCoroutine("ResetHit");
            StartCoroutine("StopStaminaRegen");
            stamina += 30f;
            if (stamina > maxStamina)
            {
                stamina = maxStamina;
            }
            staminabar.UpdateStaminaBar(maxStamina, stamina);
            isDancing = false;
            this.anim.SetLayerWeight(1, 1);
        }
    }

    IEnumerator ResetHit()
    {
        yield return new WaitForSeconds(0.1f);
        this.anim.SetBool("IsHit", false);
        yield return new WaitForSeconds(0.3f);
        isHit = false;
    }






    [PunRPC]
    void EnteredRoom(string a, string b)
    {

        enemy = GameObject.Find("Enemy").GetComponent<Transform>();                                // throws error a lot but doesn't break anything
        if (enemy != null)
        {
            bothPlayersJoined = true;
        }
        if (photonView.IsMine && !PhotonNetwork.IsMasterClient)                                                     // I'm not quite sure why this is required but it works now so I won't think about it more
        {
            Debug.Log("Is guest. Setting guest's color to " + StaticScript.guestColor);
            mesh.GetComponent<SkinnedMeshRenderer>().material = materials[StaticScript.guestColor];
        }
        Debug.Log("RPC received and successfully set");

    }

    [PunRPC]
    void PlayerDied(string winNum, string loseNum, int rand)
    {
        this.anim.SetLayerWeight(1, 0);
        if (rand < 1)
        {
            this.anim.SetTrigger("Die1");
        }
        else
        {
            this.anim.SetTrigger("Die2");
        }
        StaticScript.gameOver = true;
        StaticScript.winnerID = winNum;
        StaticScript.loserID = loseNum;
        Debug.Log("Winning player's actor number: " + winNum);
        gameOver = true;
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine("GameOverCountdown");
        }
    }
    IEnumerator GameOverCountdown()
    {
        gameOver = true;
        yield return new WaitForSeconds(4);
        PhotonNetwork.LoadLevel("WinScreen");
    }

    private void Update()
    {

        if (!isAlive)
        {
            return;
        }

        if (photonView.IsMine)
        {
            if (bothPlayersJoined)
            {
                transform.LookAt(enemy);
            }
            else
            {
                try
                {
                    enemy = GameObject.Find("Enemy").GetComponent<Transform>();

                    if (enemy != null)
                    {
                        bothPlayersJoined = true;

                        if (photonView.IsMine && !PhotonNetwork.IsMasterClient)                                                     // I'm not quite sure why this is required but it works now so I won't think about it more
                        {
                            Debug.Log("Is guest. Setting guest's color to " + StaticScript.guestColor);
                            mesh.GetComponent<SkinnedMeshRenderer>().material = materials[StaticScript.guestColor];
                        }

                    }
                }
                catch
                {
                    return;
                }

            }

            Dance();
            Move();
            Attack();
            Block();
            RegenStamina();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                escapeText.text = "Hold ESC to leave game";
                buttonHeld = true;
            }
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                escapeText.text = "";
                buttonHeld = false;
                timer = 0;
            }
            if (buttonHeld)
            {
                timer += Time.deltaTime;
            }
            if (timer >= 3f)
            {
                LeaveRoom();
            }
        }
    }

    public bool CostStamina(float cost)
    {

        if (stamina < cost)
        {
            Debug.Log("CostStamina returned false");
            return false;
        }
        stamina -= cost;
        if (stamina < 0)
        {
            stamina = 0;
        }
        Debug.Log("Stamina cost: " + cost + ". New stamina value: " + stamina);
        staminabar.UpdateStaminaBar(maxStamina, stamina);
        return true;
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(stamina);
            stream.SendNext(moveSpeed);                                                                 // use movespeed to determine speed of footstep playback???
        }
        else
        {
            this.stamina = (float)stream.ReceiveNext();
            this.moveSpeed = (float)stream.ReceiveNext();
        }
    }   



    private void RegenStamina()
    {
        if (regenStamina && moveSpeed != runSpeed && stamina < maxStamina && isAlive)
        {
            stamina += Time.deltaTime*3.75f;
            if (blocking)
            {
                stamina += Time.deltaTime*1.55f;
            }
            staminabar.UpdateStaminaBar(maxStamina, stamina);
        }

    }

    int numStopsQueued = 0;
    IEnumerator StopStaminaRegen()
    {
        numStopsQueued++;
        regenStamina = false;
        yield return new WaitForSeconds(1.8f);
        if (numStopsQueued == 1)
        {
            regenStamina = true;
        }
        numStopsQueued--;
    }


    private void Move()
    {
        if (StaticScript.gameOver)
        {
            return;
        }
        float moveZ = Input.GetAxis("Vertical");
        float moveX = Input.GetAxis("Horizontal");

        moveDirection = new Vector3(moveX, 0, moveZ);
        moveDirection = transform.TransformDirection(moveDirection);
        
    

        if (moveDirection != Vector3.zero && !Input.GetKey(KeyCode.LeftShift))
        {
            Walk();
            this.anim.SetFloat("Straight", moveZ * moveSpeed);
            this.anim.SetFloat("Sideways", moveX * moveSpeed);
        }
        else if (moveDirection != Vector3.zero && Input.GetKey(KeyCode.LeftShift))
        {
            Run();
            this.anim.SetFloat("Straight", moveZ * moveSpeed);
            this.anim.SetFloat("Sideways", moveX * moveSpeed);
        }
        else if (moveDirection == Vector3.zero)
        {
            this.anim.SetFloat("Straight", 0);
            this.anim.SetFloat("Sideways", 0);
        }

        if (!(attacking || blocking || startedBlocking) && moveDirection == Vector3.zero)
        {
            isIdle = true;
        } else
        {
            isIdle = false;
        }


        if (moveZ < 0)
        {
            moveSpeed *= 2;
            moveSpeed /= 3;
        }

        moveDirection *= moveSpeed;
        controller.Move(moveDirection * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, 1.5f, transform.position.z);
    }

    private void Walk()
    {
        if (attacking || blocking || startedBlocking)
        {
            moveSpeed = walkSpeed/2f;
        } else
        {
            moveSpeed = walkSpeed;
        }
        if (justBlocked || isHit)
        {
            moveSpeed = 0;
        }
    }

    private void Run()
    {
        if (attacking || blocking || startedBlocking)
        {
            moveSpeed = walkSpeed/2f;
        }else
        {
            moveSpeed = runSpeed;
        }
        if (justBlocked || isHit)
        {
            moveSpeed = 0;
        }
        if (regenStamina && !blocking)
        {
            StartCoroutine("StopStaminaRegen");
        }
    }

    private void Attack()
    {
        if (Input.GetMouseButtonDown(0) && !blocking && !isDancing && !isHit)           //when the user presses the left mouse button
        {
            SwordStrike();
        }
        if (Input.GetMouseButtonDown(0) && isDancing)
        {
            isDancing = false;
            this.anim.SetLayerWeight(1, 1);
        }

        this.anim.SetInteger("Attacks", attacks);
        this.anim.SetBool("Attacking", attacking);
    }

    private void Block()
    {
        if (blocking && blockTimer < 2.4f)
        {
            blockTimer += Time.deltaTime;
        }
        if (Input.GetMouseButtonDown(1) && !isHit)           //when the user presses the right mouse button
        {
            if (CostStamina(5f)){
                StartCoroutine("StopStaminaRegen");
                StartCoroutine("OnBlockStart");
                blocking = true;
                attacking = false;
                attackQueued = false;
                attacks = 0;
                isDancing = false;
            }
            else
            {
                Debug.Log("Failed posture requirement to block");
            }

        }
        if (Input.GetMouseButtonUp(1))           //when the user stops pressing the right mouse button
        {
            if (blocking)
            {
                blocking = false;
                blockTimer = 0;
            }
        }
        this.anim.SetBool("Blocking", blocking);

        if (Input.GetMouseButtonDown(2) && blocking)                   // TESTING ONLY, DELETE LATER
        {
            StartCoroutine("OnHitBlocked");
        }
        if (Input.GetKeyDown(KeyCode.M) && !StaticScript.gameOver)                               // ALSO TESTING, BUT MAYBE LEAVE IN FOR FUNNY
        {
            OnHit();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            stamina -= 25;
            staminabar.UpdateStaminaBar(maxStamina, stamina);
            StartCoroutine("StopStaminaRegen");
            Debug.Log("Stamina: " + stamina);
        }
    }

    private void Dance()
    {
        if (isIdle && Input.GetKeyDown(KeyCode.Alpha1))
        {
            this.anim.SetLayerWeight(1, 0);
            isDancing = true;
            Debug.Log("Doing chicken dance");
            this.anim.SetTrigger("StartDance1");
            StartCoroutine("ResetTriggers");
            StartCoroutine("StopStaminaRegen");
        }
        if (isIdle && Input.GetKeyDown(KeyCode.Alpha2))
        {
            this.anim.SetLayerWeight(1, 0);
            isDancing = true;
            Debug.Log("Doing silly dance");
            this.anim.SetTrigger("StartDance2");
            StartCoroutine("ResetTriggers");
            StartCoroutine("StopStaminaRegen");
        }
        if (isIdle && Input.GetKeyDown(KeyCode.Alpha3))
        {
            this.anim.SetLayerWeight(1, 0);
            isDancing = true;
            Debug.Log("Doing hiphop dance");
            this.anim.SetTrigger("StartDance3");
            StartCoroutine("ResetTriggers");
            StartCoroutine("StopStaminaRegen");
        }
        if (isIdle && Input.GetKeyDown(KeyCode.Alpha4))
        {
            this.anim.SetLayerWeight(1, 0);
            isDancing = true;
            Debug.Log("Doing spin dance");
            this.anim.SetTrigger("StartDance4");
            StartCoroutine("ResetTriggers");
            StartCoroutine("StopStaminaRegen");
        }
        if (attacking || blocking || startedBlocking || !isIdle)
        {
            if (isDancing)
            {
                this.anim.SetLayerWeight(1, 1);
                isDancing = false;
            }
        }
        this.anim.SetBool("isDancing", isDancing);
    }

    IEnumerator ResetTriggers()
    {
        yield return new WaitForSeconds(0.5f);

        this.anim.ResetTrigger("StartDance1");
        this.anim.ResetTrigger("StartDance2");
        this.anim.ResetTrigger("StartDance3");
        this.anim.ResetTrigger("StartDance4");
    }






    IEnumerator OnBlockStart()
    {
        this.anim.SetLayerWeight(1, 1);
        isDancing = false;
        this.anim.SetTrigger("StartBlock");
        startedBlocking= true;
        yield return new WaitForSeconds(0.2f);
        this.anim.ResetTrigger("StartBlock");
        startedBlocking = false;
    }
    IEnumerator OnHitBlocked()
    {
        if (CostStamina(blockTimer * 5f))
        {
            StartCoroutine("StopStaminaRegen");
            this.anim.SetTrigger("BlockHit");
            justBlocked = true;
            yield return new WaitForSeconds(0.4f);
            this.anim.ResetTrigger("BlockHit");
            justBlocked = false;
            float postureDamage = (2.4f - blockTimer) * 2.5f;
            if (photonView.IsMine)
            {
                photonView.RPC("BlockedHitStaminaCost", RpcTarget.All, postureDamage, PhotonNetwork.IsMasterClient);  // the more perfectly you block, the more the other player's posture cost
            }
        }
        else
        {
            OnHit();
            yield return null;
        }

    }




    private void SwordStrike()                            // maybe make it so this inches up the capsulecollider hitbox to match visual appearance?
    {
        if (attacking && attacks < 3)
        {
            //Queue attack
            attackQueued = true;
            Debug.Log("Queuing attack: " + (attacks + 1));
        }else if (attacks < 3)
        {
            if (CostStamina(5f))
            {
                attacks++;
                Debug.Log("Attacking: " + attacks);
                attacking = true;
                StartCoroutine("AttackLogic");
                StartCoroutine("StopStaminaRegen");
                StartCoroutine("PlayerAttacked", attacks);                 // see if this works fine
            }
            else
            {
                attacks = 0;
                attackQueued = false;
                Debug.Log("Attacks reset due to lack of posture requirement.");
            }
            
        }
    }



    [PunRPC]
    void BlockedHitStaminaCost(float amount, bool isHost)     // After a week of trying, this works. NEVER TOUCH IT AGAIN.
    {
        if (isHost != PhotonNetwork.IsMasterClient)
        {
            PlayerMovement script = GameObject.Find("Background").GetComponentInParent<PlayerMovement>(); // lesson for self: RPC calls are still just on the script of your player on the other client, not the other player on the other client
            Debug.Log("My hit was blocked, cost: " + amount);
            if (script.stamina < amount)
            {
                script.stamina = 0;
                script.CostStamina(0);
            }
            else
            {
                script.CostStamina(amount);
                script.StartCoroutine("StopStaminaRegen");
            }
        }
    }


    IEnumerator PlayerAttacked(int numAttack)
    {
        float attackLength = 4.2f;
        if (numAttack == 1)
        {
            yield return new WaitForSeconds(0.8f);
        }
        else if (numAttack == 2)
        {
            yield return new WaitForSeconds(0.8f);
            attackLength = 5f;
        }
        else
        {
            yield return new WaitForSeconds(0.98f);
        }

        // check something that gets reset if the player is hit
        if (attacking)
        {
            photonView.RPC("OtherAttacked", RpcTarget.Others, attackLength);
        }

    }

    [PunRPC]
    void OtherAttacked(float attackLength)
    {
        Debug.Log("Other player has swung");
        PlayerMovement script = GameObject.Find("Background").GetComponentInParent<PlayerMovement>();

        Debug.Log("Check if other player has hit me");
        RaycastHit objectHit;
        int layerMask = 1 << 6;
        Vector3 position = new Vector3(rb.position.x, rb.position.y + 3f, rb.position.z);
        if (Physics.Raycast(position, transform.TransformDirection(Vector3.forward), out objectHit, attackLength, layerMask))
        {
            Debug.DrawRay(position, transform.TransformDirection(Vector3.forward) * attackLength, Color.green, 0.8f);
            Debug.Log("I have been hit");

            if (script.blocking)
            {
                script.StartCoroutine("OnHitBlocked");
            }
            else
            {
                script.OnHit();
            }

        }
        else
        {
            Debug.DrawRay(position, transform.TransformDirection(Vector3.forward) * attackLength, Color.red, 0.8f);
        }


    }





    private IEnumerator AttackLogic()
    {
        if (attacks == 3)
        {
            yield return new WaitForSeconds(1.33f);
            attacks = 0;
            Debug.Log("Attacks reset");
            yield return null;
        }
        yield return new WaitForSeconds(1.01f);
        attacking = false;
        if (attackQueued && !blocking &&isAlive)
        {
            SwordStrike();
            attackQueued = false;
        }
        else
        {
            attacks = 0;
            Debug.Log("Attacks reset");
        }
    }


    private void Die()
    {
        string win;
        string lose;
        if (PhotonNetwork.IsMasterClient)
        {
            lose = StaticScript.hostUserID;
            win = StaticScript.guestUserID;
        }
        else
        {
            win = StaticScript.hostUserID;
            lose = StaticScript.guestUserID;
        }
        photonView.RPC("PlayerDied", RpcTarget.All, win, lose, Random.Range(0, 2));
        blocking = false;
        attacking = false;
        attackQueued = false;
        isDancing = false;
        isAlive = false;
        StartCoroutine("DieNumerator");
    }

    IEnumerator DieNumerator()
    {
        yield return new WaitForSeconds(1f);
        transform.Translate(Vector3.down * 0.5f);
    }

    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        Debug.Log("Other player has left room");
        StaticScript.gameOver = false;
        StaticScript.hostUserID = "";
        StaticScript.guestUserID = "";
        StaticScript.hostColor = -1;
        StaticScript.guestColor = -1;
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("ConnectScreen");
    }

    void LeaveRoom()
    {
        StaticScript.gameOver = false;
        StaticScript.hostUserID = "";
        StaticScript.guestUserID = "";
        StaticScript.hostColor = -1;
        StaticScript.guestColor = -1;
        Debug.Log("Leaving room");
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("ConnectScreen");
    }

}
