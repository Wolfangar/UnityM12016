using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [HideInInspector]
    public float distanceFromCenter;

	// Use this for initialization
	void Awake () {
        distanceFromCenter = (this.transform.position - Vector3.zero).magnitude;
	}

    [HideInInspector]
    public int currentAngle = 0;

    bool canTurn = true;

	// Update is called once per frame
	void Update () {
        if (!canTurn)
            return;

        if (Input.GetKeyDown("left"))
        {
            print("left key was pressed");
            //transform.RotateAround(Vector3.zero, Vector3.up, -30 * Time.deltaTime);
            canTurn = false;
            StartCoroutine(rotateAngle(true));
        }
        else if (Input.GetKeyDown("right"))
        {
            print("right key was pressed");
            canTurn = false;
            //transform.RotateAround(Vector3.zero, Vector3.up, 30 * Time.deltaTime);
            StartCoroutine(rotateAngle(false));
        }
    }

    public float rotateTime = 0.1f;//player speed

    IEnumerator rotateAngle(bool left)
    {
        int moveAngle = (left) ? -60 : 60;

        currentAngle += moveAngle;
        currentAngle = (currentAngle == -360 || currentAngle == 360) ? 0 : currentAngle;

        //transform.rotation = Quaternion.Euler(new Vector3(0,currentAngle,0));

        var step = 0.0f; //non-smoothed
        var rate = 1.0f / rotateTime; //amount to increase non-smooth step by
        var smoothStep = 0.0f; //smooth step this time
        var lastStep = 0.0f; //smooth step last time
        while (step < 1.0)
        { // until we're done
            step += Time.deltaTime * rate; //increase the step
            smoothStep = Mathf.SmoothStep(0.0f, 1.0f, step); //get the smooth step
            transform.RotateAround(Vector3.zero, Vector3.up, moveAngle * (smoothStep - lastStep));
            lastStep = smoothStep; //store the smooth step
            yield return null;
        }
        //finish any left-over
        if (step > 1.0) transform.RotateAround(Vector3.zero, Vector3.up, moveAngle * (1.0f - lastStep));

        //instant turn :
        //transform.RotateAround(Vector3.zero, Vector3.up, moveAngle);

        //yield return new WaitForSeconds(1);
        canTurn = true;
    }
}
