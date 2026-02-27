using UnityEngine;

public class SimplePlayer : MonoBehaviour
{
    private Rigidbody2D rigid; //����Ѻ�������͹���
    private Animator anim; //����Ѻ Animation

    [Header("Ground and Wall Check")]
    [SerializeField] private float groundDistCheck = 1f; //���� sensor ���仪����
    [SerializeField] private float wallDistCheck = 1f; //���� sensor ���仪���ѧ
    [SerializeField] private LayerMask groundLayer; //��੾�� Layer �ͧ���
    public bool isGrounded = false; //��Ǩ�����
    public bool isWalled = false; //��Ǩ����ᾧ

    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f; //��������㹡������͹������Һ
    public float X_input; //���� A,D
    public float Y_input; //���� S ����� slide ����

    [Header("Jump")]
    [SerializeField] private float jumpForce = 20f; //�ç���ⴴ
    [SerializeField] private Vector2 wallJumpForce = new Vector2(10f, 15f); //�ç���ⴴ wallJump
    public bool isJumping = false;
    public bool isWallJumping = false;
    public bool isWallSliding = false;
    public bool canDoubleJump = false; //doubleJump ����������ǵ�͡�á��ⴴ
    public int facing = 1; //�ѹ˹�ҵç������ѧ���� wallJumping

    [SerializeField] private float coyoteTimeLimit = .5f; //�������ҷ�衴��������ö�����ҧ�ҡ����
    [SerializeField] private float bufferTimeLimit = .5f; //�������ҷ�衴��������ö�����ҧ�ҡ����
    public float coyoteTime = -10000f; //�����������������顴���ⴴ��ҧ�ҡ����
    public float bufferTime = -10000f; //�����������������顴���ⴴ��͹�֧�����

    private void Awake() //�ӧҹ��͹��������
    {
        rigid = GetComponent<Rigidbody2D>(); //�ѹ������ Game Object ���
        anim = GetComponentInChildren<Animator>(); //�� InCoildren ���� Animator
    }

    private void Update()//�ӧҹ�ء���
    {
        JumpState(); //��Ǩʶҹ���� ���躹��� ���ѧ���ⴴ ���ѧŧ������� wallSlide
        Jump();// ��觡��ⴴ�Ẻ��ҧ�
        WallSlide(); // ��� wallSlide
        InputVal(); // ��Ǩ input �ҡ������
        Move(); // �������͹��Ƿ�駺��������ҡ��
        Flip(); // ����ѹ˹��价ҧ��ȡ������͹����Ѵ��ѵ
        GroundAndWallCheck(); // ��Ǩ�Ѻ�����м�ѧ
        Animation(); // ��� animation
    }

