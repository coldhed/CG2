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
    [SerializeField] float angularVelocity = 90f;
    [SerializeField] GameObject wheel;
    [SerializeField] float scale = 0.69f;
    [SerializeField] float timeBetweenWaypoints = 1f;
    [SerializeField] Vector3[] waypoints;
    [SerializeField] Vector3[] wheelPositions;

    // current waypoint
    int currentWaypoint = 0;
    float timeSinceLastWaypoint = 0f;

    // gameobject list for the wheels
    GameObject[] wheels = new GameObject[4];

    // wheel positions
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
        waypoints[0] = Vector3.zero;

        // instantiate the wheels
        for (int i = 0; i < 4; ++i)
        {
            wheels[i] = Instantiate(wheel, wheelPositions[i], Quaternion.identity);
        }

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
        // if the car has reached the current waypoint, move to the next one
        timeSinceLastWaypoint += Time.deltaTime;

        if (timeSinceLastWaypoint >= timeBetweenWaypoints)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            timeSinceLastWaypoint = 0f;
        }

        DoTransform();
    }

    void DoTransform()
    {
        Vector3 direction = Vector3.Lerp(waypoints[currentWaypoint], waypoints[(currentWaypoint + 1) % waypoints.Length], timeSinceLastWaypoint / timeBetweenWaypoints);

        Matrix4x4 move = HW_Transforms.TranslationMat(direction.x,
                                                      direction.y,
                                                      direction.z);

        Vector3 goingTo = waypoints[(currentWaypoint + 1) % waypoints.Length] - waypoints[currentWaypoint];
        float angle = Vector3.SignedAngle(Vector3.back, goingTo, Vector3.down);

        Matrix4x4 rotate = HW_Transforms.RotateMat(angle, AXIS.Y);
        Matrix4x4 composite = move * rotate;

        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector4 tmp = new Vector4(baseVertices[i].x, baseVertices[i].y, baseVertices[i].z, 1);

            newVertices[i] = composite * tmp;
        }

        // Assign the new vertices to the mesh
        mesh.vertices = newVertices;
        mesh.RecalculateNormals();

        // Do the same for the wheels, and scale them down
        Matrix4x4 scaleMat = HW_Transforms.ScaleMat(scale, scale, scale);
        Matrix4x4 spin = HW_Transforms.RotateMat(angularVelocity * Time.time, AXIS.X);
        Matrix4x4 spinComp = spin * scaleMat;

        for (int i = 0; i < 4; i++)
        {
            Matrix4x4 pivot = HW_Transforms.TranslationMat(wheelPositions[i].x, wheelPositions[i].y, wheelPositions[i].z);
            Matrix4x4 pivotBack = HW_Transforms.TranslationMat(-wheelPositions[i].x, -wheelPositions[i].y, -wheelPositions[i].z);

            for (int j = 0; j < wheelBaseVertices[i].Length; j++)
            {
                Vector4 tmp = new Vector4(wheelBaseVertices[i][j].x, wheelBaseVertices[i][j].y, wheelBaseVertices[i][j].z, 1);

                wheelNewVertices[i][j] = move * pivotBack * rotate * pivot * spinComp * tmp;
            }

            wheelMeshes[i].vertices = wheelNewVertices[i];
            wheelMeshes[i].RecalculateNormals();
        }
    }
}
