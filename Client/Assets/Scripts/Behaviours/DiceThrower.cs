using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceThrower : MonoBehaviour
{
    public const float WaitTime = 1f; //The amount seconds that the dice will roll and stay still.

    private GameObject[] Dice { get; } = new GameObject[2];
    private Rigidbody[] DiceRbs { get; } = new Rigidbody[2];
    public bool Rolling { get; private set; } = false;
    private int result;
    private float StartTime;
    private bool Threw;
    private float StopTime;

    /// <summary>
    /// Runs every tick.
    /// Does the dice animation, throwing and disappearing at the end.
    /// </summary>
    private void Update()
    {
        if (Rolling)
        {
            if (!Threw)
            {
                if (Time.time >= StartTime + WaitTime)
                {
                    Dice[0].transform.eulerAngles = new Vector3(45, 45, 45);
                    Dice[1].transform.eulerAngles = new Vector3(45, 45, 45);

                    foreach (GameObject die in Dice)
                    {
                        die.GetComponent<Collider>().enabled = true;
                    }

                    foreach (Rigidbody die in DiceRbs)
                    {
                        die.useGravity = true;
                    }

                    DiceRbs[0].velocity = new Vector3(1, 0, 2);
                    DiceRbs[1].velocity = new Vector3(1, 0, 3);

                    Threw = true;
                    StopTime = 0;
                }
                else
                {
                    foreach (GameObject die in Dice)
                    {
                        die.transform.rotation = Random.rotation;
                    }
                }
            }
            else
            {
                if (StopTime == 0)
                {
                    bool stopped = true;
                    foreach (Rigidbody rb in DiceRbs)
                    {
                        if (rb.velocity.magnitude != 0)
                            stopped = false;
                    }
                    if (stopped)
                    {
                        StopTime = Time.time;
                    }
                }
                else if (Time.time >= StopTime + WaitTime)
                {
                    HideDice();
                    Rolling = false;
                    if (result == 7)
                    {
                        GetComponent<Player>().OnScreenText.SetText("Waiting for other players to discard half the cards in their hand");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates the dice on screen.
    /// </summary>
    public void ShowDice()
    {
        if (Rolling)
        {
            Debug.LogError("There are already dice");
            return;
        }

        Dice[0] = Instantiate(Prefabs.Dice, this.transform);
        Dice[0].transform.position -= new Vector3(3, 0);

        Dice[1] = Instantiate(Prefabs.Dice, this.transform);
    }

    /// <summary>
    /// Creates new dice and throws them, resulting in set up results.
    /// </summary>
    /// <param name="expected1">The result of the first dice</param>
    /// <param name="expected2">The result of the second dice</param>
    /// <param name="result">The sum result</param>
    public void ThrowDice(int expected1, int expected2, int result)
    {
        if (Rolling)
        {
            Debug.LogError("There are already dice rolling");
            return;
        }

        ShowDice();
        DiceRbs[0] = Dice[0].GetComponent<Rigidbody>();
        DiceRbs[1] = Dice[1].GetComponent<Rigidbody>();

        Transform toRotate = Dice[0].transform.GetChild(0);
        switch (expected1)
        {
            case 1:
                toRotate.eulerAngles = new Vector3(0, 90, 0);
                break;
            case 2:
                toRotate.eulerAngles = new Vector3(0, 0, 90);
                break;
            case 3:
                toRotate.eulerAngles = new Vector3(0, 180, 0);
                break;
            case 4:
                toRotate.eulerAngles = new Vector3(0, 0, 0);
                break;
            case 5:
                toRotate.eulerAngles = new Vector3(0, 0, 270);
                break;
            case 6:
                toRotate.eulerAngles = new Vector3(0, 270, 0);
                break;
        }


        toRotate = Dice[1].transform.GetChild(0);
        switch (expected2)
        {
            case 1:
                toRotate.eulerAngles = new Vector3(270, 0, 0);
                break;
            case 2:
                toRotate.eulerAngles = new Vector3(180, 0, 0);
                break;
            case 3:
                toRotate.eulerAngles = new Vector3(0, 0, 270);
                break;
            case 4:
                toRotate.eulerAngles = new Vector3(0, 0, 90);
                break;
            case 5:
                toRotate.eulerAngles = new Vector3(0, 0, 0);
                break;
            case 6:
                toRotate.eulerAngles = new Vector3(90, 0, 0);
                break;
        }

        this.result = result;
        StartTime = Time.time;
        Threw = false;
        Rolling = true;
    }

    /// <summary>
    /// Hides the dice.
    /// </summary>
    public void HideDice()
    {
        try
        {
            for (int i = 0; i < Dice.Length; i++)
            {
                Destroy(Dice[i]);
                Dice[i] = null;
            }
        }
        catch
        {
            Debug.LogError("There are no rolling dice");
        }
    }
}
