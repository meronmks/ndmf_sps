using com.meronmks.ndmfsps.runtime;
using JetBrains.Annotations;
using VRC.SDK3.Avatars.Components;

namespace com.meronmks.ndmfsps.meronmksTools.ndmf_sps.Editor.Model
{
    public class HandTouchZone
    {
        public float length { get; private set; }
        public float radius { get; private set; }

        HandTouchZone(float length, float radius)
        {
            this.length = length;
            this.radius = radius;
        }

        public static HandTouchZone GetHandTouchZoneSize(Socket socket, VRCAvatarDescriptor avatarDescriptor)
        {
            bool enableHandTouchZone = socket.haptics == Socket.Haptics.On;
            if (!enableHandTouchZone)
            {
                return null;
            }
            var length = socket.length * (socket.unitsInMeters ? 1f : socket.transform.lossyScale.z);
            if (length <= 0)
            {
                if (avatarDescriptor == null) return null;
                length = avatarDescriptor.ViewPosition.y * 0.05f;
            }

            var radius = length / 2.5f;
            
            return new HandTouchZone(length, radius);
        }
    }
}