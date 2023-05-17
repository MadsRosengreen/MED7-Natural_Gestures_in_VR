using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class GestureLog : MonoBehaviour
{
    private List<string> logs = new List<string>();
    int fileNo = 1;

    private int[] gestureCount = new int[14];

    private string lastGesture = "";
    private string lastSeventh = "";

    string second_column = "Gesture type";
    string third_column = "Gesture count";
    string fourth_column = "Total count";
    string fifth_column = "Which Hand";
    string sixth_column = "Which Object";
    string seventh_column = "Box Velocity";
    string eighth_column = "Snapped Velocity";
    string ninth_column = "Which Gesture";
    string tenth_column = "Undefined";
    string eleventh_column = "Undefined";


    // Start is called before the first frame update
    void Start()
    {
        fileNo = PlayerPrefs.GetInt("fileNumber");
        LogAddHeaders();
        //SaveLog(); // Test
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))          // Test
        {
            PlayerPrefs.SetInt("fileNo", 1);
            fileNo = 1;
        }
    }

    public void LogAddHeaders()
    {
        logs.Add(Time.realtimeSinceStartup
            + ";" + second_column
            + ";" + third_column
            + ";" + fourth_column
            + ";" + fifth_column
            + ";" + sixth_column
            + ";" + seventh_column
            + ";" + eighth_column
            + ";" + ninth_column
            + ";" + tenth_column
            + ";" + eleventh_column);
    }
    public void Log(string first, string fourth = "NONE", string fifth = "NONE", string sixth = "NONE", string seventh = "NONE", string eighth = "NONE", string ninth = "NONE", string tenth = "NONE")
    {
        //Only log if new gesture if not the same as last and snapped velocity is different
        if (first != lastGesture && seventh != lastSeventh)
        {
            int counter = 0;
            int totalCount = 0;
            string lastLetter = fourth.Substring(fourth.Length - 1);
            string firstLetter = fourth.Substring(0, 1);
            string gestureType = fourth.Substring(fourth.Length - 3);
            //Determine gesture type
            switch (fourth.Substring(2, 1))
            {
                case "P":
                    eighth = first;
                    first = "Point";
                    break;
                case "S":
                    eighth = first;
                    first = "Stop";
                    break;
                case "T":
                    eighth = first;
                    first = "ThumbsUp";
                    break;
                case "O":
                    eighth = first;
                    first = "Ok";
                    break;
                default:
                    eighth = first;
                    first = "Error: Gesture undefined";
                    break;
            }
            //Defines left or right hand
            if (firstLetter == "R")
            {
                fourth = "Right";
            }
            else if (firstLetter == "L")
            {
                fourth = "Left";
            }
            else
            {
                fourth = "Error: Left/Right undefined";
            }

            switch (first)
            {
                case "Point":
                    gestureCount[0]++;
                    gestureCount[13]++;
                    counter = gestureCount[0];
                    totalCount = gestureCount[13];
                    break;
                case "Ok":
                    gestureCount[1]++;
                    gestureCount[13]++;
                    counter = gestureCount[1];
                    totalCount = gestureCount[13];
                    break;
                case "Forward":
                    gestureCount[2]++;
                    gestureCount[13]++;
                    counter = gestureCount[2];
                    totalCount = gestureCount[13];
                    break;
                case "Back":
                    gestureCount[3]++;
                    gestureCount[13]++;
                    counter = gestureCount[3];
                    totalCount = gestureCount[13];
                    break;
                case "Left":
                    gestureCount[4]++;
                    gestureCount[13]++;
                    counter = gestureCount[4];
                    totalCount = gestureCount[13];
                    break;
                case "Right":
                    gestureCount[5]++;
                    gestureCount[13]++;
                    counter = gestureCount[5];
                    totalCount = gestureCount[13];
                    break;
                default:
                    gestureCount[12]++;
                    gestureCount[13]++;
                    counter = gestureCount[12];
                    totalCount = gestureCount[13];
                    break;
            }
            logs.Add(Time.realtimeSinceStartup
                        + ";" + first
                        + ";" + counter
                        + ";" + totalCount
                        + ";" + fourth
                        + ";" + fifth
                        + ";" + sixth
                        + ";" + seventh
                        + ";" + eighth
                        + ";" + ninth
                        + ";" + tenth);
        }
    }

    public void SaveLog()
    {
        Log("END");
        File.WriteAllLines(Application.persistentDataPath + "/File_" + fileNo + ".csv", logs);
        logs.Clear();
        fileNo++;
        PlayerPrefs.SetInt("fileNumber", fileNo);
        LogAddHeaders();
        for (int i = 0; i <= gestureCount.Length; i++)
        {
            gestureCount[i] = 0;
        }
        lastGesture = "";
        lastSeventh = "";
    }
} 
