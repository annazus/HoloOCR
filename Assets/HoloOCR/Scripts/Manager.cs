using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;

public class OCR_EventArgs : EventArgs
{
    public string Text { get; set; }
    public float TextAngle { get; set; }
    public string Language { get; set; }
    public string Orientation { get; set; }

}

public class Manager : Singleton<Manager> {

    bool OCR_Processed;
    string OCR_Text;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Scan()
    {
        DebugDisplay.Instance.Log("Scan");

        OCR_Processed = false;
        OCR.Instance.OCR_Complete += Manager_OCR_Complete;
        OCR.Instance.OCR_Start();
 
    }

    private void Manager_OCR_Complete(object source, OCR_EventArgs args)
    {
        DebugDisplay.Instance.Log("Manager_OCR_Complete");
        OCR_Text = args.Text;
        OCR_Processed = true;
        DebugDisplay.Instance.Log(OCR_Text);

    }

    /// <summary>
    /// Called when the GameObject is unloaded.
    /// </summary>
    private void OnDestroy()
    {
        DebugDisplay.Instance.Log("OnDestroy");

        if (OCR.Instance != null)
        {
            OCR.Instance.OCR_Complete -= Manager_OCR_Complete;
        }
    }

}
