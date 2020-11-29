using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
    public Color[] colors;
    public HexGrid hexGrid;
    private Color activeColor;
    private void Awake() {
        SelectColor(0);
    }

    public void SelectColor(int idx){
        activeColor=colors[idx];
    }
    private void Update() {
        if(Input.GetMouseButton(0)&&!EventSystem.current.IsPointerOverGameObject()){
            HandleInput();
        }
    }

    void HandleInput(){
        Ray inputRay=Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(inputRay,out hit)){
            hexGrid.ColorCell(hit.point,activeColor);
        }
    }
}
