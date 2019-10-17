using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;
using UnityEngine.UI;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance;

    public GameObject MainCamera;
    public GameObject ScentCamera;
    public Image FadeImage;
    public bool UseWind = false;
    public List<VFXEffect> VFXEffects;
    public Transform WindPosition;
    public float MaxAmountOfParticles;
    public float MaxAmountOfParticlesObject;

    public float maxScentTime = 5.0f;
    public float fadeTime = 3.0f;
    
    private bool updateTime;
    public bool Smelling;
    private float scentStrength = 0f;
    private float currentFade;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        VFXEffects = new List<VFXEffect>();
        updateTime = true;
        ScentCamera.SetActive(false);
        FadeImage.color = new Color(0f, 0f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!UseWind) {
            if (Input.GetKeyDown(KeyCode.Q)) {
                Smelling = true;
                currentFade = fadeTime;
            }
            else if (Input.GetKeyUp(KeyCode.Q)) {
                Smelling = false;
                currentFade = fadeTime;
            }

            if(!Smelling) {
                if(currentFade > 0f) {
                    float part = 1f - (currentFade / fadeTime);
                    currentFade -= Time.deltaTime;
                    if (part <= 0.5f) {
                        FadeImage.color = new Color(0f, 0f, 0f, part * 2f);
                        if (currentFade <= fadeTime / 2f) {
                            MainCamera.SetActive(true);
                            ScentCamera.SetActive(false);
                        }
                    }
                    else {
                        part -= 0.5f;
                        FadeImage.color = new Color(0f, 0f, 0f, 1f - part * 2f);
                    }
                }
            }

            ApplyScent();
        }

        if (!updateTime)
            return;

        updateTime = false;
        Invoke("SetUpdateTrue", 0.1f);

        if (UseWind)
            ApplyWind();

    }

    private void ApplyWind() {
        foreach (VFXEffect vfxEffect in VFXEffects) {
            vfxEffect.SetWindPosition(WindPosition.transform.position);

            Vector3 directionWind = (vfxEffect.transform.position - WindPosition.transform.position).normalized;
            Vector3 directionPlayer = (RoomManager.Instance.Player.transform.position - vfxEffect.transform.position).normalized;
            float angle = Vector3.Angle(directionWind, directionPlayer) / 180f;
            angle = 1f - angle;
            angle = Mathf.Pow(angle, 3f);

            if (vfxEffect.StrongScent)
                vfxEffect.SetMinAndMaxParticles(MaxAmountOfParticles * angle / 2f, MaxAmountOfParticles * angle);
            else
                vfxEffect.SetMinAndMaxParticles(MaxAmountOfParticlesObject * angle / 2f, MaxAmountOfParticlesObject * angle);
        }
    }

    private void ApplyScent() {
        if(currentFade > 0f && Smelling) {
            float part = 1f - (currentFade / fadeTime);
            currentFade -= Time.deltaTime;
            if (part <= 0.5f) {
                FadeImage.color = new Color(0f, 0f, 0f, part * 2f);
                if(currentFade <= fadeTime / 2f) {
                    MainCamera.SetActive(false);
                    ScentCamera.SetActive(true);
                }
            }
            else {
                part -= 0.5f;
                FadeImage.color = new Color(0f, 0f, 0f, 1f - part * 2f); 
            }
            return;
        }

        foreach (VFXEffect vfxEffect in VFXEffects) {
            vfxEffect.SetWindPosition(WindPosition.transform.position);

            if (scentStrength < 1f && Smelling)
                scentStrength += Time.deltaTime / maxScentTime;

            else if (scentStrength > 0f && !Smelling)
                scentStrength -= Time.deltaTime / maxScentTime;

            if (vfxEffect.StrongScent)
                vfxEffect.SetMinAndMaxParticles(MaxAmountOfParticles * scentStrength / 2f, MaxAmountOfParticles * scentStrength);
            else
                vfxEffect.SetMinAndMaxParticles(MaxAmountOfParticlesObject * scentStrength / 2f, MaxAmountOfParticlesObject * scentStrength);
        }
    }

    private void SetUpdateTrue() {
        updateTime = true;
    }
}
