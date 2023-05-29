using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Lava_Ocean : MonoBehaviour
{
    float vertexSpacingX,vertexSpacingZ;

    [Header("Mesh Settings")]
    public bool squareResolution;
    public int xResolution;
    public int zResolution;

    [Header("Perlin Noise Wave Settings")]
    // perlin noise x ekseni için;
    public bool perlinNoise_x;
    public float waveHeight_x,waveScale_x;
    float WaveScale_x{get{return waveScale_x*0.05f;}}

    [Range(-2f,2f)]
    public float waveSpeed_x;
    // perlin noise y ekseni için;
    public bool perlinNoise_y;
    public float waveHeight_y,waveScale_y;
    float WaveScale_y{get{return waveScale_y*0.05f;}}

    [Range(-2f,2f)]
    public float waveSpeed_y;
    // perlin noise z ekseni için;
    public bool perlinNoise_z;
    public float waveHeight_z,waveScale_z;
    float WaveScale_z{get{return waveScale_z*0.05f;}}
    
    [Range(-2f,2f)]
    public float waveSpeed_z;
    
    [Header("Sin Wave Settings")]
    public bool sinWave;
    public bool xDirection,zDirection;
    public float sinWaveScale;
    public float sinWaveHeight;
    [Range(-10f,10f)]
    public float sinWaveSpeed;
    float sinWaveFloat;
    [Header("Moving Texture Settings")]
    public bool MoveTexture;
    [Range(-20f,20f)]
    public float textureSpeedX,textureSpeedZ;
    float TextureSpeedX{get{return textureSpeedX*0.03f;}}
    float TextureSpeedZ{get{return textureSpeedZ*0.03f;}}

    Mesh mesh;
    List<Vector3> vertices;
    List<Vector2> uvs;
    List<int> triangles;
    Renderer ownRenderer;
    Vector3[] meshVertices,_meshVertices;
    Vector3 firstPoint;
    float xScale,zScale;
    bool waveStart;
    GameObject generatePoint;
    Quaternion firstRot;
    Vector3 firstPosition;
    Transform pivot;

    void Start()
    {
        firstPosition=transform.position;
        firstRot=gameObject.transform.rotation;
        gameObject.transform.rotation=Quaternion.Euler(0,0,0);
        generatePoint=gameObject.transform.GetChild(0).gameObject;
        ownRenderer = GetComponent<Renderer>();
        xScale=ownRenderer.bounds.size.x;
        zScale=ownRenderer.bounds.size.z;
        // mesh oluşturma için başlangıç noktası belirleniyor.
        firstPoint=generatePoint.transform.position;
        transform.position=firstPoint;
        // vertex aralıkları belirleniyor
        if(squareResolution==true){
            vertexSpacingX=xScale/xResolution/transform.localScale.x;
            vertexSpacingZ=xScale/xResolution/transform.localScale.z;
            zResolution=(int)(zScale/(xScale/xResolution));
        }
        else{
            vertexSpacingX=xScale/xResolution/transform.localScale.x;
            vertexSpacingZ=zScale/zResolution/transform.localScale.z;
        }
        

        waveStart=false;
        mesh=new Mesh();
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        triangles = new List<int>();
       
        CreateVertex();
        CreateTriangles();
        UpdateMesh();
        meshVertices=mesh.vertices;
        _meshVertices=mesh.vertices;

        // objenin rotasyonunu ayarlamak için bir obje oluşturuluyor
        // yeni oluşturulan obje parent obje yapılıyor
        // ve rotasyon ilk rotasyona göre ayarlanıyor.
        pivot = new GameObject().transform;
        pivot.name="Lava&Ocean";
        pivot.transform.position=firstPosition;
        gameObject.transform.SetParent(pivot);
        pivot.transform.rotation=firstRot;
    }

    

    void CreateVertex()
    {   
        for(int z=0; z<=zResolution; z++)
        {
            for(int x=0; x<=xResolution; x++)
            {
                vertices.Add(new Vector3(x*vertexSpacingX, 0, z*vertexSpacingZ));
                uvs.Add(new Vector2((float)x / xResolution, (float)z / zResolution));
            }
        }
    }

    void CreateTriangles()
    {
        for (int i = 0, z = 0; z < zResolution; z++)
        {
            for (int x = 0; x < xResolution; x++)
            {
                triangles.Add(i);
                triangles.Add(i+xResolution+1);
                triangles.Add(i+1);
                triangles.Add(i+1);
                triangles.Add(i+xResolution+1);
                triangles.Add(i+xResolution+2);
                i++;
            }
            i+=1;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        waveStart=true;
    }

    private void Update()
    {
        if(waveStart==true){
            
            if(perlinNoise_y==true && sinWave==false){
                for(int i=0; i<meshVertices.Length;i++){
                    meshVertices[i].y=CalculatePerlinNoise(waveHeight_y,waveSpeed_y,WaveScale_y,i);
                }
            }

            if(perlinNoise_y==false && sinWave==true){
                for(int i=0; i<meshVertices.Length;i++){
                    meshVertices[i].y=CalculateSin(i);
                }
            }

            if(perlinNoise_y==true && sinWave==true){
                for(int i=0; i<meshVertices.Length;i++){
                    meshVertices[i].y=CalculatePerlinNoise(waveHeight_y,waveSpeed_y,WaveScale_y,i)+CalculateSin(i);
                }
            }

            if(perlinNoise_x==true || perlinNoise_z==true){
                for(int i=0; i<meshVertices.Length;i++){
                    if(perlinNoise_x==true){
                        meshVertices[i].x=_meshVertices[i].x+CalculatePerlinNoise(waveHeight_x,waveSpeed_x,WaveScale_x,i);
                    }
                   
                    if(perlinNoise_z==true){
                        meshVertices[i].z=_meshVertices[i].z+CalculatePerlinNoise(waveHeight_z,waveSpeed_z,WaveScale_z,i);
                    }
                }
            }
            
            mesh.vertices=meshVertices;
        }

        if(MoveTexture==true){
            ownRenderer.material.mainTextureOffset = new Vector2(
                Time.time * TextureSpeedX, 
                Time.time * TextureSpeedZ);
        }
    }

    private float CalculatePerlinNoise(float waveHeight,float waveSpeed,float waveScale,int i){
        return waveHeight*Mathf.PerlinNoise(
            Time.time*waveSpeed+(meshVertices[i].x*waveScale),
            Time.time*waveSpeed+(meshVertices[i].z*waveScale));
    }
    private float CalculateSin(int i){
        if(xDirection==true && zDirection==false){
            sinWaveFloat= sinWaveHeight*Mathf.Sin(meshVertices[i].x*sinWaveScale+Time.time*sinWaveSpeed);
        }
        if(xDirection==false && zDirection==true){
            sinWaveFloat= sinWaveHeight*Mathf.Sin(meshVertices[i].z*sinWaveScale+Time.time*sinWaveSpeed);
        }
        if(xDirection==true && zDirection==true){
            sinWaveFloat= sinWaveHeight*Mathf.Sin(meshVertices[i].x*sinWaveScale+meshVertices[i].z*sinWaveScale+Time.time*sinWaveSpeed);
        }
        return sinWaveFloat;
    }
}
