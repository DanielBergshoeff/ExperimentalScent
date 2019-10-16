using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class VFXEffect : MonoBehaviour
{
    public bool StrongScent = false;
    private VisualEffect VFX;

    // Start is called before the first frame update
    void Start()
    {
        VFX = GetComponent<VisualEffect>();
        VFXManager.Instance.VFXEffects.Add(this);
    }

    public void SetWindPosition(Vector3 v3) {
        VFX.SetVector3("WindPosition", v3);
    }

    public void SetMinAndMaxParticles(float min, float max) {
        VFX.SetFloat("SpawnMinRate", min);
        VFX.SetFloat("SpawnMaxRate", max);
    }
}
