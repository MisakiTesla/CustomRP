using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRender
{
    private ScriptableRenderContext _context;
    private Camera _camera;
    private const string BUFFER_NAME = "Render Camera";
#if UNITY_EDITOR
    string SampleName { get; set; }
#else
    const string SampleName = BUFFER_NAME;
#endif

    
    private static ShaderTagId _unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    private CommandBuffer _buffer = new CommandBuffer() {name = BUFFER_NAME};
    private CullingResults _cullingResults;
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        _context = context;
        _camera = camera;
        //设置命令缓冲区的名字
        PrepareBuffer();

        PrepareForSceneWindow();

        if (!Cull())
        {
            return;
        }
        
        Setup();
        //绘制几何体
        DrawVisibleGeometry();
        //绘制SRP不支持的着色器类型
        DrawUnSupportedShaders();
        //绘制Gizmos
        DrawGizmos();
        Submit();
    }

    /// <summary>
    /// 设置相机属性和矩阵
    /// </summary>
    private void Setup()
    {
        _context.SetupCameraProperties(_camera);
        //得到相机的clear flags
        CameraClearFlags flags = _camera.clearFlags;
        //设置相机清除状态
        _buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth, 
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? _camera.backgroundColor.linear : Color.clear);
        ////清空上一帧的FrameBuffer
        //_buffer.ClearRenderTarget(true, true, Color.clear);
        _buffer.BeginSample(SampleName);
        ExcuteBuffer();
    }

    private void ExcuteBuffer()
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    /// <summary>
    /// 提交缓冲区渲染命令
    /// </summary>
    private void Submit()
    {
        _buffer.EndSample(SampleName);
        ExcuteBuffer();
        _context.Submit();
    }

    /// <summary>
    /// 绘制可见物体
    /// </summary>
    private void DrawVisibleGeometry()
    {
        //不透明物体->天空盒->透明物体
        //设置绘制顺序和指定渲染相机
        var sortingSettings = new SortingSettings(_camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        //设置要渲染的Shader Pass 和排序模式
        var drawingSettings = new DrawingSettings(_unlitShaderTagId, sortingSettings);
        //设置只绘制RenderQueue为opaque的不透明物体
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        
        //1.绘制不透明物体
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        //2.绘制天空盒
        _context.DrawSkybox(_camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        //设置只绘制RenderQueue为transparent的不透明物体
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        
        //3.绘制透明物体
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }

    partial void DrawUnSupportedShaders();
    partial void DrawGizmos();
    /// <summary>
    /// 在Game视图绘制的几何体也绘制到Scene视图中
    /// </summary>
    partial void PrepareForSceneWindow();
    partial void PrepareBuffer();


    private bool Cull()
    {
        ScriptableCullingParameters p;
        if (_camera.TryGetCullingParameters(out p))
        {
            _cullingResults = _context.Cull(ref p);
            return true;
        }

        return false;
    }
}
