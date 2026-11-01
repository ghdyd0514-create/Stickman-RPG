using System.Collections.Generic;
using UnityEngine;

//  이 스크립트의 기능 : 
//  1. 증강 선택 ui가 만들어질 때, 증강 3개 추첨.
//  2. 또한 증강 3개를 추첨할 때 해당 증강이 더 이상 등장 가능한 증강인지도 확인해야 함.
public class AugmentSelectionManager : MonoBehaviour
{
    [Header("추첨 될 3개의 증강")]
    AugmentsData[] selectedAugments = new AugmentsData[3];

    //  이 리스트는 배열인 allAugments를 리스트로 나타내기 위함이다.
    public List<AugmentsData> allListAugments;

    //  전체 증강들 중에서 현재 획득 가능한 총 3개의 증강을 뽑는다.
    //  추첨 된 3개의 증강 중에서 하나를 가져온다.
    public AugmentsData GetOneAugment(int cardNum)
    {
        return selectedAugments[cardNum];
    }

    public void GetRandomAugments()
    {
        //  0. 일단 selectedAugments를 초기화해준다.
        for (int i = 0; i < selectedAugments.Length; i++)
        {
            selectedAugments[i] = null;
        }

        //  1. GameProgress스크립트를 통해, 증강 리스트를 받는다.
        List<AugmentsData> remainderAugments = Managers.instance.augmentManager.ReturnAugmentsListWithoutDuplication();

        //  2. 이 함수의 로컬 리스트에 추첨 될 3개를 정한다. (잔여 증강의 개수가 3개보다 적을 경우를 대비해 switch로 경우를 나눈다.)
        int[] randomIndex = new int[3];
        for (int i = 0; i < 3; i++) 
        {
            //  remainderAugments를 remove하면서 결국 더 이상 뽑을 증강이 없을경우 break
            if (remainderAugments.Count <= 0)
            {
                break;
            }

            randomIndex[i] = Random.Range(0, remainderAugments.Count);  
            selectedAugments[i] = remainderAugments[randomIndex[i]];
            remainderAugments.RemoveAt(randomIndex[i]);
        }
    }
}
/*
    [Header("각 등급당 뽑힐 확률(인스펙터에서 조정할 때 무조건 세 개의 합이 100이여야 함)")]
    [Min(0)]
    [SerializeField] int gradeC = 60;   //  60%
    [SerializeField] int gradeB = 30;   //  30%
    [SerializeField] int gradeA = 10;   //  10%
    */
