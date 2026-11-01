using UnityEngine;
using System.Collections.Generic;

//  스크립트 기능 : 
//  1.  플레이어가 현재 선택한 증강을 인식해서, 해당 증강의 획득 횟수를 +1 시킨다. 
//  2.  플레이어가 획득한 증강의 획득 횟수를 반환한다.
public class OldAugmentManager : MonoBehaviour
{
    [Header("각 증강을 몇 개 획득했는지 알려줌.")]
    Dictionary<AugmentsData, int> acquireAugments = new Dictionary<AugmentsData, int>();

    [Header("증강 선택 매니저")]
    [SerializeField] AugmentSelectionManager augmentSelectionManager;

    //  플레이어가 현재 획득한 증강의 획득 횟수를 +1 시킨다.
    public void AcquireAugment(AugmentsData augmentsData)
    {
        //  만약에 처음 획득한 증강이라면 획득 횟수를 1로 고정시킨다.
        if (!acquireAugments.ContainsKey(augmentsData))
        {
            acquireAugments.Add(augmentsData, 1);
            Managers.instance.augmentManager.AcquireAugment(augmentsData);
            //Debug.Log("증강 등록 됨 : " + augmentsData.augmentName + " 를 처음 먹었습니다. " + acquireAugments[augmentsData]);
        }

        //  이미 보유하고 있는 증강이라면 그 증강의 획득 횟수를 +1 늘린다.
        else
        {
            acquireAugments[augmentsData]++;
            //Debug.Log("증강 연속 획득 : " + augmentsData.augmentName + "를 " + acquireAugments[augmentsData] + "만큼 획득함");
        }
    }

    //  해당 증강이 현재 나타날 수 있는지 아닌지를 판단한다.
    public bool CanAppear(AugmentsData augmentData)
    {
        //  1. 선행증강의 유무 (선행증강이 있는데 아직 선행증강을 획득하지 않았다면 false반환)
        if (augmentData.requiredAugment != null)
        {
            if (!acquireAugments.ContainsKey(augmentData.requiredAugment)) {return false;}
        }

        //  2. 처음 획득하는 증강의 경우에는 true를 반환하여 획득할 수 있게 한다.
        if (!acquireAugments.ContainsKey(augmentData)) {return true;}

        //  3. 최대 획득 count만큼 획득한 증강은 false를 반환하여 더 이상 증강이 나타나지 않도록 한다.
        if (acquireAugments[augmentData] >= augmentData.maximumAppear)
        {
            //  여기다가 최대 획득만큼 획득한 증강을 더 못 나오게끔 해야 함.
            //Debug.Log(augmentData.augmentName + " 증강을 최대 횟수 만큼 먹음");
            return false;
        }

        return true;
    }

    //  전체 증강들이 이제 더 이상 등장이 가능한지 아닌지를 판별
    public bool HasAugments(AugmentsData[] augmentsData)
    {
        for (int i = 0; i < augmentsData.Length; i++)
        {
            if (CanAppear(augmentsData[i]))
            {
                return true;
            }
        }

        
        return false;
    }

    //  현재 보유중인 증강들과 해당 증강들의 획득 개수를 반환한다.
    public Dictionary<AugmentsData, int> GetAcquireAugmentsValues() 
    {
        return acquireAugments;
    }
}