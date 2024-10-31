//Reference1 : Universal RP URP RenderGraph Samples
//How to import PacakgeManager > Universal RP > Samples > URP RenderGraph Samples
//How to access Assets > Universal RP > 17.0.3 > URP RenderGraph Samples > Compute
//Reference2 : https://github.com/robin-boucher/URPRendererFeatureExample6/tree/master/Assets/Samples/

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;

public class Chapter3RenderFreature : ScriptableRendererFeature
{
    public class Chapter3RenderPass : ScriptableRenderPass
    {
        private const string PASS_NAME = "Chapter3RenderPass";

        //リソースハンドルと参照を保持するためのクラス
        //必須:RenderGraphBuilder.SetRenderFuncで使う
        private class PassData
        {
            public RendererListHandle rendererListHandle;
            public RendererListHandle skyboxRendererListHandle;
        }

        private RenderQueueType renderQueueType;
        private FilteringSettings filteringSettings;
        private List<ShaderTagId> shaderTagIds;
        private string renderTargetName;
        private int renderTargetId;

        private int renderDepthTargetId;

        //コンストラクタ内でRenderPass内で固定できるものを解決する
        public Chapter3RenderPass(string renderTargetname, RenderPassEvent passEvent, RenderQueueType queueType, LayerMask layerMask)
        {
            profilingSampler = new ProfilingSampler(nameof(Chapter3RenderPass));
            renderPassEvent = passEvent;
            renderQueueType = queueType;
            renderTargetName = renderTargetname;
            renderTargetId = Shader.PropertyToID(renderTargetname);

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
                //TextureHandle cameraColorTextureHandle = resourceData.activeColorTexture;
                RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                desc.colorFormat = RenderTextureFormat.ARGB32;
                desc.msaaSamples = 1;
                desc.depthBufferBits = 0;

                TextureHandle targetTextureHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, renderTargetName, true);

                RenderTextureDescriptor depthDesc = cameraData.cameraTargetDescriptor;
                desc.colorFormat = RenderTextureFormat.Depth;
                desc.msaaSamples = 0;
                desc.depthBufferBits = 24;

                TextureHandle targetDepthTextureHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_CustomRenderDepthTex", true);


                SortingCriteria sortingCriteria = renderQueueType == RenderQueueType.Opaque ? cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;

                //該当のShaderTagを描画する設定
                DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(shaderTagIds, renderingData, cameraData, lighData, sortingCriteria);
                RendererListParams rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);

                //描画対象のRenderer(オブジェクト)リストのハンドル
                passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParams);
                passData.skyboxRendererListHandle = CreateSkyBoxRendererList(renderGraph, cameraData);

                builder.UseRendererList(passData.rendererListHandle);
                builder.UseRendererList(passData.skyboxRendererListHandle);

                //レンダーターゲットに書き込む設定
                builder.SetRenderAttachment(targetTextureHandle, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(resourceData.cameraDepthTexture, AccessFlags.Write);

                builder.SetGlobalTextureAfterPass(targetTextureHandle, renderTargetId);//Shader内から_CustomRenderTexでアクセスできる

                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PassData passData, RasterGraphContext graphContext) => ExecutePass(passData, graphContext));
            }
        }

        private static void ExecutePass(PassData passData, RasterGraphContext graphContext)
        {
            RasterCommandBuffer cmd = graphContext.cmd;
            //描画
            cmd.ClearRenderTarget(true, true, Color.green);
            cmd.DrawRendererList(passData.rendererListHandle);
            cmd.DrawRendererList(passData.skyboxRendererListHandle);

        }

        private RendererListHandle CreateSkyBoxRendererList(RenderGraph renderGraph, UniversalCameraData cameraData)
        {
            var skyRendererListHandle = renderGraph.CreateSkyboxRendererList(cameraData.camera);
            return skyRendererListHandle;
        }
    }

    [SerializeField] private string renderTagetName = "_CustomRenderTex";
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    [SerializeField] private RenderQueueType renderQueueType = RenderQueueType.Opaque;
    [SerializeField] private LayerMask layerMask = -1;

    private Chapter3RenderPass renderPass;

    public override void Create()
    {
        renderPass = new Chapter3RenderPass(renderTagetName, renderPassEvent, renderQueueType, layerMask);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }

    protected override void Dispose(bool disposing)
    {
        //破棄するリソースはここに
    }
}
