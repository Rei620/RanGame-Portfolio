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
        // PC�p�i�}�E�X����j
        SwipeWithMouth();
    #elif UNITY_IOS || UNITY_ANDROID
       // �X�}�z�p�i�^�b�`����j
        SwipeWithTouch();
    #endif
    }

    //  PC�p�i�}�E�X�X���C�v�j
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

    //  �X�}�z�p�i�^�b�`�X���C�v�j
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

    //  ���ʂ̃X���C�v���菈��
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
            moveDirection.y = -10f; // �X���C�f�B���O�����ɉ���
        }
    }


    //���[���̈ړ��̐��l�����ꂼ��̕ϐ��Ő錾���܂��B
    const int MinLane = -1;
    const int MaxLane = 1;
    const float LaneWidth = 5.0f;
    const int DefaultLife = 1;
    const float StunDuration = 0.5f;

    //CharacterController�^��ϐ�controller�Ő錾���܂��B
    CharacterController controller;
    //Animator�^��ϐ�animator�Ő錾���܂��B
    Animator animator;

    
    //���ꂼ��̍��W���O�Ő錾���܂��B
    Vector3 moveDirection = Vector3.zero;
    //int�^��ϐ�targetLane�Ő錾���܂��B
    int targetLane;
    int life = DefaultLife;
    
    //���ꂼ��̃p�����[�^�[�̐ݒ��Inspector�ŕς���l�ɂ��܂��B
    public float gravity;
    public float speedZ;
    public float speedX;
    public float speedRoling;
    public float speedJump;
    public float accelerationZ;
    public float time;
    public float jumpMotion;

    private const string ps_JumpMotion = "jumpMotion";

    //���C�t���擾����֐�

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
        //CharacterController �R���|�[�l���g���擾���ĕϐ� controller �ɓ����
        controller = GetComponent<CharacterController>();
        //Animator �R���|�[�l���g���擾���ĕϐ� animator �ɓ����
        animator = GetComponent<Animator>();

        p_Animator = this.GetComponent<Animator>();
    }

    void Update()
    {
        p_Animator.SetFloat(ps_JumpMotion, jumpMotion);

        // �X���C�v����
        SwipeInput();

        // CharacterController �̃T�C�Y�X�V
        controller.height = animator.GetFloat("ColliderHeight");
        controller.center = new Vector3(controller.center.x, animator.GetFloat("ColliderCenter"), controller.center.z);
        controller.radius = animator.GetFloat("ColliderRadius");

        // 3�b�����ɉ���
        time += Time.deltaTime;
        if (Mathf.Floor(time) % 3 == 0)
        {
            // speedZ �� 80f �ȉ��̎���������
            if (speedZ < 80f)
            {
                speedZ += 0.03f;
                speedJump += 0.02f;
                gravity += 0.1f;
                jumpMotion += 0.0005f;
            }
        }

        

        // �C�⏈��
        if (IsStan())
        {
            moveDirection.x = 0.0f;
            moveDirection.z = 0.0f;
           
        }
        else
        {
            // Z��������
            float acceleratedZ = moveDirection.z + (accelerationZ * Time.deltaTime);
            moveDirection.z = Mathf.Clamp(acceleratedZ, 0, speedZ);

            // X�����F���[���ړ�
            float ratioX = (targetLane * LaneWidth - transform.position.x) / LaneWidth;
            moveDirection.x = ratioX * speedX;
        }

        // �d�͏���
        moveDirection.y -= gravity * Time.deltaTime;

        // �ړ����s
        Vector3 globalDirection = transform.TransformDirection(moveDirection);
        controller.Move(globalDirection * Time.deltaTime);

        if (controller.isGrounded) moveDirection.y = 0;

        // �A�j���[�V�����X�V
        animator.SetBool("run", moveDirection.z > 0.0f);
    }



    //�V����������֐��̂��ꂼ��̏����B
    public void MoveToLeft()
    {
        if (IsStan()) return;      // �C�⒆�͈ړ��s��
        if (targetLane > MinLane)  // ���̃��[���Ɉړ��\�Ȃ�
        {
            targetLane--;
            a1.PlayOneShot(moveSE);
        }
    }

    public void MoveToRight()
    {
        if (IsStan()) return;
        if (targetLane < MaxLane)  // �E�̃��[���Ɉړ��\�Ȃ�
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
   �@        moveDirection.y = speedJump;

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
    
   
    

    //CharacterController�ɃR���C�_�[�������������̏����B
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