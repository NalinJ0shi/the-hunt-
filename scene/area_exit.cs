using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
public class AreaExit : MonoBehaviour 
{
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string sceneTransitionName;
    private bool isEnabled = false;
    
    private void Start() {
        // Disable collider initially
        GetComponent<Collider2D>().enabled = false;
        
        // Enable after delay
        Invoke("EnableExit", 2f);
    }
    
    private void EnableExit() {
        GetComponent<Collider2D>().enabled = true;
        isEnabled = true;
    }
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (!isEnabled) return;
        
        playercontrols player = other.GetComponent<playercontrols>();
        if (player == null) {
            player = other.GetComponentInParent<playercontrols>();
        }
        
        if (player != null) {
            isEnabled = false;
            SceneManagement.Instance.SetTransitionName(sceneTransitionName);
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}