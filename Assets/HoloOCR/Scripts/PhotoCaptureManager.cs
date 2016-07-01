using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HoloToolkit.Unity;
using UnityEngine.VR.WSA.WebCam;
using System.Linq;

public class PhotoCaptureManager : Singleton<PhotoCaptureManager>
{
    
    PhotoCapture photoCaptureObject = null;
    List<byte> imageBufferList;
    public delegate void PhotoReadyCallBack(bool success, List<byte> imageBufferList);
    PhotoReadyCallBack photoReadyCallBack;

    AudioSource cameraAudio;

    public void Start()
    {
        cameraAudio = GetComponent<AudioSource>();

    }

    public void PhotoCaptureDo(PhotoReadyCallBack photoReadyCallBack)
    {
        DebugDisplay.Instance.Log("PhotoCaptureDo ");

        this.photoReadyCallBack = photoReadyCallBack;
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
        cameraAudio.Play();
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        DebugDisplay.Instance.Log("OnPhotoCaptureCreated ");
        photoCaptureObject = captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        
        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.JPEG;// CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, false, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            DebugDisplay.Instance.Log("OnPhotoModeStarted ");

            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            DebugDisplay.Instance.Log("Unable to start photo mode!");
            photoReadyCallBack(false, imageBufferList);
        }
    }

     void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            DebugDisplay.Instance.Log("OnCapturedPhotoToMemory Copy Started ");

            imageBufferList = new List<byte>();
            // Copy the raw IMFMediaBuffer data into our empty byte list.
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);
            DebugDisplay.Instance.Log("OnCapturedPhotoToMemory " + imageBufferList.Count);

        }
        else
        {
            DebugDisplay.Instance.Log("Failed to save Photo to memory");
            photoReadyCallBack(false, imageBufferList);

        }

        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
        photoReadyCallBack(true, imageBufferList);
    }
}
