using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Painting : MonoBehaviour
{

    public float brushSize = 10;
    float initialInk = 1;
    float normalizedStrokeTime;
    public CustomRenderTexture paintingTex;
    public Material paintingMat;
    public Material targetMat;
    public Texture2D paintingSavedTex;


    public int nodeCountRefreshThreshold = 50;
    int nodeCount;
    Vector2 avgDirection;
    float[] curMousePos;
    float curPressure;

    float[] lastMousePos;
    float lastPressure;


    bool isPainting = false;
    void Start()
    {
        paintingTex = new CustomRenderTexture(
            Screen.width, Screen.height,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear
        ); 
        paintingTex.useDynamicScale = true;

        paintingMat = new Material(Shader.Find("Custom/PaintingTextureUpdate"));
        
        paintingTex.initializationSource = CustomRenderTextureInitializationSource.TextureAndColor;
        paintingTex.initializationColor = Color.white;
        paintingTex.initializationMode = CustomRenderTextureUpdateMode.OnLoad;
        
        paintingTex.updateMode = CustomRenderTextureUpdateMode.Realtime;
        paintingTex.material = paintingMat;

        paintingSavedTex = new Texture2D(
            Screen.width,
            Screen.height
        );
        Shader.SetGlobalTexture("_PaintingTexture", paintingSavedTex);
        targetMat.SetTexture("_MainTex", paintingSavedTex);

        StartCoroutine(UpdateRTMat());
    }

    IEnumerator UpdateRTMat(){
        // After render texture initialized at first frame, start paint rendering
        yield return null;
        paintingTex.initializationSource = CustomRenderTextureInitializationSource.Material;
        paintingTex.initializationMaterial = paintingMat;
        paintingTex.Initialize();
    }


    void Update()
    {
        ReadPixelsIntoSavedTexture();

        if(Input.GetKey(KeyCode.R)){
            ClearCanvasTexture();
        }

        if(Input.GetMouseButtonDown(0)){
            isPainting = true;
            normalizedStrokeTime = 1;
            nodeCount = -1;
            avgDirection = Vector2.zero;

            lastPressure = (1 - Mathf.Pow(2*normalizedStrokeTime-1, 2)*0.5f) * brushSize;
            lastMousePos = new float[2]{
                Input.mousePosition.x,
                Input.mousePosition.y
            };       
        }
        else if(Input.GetMouseButtonUp(0)) {
            isPainting = false;
        }
        

        if(isPainting && normalizedStrokeTime > Mathf.Epsilon){
            curMousePos = new float[2]{
                Input.mousePosition.x,
                Input.mousePosition.y
            };
            curPressure = (1 - Mathf.Pow(2*normalizedStrokeTime-1, 2)*0.5f) * brushSize;

            Vector2 mouseDir = new Vector2(
                curMousePos[0] - lastMousePos[0],
                curMousePos[1] - lastMousePos[1]
            );
            normalizedStrokeTime -= 0.5f * Time.deltaTime + 0.0005f * mouseDir.magnitude;

            paintingMat.EnableKeyword("PAINTING_ON");
            SetShaderData();

            // Calculate average direction of a stroke
            nodeCount++;
            if(nodeCount > 0){
                if(Vector2.Dot(mouseDir, avgDirection) < 0 || nodeCount > nodeCountRefreshThreshold){
                    // If direction change too much or the stroke last too long, reset
                    avgDirection = Vector2.zero;
                    nodeCount = 0;
                }
                else {
                    // Recalculate Average with weight
                    avgDirection = (avgDirection*(nodeCount-1)+mouseDir*5)/(nodeCount+4);
                }
            }
            
            lastMousePos = curMousePos;
            lastPressure = curPressure;
        }
        else{
            paintingMat.DisableKeyword("PAINTING_ON");
        }

        // Actually its used as Update,
        // I dont know why my Update doesnt work but Initialize do :(
        // paintingTex.Update();
        paintingTex.Initialize();
    }

    void SetShaderData(){

        paintingMat.SetFloatArray("_LastMousePos", lastMousePos);
        paintingMat.SetFloatArray("_MousePos", curMousePos);

        paintingMat.SetFloatArray("_AvgMouseDir", new float[2]{avgDirection.x, avgDirection.y});

        paintingMat.SetFloat("_LastPressure", lastPressure);
        paintingMat.SetFloat("_Pressure", curPressure);
            
        paintingMat.SetFloat("_InitialInk", normalizedStrokeTime);
    }

    void ReadPixelsIntoSavedTexture(){
        RenderTexture.active = paintingTex;
        paintingSavedTex.ReadPixels(
            new Rect(0, 0, paintingTex.width, paintingTex.height),
            0, 0,
            false);
        paintingSavedTex.Apply();
        RenderTexture.active = null;
    }

    void ClearCanvasTexture(){
        Color[] clearColorArray = paintingSavedTex.GetPixels();
        for(int i = 0; i < clearColorArray.Length; i++)
            clearColorArray[i] = Color.white;
        paintingSavedTex.SetPixels(clearColorArray);
        paintingSavedTex.Apply();
        Debug.Log("!!");
    }


    public void ClearCanvasButton(){
        // Execute function after update 
        // to prevent from clean texture stained by ReadPixels
        // it seems that OnClick event is executed before update
        StartCoroutine(ClearCanvasRoutine());
    }
    IEnumerator ClearCanvasRoutine(){
        yield return null;
        ClearCanvasTexture();
    }
}
