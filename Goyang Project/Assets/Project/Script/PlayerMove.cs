using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;

    // 플레이어 스테이터스
    public float maxSpeed;
    public float jumpPower;
    private int attackMotion = 0;

    private bool isGround;

    // 점프 체크
    public Transform jumpPos;
    public float checkRadius;
    public LayerMask islayer;


    private int jumpCnt = 2;

    // 타격 위치
    public Transform pos;
    public Vector2 boxSize;

    // 사운드
    public AudioClip attackSound1;
    public AudioClip attackSound2;
    public AudioClip walkSound;
    public AudioClip jumpSound;

    AudioSource source;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    BoxCollider2D col;
    Animator anim;
    EnemyMove enemymove;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();
        source = GetComponent<AudioSource>();

        enemymove = GetComponent<EnemyMove>();
    }

    // Update is called once per frame



    private void Update()
    {
        isGround = Physics2D.OverlapCircle(jumpPos.position, checkRadius, islayer); // 바닥 확인

        if (isGround)
        {
            anim.SetBool("Jumping", false);
            jumpCnt = 2;
        }


        if (Input.GetButtonDown("Jump") && isGround && jumpCnt > 0)
        {
            rigid.velocity = Vector2.up * jumpPower;

            PlaySound(jumpSound);

            anim.SetBool("Jumping", true);
        }
        if (Input.GetButtonDown("Jump") && !isGround && jumpCnt > 0)
        {
            rigid.velocity = Vector2.up * jumpPower;

            PlaySound(jumpSound);

            anim.SetBool("DoubleJumping", true);
        }
        if (Input.GetButtonUp("Jump"))
        {
            jumpCnt--;
        }



        // power off
        if (Input.GetButtonUp("Horizontal"))
        {

            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f
                , rigid.velocity.y);
        }

        if (Input.GetButton("Horizontal"))
        {
            if (!source.isPlaying && isGround)
            {
                PlaySound(walkSound);
            }
        }

        // 플레이어 회전
        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (Input.GetAxisRaw("Horizontal") > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);           
        }



        if (Input.GetKeyDown(KeyCode.Z))
        {
            Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
            foreach (Collider2D collider in collider2Ds)
            {
                if (collider.tag == "Enemy")
                {
                    collider.GetComponent<EnemyMove>().OnDamaged();
                }
            }

            

            if (attackMotion == 0)
            {
                anim.SetTrigger("Attack1");
                PlaySound(attackSound1);
                attackMotion += 1;
            }
            else if (attackMotion == 1)
            {
                anim.SetTrigger("Attack2");
                PlaySound(attackSound2);
                attackMotion = 0;
            }
        }

        if (Mathf.Abs(rigid.velocity.x) < 0.3)
        {
            // anim.SetBool("Walking", false);
        }
        else
        {
            //anim.SetBool("Walking", true);
        }

    }
    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");

        rigid.velocity = new Vector2(h * 3, rigid.velocity.y);



        if (rigid.velocity.x > maxSpeed)
        {
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        }
        else if (rigid.velocity.x < maxSpeed * (-1))
        {
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);
        }


    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");

            if (isBronze)
            {
                gameManager.stagePoint += 50;
            }
            else if (isSilver)
            {
                gameManager.stagePoint += 100;
            }
            else if (isGold)
            {
                gameManager.stagePoint += 200;
            }


            collision.gameObject.SetActive(false);
        }

        if (collision.gameObject.tag == "Enemy")
        {
            // Attack
            if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
            }                
        }

        

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            GetComponent<CircleCollider2D>().isTrigger = true;
        }

        if (collision.gameObject.tag == "Item")
        {
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");

            if (isBronze)
            {
                gameManager.stagePoint += 50;
            }
            else if (isSilver)
            {
                gameManager.stagePoint += 100;
            }
            else if (isGold)
            {
                gameManager.stagePoint += 200;
            }


            collision.gameObject.SetActive(false);
        }
        else if (collision.gameObject.tag == "Finish")
        {
            gameManager.NextStage();
        }

        if (collision.gameObject.tag == "Enemy Attack") // 플레이어 피격
        {
            OnDamaged(collision.transform.position);

        }

    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            GetComponent<CircleCollider2D>().isTrigger = false;
        }
    }

    private void PlaySound(AudioClip _clip)
    {
        source.clip = _clip;
        source.Play();
    }


    void OnAttack(Transform enemy)
    {
        gameManager.stagePoint += 100;

        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
    }

    void OnDamaged(Vector2 tagetPos)
    {
        gameManager.HealthDown();

        gameObject.layer = 11;

        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        float dirc = (transform.position.x - tagetPos.x > 0 ? 1 : -1); // 양수면 1 아니면 -1

        rigid.velocity = new Vector2(dirc, 1) * 6;
        //  anim.SetTrigger("Damage");

        Invoke("offDamaged", 3);
    }

    void offDamaged()
    {
        gameObject.layer = 10;

        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(pos.position, boxSize);
    }

    public void OnDie()
    {
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        spriteRenderer.flipY = true;

        col.enabled = false;

        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }
}
