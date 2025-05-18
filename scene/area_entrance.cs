using UnityEngine;

public class AreaEntrance : MonoBehaviour
{
    [SerializeField] private string transitionName;

    private void Start() {
        if (transitionName == SceneManagement.Instance.SceneTransitionName) {
            playercontrols.Instance.transform.position = this.transform.position;
        }
    }
}
