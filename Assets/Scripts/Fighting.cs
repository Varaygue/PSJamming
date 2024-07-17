using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fighting : MonoBehaviour
{
    [Header("Activation")]
    public GameObject DrawCanvas; // Reference to the Canvas
    public bool isDrawing = false;
    public GameObject drawCircle;
    public Animator handsAnimation;
    public RawImage drawSurface;
    public Texture2D drawingTexture;
    private Vector2 previousMousePos;

    void Start()
    {
        InitializeDrawingSurface();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isDrawing)
        {

            handsAnimation.SetTrigger("Signing");
            DrawCanvas.SetActive(true);
            drawCircle.SetActive(true);
            isDrawing = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
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

    void HandleDrawing()
    {
        if (Input.GetMouseButtonDown(0))
        {
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

            // Reset the drawing state
            handsAnimation.SetTrigger("SigningOver");
            isDrawing = false;
            DrawCanvas.SetActive(false);
            drawCircle.SetActive(false);
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
            drawingTexture.SetPixel(x0, y0, Color.blue);

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
}
