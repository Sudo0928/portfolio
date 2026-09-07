using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControll : MonoBehaviourPunCallbacks
{
    private Animator _animator;

    private Vector3 MoveDir;

    [SerializeField] private MouseRotate m_MouseRotate;
    [SerializeField] private Camera m_Camera;
    private CharacterController controller;

    public float gravity = 10f;
    public float JumpHeight = 3f;
    public float moveSpd = 5.0f;
    public bool isMove = true;
    public int animove = 0;
    public GameObject ItemSet;
    public Inventory inventory;
    public bool ActiveJump = false;

    private static readonly int Move = Animator.StringToHash("Move");
    private static readonly int Attack = Animator.StringToHash("Attack");


    public void ChangeLayers(Transform trans, string name)
    {
        trans.gameObject.layer = LayerMask.NameToLayer(name);
        for(int i = 0; i < trans.childCount; i++)
        {
            ChangeLayers(trans.GetChild(i), name);
        }
    }

    // Use this for initialization
    void Start()
    {
        if(!photonView.IsMine)
        {
            ChangeLayers(this.gameObject.transform, "Enemy");
            return;
        }

        m_Camera = GameManager._instance.Main_Camera;
        _animator = GetComponent<Animator>();
        m_MouseRotate.Init(transform, m_Camera.transform);
        controller = this.GetComponent<CharacterController>();
    }

    public void UseAnimator()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Sword And Shield Slash"))
        {
            _animator.SetInteger(Attack, 0);
            isMove = false;
            MoveDir.x = 0;
            MoveDir.z = 0;
        }
        else if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Standing Melee Attack 360 High"))
        {
            _animator.SetInteger(Attack, 0);
            isMove = false;
            MoveDir.x = 0;
            MoveDir.z = 0;
        }
        else
        {
            isMove = true;
        }

        if (Input.GetMouseButtonDown(0) &&
            inventory.inventory[int.Parse(UIManager._instance.Pick.transform.parent.name) - 1].iType == Item.ItemType.Weapon)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                _animator.SetInteger(Attack, 1);
            }
            else
            {
                _animator.SetInteger(Attack, 2);
            }
        }

        if (controller.velocity == Vector3.zero)
        {
            if (ActiveJump)
            {
                _animator.SetInteger(Move, 5);
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                _animator.SetInteger(Move, 4);
            }
            else _animator.SetInteger(Move, 0);
        }
        else
        {
            if (ActiveJump) _animator.SetInteger(Move, 5);
            else if (animove == 3) _animator.SetInteger(Move, 3);
            else if (animove == 2) _animator.SetInteger(Move, 2);
            else if (animove == 1) _animator.SetInteger(Move, 1);
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        moveSpd = 3.0f;

        UseAnimator();

        if (Input.GetKeyDown(KeyCode.I)) UserInfoManager._instance.PlayerHealth--;

        if (controller.isGrounded && isMove)
        {
            if (Input.GetKey((KeyCode.LeftShift)))
            {
                moveSpd = 1.0f;
                animove = 3;
            }
            else if (Input.GetKey(KeyCode.R))
            {
                moveSpd = 5.0f;
                animove = 2;
            }
            else
            {
                moveSpd = 3.0f;
                animove = 1;
            }

            MoveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * moveSpd;

            MoveDir = transform.TransformDirection(MoveDir);

            if (Input.GetButton("Jump") && (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Jumping") && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Jumping Crouch")))
            {
                ActiveJump = true;
                MoveDir.y = JumpHeight;
            }
            else ActiveJump = false;
        }

        MoveDir.y -= gravity * Time.deltaTime;

        controller.Move(MoveDir * Time.deltaTime);

        RotateView();

        StartCoroutine(inputNum());
    }

    IEnumerator inputNum()
    {
        switch (Input.inputString)
        {
            case "1":
                UIManager._instance.Pick.transform.SetParent(UIManager._instance.ItemBar.transform.GetChild(0));
                UIManager._instance.Pick.transform.localPosition = Vector2.zero;
                UIManager._instance.Pick.transform.SetAsFirstSibling();
                break;
            case "2":
                UIManager._instance.Pick.transform.SetParent(UIManager._instance.ItemBar.transform.GetChild(1));
                UIManager._instance.Pick.transform.localPosition = Vector2.zero;
                UIManager._instance.Pick.transform.SetAsFirstSibling();
                break;
            case "3":
                UIManager._instance.Pick.transform.SetParent(UIManager._instance.ItemBar.transform.GetChild(2));
                UIManager._instance.Pick.transform.localPosition = Vector2.zero;
                UIManager._instance.Pick.transform.SetAsFirstSibling();
                break;
            case "4":
                UIManager._instance.Pick.transform.SetParent(UIManager._instance.ItemBar.transform.GetChild(3));
                UIManager._instance.Pick.transform.localPosition = Vector2.zero;
                UIManager._instance.Pick.transform.SetAsFirstSibling();
                break;
            case "5":
                UIManager._instance.Pick.transform.SetParent(UIManager._instance.ItemBar.transform.GetChild(4));
                UIManager._instance.Pick.transform.localPosition = Vector2.zero;
                UIManager._instance.Pick.transform.SetAsFirstSibling();
                break;
            case "6":
                UIManager._instance.Pick.transform.SetParent(UIManager._instance.ItemBar.transform.GetChild(5));
                UIManager._instance.Pick.transform.localPosition = Vector2.zero;
                UIManager._instance.Pick.transform.SetAsFirstSibling();
                break;
            case "7":
                UIManager._instance.Pick.transform.SetParent(UIManager._instance.ItemBar.transform.GetChild(6));
                UIManager._instance.Pick.transform.localPosition = Vector2.zero;
                UIManager._instance.Pick.transform.SetAsFirstSibling();
                break;
            case "8":
                UIManager._instance.Pick.transform.SetParent(UIManager._instance.ItemBar.transform.GetChild(7));
                UIManager._instance.Pick.transform.localPosition = Vector2.zero;
                UIManager._instance.Pick.transform.SetAsFirstSibling();
                break;
            case "9":
                UIManager._instance.Pick.transform.SetParent(UIManager._instance.ItemBar.transform.GetChild(8));
                UIManager._instance.Pick.transform.localPosition = Vector2.zero;
                UIManager._instance.Pick.transform.SetAsFirstSibling();
                break;
        }
        yield return null;
    }

    private void RotateView()
    {
        m_MouseRotate.LookRotation(transform, m_Camera.transform);
    }
}