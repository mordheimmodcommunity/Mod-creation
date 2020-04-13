using UnityEngine;

[ExecuteInEditMode]
public class AssignGeneratorUV : MonoBehaviour
{
    [SerializeField]
    private FlowmapGenerator generator;

    [SerializeField]
    private Vector3 position;

    [SerializeField]
    private Vector2 dimensions;

    [SerializeField]
    private Renderer[] renderers;

    [SerializeField]
    private bool assignToSharedMaterial = true;

    [SerializeField]
    private string uvVectorName = "_FlowmapUV";

    private void Update()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if ((bool)generator)
            {
                position = generator.Position;
                dimensions = generator.Dimensions;
            }
            Vector4 zero = Vector4.zero;
            zero = ((dimensions.x < dimensions.y) ? new Vector4(dimensions.x * (dimensions.y / dimensions.x), dimensions.y, position.x, position.z) : new Vector4(dimensions.x, dimensions.y * (dimensions.x / dimensions.y), position.x, position.z));
            if (assignToSharedMaterial)
            {
                renderers[i].sharedMaterial.SetVector(uvVectorName, zero);
            }
            else
            {
                renderers[i].material.SetVector(uvVectorName, zero);
            }
        }
    }
}
