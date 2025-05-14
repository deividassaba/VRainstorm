using UnityEngine;

public class HDRISwitch : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Material[] skyboxes;
    private int currentSkybox = 0;

    public void SwitchSkybox()
    {
        currentSkybox = (currentSkybox + 1) % skyboxes.Length;
        RenderSettings.skybox = skyboxes[currentSkybox];
        DynamicGI.UpdateEnvironment(); // For lighting update
    }
}
