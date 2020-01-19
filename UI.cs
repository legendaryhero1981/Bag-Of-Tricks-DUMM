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
    public class BaseUi
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

    public class UiCanvas : BaseUi
    {
        public UiCanvas(string gameObjectName)
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

    public class UitmpText : BaseUi
    {
        public TextMeshProUGUI textTmp;

        public UitmpText(GameObject canvasGameObject, string gameObjectName)
        {
            baseGameObject.name = gameObjectName;
            Object.DontDestroyOnLoad(baseGameObject);
            baseGameObject.transform.SetParent(canvasGameObject.transform, false);

            textTmp = baseGameObject.AddComponent<TextMeshProUGUI>();
            textTmp.text = "";
            textTmp.richText = true;
            textTmp.alignment = TextAlignmentOptions.Top;
            textTmp.outlineColor = Color.black;

            var rectTransform = baseGameObject.GetComponent<RectTransform>();
            rectTransform.position = new Vector3(Screen.width / 2f, Screen.height * 0.95f, 0);
            rectTransform.sizeDelta = new Vector2(500f, rectTransform.sizeDelta.y);
        }

        public void Text(string s)
        {
            textTmp.text = s;
        }

        public void Size(int i)
        {
            textTmp.fontSize = i;
        }

        public void OutlineWidth(float f)
        {
            textTmp.outlineWidth = f;
        }
    }

    public class UiPanel : BaseUi
    {
        public UiPanel(GameObject canvasGameObject, string gameObjectName)
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
                    $"{Strings.GetText("label_CombatDifficulty")}: {Common.GetDifficulty()} (CR {Common.GetEncounterCr()})"));
        }

        public void HandleUnitLeaveCombat(UnitEntityData unit)
        {
        }
    }
}