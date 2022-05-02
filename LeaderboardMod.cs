using System;
using System.Globalization;
using HarmonyLib;
using MelonLoader;
using TMPro;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;


namespace ScoreKata
{
    public class LeaderboardMod : MelonMod
    {
        static ScoreKataHandler scoreKata = new ScoreKataHandler();
        public static LeaderboardDisplay leaderboardDisplay = null;
        static Transform scoreKataTabButton;

        [HarmonyPatch(typeof(LeaderboardDisplay), "UpdateScores")]
        [HarmonyPrefix]
        public static bool UpdateScores(LeaderboardDisplay __instance)
        {
            leaderboardDisplay = __instance;
            
            if(scoreKataTabButton == null) 
                CreateButtons();
            
            return true;
        }
        
        [HarmonyPatch(typeof(LeaderboardDisplay), "ViewTop")]
        [HarmonyPatch(typeof(LeaderboardDisplay), "ViewSelf")]
        [HarmonyPatch(typeof(LeaderboardDisplay), "ViewFriends")]
        [HarmonyPatch(typeof(LeaderboardDisplay), "ToggleExtras")]
        [HarmonyPostfix]
        public static void RevertTitle(LeaderboardDisplay __instance)
        {
            __instance.transform.parent.Find("Title").GetComponent<TMP_Text>()?.SetText("Total Leaderboard");
            CleanUpPreviouslySetData(__instance);
        }

        public override void OnApplicationStart()
        {
            this.HarmonyInstance.PatchAll(typeof(LeaderboardMod));
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicy) => { return true; };
        }

        public static async void GetLeaderboardAndUpdateUI()
        {
            var scoreKataLeaderboard = await scoreKata.GetLeaderboard();
            await scoreKata.GetPreviousWinners();
            var currentUserId = PlatformChooser.I.GetLeaderboardID();
            
            for (var index = 0; index < leaderboardDisplay.rowsTotal.Count; index++)
            {
                var leaderboardRow = leaderboardDisplay.rowsTotal[index];
                if (scoreKataLeaderboard.Length > index)
                {
                    var data = scoreKataLeaderboard[index];
                    leaderboardRow.score.text = GetScoreText(data);
                    leaderboardRow.username.text = LeaderboardDisplay.LaurelWrap(data.username,scoreKata.IsPreviousWinner(data));
                    leaderboardRow.rank.text = data.rank.ToString();
                    leaderboardRow.username.color = currentUserId == data.user_id ? Color.yellow : Color.white;
                    leaderboardRow.platform.sprite = GetSpriteForPlatform(data.platform);
                }
                else
                {
                    leaderboardRow.SetNone();
                    leaderboardRow.username.text = string.Empty;
                }
                leaderboardRow.compareButton.SetActive(false);
            }
            
            leaderboardDisplay.transform.parent.Find("Title").GetComponent<TMP_Text>()?.SetText("Score Kata Leaderboard");
        }

        public static void CreateButtons()
        {
            leaderboardDisplay.selectionTabs.transform.localPosition = new Vector3(-20, -289, 0);

            scoreKataTabButton = GameObject.Instantiate(leaderboardDisplay.selectionTabs.transform.GetChild(0), leaderboardDisplay.selectionTabs.transform);
            var gunButton = scoreKataTabButton.GetComponentInChildren<GunButton>();
            gunButton.onHitEvent = new UnityEvent();
            gunButton.onHitEvent.AddListener(new Action(() =>
            {
                leaderboardDisplay.rowsTotalParent.SetActive(true);
                leaderboardDisplay.rowsFriendsParent.SetActive(false);
                leaderboardDisplay.rowsTotalFriendsParent.SetActive(false);
                leaderboardDisplay.rowsStandardParent.SetActive(false);
                GetLeaderboardAndUpdateUI();
            }));
            scoreKataTabButton.localPosition += Vector3.right * 5;
            GameObject.Destroy(scoreKataTabButton.GetComponentInChildren<Localizer>());
            scoreKataTabButton.GetComponentInChildren<TextMeshPro>().text = "ScoreKata";
        }

        static void CleanUpPreviouslySetData(LeaderboardDisplay leaderboardDisplay)
        {
            foreach (var leaderboardRow in leaderboardDisplay.rowsTotal) 
                leaderboardRow.username.color = Color.white;
        }
        
        static Sprite GetSpriteForPlatform(string platform)
        {
            switch (platform)
            {
                case "steam":
                    return KataConfig.I.steamTexture;
                case "oculus":
                    return KataConfig.I.oculusTexture;
                default:
                    return KataConfig.I.viveportTexture;
            }
        }

        static string GetScoreText(ScoreKataLeaderboardEntry data) =>
            (int.TryParse(data.score, out int score) ? score : 0).ToString("N0", CultureInfo.CreateSpecificCulture("en-US"));
    }
}