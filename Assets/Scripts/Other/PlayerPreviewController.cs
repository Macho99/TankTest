using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AppearanceType { Hair, Breard, Preset, Color, Size }
public class PlayerPreviewController : MonoBehaviour
{
    
    public enum ModelTransition { Left = -1, Right = 1 }
    [SerializeField] private Transform presetRoot;
    [SerializeField] private Transform hairRoot;
    [SerializeField] private Transform breardRoot;
    [SerializeField] private Material[] materials;

    [SerializeField] private GameObject playerPreset;
    private Dictionary<AppearanceType, Transform[]> appearModels;
    private Dictionary<AppearanceType, int> selectIndexDic;

    private float rotateSpeed;
    private Quaternion originRotate;
    private int originIndex;

    public Material GetMaterial(int index)
    {
        return materials[index];
    }
    private void Awake()
    {
        originIndex = 0;
        rotateSpeed = 5f;
        originRotate = transform.rotation;
        appearModels = new Dictionary<AppearanceType, Transform[]>();
        selectIndexDic = new Dictionary<AppearanceType, int>();
        AppearanceInit(AppearanceType.Hair, hairRoot);
        AppearanceInit(AppearanceType.Breard, breardRoot);
        AppearanceInit(AppearanceType.Preset, presetRoot);
        selectIndexDic.Add(AppearanceType.Color, 0);


    }
    public int GetCurrenIndex(AppearanceType appearanceType)
    {
        return selectIndexDic[appearanceType];
    }
    public GameObject SelectPreset()
    {
        return playerPreset;
    }

    public void ChangePreset(AppearanceType appearanceType, ModelTransition transition)
    {
        if (appearanceType == AppearanceType.Size)
            return;

        selectIndexDic[appearanceType] += (int)transition;
        transform.rotation = originRotate;


        if (appearanceType == AppearanceType.Color)
        {
            if (selectIndexDic[appearanceType] < 0)
            {
                selectIndexDic[appearanceType] = materials.Length - 1;
            }
            else if (selectIndexDic[appearanceType] > materials.Length - 1)
            {
                selectIndexDic[appearanceType] = 0;
            }
            ActiveMatarial(selectIndexDic[appearanceType]);
        }
        else
        {
            if (selectIndexDic[appearanceType] < 0)
            {
                selectIndexDic[appearanceType] = appearModels[appearanceType].Length - 1;
            }
            else if (selectIndexDic[appearanceType] > appearModels[appearanceType].Length - 1)
            {
                selectIndexDic[appearanceType] = 0;
            }
            ActiveModel(appearanceType, selectIndexDic[appearanceType]);
        }
    }

    public void RotateModel(float delta)
    {
        transform.Rotate(Vector3.up, delta * rotateSpeed);
    }
    public void ResetModel()
    {
        transform.rotation = originRotate;

        for (int i = 0; i < (int)AppearanceType.Size; i++)
        {
            selectIndexDic[(AppearanceType)i] = originIndex;
        }
        ActiveModel(AppearanceType.Hair, originIndex);
        ActiveModel(AppearanceType.Breard, originIndex);
        ActiveModel(AppearanceType.Preset, originIndex);
        ActiveMatarial(originIndex);
    }
    public void ActiveModel(AppearanceType appearanceType, int index)
    {
        for (int i = 0; i < appearModels[appearanceType].Length; i++)
        {
            appearModels[appearanceType][i].gameObject.SetActive(i == index);
        }
    }
    public void ActiveMatarial(int index)
    {
        for (int i = 0; i < appearModels[AppearanceType.Preset].Length; i++)
        {
            if (appearModels[AppearanceType.Preset][i].gameObject.activeSelf)
            {
                appearModels[AppearanceType.Preset][i].GetComponent<SkinnedMeshRenderer>().material = materials[index];
                return;
            }
        }
    }
    private void AppearanceInit(AppearanceType appearanceType, Transform root)
    {
        Transform[] Models = new Transform[root.childCount];
        for (int i = 0; i < root.childCount; i++)
        {
            Transform model = root.GetChild(i).transform;
            if (appearanceType == AppearanceType.Preset)
                model.GetComponent<SkinnedMeshRenderer>().material = materials[originIndex];
            Models[i] = model;
            model.gameObject.SetActive(false);
        }
        appearModels.Add(appearanceType, Models);

        Models[originIndex].gameObject.SetActive(true);
        selectIndexDic.Add(appearanceType, 0);

    }
}
