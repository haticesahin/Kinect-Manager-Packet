using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MoveForward : MonoBehaviour
{
 	public float forwardspeed = 2.0f;
    public Text countText;
    public Text winText;
    public GameObject yenidenBaslat;
    Turning rotatespeed;

    private Rigidbody rb;
    public int count;
    AudioSource audio;

    //public static bool GameIsPaused = false;
    bool yenidenBaslatKontrol = false;
    bool oyunBittiKontrol = false;
    // Use this for initialization 
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        setCountText();
        winText.text = "";
        audio = GetComponent<AudioSource>();
    }
 	// Update is called once per frame 
 	void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * forwardspeed);
        audio.Play();

        if (Input.GetKeyDown(KeyCode.W) && yenidenBaslatKontrol)
        {
            SceneManager.LoadScene("MiniGame");
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Quit();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
    void FixedUpdate()
    {
        //rb.velocity = transform.TransformDirection(Vector3.forward  * forwardspeed);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pick Up"))
        {
            other.gameObject.SetActive(false); //deactivates the game object
                count = count + 1;
                setCountText();
            
            if (count >= 8)
            {
                Finish();
             winText.text = "You win!";
            }

        }
    }
   public void setCountText()
    {
        countText.text = "Count: " + count.ToString();
    }
    
    public void Finish()
    {
        forwardspeed = 0;
        oyunBittiKontrol = true;
        yenidenBaslatKontrol = true;
       // rotatespeed = 0f;
        Debug.Log("Oyun Bitti");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
