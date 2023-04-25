using System.Collections;
using UnityEngine;

public class Cooldown : MonoBehaviour
{
    public void StartCooldown(float cooldownTime)
    {
        StartCoroutine(CooldownCoroutine(cooldownTime));
    }


    IEnumerator CooldownCoroutine(float cooldownTime)
    {
        yield return new WaitForSeconds(0.01f); // QUICK WORKAROUND small delay so it gets cloned
        GameObject egg = gameObject.GetComponentInChildren<EnemyEgg>().gameObject;
        egg.SetActive(false);
        yield return new WaitForSeconds(cooldownTime);
        egg.SetActive(true);
    }
}