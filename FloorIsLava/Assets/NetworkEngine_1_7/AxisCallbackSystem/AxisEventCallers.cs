using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class AxisEventCallers : MonoBehaviour
{

    float LastHorizontal = 0;
    float LastVertical = 0;
    float LastFire = 0;
    float LastJump = 0;
    public static Dictionary<string, AxisEventSystem> InputEvents;
    public static Dictionary<string, float> LastInput;
    public string[] WatchedAxis;
    public bool DirChanged = false;
    public bool IsMoving = false;

    public static AxisEventCallers current;
    public event Action OnDirectionChanged;
    public void DirectionChanged()
    {
        if (OnDirectionChanged != null)
        {
            OnDirectionChanged();
        }
    }
    public event Action OnMove;
    public void Move()
    {
        if (OnMove != null)
        {
            OnMove();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        //Initialize LastInputs
        LastInput = new Dictionary<string, float>();
        //Initialize InputEvents
        InputEvents = new Dictionary<string, AxisEventSystem>();
        foreach (string x in WatchedAxis)
        {
            LastInput.Add(x, 0);
            InputEvents.Add(x, new AxisEventSystem());
        }


    }

    private void Awake()
    {
        current = this;
    }

    // Update is called once per frame
    void Update()
    {
        foreach(string x in WatchedAxis)
        {
            if(Input.GetAxis(x) != LastInput[x])
            {
                if(Mathf.Abs(Input.GetAxis(x))< .1f)
                {
                    InputEvents[x].AxisKeyUp();
                }
                else
                {
                    InputEvents[x].AxisKeyDown();
                }
                LastInput[x] = Input.GetAxis(x);
                if(x == "Vertical" || x == "Horizontal")
                {
                    DirChanged = true;
                }
            }
            else if (Mathf.Abs(Input.GetAxis(x)) > .1f)
            {
                InputEvents[x].AxisKeyStay();
                if (x == "Vertical" || x == "Horizontal")
                {
                    IsMoving = true;
                }
            }
        }
        if(DirChanged)
        {
            DirectionChanged();
            DirChanged = false;
        }
        if(IsMoving)
        {
            Move();
            IsMoving = false;
        }
 


    }
}
