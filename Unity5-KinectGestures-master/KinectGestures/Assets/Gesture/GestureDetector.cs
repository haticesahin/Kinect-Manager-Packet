//------------------------------------------------------------------------------
// <copyright file="GestureDetector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Windows.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using UnityEngine;

public class GestureEventArgs : EventArgs
{
    public bool IsBodyTrackingIdValid { get; private set; }

    public bool IsGestureDetected { get; private set; }

    public float DetectionConfidence { get; private set; }

    //my modification
    public string GestureID { get; private set; }

    //public GestureEventArgs(bool isBodyTrackingIdValid, bool isGestureDetected, float detectionConfidence)
    //{
    //    this.IsBodyTrackingIdValid = isBodyTrackingIdValid;
    //    this.IsGestureDetected = isGestureDetected;
    //    this.DetectionConfidence = detectionConfidence;
    //}

    //my mod
    public GestureEventArgs(bool isBodyTrackingIdValid, bool isGestureDetected, float detectionConfidence, string gestureID)
    {
        this.IsBodyTrackingIdValid = isBodyTrackingIdValid; //Vücut İzleme Kimliği Geçerli mi?
        this.IsGestureDetected = isGestureDetected;         //Hareket Algılandı mı?
        this.DetectionConfidence = detectionConfidence;     //Tespit Güven
        this.GestureID = gestureID; 
    }
}

/// <summary>
/// Gesture Detector class which listens for VisualGestureBuilderFrame events from the service
/// and calls the OnGestureDetected event handler when a gesture is detected.
/// Hizmetten VisualGestureBuilderFrame olaylarını dinleyen ve bir hareket algılandığında
/// OnGestureDetected olay işleyicisini çağıran Gesture Detector sınıfı.
/// </summary>
public class GestureDetector : IDisposable
{
    /// <summary> Path to the gesture database that was trained with VGB 
    /// VGB ile eğitilmiş hareket veritabanının yolu</summary>
    private readonly string leanDB = "GestureDB\\Lean.gbd";


    /// <summary> Name of the discrete gesture in the database that we want to track
    /// İzlemek istediğimiz veritabanındaki ayrık hareketin adı</summary>
    private readonly string leanLeftGestureName = "Lean_Left";
    private readonly string leanRightGestureName = "Lean_Right";
    private readonly string leanSpineGestureName = "Stop";


    /// <summary> Gesture frame source which should be tied to a body tracking ID 
    /// Bir gövde izleme kimliğine bağlı olması gereken hareket çerçevesi kaynağı</summary>
    private VisualGestureBuilderFrameSource vgbFrameSource = null;

    /// <summary> Gesture frame reader which will handle gesture events coming from the sensor 
    /// Sensörden gelen hareket olaylarını işleyecek hareket çerçevesi okuyucu</summary>
    private VisualGestureBuilderFrameReader vgbFrameReader = null;

    public event EventHandler<GestureEventArgs> OnGestureDetected;

