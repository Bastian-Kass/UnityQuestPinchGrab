using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

public class ActivateCheatMode : MonoBehaviour
{

    [SerializeField, Optional]
    public InteractableColorVisual.ColorState activeCS;
    [SerializeField, Optional]
    public InteractableColorVisual.ColorState inactiveCS;


    [SerializeField]
    private GameManagerScript throwGameManager;
    public InteractableColorVisual button_visual;

    public void ToggleCheatMode(){
        throwGameManager.ToggleCheatMode();
    }

    private void Start(){
        if(activeCS == null){
            activeCS = new InteractableColorVisual.ColorState();
            activeCS.Color = new Color(200, 40, 40, 40);
        }

        if(inactiveCS == null){
            inactiveCS = new InteractableColorVisual.ColorState();
            inactiveCS.Color = new Color(255, 255, 255, 36);
        }
    }

    void OnEnable()
    {
        throwGameManager.OnCheatModeChange.AddListener(ChangeButtonColor);
    }

    void OnDisable()
    {
        throwGameManager.OnCheatModeChange.RemoveListener(ChangeButtonColor);
    }

    private void ChangeButtonColor(bool state){
        
        if(state)
            button_visual.InjectOptionalNormalColorState(activeCS);
        else
            button_visual.InjectOptionalNormalColorState(inactiveCS);

    }
}
