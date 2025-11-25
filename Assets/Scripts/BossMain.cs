using UnityEngine;

public class Boss : MonoBehaviour
{
    // HP, UI, ...
    protected bool isDead = false;
    protected Animator anim;

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
    }

    // TakeDamage()

    // Die()
}