    /// <summary>
    /// Initializes a new instance of the GestureDetector class along with the gesture frame source and reader
    /// Hareket çerçevesi kaynağı ve okuyucu ile birlikte GestureDetector sınıfının yeni bir örneğini başlatır</summary>
    /// <param name="kinectSensor">Active sensor to initialize the VisualGestureBuilderFrameSource object with
    /// VisualGestureBuilderFrameSource nesnesini ile başlatmak için etkin sensör</param>
    public GestureDetector(KinectSensor kinectSensor)
    {
        if (kinectSensor == null)
        {
            throw new ArgumentNullException("kinectSensor");
        }

        // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
        //vgb kaynağını oluşturun.İlişkili gövde izleme kimliği, sensörden geçerli bir gövde çerçevesi geldiğinde ayarlanır.
        this.vgbFrameSource = VisualGestureBuilderFrameSource.Create(kinectSensor, 0);
        this.vgbFrameSource.TrackingIdLost += this.Source_TrackingIdLost;

        // open the reader for the vgb frames
        this.vgbFrameReader = this.vgbFrameSource.OpenReader();
        if (this.vgbFrameReader != null)
        {
            this.vgbFrameReader.IsPaused = true;
            this.vgbFrameReader.FrameArrived += this.Reader_GestureFrameArrived;
        }

        //// load the 'Seated' gesture from the gesture database
        //var databasePath = Path.Combine(Application.streamingAssetsPath, this.gestureDatabase);
        //using (VisualGestureBuilderDatabase database = VisualGestureBuilderDatabase.Create(databasePath))
        //{
        //    // we could load all available gestures in the database with a call to vgbFrameSource.AddGestures(database.AvailableGestures), 
        //    // but for this program, we only want to track one discrete gesture from the database, so we'll load it by name
        //    foreach (Gesture gesture in database.AvailableGestures)
        //    {
        //        if (gesture.Name.Equals(this.seatedGestureName))
        //        {
        //            this.vgbFrameSource.AddGesture(gesture);
        //        }
        //    }
        //}

        //// "Oturmuş" hareketini hareket veritabanından yükleyin
        // var databasePath = Path.Combine (Application.streamingAssetsPath, this.gestureDatabase);
        // kullanarak (VisualGestureBuilderDatabase database = VisualGestureBuilderDatabase.Create (databasePath))
        // {
        // // vgbFrameSource.AddGestures (database.AvailableGestures) çağrısıyla veritabanındaki tüm kullanılabilir hareketleri yükleyebiliriz,
        // // ancak bu program için, veritabanından yalnızca bir ayrı hareketi izlemek istiyoruz, bu yüzden isme göre yükleyeceğiz
        // foreach (Veritabanında jest hareketi. Kullanılabilir Hareketler)
        // {
        // if (gesture.Name.Equals (this.seatedGestureName))
        // {
        // this.vgbFrameSource.AddGesture (jest);
        //}
        //}
        //}

        // load the 'Seated' gesture from the gesture database
        // 'Oturmuş' hareketini hareket veritabanından yükleyin
        var databasePath = Path.Combine(Application.streamingAssetsPath, this.leanDB);
        using (VisualGestureBuilderDatabase database = VisualGestureBuilderDatabase.Create(databasePath))
        {
            // we could load all available gestures in the database with a call to vgbFrameSource.AddGestures(database.AvailableGestures), 
            // but for this program, we only want to track one discrete gesture from the database, so we'll load it by name
            // vgbFrameSource.AddGestures (database.AvailableGestures) çağrısıyla veritabanındaki tüm kullanılabilir hareketleri yükleyebiliriz,
            // ama bu program için, veritabanından yalnızca bir ayrık hareket izlemek istiyoruz, bu yüzden isme göre yükleyeceğiz
            foreach (Gesture gesture in database.AvailableGestures)
            {
                if (gesture.Name.Equals(this.leanLeftGestureName))
                {
                    this.vgbFrameSource.AddGesture(gesture);
                }
                if (gesture.Name.Equals(this.leanRightGestureName))
                {
                    this.vgbFrameSource.AddGesture(gesture);
                }
                if (gesture.Name.Equals(this.leanSpineGestureName))
                {
                    this.vgbFrameSource.AddGesture(gesture);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the body tracking ID associated with the current detector
    /// The tracking ID can change whenever a body comes in/out of scope
    /// Geçerli dedektörle ilişkili gövde izleme kimliğini alır veya ayarlar
    /// Bir gövde kapsamına girdiğinde / kapsam dışı olduğunda izleme kimliği değişebilir
    /// </summary>
    public ulong TrackingId
    {
        get
        {
            return this.vgbFrameSource.TrackingId;
        }

        set
        {
            if (this.vgbFrameSource.TrackingId != value)
            {
                this.vgbFrameSource.TrackingId = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether or not the detector is currently paused
    /// If the body tracking ID associated with the detector is not valid, then the detector should be paused
    /// Dedektörün şu anda duraklatılmış olup olmadığını gösteren bir değer alır veya ayarlar
    /// Dedektörle ilişkilendirilmiş gövde izleme kimliği geçerli değilse dedektör duraklatılmalıdır
    /// </summary>
    public bool IsPaused
    {
        get
        {
            return this.vgbFrameReader.IsPaused;
        }

        set
        {
            if (this.vgbFrameReader.IsPaused != value)
            {
                this.vgbFrameReader.IsPaused = value;
            }
        }
    }

    /// <summary>
    /// Disposes all unmanaged resources for the class
    /// Yönetilmeyen tüm kaynakları sınıfa atar
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader objects
    /// VisualGestureBuilderFrameSource ve VisualGestureBuilderFrameReader nesnelerini atar
    /// </summary>
    /// <param name="disposing">True if Dispose was called directly, false if the GC handles the disposing
    /// Dispose doğrudan çağrıldıysa true, GC imha ederse yanlış</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.FrameArrived -= this.Reader_GestureFrameArrived;
                this.vgbFrameReader.Dispose();
                this.vgbFrameReader = null;
            }

            if (this.vgbFrameSource != null)
            {
                this.vgbFrameSource.TrackingIdLost -= this.Source_TrackingIdLost;
                this.vgbFrameSource.Dispose();
                this.vgbFrameSource = null;
            }
        }
    }

    /// <summary>
    /// Handles gesture detection results arriving from the sensor for the associated body tracking Id
    /// İlişkili gövde izleme kimliği için sensörden gelen hareket algılama sonuçlarını işler
    /// </summary>
    /// <param name="sender">object sending the event
    /// olayı gönderen nesne</param>
    /// <param name="e">event arguments
    /// olay argümanları</param>
    private void Reader_GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
    {
        VisualGestureBuilderFrameReference frameReference = e.FrameReference;
        using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
        {
            if (frame != null)
            {
                // get the discrete gesture results which arrived with the latest frame
                //en son kare ile gelen ayrık hareket sonuçlarını elde edin
                var discreteResults = frame.DiscreteGestureResults;

                if (discreteResults != null)
                {
                    // we only have one gesture in this source object, but you can get multiple gestures
                    // bu kaynak nesnede yalnızca bir hareketimiz var, ancak birden çok hareket elde edebilirsiniz
                    foreach (Gesture gesture in this.vgbFrameSource.Gestures)
                    {

                        if (gesture.Name.Equals(this.leanLeftGestureName) && gesture.GestureType == GestureType.Discrete)
                        {
                            DiscreteGestureResult result = null;
                            discreteResults.TryGetValue(gesture, out result);

                            if (result != null)
                            {
                                if (this.OnGestureDetected != null)
                                {
                                    this.OnGestureDetected(this, new GestureEventArgs(true, result.Detected, result.Confidence, this.leanLeftGestureName));
                                }
                            }
                        }

                        if (gesture.Name.Equals(this.leanRightGestureName) && gesture.GestureType == GestureType.Discrete)
                        {
                            DiscreteGestureResult result = null;
                            discreteResults.TryGetValue(gesture, out result);

                            if (result != null)
                            {
                                if (this.OnGestureDetected != null)
                                {
                                    this.OnGestureDetected(this, new GestureEventArgs(true, result.Detected, result.Confidence, this.leanRightGestureName));
                                }
                            }
                        }


                        if (gesture.Name.Equals(this.leanSpineGestureName) && gesture.GestureType == GestureType.Discrete)
                        {
                            DiscreteGestureResult result = null;
                            discreteResults.TryGetValue(gesture, out result);

                            if (result != null)
                            {
                                if (this.OnGestureDetected != null)
                                {
                                    this.OnGestureDetected(this, new GestureEventArgs(true, result.Detected, result.Confidence, this.leanSpineGestureName));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handles the TrackingIdLost event for the VisualGestureBuilderSource object
    /// VisualGestureBuilderSource nesnesi için TrackingIdLost olayını işler
    /// </summary>
    /// <param name="sender">
    /// olayı gönderen nesne</param>
    /// <param name="e">event arguments
    /// olay argümanları</param>
    private void Source_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
    {
        if (this.OnGestureDetected != null)
        {
            this.OnGestureDetected(this, new GestureEventArgs(false, false, 0.0f, "none"));
        }
    }
}
