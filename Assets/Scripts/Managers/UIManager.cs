using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private EventSystem eventSystem;
    private Canvas popUpCanvas;
    private Stack<PopUpUI> popUpStack;
    private Canvas windowCanvas;
    private Canvas inGameCanvas;
    private Canvas sceneCanvas;
    public Canvas SceneCanvas => sceneCanvas;

    private Canvas bossCanvas;

    private bool menuOpened;
    [HideInInspector] public UnityEvent<bool> OnHideSceneUI = new();

    private void Awake()
    {

        StartSceneInit();

        SceneManager.sceneLoaded += StartSceneInit;
        //menu = GameManager.Resource.Instantiate<MenuUI>("UI/PopUpUI/Menu");
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= StartSceneInit;
    }

    public void StartSceneInit(Scene scene =default,LoadSceneMode loadScene=default) 
    {
        if (eventSystem == null)
        {
            eventSystem = GameManager.Resource.Instantiate<EventSystem>("UI/EventSystem");
           // eventSystem.transform.parent = transform;
        }

        if (popUpCanvas == null)
        {
            popUpCanvas = GameManager.Resource.Instantiate<Canvas>("UI/Canvas");
            popUpCanvas.gameObject.name = "PopUpCanvas";
            popUpCanvas.sortingOrder = 100;
            popUpStack = new Stack<PopUpUI>();
        }



        if (windowCanvas == null)
        {
            windowCanvas = GameManager.Resource.Instantiate<Canvas>("UI/Canvas");
            windowCanvas.gameObject.name = "WindowCanvas";
            windowCanvas.sortingOrder = 10;
        }


        if (sceneCanvas == null)
        {
            sceneCanvas = GameManager.Resource.Instantiate<Canvas>("UI/Canvas");
            sceneCanvas.gameObject.name = "SceneCanvas";
            sceneCanvas.sortingOrder = 1;
        }


        if (inGameCanvas == null)
        {
            inGameCanvas = GameManager.Resource.Instantiate<Canvas>("UI/Canvas");
            inGameCanvas.gameObject.name = "InGameCanvas";
            inGameCanvas.sortingOrder = 0;

        }
    }


    public T ShowPopUpUI<T>(T popUpUI, bool setInactivePrev = true) where T : PopUpUI
    {
        if (popUpStack.Count > 0)
        {
            if (setInactivePrev == true)
            {
                PopUpUI prevUI = popUpStack.Peek();
                prevUI.gameObject.SetActive(false);
            }
        }
        else
        {
            NetworkRunner runner = FindObjectOfType<NetworkRunner>();
            if (runner != null)
            {
                NetworkObject player = runner.GetPlayerObject(runner.LocalPlayer);
                if (player != null)
                {
                    PlayerInputListner inputListner = player.GetComponent<PlayerInputListner>();
                    if (inputListner != null)
                    {
                        inputListner.RPC_ActiveButton(ButtonType.MouseDelta, false);
                    }
                }
            }
        }

        T ui = GameManager.Pool.GetUI<T>(popUpUI);
        ui.transform.SetParent(popUpCanvas.transform, false);
        popUpStack.Push(ui);

        CanvasGroup canvasGroup = ui.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            _ = StartCoroutine(FadeIn(canvasGroup));
        }

        return ui;
    }

    private IEnumerator FadeIn(CanvasGroup cg)
    {
        float fadeTime = 0.2f;
        float accumTime = 0f;
        while (accumTime < fadeTime)
        {
            cg.alpha = Mathf.Lerp(0f, 1f, accumTime / fadeTime);
            yield return 0;
            accumTime += Time.deltaTime;
        }
        cg.alpha = 1f;
    }

    public T ShowPopUpUI<T>(string path, bool setInactivePrev = true) where T : PopUpUI
    {
        T ui = GameManager.Resource.Load<T>(path);
        return ShowPopUpUI(ui, setInactivePrev);
    }

    public void ClosePopUpUI()
    {
        PopUpUI ui = popUpStack.Pop();
        CanvasGroup canvasGroup = ui.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            _ = StartCoroutine(FadeOut(ui.GetComponent<CanvasGroup>()));
        }
        else
        {
            GameManager.Pool.ReleaseUI(ui.gameObject);
        }

        if (popUpStack.Count > 0)
        {
            PopUpUI curUI = popUpStack.Peek();
            curUI.gameObject.SetActive(true);
        }
        else
        {

            NetworkRunner runner = FindObjectOfType<NetworkRunner>();
            if (runner != null)
            {

                NetworkObject player = runner.GetPlayerObject(runner.LocalPlayer);
                if (player != null)
                {
                    PlayerInputListner inputListner = player.GetComponent<PlayerInputListner>();
                    if (inputListner != null)
                    {
                        inputListner.RPC_ActiveButton(ButtonType.MouseDelta, true);
                    }
                }
            }
        }
    }

    private IEnumerator FadeOut(CanvasGroup cg)
    {
        float fadeTime = 0.2f;
        float accumTime = 0f;
        while (accumTime < fadeTime)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, accumTime / fadeTime);
            yield return 0;
            accumTime += Time.deltaTime;
        }
        cg.alpha = 0f;
        GameManager.Pool.ReleaseUI(cg.GetComponent<PopUpUI>().gameObject);
    }

    public void ClearPopUpUI()
    {
        while (popUpStack.Count > 0)
        {
            ClosePopUpUI();
        }
    }

    public T ShowWindowUI<T>(T windowUI) where T : WindowUI
    {
        T ui = GameManager.Pool.GetUI(windowUI);
        ui.transform.SetParent(windowCanvas.transform, false);
        _ = StartCoroutine(FadeIn(ui.GetComponent<CanvasGroup>()));
        return ui;
    }

    public T ShowWindowUI<T>(string path) where T : WindowUI
    {
        T ui = GameManager.Resource.Load<T>(path);
        return ShowWindowUI(ui);
    }

    public void SelectWindowUI<T>(T windowUI) where T : WindowUI
    {
        windowUI.transform.SetAsLastSibling();
    }

    public void CloseWindowUI<T>(T windowUI) where T : WindowUI
    {
        GameManager.Pool.ReleaseUI(windowUI.gameObject);
    }

    public void ClearWindowUI()
    {
        WindowUI[] windows = windowCanvas.GetComponentsInChildren<WindowUI>();

        foreach (WindowUI windowUI in windows)
        {
            GameManager.Pool.ReleaseUI(windowUI.gameObject);
        }
    }

    public T ShowInGameUI<T>(T gameUi) where T : InGameUI
    {
        T ui = GameManager.Pool.GetUI(gameUi);
        ui.transform.SetParent(inGameCanvas.transform, false);

        return ui;
    }

    public T ShowInGameUI<T>(string path) where T : InGameUI
    {
        T ui = GameManager.Resource.Load<T>(path);
        return ShowInGameUI(ui);
    }

    public void CloseInGameUI<T>(T inGameUI) where T : InGameUI
    {
        GameManager.Pool.ReleaseUI(inGameUI.gameObject);
    }

    public void ClearInGameUI()
    {
        InGameUI[] inGames = inGameCanvas.GetComponentsInChildren<InGameUI>();

        foreach (InGameUI inGameUI in inGames)
        {
            GameManager.Pool.ReleaseUI(inGameUI.gameObject);
        }
    }

    public T ShowSceneUI<T>(T gameUi) where T : SceneUI
    {
        T ui = GameManager.Pool.GetUI(gameUi);
        ui.transform.SetParent(sceneCanvas.transform, false);

        return ui;
    }

    public T ShowSceneUI<T>(string path) where T : SceneUI
    {
        T ui = GameManager.Resource.Load<T>(path);
        return ShowSceneUI(ui);
    }

    public void CloseSceneUI<T>(T inGameUI) where T : SceneUI
    {
        GameManager.Pool.ReleaseUI(inGameUI.gameObject);
    }

    public void ClearSceneUI()
    {
        if (inGameCanvas == null) return;

        SceneUI[] inGames = inGameCanvas.GetComponentsInChildren<SceneUI>();

        foreach (SceneUI inGameUI in inGames)
        {
            GameManager.Pool.ReleaseUI(inGameUI.gameObject);
        }
    }

    public void HideSceneUI(bool hide, float wait = 0f)
    {
        _ = StartCoroutine(CoHide(hide, wait));
    }

    private IEnumerator CoHide(bool hide, float wait)
    {
        yield return new WaitForSeconds(wait);
        OnHideSceneUI?.Invoke(hide);
    }
}