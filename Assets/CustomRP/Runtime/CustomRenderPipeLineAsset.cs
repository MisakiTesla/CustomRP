﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/CreateCustomRenderPipeline")]
public class CustomRenderPipeLineAsset : RenderPipelineAsset
{
    
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline();
    }
}
