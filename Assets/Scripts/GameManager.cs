using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour{

    private TextMeshProUGUI feedbackUI;
    private TextMeshProUGUI scoreUI;

    public GameObject enemyPrefab;
    public GameObject flagPrefab;
    public GameObject hazardPrefab;

    public Vector3 enemyPos;
    public Vector3 flagPos;
    public float mapXY;

    public Transform floor;
    public int hazardNumber;

    private GameObject enemy;
    private GameObject flag;
    private GameObject hazard;
    private ScoreManager myScoreManager;

    void Start(){
    myScoreManager = FindObjectOfType<ScoreManager>();
	feedbackUI = GameObject.Find("Feedback UI").GetComponent<TextMeshProUGUI>();
	feedbackUI.text = "";
    //scoreUI = GameObject.Find("Score UI").GetComponent<TextMeshProUGUI>();
	//scoreUI.text = "";
	InitialiseScene();
    }

    void Update() {
        //scoreUI.text = "Time Since Loaded : " + Time.timeSinceLevelLoad + "\nHigh Score: " + myScoreManager.highscore;
    }

    public void Restart(){ // Replace with Athina's function
    myScoreManager.GetHighScore();
	// string scene = SceneManager.GetActiveScene().name;
    //     SceneManager.LoadScene(scene, LoadSceneMode.Single);
    // For whatever reason this reloading scene successfully doesn't destroy
    // the score manager so it can keep track of highscore.
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public IEnumerator DelayedRestart(){
	Time.timeScale = 0;
	yield return new WaitForSecondsRealtime(1);
	Restart();
	Time.timeScale = 1;
    }

    public void HealthFeedback(){
	feedbackUI.text = "The enemy is dead!";
    }

    public void DistanceFeedback(){
	feedbackUI.text = "The enemy has captured the flag!";
    }

    void InitialiseScene(){
    float scaleFactor = mapXY;
	mapXY = scaleFactor * (0.5f * floor.localScale.x) - 1;
	enemyPos = new Vector3(Random.Range(-mapXY, mapXY), 0.25f, Random.Range(mapXY, mapXY));
	flagPos = new Vector3(Random.Range(-mapXY/4, mapXY/4), 0.25f, Random.Range(-mapXY/4, mapXY/4));
        enemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
        enemy.transform.localScale = enemy.transform.localScale * scaleFactor;
	flag = Instantiate(flagPrefab, flagPos, Quaternion.identity);
    flag.transform.localScale = flag.transform.localScale * scaleFactor;
	for (int i = 0; i < hazardNumber; i++){
	    Vector3 hazardPos = new Vector3(Random.Range(-mapXY, mapXY), 0.25f, Random.Range(-mapXY, 0.9f * mapXY));
	    hazard = Instantiate(hazardPrefab, hazardPos, Quaternion.identity);
        hazard.transform.localScale = hazard.transform.localScale * scaleFactor;
	}
    }

}
