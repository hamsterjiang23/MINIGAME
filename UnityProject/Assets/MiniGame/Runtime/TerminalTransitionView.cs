using MiniGame.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace MiniGame.UnityHost
{
    internal sealed class TerminalTransitionView
    {
        private readonly GameObject console, loading;
        private readonly Text[] rows = new Text[28];
        public string ConsoleText { get; private set; } = "";
        private string previousPowerText = "";
        private TerminalPowerPhase previousPowerPhase = TerminalPowerPhase.Running;
        private readonly GameObject[] masks;
        private static readonly Rect[] Regions = {
            new Rect(9,9,782,27),new Rect(9,37,782,71),
            new Rect(9,108,782,30),new Rect(9,138,782,30),new Rect(9,168,782,30),new Rect(9,198,782,30),new Rect(9,228,782,30),
            new Rect(9,258,782,135),new Rect(9,394,782,38),new Rect(9,432,782,26),new Rect(9,458,782,26),new Rect(9,484,782,26),new Rect(9,510,782,38),new Rect(9,548,782,43)
        };
        public TerminalTransitionView(RectTransform parent, Font font)
        {
            loading = TerminalIndustrialSkin.Panel(parent,"TerminalPartitionLoading",Color.clear,0,0,800,600).gameObject;
            masks = new GameObject[Regions.Length];
            for(int i=0;i<Regions.Length;i++) { var r=Regions[i]; masks[i]=TerminalIndustrialSkin.Panel(loading.transform,"UnloadedPartition"+i,TerminalView.Desk,r.x,r.y,r.width,r.height).gameObject; }
            console = TerminalIndustrialSkin.Panel(parent,"TerminalHandoff",TerminalIndustrialSkin.ColorOf("080603"),0,0,800,600).gameObject;
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = TerminalIndustrialSkin.Label(console.transform, font, i == 0 ? "BootConsole" : "BootConsoleRow" + i,
                    "", 28, 24 + i * 20, 744, 20, 14, TerminalView.Ink);
                rows[i].alignment = TextAnchor.UpperLeft;
                rows[i].horizontalOverflow = HorizontalWrapMode.Overflow;
            }
            if (font != null && font.dynamic)
                font.RequestCharactersInTexture("PMS console 3.14 Self-test Tests completed. BOOT Console ready. Closing session... Session closed. System halted. >>> _ 0123456789", 14, FontStyle.Normal);
            SetPowerFrame(new TerminalPowerFrame(TerminalPowerPhase.Running));
        }
        public int SetPowerFrame(TerminalPowerFrame frame)
        {
            string text = frame.ConsoleText ?? "";
            int newCharacters = 0;
            if (frame.Phase == TerminalPowerPhase.Boot || frame.Phase == TerminalPowerPhase.Shutdown)
            {
                int shared = 0;
                if (previousPowerPhase == frame.Phase)
                    while (shared < previousPowerText.Length && shared < text.Length && previousPowerText[shared] == text[shared]) shared++;
                for (int i = shared; i < text.Length; i++)
                    if (!char.IsWhiteSpace(text[i])) newCharacters++;
            }
            previousPowerText = text;
            previousPowerPhase = frame.Phase;
            bool load=frame.Phase==TerminalPowerPhase.Loading;
            bool visible=frame.Phase!=TerminalPowerPhase.Running && !load;
            loading.SetActive(load); console.SetActive(visible);
            if(load) for(int i=0;i<masks.Length;i++) masks[i].SetActive(i>=frame.LoadedRegions);
            string output = frame.Phase == TerminalPowerPhase.Off ? "" : frame.ConsoleText + (frame.Cursor ? "_" : "");
            if (ConsoleText != output)
            {
                ConsoleText = output;
                string[] lines = ConsoleText.Split('\n');
                for (int i = 0; i < rows.Length; i++) rows[i].text = i < lines.Length ? lines[i] : "";
            }
            if(visible) console.transform.SetAsLastSibling();
            if(load) loading.transform.SetAsLastSibling();
            return newCharacters;
        }
        public void ResetTyping() { previousPowerText = ""; previousPowerPhase = TerminalPowerPhase.Running; }
        // Camera motion remains separate from the terminal's power timeline.
        public void SetFrame(float progress,bool exiting,bool active) { }
    }
}
