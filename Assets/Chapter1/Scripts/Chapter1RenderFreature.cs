//Reference1 : Universal RP URP RenderGraph Samples
//How to import PacakgeManager > Universal RP > Samples > URP RenderGraph Samples
//How to access Assets > Universal RP > 17.0.3 > URP RenderGraph Samples > Compute
//Reference2 : https://github.com/robin-boucher/URPRendererFeatureExample6/tree/master/Assets/Samples/

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;

public class Chapter1RenderFreature : ScriptableRendererFeature
{
    public class Chapter1RenderPass : ScriptableRenderPass
    {
        private const string PASS_NAME = "Chapter1RenderPass";
        //リソースハンドルと参照を保持するためのクラス
        //必須:RenderGraphBuilder.SetRenderFuncで使う
        private class PassData
        {
            public RendererListHandle rendererListHandle;
        }

        private RenderQueueType renderQueueType;
        private FilteringSettings filteringSettings;
        private List<ShaderTagId> shaderTagIds;

        //コンストラクタ内でRenderPass内で固定できるものを解決する
        public Chapter1RenderPass(RenderPassEvent passEvent, RenderQueueType queueType, LayerMask layerMask)
        {
            profilingSampler = new ProfilingSampler(nameof(Chapter1RenderPass));
            renderPassEvent = passEvent;
            renderQueueType = queueType;

            RenderQueueRange renderQueueRange = renderQueueType == RenderQueueType.Opaque ? RenderQueueRange.opaque : RenderQueueRange.transparent;
            // RenderQueueTypeのオブジェクトかつ該当するlayerMaskのオブジェクトをフィルタリングする設定
            filteringSettings = new FilteringSettings(renderQueueRange, layerMask);

            //ShaderのTags設定のリスト
            //該当するシェーダーが実行される
            //通常はSubShader内に書かれる
            shaderTagIds = new List<ShaderTagId>() {
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly")
            };
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lighData = frameData.Get<UniversalLightData>();

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(PASS_NAME, out PassData passData, profilingSampler))
            {
                SortingCriteria sortingCriteria = renderQueueType == RenderQueueType.Opaque ? cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;

                //該当のShaderTagを描画する設定
                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(shaderTagIds, renderingData, cameraData, lighData, sortingCriteria);
                RendererListParams rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);

                //描画対象のRenderer(オブジェクト)リストのハンドル
                passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParams);

                builder.UseRendererList(passData.rendererListHandle);

                //レンダーターゲットに書き込む設定
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

                builder.SetRenderFunc((PassData passData, RasterGraphContext graphContext) => ExecutePass(passData, graphContext));
            }
        }

        private static void ExecutePass(PassData passData, RasterGraphContext graphContext)
        {
            RasterCommandBuffer cmd = graphContext.cmd;
            //描画
            cmd.DrawRendererList(passData.rendererListHandle);
        }
    }

    [SerializeField] private RenderPassEvent renderPassEvent;
    [SerializeField] private RenderQueueType renderQueueType;
    [SerializeField] private LayerMask layerMask = -1;

    private Chapter1RenderPass renderPass;

    public override void Create()
    {
        renderPass = new Chapter1RenderPass(renderPassEvent, renderQueueType, layerMask);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }

    protected override void Dispose(bool disposing)
    {
        //破棄するものがあれば
    }
}
