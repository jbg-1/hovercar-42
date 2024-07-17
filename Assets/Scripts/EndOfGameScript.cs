using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndOfGameScript : MonoBehaviour
{
    [SerializeField] private Light[] lights;

    private void Start()
    {
        StartCoroutine(Show());
    }

    public void ShowEOG(List<int> ints)
    {

    }

    private IEnumerator Show()
    {
        yield return new WaitForSeconds(1f);
        lights[0].enabled = true;
        yield return new WaitForSeconds(1f);
        lights[1].enabled = true;
        yield return new WaitForSeconds(1f);
        lights[2].enabled = true;
    }
}
