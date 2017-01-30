using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationController : MonoBehaviour {
    public GameObject hexagonesMap;
    private AudioSource theSong;

	// Use this for initialization
	void Start () {
        /*
        for(int i = 0; i < 6; i++)
        {
            GameObject test = (GameObject)GameObject.Instantiate(quad, new Vector3(0, 0.6f,0), Quaternion.identity);
            Part script = test.GetComponent<Part>();
            script.angle = i * 60;
            test.transform.Rotate(new Vector3(-90,0,0));
            test.transform.parent = GameObject.Find("Parts").transform;
        }
        */

        theSong = GetComponents<AudioSource>()[0];//;GetComponent<Spawner>().theSong;

        StartCoroutine(changeSideRotation());
	}
    

    float angle;//degrees
    float speed = 1f;


    bool clockWise = true;

	void Update () {
        //hexagonesMap.transform.rotation = Quaternion.Euler(new Vector3(4* Mathf.Sin(angle * Mathf.Deg2Rad), angle, 4* Mathf.Sin(360 - angle * Mathf.Deg2Rad)));

        //hexagonesMap.transform.Rotate(Vector3.up, 45 * Time.deltaTime * speed);
        analyzeVolume();

        

        //hexagonesMap.transform.localScale = new Vector3(1 + Mathf.Abs(Mathf.Sin(angle * Mathf.Deg2Rad) / 2), 1, 1 + Mathf.Abs(Mathf.Sin(angle * Mathf.Deg2Rad)) / 2);
        //angle++;

        if(clockWise)
            angle += 60 * Time.deltaTime * speed;
        else
            angle -= 60 * Time.deltaTime * speed;

        while (angle >= 360)
            angle = angle - 360;

        while (angle < 0)
            angle = 360 - angle;

        hexagonesMap.transform.rotation = Quaternion.Euler(new Vector3(6 * Mathf.Sin(angle * Mathf.Deg2Rad), angle, 6 * Mathf.Sin(360 - angle * Mathf.Deg2Rad)));
    }


    IEnumerator changeSideRotation()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(4, 12));
            clockWise = !clockWise;
        }
    }
    
    const int qSamples = 1024;  // array size
    float refValue = 0.1f; // RMS value for 0 dB

    float[] samples = new float[qSamples];

    void analyzeVolume()
    {
        theSong.GetOutputData(samples, 0); // fill array with samples
        int i;
        float sum = 0f;
        for (i = 0; i < qSamples; i++)
        {
            sum += samples[i] * samples[i]; // sum squared samples
        }
        float rmsValue = Mathf.Sqrt(sum / qSamples); // rms = square root of average
        float dbValue = 20 * Mathf.Log10(rmsValue / refValue); // calculate dB
        if (dbValue < -160) dbValue = -160; // clamp it to -160dB min


        speed = rmsValue * 10;
        //Debug.Log("db: " + dbValue + " rms: " + rmsValue);
    }
}
