using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIHandlers : BehaviourSingleton<UIHandlers>
{
    [SerializeField]
    private Text scoreText;

    private void Awake()
    {
        Debug.Assert(this.scoreText, this);

        instance = this;
    }

    public void UpdateScore(int team0Score, int team1Score)
    {
        this.scoreText.text = string.Format("{0} - {1}", team0Score, team1Score);
    }

    public void UGUI_OnResetButtonPress()
    {
        SceneManager.LoadScene(0);
    }
}
