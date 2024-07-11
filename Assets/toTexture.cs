using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class toTexture : MonoBehaviour
{
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] Camera cam;


    // Start is called before the first frame update
    void Start()
    {
        /*
        var dirPath = Application.dataPath + "/RenderOutput";
        var tex = new Texture2D(renderTexture.width, renderTexture.height);
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex.Apply();

        System.IO.File.WriteAllBytes(dirPath + "/R_" + Random.Range(0, 100000) + ".png", tex.EncodeToPNG());
        */



        RenderTexture mRt = new RenderTexture(renderTexture.width, renderTexture.height, renderTexture.depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        mRt.antiAliasing = renderTexture.antiAliasing;

        var tex = new Texture2D(mRt.width, mRt.height, TextureFormat.ARGB32, false);
        cam.targetTexture = mRt;
        cam.Render();
        RenderTexture.active = mRt;

        tex.ReadPixels(new Rect(0, 0, mRt.width, mRt.height), 0, 0);
        tex.Apply();

        var dirPath = Application.dataPath + "/RenderOutput";
        var path = dirPath + "/R_" + Random.Range(0, 100000) + ".png";
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        Debug.Log("Saved file to: " + path);

        DestroyImmediate(tex);

        cam.targetTexture = renderTexture;
        cam.Render();
        RenderTexture.active = renderTexture;

        DestroyImmediate(mRt);
    }

    // Update is called once per frame
    void Update()
    {
   
    }
}