    private void JumpState() //��Ǩʶҹе���Ф�
    {
        if (!isGrounded && !isJumping) // fall/takeoff
        {
            isJumping = true; //ⴴ����

            if (rigid.linearVelocityY <= 0f) //fall �������
            {
                coyoteTime = Time.time; //������Ѻ���� coyote
            }
        }

        if (isGrounded && isJumping) // landing
        {
            isJumping = false;
            isWallJumping = false;
            isWallSliding = false;
            canDoubleJump = false;
        }

        if (isWalled) // ��Ǩ wallSliding
        {
            isJumping = false;
            isWallJumping = false;
            canDoubleJump = false;

            if (isGrounded) //������躹���
            {
                isWallSliding = false;
            }
            else //���������躹���
            {
                isWallSliding = true;
            }
        }
        else //¡��ԡ wallSlide ������Դ��ᾧ
        {
            isWallSliding = false;
        }
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space)) //��ҡ� Space Bar
        {
            if (!isWalled) // ������Դ��ᾧ
            {
                if (isGrounded) // *** normalJump // ������躹���
                {
                    canDoubleJump = true;
                    rigid.linearVelocity = new Vector2(rigid.linearVelocityX, jumpForce);
                }
                else // doubleJump, coyoteJump // ���������躹���
                {
                    if (rigid.linearVelocityY > 0f && canDoubleJump) // *** doubleJump
                    {
                        canDoubleJump = false; // doubleJump ��������
                        rigid.linearVelocity = new Vector2(rigid.linearVelocityX, jumpForce);
                    }

                    if (rigid.linearVelocityY <= 0f)
                    {
                        if (Time.time < coyoteTime + coyoteTimeLimit) // *** coyoteJump
                        {
                            coyoteTime = 0f;
                            rigid.linearVelocity = new Vector2(rigid.linearVelocityX, jumpForce); //ⴴ
                        }
                        else // ������Ѻ bufferJump
                        {
                            bufferTime = Time.time; //������Ѻ bufferTime
                        }
                    }
                }
            }
            else //��ҵԴ��ᾧ�ʴ������ wallJump
            {
                canDoubleJump = false; 
                isWallJumping = true; //�����͡�ҡ wallSliding
                rigid.linearVelocity = new Vector2(wallJumpForce.x * facing, wallJumpForce.y); //***wallJump
            }
        }
        else //�����衴���� bufferJump
        {
            if (isGrounded && Time.time < bufferTime + bufferTimeLimit) //����������������������ҷ�� buffer ��
            {
                bufferTime = 0f;
                rigid.linearVelocity = new Vector2(rigid.linearVelocityX, jumpForce); //***bufferJump
            }
        }
    }

    private void WallSlide()
    {
        if (!isWalled || isGrounded || isWallJumping || rigid.linearVelocityY > 0f)
            return; //������÷Ѵ��������

        float Y_slide = Y_input < 0f ? 1f : .5f;
        rigid.linearVelocity = new Vector2(X_input * moveSpeed, rigid.linearVelocityY * Y_slide); //�����ŧ
    }

    private void InputVal()
    {
        X_input = Input.GetAxisRaw("Horizontal"); //GetAxisRaw() ������Ẻ��Һ
        Y_input = Input.GetAxisRaw("Vertical");
    }

    private void Move()
    {
        if (isWallJumping || isWallSliding) //��� wallJumping ��������͡�ҡ��äǺ����ҡ Player
            return; //�ѹ�������ҹ��÷Ѵ��������

        if (isGrounded) //������躹���
        {
            rigid.linearVelocity = new Vector2(X_input * moveSpeed, rigid.linearVelocityY); //�͡�ç��ѡ rigid �������͹���
        }
        else //�����¡�ҧ�ҡ��
        {
            float X_airMove = X_input != 0f ? X_input * moveSpeed : rigid.linearVelocityX; //᡹ X �����衴������͹������ç Physics
            rigid.linearVelocity = new Vector2(X_airMove, rigid.linearVelocityY);
        }
    }

    private void Flip() //��ع����Ф� �����ȡ������͹���
    {
        if (rigid.linearVelocityX > 0.1f) //��ѡ价ҧ������
        {
            facing = -1; //�ѹ˹�ҵç����
            transform.rotation = Quaternion.identity; //identity �դ����ҡѺ 0,0,0 /�ѹ�͹�����
        }
        else if (rigid.linearVelocityX < -0.01f) //�������ѡ�ҧ���
        {
            facing = 1; //�ѹ˹�ҵç����
            transform.rotation = Quaternion.Euler(0f, 180f, 0f); //�ѹ� 180 ͧ��
        }
    }

    private void GroundAndWallCheck()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundDistCheck, groundLayer); //sensor ���
        isWalled = Physics2D.Raycast(transform.position, transform.right, wallDistCheck, groundLayer); //sensor ��ѧ
    }

    private void OnDrawGizmos() //��ҿ�ԡ�ʴ��Ţͧ sensor ��Ǩ�Ѻ�����м�ѧ
    {
        Gizmos.color = Color.blue; //����չ���Թ
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundDistCheck); //��� sensor ��Ǩ���
        Gizmos.color = Color.red; //�����ᴧ
        Gizmos.DrawLine(transform.position, transform.position + transform.right * wallDistCheck); //��� sensor ��Ǩ��ѧ
    }

    private void Animation()
    {
        anim.SetBool("isGrounded", isGrounded); // ตัดสินใจว่า ลอย/อยู่บนพื้น
        anim.SetBool("isWallSliding", isWallSliding); // ตัดสินใจ wallSliding

        if(isWalled) // ถ้าติดกำแพงจะหยุดเดิน
        {
            anim.SetFloat("velX", 0f); // หยุด run
        }
        else
        {
            anim.SetFloat("velX", rigid.linearVelocityX); // จะ idle หรือ run
        }

        anim.SetFloat("velX", rigid.linearVelocityX); // จะ idle หรือ run
        anim.SetFloat("velY", rigid.linearVelocityY); // จะโดดขึ้นหรือลง
    }

}

