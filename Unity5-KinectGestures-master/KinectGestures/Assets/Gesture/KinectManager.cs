using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Kinect;
using System.Text;

public class KinectManager : MonoBehaviour
{
    public Text GestureTextGameObject;
    public Text ConfidenceTextGameObject;
    public GameObject Player;
    private Turning turnScript; 

    // Kinect 
    private KinectSensor kinectSensor;
    
    // color frame and data 
    private ColorFrameReader colorFrameReader;
    private byte[] colorData;
    private Texture2D colorTexture;

    private BodyFrameReader bodyFrameReader;
    private int bodyCount;
    private Body[] bodies;

    private string leanLeftGestureName = "Lean_Left";
    private string leanRightGestureName = "Lean_Right";
    private string leanSpineGestureName = "Stop";

    // GUI output
    private UnityEngine.Color[] bodyColors;
    //private string[] bodyText;

    /// <summary> List of gesture detectors, there will be one detector created for each potential body (max of 6)
    /// Hareket dedektörlerinin listesi, her potansiyel vücut için bir dedektör oluşturulacaktır (maks. 6)</summary>
    private List<GestureDetector> gestureDetectorList = null;

    // Use this for initialization
    // Başlatma için bunu kullanın
    void Start()
    {
        turnScript = Player.GetComponent<Turning>();
        // get the sensor object
        // sensör nesnesini al

        this.kinectSensor = KinectSensor.GetDefault();

        if (this.kinectSensor != null)
        {
            this.bodyCount = this.kinectSensor.BodyFrameSource.BodyCount;

            // color reader
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

            // create buffer from RGBA frame description
            // RGBA çerçeve açıklamasından arabellek oluştur
            var desc = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);


            // body data
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader(); //acık okuyucu

            // body frame to use
            // kullanılacak gövde çerçevesi
            this.bodies = new Body[this.bodyCount];

            // initialize the gesture detection objects for our gestures
            // jestlerimiz için hareket algılama nesnelerini başlat
            this.gestureDetectorList = new List<GestureDetector>();
            for (int bodyIndex = 0; bodyIndex < this.bodyCount; bodyIndex++)
            {
                //PUT UPDATED UI STUFF HERE FOR NO GESTURE
                // GÜNCELLEŞTİRİLMİŞ UI ÇALIŞMALARINI HİÇBİR ZAMAN İÇİN BURAYA
                GestureTextGameObject.text = "none";
                //this.bodyText[bodyIndex] = "none";
                this.gestureDetectorList.Add(new GestureDetector(this.kinectSensor));
            }

            // start getting data from runtime
            this.kinectSensor.Open();
        }
        else
        {
            //kinect sensor not connected
        }
    }

    // Update is called once per frame
    // Her kare için güncelleme çağrılır
    void Update()
    {

        // process bodies
        // süreç gövdeleri
        bool newBodyData = false;
            using (BodyFrame bodyFrame = this.bodyFrameReader.AcquireLatestFrame())  //son kareyi al
            {
                if (bodyFrame != null)
                {
                    bodyFrame.GetAndRefreshBodyData(this.bodies);  //gövde verilerini al ve yenile
                    newBodyData = true;
                }
            }

            if (newBodyData)
            {
            // update gesture detectors with the correct tracking id
            // hareket dedektörlerini doğru izleme kimliğiyle güncelleyin
            for (int bodyIndex = 0; bodyIndex < this.bodyCount; bodyIndex++)
                {
                    var body = this.bodies[bodyIndex];
                    if (body != null)
                    {
                        var trackingId = body.TrackingId;

                    // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                    // geçerli gövde TrackingId değiştiyse, karşılık gelen hareket algılayıcıyı yeni değerle güncelleyin
                    if (trackingId != this.gestureDetectorList[bodyIndex].TrackingId)
                        {
                        GestureTextGameObject.text = "none";
                            //this.bodyText[bodyIndex] = "none";
                            this.gestureDetectorList[bodyIndex].TrackingId = trackingId;

                        // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                        // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                        // geçerli gövde izlenirse, VisualGestureBuilderFrameArrived olaylarını almak için dedektörünün duraklatmasını kaldırın
                        // mevcut gövde izlenmezse, dedektörünü duraklatın, böylece geçersiz hareket sonuçları almaya çalışırken kaynakları boşa harcamayız
                        this.gestureDetectorList[bodyIndex].IsPaused = (trackingId == 0);
                            this.gestureDetectorList[bodyIndex].OnGestureDetected += CreateOnGestureHandler(bodyIndex);
                    }
                    }
                }
            }
        
    }

    private EventHandler<GestureEventArgs> CreateOnGestureHandler(int bodyIndex)
    {
        return (object sender, GestureEventArgs e) => OnGestureDetected(sender, e, bodyIndex);
    }

    private void OnGestureDetected(object sender, GestureEventArgs e, int bodyIndex)
    {
        var isDetected = e.IsBodyTrackingIdValid && e.IsGestureDetected;
        
        if(e.GestureID == leanLeftGestureName)
        {
            //NEW UI FOR GESTURE DETECTed
            // HAREKET TESPİT EDİLEN YENİ UI
            GestureTextGameObject.text = "Gesture Detected: " + isDetected;
            //StringBuilder text = new StringBuilder(string.Format("Gesture Detected? {0}\n", isDetected));
            ConfidenceTextGameObject.text = "Confidence: " + e.DetectionConfidence;
            //text.Append(string.Format("Confidence: {0}\n", e.DetectionConfidence));
            if (e.DetectionConfidence > 0.65f)
            {
                turnScript.turnLeft = true;
            }
            else
            {
                turnScript.turnLeft = false;
            }
        }

        if (e.GestureID == leanRightGestureName)
        {
            //NEW UI FOR GESTURE DETECTed
            GestureTextGameObject.text = "Gesture Detected: " + isDetected;
            //StringBuilder text = new StringBuilder(string.Format("Gesture Detected? {0}\n", isDetected));
            ConfidenceTextGameObject.text = "Confidence: " + e.DetectionConfidence;
            //text.Append(string.Format("Confidence: {0}\n", e.DetectionConfidence));
            if (e.DetectionConfidence > 0.65f)
            {
                turnScript.turnRight = true;
            }
            else
            {
                turnScript.turnRight = false;
            }
        }

        //this.bodyText[bodyIndex] = text.ToString();
    }

    private void OnRightLeanGestureDetected(object sender, GestureEventArgs e, int bodyIndex)
    {
        var isDetected = e.IsBodyTrackingIdValid && e.IsGestureDetected;

        //NEW UI FOR GESTURE DETECTed
        // HAREKET TESPİT EDİLEN YENİ UI
        GestureTextGameObject.text = "Gesture Detected: " + isDetected;
        //StringBuilder text = new StringBuilder(string.Format("Gesture Detected? {0}\n", isDetected));
        ConfidenceTextGameObject.text = "Confidence: " + e.DetectionConfidence;
        //text.Append(string.Format("Confidence: {0}\n", e.DetectionConfidence));
        if (e.DetectionConfidence > 0.65f)
        {
            turnScript.turnRight = true;
        }
        else
        {
            turnScript.turnRight = false;
        }

        //this.bodyText[bodyIndex] = text.ToString();
    }

    void OnApplicationQuit()
    {
        if (this.colorFrameReader != null)
        {
            this.colorFrameReader.Dispose();
            this.colorFrameReader = null;
        }

        if (this.bodyFrameReader != null)
        {
            this.bodyFrameReader.Dispose();
            this.bodyFrameReader = null;
        }

        if (this.kinectSensor != null)
        {
            if (this.kinectSensor.IsOpen)
            {
                this.kinectSensor.Close();
            }

            this.kinectSensor = null;
        }
    }

}
