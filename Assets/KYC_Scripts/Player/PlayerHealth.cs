using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

// 담당하는 기능 : HP, 피격 후 무적, 사망
public class PlayerHealth : MonoBehaviour
{
    SpriteRenderer[] spriteRenderers;
    Rigidbody2D rigid;
    Animator anim;
    PlayerCombat playerCombat;
    PlayerMove playerMove;

    [Header("HP")]
    public int maxHP = 50;

    [Header("추가체력(추가 체력이 얼마인지 알기위함)")]
    public int plusMaxHP = 0;

    [Header("넉백 크기")]
    [SerializeField] float knockBackPower = 1;

    [SerializeField]
    float knockBackDuration = 0.3f;

    [Header("현재 피격 됐는가?")]
    public bool isDamaged = false;

    [Header("피격 후 무적시간")]
    [SerializeField] float afterDamaged_InvincibilityTime = 1.5f;

    [Header("HP슬라이더 세팅")]
    [SerializeField] Slider hpSlider;
    public int currentHP;

    [Header("기존 콜라이더 / 죽을 때 콜라이더")]
    [SerializeField] CapsuleCollider2D surviveCollider;
    [SerializeField] BoxCollider2D deadCollider;

    [Header("죽을 때 넉백 크기")]
    [SerializeField] float dieKnockBackPower = 10.0f;
    [SerializeField] float verticalKnockBackPower = 0.4f;

    [Header("죽을 때 ray의 길이 및 바닥 레이어")]
    [SerializeField] float deadGroundCheckDistance = 0.15f;
    [SerializeField] LayerMask groundLayer;

    [Header("죽고 바닥에 닿았냐")]
    [SerializeField] bool isDeadGrounded = false;
    bool canCheckDeadGround = false;

    //  체력 변화가 있으면 그 상태를 직접적으로 알려주는 이벤트
    public event Action hpChanged;

    //  "현재 체력이 최대 체력에 비해 몇 퍼센트 남았는지 알려주는 이벤트"
    public event Action<float> onHpPercent;

    public bool isSuperArmor = false;
    public bool invincibility = false;
    public bool isDead = false;

    //  증강 획득 유무
    bool onSpikeArmor = false;

    [Header("가시갑옷 데미지")]
    [SerializeField] int spikeArmor = 3;

    //  -------------------------------------------------
    //  효과음 변수
    //  ---------------------------------------------------
    [Header("피격 받는 효과음")]
    [SerializeField] AudioClip TakeDamageSFX, playerDieSFX;
    [SerializeField] float volume = 1.0f, dieVolume = 1.0f;

    void Awake()
    {
        hpSlider = FindAnyObjectByType<hpSliderFindScript>(FindObjectsInactive.Include).GetComponent<Slider>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        playerCombat = GetComponent<PlayerCombat>();
        playerMove = GetComponent<PlayerMove>();

        currentHP = maxHP;
        hpSlider.maxValue = maxHP;
        hpSlider.value = currentHP;

        deadCollider = GetComponent<BoxCollider2D>();
        deadCollider.enabled = false;

        surviveCollider = GetComponent<CapsuleCollider2D>();
        surviveCollider.enabled = true;
    }

    void Update()
    {
        if (isDead && !isDeadGrounded && canCheckDeadGround && rigid.linearVelocity.y <= 0f)
        {
            RaycastHit2D boxHit = Physics2D.BoxCast(deadCollider.bounds.center, deadCollider.bounds.size / 1.05f,
                                                    0f, Vector2.down, deadGroundCheckDistance, groundLayer);

            DrawBoxCastSimple(deadCollider.bounds.center, deadCollider.bounds.size / 1.05f, Vector2.down, deadGroundCheckDistance);

            if (boxHit.collider != null)
            {
                Debug.DrawRay(boxHit.point, boxHit.normal * 0.5f, Color.green);

                // hit.normal.y가 높으면 바닥.
                // 벽에 박으면 normal이 옆을 향하므로 y가 낮음.
                if (boxHit.normal.y > 0.5f)
                {
                    DeadGrounded();
                }
            }
        }
    }

