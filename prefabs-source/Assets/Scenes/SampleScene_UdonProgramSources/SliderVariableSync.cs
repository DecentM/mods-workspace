
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SliderVariableSync : UdonSharpBehaviour
{
    public UdonBehaviour behaviour;
    public TextMeshProUGUI textMesh;
    public string variableName;

    private Slider slider;

    private void Start()
    {
        this.slider = GetComponent<Slider>();

        this.OnUpdate();
    }

    public void OnUpdate()
    {
        float value = this.slider.value;
        this.behaviour.SetProgramVariable(this.variableName, value);
        this.textMesh.text = $"{this.variableName}: {value.ToString("0.00")}";
    }
}
