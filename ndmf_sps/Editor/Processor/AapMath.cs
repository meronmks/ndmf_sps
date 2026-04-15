using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace com.meronmks.ndmfsps
{
    /// <summary>
    /// Direct BlendTree内でAAP (Animator As Property) を使った演算を行うユーティリティ。
    /// VRCFury v1.1001.0のMathServiceに相当する軽量版。
    /// </summary>
    internal class AapMath
    {
        private readonly AnimatorController _controller;
        private readonly BlendTree _directTree;
        private readonly HashSet<string> _registeredParams = new HashSet<string>();
        private readonly Dictionary<string, float> _paramDefaults = new Dictionary<string, float>();
        private readonly string _paramPrefix;
        private readonly Dictionary<float, string> _cachedSpeeds = new Dictionary<float, string>();
        private bool _needsFrameTimeLayer;

        private const string AlwaysOneParam = "__ndmfsps_one";

        public BlendTree DirectTree => _directTree;

        public AapMath(AnimatorController controller, string paramPrefix = "")
        {
            _controller = controller;
            _paramPrefix = paramPrefix;
            _directTree = new BlendTree
            {
                name = "DBT",
                blendType = BlendTreeType.Direct,
                useAutomaticThresholds = false
            };
            EnsureParam(AlwaysOneParam, 1f);
        }

        public void EnsureParam(string name, float defaultValue = 0f)
        {
            if (!_registeredParams.Add(name)) return;
            _paramDefaults[name] = defaultValue;
            _controller.AddParameter(new AnimatorControllerParameter
            {
                name = name,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = defaultValue
            });
        }

        public float GetDefault(string name)
        {
            return _paramDefaults.TryGetValue(name, out var val) ? val : 0f;
        }

        public string MakeAap(string name, float defaultValue = 0f)
        {
            EnsureParam(name, defaultValue);
            AddDirect(AlwaysOneParam, MakeSetterClip(name, 0f));
            return name;
        }

        public AnimationClip MakeSetterClip(string paramName, float value)
        {
            var clip = new AnimationClip { name = $"AAP {paramName}={value}" };
            var curve = new AnimationCurve(new Keyframe(0f, value));
            clip.SetCurve("", typeof(Animator), paramName, curve);
            return clip;
        }

        public void AddDirect(string blendParam, Motion motion)
        {
            _directTree.AddChild(motion);
            var children = _directTree.children;
            var child = children[children.Length - 1];
            child.directBlendParameter = blendParam;
            children[children.Length - 1] = child;
            _directTree.children = children;
        }

        public void AddDirect(Motion motion)
        {
            AddDirect(AlwaysOneParam, motion);
        }

        public BlendTree Make1D(string name, string blendParam, params (float threshold, Motion motion)[] children)
        {
            var tree = new BlendTree
            {
                name = name,
                blendType = BlendTreeType.Simple1D,
                blendParameter = blendParam,
                useAutomaticThresholds = false
            };
            foreach (var (threshold, motion) in children)
            {
                tree.AddChild(motion ?? new AnimationClip(), threshold);
            }
            return tree;
        }

        public string Map(string outputName, string inputParam, float inMin, float inMax, float outMin, float outMax)
        {
            EnsureParam(inputParam);
            // 入力パラメータのデフォルト値から出力デフォルトを計算
            var inputDefault = GetDefault(inputParam);
            var outputDefault = (inMax - inMin) != 0
                ? outMin + (outMax - outMin) * (inputDefault - inMin) / (inMax - inMin)
                : outMin;
            outputDefault = Math.Min(Math.Max(outputDefault, Math.Min(outMin, outMax)), Math.Max(outMin, outMax));

            if (Math.Abs(inMax - inMin) < 0.00001f)
            {
                var output2 = MakeAap(outputName, outputDefault);
                AddDirect(MakeSetterClip(output2, outMin));
                return output2;
            }
            var output = MakeAap(outputName, outputDefault);

            var minClip = MakeSetterClip(output, outMin);
            var maxClip = MakeSetterClip(output, outMax);

            BlendTree tree;
            if (inMin < inMax)
            {
                tree = Make1D($"{inputParam} ({inMin}-{inMax}) -> ({outMin}-{outMax})", inputParam,
                    (inMin, minClip),
                    (inMax, maxClip));
            }
            else
            {
                tree = Make1D($"{inputParam} ({inMax}-{inMin}) -> ({outMax}-{outMin})", inputParam,
                    (inMax, maxClip),
                    (inMin, minClip));
            }
            AddDirect(tree);
            return output;
        }

        public delegate Motion ConditionFactory(Motion whenTrue, Motion whenFalse);

        public ConditionFactory GreaterThan(string paramA, float threshold, bool orEqual = false)
        {
            EnsureParam(paramA);
            var belowThreshold = orEqual ? NextFloatDown(threshold) : threshold;
            var aboveThreshold = orEqual ? threshold : NextFloatUp(threshold);
            return (whenTrue, whenFalse) => Make1D(
                $"{paramA} {(orEqual ? ">=" : ">")} {threshold}",
                paramA,
                (belowThreshold, whenFalse ?? new AnimationClip()),
                (aboveThreshold, whenTrue ?? new AnimationClip()));
        }

        /// <summary>
        /// IEEE 754 の最小精度で次に大きいfloatを返す
        /// </summary>
        private static float NextFloatUp(float input)
        {
            return NextFloat(input, 1);
        }

        /// <summary>
        /// IEEE 754 の最小精度で次に小さいfloatを返す
        /// </summary>
        private static float NextFloatDown(float input)
        {
            return NextFloat(input, -1);
        }

        private static float NextFloat(float input, int offset)
        {
            if (float.IsNaN(input) || float.IsPositiveInfinity(input) || float.IsNegativeInfinity(input))
                return input;
            if (input == 0)
                return (offset > 0) ? float.Epsilon : -float.Epsilon;

            var bytes = BitConverter.GetBytes(input);
            var bits = BitConverter.ToInt32(bytes, 0);
            if (input > 0) bits += offset;
            else bits -= offset;
            bytes = BitConverter.GetBytes(bits);
            return BitConverter.ToSingle(bytes, 0);
        }

        public void SetValueWithConditions(params (Motion motion, ConditionFactory condition)[] targets)
        {
            Motion elseTree = null;
            foreach (var (motion, condition) in targets.Reverse())
            {
                if (condition == null)
                {
                    elseTree = motion;
                    continue;
                }
                elseTree = condition(motion, elseTree);
            }
            if (elseTree != null) AddDirect(elseTree);
        }

        public Motion MakeCopier(string fromParam, string toParam)
        {
            EnsureParam(fromParam);
            var subTree = new BlendTree
            {
                name = $"Copy {fromParam} -> {toParam}",
                blendType = BlendTreeType.Direct,
                useAutomaticThresholds = false
            };
            var setterClip = MakeSetterClip(toParam, 1f);
            subTree.AddChild(setterClip);
            var children = subTree.children;
            var child = children[0];
            child.directBlendParameter = fromParam;
            children[0] = child;
            subTree.children = children;
            return subTree;
        }

        public Motion MakeConstSetter(string toParam, float value)
        {
            return MakeSetterClip(toParam, value);
        }

        /// <summary>
        /// パラメータに定数を掛けたAAPを作成。output = param * constant
        /// </summary>
        public string Multiply(string name, string param, float constant)
        {
            EnsureParam(param);
            var output = MakeAap(name);
            AddDirect(param, MakeSetterClip(output, constant));
            return output;
        }

        /// <summary>
        /// パラメータの1フレーム遅延コピーを作成。
        /// Direct BlendTreeは前フレームのパラメータ値を読むため、
        /// copierを追加するだけで1フレーム遅延が得られる。
        /// </summary>
        public string Buffer(string fromParam, string toName = null)
        {
            EnsureParam(fromParam);
            toName = toName ?? $"{fromParam}_buf";
            var output = MakeAap(toName);
            AddDirect(MakeCopier(fromParam, output));
            return output;
        }

        /// <summary>
        /// a - b を計算するAAPを作成
        /// </summary>
        public string Subtract(string name, string paramA, string paramB)
        {
            EnsureParam(paramA);
            EnsureParam(paramB);
            var output = MakeAap(name);
            AddDirect(paramA, MakeSetterClip(output, 1f));
            AddDirect(paramB, MakeSetterClip(output, -1f));
            return output;
        }

        private string _frameTimeParam;

        /// <summary>
        /// フレームデルタタイムパラメータを取得（遅延初期化）。
        /// 初回呼び出し時にフレームタイム計測用レイヤーのセットアップをマークする。
        /// </summary>
        private string GetOrCreateFrameTime()
        {
            if (_frameTimeParam != null) return _frameTimeParam;
            _needsFrameTimeLayer = true;

            var prefix = string.IsNullOrEmpty(_paramPrefix) ? "__ndmfsps" : _paramPrefix;
            var timeSinceLoadParam = $"{prefix}/__time";
            EnsureParam(timeSinceLoadParam);

            var lastTimeParam = Buffer(timeSinceLoadParam, $"{prefix}/__lastTime");
            _frameTimeParam = Subtract($"{prefix}/__frameTime", timeSinceLoadParam, lastTimeParam);
            return _frameTimeParam;
        }

        /// <summary>
        /// VRCFuryのGetFramesRequired相当。
        /// fractionPerFrameの速度で2パススムージングした場合に
        /// 90%に到達するフレーム数を返す。
        /// </summary>
        private static int GetFramesRequired(float fractionPerFrame)
        {
            var targetFraction = 0.9f;
            float target = 0f;
            float position = 0f;
            for (var frame = 1; frame < 1000; frame++)
            {
                target += (1 - target) * fractionPerFrame;
                position += (target - position) * fractionPerFrame;
                if (position >= targetFraction) return frame;
            }
            return 1000;
        }

        /// <summary>
        /// smoothingSecondsからfractionPerSecondを算出する。
        /// 二分探索で目標フレーム数に合うfractionPerFrameを見つけ、
        /// それを60倍してfractionPerSecondにする。
        /// </summary>
        private static float CalculateFractionPerSecond(float smoothingSeconds)
        {
            var framerateForCalculation = 60;
            var targetFrames = smoothingSeconds * framerateForCalculation;
            var currentSpeed = 0.5f;
            var nextStep = 0.25f;
            for (var i = 0; i < 20; i++)
            {
                var currentFrames = GetFramesRequired(currentSpeed);
                if (currentFrames > targetFrames)
                    currentSpeed += nextStep;
                else
                    currentSpeed -= nextStep;
                nextStep *= 0.5f;
            }
            return currentSpeed * framerateForCalculation;
        }

        /// <summary>
        /// 単一パスのスムージング。
        /// 1D BlendTreeで[maintain current, use target]を速度パラメータで補間する。
        /// </summary>
        private string SmoothSinglePass(string name, string targetParam, string speedParam)
        {
            var output = MakeAap(name, GetDefault(targetParam));

            // maintainTree: 現在値を維持 (output → output)
            var maintainTree = MakeCopier(output, output);

            // targetTree: 目標値にジャンプ (target → output)
            var targetTree = MakeCopier(targetParam, output);

            // speedで補間: speed=0→maintain, speed=1→target
            var smoothTree = Make1D(
                $"{name} smoothto {targetParam}",
                speedParam,
                (0f, maintainTree),
                (1f, targetTree));

            AddDirect(smoothTree);
            return output;
        }

        /// <summary>
        /// パラメータにスムージングを適用する（2パス、加速度付き）。
        /// VRCFury v1.1001.0のSmoothingService.Smooth相当。
        /// </summary>
        public string Smooth(string name, string targetParam, float smoothingSeconds)
        {
            if (smoothingSeconds <= 0) return targetParam;
            if (smoothingSeconds > 10) smoothingSeconds = 10;

            // 同じsmoothingSecondsのSpeedパラメータをキャッシュして共有
            if (!_cachedSpeeds.TryGetValue(smoothingSeconds, out var speedParam))
            {
                var frameTime = GetOrCreateFrameTime();
                var fractionPerSecond = CalculateFractionPerSecond(smoothingSeconds);
                speedParam = Multiply($"smoothingSpeed/{smoothingSeconds}", frameTime, fractionPerSecond);
                _cachedSpeeds[smoothingSeconds] = speedParam;
            }

            // 2パス: 加速度付きスムージング
            // Pass1は中間パラメータ、Pass2は呼び出し元が期待するnameに直接書き込む
            var pass1 = SmoothSinglePass($"{name}/Pass1", targetParam, speedParam);
            return SmoothSinglePass(name, pass1, speedParam);
        }

        /// <summary>
        /// AnimatorControllerにすべてのレイヤーを追加する。
        /// フレームタイムレイヤー（必要時）+ DBTレイヤーを作成。
        /// </summary>
        public void FinalizeController(string dbtLayerName)
        {
            if (_needsFrameTimeLayer)
            {
                var prefix = string.IsNullOrEmpty(_paramPrefix) ? "__ndmfsps" : _paramPrefix;
                var timeSinceLoadParam = $"{prefix}/__time";

                _controller.AddLayer("FrameTime Counter");
                var ftIdx = _controller.layers.Length - 1;
                var ftLayer = _controller.layers[ftIdx];
                ftLayer.defaultWeight = 1f;
                var layers = _controller.layers;
                layers[ftIdx] = ftLayer;
                _controller.layers = layers;

                var clip = new AnimationClip { name = "FrameTime Counter" };
                clip.SetCurve("", typeof(Animator), timeSinceLoadParam,
                    AnimationCurve.Linear(0, 0, 10_000_000, 10_000_000));

                var ftState = ftLayer.stateMachine.AddState("Time");
                ftState.motion = clip;
                ftState.writeDefaultValues = true;
            }

            _controller.AddLayer(dbtLayerName);
            var dbtIdx = _controller.layers.Length - 1;
            var dbtLayer = _controller.layers[dbtIdx];
            dbtLayer.defaultWeight = 1f;
            var dbtLayers = _controller.layers;
            dbtLayers[dbtIdx] = dbtLayer;
            _controller.layers = dbtLayers;

            var dbtState = dbtLayer.stateMachine.AddState("DBT");
            dbtState.motion = _directTree;
            dbtState.writeDefaultValues = true;
        }
    }
}