//  전체 증강 중에서 3개를 임의로 추첨한다.
/*
public void GetRandomAugments()
{
    //  로컬 변수로써 사용하는 모든 증강이 들어간 리스트.   로컬 변수기 때문에 막 수정해도 된다 ㅇㅇ
    List<AugmentsData> availableAugments = new List<AugmentsData>(allListAugments);
    Unusable(availableAugments);

    List<AugmentsData> augmentGradeC = new List<AugmentsData>();
    List<AugmentsData> augmentGradeB = new List<AugmentsData>();
    List<AugmentsData> augmentGradeA = new List<AugmentsData>();

    //  증강 추첨 배열을 초기화시킨다.
    for (int i = 0; i < selectedAugments.Length; i++)
    {
        selectedAugments[i] = null;
    }

    //  해당 리스트는 3개의 카드가 증강 데이터를 받을 때 받는 데이터가 중복이 되지 않도록 해주는 리스트이다
    List<AugmentsData> remainingAugments = new List<AugmentsData>(availableAugments);

    //  각 증강을 등급별로 나누어 등급 리스트에 넣는다
    //  1. 우선 전체 리스트를 순회하며 각 증강의 등급에 맞는 리스트에 증강을 넣는다.
    for (int i = 0; i < remainingAugments.Count; i++)
    {
        if (remainingAugments[i].augmentGrade == AugmentGrade.C)
        {
            augmentGradeC.Add(remainingAugments[i]);
        }
        else if (remainingAugments[i].augmentGrade == AugmentGrade.B)
        {
            augmentGradeB.Add(remainingAugments[i]);
        }
        else if (remainingAugments[i].augmentGrade == AugmentGrade.A)
        {
            augmentGradeA.Add(remainingAugments[i]);
        }
    }

    //  2. 그리고 뽑힐 각각의 3개 카드들은 3개의 등급 중에서 확률에 맞게 하나를 선택하고
    //  등급 리스트에 있는 증강을 아무거나 랜덤으로 하나 가져온다.
    for (int i = 0; i < selectedAugments.Length; i++)
    {
        //  남아있는 증강들의 개수
        int remainingCount = augmentGradeA.Count + augmentGradeB.Count + augmentGradeC.Count;

        //  카드가 받을 증강 데이터가 이제 더이상 존재하지 않는다면, 반복문 종료
        //  해당 카드는 증강 데이터가 없으니 null을 받는다.
        if (remainingCount <= 0)
        {
            break;
        }

        int randomGrade = Random.Range(1, 101);

        //  등급 C-----------------------------
        if (randomGrade >= 1 && randomGrade < gradeC + 1)
        {
            //  C등급 증강이 존재한다면
            if (augmentGradeC.Count > 0)
            {
                int randomIndex = Random.Range(0, augmentGradeC.Count);
                selectedAugments[i] = augmentGradeC[randomIndex];

                //  만약 3개의 카드 전부 C등급이 뜰 수 있다.
                //  이때, C등급 내에서 동일한 증강이 나오는 걸 막기 위해 중복을 제거한다.
                augmentGradeC.RemoveAt(randomIndex);
            }

            //  C등급 증강이 존재하지 않는다면 다른 등급에서 대신 추첨
            else
            {
                //  사용 가능한 타 등급 증강들만 따로 모아서 하나의 리스트로 만든다
                List<List<AugmentsData>> replaceGradeLists = new List<List<AugmentsData>>();

                if (augmentGradeB.Count > 0)
                {
                    replaceGradeLists.Add(augmentGradeB);
                }

                if (augmentGradeA.Count > 0)
                {
                    replaceGradeLists.Add(augmentGradeA);
                }

                int randomGradeClass = Random.Range(0, replaceGradeLists.Count);
                List<AugmentsData> selectedGradeList = replaceGradeLists[randomGradeClass];

                int replaceRandomAugment = Random.Range(0, selectedGradeList.Count);

                selectedAugments[i] = selectedGradeList[replaceRandomAugment];
                selectedGradeList.RemoveAt(replaceRandomAugment);
            }
        }

        //  등급 B-----------------------------
        else if (randomGrade >= gradeC && randomGrade < gradeC + gradeB + 1)
        {
            //  B등급 증강이 존재한다면
            if (augmentGradeB.Count > 0)
            {
                int randomIndex = Random.Range(0, augmentGradeB.Count);
                selectedAugments[i] = augmentGradeB[randomIndex];

                //  만약 3개의 카드 전부 B등급이 뜰 수 있다.
                //  이때, B등급 내에서 동일한 증강이 나오는 걸 막기 위해 중복을 제거한다
                augmentGradeB.RemoveAt(randomIndex);
            }

            //  B등급 증강이 존재하지 않는다면 C,A등급 증강을 추첨한다
            else
            {
                //  사용 가능한 타 등급 증강들만 따로 모아서 하나의 리스트로 만든다
                List<List<AugmentsData>> replaceGradeLists = new List<List<AugmentsData>>();

                if (augmentGradeC.Count > 0)
                {
                    replaceGradeLists.Add(augmentGradeC);
                }

                if (augmentGradeA.Count > 0)
                {
                    replaceGradeLists.Add(augmentGradeA);
                }

                //  존재하는 등급 리스트 중 하나를 선택한다.
                int randomGradeClass = Random.Range(0, replaceGradeLists.Count);
                List<AugmentsData> selectedGradeList = replaceGradeLists[randomGradeClass];

                int replaceRandomAugment = Random.Range(0, selectedGradeList.Count);

                selectedAugments[i] = selectedGradeList[replaceRandomAugment];
                selectedGradeList.RemoveAt(replaceRandomAugment);
            }
        }

        // 등급 A-----------------------------
        else if (randomGrade >= gradeB && randomGrade < gradeC + gradeB + gradeA + 1)
        {
            //  A등급 증강이 존재한다면
            if (augmentGradeA.Count > 0)
            {
                int randomIndex = Random.Range(0, augmentGradeA.Count);
                selectedAugments[i] = augmentGradeA[randomIndex];

                //  만약 3개의 카드 전부 A등급이 뜰 수 있다.
                //  이때, A등급 내에서 동일한 증강이 나오는 걸 막기 위해 중복을 제거한다.
                augmentGradeA.RemoveAt(randomIndex);
            }

            //  A등급 증강이 존재하지 않는다면 C,B 등급에서 추첨한다
            else
            {
                //  사용 가능한 타 등급 증강들만 따로 모아서 하나의 리스트로 만든다
                List<List<AugmentsData>> replaceGradeLists = new List<List<AugmentsData>>();

                if (augmentGradeC.Count > 0)
                {
                    replaceGradeLists.Add(augmentGradeC);
                }

                if (augmentGradeB.Count > 0)
                {
                    replaceGradeLists.Add(augmentGradeB);
                }

                int randomGradeClass = Random.Range(0, replaceGradeLists.Count);
                List<AugmentsData> selectedGradeList = replaceGradeLists[randomGradeClass];

                int replaceRandomAugment = Random.Range(0, selectedGradeList.Count);

                selectedAugments[i] = selectedGradeList[replaceRandomAugment];
                selectedGradeList.RemoveAt(replaceRandomAugment);
            }
        }
    }
}
*/

/*
    //  현재 사용불가능한 증강은 나타나지 않게 수정한다.
    void Unusable(List<AugmentsData> allLocalAugments)
    {
        for (int i = allLocalAugments.Count - 1; i >= 0; i--)
        {
            if (!augmentManager.CanAppear(allLocalAugments[i]))
            {
                //  CanAppear가 false판정 받은 증강은 제거한다.
                allLocalAugments.RemoveAt(i);
            }
        }

    }

    //  Augment.cs가 참조한다.
    //  augmentManager에서 가져 올 bool값.
    //  이제 더 이상 사용가능한 증강이 있는지 판단할 때 사용한다
    public bool AreYouAlreadyTheAugment(AugmentsData[] augmentsData)
    {
        return augmentManager.HasAugments(augmentsData);
    }
    */