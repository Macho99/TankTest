using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class UIImageDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
{

    public event Action<float> onDrag;

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {

        if (eventData.pointerDrag == gameObject)
        {
            float delta = eventData.delta.x > 0 ? -1 : 1;

            onDrag?.Invoke(delta);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
