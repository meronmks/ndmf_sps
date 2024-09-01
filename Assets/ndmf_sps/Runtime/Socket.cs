using System.Collections.Generic;
using UnityEngine;

namespace com.meronmks.ndmfsps.runtime
{
    [AddComponentMenu(Values.COMPONENTS_BASE + nameof(Socket))]
    public class Socket : SPSforNDMFTagComponent
    {
        public enum SocketMode
        {
            Hole,
            Ring
        }

        public SocketMode mode;
        public bool enableActiveAnimation;
        public List<Action> activeAnimationActions = new List<Action>();

        public enum Haptics
        {
            On,
            Off
        }

        public Haptics haptics;
    }
}