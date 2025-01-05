using System.Collections.Generic;
using UnityEngine;


public class Broke : MonoBehaviour
{
    [Header("Kýrýlma Ayarlarý")]
    public AudioClip breakSound; // Kýrýlma sýrasýnda çalýnacak ses
    public float breakForceThreshold = 2f; // Kýrýlma için gereken minimum çarpýþma kuvveti

    private AudioSource audioSource; // Ses çalma iþlemi için kullanýlacak
    private bool isBroken = false; // Nesnenin kýrýlma durumunu takip eder


    private void Start()
    {
        // Ses kaynaðý oluþturma ve ayarlama
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        if (breakSound != null)
        {
            audioSource.clip = breakSound;
        }
    }

 
 
    private void OnCollisionEnter(Collision collision)
    {
        if (isBroken) return;

        // Çarpýþma kuvvetini hesapla
        float collisionForce = collision.relativeVelocity.magnitude;

        // Eðer kuvvet eþik deðerini aþýyorsa nesne kýrýlýr
        if (collisionForce >= breakForceThreshold)
        {
            BreakObject();
        }
    }


    private void BreakObject()
    {
        isBroken = true; // Nesnenin kýrýldýðýný iþaretle

        // Mevcut mesh'i parçalara ayýr
        List<PartMesh> parts = SplitMesh();
        foreach (PartMesh part in parts)
        {
            // Her bir parçayý sahnede fiziksel bir nesne olarak oluþtur
            part.MakeGameObject(this);
        }

        // Kýrýlma sesini çal
        if (breakSound != null)
        {
            PlayBreakSound();
        }
    }


    private void PlayBreakSound()
    {
        // Ses çalmak için geçici bir nesne oluþtur
        GameObject tempAudioObject = new GameObject("BreakSound");
        AudioSource tempAudioSource = tempAudioObject.AddComponent<AudioSource>();
        tempAudioSource.clip = breakSound;
        tempAudioSource.Play();

  
        Destroy(tempAudioObject, breakSound.length);
    }

  
    private List<PartMesh> SplitMesh()
    {
        Mesh originalMesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = originalMesh.vertices;
        Vector3[] normals = originalMesh.normals;
        Vector2[] uv = originalMesh.uv;
        int[] triangles = originalMesh.triangles;

        // Parça sayýsýný rastgele belirle (5-15 arasý)
        int numParts = Random.Range(5, 15);
        List<PartMesh> parts = new List<PartMesh>();

        // Her bir parça için üçgenleri ayýrma iþlemi
        for (int i = 0; i < numParts; i++)
        {
            List<Vector3> partVertices = new List<Vector3>();
            List<Vector3> partNormals = new List<Vector3>();
            List<Vector2> partUV = new List<Vector2>();
            List<int> partTriangles = new List<int>();

            // Mesh'in üçgenlerini parçaya ekleme
            for (int j = 0; j < triangles.Length; j += 3)
            {
                if (Random.value > 0.5f)
                {
                    int index1 = triangles[j];
                    int index2 = triangles[j + 1];
                    int index3 = triangles[j + 2];

                    partTriangles.Add(partVertices.Count);
                    partTriangles.Add(partVertices.Count + 1);
                    partTriangles.Add(partVertices.Count + 2);

                    partVertices.Add(vertices[index1]);
                    partVertices.Add(vertices[index2]);
                    partVertices.Add(vertices[index3]);

                    partNormals.Add(normals[index1]);
                    partNormals.Add(normals[index2]);
                    partNormals.Add(normals[index3]);

                    partUV.Add(uv[index1]);
                    partUV.Add(uv[index2]);
                    partUV.Add(uv[index3]);
                }
            }

            if (partTriangles.Count > 0)
            {
                PartMesh partMesh = new PartMesh
                {
                    Vertices = partVertices.ToArray(),
                    Normals = partNormals.ToArray(),
                    UV = partUV.ToArray(),
                    Triangles = new int[1][] { partTriangles.ToArray() }
                };

                parts.Add(partMesh);
            }
        }

        return parts;
    }
}


public class PartMesh
{
    public Vector3[] Vertices; // Köþe noktalarý
    public Vector2[] UV; // Kaplama koordinatlarý
    public Vector3[] Normals; // Normaller
    public int[][] Triangles; // Üçgen dizileri
    public GameObject GameObject; // Fiziksel sahne nesnesi


    public void MakeGameObject(Broke destroyer)
    {
        // Parçayý sahnede bir nesne olarak oluþtur
        GameObject = new GameObject("PartMesh");
        GameObject.transform.position = destroyer.transform.position;
        GameObject.transform.rotation = destroyer.transform.rotation;

        // Yeni mesh oluþtur ve sahneye ekle
        Mesh mesh = new Mesh
        {
            vertices = Vertices,
            uv = UV,
            normals = Normals
        };

        for (int i = 0; i < Triangles.Length; i++)
        {
            mesh.SetTriangles(Triangles[i], i);
        }

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        MeshFilter meshFilter = GameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = GameObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;
        meshRenderer.materials = destroyer.GetComponent<MeshRenderer>().materials;

        // Rigidbody eklenerek fiziksel hareket saðlanýr
        Rigidbody rigidbody = GameObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = true;

        // Parçayý 10 saniye sonra yok et
        Object.Destroy(GameObject, 10f);
    }
}
