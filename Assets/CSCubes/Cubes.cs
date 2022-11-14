using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public struct Cube 
{
    public Vector3 position;
    public Color color;
}


public class Cubes : MonoBehaviour 
{
    public ComputeShader computeShader;
    // public RenderTexture renderTexture;

    public Mesh mesh;
    public Material material;
    public int count = 50;
    public int repetitions = 1;
    public List<GameObject> objects;

    private Cube[] data;

    void Start()
    {
        CreateCubes();
    }

    void Update()
    {
        
    }
    
    public void CreateCubes()
    {
        objects = new List<GameObject>();
        data = new Cube[count * count];
        
        for (int x = 0; x < count; x++)
        {
            for (int y = 0; y < count; y++)
            {
                CreateCube(x, y);
            }
        }
    }
    
    public void CreateCube(int x, int y)
    {
        GameObject cube = new GameObject("Cube " + x * count + y,
            typeof(MeshFilter),
            typeof(MeshRenderer));
        cube.GetComponent<MeshFilter>().mesh = mesh;
        cube.GetComponent<MeshRenderer>().material = new Material(material);
        cube.transform.position = new Vector3(x, y, Random.Range(-0.1f, 0.1f));
        Color color = Random.ColorHSV();
        cube.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        
        objects.Add(cube);

        Cube cubeData = new Cube();
        cubeData.position = cube.transform.position;
        cubeData.color = color;
        data[x * count + y] = cubeData;
    }
    
    public void RandomizeCPU()
    {
        for (int i = 0; i < repetitions; i++)
        {
            for (int j = 0; j < objects.Count; j++)
            {
                GameObject obj = objects[j];
                obj.transform.position = new Vector3(
                    obj.transform.position.x, 
                    obj.transform.position.y,
                    Random.Range(-0.1f, 0.1f));
                obj.GetComponent<MeshRenderer>().material.SetColor("_Color", Random.ColorHSV());
            }
        }
    }
    
    public void RandomizeGPU()
    {
        int colorSize = sizeof(float) * 4;
        int vector3Size = sizeof(float) * 3;
        int totalSize = colorSize + vector3Size;

        ComputeBuffer cubesBuffer = new ComputeBuffer(data.Length, totalSize);
        cubesBuffer.SetData(data);

        computeShader.SetBuffer(0, "cubes", cubesBuffer);
        computeShader.SetFloat("resolution", data.Length);
        computeShader.SetFloat("repetitions", repetitions);
        computeShader.Dispatch(0, data.Length / 10, 1, 1);

        cubesBuffer.GetData(data);

        for (int i = 0; i < objects.Count; i++)
        {
            GameObject obj = objects[i];
            Cube cube = data[i];
            obj.transform.position = cube.position;
            obj.GetComponent<MeshRenderer>().material.SetColor("_Color", cube.color);
        }
        
        cubesBuffer.Dispose();
    }

    // private void OnRenderImage(RenderTexture src, RenderTexture dest)
    // {
    //     if (renderTexture == null)
    //     {
    //         renderTexture = new RenderTexture(256, 256, 24);
    //         renderTexture.enableRandomWrite = true;
    //         renderTexture.Create();
    //     }
    //     
    //     computeShader.SetTexture(0, "Result", renderTexture);
    //     computeShader.SetFloat("Resolution", renderTexture.width);
    //     computeShader.Dispatch(
    //         0, 
    //         renderTexture.width / 10,
    //         renderTexture.height / 1, 
    //         1);
    //     
    //     Graphics.Blit(renderTexture, dest);
    // }

    private void OnGUI()
    {
        if (objects == null)
        {
            if (GUI.Button(new Rect(0, 0, 100, 50), "Create"))
            {
                CreateCubes();
            }
        }
        else
        {
            if (GUI.Button(new Rect(0, 0, 100, 50), "Random CPU"))
            {
                RandomizeCPU();
            }
            if (GUI.Button(new Rect(100, 0, 100, 50), "Random GPU"))
            {
                RandomizeGPU();
            }
        }
    }
}
