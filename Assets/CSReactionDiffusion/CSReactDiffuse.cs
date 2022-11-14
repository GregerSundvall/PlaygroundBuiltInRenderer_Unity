using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Pixel {
    public float A; 
    public float B;
}

// ReSharper disable once InconsistentNaming
public class CSReactDiffuse : MonoBehaviour {
    [SerializeField] ComputeShader computeShader;
    [SerializeField] private RenderTexture rTexture;

    [SerializeField] private int width = 200;
    [SerializeField] private int height = 200;

    [SerializeField] private float diffuseA = 1.0f;
    [SerializeField] private float diffuseB = 0.5f;
    [SerializeField] private float feed = 0.055f;
    [SerializeField] private float kill = 0.062f;

    private Pixel[][] pixels;
    

    void Start()
    {
        pixels = new Pixel[width * height][];
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                pixels[x][y].A = 1;
                pixels[x][y].B = 0;
            }
        }
    }

    void Update()
    {
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        ComputeTexture(dest);
    }

    
    public void ComputeTexture(RenderTexture destination)
    {
        InitRenderTexture();
        
        computeShader.SetTexture(0, "texture", rTexture);
        
        ComputeBuffer pixelsBuffer =
            new ComputeBuffer(width * height, sizeof(float) * 2);
        pixelsBuffer.SetData(pixels);
        computeShader.SetBuffer(0, "pixels", pixelsBuffer);
        computeShader.SetFloat("width", width);
        computeShader.SetFloat("height", height);
        computeShader.SetFloat("diffuseA", diffuseA);
        computeShader.SetFloat("diffuseB", diffuseB);
        computeShader.SetFloat("feed", feed);
        computeShader.SetFloat("kill", kill);
        computeShader.Dispatch(
            0, width * height/10, 1, 1);
        
        pixelsBuffer.GetData(pixels); 
        
        Graphics.Blit(rTexture, destination);
    }
    
    private void InitRenderTexture()
    {
        if (rTexture == null || rTexture.width != width ||
            rTexture.height != height)
        {
            if (rTexture != null)
            {
                rTexture.Release();
            }

            rTexture = new RenderTexture(width, height, 0, 
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            rTexture.enableRandomWrite = true;
            rTexture.Create();
        }
    }
}
























