using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.meronmks.ndmfsps.runtime;
using nadena.dev.ndmf;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace com.meronmks.ndmfsps
{
    [ParameterProviderFor(typeof(Socket))]
    internal class SocketParameterProvider : IParameterProvider
    {
        private readonly Socket component;
        public SocketParameterProvider(Socket component) => this.component = component;

        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null)
        {
            if (!this.component.enabled  || IsEditorOnly(component.transform)) return Array.Empty<ProvidedParameter>();
            return new ProvidedParameter[]{
                new(
                    component.name,
                    ParameterNamespace.Animator,
                    component,
                    SPSforNDMFPlugin.Instance,
                    AnimatorControllerParameterType.Bool
                ){
                    IsAnimatorOnly = false,
                    IsHidden = false,
                    WantSynced = true
                }};
        }

        public void RemapParameters(ref ImmutableDictionary<(ParameterNamespace, string), ParameterMapping> nameMap, BuildContext context = null)
        {
            // なにもなし
        }

        private bool IsEditorOnly(Transform t)
        {
            if(t.CompareTag("EditorOnly")) return true;
            if (t.parent == null)
            {
                return false;
            }
            else
            {
                return IsEditorOnly(t.parent);
            }
        }
    }
}