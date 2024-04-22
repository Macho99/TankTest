using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManagers : MonoBehaviour
{

    private GameObject popupCanvas;
    [SerializeField] private MessageBoxUI messageBoxUIPrefab;
    [SerializeField] private GameObject loadingUIPrefab;

    private LoadingUI loadingUI;

    private void Awake()
    {

        if (!CreateCanvas())
        {
            loadingUI = Instantiate(loadingUIPrefab, popupCanvas.transform).GetComponent<LoadingUI>();

        }
        else
        {
            loadingUI = popupCanvas.transform.Find("LoadingUI").GetComponent<LoadingUI>();
        }

    }

    public void CreateMessageBoxUI(string title, string content, params CreateButton[] createButton)
    {
        MessageBoxUI messageBoxUI = Instantiate(messageBoxUIPrefab, popupCanvas.transform);

        messageBoxUI.Init(title, content, createButton);

    }

    public bool CreateCanvas()
    {
        popupCanvas = GameObject.Find("PopupCanvas");
        if (popupCanvas == null)
        {
            popupCanvas = new GameObject("PopupCanvas");
            Canvas canvas = popupCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = byte.MaxValue;

            CanvasScaler canvasScaler = popupCanvas.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            popupCanvas.AddComponent<GraphicRaycaster>();
            return false;
        }

        return true;
    }

    public void ActiveLoading(bool isOn, string content = null)
    {
        loadingUI.gameObject.SetActive(isOn);
        loadingUI.Init(content);
    }
}
