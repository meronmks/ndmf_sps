using UnityEngine;

namespace com.meronmks.spsforndmf.runtime
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

        public enum Haptics
        {
            On,
            Off
        }

        public Haptics haptics;
    }
}