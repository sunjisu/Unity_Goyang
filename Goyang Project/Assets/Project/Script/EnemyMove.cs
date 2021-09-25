﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D col;

    public GameObject coin;
    public int nextMove;
    public float movePower;

    public GameObject hp_Bar;
    public GameObject canvas;

    RectTransform hpBar;
    Image nowHpBar;

    public GameObject bullet; // 총알

    // 근거리 타격 위치
    public BoxCollider2D meleeArea;


    public float height;
    public bool isDie;
    bool isTracing; // 플레이어가 인식되어 있는가?
    bool isAttack;

    // 플레이어 인식
    GameObject traceTarget;    

    // Enemy state
    public string enemyName;
    public int maxHp;
    public int nowHp;
    public int atkDmg;
    public int atkSpeed;

    void SetEnemyStatus(string _enemyName, int _maxHp, int _atkDmg, int _atkSpeed)
    {
        enemyName = _enemyName;
        maxHp = _maxHp;
        atkDmg = _atkDmg;
        atkSpeed = _atkSpeed;
    }


    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<CapsuleCollider2D>();

      
    }

    private void Start()
    {
        Physics2D.IgnoreLayerCollision(9, 12, true);
        hpBar = Instantiate(hp_Bar, canvas.transform).GetComponent<RectTransform>();
        nowHpBar = hpBar.transform.GetChild(0).GetComponent<Image>();

        StartCoroutine("ChangeMovement");
    }

    private void FixedUpdate()
    {
        Move();
        PlatformCheck();
        AttackRange();
    }

    private void Update()
    {
        Vector3 _HPBarPos =
        Camera.main.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + height, 0));
        hpBar.position = _HPBarPos;
        nowHpBar.fillAmount = (float)nowHp / (float)maxHp;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.gameObject.tag == "Player") // 플레이어가 인식됐을때
        {
            traceTarget = other.gameObject;

            StopCoroutine("ChangeMovement"); 
           
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {

        if (other.gameObject.tag == "Player") // 플레이어가 계속 있을때
        {
            isTracing = true;
            anim.SetBool("isMoving", true);
        }


    }
    void OnTriggerExit2D(Collider2D other)
    {

        if (other.gameObject.tag == "Player") // 플레이어가 인식 범위를 벗어났을때
        {
            isTracing = false;
            StartCoroutine("ChangeMovement");
        }
    }

    // 플레이어 위치에 따라 플레이어를 따라가는 코루틴함수
    IEnumerator ChangeMovement()
    {
        nextMove = Random.Range(-1, 2);

        if (nextMove == 0)
        {
            anim.SetBool("isMoving", false);
        }            
        else
        {
            anim.SetBool("isMoving", true);
        }           

        yield return new WaitForSeconds(3f);

        StartCoroutine("ChangeMovement");
    }

    private void PlatformCheck() // 지면 체크
    {
        // Platform Check
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.5f, rigid.position.y);

        Debug.DrawRay(frontVec, Vector3.down, new Color(0, 2, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 2, LayerMask.GetMask("Platform"));

        if (rayHit.collider == null)
        {
            check();
            Debug.Log("지면이 없습니다");
        }
    }

    public float cooltime;
    private float currenttime;


    void AttackRange()
    {
        if (!isDie)
        {
            float targetRange = 0;

            switch (enemyName)
            {
                case "Sword":
                    targetRange = 1.8f;
                    break;
                case "Gun":                    
                    targetRange = 5;    
                    break;
                case "Boom":
                    break;
                default:
                    break;
            }

            Debug.DrawRay(transform.position, new Vector3(-transform.localScale.x * targetRange, 0, 0), new Color(10, 0, 0));

            RaycastHit2D rayHit = Physics2D.Raycast(transform.position, new Vector3(-transform.localScale.x, 0, 0)
                    , targetRange, LayerMask.GetMask("Player"));

            if (rayHit.collider && !isAttack) // 충돌한게 있고 공격 실행중이 아니라면
            {
                StartCoroutine(AttackCoroutine()); // 공격실행
                               
            }

            

        }

    }

    IEnumerator AttackCoroutine()
    {
        isTracing = false;
        isAttack = true;
        anim.SetBool("Attack", true);

        yield return new WaitForSeconds(0.2f);

        switch (enemyName)
        {
            case "Sword":
                yield return new WaitForSeconds(0.2f);

                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);

                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;
            case "Gun":
                if(!isDie)
                {
                    GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                    Rigidbody2D rigidBullet = instantBullet.GetComponent<Rigidbody2D>();
                    rigidBullet.velocity = transform.right * 5 * transform.localScale.x; // 발사 속도

                    yield return new WaitForSeconds(2f);
                    Destroy(instantBullet, 3f);
                }
                
                break;
            case "Boom":
                break;
            default:
                break;
        }

        yield return new WaitForSeconds(0.2f);

        isTracing = true;
        isAttack = false;
        anim.SetBool("Attack", false);


    }
    

    void Move()
        {
        Vector3 moveVelocity = Vector3.zero;
        string dist = "";
        if (!isDie && !isAttack)
        {

            if (isTracing) // 플레이어가 인식된 상태면
            {
                Vector3 playerPos = traceTarget.transform.position; // 플레이어의 위치를 받음

                if (playerPos.x < transform.position.x)
                {
                    nextMove = -1;
                    dist = "Left";
                }                   
                else if (playerPos.x > transform.position.x)
                {
                    nextMove = 1;
                    dist = "Right";
                }
                    
            }
            else
            {
                if (nextMove == -1)
                    dist = "Left";
                else if (nextMove == 1)
                    dist = "Right";
            }

            if (dist == "Left")
            {
                moveVelocity = Vector3.left;
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (dist == "Right")
            {
                moveVelocity = Vector3.right;
                transform.localScale = new Vector3(-1, 1, 1);
            };

            transform.position += moveVelocity * movePower * Time.deltaTime;
        }

    }
    
    void check()
    {
        nextMove = nextMove * -1;        

        transform.localScale = new Vector3(nextMove, 1, 1);

        StartCoroutine("ChangeMovement");
    }

    public void OnDamaged()
    {       
        nowHp -= 1;

        if(nowHp == 0)
        {
            Die();
        }
       

    }

    void Die()
    {
        nextMove = 0;

        // 죽음 판정
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        anim.SetBool("Die", true);

        isDie = true;

        // spriteRenderer.flipY = true;

        // col.enabled = false;

        //  rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        // 죽으면 코인 떨구기        
        Instantiate(coin, transform.position, transform.rotation);

        Destroy(gameObject, 2f);

        Destroy(hpBar.gameObject, 2f);
    }





}
