using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRender _cameraRender = new CameraRender();
    /// <summary>
    /// Unity每一帧都会调用CustomRenderPipeline实例的Render()方法进行画面渲染，
    /// 该方法是SRP的入口，进行渲染时底层接口会调用它并传递两个参数，
    /// 一个是ScriptableRenderContext对象，一个是Camera[]对象。
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cameras"></param>
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            _cameraRender.Render(context, camera);
        }
        
    }
}
