using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushCursor : MonoBehaviour
{
    [SerializeField] Texture2D cursorTexture;
    [SerializeField] Texture2D ttt;
    Painting paintingComponent;
    Vector2Int mousePos = Vector2Int.zero;
    int cursorSize = 20;
    float lineAngle = 0;
    public bool isDrawAvgDir = true;
    void Start()
    {
        paintingComponent = GetComponent<Painting>();
    }

    void Update()
    {
        mousePos = new Vector2Int((int)Input.mousePosition.x, (int)Input.mousePosition.y);
        cursorSize = (int) paintingComponent.brushSize*2;
        if(isDrawAvgDir)
            lineAngle = Mathf.Lerp(lineAngle, -Vector2.SignedAngle(Vector2.down, paintingComponent.avgDirection), 0.8f);
    }

    void OnGUI() {
        GUI.DrawTexture(
            new Rect(mousePos.x - cursorSize/2, Screen.height - mousePos.y - cursorSize/2,
                cursorSize, cursorSize),
            cursorTexture
        );
        
        if(isDrawAvgDir){
            GUIUtility.RotateAroundPivot(
                lineAngle,
                new Vector2(mousePos.x, Screen.height - mousePos.y));
            GUI.DrawTexture(
                new Rect(mousePos.x - 7, Screen.height - mousePos.y,
                    15, 100),
                ttt, ScaleMode.StretchToFill);
        }
        
    }

}
