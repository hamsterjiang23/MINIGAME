using System;
using UnityEngine;

namespace MiniGame.UnityHost
{
    [CreateAssetMenu(menuName = "MiniGame/人物监控室配置")]
    public sealed class PatientMonitorProfile : ScriptableObject
    {
        [Serializable]
        public sealed class Room
        {
            [InspectorName("人物 ID")] public string PatientId;
            [InspectorName("房间编号")] public string Label;
            [InspectorName("房间变化种子")] public int Seed;
            [InspectorName("软包颜色")] public Color WallTint = new Color(.74f, .77f, .72f);
            [InspectorName("顶灯颜色")] public Color LightTint = new Color(.82f, .91f, .88f);
            [InspectorName("污旧程度"), Range(0, 1)] public float Grime = .3f;
            [InspectorName("人物高度（米）"), Range(.6f, 2.5f)] public float ActorHeight = 1.6f;
            [InspectorName("人物位置微调")] public Vector3 ActorOffset;
            [InspectorName("监控机位置")] public Vector3 CameraPosition = new Vector3(-.3f, 2.7f, -4.5f);
            [InspectorName("监控机看向")] public Vector3 CameraTarget = new Vector3(0, .95f, .3f);
            [InspectorName("镜头视角"), Range(15, 75)] public float FieldOfView = 40;
        }

        [InspectorName("开启房间监控")] public bool Enabled = true;
        [InspectorName("病房背景（留空使用三维病房）")] public Texture2D Backdrop;
        [InspectorName("画面宽度"), Range(346, 1600)] public int Width = 928;
        [InspectorName("画面高度"), Range(140, 800)] public int Height = 522;
        [InspectorName("监控帧率"), Range(1, 30)] public int FramesPerSecond = 12;
        [InspectorName("监控明暗"), Range(.25f, 2)] public float Exposure = 1.15f;
        [InspectorName("画面色彩"), Range(0, 1)] public float Saturation = .45f;
        [InspectorName("信号噪点"), Range(0, .2f)] public float Noise = .035f;
        [InspectorName("人物轻微呼吸"), Range(0, .03f)] public float IdleMotion = .006f;
        [InspectorName("各人物房间")] public Room[] Rooms = Defaults();
        public int Revision { get; private set; }
        public void MarkChanged() => Revision++;
        private void OnValidate() => MarkChanged();

        public Room Find(string id)
        {
            if (Rooms == null) return null;
            foreach (var room in Rooms) if (room != null && room.PatientId == id) return room;
            return null;
        }

        private static Room[] Defaults() => new[]
        {
            new Room { PatientId = "child", Label = "B-12", Seed = 12, ActorHeight = 1.32f, Grime = .16f },
            new Room { PatientId = "former-ruler", Label = "B-13", Seed = 13, ActorHeight = 1.63f, Grime = .34f,
                WallTint = new Color(.74f, .76f, .69f) },
            new Room { PatientId = "battlefield-doctor", Label = "B-14", Seed = 14, ActorHeight = 1.58f, Grime = .4f },
            new Room { PatientId = "diplomat", Label = "B-15", Seed = 15, ActorHeight = 1.68f, Grime = .2f,
                LightTint = new Color(.77f, .86f, .89f) },
            new Room { PatientId = "priest", Label = "B-16", Seed = 16, ActorHeight = 1.73f, Grime = .46f },
            new Room { PatientId = "tutorial", Label = "B-17", Seed = 17, ActorHeight = 1.57f, Grime = .24f }
        };
    }
}
