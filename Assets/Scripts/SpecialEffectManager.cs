﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightPlus;

public class SpecialEffectManager : MonoBehaviour
{
    private HighlightEffect highlightEffect;

    public int typeOfHighlight = 0;

    private AudioSource myAudioSource;
    private AudioClip[] mAudioClips;
    void Awake()
    {
        highlightEffect = GetComponent<HighlightEffect>();
    }
    private void Start()
    {
        myAudioSource = ProjectManager.Instance.getCameraRig().GetComponentInChildren<AudioSource>();
        mAudioClips = ProjectManager.Instance.getAudioClips();
    }

    private void Update()
    {
        highlightEffect.seeThrough = SeeThroughMode.Never;
        if (this.gameObject.layer == LayerMask.NameToLayer("NearFieldObjects") && typeOfHighlight != 2)
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
                    if (this.gameObject.layer == LayerMask.NameToLayer("NearFieldObjects"))
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
                    if (mAudioClips != null)
                    {
                        myAudioSource.clip = mAudioClips[1];
                        myAudioSource.Play();
                    }
                    else
                        Debug.Log("Can't Find Audio Clip");
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
