using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndOfGameScript : MonoBehaviour
{
    [SerializeField] private Light[] lights;
    [SerializeField] private MeshRenderer[] cars;

    private void Start()
    {
        List<int> rank = ResultNet.instance.LastRaceResult;
        for(int i = 0; i < 3; i++)
        {
            if (rank[i] == -1)
            {
                cars[i].gameObject.SetActive(false);
            }
            else
            {
                cars[i].material = PlayerColors.instance.getColor(rank[i]).material;
            }
        }
        StartCoroutine(Show());
    }

    private IEnumerator Show()
    {
        yield return new WaitForSeconds(1f);
        lights[0].enabled = true;
        yield return new WaitForSeconds(1f);
        lights[1].enabled = true;
        yield return new WaitForSeconds(1f);
        lights[2].enabled = true;
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Lobby");
    }
}
