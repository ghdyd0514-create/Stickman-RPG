using System.Collections;
using UnityEngine;

//  나무 보스의 로직
//  1. 플레이어가 멀리있을 때는 채찍 공격(원거리 공격)을 함
//  2. 플레이어가 보스의 근접 공격 인식 범위에 있을 때 근접 공격 함
//  3. 나무 보스는 원거리에서 공격이 가능하기에 잘 안 움직이며, 가만히 서 있음 
//      그렇다고 안 움직이는 건 아니고, 공격 쿨타임 동안 잠깐 조금씩(한 두 발자국?) 움직일 수 잇음.
//  4.  만약 플레이어가 근접 공격 인식 범위에 있다면, 이때는 원거리 공격을 하면 안 됨.
public class TreeBoss : EnemyAttack
{
    Rigidbody2D rigid;
    Animator anim;
    EnemyMove enemyMove;
    SpriteRenderer sprite;

    [Header("근접 공격 이펙트 이미지")]
    [SerializeField] GameObject nearAttackEffectImage;

    [Header("근접 공격 인식 콜라이더")]
    [SerializeField] Collider2D nearAttackCognitiveCollider;

    [Header("근접 공격 범위 콜라이더")]
    [SerializeField] Collider2D nearAttackCollider;

    [Header("공격 전, 선딜 딜레이(근접)")]
    [SerializeField] float beforeNearAttackDelay = 2.0f;

    [Header("근접 공격 유지 시간")]
    [SerializeField] float nearAttackingTime = 1.0f;
    //--------------------------------------------------------------
    [Header("덩굴 채찍")]
    [SerializeField] GameObject farAttackObject;

    [Header("덩굴채찍 나오는 곳")]
    [SerializeField] GameObject farAttackObjectPoint;

    [Header("덩굴 날라가는 속도")]
    [SerializeField] float farAttackSpeed = 1.0f;

    [Header("원거리 공격 인식 콜라이더")]
    [SerializeField] Collider2D farAttackCognitiveCollider;

    [Header("공격 전, 선딜 딜레이(원거리)")]
    [SerializeField] float beforeFarAttackDelay = 2.0f;

    [SerializeField] float originalWhipLength = 2f;

    [Header("플레이어 발견?")]
    [SerializeField] bool hasPlayerIn_nearAttack = false;
    [SerializeField] bool hasPlayerIn_farAttack = false;
    [SerializeField] bool isPreparingAttack = false;

    //=----------------
    [Header("공격이 끝나고도 움직일까?")]
    [SerializeField] bool isMoving = false;

    [Header("공격 효과음")]
    [SerializeField] AudioClip farAttackSFX;
    [SerializeField] AudioClip nearAttackSFX; 
    [SerializeField] float volume = 1.0f;

    bool checkingCondition = false;

    protected override void Awake()
    {
        base.Awake();

        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        enemyMove = GetComponent<EnemyMove>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        nearAttackCollider.gameObject.SetActive(false);
        farAttackObjectPoint.gameObject.SetActive(false);
    }

    void Update()
    {
        if (canAttack && !isAttacking && !isPreparingAttack)
        {
            CheckAttackHit();
            return;
        }

        //  공격이 끝났고, 다음 공격 대기시간까지 보스가 움직일지 안 움직일지 랜덤하게 부여함.
        if (!canAttack && !isAttacking && !checkingCondition)
        {
            checkingCondition = true;

            StartCoroutine(RandomWalking());
        }
    }

    //  원거리 공격 인식 콜라이더에 플레이어가 들어왔는가.
    //  원거리 공격 인식 콜라이더와, 근접 공격 인식 콜라이더를 구분해야함. 
    void CheckAttackHit()
    {
        Collider2D farAttackHit = Physics2D.OverlapBox(farAttackCognitiveCollider.bounds.center, farAttackCognitiveCollider.bounds.size, 0, LayerMask.GetMask("Player"));

        if (farAttackHit != null && !hasPlayerIn_farAttack)
        {
            hasPlayerIn_farAttack = true;
        }

        Collider2D nearAttackHit = Physics2D.OverlapBox(nearAttackCognitiveCollider.bounds.center, nearAttackCognitiveCollider.bounds.size, 0, LayerMask.GetMask("Player"));

        if (nearAttackHit != null && !hasPlayerIn_nearAttack)
        {
            hasPlayerIn_nearAttack = true;
        }

        if (hasPlayerIn_farAttack && !isPreparingAttack)
            StartCoroutine(BeforeAttackDelay());
    }

    //  공격 전, 선딜 애니메이션
    IEnumerator BeforeAttackDelay()
    {
        isPreparingAttack = true;
        isMoving = false;
        rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);

        //  원거리 공격 선딜 애니메이션
        if (hasPlayerIn_farAttack && !hasPlayerIn_nearAttack)
        {
            enemyMove.canMove = false;
            rigid.linearVelocity = Vector2.zero;

            //  기존 애니메이션들 초기화
            anim.SetBool("nearAttackEnd", false);
            anim.SetBool("nearAttack", false);
            anim.SetBool("farAttackEnd", false);
            anim.SetBool("farAttack", false);

            anim.SetTrigger("farAttackReady");

            yield return new WaitForSeconds(beforeFarAttackDelay);
            Attack();
        }

