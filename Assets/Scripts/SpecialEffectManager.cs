using System;
using UnityEngine;
using HighlightPlus;

public class SpecialEffectManager : MonoBehaviour
{
    private HighlightEffect highlightEffect;

    public int typeOfHighlight = 0;

    private AudioClip[] mAudioClips;

    private AudioManager AudioMgr;
    void Awake()
    {
        highlightEffect = GetComponent<HighlightEffect>();
    }
    private void Start()
    {
        AudioMgr = ProjectManager.Instance.getAudioManager();
        mAudioClips = ProjectManager.Instance.getAudioClips();
    }

    private void Update()
    {
        highlightEffect.seeThrough = SeeThroughMode.Never;
        if (this.gameObject.layer == LayerMask.NameToLayer("LocalWim") && typeOfHighlight != 2)
        {
            var objColor = GetComponent<MeshRenderer>().materials[0].color;
            highlightEffect.highlighted = false;
            if (objColor == Color.black)
            {
                objColor = Color.grey * 0.5f;
            }
            objColor *= 1.1f;
            // Outline
            highlightEffect.outline = 1.0f;
            highlightEffect.outlineWidth = 0.3f;
            highlightEffect.outlineColor = objColor;
            highlightEffect.outlineQuality = HighlightPlus.QualityLevel.High;
            highlightEffect.outlineVisibility = Visibility.Normal;
            highlightEffect.outlineIndependent = true;
            //Outer Glow
            highlightEffect.glow = 0.0f;
            //Inner Glow
            highlightEffect.innerGlow = 0f;
            // Overlay
            highlightEffect.overlay = 0.0f;
            // OnOff
            highlightEffect.highlighted = true;
        }
    }
    public void Highlight(string type, bool OnOff)
    {
        if (OnOff == true)
        {
            highlightEffect.seeThrough = SeeThroughMode.Never;
            var objColor = GetComponent<MeshRenderer>().materials[0].color;
            switch (type)
            {
                case "Touch":
                    if (typeOfHighlight != 0 || typeOfHighlight == 2)
                        return;
                    if (this.gameObject.layer == LayerMask.NameToLayer("LocalWim"))
                    {
                        highlightEffect.HitFX(Color.white, 1.0f);
                    }                    
                    typeOfHighlight = 1;
                    highlightEffect.highlighted = false;
                    if (objColor == Color.black)
                    {
                        objColor = Color.grey * 0.5f;
                    }
                    objColor *= 1.3f;
                    // Outline
                    highlightEffect.outline = 0.0f;
                    highlightEffect.outlineWidth = 0f;
                    // Outer Glow
                    highlightEffect.glow = 0.0f;
                    // Inner Glow
                    highlightEffect.innerGlow = 5.0f;
                    highlightEffect.innerGlowWidth = 1.5f;
                    highlightEffect.innerGlowColor = objColor;
                    highlightEffect.innerGlowVisibility = Visibility.Normal;
                    // Overlay
                    highlightEffect.overlay = 1.0f;
                    highlightEffect.overlayColor = Color.white;
                    highlightEffect.overlayMinIntensity = 0;
                    highlightEffect.overlayAnimationSpeed = 2;
                    // OnOff
                    highlightEffect.highlighted = true;
                    break;
                case "Grab":
                   try{
                        // sometimes Audio Manager does not get the reference
                        // Rerun the Start to get it
                        if (AudioMgr is null)
                            Start();
                        AudioMgr.setAudioClip(mAudioClips[1]);
                        AudioMgr.playSound();
                    }
                    catch(Exception ex)
                    {
                        Debug.LogException(ex);
                    }

                    typeOfHighlight = 2;
                    highlightEffect.highlighted = false;
                    if (objColor == Color.black)
                    {
                        objColor = Color.grey * 0.5f;
                    }
                    objColor *= 1.6f;
                    // Outline
                    highlightEffect.outline = 1.0f;
                    highlightEffect.outlineWidth = 0.65f;
                    highlightEffect.outlineColor = objColor;
                    highlightEffect.outlineQuality = HighlightPlus.QualityLevel.High;
                    highlightEffect.outlineVisibility = Visibility.Normal;
                    highlightEffect.outlineIndependent = true;
                    //Outer Glow
                    highlightEffect.glow = 0f;
                    highlightEffect.glowWidth = 0.31f;
                    highlightEffect.glowQuality = HighlightPlus.QualityLevel.High;
                    highlightEffect.glowDithering = false;
                    highlightEffect.glowPasses = new GlowPassData[0];
                    //Inner Glow
                    highlightEffect.innerGlow = 0f;
                    // Overlay
                    highlightEffect.overlay = 1.0f;
                    highlightEffect.overlayColor = objColor;
                    highlightEffect.overlayAnimationSpeed = 0;
                    // OnOff
                    highlightEffect.highlighted = true;
                    break;
            }
        }
        else
        {
            if (typeOfHighlight != 2)
            {
                typeOfHighlight = 0;
                highlightEffect.highlighted = false;
            }
        }
    }
}
