using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CharaMove : MonoBehaviour
{
    [SerializeField] GameManager gameManager;

    public AudioClip jumpSE;
    public AudioClip voiceSE;
    public AudioClip moveSE;
    public AudioClip slideSE;
    public AudioClip hitSE;


    public AudioSource a1;
    public AudioSource a2;
    public AudioSource a3;
    public AudioSource a4;


    Vector3 firstPressPosition;
    Vector3 secondPressPosition;
    Vector3 currentSwipePosition;
    public float detectionSensitivBottom = -0.8f;
    public float detectionSensitivUp = 0.8f;

    private Animator p_Animator;

    public void SE1()
    {
        a1.PlayOneShot(jumpSE);
        a1.PlayOneShot(moveSE);
    }
    public void SE2()
    {
        a2.PlayOneShot(slideSE);
    }
    public void SE3()
    {
        a3.PlayOneShot(hitSE);
    }

    public void SE4()
    {
        a4.PlayOneShot(voiceSE);
    }

    public void SwipeInput()
    {
    #if UNITY_EDITOR || UNITY_STANDALONE
        // PC用（マウス操作）
        SwipeWithMouth();
    #elif UNITY_IOS || UNITY_ANDROID
       // スマホ用（タッチ操作）
        SwipeWithTouch();
    #endif
    }

    //  PC用（マウススワイプ）
    public void SwipeWithMouth()
    {
        if (Input.GetMouseButtonDown(0))
        {
            firstPressPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
        }
        if (Input.GetMouseButtonUp(0))
        {
            secondPressPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
            currentSwipePosition = secondPressPosition - firstPressPosition;
            currentSwipePosition.Normalize();

            DetectSwipe();
        }
    }

    //  スマホ用（タッチスワイプ）
    public void SwipeWithTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                firstPressPosition = new Vector3(t.position.x, t.position.y);
            }
            else if (t.phase == TouchPhase.Ended)
            {
                secondPressPosition = new Vector3(t.position.x, t.position.y);
                currentSwipePosition = secondPressPosition - firstPressPosition;
                currentSwipePosition.Normalize();

                DetectSwipe();
            }
        }
    }

    //  共通のスワイプ判定処理
    private void DetectSwipe()
    {
        if (currentSwipePosition.y > 0 && currentSwipePosition.x > detectionSensitivBottom && currentSwipePosition.x < detectionSensitivUp)
            Jump();

        if (currentSwipePosition.x < 0 && currentSwipePosition.y > detectionSensitivBottom && currentSwipePosition.y < detectionSensitivUp && transform.position.x > -2)
            MoveToLeft();

        if (currentSwipePosition.x > 0 && currentSwipePosition.y > detectionSensitivBottom && currentSwipePosition.y < detectionSensitivUp)
            MoveToRight();

        if (currentSwipePosition.y < 0 && currentSwipePosition.x > detectionSensitivBottom && currentSwipePosition.x < detectionSensitivUp)
        {
            Roling();
            moveDirection.y = -10f; // スライディング時下に加速
        }
    }


    //レーンの移動の数値をそれぞれの変数で宣言します。
    const int MinLane = -1;
    const int MaxLane = 1;
    const float LaneWidth = 5.0f;
    const int DefaultLife = 1;
    const float StunDuration = 0.5f;

    //CharacterController型を変数controllerで宣言します。
    CharacterController controller;
    //Animator型を変数animatorで宣言します。
    Animator animator;

    
    //それぞれの座標を０で宣言します。
    Vector3 moveDirection = Vector3.zero;
    //int型を変数targetLaneで宣言します。
    int targetLane;
    int life = DefaultLife;
    
    //それぞれのパラメーターの設定をInspectorで変える様にします。
    public float gravity;
    public float speedZ;
    public float speedX;
    public float speedRoling;
    public float speedJump;
    public float accelerationZ;
    public float time;
    public float jumpMotion;

    private const string ps_JumpMotion = "jumpMotion";

    //ライフを取得する関数

    public int Life ()
    { 
        return life;
    
    }

    public bool IsStan ()
    {
        return life <= 0;

    }

    void Start()
    {
        //CharacterController コンポーネントを取得して変数 controller に入れる
        controller = GetComponent<CharacterController>();
        //Animator コンポーネントを取得して変数 animator に入れる
        animator = GetComponent<Animator>();

        p_Animator = this.GetComponent<Animator>();
    }

    void Update()
    {
        p_Animator.SetFloat(ps_JumpMotion, jumpMotion);

        // スワイプ入力
        SwipeInput();

        // CharacterController のサイズ更新
        controller.height = animator.GetFloat("ColliderHeight");
        controller.center = new Vector3(controller.center.x, animator.GetFloat("ColliderCenter"), controller.center.z);
        controller.radius = animator.GetFloat("ColliderRadius");

        // 3秒おきに加速
        time += Time.deltaTime;
        if (Mathf.Floor(time) % 3 == 0)
        {
            // speedZ が 80f 以下の時だけ加速
            if (speedZ < 80f)
            {
                speedZ += 0.03f;
                speedJump += 0.02f;
                gravity += 0.1f;
                jumpMotion += 0.0005f;
            }
        }

        

        // 気絶処理
        if (IsStan())
        {
            moveDirection.x = 0.0f;
            moveDirection.z = 0.0f;
           
        }
        else
        {
            // Z方向加速
            float acceleratedZ = moveDirection.z + (accelerationZ * Time.deltaTime);
            moveDirection.z = Mathf.Clamp(acceleratedZ, 0, speedZ);

            // X方向：レーン移動
            float ratioX = (targetLane * LaneWidth - transform.position.x) / LaneWidth;
            moveDirection.x = ratioX * speedX;
        }

        // 重力処理
        moveDirection.y -= gravity * Time.deltaTime;

        // 移動実行
        Vector3 globalDirection = transform.TransformDirection(moveDirection);
        controller.Move(globalDirection * Time.deltaTime);

        if (controller.isGrounded) moveDirection.y = 0;

        // アニメーション更新
        animator.SetBool("run", moveDirection.z > 0.0f);
    }



    //新しく作った関数のそれぞれの処理。
    public void MoveToLeft()
    {
        if (IsStan()) return;      // 気絶中は移動不可
        if (targetLane > MinLane)  // 左のレーンに移動可能なら
        {
            targetLane--;
            a1.PlayOneShot(moveSE);
        }
    }

    public void MoveToRight()
    {
        if (IsStan()) return;
        if (targetLane < MaxLane)  // 右のレーンに移動可能なら
        {
            targetLane++;
            a1.PlayOneShot(moveSE);
        }
    }

    public void Jump()
    {
      

            if (IsStan()) return;
        if (controller.isGrounded)
        {
   　        moveDirection.y = speedJump;

            animator.SetTrigger("jump");
                a1.PlayOneShot(jumpSE);

        }
    }


  


    public void Roling()

    {
     if(IsStan()) return;  
        {

            animator.SetTrigger("roling");
            a2.PlayOneShot(slideSE);
            
        }

    }
    
   
    

    //CharacterControllerにコライダーが当たった時の処理。
     void OnControllerColliderHit(ControllerColliderHit hit)

    {
        if (IsStan()) return;

        if(hit.gameObject.tag == "Robo")

        {

            life--;
           
            animator.SetTrigger("damage");
            a3.PlayOneShot(hitSE);
            a4.PlayOneShot(voiceSE);
            Debug.Log("GameOver");
            gameManager.GameOver();
            
          
        }
       
    }
  
   
        
        

}