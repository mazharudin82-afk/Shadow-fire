using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace ShadowFire
{
    public static class MatchConfig
    {
        public static bool Squad;
        public static readonly string[] Slots = { "You", "Empty", "Empty", "Empty" };
        public const string MatchScene = "Island_Greybox";
    }

    public class LobbyManager : MonoBehaviour
    {
        Text modeTxt; Button[] slotBtns = new Button[4]; Text[] slotTxt = new Text[4];

        void Start()
        {
            var cv = UiKit.MakeCanvas("Lobby", 0).transform;
            UiKit.Img(cv, "Bg", Vector2.zero, Vector2.zero, Vector2.zero, Color.clear); // placeholder
            UiKit.Txt(cv, "SHADOW FIRE", 96, new Vector2(.5f, 1f), new Vector2(0, -80), new Vector2(1000, 140));
            var mode = UiKit.Btn(cv, "", new Vector2(.5f, .5f), new Vector2(0, 160), new Vector2(420, 90), ToggleMode);
            modeTxt = mode.GetComponentInChildren<Text>();
            for (int i = 0; i < 4; i++)
            {
                int k = i;
                slotBtns[i] = UiKit.Btn(cv, "", new Vector2(.5f, .5f), new Vector2(-480 + i * 320, 0), new Vector2(290, 150), () => CycleSlot(k));
                slotTxt[i] = slotBtns[i].GetComponentInChildren<Text>();
            }
            UiKit.Btn(cv, "START MATCH", new Vector2(.5f, .5f), new Vector2(0, -170), new Vector2(520, 110), () => SceneManager.LoadScene(MatchConfig.MatchScene));
            UiKit.Btn(cv, "SETTINGS", new Vector2(.5f, .5f), new Vector2(0, -300), new Vector2(360, 80), SettingsPanel.Toggle);
            Refresh();
        }

        void ToggleMode() { MatchConfig.Squad = !MatchConfig.Squad; Refresh(); }
        void CycleSlot(int i) { if (i == 0 || !MatchConfig.Squad) return; MatchConfig.Slots[i] = MatchConfig.Slots[i] == "Empty" ? "Bot" : "Empty"; Refresh(); }
        void Refresh()
        {
            modeTxt.text = MatchConfig.Squad ? "MODE: SQUAD" : "MODE: SOLO";
            for (int i = 0; i < 4; i++)
            {
                slotBtns[i].interactable = MatchConfig.Squad || i == 0;
                slotTxt[i].text = "Slot " + (i + 1) + "\n" + (i == 0 ? "You" : MatchConfig.Squad ? MatchConfig.Slots[i] : "-");
            }
        }
    }
}
