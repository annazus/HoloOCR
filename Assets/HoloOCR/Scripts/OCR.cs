using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine.Networking;
using System.IO;
using System.Text;
using System.Runtime.Serialization;

#if UNITY_METRO && !UNITY_EDITOR
using System.Runtime.Serialization.Json;
#endif

#if UNITY_METRO && !UNITY_EDITOR

[DataContract]
public class Response
{
    [DataMember(Name = "language")]
    public string Language { get; set; }
    [DataMember(Name = "textAngle")]
    public float TextAngle { get; set; }
    [DataMember(Name = "orientation")]
    public string Orientation { get; set; }
    [DataMember(Name = "regions")]
    public Region[] Regions { get; set; }
}

[DataContract]
public class Region
{
    [DataMember(Name = "boundingBox")]
    public string BoundingBox { get; set; }
    [DataMember(Name = "lines")]
    public Line[] Lines { get; set; }
}

[DataContract]
public class Line
{
    [DataMember(Name = "boundingBox")]
    public string BoundingBox { get; set; }
    [DataMember(Name = "words")]
    public Word[] Words { get; set; }
}

[DataContract]
public class Word
{
    [DataMember(Name = "boundingBox")]
    public string BoundingBox { get; set; }
    [DataMember(Name = "text")]
    public string Text { get; set; }
}

#endif
public class OCR : Singleton<OCR>
{
    public string OCRUrl = "https://api.projectoxford.ai/vision/v1.0/ocr";
    public string OCRSubscriptionKey = "c50c0de9e1fe4513be4bf6a636d44feb";
    public string language = "unk";
    public bool detectOrientation=true;

    /// Delegate which is called when the OCRComplete event is triggered.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="args"></param>
    public delegate void EventHandler(object source, OCR_EventArgs   args);

    /// <summary>
    /// EventHandler which is triggered when the OCR Routine is finished.
    /// </summary>
    public event EventHandler OCR_Complete;


    UnityWebRequest wr;
    UploadHandler uploader;


    public void OCR_Start()
    {
        PhotoCaptureManager.Instance.PhotoCaptureDo(PhotoCaptureCompleted);

    }

    void PhotoCaptureCompleted(bool result, List<byte> imageBufferList)
    {
        DebugDisplay.Instance.Log("Started OCR");

        OCR_Request(imageBufferList);
    }

    void OCR_Request(List<byte> imageBufferList)
    {
        DebugDisplay.Instance.Log("OCR_Request ");

        uploader = new UploadHandlerRaw(imageBufferList.ToArray());
        string url;
        url = OCRUrl + "?" + "language=" + language + "&detectOrientation=" + (detectOrientation ? "true" : "false");
        wr = new UnityWebRequest(OCRUrl, UnityWebRequest.kHttpVerbPOST);
        wr.SetRequestHeader("Ocp-Apim-Subscription-Key", OCRSubscriptionKey);
        wr.SetRequestHeader("Content-Type", "application/octet-stream");
        wr.downloadHandler = new DownloadHandlerBuffer();
        wr.uploadHandler = uploader;

        StartCoroutine(MakeRequest());

    }

    IEnumerator MakeRequest()
    {
        DebugDisplay.Instance.Log("MakeRequest");

        yield return wr.Send();

        if (wr.isError)
        {
            DebugDisplay.Instance.Log(wr.error);
        }
        else
        {
            DebugDisplay.Instance.Log(wr.downloadHandler.text);


#if UNITY_METRO && !UNITY_EDITOR

            Response jsonResponse;
            MemoryStream responseStream = new MemoryStream(Encoding.ASCII.GetBytes(wr.downloadHandler.text));

            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Response));
            object objResponse = jsonSerializer.ReadObject(responseStream);
            jsonResponse = objResponse as Response;


            //Debug.Log(jsonResponse.TextAngle + jsonResponse.Language + jsonResponse.Orientation);
            string scannedText = "";
            //TextWriter.Instance.txtBox.text = jsonResponse.Regions[0].Lines[0].Words[0].Text;
            foreach (Region region in jsonResponse.Regions){
                foreach(Line line in region.Lines){
                    foreach (Word word in line.Words){
                        scannedText = scannedText + word.Text + " ";
                    }
                }
            }
            DebugDisplay.Instance.Log(scannedText);
            EventHandler handler = OCR_Complete;
            OCR_EventArgs eventArg = new OCR_EventArgs();
            eventArg.Text = scannedText;
            eventArg.Language = jsonResponse.Language;
            eventArg.TextAngle = jsonResponse.TextAngle;
            eventArg.Orientation = jsonResponse.Orientation;
            if (handler != null)
            {
                handler(this, eventArg);
            }

#endif


        }
    }
}
