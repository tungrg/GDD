using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipHealer : MonoBehaviour
{
    [Header("Heal Settings")]
    public float healAmount = 20f;                
    public float throwHeight = 3f;                
    public float throwDuration = 1f;              
    public List<GameObject> healPrefabs;          

    [Header("Detection")]
    public string enemyTag = "Enemy";             
    public float triggerRadius = 5f;              
    public float throwInterval = 1f;              

    [Header("Effects")]
    public float effectDuration = 1f;             

    private void Start()
    {
        // Mỗi toa sẽ có 1 delay random khác nhau trước khi bắt đầu heal
        float randomDelay = Random.Range(0f, throwInterval);
        StartCoroutine(StartWithDelay(randomDelay));
    }

    private IEnumerator StartWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(HealLoop());
    }

    private IEnumerator HealLoop()
    {
        while (true)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, triggerRadius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag(enemyTag))
                {
                    StartCoroutine(ThrowHealBox(hit.transform));
                }
            }
            yield return new WaitForSeconds(throwInterval);
        }
    }

    private IEnumerator ThrowHealBox(Transform target)
    {
        if (healPrefabs.Count == 0 || target == null) yield break;

        GameObject prefab = healPrefabs[Random.Range(0, healPrefabs.Count)];
        GameObject box = Instantiate(prefab, transform.position, Quaternion.identity);

        Vector3 start = transform.position;
        Vector3 end = target.position;

        float time = 0f;
        while (time < throwDuration)
        {
            time += Time.deltaTime;
            float t = time / throwDuration;

            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y += throwHeight * (1 - Mathf.Abs(2 * t - 1));
            box.transform.position = pos;

            yield return null;
        }

        if (target != null)
        {
            BossManager enemyHp = target.GetComponent<BossManager>();
            if (enemyHp != null)
            {
                enemyHp.Heal(healAmount);

                Transform effect = target.Find("HealEffect");
                if (effect != null)
                {
                    TriggerHealEffect(effect.gameObject);
                }
            }
        }

        Destroy(box);
    }

    private Dictionary<GameObject, Coroutine> activeEffects = new Dictionary<GameObject, Coroutine>();

    private void TriggerHealEffect(GameObject effectObj)
    {
        // Nếu effect chưa bật thì bật
        effectObj.SetActive(true);

        // Nếu effect này đang chạy coroutine → dừng lại
        if (activeEffects.ContainsKey(effectObj) && activeEffects[effectObj] != null)
        {
            StopCoroutine(activeEffects[effectObj]);
        }

        // Tạo coroutine mới để tắt sau effectDuration
        Coroutine co = StartCoroutine(DisableAfter(effectObj, effectDuration));
        activeEffects[effectObj] = co;
    }

    private IEnumerator DisableAfter(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
        activeEffects[obj] = null;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
