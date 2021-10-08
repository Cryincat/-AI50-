using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeComponent : MonoBehaviour
{
    public Node node;
    public MeshFilter meshFilter;
    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        List<Color> colors = new List<Color>();
        foreach (var item in meshFilter.mesh.vertices)
        {
            colors.Add(new Color(1,1,1,1));
        }
        meshFilter.mesh.colors = colors.ToArray();
    }
}
