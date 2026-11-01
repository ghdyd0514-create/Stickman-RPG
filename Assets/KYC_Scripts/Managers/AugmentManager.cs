using System.Collections.Generic;
using UnityEngine;

public class AugmentManager : MonoBehaviour
{
    public List<AugmentsData> allAugments = new List<AugmentsData>();   //  시스템 상 존재하는 모든 증강들.
    public HashSet<int> acquiredAugments = new HashSet<int>();

    //  획득한 증강을 HashSet에 넣는다.
    public void AcquireAugment(AugmentsData augmentsData)
    {
        acquiredAugments.Add(augmentsData.ID);
    }
    
    //  현재 획득한 증강이 전체 증강과 동일할 때.
    public bool HasAvailableAugment()
    {
        if (acquiredAugments.Count >= allAugments.Count) return false;

        return true;
    }

    public bool CanAppear(AugmentsData augments)
    {
        if (augments.requiredAugment != null)
        {
            //  내가 현재 획득한 증강들 중에서 선행증강을 필요로 하는 증강들 중, 선행증강을 가지고 있다면~
            if (acquiredAugments.Contains(augments.requiredAugment.ID))
            {
                return true;
            }

            return false;
        }

        return true;
    }

    //  획득한 증강들을 배제한 나머지 리스트를 반환한다.
    public List<AugmentsData> ReturnAugmentsListWithoutDuplication()
    {
        List<AugmentsData> returnAugments = new List<AugmentsData>();

        for (int i = 0; i < allAugments.Count; i++)
        {
            //  1. 우선 전체 증강을 순회하면서, 이 증강이 선행증강을 필요로 하는지부터 체크한다.
            if (CanAppear(allAugments[i]))
            {
                //  2. 이후, 중복 획득 가능성을 체크한다.
                if (!acquiredAugments.Contains(allAugments[i].ID))
                {
                    returnAugments.Add(allAugments[i]);
                }
            }
        }

        return returnAugments;
    }

    public HashSet<int> ReturnAcquiredAugments()
    {
        return acquiredAugments;
    }

    public List<AugmentsData> GetAcquiredAugments()
    {
        List<AugmentsData> results = new();

        for (int i = 0; i < allAugments.Count; i++)
        {
            if (acquiredAugments.Contains(allAugments[i].ID))
            {
                results.Add(allAugments[i]);
            }
        }

        return results;
    }
}