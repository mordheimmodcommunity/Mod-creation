using UnityEngine;

public class SM_materialColorRandomizer : MonoBehaviour
{
	public Color color1 = new Color(0.6f, 0.6f, 0.6f, 1f);

	public Color color2 = new Color(0.4f, 0.4f, 0.4f, 1f);

	public bool unifiedColor = true;

	private float ColR;

	private float ColG;

	private float ColB;

	private float ColA;

	private void Start()
	{
		if (!unifiedColor)
		{
			ColR = Random.Range(color1.r, color2.r);
			ColG = Random.Range(color1.g, color2.g);
			ColB = Random.Range(color1.b, color2.b);
			ColA = Random.Range(color1.a, color2.a);
		}
		else
		{
			float value = Random.value;
			ColR = Mathf.Min(color1.r, color2.r) + Mathf.Abs(color1.r - color2.r) * value;
			ColG = Mathf.Min(color1.g, color2.g) + Mathf.Abs(color1.g - color2.g) * value;
			ColB = Mathf.Min(color1.b, color2.b) + Mathf.Abs(color1.b - color2.b) * value;
		}
		GetComponent<Renderer>().material.color = new Color(ColR, ColG, ColB, ColA);
	}
}
