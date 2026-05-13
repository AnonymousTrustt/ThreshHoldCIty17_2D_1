using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResourceCapOverride
{
    public ResourceType resourceType;
    public bool useCap = true;
    public int cap = 1000;
}

[Serializable]
public class TownStageConfig
{
    public int level = 1;
    public string stageName = "Small Town";
    public List<ResourceCapOverride> resourceCaps = new List<ResourceCapOverride>
    {
        new ResourceCapOverride { resourceType = ResourceType.Pollution, useCap = true, cap = 1000 }
    };
}

public class TownStageManager : MonoBehaviour
{
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private List<TownStageConfig> stages = new List<TownStageConfig>
    {
        new TownStageConfig { level = 1, stageName = "Small Town" },
        new TownStageConfig
        {
            level = 2,
            stageName = "Growing Town",
            resourceCaps = new List<ResourceCapOverride>
            {
                new ResourceCapOverride { resourceType = ResourceType.Pollution, useCap = true, cap = 50000 }
            }
        },
        new TownStageConfig
        {
            level = 3,
            stageName = "Regional City",
            resourceCaps = new List<ResourceCapOverride>
            {
                new ResourceCapOverride { resourceType = ResourceType.Pollution, useCap = true, cap = 100000 }
            }
        }
    };

    public int CurrentLevel => currentLevel;

    public event Action<int> LevelChanged;

    private void Awake()
    {
        if (resourceManager == null)
        {
            resourceManager = FindFirstObjectByType<ResourceManager>();
        }
    }

    private void Start()
    {
        ApplyCurrentLevelCaps();
        LevelChanged?.Invoke(currentLevel);
    }

    public void AdvanceLevel()
    {
        SetLevel(currentLevel + 1);
    }

    public void SetLevel(int level)
    {
        currentLevel = Mathf.Max(1, level);
        ApplyCurrentLevelCaps();
        LevelChanged?.Invoke(currentLevel);
    }

    public int GetPollutionCapForCurrentLevel()
    {
        TownStageConfig config = GetStageConfig(currentLevel);
        if (config == null)
        {
            return 0;
        }

        foreach (ResourceCapOverride capOverride in config.resourceCaps)
        {
            if (capOverride.resourceType == ResourceType.Pollution && capOverride.useCap)
            {
                return capOverride.cap;
            }
        }

        return 0;
    }

    private void ApplyCurrentLevelCaps()
    {
        if (resourceManager == null)
        {
            return;
        }

        TownStageConfig config = GetStageConfig(currentLevel);
        if (config == null)
        {
            return;
        }

        foreach (ResourceCapOverride capOverride in config.resourceCaps)
        {
            resourceManager.SetResourceCap(capOverride.resourceType, capOverride.cap, capOverride.useCap);
        }
    }

    private TownStageConfig GetStageConfig(int level)
    {
        foreach (TownStageConfig config in stages)
        {
            if (config.level == level)
            {
                return config;
            }
        }

        return null;
    }
}