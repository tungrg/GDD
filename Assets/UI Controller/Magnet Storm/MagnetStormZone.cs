using UnityEngine;
using System.Collections;

public class MagnetStormZone : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 20f;      
    public float damageInterval = 2f; 
    public float damage = 20f;       
    public float radius = 5f;        
    public LayerMask enemyLayer;     

    private void Start()
    {
        StartCoroutine(ZoneEffect());
        Destroy(gameObject, duration); 
    }

    IEnumerator ZoneEffect()
    {
        int ticks = Mathf.CeilToInt(duration / damageInterval);

        for (int i = 0; i < ticks; i++)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayer);
            foreach (Collider hit in hits)
            {
                Enemy e = hit.GetComponent<Enemy>();
                if (e != null)
                {
                    e.TakeDamage(damage);
                    e.Stun(2f);
                }
            }
            yield return new WaitForSeconds(damageInterval);
        }
    }
}
