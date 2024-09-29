using com.meronmks.ndmfsps;
using nadena.dev.ndmf;
using UnityEditor.Graphs;
using UnityEngine;

[assembly: ExportsPlugin(typeof(SPSforNDMFPlugin))]

namespace com.meronmks.ndmfsps
{
    public class SPSforNDMFPlugin : Plugin<SPSforNDMFPlugin>
    {
        public override Color? ThemeColor => new Color(0.213f, 0.520f, 0.742f);
        protected override void Configure()
        {
            // いろいろ作る
            var generating = InPhase(BuildPhase.Generating).BeforePlugin("nadena.dev.modular-avatar");
            generating.Run("Create SPS Components", ctx => Processor.CreateComponent(ctx));
            
            var transforming = InPhase(BuildPhase.Transforming).BeforePlugin("nadena.dev.modular-avatar");
            // Animationを作る
            transforming.Run("Create Animation", ctx => Processor.CreateAnim(ctx));
            // Menuを作る
            transforming.Run("Create Menu", ctx => Processor.CreateMenu(ctx));

            
            // シェーダーの差し替えとかはMAやTTTの後
            var transformingPostProcess = InPhase(BuildPhase.Transforming).AfterPlugin("nadena.dev.modular-avatar").AfterPlugin("net.rs64.tex-trans-tool");
            transformingPostProcess.Run("Remove Component", ctx => Processor.RemoveComponent(ctx));
        }
    }
}