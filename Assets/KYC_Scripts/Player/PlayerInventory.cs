using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

//  플레이어 상태창 표시 및 가지고 있는 포션을 뜻하는 스크립트
//  뭔가 상태창이랑 포션이랑 따로 놀고 있긴 한데... inventory는 둘 다 맞으니까.... 아 스크립트 또 만들기 귀찮음;;;
public class PlayerInventory : MonoBehaviour
{
    PlayerCombat playerCombat;
    PlayerHealth playerHealth;

    [Header("플레이어 상태창 캔버스")]
    [SerializeField] PlayerStatusCanvas playerStatusCanvas;

    [Header("플레이어가 현재 가지고 있는 포션의 개수 (최대 10개 제한)")]
    public int potionMaxCount = 99;
    public int potionAmount = 0;
    public event Action<int> getPotionAmountEvent;

    [Header("포션 회복 개수")]
    [SerializeField] int potionHealAmount = 10;

    [Header("포션 재사용 쿨타임")]
    [SerializeField] float potionCooldown = 3.0f;

    [Header("포션 사용 가능 여부")]
    [SerializeField] bool canUsePotion = true;

    [Header("포션 아이콘 이미지")]
    [SerializeField] SkillCooldownUI potionCooldownImage;

    [Header("가지고 있는 돈")]
    [SerializeField] int cash;
    public event Action getCashChangeEvent;

    [Header("포션 사용 소리")]
    [SerializeField] AudioClip potionUseSFX;
    [SerializeField] float volume;

    void Awake()
    {
        playerCombat = GetComponent<PlayerCombat>();
        playerHealth = GetComponent<PlayerHealth>();
        playerStatusCanvas = FindAnyObjectByType<PlayerStatusCanvas>(FindObjectsInactive.Include);
        
        //  원래 스크립트라면 포션 전용 쿨타임 나타내는 ui스크립트를 부착시켜줘야 겠지만.. 그냥 동일한 기능 잇는 녀석으로 선택해서 스크립트명이 이런거임;;;;
        PotionUI potionUI =FindAnyObjectByType<PotionUI>(FindObjectsInactive.Include);

        if (potionUI != null)
        {
            potionCooldownImage = potionUI.GetComponentInChildren<SkillCooldownUI>(true);
        }
        playerStatusCanvas.gameObject.SetActive(false);

        canUsePotion = true;
    }

    void Start()
    {
        cash = GameProgress.instance.cashData;
        potionAmount = GameProgress.instance.potionData;
        getCashChangeEvent?.Invoke();
        getPotionAmountEvent?.Invoke(potionAmount);
    }

    //  ---------------------상태창 관련
    public void OnPlayerStatusCanvas(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerStatusCanvas.OpenCanvas(playerHealth, playerCombat);
        }
        else if (context.canceled)
        {
            playerStatusCanvas.gameObject.SetActive(false);
        }
    }

    //-------------------------- 돈 관련
    //  정적 스크립트한테 데이터 전달
    public int CashData
    {
        get => GameProgress.instance.cashData;
        set => GameProgress.instance.cashData = value;
    }

    //  따로 ui를 위해 플레이어 인벤토리 스크립트에서도 증가하게 해줌
    public void SetMoney(int cash)
    {
        this.cash += cash;
        CashData = this.cash;

        getCashChangeEvent?.Invoke();
    }

    public int GetMoney()
    {
        return cash;
    }

    // -----------------------------------포션 관련
    //  정적 스크립트한테 전달
    public int Potion
    {
        get => GameProgress.instance.potionData;
        set => GameProgress.instance.potionData = value;
    }

    //  플레이어가 포션을 획득하면 포션 ui한테ㅔ 알려줘서 현재 포션이 몇 개인지 알려준다.
    public void GetPotionAmount(int potion)
    {
        potionAmount += potion;
        Potion = potionAmount;

        getPotionAmountEvent?.Invoke(potionAmount);
    }

    //  포션 쿨타임
    IEnumerator PotionCooldown()
    {
        canUsePotion = false;

        StartCoroutine(potionCooldownImage.SkillCooldown(potionCooldown));
        yield return new WaitForSeconds(potionCooldown);

        canUsePotion = true;
    }

    public int SetPotion()
    {
        return potionAmount;
    }

    //  이 함수는 포션 사용 함수
    //  1번 키 누르면 사용하게끔 할 생각?
    public void OnUsePotion(InputAction.CallbackContext context)
    {
        if (context.performed && potionAmount > 0 && canUsePotion && !playerHealth.isDead)
        {
            GetPotionAmount(-1);
            StartCoroutine(PotionCooldown());
            Managers.instance.audioManager.PlaySFX(potionUseSFX, volume);
            playerHealth.HPRecovery(potionHealAmount);
        }
    }
}