    // 체력이 깎이는 함수
    public void OnDamaged(Collider2D collider, int enemyAttackPower)
    {
        isDamaged = true;   //  데미지 입었다는 걸 타 스크립트한테 알려줌
        invincibility = true; // 무적 On
        currentHP -= enemyAttackPower;
        hpSlider.value = currentHP;

        hpChanged?.Invoke();

        //  가시값옷 증강이 활성화 되어 있다면 상대한테 데미지 준다
        if (onSpikeArmor)
        {
            if (collider.GetComponentInChildren<EnemyHurtbox>() != null)
            {
                collider.GetComponentInChildren<EnemyHurtbox>().TakeDamage(spikeArmor);
            }
        }

        if (currentHP <= 0) { PlayerDie(collider); return; }

        StartCoroutine(KnockBack(collider));
        StartCoroutine(OnDamagedAnim()); // 피격 받는 애니메이션을 실행한다.
        Managers.instance.audioManager.PlaySFX(TakeDamageSFX, volume);
        SurveyHpPercent();
    }

    IEnumerator KnockBack(Collider2D collider)
    {
        if (isSuperArmor)
        {
            yield break;
        }
        
        playerMove.canMove = false;

        rigid.linearVelocity = Vector2.zero;

        float knockBackDirX = transform.position.x >= collider.transform.position.x ? 1f : -1f;
        Vector2 knockBackVec = new Vector2(knockBackDirX, 0.4f).normalized;
        rigid.AddForce(knockBackVec * knockBackPower, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockBackDuration);

        playerMove.canMove = true;
    }

    //  자해요 ㅇ
    public void SelfDamage(int selfDamage, Collider2D collider)
    {
        currentHP -= selfDamage;
        hpSlider.value = currentHP;
        hpChanged?.Invoke();
        if (currentHP <= 0) { PlayerDie(collider); return; }
        SurveyHpPercent();
    }

    //  체력을 회복해주게 하는 함수
    public void HPRecovery(int recoveryAmount)
    {
        //  현재 체력이 최대체력을 넘지 않도록 조정
        if (currentHP < maxHP)
        {
            Debug.Log("회복 전 현재 체력 : " + currentHP);
            currentHP += recoveryAmount;
            hpSlider.value = currentHP;
            hpChanged?.Invoke();

            //  체력 회복 했는데 현 체력이 최대체력보다 많다면 최대체력으로 고정시킨다
            if (currentHP > maxHP)
            {
                currentHP = maxHP;
                hpChanged?.Invoke();
            }

            Debug.Log("체력이 회복되었습니다 : " + recoveryAmount);
            Debug.Log("현재 체력 : " + currentHP);
        }
        SurveyHpPercent();
    }

    //  현재 체력이 전체 체력에 비해 얼마나 있는지 계산
    float CurrentHpCalculate()
    {
        float hpPercent = (float)currentHP / maxHP * 100f;

        return hpPercent;
    }

    //  각 스크립트한테 알려준다~
    void SurveyHpPercent()
    {
        float hpPercent = CurrentHpCalculate();
        Debug.Log("hp퍼센트 : " + hpPercent);

        onHpPercent?.Invoke(hpPercent);
    }

    //  피격 받는 애니메이션을 담당하는 코루틴
    IEnumerator OnDamagedAnim()
    {
        spriteRenderers[0].color = new Color(1, 1, 1, 0.5f);  //  반투명해지기
        spriteRenderers[1].color = new Color(1, 1, 1, 0.5f);  //  반투명해지기

        if (!isSuperArmor)
            anim.SetTrigger("Damage");

        //  무적시간이 남아도, 피격 받는 애니메이션이 끝나면 공격을 할 수 있게 조정해준다.
        StartCoroutine(OnDmaged_But_You_Can_Attack());

        //  피격 받은 후 무적시간을 부여해준다.
        yield return StartCoroutine(InvincibilityTime(afterDamaged_InvincibilityTime));
        spriteRenderers[0].color = new Color(1, 1, 1, 1);
        spriteRenderers[1].color = new Color(1, 1, 1, 1);
    }

    //  무적시간 중에서 공격할 수 있는 시간을 주는 코루틴
    IEnumerator OnDmaged_But_You_Can_Attack()
    {
        yield return new WaitForSeconds(1f / 3f); //  피격 애니메이션이 끝나는 시간. samples는 3, 총 프레임은 1임.

        isDamaged = false;  //  피격 애니메이션이 끝나면 isDamaged를 false로 하여 무적상태동안 공격할 수 있도록 한다.
    }

    // 무적시간을 주는 코루틴
    public IEnumerator InvincibilityTime(float invincibilityTimeParameter)
    {
        //  피격을 받을 때는 피격 받는 함수(void OnDamaged)에 바로 무적을 on 하지만, 
        //  타 스크립트에서 불러올 것을 고려하여 해당 코루틴에도 무적을 on시켜주는 변수를 넣는다.
        invincibility = true;
        float timer = 0;
        while (timer <= invincibilityTimeParameter)
        {
            timer += Time.deltaTime;


            yield return null;
        }
        invincibility = false;
    }

