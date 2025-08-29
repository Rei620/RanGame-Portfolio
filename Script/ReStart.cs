using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReStart : MonoBehaviour
{
  public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
