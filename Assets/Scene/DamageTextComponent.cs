
using UnityEngine;

class DamageTextComponent : MonoBehaviour
{
    public void DamageNumberAnimation_End()
    {
        GameObject.Destroy(transform.parent.gameObject);
    }
}

