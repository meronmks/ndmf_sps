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
            // Contactを作る
            var generating = InPhase(BuildPhase.Generating).BeforePlugin("nadena.dev.modular-avatar");
            generating.Run("Find SPS for NDMF Components", ctx => Processor.CreateComponent(ctx));
            
            // Animationを作る
            
            // シェーダーの差し替えとかはMAやTTTの後
            // var transformingPostProcess = InPhase(BuildPhase.Transforming).AfterPlugin("nadena.dev.modular-avatar");
            // transformingPostProcess.Run("Remove Component", ctx => Processor.RemoveComponent(ctx));
        }
    }
}