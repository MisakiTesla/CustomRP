using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public partial class CameraRender
{
#if UNITY_EDITOR
    private static ShaderTagId[] _legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM"),
    };

    private static Material errorMaterial;


    /// <summary>
    /// 绘制SRP不支持的着色器类型
    /// </summary>
    partial void DrawUnSupportedShaders()
    {
        if (!errorMaterial)
        {
            errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        var drawingSettings = new DrawingSettings(_legacyShaderTagIds[0], new SortingSettings(_camera))
        {
            overrideMaterial = errorMaterial
        };
        for (int i = 1; i < _legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, _legacyShaderTagIds[i]);
        }

        var filteringSettings = FilteringSettings.defaultValue;

        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }
    
    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }
    }
    
    partial void PrepareForSceneWindow()
    {
        if (_camera.cameraType == CameraType.SceneView)
        {
            //如果切换到了Scene视图,调用此方法完成绘制
            ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
    }
    
    partial void PrepareBuffer()
    {
        //设置一下只有在编辑器模式下才分配内存
        Profiler.BeginSample("Editor Only");
        _buffer.name = SampleName = _camera.name;
        Profiler.EndSample();
    }
#endif
}