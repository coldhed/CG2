/*
Script to control the car's and wheel's transforms -> rotation and translation

Santiago Rodríguez
2023-11-13
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] float speed = 1f;
    [SerializeField] Vector3 displacement;
    [SerializeField] float angularVelocity = 1f;
    [SerializeField] GameObject wheel;
    [SerializeField] float scale = 0.69f;

    GameObject[] wheels = new GameObject[4];

    Vector3 frontAxis = new Vector3(2.1f, 0.85f, -2.8f);
    Vector3 backAxis = new Vector3(2.1f, 0.85f, 2.87f);

    // meshes and vertices for the car
    Mesh mesh;
    Vector3[] baseVertices;
    Vector3[] newVertices;

    // meshes and vertices for the wheels
    Mesh[] wheelMeshes = new Mesh[4];
    Vector3[][] wheelBaseVertices = new Vector3[4][];
    Vector3[][] wheelNewVertices = new Vector3[4][];

    // Start is called before the first frame update
    void Start()
    {
        // instantiate the wheels
        wheels[0] = Instantiate(wheel, frontAxis, Quaternion.identity);
        wheels[1] = Instantiate(wheel, Vector3.Scale(frontAxis, new Vector3(-1, 1, 1)), Quaternion.identity);
        wheels[2] = Instantiate(wheel, backAxis, Quaternion.identity);
        wheels[3] = Instantiate(wheel, Vector3.Scale(backAxis, new Vector3(-1, 1, 1)), Quaternion.identity);

        // get the meshes and vertices for the car
        mesh = GetComponentInChildren<MeshFilter>().mesh;
        baseVertices = mesh.vertices;

        // Copy the vertices to a new array
        newVertices = new Vector3[baseVertices.Length];
        for (int i = 0; i < baseVertices.Length; i++)
        {
            newVertices[i] = baseVertices[i];
        }

        // get the meshes and vertices for the wheels
        for (int i = 0; i < 4; ++i)
        {
            wheelMeshes[i] = wheels[i].GetComponentInChildren<MeshFilter>().mesh;

            wheelBaseVertices[i] = wheelMeshes[i].vertices;

            // Copy the vertices to a new array
            wheelNewVertices[i] = new Vector3[wheelBaseVertices[i].Length];
            for (int j = 0; j < wheelBaseVertices[i].Length; j++)
            {
                wheelNewVertices[i][j] = wheelBaseVertices[i][j];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        DoTransform();
    }

    void DoTransform()
    {
        Matrix4x4 move = HW_Transforms.TranslationMat(displacement.x * Time.time,
                                                      displacement.y * Time.time,
                                                      displacement.z * Time.time);

        Matrix4x4 moveOrigin = HW_Transforms.TranslationMat(0, 0, 0);

        // Matrix4x4 moveObject = HW_Transforms.TranslationMat(rotationOrigin.x,
        //                                                     rotationOrigin.y,
        //                                                     rotationOrigin.z);


        // Combine all the matrices into a single one
        // Matrix4x4 composite = move * moveObject * rotate * moveOrigin;


        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector4 tmp = new Vector4(baseVertices[i].x, baseVertices[i].y, baseVertices[i].z, 1);

            newVertices[i] = move * tmp;
        }

        // Assign the new vertices to the mesh
        mesh.vertices = newVertices;
        mesh.RecalculateNormals();

        // Do the same for the wheels, and scale them down
        Matrix4x4 scaleMat = HW_Transforms.ScaleMat(scale, scale, scale);
        Matrix4x4 rotate = HW_Transforms.RotateMat(angularVelocity * Time.time, AXIS.X);
        Matrix4x4 composite = move * rotate * scaleMat;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < wheelBaseVertices[i].Length; j++)
            {
                Vector4 tmp = new Vector4(wheelBaseVertices[i][j].x, wheelBaseVertices[i][j].y, wheelBaseVertices[i][j].z, 1);

                wheelNewVertices[i][j] = composite * tmp;
            }

            wheelMeshes[i].vertices = wheelNewVertices[i];
            wheelMeshes[i].RecalculateNormals();
        }
    }
}
