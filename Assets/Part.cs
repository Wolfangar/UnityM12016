using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Part : MonoBehaviour {

    //Color is not right
    public Material mat;
    public Material matDark;


    /*
    void OnPostRender()
    {
        if (!mat)
        {
            Debug.LogError("Please Assign a material on the inspector");
            return;
        }

        mat.SetPass(0);
        Rect position = new Rect(-2, -3, 15, 60);
        GL.Color(Color.red);
        GL.Begin(GL.QUADS);
        GL.Vertex3(position.x, position.y, 1);
        GL.Vertex3(position.x + position.width, position.y, 1);
        GL.Vertex3(position.x + position.width, position.y + position.height, 1);
        GL.Vertex3(position.x, position.y + position.height, 1);
        GL.End();
    }
    */
    [HideInInspector]
    public int angle = 0;//rotate/side
    private int angleSize = 60;

    [HideInInspector]
    public float distance = 4f;
    [HideInInspector]
    public float speed = 1f;


    int i = 0;

    void Start()
    {
        if(angle == 60 || angle == 180 || angle == 300)
        {
            GetComponent<MeshRenderer>().material = matDark;
        }
        else
        {
            GetComponent<MeshRenderer>().material = mat;
        }
    }
    [HideInInspector]
    public bool isStopped = false;

    void Update()
    {
        if (isStopped)
            return;

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        /*Debug.Log("vertices:" + mesh.vertices.Length);
        Debug.Log("vertices1:" + mesh.vertices[0].ToString());
        Debug.Log("vertices2:" + mesh.vertices[1].ToString());
        Debug.Log("vertices3:" + mesh.vertices[2].ToString());
        Debug.Log("vertices4:" + mesh.vertices[3].ToString());*/

        /*angle = Mathf.PI * i / 180;
        i++;
        if (i == 360)
            i = 0;
            */
        //mesh.Clear();
        distance -= speed * Time.deltaTime;

        float offset = 0f;//eloignement centre
        float width = 1.1f;//longueur


        float angleLeft =  Mathf.PI * angle / 180;
        float angleRight = Mathf.PI * (angle + angleSize) / 180;

        var x = offset + Mathf.Cos(angleLeft) * distance;
        var y = offset + Mathf.Sin(angleLeft) * distance;

        var x2 = offset + Mathf.Cos(angleRight) * distance;
        var y2 = offset + Mathf.Sin(angleRight) * distance;

        var x3 = offset + Mathf.Cos(angleRight) * (distance + width);
        var y3 = offset + Mathf.Sin(angleRight) * (distance + width);

        var x4 = offset + Mathf.Cos(angleLeft) * (distance + width);
        var y4 = offset + Mathf.Sin(angleLeft) * (distance + width);

        Vector3 bottomLeft = new Vector3(x,y,0);
        Vector3 bottomRight = new Vector3(x2,y2,0);
        Vector3 topLeft = new Vector3(x4,y4,0);
        Vector3 topRight = new Vector3(x3,y3,0);


        //mesh.vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0) };
        mesh.vertices = new Vector3[] { bottomLeft, topRight, bottomRight, topLeft };
        mesh.RecalculateBounds();

        //MeshCollider mc = this.GetComponent<MeshCollider>();
        //mc.sharedMesh = mesh;


        //if (distance <= 0)
          //  isStopped = true;
    }
}