        else if (hasPlayerIn_farAttack && hasPlayerIn_nearAttack)
        {
            enemyMove.canMove = false;
            enemyMove.canFlip = false;
            rigid.linearVelocity = Vector2.zero;

            //  기존 애니메이션들 초기화
            anim.SetBool("nearAttackEnd", false);
            anim.SetBool("nearAttack", false);
            anim.SetBool("farAttackEnd", false);
            anim.SetBool("farAttack", false);

            anim.SetTrigger("nearAttackReady");

            yield return new WaitForSeconds(beforeNearAttackDelay);
            Attack();
        }
    }

    public override void Attack()
    {
        isAttacking = true;
        canAttack = false;

        //  원거리 공격
        if (hasPlayerIn_farAttack && !hasPlayerIn_nearAttack)
        {
            anim.SetBool("farAttack", true);
            Managers.instance.audioManager.PlaySFX(farAttackSFX, volume);

            StartCoroutine(FarAttack());
        }

        //  근접 공격
        else if (hasPlayerIn_farAttack && hasPlayerIn_nearAttack)
        {
            anim.SetBool("nearAttack", true);
            Managers.instance.audioManager.PlaySFX(nearAttackSFX, volume);

            nearAttackEffectImage.gameObject.SetActive(true);
            nearAttackCollider.gameObject.SetActive(true);

            StartCoroutine(NearAttackingTime());
        }
    }

    //  덩굴 채찍 공격 방식
    //  덩굴 채찍은 현재 플레이어의 위치를 가져와서, 그 위치로 덩굴 채찍의 scale을 늘린다(이게 지금으로서 가장 쉬운 방법일듯. 아마도)
    //  플레이어가 맞든, 맞지 않든, 덩굴 채찍은 플레이어의 위치까지 뻗게 되면, 채찍이 나온 곳으로 되돌아간다.
    IEnumerator FarAttack()
    {
        farAttackObject.gameObject.SetActive(true);

        Vector2 startPos = farAttackObjectPoint.transform.position;
        Vector2 targetPos = Player.instance.transform.position;
        Vector2 dir = targetPos - startPos;

        float distance = dir.magnitude;
        float targetScaleX = distance / originalWhipLength;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (sprite.transform.localScale.x > 0) 
            farAttackObject.transform.rotation = Quaternion.Euler(0, 0, angle + 180.0f);
        else
            farAttackObject.transform.rotation = Quaternion.Euler(0, 0, angle);

        //  스케일 x 0으로 초기화
        farAttackObject.transform.localScale = new Vector2(0, 1);

        float currentLength = 0f;
        //  플레이어 방향으로 덩굴 뻗음
        while (currentLength < targetScaleX)
        {
            currentLength += farAttackSpeed * Time.deltaTime;
            currentLength = Mathf.Min(currentLength, targetScaleX);

            farAttackObject.transform.localScale = new Vector3(currentLength, 1f, 1f);
            yield return null;
        }

        //  잠깐 유지
        yield return new WaitForSeconds(0.1f);

        //  다시 되돌아옴
        while (currentLength > 0)
        {
            currentLength -= farAttackSpeed * Time.deltaTime;
            currentLength = Mathf.Max(currentLength, 0f);

            farAttackObject.transform.localScale = new Vector3(currentLength, 1f, 1f);
            yield return null;
        }

        EndAttack();
    }

    //  근접 공격 동작 유지 시간
    IEnumerator NearAttackingTime()
    {
        yield return new WaitForSeconds(nearAttackingTime);
        EndAttack();
    }

    //  공격 종료
    void EndAttack()
    {
        isAttacking = false;
        isPreparingAttack = false;
        enemyMove.canFlip = true;

        farAttackObject.SetActive(false);

        nearAttackEffectImage.SetActive(false);
        nearAttackCollider.gameObject.SetActive(false);

        anim.SetBool("nearAttack", false);
        anim.SetBool("farAttack", false);

        anim.SetBool("farAttackEnd", true);
        anim.SetBool("nearAttackEnd", true);

        hasPlayerIn_farAttack = false;
        hasPlayerIn_nearAttack = false;

        StartCoroutine(CoolDown());
    }


    //  공격 끝나고 움직일지 말지 정한다.
    IEnumerator RandomWalking()
    {
        //  공격 안하고 잇는 동안 움직일지 말지 1초마다 체크
        while (!canAttack && !isAttacking && !isPreparingAttack)
        {
            int randomNum = Random.Range(0, 4); // 0~3

            switch (randomNum)
            {
                case 0:
                    isMoving = true;
                    break;
                default:
                    isMoving = false;
                    break;
            }

            //  어차피 공격 중일 땐, enemyMove.canMove를 false시키니까
            //  다음 공격 전까지만 안 움직이면 되니 코루틴 내에서 처리하자...
            if (isMoving) { enemyMove.canMove = true; }
            else { enemyMove.canMove = false; rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);}

            yield return new WaitForSeconds(1); //  1초마다 체크
        }

        isMoving = false;

        rigid.linearVelocity = new Vector2(0f, rigid.linearVelocity.y);

        checkingCondition = false;
    }
}