using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fighting : MonoBehaviour
{
    [Header("Activation")]
    public GameObject DrawCanvas;
    public bool isDrawing = false;
    public Animator handsAnimation;
    public RawImage drawSurface;
    public GameObject DrawCircle;
    private Texture2D drawingTexture;
    private Vector2 previousMousePos;
    public FirstPersonController fpsScript;
    public Animator signsAnimation;

    [Header("Drawing properties")]
    public Color lineColor = Color.black;
    public int lineThickness = 2;

    [Header("Shapes Templates")]
    public Texture2D[] shapeTemplates;
    public string[] shapeTemplateNames;
    public float similarityThreshold = 0.8f;

    [Header("Signing Prefabs")]
    public Vector3 instantiateLocation;
    public RaycastCamera raycastScript;
    public GameObject earthPrefab;
    public GameObject airPrefab;


    void Start()
    {
        InitializeDrawingSurface();
    }

    void Update()
    {
        instantiateLocation = raycastScript.lastHitPoint;
        if (Input.GetKeyDown(KeyCode.F) && !isDrawing)
        {
            StartDrawingSession();
        }

        if (isDrawing)
        {
            HandleDrawing();
        }
    }

    void InitializeDrawingSurface()
    {
        drawingTexture = new Texture2D((int)drawSurface.rectTransform.rect.width, (int)drawSurface.rectTransform.rect.height);
        drawSurface.texture = drawingTexture;
        ClearDrawingSurface();
    }

    void ClearDrawingSurface()
    {
        Color[] clearPixels = drawingTexture.GetPixels();
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = Color.clear;
        }
        drawingTexture.SetPixels(clearPixels);
        drawingTexture.Apply();
    }

    void StartDrawingSession()
    {
        fpsScript.lockLook=true;
        handsAnimation.SetTrigger("Signing");
        DrawCanvas.SetActive(true);
        DrawCircle.SetActive(true);
        isDrawing = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        ClearDrawingSurface();
    }

    void HandleDrawing()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Started Drawing");
            previousMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 currentMousePos = Input.mousePosition;

            if (RectTransformUtility.RectangleContainsScreenPoint(drawSurface.rectTransform, currentMousePos))
            {
                DrawLine(previousMousePos, currentMousePos);
                previousMousePos = currentMousePos;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("Drawing complete. Analyze the drawn points and cast the spell.");
            RecognizeShape();
            isDrawing = false;
            fpsScript.lockLook=false;
            handsAnimation.SetTrigger("SigningOver");
            DrawCanvas.SetActive(false);
            DrawCircle.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void DrawLine(Vector2 start, Vector2 end)
    {
        int width = drawingTexture.width;
        int height = drawingTexture.height;

        Vector2 startUV = new Vector2(start.x / Screen.width * width, start.y / Screen.height * height);
        Vector2 endUV = new Vector2(end.x / Screen.width * width, end.y / Screen.height * height);

        int x0 = (int)startUV.x;
        int y0 = (int)startUV.y;
        int x1 = (int)endUV.x;
        int y1 = (int)endUV.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawThickPixel(x0, y0, lineThickness);

            if (x0 == x1 && y0 == y1) break;
            int e2 = err * 2;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        drawingTexture.Apply();
    }

    void DrawThickPixel(int x, int y, int thickness)
    {
        for (int i = -thickness; i <= thickness; i++)
        {
            for (int j = -thickness; j <= thickness; j++)
            {
                if (x + i >= 0 && x + i < drawingTexture.width && y + j >= 0 && y + j < drawingTexture.height)
                {
                    drawingTexture.SetPixel(x + i, y + j, lineColor);
                }
            }
        }
    }

    void RecognizeShape()
{
    float bestMatchScore = 0f;
    string bestMatchName = "";

    for (int i = 0; i < shapeTemplates.Length; i++)
    {
        float similarity = CompareTextures(drawingTexture, shapeTemplates[i]);
        if (similarity > bestMatchScore)
        {
            bestMatchScore = similarity;
            bestMatchName = shapeTemplateNames[i];
        }
        Debug.Log("Shape " + shapeTemplateNames[i] + ": Similarity = " + similarity);
    }

    if (!string.IsNullOrEmpty(bestMatchName) && bestMatchScore >= similarityThreshold)
    {
        Debug.Log("Shape recognized: " + bestMatchName + " with score " + bestMatchScore);
        if(bestMatchName=="Earth")
        {
            GameObject earthPrefabInstance = Instantiate(earthPrefab, instantiateLocation, Quaternion.identity);
            StartCoroutine(SignsAnimation("EarthAppear", "EarthDisappear"));
        }
        if(bestMatchName=="Air")
        {
            StartCoroutine(SignsAnimation("AirAppear", "AirDisappear"));
            airPrefab.SetActive(true);
        }
        if(bestMatchName=="Fire")
        {
            StartCoroutine(SignsAnimation("FireAppear", "FireDisappear"));
        }
        if (bestMatchName == "Water")
        {
            StartCoroutine(SignsAnimation("WaterAppear", "WaterDisappear"));
        }
    }
    else
    {
        Debug.Log("No matching shape found.");
    }
}

IEnumerator SignsAnimation(string animationTrigger, string animationEnd)
    {
        signsAnimation.SetTrigger(animationTrigger);

        if(animationTrigger=="AirAppear")
        {
            yield return new WaitForSeconds(3f);
            signsAnimation.SetTrigger(animationEnd);
            airPrefab.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(3f);
            signsAnimation.SetTrigger(animationEnd);
        }
    }

float CompareTextures(Texture2D drawnTexture, Texture2D templateTexture)
{
    int width = 64;  // Normalize width
    int height = 64; // Normalize height

    // Resize textures to the same size
    Texture2D resizedDrawnTexture = ResizeTexture(drawnTexture, width, height);
    Texture2D resizedTemplateTexture = ResizeTexture(templateTexture, width, height);

    Color[] drawnPixels = resizedDrawnTexture.GetPixels();
    Color[] templatePixels = resizedTemplateTexture.GetPixels();
    int matchingPixels = 0;
    int drawnPixelsCount = 0;
    int templatePixelsCount = 0;

    for (int i = 0; i < drawnPixels.Length; i++)
    {
        bool drawnPixel = drawnPixels[i].a > 0.1f;
        bool templatePixel = templatePixels[i].a > 0.1f;

        if (drawnPixel)
            drawnPixelsCount++;
        if (templatePixel)
            templatePixelsCount++;
        if (drawnPixel && templatePixel)
        {
            matchingPixels++;
        }
    }

    // Calculate similarity as a proportion of matching pixels to total template pixels
    float similarity = (float)matchingPixels / Mathf.Max(drawnPixelsCount, templatePixelsCount);
    return similarity;
}

Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
{
    RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 24);
    RenderTexture.active = rt;
    Graphics.Blit(source, rt);
    Texture2D result = new Texture2D(targetWidth, targetHeight);
    result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
    result.Apply();
    return result;
}

}
