using UnityEngine;

namespace MiniGame.UnityHost
{
    [CreateAssetMenu(menuName = "MiniGame/全局交互配置", fileName = "RoomInteractionSettings")]
    public sealed class RoomInteractionSettings : ScriptableObject
    {
        public const string ResourcePath = "MiniGame/RoomInteractionSettings";

        [InspectorName("监督员对话后锁门")]
        [Tooltip("第 1 天及之后，监督员对话结束后锁住房门；当天初次会面和次日早晨仍可开门。关闭后允许再次开门，但不重复监督员对白。运行中修改立即生效，读档时使用当前全局设置。")]
        public bool LockDoorAfterSupervisor = true;
    }
}
