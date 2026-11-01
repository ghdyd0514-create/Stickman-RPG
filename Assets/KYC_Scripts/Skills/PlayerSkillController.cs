using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    public bool isSkillLocked = false;

    public void LockSkill()
    {
        isSkillLocked = true;
    }

    public void UnlockSkill()
    {
        isSkillLocked = false;
    }

    public bool CanUseSkill()
    {
        return !isSkillLocked;
    }
}