using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public TextAsset onsets;
    public GameObject quad;//model
    public GameObject root;
    public GameObject parentForParts;


    public Material mat;
    public Material matDark;

    [HideInInspector]
    public AudioSource theSong;

    private List<Onset> listOnsets;

    public GameObject player;

    private Player playerScript;

    private float timeBlock = 1.8f;//time from start to end
    private float spawnDistance = 12f;


    private float centerRadius = 1f;
    private float blockWidth = 1.1f * 2; //*2 = 1 block width

    private int currentIndex = 0;

    //private List<List<GameObject>> listOfBlock;
    private List<List<Part>> listOfBlock;
    
    private void Awake()
    {
        theSong = GetComponents<AudioSource>()[0];
    }

    public bool readJson = true;

    //remove at first onsets too close beginning + too close ones
    void reduceNoise()
    {
        //List<int> toDestroy = new List<int>();
        //for(int i = 0; i < listOnsets.Count; i++)
        //{
        //    Debug.Log("onset at " + listOnsets[i].time);
        //    if(listOnsets[i].time < timeMinBeginning)
        //    {
        //        toDestroy.Add(i);
        //    }
        //}
        //for (int i = 0; i < toDestroy.Count; i++)
        //{
        //    listOnsets.RemoveAt(toDestroy[i]);
        //}
        //toDestroy.Clear();


        //beginning
        float timeMinBeginning = 0.8f;

        List<Onset> cleanedList = new List<Onset>();

        for (int i = 0; i < listOnsets.Count; i++)
        {
            if (listOnsets[i].time >= timeMinBeginning)
            {
                cleanedList.Add(listOnsets[i]);
            }
        }

        //too close ones
        float tooClose = 0.2f;

        List<Onset> cleanedList2 = new List<Onset>();

        if (cleanedList.Count == 1)
            cleanedList2.Add(cleanedList[0]);
        
        for (int i = 0; i < cleanedList.Count; i++)
        {
            if (i < cleanedList.Count - 1 && cleanedList[i + 1].time - cleanedList[i].time < tooClose)
            {
                List<Onset> tooCloseOnes = new List<Onset>();
                int j = i;
                tooCloseOnes.Add(cleanedList[j]);
                while (j < cleanedList.Count - 1 && cleanedList[j + 1].time - cleanedList[j].time < tooClose)
                {
                    tooCloseOnes.Add(cleanedList[j + 1]);
                    j++;
                }

                Onset newOnset = new Onset();
                float averageTime = 0f;
                float sumEnergy = 0f;
                float sum = 0f;
                foreach (Onset close in tooCloseOnes)
                {
                    sum += close.time;
                    sumEnergy += close.energy;
                }
                averageTime = sum / tooCloseOnes.Count;

                newOnset.time = averageTime;
                newOnset.energy = sumEnergy;

                cleanedList2.Add(newOnset);

                i = j;
            }
            else
            {
                cleanedList2.Add(cleanedList[i]);
            }
        }


        listOnsets = cleanedList2;

        foreach(Onset onset in listOnsets)
        {
            Debug.Log("onset at " + onset.time);
        }


        //for (int i = 0; i < listOnsets.Count - 1; i++)
        //{
        //    if (listOnsets[i + 1].time - listOnsets[i].time < tooClose)
        //    {
        //        Onset newOnset = new Onset();
        //        float averageTime = (listOnsets[i + 1].time + listOnsets[i].time) / 2;
        //        float sumEnergy = listOnsets[i].energy + listOnsets[i + 1].energy;
        //        newOnset.time = averageTime;
        //        newOnset.energy = sumEnergy;

        //        cleanedList.Add(newOnset);
        //    }
        //    else
        //    {
        //        cleanedList.Add(listOnsets[i]);
        //    }
        //}
    }

    void Start () {

        //Reading pre generated/created file of onsets
        if(readJson)
        {
            Debug.Log("JsonParty");
            listOnsets = new List<Onset>();
            JSONObject json = new JSONObject(onsets.text);
            json = json.list[0];
            Onset temp;
            foreach (JSONObject entry in json.list)
            {
                Debug.Log(entry);
                temp = new Onset();
                JsonUtility.FromJsonOverwrite(entry.ToString(), temp);
                listOnsets.Add(temp);
            }
        }
        else //spectrumanalyzer
        {
            Debug.Log("SpectrumParty");
            GetComponent<SpectrumAnalyzer>().analyze();
            listOnsets = GetComponent<SpectrumAnalyzer>().getListOnset();
            reduceNoise();
        }
        

        playerScript = player.GetComponent<Player>();

        

        


        listOfBlock = new List<List<Part>>();


        centerRadius = playerScript.distanceFromCenter;


        generatePatterns();
        checkSpawn();

        AudioSettings.outputSampleRate = theSong.clip.frequency;
        theSong.Play();
        theSong.timeSamples = 0;
    }


	// Update is called once per frame
	void Update () {

        checkCollision();
        /*
            if (currentIndex >= listOnsets.Count)
            {
                return;
            }

            Onset current = listOnsets[currentIndex];
            while ((current.time - timeBlock) <= getTimeSong())
            {
            
                //GameObject o = Instantiate(pnjPrefabs[Random.Range(0, pnjPrefabs.Length)], spawns[current.door].transform.position, Quaternion.identity) as GameObject; //Quaternion.LookRotation(direction); pour plus tard
                //Vector3 direction = (targets[current.door].transform.position - o.transform.position).normalized;
                //o.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
                //Debug.Log("create pnj " + theSong.time + " " + o.name);
                //o.GetComponent<PnjBehavior>().target = targets[current.door];
                //o.GetComponent<PnjBehavior>().itsNote = current;
            
                StartCoroutine(sayIt(timeBlock));
                launchAPattern();

                currentIndex++;
                if (currentIndex >= listOnsets.Count)
                    return;
                current = listOnsets[currentIndex];
            }
        */
        
    }


    float getTimeSong()
    {
        return (float) theSong.timeSamples / theSong.clip.frequency;
    }

    //adjust speed

    IEnumerator sayIt(float sayWhen)
    {
        yield return new WaitForSeconds(sayWhen);
        //Time.timeScale = 0;
        Debug.Log("Arrived!");
        theSong.pitch = -1;

        yield return new WaitForSeconds(3);
        //rewind effect
        /*
        foreach (GameObject g in listOfBlock)
        {
            Part p = g.GetComponent<Part>();
            //p.isStopped = false;
            p.speed = - 2*(spawnDistance - centerRadius - blockWidth) / timeBlock;
        }
        */
    }

    private float blockY = 0.6f;//0.6f


    



    //Variations

    //Close Range
    //public float minCloseOnset = 0.25f;//min time between onsets considered close => relative time to move (player speed), need to have time
    //public float maxCloseOnset = 0.40f;


    int supposedIndexOfPlayer = 0;

    void launchAPattern()
    {
        pattern1();

        /*
        int indexPlayer = 0;
        if (playerScript.currentAngle > 0)
            indexPlayer = (7 - (360 / playerScript.currentAngle));

        Debug.Log("indexPlayer: " + indexPlayer + " angle: " + playerScript.currentAngle);

        for (int i = getIndexPlayer() + 1; i < 5 + getIndexPlayer() + 1; i++)
        {
            GameObject test = (GameObject)GameObject.Instantiate(quad, new Vector3(0, blockY, 0), root.transform.rotation);//Quaternion.identity
            Part script = test.GetComponent<Part>();
            script.angle = i * 60;
            script.distance = spawnDistance;
            script.speed = (spawnDistance - centerRadius - blockWidth) / timeBlock;


            test.transform.Rotate(new Vector3(-90, 0, 0));
            test.transform.parent = GameObject.Find("Parts").transform;
        }*/
    }

    List<Pattern> listPatterns = new List<Pattern>();

    class Pattern
    {
        public List<int> listPartsDegree = new List<int>();
        public float speed;
        public float distance;//from previous pattern
        //public List<Onset> listOnsets = new List<Onset>();
        public Onset onset;
        //for  the next pattern:
        public List<int> supposedPlayerPositions = new List<int>();
    }

    //howManyUseBeats per pattern
   // enum PatternsUse { Pat1 = 1, Pat2 = 3};

    void generatePatterns()
    {
        while (currentIndex < listOnsets.Count)// && currentIndex + (int) PatternsUse.Pat1 <= listOnsets.Count)
        {
            //Random selection pattern here
            pattern1();
        }
    }

    //rotate multiple of 60 (positive only)
    void rotatePattern(Pattern p, int number)
    {
        List<int> listPartsDegree = p.listPartsDegree;
        for(int i = 0; i < listPartsDegree.Count; i++)
        {
            int current = listPartsDegree[i];
            current += 60 * number;
            while(current >= 360)
            {
                current -= 360;
            }
            listPartsDegree[i] = current;
        }
    }

    //rotate on index
    int rotatePlayer(int current, int number)
    {
        current += number;

        while(current > 5)
        {
            current -= 6;
        }

        while(current < 0)
        {
            current += 6;
        }

        return current;
    }

    float reflexTimeDivisor = 2f;

    void pattern1()
    {
        //int howManyUseBeats = (int) PatternsUse.Pat1;

        Pattern p = new Pattern();


        //while(currentIndex < listOnsets.Count && p.listOnsets.Count < howManyUseBeats)
        //{
        //    p.listOnsets.Add(listOnsets[currentIndex]);
        //    currentIndex++;
        //}
        p.onset = listOnsets[currentIndex];
        currentIndex++;

        //Pattern 1
        for (int i = 0; i < 5 ; i++)
        {
            p.listPartsDegree.Add(i * 60);

            //p.distance = spawnDistance;
            //p.speed = (spawnDistance - centerRadius - blockWidth) / timeBlock;
        }

        int supposedPlayerPosition = 0;

        //first pattern
        if(listPatterns.Count == 0)
        {
            supposedPlayerPosition = getIndexPlayer();
            Debug.Log("supposed first pos: " + supposedPlayerPosition);
        }
        else if(listPatterns.Count > 0)
        {
            //get from previous pattern
            supposedPlayerPosition = listPatterns[listPatterns.Count - 1].supposedPlayerPositions[0];
        }

        //Orientation of the pattern to match player position
        //left of player
        if (Random.Range(0, 2) == 0)
        {
            rotatePattern(p, supposedPlayerPosition);
            p.supposedPlayerPositions.Add(rotatePlayer(supposedPlayerPosition, -1));
            Debug.Log("left from: " + supposedPlayerPosition);
        }
        //right of player
        else
        {
            rotatePattern(p, supposedPlayerPosition + 2);
            p.supposedPlayerPositions.Add(rotatePlayer(supposedPlayerPosition, 1));
            Debug.Log("right from: " + supposedPlayerPosition);
        }

        //TODO
        //speed
        //distance from previous pattern

        //player distanceFromCenter



        //first
        if (listPatterns.Count == 0)
        {
            p.distance = spawnDistance;
            p.speed = (spawnDistance - centerRadius - blockWidth / reflexTimeDivisor) / p.onset.time;
        }
        else// (listPatterns.Count > 0)
        {
            float spaceBetweenPattern = (blockWidth * 2);

            p.speed = spaceBetweenPattern / (p.onset.time - listPatterns[listPatterns.Count - 1].onset.time);
            Debug.Log("compute speed: " + p.speed);
            p.distance = spaceBetweenPattern;//listPatterns[listPatterns.Count - 1].distance + 
        }


        listPatterns.Add(p);

        /*
        int indexPlayer = getIndexPlayer();

        Debug.Log("indexPlayer: " + indexPlayer + " angle: " + playerScript.currentAngle);

        for (int i = getIndexPlayer() + 2; i < 5 + getIndexPlayer() + 2; i++)
        {
            GameObject test = (GameObject)GameObject.Instantiate(quad, new Vector3(0, blockY, 0), root.transform.rotation);//Quaternion.identity
            Part script = test.GetComponent<Part>();
            script.angle = i * 60;
            script.distance = spawnDistance;
            script.speed = (spawnDistance - centerRadius - blockWidth) / timeBlock;


            test.transform.Rotate(new Vector3(-90, 0, 0));
            test.transform.parent = GameObject.Find("Parts").transform;

            listOfBlock.Add(test);
        }
        */

        
    }

    //check if need to spawn pattern from the list
    int indexOfSpawn = 0;
    void checkSpawn()
    {
        //spawn
        /*
        
        */

        //Debug.Log("pos player: " + getIndexPlayer());

        for (int i = indexOfSpawn; i < listPatterns.Count; i++)
        {
            Pattern currentPattern = listPatterns[i];

            List<Part> parts = new List<Part>();

            foreach (int degree in currentPattern.listPartsDegree)
            {
                GameObject test = (GameObject)GameObject.Instantiate(quad, new Vector3(0, blockY, 0), root.transform.rotation);//Quaternion.identity
                Part script = test.GetComponent<Part>();
                script.angle = degree;


                //Base for all
                script.speed = listPatterns[0].speed;// (spawnDistance - centerRadius - blockWidth/4) / timeBlock;
                if(i == 0) //if(i - 1 < 0)
                {
                    
                    script.distance = listPatterns[0].distance;//spawnDistance;

                    Debug.Log("first distance: " + script.distance);
                }
                else
                {
                    script.distance = currentPattern.distance + listOfBlock[i - 1][0].distance;
                }


                test.transform.Rotate(new Vector3(-90, 0, 0));
                test.transform.parent = parentForParts.transform;

                parts.Add(script);
            }
            listOfBlock.Add(parts);
            //indexOfSpawn++;
        }

        if (listPatterns.Count > 1)
            changeSpeedCo = StartCoroutine(changeSpeed(listPatterns[0].onset.time));//useless because first =>  - getTimeSong()));
    }

    Coroutine changeSpeedCo;

    //index for pattern passed
    int indexOfBlocks = 0;

    float shade_factor = 0.15f;

    //Dynamically change speed
    IEnumerator changeSpeed(float wait)//onset occured
    {
        yield return new WaitForSeconds(wait);

        indexOfBlocks++;
        if (indexOfBlocks < listPatterns.Count)
        {
            //if(indexOfBlocks == 3)
            //Time.timeScale = 0;

            Debug.Log("new speed: " + listPatterns[indexOfBlocks].speed);

            foreach (List<Part> parts in listOfBlock)
            {
                foreach (Part script in parts)
                {
                    script.speed = listPatterns[indexOfBlocks].speed;
                }
            }

            Color c = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

            mat.color = c;

            c.r *= (1 - shade_factor);
            c.g *= (1 - shade_factor);
            c.b *= (1 - shade_factor);

            matDark.color = c;

            Debug.Log("next speed: " + (listPatterns[indexOfBlocks].onset.time) +" " + getTimeSong());
            changeSpeedCo = StartCoroutine(changeSpeed(listPatterns[indexOfBlocks].onset.time - getTimeSong()));
        }
        else
        {
            //GameObject.Find("WinText").SetActive(true);
        }
    }

    void rewind()
    {
        StopCoroutine(changeSpeedCo);
        indexOfBlocks = indexOfSpawn = 0;
        
        

        StartCoroutine(rewindAnimation());
    }

    IEnumerator rewindAnimation()
    {
        float timeRewind = 2f;

        float distanceToMake = 0f;
        if (listOfBlock[0][0].distance < 0)
        {
            distanceToMake = spawnDistance + Mathf.Abs(listOfBlock[0][0].distance);
        }
        else
        {
            distanceToMake = spawnDistance - listOfBlock[0][0].distance;
        }

        float startDistance = listOfBlock[0][0].distance;


        Debug.Log("distancefromlast: " + listOfBlock[0][0].distance);


        foreach (List<Part> parts in listOfBlock)
        {
            foreach (Part script in parts)
            {
                script.speed = 0f;
            }
        }

        //return (float)theSong.timeSamples / theSong.clip.frequency;
        int currentSample = theSong.timeSamples;
        int sampleToReach = 0;
        int distanceSample = currentSample - sampleToReach;
        float distanceSampleSeconds = distanceSample / theSong.clip.frequency;

        float reversePitch = (float) (distanceSample / timeRewind) / theSong.clip.frequency;

        //float initPitch = theSong.pitch;
        //Debug.Log("init pitch: " + initPitch);
        //theSong.pitch = -1;


        /*float[] samples = new float[theSong.clip.samples * theSong.clip.channels];
        theSong.clip.GetData(samples, 0);
        Debug.Log("samples length: " + samples.Length);
        float[] reverseSamples = new float[distanceSample + 1];

        int j = 0;
        for(int i = currentSample; i >= sampleToReach; i--)
        {
            reverseSamples[j] = samples[i];
            j++;
        }
        Debug.Log("reverses length: " + reverseSamples.Length);
        Debug.Log("distance sample: " + distanceSample);
        */
        theSong.Pause();
        //theSong.clip.SetData(reverseSamples, 0);
        theSong.pitch = -reversePitch;
        theSong.UnPause();
        //theSong.timeSamples = 0;

        var step = 0.0f;
        var rate = 1.0f / timeRewind;
        var smoothStep = 0.0f;
        var lastStep = 0.0f;

        //int lastSupoosedTimeSample = 0;

        while (step < 1.0)
        {
            step += Time.deltaTime * rate; //increase the step
            smoothStep = Mathf.SmoothStep(0.0f, 1.0f, step);//(listOfBlock[0][0].distance, spawnDistance, step); //get the smooth step
            float newDistance = distanceToMake * (smoothStep - lastStep);

            foreach (List<Part> parts in listOfBlock)
            {
                foreach (Part script in parts)
                {
                    script.distance += newDistance;
                }
            }

           
            //int sample = lastSupoosedTimeSample + (int) (distanceSample * (smoothStep - lastStep));
            //sample = (sample > reverseSamples.Length - 1) ? reverseSamples.Length - 1 : sample;
            //theSong.timeSamples = lastSupoosedTimeSample = sample;
            //Debug.Log("current sample: " + sample);
            

            lastStep = smoothStep;
            yield return null;
        }
        if (step > 1.0)
        {
            foreach (List<Part> parts in listOfBlock)
            {
                foreach (Part script in parts)
                {
                    script.distance += distanceToMake * (1.0f - lastStep);
                }
            }
            //theSong.pitch = initPitch;
            theSong.Stop();
            //theSong.clip.SetData(samples, 0);
            theSong.pitch = 1;
            theSong.Play();
            theSong.timeSamples = sampleToReach;
        }

        listOfBlock.Clear();
        foreach (Transform child in parentForParts.transform)
        {
            Destroy(child.gameObject);
        }
        checkSpawn();
        theSong.timeSamples = 0;
    }

    void checkCollision()
    {
        //Current next 
        //if (indexOfBlocks < listPatterns.Count)
        if (indexOfBlocks - 1 >= 0)
        {

            int indexPlayer = getIndexPlayer();

            //Debug.Log("check coll of: " + (indexOfBlocks -1));

            foreach (Part part in listOfBlock[indexOfBlocks - 1])
            {
                //part.speed
                //part.transform.position
                //centerRadius
                int indexPart = (part.angle > 0) ? part.angle / 60 : 0;

                if (indexPlayer != indexPart)
                    continue;

                //Debug.Log("indexpart: " + part.angle + " playindex: " + indexPlayer);

                float nextDistance = part.distance - part.speed * Time.deltaTime;//not sure if compute distance of script part before here

                //Debug.Log("currentdistance: " + part.distance + " nexdistance: " + nextDistance);
                //Debug.Log("centerradius: " + centerRadius);
                //if (part.distance  > centerRadius && nextDistance < centerRadius)"
                if (part.distance  > centerRadius && nextDistance < centerRadius)
                {
                    Debug.Log("currentdistance: " + part.distance + " nexdistance: " + nextDistance);
                    Debug.Log("centerradius: " + centerRadius);
                    //Time.timeScale = 0;
                    rewind();
                    return;
                } 
            }
        }
    }

    int getIndexPlayer()
    {
        int currentAngle = playerScript.currentAngle;

        /*if (currentAngle == 0)
            return 0;
        else*/
        if (currentAngle == 60 || currentAngle == -300)
            return 1;
        else if (currentAngle == 120 || currentAngle == -240)
            return 2;
        else if (currentAngle == 180 || currentAngle == -180)
            return 3;
        else if (currentAngle == 240 || currentAngle == -120)
            return 4;
        else if (currentAngle == 300 || currentAngle == -60)
            return 5;

        return 0;
    }
}
