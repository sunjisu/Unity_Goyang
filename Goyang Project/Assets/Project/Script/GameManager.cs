﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;

    public PlayerMove player;
    public int health;
    public GameObject[] Stages;

    public Image[] UIhealth;
    public Text UIPoint;
    public Text UIStage;

    // 대화창
    public GameObject talkPanel;
    public Text talkText;
    private GameObject scanObject;
    public bool isAction;


    public GameObject RestartButton;


    private void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }

    // 대화창 액션
    public void Action(GameObject scanObj)
    {
        if(isAction)
        {
            isAction = false;           
        }
        else
        {
            isAction = true;           
            scanObject = scanObj;
            talkText.text = "아아 이것의 이름은" + scanObj.name + "라구!";
        }

        talkPanel.SetActive(isAction);

    }

    public void NextStage()
    {
        if(stageIndex < Stages.Length -1)
        {
            Stages[stageIndex].SetActive(false);

            stageIndex++;

            Stages[stageIndex].SetActive(true);

            PlayerReposition();

          //  UIStage.text = "STAGE" + (stageIndex + 1);


        }
        else // Game Clear
        {
            Time.timeScale = 0;

            Debug.Log("Game Clear!");
           
            Text btnText = RestartButton.GetComponentInChildren<Text>();
            btnText.text = "Clear!";
            RestartButton.SetActive(true);
        }

        totalPoint += stagePoint;
        stagePoint = 0;

    }

    public void HealthDown()
    {
        if(health > 1)
        {
            health--;
            UIhealth[health].color = new Color(1, 0, 0, 0.4f);
        }
        else
        {
            UIhealth[0].color = new Color(1, 0, 0, 0.4f);

            player.OnDie();

            RestartButton.SetActive(true);
        }
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            HealthDown();

            if(health > 1)
            {
                PlayerReposition();
            }
        }
            // Player Reposition
           
    }

    void PlayerReposition()
    {
        player.transform.position = new Vector3(0, 0, 0);
        player.VelocityZero();
    }

    public void Restart()
    {
        Time.timeScale = 1;

        SceneManager.LoadScene(0);
    }
  
}
