using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.meronmks.ndmfsps.runtime
{
    [AddComponentMenu(Values.COMPONENTS_BASE + nameof(Socket))]
    public class Socket : SPSforNDMFTagComponent
    {
        public enum SocketMode
        {
            None,
            Hole,
            Ring,
            RingOneWay
        }

        public SocketMode mode;
        public bool enableActiveAnimation;
        [SerializeReference, SubclassSelector(typeof(IAction))] public List<IAction> activeAnimationActions = new ();

        public enum Haptics
        {
            On,
            Off
        }

        public Haptics haptics;

        public float length;
        public bool unitsInMeters = true;
        public bool sendersOnly = false;
    }
}