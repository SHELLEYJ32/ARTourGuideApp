using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public Sprite playImage;
    public Sprite pauseImage;
    public Button playPauseButton;

    public void PlayOrPause()
    {
        if (playPauseButton.image.sprite == pauseImage)
        {
            playPauseButton.image.sprite = playImage;
            GetComponent<AudioSource>().Pause();
        }
        else
        {
            playPauseButton.image.sprite = pauseImage;
            GetComponent<AudioSource>().UnPause();
        }
    }
}