    void PlayerDie(Collider2D collider)
    {
        isDead = true;
        currentHP = 0;
        hpSlider.value = currentHP;
        hpChanged?.Invoke();
        deadCollider.enabled = true;
        surviveCollider.enabled = false;
        GetComponent<PlayerMove>().canMove = false;
        GetComponent<PlayerMove>().canJump = false;

        StartCoroutine(Managers.instance.gameManager.PlayerDie());

        //  죽을 때는 좀 적이랑 반대 방향으로 날아가면서 죽는 게 웬지 이펙트 있을 거 같음.
        rigid.linearVelocity = Vector2.zero;
        Vector2 knockBackVec = (transform.position - collider.transform.position).normalized;
        knockBackVec.y = verticalKnockBackPower;
        knockBackVec.Normalize();
        rigid.AddForce(knockBackVec * dieKnockBackPower, ForceMode2D.Impulse);
        StartCoroutine(EnableDeadGroundCheck());

        spriteRenderers[0].color = new Color(1, 1, 1, 0.5f); // 죽었으면은 알파값을 낮춰서 죽은 게 보이게끔
        spriteRenderers[1].color = new Color(1, 1, 1, 0.5f); // 죽었으면은 알파값을 낮춰서 죽은 게 보이게끔

        //  ------------------- 죽었으니까 애니메이션 초기화-------------------------
        anim.SetBool("Walk", false);
        anim.SetBool("Idle", false);
        anim.SetBool("Air", false);
        anim.ResetTrigger("Damage");
        switch (Player.instance.currentClass)
        {
            case PlayerClass.Warrior:
                GetComponent<WarriorDash>().CancelDash();
                GetComponent<WarriorParry>().CancelParry();
                break;

            case PlayerClass.Archer:
                break;

            case PlayerClass.Magician:
                break;
        }

        //  사망 애니메이션 on;;
        anim.SetTrigger("DieAir");
        Managers.instance.audioManager.PlaySFX(playerDieSFX, dieVolume);
    }

    //  플레이어가 뒤1지고 바로 땅에 눕는 게 아니라 살짝 날아가는 연출을 위해 땅을 무시하는 보정시간을 살짝 넣는다
    IEnumerator EnableDeadGroundCheck()
    {
        yield return new WaitForSeconds(0.1f);
        canCheckDeadGround = true;
    }

    void DeadGrounded()
    {
        if (isDeadGrounded) return;

        isDeadGrounded = true;
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;

        // 죽어서 땅에 누운 뒤 더 이상 물리로 밀리지 않게 하고 싶으면 Kinematic.
        rigid.bodyType = RigidbodyType2D.Kinematic;
        anim.SetBool("DieGround", true);
    }

    public void MoveTitleWhenPlayerDie(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StartCoroutine(Managers.instance.sceneMoveManager.EnterVillage("Village"));
        }
    }

    //------------------------------------------------------- 함수를 통해 변수 변경

    //  최대체력이 증가되면 그만큼 hp슬라이더에 적용시켜야 되니. 그를 담당하는 함수
    public void AddMaxHP(int addMaxHP)
    {
        maxHP += addMaxHP;
        hpSlider.maxValue = maxHP;
        plusMaxHP += addMaxHP;
        hpChanged?.Invoke();
    }

    //  가시갑옷 증강 획득
    public void SetSpikeArmor()
    {
        onSpikeArmor = true;
    }

    public void SetInvincibilityTime(float value)
    {
        afterDamaged_InvincibilityTime += value;
    }







    void DrawBoxCastSimple(Vector2 origin, Vector2 size, Vector2 direction, float distance)
    {
        Vector2 endPosition = origin + direction.normalized * distance;

        DrawBox(endPosition, size, Color.red);

        Debug.DrawLine(origin, endPosition, Color.red);
    }

    void DrawBox(Vector2 center, Vector2 size, Color color)
    {
        Vector2 halfSize = size * 0.5f;

        Vector2 topLeft = new Vector2(center.x - halfSize.x, center.y + halfSize.y);
        Vector2 topRight = new Vector2(center.x + halfSize.x, center.y + halfSize.y);
        Vector2 bottomLeft = new Vector2(center.x - halfSize.x, center.y - halfSize.y);
        Vector2 bottomRight = new Vector2(center.x + halfSize.x, center.y - halfSize.y);

        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);
    }
}
