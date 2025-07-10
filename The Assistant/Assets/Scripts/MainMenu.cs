using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        StartCoroutine(LoadSceneWithDelay());
    }

    private IEnumerator LoadSceneWithDelay()
    {
        // Wait for 3 seconds
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    private IEnumerator QuitSceneWithDelay()
    {
        // Wait for 3 seconds
        yield return new WaitForSeconds(1f);

        Application.Quit();
    }
    public void QuitGame()
    {
        StartCoroutine(QuitSceneWithDelay());
    }
}
