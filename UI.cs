using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.Common;
using Kingmaker.Utility;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Object = UnityEngine.Object;

namespace BagOfTricks
{
    public class BaseUI
    {
        public GameObject baseGameObject = new GameObject("gameObject");

        public void On()
        {
            baseGameObject.SetActive(true);
        }

        public void Off()
        {
            baseGameObject.SetActive(false);
        }
    }

    public class UICanvas : BaseUI
    {
        public UICanvas(string gameObjectName)
        {
            baseGameObject.name = gameObjectName;
            Object.DontDestroyOnLoad(baseGameObject);

            var canvas = baseGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var canvasScaler = baseGameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            var graphicRaycaster = baseGameObject.AddComponent<GraphicRaycaster>();

            var canvasGroup = baseGameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
        }
    }

    public class UITMPText : BaseUI
    {
        public TextMeshProUGUI textTMP;

        public UITMPText(GameObject canvasGameObject, string gameObjectName)
        {
            baseGameObject.name = gameObjectName;
            Object.DontDestroyOnLoad(baseGameObject);
            baseGameObject.transform.SetParent(canvasGameObject.transform, false);

            textTMP = baseGameObject.AddComponent<TextMeshProUGUI>();
            textTMP.text = "";
            textTMP.richText = true;
            textTMP.alignment = TextAlignmentOptions.Top;
            textTMP.outlineColor = Color.black;

            var rectTransform = baseGameObject.GetComponent<RectTransform>();
            rectTransform.position = new Vector3(Screen.width / 2f, Screen.height * 0.95f, 0);
            rectTransform.sizeDelta = new Vector2(500f, rectTransform.sizeDelta.y);
        }

        public void Text(string s)
        {
            textTMP.text = s;
        }

        public void Size(int i)
        {
            textTMP.fontSize = i;
        }

        public void OutlineWidth(float f)
        {
            textTMP.outlineWidth = f;
        }
    }

    public class UIPanel : BaseUI
    {
        public UIPanel(GameObject canvasGameObject, string gameObjectName)
        {
            baseGameObject.transform.SetParent(canvasGameObject.transform, false);
            baseGameObject.AddComponent<CanvasRenderer>();
            var image = baseGameObject.AddComponent<Image>();
            image.color = Color.red;
            var rectTransform = baseGameObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        }
    }

    public class CombatDifficultyMessage : IUnitCombatHandler, IGlobalSubscriber
    {
        public void HandleUnitJoinCombat(UnitEntityData unit)
        {
            if (!BuildModeUtility.IsDevelopment)
                UIUtility.SendWarning(string.Format(
                    $"{Strings.GetText("label_CombatDifficulty")}: {Common.GetDifficulty()} (CR {Common.GetEncounterCR()})"));
        }

        public void HandleUnitLeaveCombat(UnitEntityData unit)
        {
        }
    }
}